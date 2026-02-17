using DCAS.Data;
using DCAS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DCAS.Controllers
{
    public class MedicalServicesController : Controller
    {
        private readonly DCASContext _context;

        public MedicalServicesController(DCASContext context)
        {
            _context = context;
        }

        // GET: MedicalServices
        public async Task<IActionResult> Index()
        {
            var services = await _context.Services.ToListAsync();
            return View(services);
        }

        // POST: MedicalServices/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string MedicalNameService, decimal Price)
        {
            if (ModelState.IsValid)
            {
                var service = new Services
                {
                    MedicalNameService = MedicalNameService,
                    Price = Price
                };

                _context.Add(service);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "New medical service created successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View();
        }

        // GET: MedicalServices/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                return NotFound();
            }
            return View(service);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string MedicalNameService, decimal Price)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    service.MedicalNameService = MedicalNameService;
                    service.Price = Price;

                    _context.Update(service);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Medical service updated successfully!";
                    return RedirectToAction(nameof(Index)); // Adjust if needed
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceExists(service.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(service); 
        }

        // POST: MedicalServices/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                return NotFound();
            }

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Medical service deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private bool ServiceExists(int id)
        {
            return _context.Services.Any(e => e.Id == id);
        }
    }
}
