using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DCAS.Data;
using DCAS.Models;
using System.Globalization;

namespace DCAS.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly DCASContext _context;

        public PaymentsController(DCASContext context)
        {
            _context = context;
        }

        // GET: Payments
        public async Task<IActionResult> Index()
        {
            var payments = await _context.Payments
                .Include(p => p.Person)
                .Include(p => p.PaymentServices).ThenInclude(ps => ps.Service)
                .Where(p => p.Status == "Unpaid")
                .ToListAsync();

            return View(payments);
        }

        public async Task<IActionResult> PaymentHistory()
        {
            var paidPayments = await _context.Payments
                .Include(p => p.Person)
                .Include(p => p.PaymentServices).ThenInclude(ps => ps.Service)
                .Include(p => p.PaymentMedicine)
                .Where(p => p.Status == "Paid")
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            return View(paidPayments);
        }

        // GET: Payments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var payment = await _context.Payments
                .Include(p => p.PaymentMedicine)
                .Include(p => p.Person)
                .Include(p => p.PaymentServices).ThenInclude(ps => ps.Service)
                .FirstOrDefaultAsync(m => m.PaymentId == id);

            if (payment == null) return NotFound();

            return View(payment);
        }


        // GET: Payments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var payment = await _context.Payments
                .Include(p => p.PaymentMedicine)
                .Include(p => p.PaymentServices)
                .FirstOrDefaultAsync(m => m.PaymentId == id);

            if (payment == null) return NotFound();

            var allServices = await _context.Services.ToListAsync();
            ViewBag.Services = allServices;
            ViewBag.SelectedServiceIds = payment.PaymentServices.Select(ps => ps.ServiceId).ToArray();
            ViewBag.MedicineInventory = await _context.MedicineInventory.ToListAsync();

            return View(payment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Payment payment, int[] MedicineId, int[] MedicineQuantity, int[] servicesIds)
        {
            if (id != payment.PaymentId) return NotFound();

            ModelState.Remove("Status");
            ModelState.Remove("AmountPaid");
            ModelState.Remove("AmountChanged");
            ModelState.Remove("PaymentDate");
            ModelState.Remove("Person");
            ModelState.Remove("Service");
            ModelState.Remove("PaymentMedicine");
            ModelState.Remove("PaymentServices");

            if (!ModelState.IsValid)
            {
                await RehydrateEditViewBags(payment, servicesIds);
                return View(payment);
            }

            try
            {
                var existingPayment = await _context.Payments
                    .Include(p => p.PaymentMedicine)
                    .Include(p => p.PaymentServices)
                    .FirstOrDefaultAsync(p => p.PaymentId == id);

                if (existingPayment == null) return NotFound();

                // Update basics
                existingPayment.Cash = payment.Cash;
                existingPayment.DoctorName = payment.DoctorName;
                existingPayment.PaymentMethod = payment.PaymentMethod;
                existingPayment.DentistFee = payment.DentistFee;
                existingPayment.PaymentDate = DateTime.Today;

                // Services total
                servicesIds = servicesIds ?? Array.Empty<int>();
                var selectedServices = await _context.Services
                    .Where(s => servicesIds.Contains(s.Id))
                    .ToListAsync();

                decimal serviceTotal = selectedServices.Sum(s => s.Price);

                // pick a primary service id to satisfy non-null DB column
                var primaryServiceId = selectedServices.FirstOrDefault()?.Id;
                existingPayment.ServicesId = primaryServiceId ?? existingPayment.ServicesId;

                // replace PaymentServices as before
                if (existingPayment.PaymentServices.Any())
                {
                    _context.PaymentServices.RemoveRange(existingPayment.PaymentServices);
                }
                foreach (var svc in selectedServices)
                {
                    existingPayment.PaymentServices.Add(new PaymentService
                    {
                        ServiceId = svc.Id,
                        UnitPrice = svc.Price
                    });
                }

                // Medicines: handle MedicineId[] and MedicineQuantity[] (unchanged)
                if (existingPayment.PaymentMedicine.Any())
                {
                    _context.PaymentMedicine.RemoveRange(existingPayment.PaymentMedicine);
                }

                decimal totalMedicinePrice = 0;
                if (MedicineId != null && MedicineId.Length > 0)
                {
                    MedicineQuantity = MedicineQuantity ?? Array.Empty<int>();
                    var requestedQty = new Dictionary<int, int>();
                    for (int i = 0; i < MedicineId.Length; i++)
                    {
                        var mid = MedicineId[i];
                        var qty = (i < MedicineQuantity.Length) ? Math.Max(1, MedicineQuantity[i]) : 1;
                        requestedQty[mid] = qty;
                    }

                    var meds = await _context.MedicineInventory
                        .Where(m => requestedQty.Keys.Contains(m.Id))
                        .ToListAsync();

                    var insufficient = meds.Where(m => m.Quantity < requestedQty[m.Id]).ToList();
                    if (insufficient.Any())
                    {
                        var msg = string.Join(", ", insufficient.Select(m => $"{m.MedicineName} (available {m.Quantity})"));
                        ModelState.AddModelError("", $"Insufficient stock for: {msg}. Please adjust quantities.");
                        await RehydrateEditViewBags(payment, servicesIds);
                        return View(payment);
                    }

                    foreach (var med in meds)
                    {
                        var qty = requestedQty[med.Id];
                        var lineTotal = med.Price * qty;
                        totalMedicinePrice += lineTotal;

                        existingPayment.PaymentMedicine.Add(new PaymentMedicine
                        {
                            PaymentId = id,
                            MedicineId = med.Id,
                            MedicineName = med.MedicineName,
                            UnitPrice = med.Price,
                            Quantity = qty,
                            Price = lineTotal
                        });

                        med.Quantity -= qty;
                        _context.MedicineInventory.Update(med);
                    }
                }

                // Totals
                decimal totalAmountDue = serviceTotal + existingPayment.DentistFee + totalMedicinePrice;
                existingPayment.AmountPaid = totalAmountDue;
                existingPayment.AmountChanged = existingPayment.Cash - totalAmountDue;
                existingPayment.Status = "Paid";

                _context.Payments.Update(existingPayment);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                var innerMsg = ex.InnerException?.Message ?? ex.Message;
                ModelState.AddModelError("", $"Error saving: {innerMsg}");
            }

            await RehydrateEditViewBags(payment, servicesIds);
            return View(payment);
        }

        private async Task RehydrateEditViewBags(Payment payment, int[] servicesIds)
        {
            var allServices = await _context.Services.ToListAsync();
            ViewBag.Services = allServices;
            ViewBag.SelectedServiceIds = servicesIds ?? Array.Empty<int>();
            ViewBag.MedicineInventory = await _context.MedicineInventory.ToListAsync();
        }

        // GET: Payments/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var payment = await _context.Payments
                .Include(p => p.Person)
                .Include(p => p.Service)
                .FirstOrDefaultAsync(p => p.PaymentId == id);

            if (payment == null) return NotFound();

            return View(payment);
        }

        // POST: Payments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var payment = await _context.Payments
                 .Include(p => p.PaymentServices)
                 .Include(p => p.PaymentMedicine)
                 .FirstOrDefaultAsync(p => p.PaymentId == id);

            if (payment != null)
            {
                if (payment.PaymentServices?.Any() == true)
                    _context.PaymentServices.RemoveRange(payment.PaymentServices);

                if (payment.PaymentMedicine?.Any() == true)
                    _context.PaymentMedicine.RemoveRange(payment.PaymentMedicine);

                _context.Payments.Remove(payment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Payments/WalkIn?q=searchTerm
        // Shows clients with WalkInStatus == "Registered" (and related records)
        public async Task<IActionResult> WalkIn(string q)
        {
            var query = _context.PersonInfo
                .Where(p =>
                    p.WalkInStatus == "Registered"
                    || _context.TodaySchedule.Any(ts => ts.PersonName == p.Name)
                    || _context.Payments.Any(pay => pay.PersonInfoId == p.Id)
                );

            if (!string.IsNullOrWhiteSpace(q))
            {
                var trimmed = q.Trim();
                query = query.Where(p =>
                    (p.Name != null && p.Name.Contains(trimmed)) ||
                    (p.MobileNumber != null && p.MobileNumber.Contains(trimmed)) ||
                    (p.EmailAddress != null && p.EmailAddress.Contains(trimmed)));
            }

            var list = await query.OrderBy(p => p.Name).ToListAsync();
            ViewBag.Query = q ?? string.Empty;
            return View(list);
        }

        // POST: Payments/RegisterWalkIn (AJAX) - updates a PersonInfo.WalkInStatus to "Registered"
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterWalkIn(int id)
        {
            var person = await _context.PersonInfo.FirstOrDefaultAsync(p => p.Id == id);
            if (person == null) return Json(new { success = false, error = "Client not found." });

            person.WalkInStatus = "Registered";
            _context.PersonInfo.Update(person);
            await _context.SaveChangesAsync();

            return Json(new { success = true, id = id, status = person.WalkInStatus });
        }

        // POST: Payments/CreateWalkInNew (AJAX) - accepts full profile except AvailableDay, AvailableTime, HomeNumber, Date (Date of visit)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateWalkInNew()
        {
            // form fields are posted from the modal; read Request.Form and copy only allowed fields
            var form = Request.Form;
            var name = (form["Name"].ToString() ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name))
                return Json(new { success = false, error = "Name is required." });

            // Build PersonInfo instance using posted values but intentionally exclude AvailableDay, AvailableTime, HomeNumber and Date
            var person = new PersonInfo
            {
                Name = name,
                DetailOnee = string.IsNullOrWhiteSpace(form["DetailOnee"]) ? "N/A" : form["DetailOnee"].ToString(),
                HomeAddress = string.IsNullOrWhiteSpace(form["HomeAddress"]) ? null : form["HomeAddress"].ToString(),
                BirthDay = DateTime.TryParse(form["BirthDay"], out var bd) ? bd : (DateTime?)null,
                Age = int.TryParse(form["Age"], out var ageVal) ? ageVal : (int?)null,
                Occupation = string.IsNullOrWhiteSpace(form["Occupation"]) ? null : form["Occupation"].ToString(),
                // HomeNumber intentionally omitted per request
                HomeNumber = null,
                MobileNumber = string.IsNullOrWhiteSpace(form["MobileNumber"]) ? string.Empty : form["MobileNumber"].ToString(),
                OfficeAddress = string.IsNullOrWhiteSpace(form["OfficeAddress"]) ? null : form["OfficeAddress"].ToString(),
                EmailAddress = string.IsNullOrWhiteSpace(form["EmailAddress"]) ? null : form["EmailAddress"].ToString(),
                // Use posted Status (marital) if provided; otherwise leave null
                Status = string.IsNullOrWhiteSpace(form["Status"]) ? null : form["Status"].ToString(),
                NameOfSpouse = string.IsNullOrWhiteSpace(form["NameOfSpouse"]) ? string.Empty : form["NameOfSpouse"].ToString(),
                PersonalResponsibleforAccount = string.IsNullOrWhiteSpace(form["PersonalResponsibleforAccount"]) ? string.Empty : form["PersonalResponsibleforAccount"].ToString(),
                Relationship = string.IsNullOrWhiteSpace(form["Relationship"]) ? string.Empty : form["Relationship"].ToString(),
                DetailTwoo = string.IsNullOrWhiteSpace(form["DetailTwoo"]) ? "N/A" : form["DetailTwoo"].ToString(),
                PhysicianCare = string.IsNullOrWhiteSpace(form["PhysicianCare"]) ? string.Empty : form["PhysicianCare"].ToString(),
                PhysicianName = string.IsNullOrWhiteSpace(form["PhysicianName"]) ? string.Empty : form["PhysicianName"].ToString(),
                ContactNumber = string.IsNullOrWhiteSpace(form["ContactNumber"]) ? string.Empty : form["ContactNumber"].ToString(),
                MedicalServices = string.IsNullOrWhiteSpace(form["MedicalServices"]) ? "N/A" : form["MedicalServices"].ToString(),
                Price = decimal.TryParse(form["Price"], out var p) ? p : 0m,
                DetailOne = string.IsNullOrWhiteSpace(form["DetailOne"]) ? "N/A" : form["DetailOne"].ToString(),
                DetailTwo = string.IsNullOrWhiteSpace(form["DetailTwo"]) ? "N/A" : form["DetailTwo"].ToString(),
                DetailThree = string.IsNullOrWhiteSpace(form["DetailThree"]) ? "N/A" : form["DetailThree"].ToString(),
                DetailFour = string.IsNullOrWhiteSpace(form["DetailFour"]) ? "N/A" : form["DetailFour"].ToString(),
                DetailFive = string.IsNullOrWhiteSpace(form["DetailFive"]) ? "N/A" : form["DetailFive"].ToString(),
                DetailSix = string.IsNullOrWhiteSpace(form["DetailSix"]) ? "N/A" : form["DetailSix"].ToString(),
                DetailSeven = string.IsNullOrWhiteSpace(form["DetailSeven"]) ? "N/A" : form["DetailSeven"].ToString(),
                DetailEigth = string.IsNullOrWhiteSpace(form["DetailEigth"]) ? "N/A" : form["DetailEigth"].ToString(),
                // AvailableDay and AvailableTime intentionally not set
                AvailableDay = null,
                AvailableTime = null,
                // WalkInStatus defaults to Registered for walk-ins
                WalkInStatus = "Registered",
                // Date (Date of Visit) intentionally not set here per request
                Date = string.IsNullOrWhiteSpace(form["Date"])
                    ? (DateTime?)null
                    : (DateTime.TryParseExact(form["Date"], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedExact)
                        ? parsedExact
                        : (DateTime.TryParse(form["Date"], out var parsed) ? parsed : (DateTime?)null))
            };

            // Server-side validation: MobileNumber must be exactly 11 digits if provided
            // PersonInfo model already has RegularExpression attribute; ensure model is validated
            if (!TryValidateModel(person))
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).Where(m => !string.IsNullOrWhiteSpace(m));
                return Json(new { success = false, error = string.Join("; ", errors) });
            }

            _context.PersonInfo.Add(person);

            // pick a default service (first available) if any
            var defaultService = await _context.Services.OrderBy(s => s.Id).FirstOrDefaultAsync();

            // create a Payment for this person so they appear in Payments index (Unpaid)
            var payment = new Payment
            {
                Cash = 0m,
                DoctorName = "DR. NINA RICCI ANTIPALA-RIVERA",
                Status = "Unpaid",
                PaymentMethod = "Cash",
                AmountPaid = 0m,
                AmountChanged = 0m,
                DentistFee = 0m,
                PaymentDate = DateTime.Now,
                Person = person // EF will set PersonInfoId
            };

            if (defaultService != null)
            {
                payment.PaymentServices.Add(new PaymentService
                {
                    ServiceId = defaultService.Id,
                    UnitPrice = defaultService.Price
                });

                payment.ServicesId = defaultService.Id;
            }

            _context.Payments.Add(payment);

            await _context.SaveChangesAsync();

            return Json(new { success = true, id = person.Id, name = person.Name, paymentId = payment.PaymentId });
        }

        // Add this method to PaymentsController (near other POST actions)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterAndCreatePayment(int id)
        {
            var person = await _context.PersonInfo.FirstOrDefaultAsync(p => p.Id == id);
            if (person == null) return Json(new { success = false, error = "Client not found." });

            // mark registered
            person.WalkInStatus = "Registered";
            _context.PersonInfo.Update(person);

            // If there is already an unpaid payment for this person, do nothing (keep it)
            var existingPayment = await _context.Payments
                .FirstOrDefaultAsync(p => p.PersonInfoId == person.Id && p.Status == "Unpaid");

            if (existingPayment == null)
            {
                // choose a default service if available
                var defaultService = await _context.Services.OrderBy(s => s.Id).FirstOrDefaultAsync();

                var payment = new Payment
                {
                    Cash = 0m,
                    DoctorName = "DR. NINA RICCI ANTIPALA-RIVERA",
                    Status = "Unpaid",
                    PaymentMethod = "Cash",
                    AmountPaid = 0m,
                    AmountChanged = 0m,
                    DentistFee = 0m,
                    PaymentDate = DateTime.Now,
                    PersonInfoId = person.Id
                };

                if (defaultService != null)
                {
                    payment.PaymentServices.Add(new PaymentService
                    {
                        ServiceId = defaultService.Id,
                        UnitPrice = defaultService.Price
                    });

                    // keep legacy FK in sync
                    payment.ServicesId = defaultService.Id;
                }

                _context.Payments.Add(payment);
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, id = person.Id });
        }

        // DELETE actions unchanged...
        // GET: Payments/PatientHistory?personId=123
        [HttpGet]
        public async Task<IActionResult> PatientHistory(int personId)
        {
            var person = await _context.PersonInfo.FindAsync(personId);
            if (person == null) return NotFound();

            var payments = await _context.Payments
                .Where(p => p.PersonInfoId == personId)
                .Include(p => p.PaymentServices).ThenInclude(ps => ps.Service)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            ViewBag.PersonName = person.Name ?? "Unknown";
            return View(payments);
        }

        private bool PaymentExists(int id) => _context.Payments.Any(e => e.PaymentId == id);
    }
}