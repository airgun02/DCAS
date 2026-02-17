using DCAS.Data;
using DCAS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization; // if needed for parsing

namespace DCAS.Controllers
{
    public class AppointmentRegisterController : Controller
    {
        private readonly DCASContext _context;

        public AppointmentRegisterController(DCASContext context)
        {
            _context = context;
        }

        // ============================
        // GET: Appointment Register
        // ============================
        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.AvailableDays = GetAvailableDays();

            ViewBag.MedicalServices = _context.Services
                .Select(s => new SelectListItem
                {
                    Text = $"{s.MedicalNameService} - ₱{s.Price}",
                    Value = s.Id.ToString()
                })
                .ToList();

            ViewBag.AvailableTimes = new List<SelectListItem>();

            var model = new PersonInfo
            {
                Date = DateTime.Today,
                BirthDay = DateTime.Today.AddYears(-30)
            };

            return View(model);
        }

        // ============================
        // POST: Register + Book
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(PersonInfo personInfo)
        {
            // check for existing patient by name first
            if (personInfo.Id == 0 && !string.IsNullOrWhiteSpace(personInfo.Name))
            {
                var existingPerson = await _context.PersonInfo
                    .FirstOrDefaultAsync(p => p.Name == personInfo.Name);

                if (existingPerson != null)
                {
                    personInfo.Id = existingPerson.Id; // reuse existing patient
                }
            }

            bool isNewPatient = personInfo.Id == 0;

            ViewBag.AvailableDays = GetAvailableDays();
            ViewBag.AvailableTimes = new List<SelectListItem>();

            // Age calculation
            if (personInfo.BirthDay.HasValue)
                personInfo.Age = CalculateAge(personInfo.BirthDay.Value);
            else
                personInfo.Age = 0;

            // Default service
            if (string.IsNullOrEmpty(personInfo.MedicalServices))
                personInfo.MedicalServices = "N/A";

            // Ensure marital Status is blank for new appointments (only set via profile updates)
            if (isNewPatient)
            {
                personInfo.Status = null;
            }

            // Validate date/time
            if (string.IsNullOrEmpty(personInfo.AvailableDay) ||
                string.IsNullOrEmpty(personInfo.AvailableTime))
            {
                ModelState.AddModelError("", "Please select a valid day and time.");
                return View(personInfo);
            }

            if (!ModelState.IsValid)
                return View(personInfo);

            // Parse date/time
            if (!DateTime.TryParseExact(
                    personInfo.AvailableDay,
                    "yyyy-MM-dd",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out var date)
                || !TimeSpan.TryParse(personInfo.AvailableTime, out var time))
            {
                ModelState.AddModelError("", "Invalid date or time format.");
                return View(personInfo);
            }

            // Normalize a human-friendly time string (e.g. "9:30 am")
            var formattedTime = date.Add(time).ToString("h:mm tt").ToLower(); // "9:30 am"

            // Slot availability check (exclude if same person to allow update)
            if (IsSlotTaken(date, time, personInfo.Name))
            {
                ModelState.AddModelError("AvailableTime",
                    $"The slot on {date:MM/dd/yyyy} at {date.Add(time):hh:mm tt} is already taken.");
                return View(personInfo);
            }

            // SAVE / UPDATE PERSON
            PersonInfo existingPersonEntity = null;

            if (isNewPatient)
            {
                personInfo.AvailableTime = formattedTime;
                personInfo.AvailableDay = date.ToString("yyyy-MM-dd");

                // mark walk-in state -> REGISTERED (removed Pre-register)
                personInfo.WalkInStatus = "Registered";

                _context.PersonInfo.Add(personInfo);
                await _context.SaveChangesAsync(); // generate Id
            }
            else
            {
                existingPersonEntity = await _context.PersonInfo.FindAsync(personInfo.Id);
                if (existingPersonEntity == null)
                    return NotFound();

                // Clear OLD slot if changing time
                var oldSlot = _context.TodaySchedule.FirstOrDefault(s => s.PersonName == existingPersonEntity.Name);
                if (oldSlot != null) oldSlot.PersonName = "Available Slots";

                existingPersonEntity.Name = personInfo.Name;
                existingPersonEntity.AvailableDay = date.ToString("yyyy-MM-dd");
                existingPersonEntity.AvailableTime = formattedTime;
                existingPersonEntity.BirthDay = personInfo.BirthDay;
                existingPersonEntity.Age = personInfo.Age;
                existingPersonEntity.HomeAddress = personInfo.HomeAddress;
                existingPersonEntity.MobileNumber = personInfo.MobileNumber;
                existingPersonEntity.EmailAddress = personInfo.EmailAddress;
                existingPersonEntity.MedicalServices = personInfo.MedicalServices;

                // mark walk-in state -> REGISTERED (removed Pre-register)
                existingPersonEntity.WalkInStatus = "Registered";

                _context.PersonInfo.Update(existingPersonEntity);
            }

            // ASSIGN SLOT SAFELY
            var slot = _context.TodaySchedule.FirstOrDefault(s =>
                s.EventDate.Date == date.Date &&
                s.EventTime == time);

            if (slot != null &&
                (string.IsNullOrEmpty(slot.PersonName) ||
                 slot.PersonName.Equals("Available Slots", StringComparison.OrdinalIgnoreCase)))
            {
                slot.PersonName = personInfo.Name;
                _context.TodaySchedule.Update(slot);
            }
            else if (slot == null)
            {
                _context.TodaySchedule.Add(new TodaySchedule
                {
                    EventDate = date,
                    EventTime = time,
                    PersonName = personInfo.Name
                });
            }
            else
            {
                ModelState.AddModelError("AvailableTime", "This time slot is already taken.");
                return View(personInfo);
            }

            await _context.SaveChangesAsync();

            // Helper to parse CSV of service IDs
            static List<int> ParseServiceIds(string servicesCsv)
            {
                return (servicesCsv ?? string.Empty)
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(s => int.TryParse(s, out var id) ? id : (int?)null)
                    .Where(id => id.HasValue)
                    .Select(id => id.Value)
                    .Distinct()
                    .ToList();
            }

            var serviceIds = ParseServiceIds(personInfo.MedicalServices);

            // PAYMENT (NEW ONLY) — only create payment when at least one service was selected
            if (isNewPatient && serviceIds.Any())
            {
                await CreatePaymentAsync(personInfo, serviceIds);
            }

            TempData["SuccessMessage"] =
                 $"Appointment booked on {date:MM/dd/yyyy} at {date.Add(time):h:mm tt}".ToLower();

            // Redirect to Today's Schedules index after creating/updating an appointment
            return RedirectToAction("Index", "TodaySchedules");
        }

