using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DCAS.Data;
using DCAS.Models;

namespace DCAS.Controllers
{
    public class ReschedulesController : Controller
    {
        private readonly DCASContext _context;

        public ReschedulesController(DCASContext context)
        {
            _context = context;
        }

        // GET: Reschedules
        public async Task<IActionResult> Index(string q)
        {
            var query = _context.PersonInfo.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                query = query.Where(p => p.Name.Contains(q));
                ViewBag.Query = q; // optional: expose for view if you want to show the search term
            }

            var list = await query.OrderBy(p => p.Name).ToListAsync();
            return View(list);
        }

        [HttpGet]
        public IActionResult GetById(int id)
        {
            var person = _context.PersonInfo.FirstOrDefault(p => p.Id == id);
            if (person == null)
                return NotFound();

            return Json(person);
        }

        [HttpPost]
        public IActionResult UpdateDetails(PersonInfo model)
        {
            if (model == null) return BadRequest();

            var person = _context.PersonInfo.Find(model.Id);
            if (person == null) return NotFound();

            // Update ONLY details fields
            person.DetailOnee = model.DetailOnee;
            person.DetailTwoo = model.DetailTwoo;
            person.DetailOne = model.DetailOne;
            person.DetailTwo = model.DetailTwo;
            person.DetailThree = model.DetailThree;
            person.DetailFour = model.DetailFour;
            person.DetailFive = model.DetailFive;
            person.DetailSix = model.DetailSix;
            person.DetailSeven = model.DetailSeven;
            person.DetailEigth = model.DetailEigth;

            _context.SaveChanges();

            return Json(new { success = true, message = "Details updated successfully." });
        }


        [HttpPost]
        public IActionResult Update(PersonInfo model)
        {
            var person = _context.PersonInfo.FirstOrDefault(p => p.Id == model.Id);
            if (person == null)
                return NotFound();

            // Store the old name before updating
            string oldName = person.Name;

            // Update PersonInfo fields (preserve WalkInStatus — do not overwrite from modal)
            person.Name = model.Name;
            person.HomeAddress = model.HomeAddress;
            person.Date = model.Date;
            person.BirthDay = model.BirthDay;
            person.Age = model.Age;
            person.HomeNumber = model.HomeNumber;
            person.Occupation = model.Occupation;
            person.MobileNumber = model.MobileNumber;
            person.OfficeAddress = model.OfficeAddress;
            person.EmailAddress = model.EmailAddress;
            // NOTE: intentionally do NOT set person.WalkInStatus here so the DB value is preserved
            person.NameOfSpouse = model.NameOfSpouse;
            person.PersonalResponsibleforAccount = model.PersonalResponsibleforAccount;
            person.Relationship = model.Relationship;
            person.PhysicianCare = model.PhysicianCare;
            person.PhysicianName = model.PhysicianName;
            person.ContactNumber = model.ContactNumber;
            person.AvailableDay = model.AvailableDay;
            person.AvailableTime = model.AvailableTime;

            // Validate and set marital status
            var allowedMarital = new[] { "", "Single", "Married", "Divorced", "Widowed" };
            if (!string.IsNullOrEmpty(model.Status) && !allowedMarital.Contains(model.Status))
            {
                // invalid value: ignore or return bad request
                ModelState.AddModelError("Status", "Invalid marital status.");
                return BadRequest(ModelState);
            }

            person.Status = string.IsNullOrWhiteSpace(model.Status) ? null : model.Status;

            // If the name was changed, update it in TodaySchedule as well
            if (oldName != model.Name && !string.IsNullOrEmpty(oldName))
            {
                var scheduleEntry = _context.TodaySchedule.FirstOrDefault(s => s.PersonName == oldName);
                if (scheduleEntry != null)
                {
                    scheduleEntry.PersonName = model.Name;
                    _context.TodaySchedule.Update(scheduleEntry);
                }
            }

            _context.SaveChanges();
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableDates()
        {
            var today = DateTime.Today;

            var availableDates = await _context.TodaySchedule
                .Where(s => s.PersonName == "Available Slots" && s.EventDate.Date >= today) 
                .Select(s => s.EventDate.Date)
                .Distinct()
                .OrderBy(d => d)
                .ToListAsync();

            var result = availableDates.Select(date => new
            {
                Value = date.ToString("yyyy-MM-dd"),
                Text = date.ToString("dddd, MMMM dd, yyyy"),
                AvailableSlots = _context.TodaySchedule.Count(s => s.EventDate.Date == date && s.PersonName == "Available Slots")
            }).ToList();

            return Json(result);
        }


        [HttpGet]
        public async Task<IActionResult> GetAvailableTimeSlots(DateTime date)
        {
            // Get time slots where PersonName = "Available Slots" for the selected date
            var availableSlots = await _context.TodaySchedule
                .Where(s => s.EventDate.Date == date.Date && s.PersonName == "Available Slots")
                .Select(s => s.EventTime)
                .OrderBy(t => t)
                .ToListAsync();

            var result = availableSlots.Select(t => new {
                Value = t.ToString(@"hh\:mm\:ss"),
                Text = DateTime.Today.Add(t).ToString("h:mm tt")
            }).ToList();

            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> RescheduleAppointment(int personId, DateTime newDate, TimeSpan newTime)
        {
            try
            {
                var person = await _context.PersonInfo.FirstOrDefaultAsync(p => p.Id == personId);
                if (person == null)
                    return Json(new { success = false, message = "Person not found." });

                // Find the current schedule for this person
                var currentSchedule = await _context.TodaySchedule
                    .FirstOrDefaultAsync(s => s.PersonName == person.Name);

                if (currentSchedule == null)
                    return Json(new { success = false, message = "Current schedule not found." });

                // Find the target available slot
                var targetSlot = await _context.TodaySchedule
                    .FirstOrDefaultAsync(s => s.EventDate.Date == newDate.Date &&
                                            s.EventTime == newTime &&
                                            s.PersonName == "Available Slots");

                if (targetSlot == null)
                    return Json(new { success = false, message = "Selected time slot is not available." });

                // Swap: Put person's name in the new slot and make old slot available
                targetSlot.PersonName = person.Name;
                currentSchedule.PersonName = "Available Slots";

                // Update PersonInfo with new date and time
                person.Date = newDate;
                person.AvailableTime = DateTime.Today.Add(newTime).ToString("h:mm tt"); // Format as readable time
                // Update AvailableDay with the date in dd/MM/yyyy format:
                person.AvailableDay = newDate.ToString("dd/MM/yyyy"); // e.g., "01/08/2025"

                // Update both TodaySchedule and PersonInfo
                _context.TodaySchedule.UpdateRange(currentSchedule, targetSlot);
                _context.PersonInfo.Update(person);

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = $"Appointment rescheduled successfully for {newDate.ToString("MMMM dd, yyyy")} at {DateTime.Today.Add(newTime).ToString("h:mm tt")}"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
    }
}