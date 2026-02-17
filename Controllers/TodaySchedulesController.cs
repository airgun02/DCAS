using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DCAS.Data;
using DCAS.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DCAS.Controllers
{
    public class TodaySchedulesController : Controller
    {
        private readonly DCASContext _context;

        public TodaySchedulesController(DCASContext context)
        {
            _context = context;
        }

        // GET: TodaySchedules
        public async Task<IActionResult> Index(int page = 0)
        {
            var schedules = await _context.TodaySchedule.ToListAsync();

            if (!schedules.Any())
            {
                ViewBag.Dates = new List<DateTime>();
                ViewBag.Times = new List<TimeSpan>();
                ViewBag.ScheduleMap = new Dictionary<TimeSpan, Dictionary<DateTime, string>>();
                ViewBag.CurrentPage = 0;
                ViewBag.Message = "No schedules found. Create some entries to see the schedule grid.";

                // still provide services so modal select renders
                ViewBag.MedicalServices = await _context.Services
                    .OrderBy(s => s.MedicalNameService)
                    .Select(s => new SelectListItem
                    {
                        Text = $"{s.MedicalNameService} - ₱{s.Price:F2}",
                        Value = s.Id.ToString()
                    })
                    .ToListAsync();

                return View();
            }

            var today = DateTime.Today;

            var allDates = schedules
                .Select(s => s.EventDate.Date)
                .Where(d => d >= today)
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            var times = schedules
                .Select(s => s.EventTime)
                .Distinct()
                .OrderBy(t => t)
                .ToList();

            var pageSize = 5;
            var totalPages = (int)Math.Ceiling((double)allDates.Count / pageSize);

            if (page < 0) page = 0;
            if (page >= totalPages && totalPages > 0) page = totalPages - 1;

            var dates = allDates.Skip(page * pageSize).Take(pageSize).ToList();

            var scheduleMap = new Dictionary<TimeSpan, Dictionary<DateTime, string>>();
            foreach (var time in times)
            {
                scheduleMap[time] = new Dictionary<DateTime, string>();
                foreach (var date in dates)
                {
                    var schedule = schedules.FirstOrDefault(s =>
                        s.EventDate.Date == date && s.EventTime == time);
                    scheduleMap[time][date] = schedule?.PersonName ?? "Time Slots";
                }
            }

            // Provide services list to the view (important)
            ViewBag.MedicalServices = await _context.Services
                .OrderBy(s => s.MedicalNameService)
                .Select(s => new SelectListItem
                {
                    Text = $"{s.MedicalNameService} - ₱{s.Price:F2}",
                    Value = s.Id.ToString()
                })
                .ToListAsync();

            ViewBag.Dates = dates;
            ViewBag.AllDates = allDates;
            ViewBag.Times = times;
            ViewBag.ScheduleMap = scheduleMap;
            ViewBag.CurrentPage = page;
            ViewBag.Message = TempData["Message"];

            return View();
        }

        // GET: TodaySchedules/Create
        public IActionResult Create()
        {
            var model = new TodaySchedule
            {
                EventDate = DateTime.Today,
                EventTime = new TimeSpan(8, 0, 0), // 8:00 AM default
                PersonName = "Available Slots"
            };

            return View(model);
        }

        // POST: TodaySchedules/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EventDate,EventTime,PersonName")] TodaySchedule todaySchedule, string selectedTimes)
        {
            if (!string.IsNullOrWhiteSpace(selectedTimes))
            {
                // Multiple-create flow: selectedTimes is CSV "08:00,08:30,..."
                var times = selectedTimes.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(t =>
                    {
                        if (TimeSpan.TryParse(t, out var ts)) return (success: true, ts);
                        return (success: false, ts: TimeSpan.Zero);
                    })
                    .ToList();

                if (times.Any(x => !x.success))
                {
                    ModelState.AddModelError("", "One or more selected times are invalid.");
                    return View(todaySchedule);
                }

                var dateOnly = todaySchedule.EventDate.Date;

                // check for conflicts
                var requestedTimeSpans = times.Select(x => x.ts).ToList();
                var existingConflicts = await _context.TodaySchedule
                    .Where(s => s.EventDate.Date == dateOnly && requestedTimeSpans.Contains(s.EventTime))
                    .Select(s => s.EventTime)
                    .ToListAsync();

                if (existingConflicts.Any())
                {
                    var conflictList = string.Join(", ", existingConflicts.Select(t => t.ToString(@"hh\:mm")));
                    ModelState.AddModelError("", $"The following time slot(s) are already taken: {conflictList}. Please adjust your selection.");
                    return View(todaySchedule);
                }

                // create multiple entries
                var entities = requestedTimeSpans.Select(ts => new TodaySchedule
                {
                    EventDate = dateOnly,
                    EventTime = ts,
                    PersonName = string.IsNullOrWhiteSpace(todaySchedule.PersonName) ? "Available Slots" : todaySchedule.PersonName
                }).ToList();

                _context.AddRange(entities);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Single-create fallback (existing behavior)
            if (ModelState.IsValid)
            {
                // Check if this time slot is already taken
                var existingSchedule = await _context.TodaySchedule
                    .FirstOrDefaultAsync(s => s.EventDate.Date == todaySchedule.EventDate.Date
                                           && s.EventTime == todaySchedule.EventTime);

                if (existingSchedule != null)
                {
                    ModelState.AddModelError("", "This Time/Date slot have already created. Please choose a different time.");
                    return View(todaySchedule);
                }

                _context.Add(todaySchedule);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(todaySchedule);
        }

        // Method to get available time slots
        public async Task<IActionResult> GetAvailableSlots(DateTime date)
        {
            var bookedTimes = await _context.TodaySchedule
                .Where(s => s.EventDate.Date == date.Date)
                .Select(s => s.EventTime)
                .ToListAsync();

            // Define standard business hours (8 AM to 6 PM, 30-minute intervals)
            var allTimeSlots = new List<TimeSpan>();
            for (int hour = 8; hour < 18; hour++)
            {
                allTimeSlots.Add(new TimeSpan(hour, 0, 0));
                allTimeSlots.Add(new TimeSpan(hour, 30, 0));
            }

            var availableSlots = allTimeSlots.Except(bookedTimes).ToList();

            return Json(availableSlots.Select(t => new {
                Value = t.ToString(@"hh\:mm"),
                Text = DateTime.Today.Add(t).ToString("h:mm tt")
            }));
        }
    }
}