        // ============================
        // AJAX: Available Times
        // ============================
        [HttpGet]
        public JsonResult GetAvailableTimes(string selectedDate)
        {
            var options = new List<SelectListItem>();

            if (DateTime.TryParse(selectedDate, out var date))
            {
                var availableSlots = _context.TodaySchedule
                 .Where(s =>
                     s.EventDate >= date.Date && s.EventDate < date.Date.AddDays(1) &&
                     (s.PersonName == null || s.PersonName.ToLower() == "available slots"))
                 .OrderBy(s => s.EventTime)
                 .Select(s => new SelectListItem
                 {
                     // ensure the returned Value is a normalized time string you can parse easily (hh:mm)
                     Value = s.EventTime.ToString(@"hh\:mm"),
                     Text = date.Add(s.EventTime).ToString("h:mm tt").ToLower()
                 })
                 .ToList();

                options = availableSlots;
            }

            return Json(options.Select(o => new { o.Text, o.Value }));
        }

        // ============================
        // AJAX: Patient Search
        // ============================
        [HttpGet]
        public async Task<JsonResult> GetPatients(string q = "", int page = 1, int pageSize = 20)
        {
            var query = _context.PersonInfo.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                query = query.Where(p =>
                    p.Name.Contains(q) ||
                    p.MobileNumber.Contains(q) ||
                    p.EmailAddress.Contains(q));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    mobile = p.MobileNumber,
                    email = p.EmailAddress,
                    address = p.HomeAddress,
                    birthDate = p.BirthDay,
                    availableDay = p.AvailableDay,
                    availableTime = p.AvailableTime
                })
                .ToListAsync();

            return Json(new { items, totalCount });
        }

        // ============================
        // AJAX: Quick Update (MODAL)
        //  - Persist person updates
        //  - If medical service is provided (not "N/A"), create a Payment
        //  - If preferred date/time provided, attempt to move the person's slot
        // ============================
        [HttpPost]
        public async Task<IActionResult> QuickUpdate([FromBody] PersonInfo model)
        {
            if (model == null || model.Id == 0)
                return Json(new { success = false, error = "Invalid payload" });

            var patient = await _context.PersonInfo.FindAsync(model.Id);
            if (patient == null) return Json(new { success = false, error = "Patient not found" });

            try
            {
                // capture previous identifying fields so we can update TodaySchedule entries
                var previousName = patient.Name;
                var previousAvailableDay = patient.AvailableDay;
                var previousAvailableTime = patient.AvailableTime;

                // update basic fields...
                patient.Name = model.Name ?? patient.Name;
                patient.HomeAddress = model.HomeAddress ?? patient.HomeAddress;
                patient.MobileNumber = model.MobileNumber ?? patient.MobileNumber;
                patient.EmailAddress = model.EmailAddress ?? patient.EmailAddress;
                patient.MedicalServices = string.IsNullOrEmpty(model.MedicalServices) ? patient.MedicalServices : model.MedicalServices;

                // If the modal supplied a preferred day/time, apply them and move slot
                DateTime? newDate = null;
                TimeSpan? newTime = null;
                if (!string.IsNullOrWhiteSpace(model.AvailableDay) && !string.IsNullOrWhiteSpace(model.AvailableTime))
                {
                    // model.AvailableDay expected "yyyy-MM-dd", model.AvailableTime expected "HH:mm"
                    if (DateTime.TryParseExact(model.AvailableDay, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)
                        && TimeSpan.TryParse(model.AvailableTime, out var ts))
                    {
                        newDate = d.Date;
                        newTime = ts;
                        // store human friendly strings consistent with Index handler
                        patient.AvailableDay = d.ToString("yyyy-MM-dd");
                        patient.AvailableTime = d.Add(ts).ToString("h:mm tt").ToLower();
                    }
                }

                // mark walk-in when appointment/payment completed
                if (!string.IsNullOrWhiteSpace(model.MedicalServices) &&
                    !string.Equals(model.MedicalServices.Trim(), "N/A", StringComparison.OrdinalIgnoreCase))
                {
                    patient.WalkInStatus = "Registered";
                }
                else
                {
                    patient.WalkInStatus = model.WalkInStatus ?? patient.WalkInStatus;
                }

                // Persist patient changes first so we have latest Name/AvailableDay/Time
                _context.PersonInfo.Update(patient);
                await _context.SaveChangesAsync();

                // If we have a new date/time, update TodaySchedule:
                if (newDate.HasValue && newTime.HasValue)
                {
                    // Clear any existing TodaySchedule entries that referenced the previous name
                    if (!string.IsNullOrWhiteSpace(previousName))
                    {
                        var oldSlots = await _context.TodaySchedule
                            .Where(s => s.PersonName == previousName)
                            .ToListAsync();

                        foreach (var s in oldSlots)
                        {
                            s.PersonName = "Available Slots";
                            _context.TodaySchedule.Update(s);
                        }
                    }

                    // Assign the requested slot
                    var slot = await _context.TodaySchedule
                        .FirstOrDefaultAsync(s => s.EventDate.Date == newDate.Value.Date && s.EventTime == newTime.Value);

                    if (slot != null)
                    {
                        // If the slot is free/available -> assign; otherwise replace only if it was our own (should be free because we cleared)
                        if (string.IsNullOrWhiteSpace(slot.PersonName) || slot.PersonName.Equals("Available Slots", StringComparison.OrdinalIgnoreCase))
                        {
                            slot.PersonName = patient.Name;
                            _context.TodaySchedule.Update(slot);
                        }
                        else
                        {
                            // slot taken by someone else - do not overwrite, return informative error
                            return Json(new { success = false, error = "Selected slot is already taken by another patient." });
                        }
                    }
                    else
                    {
                        // create the slot and assign
                        _context.TodaySchedule.Add(new TodaySchedule
                        {
                            EventDate = newDate.Value,
                            EventTime = newTime.Value,
                            PersonName = patient.Name
                        });
                    }

                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        // ============================
        // AJAX: Toggle Schedule Status
        // ============================
        // small DTO for binding
        public class ToggleScheduleRequest
        {
            public int scheduleId { get; set; }
        }

        private async Task EnsureDefaultServiceAsync()
        {
            if (!await _context.Services.AnyAsync())
            {
                _context.Services.Add(new Services { MedicalNameService = "General Cleaning", Price = 100.00m });
                await _context.SaveChangesAsync();
            }
        }

        [HttpPost]
        public async Task<JsonResult> ToggleScheduleStatus([FromBody] ToggleScheduleRequest req)
        {
            if (req == null || req.scheduleId <= 0)
                return Json(new { success = false, error = "Invalid payload" });

            await EnsureDefaultServiceAsync();

            var slot = await _context.TodaySchedule.FindAsync(req.scheduleId);
            if (slot == null)
                return Json(new { success = false, error = "Schedule not found" });

            var current = (slot.Status ?? "Waiting").Trim().ToLowerInvariant();
            string next;
            switch (current)
            {
                case "waiting":
                    next = "On-going";
                    break;
                case "on-going":
                case "ongoing":
                    next = "Done";
                    break;
                case "done":
                default:
                    next = "Waiting";
                    break;
            }

            slot.Status = next;
            _context.TodaySchedule.Update(slot);
            await _context.SaveChangesAsync();

            bool paymentCreated = false;
            PersonInfo person = null;

            // Try to resolve the PersonInfo robustly. Prefer exact trimmed case-insensitive match,
            // fallback to contains match if exact lookup fails.
            if (!string.IsNullOrWhiteSpace(slot.PersonName) &&
                !slot.PersonName.Equals("Available Slots", StringComparison.OrdinalIgnoreCase))
            {
                var trimmedName = slot.PersonName.Trim();
                var trimmedLower = trimmedName.ToLowerInvariant();

                // exact case-insensitive match using ToLower (translatable to SQL)
                person = await _context.PersonInfo
                    .FirstOrDefaultAsync(p => p.Name != null && p.Name.ToLower() == trimmedLower);

                if (person == null)
                {
                    // fallback: case-insensitive contains (also translatable)
                    person = await _context.PersonInfo
                        .Where(p => p.Name != null && p.Name.ToLower().Contains(trimmedLower))
                        .FirstOrDefaultAsync();
                }
            }

            // Only attempt to create payment when status becomes Done and we found a person
            if (string.Equals(next, "Done", StringComparison.OrdinalIgnoreCase) && person != null)
            {
                var serviceIds = await ResolveServiceIdsAsync(person.MedicalServices);

                // Ensure at least one service id exists (fallback to first service)
                if (!serviceIds.Any())
                {
                    var first = await _context.Services.OrderBy(s => s.Id).Select(s => s.Id).FirstOrDefaultAsync();
                    if (first != 0) serviceIds.Add(first);
                }

                // Existing behavior prevented duplicate unpaid payments. Keep that guard by default.
                // If you prefer creating a payment every time a schedule reaches Done, remove the hasUnpaid check.
                var hasUnpaid = await _context.Payments.AnyAsync(p => p.PersonInfoId == person.Id && p.Status == "Unpaid");
                if (!hasUnpaid && serviceIds.Any())
                {
                    await CreatePaymentAsync(person, serviceIds);
                    paymentCreated = true;
                }
            }

            // When status becomes Done return person + services (same contract as before)
            if (string.Equals(next, "Done", StringComparison.OrdinalIgnoreCase))
            {
                var personDto = person == null ? null : new
                {
                    id = person.Id,
                    name = person.Name,
                    medicalServices = person.MedicalServices
                };

                var services = await _context.Services
                    .OrderBy(s => s.MedicalNameService)
                    .Select(s => new { Value = s.Id.ToString(), Text = $"{s.MedicalNameService} - ₱{s.Price}" })
                    .ToListAsync();

                return Json(new
                {
                    success = true,
                    status = next,
                    rebook = true,
                    paymentCreated,
                    scheduleId = slot.Id,
                    person = personDto,
                    services
                });
            }

            return Json(new { success = true, status = next, rebook = false, paymentCreated, scheduleId = slot.Id });
        }

        [HttpGet]
        public async Task<JsonResult> GetServices()
        {
            await EnsureDefaultServiceAsync();
            var services = await _context.Services
                .OrderBy(s => s.MedicalNameService)
                .Select(s => new { Value = s.Id.ToString(), Text = $"{s.MedicalNameService} - ₱{s.Price}" })
                .ToListAsync();

            return Json(services);
        }



        // ============================
        // HELPERS
        // ============================
        private List<SelectListItem> GetAvailableDays()
        {
            return _context.TodaySchedule
                .Where(s => s.EventDate.Date >= DateTime.Today)
                .GroupBy(s => s.EventDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Count = g.Count(s =>
                        string.IsNullOrEmpty(s.PersonName) ||
                        s.PersonName.ToLower() == "available slots")
                })
                .Where(x => x.Count > 0)
                .Select(x => new SelectListItem
                {
                    Text = $"{x.Date:MM/dd/yyyy} ({x.Count} slots available)",
                    Value = x.Date.ToString("yyyy-MM-dd")
                })
                .ToList();
        }


        private int CalculateAge(DateTime birthDate)
        {
            int age = DateTime.Today.Year - birthDate.Year;
            if (DateTime.Today.DayOfYear < birthDate.DayOfYear)
                age--;
            return age;
        }

        private bool IsSlotTaken(DateTime date, TimeSpan time, string excludePerson = null)
        {
            return _context.TodaySchedule.Any(s =>
                s.EventDate.Date == date.Date &&
                s.EventTime == time &&
                s.PersonName.ToLower() != "available slots" &&
                (excludePerson == null || s.PersonName != excludePerson));
        }

        private async Task<Payment> CreatePaymentAsync(PersonInfo person, IEnumerable<int> serviceIds)
        {
            var serviceIdList = serviceIds?.Distinct().ToList() ?? new List<int>();
            var services = await _context.Services
                .Where(s => serviceIdList.Contains(s.Id))
                .ToListAsync();

            var payment = new Payment
            {
                DoctorName = "DR. NINA RICCI ANTIPALA-RIVERA",
                Status = "Unpaid",
                PaymentMethod = "Cash",
                PaymentDate = DateTime.Now,
                PersonInfoId = person.Id,
                ServicesId = services.Count == 1 ? services[0].Id : (int?)null // keep legacy column for single-service
            };

            foreach (var svc in services)
            {
                payment.PaymentServices.Add(new PaymentService
                {
                    ServiceId = svc.Id,
                    UnitPrice = svc.Price
                });
            }

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return payment;
        }

        private async Task<List<int>> ResolveServiceIdsAsync(string servicesCsv)
        {
            var parts = (servicesCsv ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();

            // numeric IDs first
            var ids = parts
                .Select(p => int.TryParse(p, out var id) ? id : (int?)null)
                .Where(x => x.HasValue)
                .Select(x => x.Value)
                .ToList();

            // match by name if no numeric
            if (!ids.Any() && parts.Any())
            {
                var matchedByName = await _context.Services
                    .Where(s => parts.Contains(s.MedicalNameService))
                    .Select(s => s.Id)
                    .ToListAsync();
                ids.AddRange(matchedByName);
            }

            // fallback: first available service
            if (!ids.Any())
            {
                var first = await _context.Services
                    .OrderBy(s => s.Id)
                    .Select(s => s.Id)
                    .FirstOrDefaultAsync();
                if (first != 0) ids.Add(first);
            }

            return ids.Distinct().ToList();
        }
    }
}
