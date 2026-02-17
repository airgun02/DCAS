using DCAS.Data;
using DCAS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DCAS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class LatestUpdatesController : Controller
    {
        private readonly DCASContext _context;
        public LatestUpdatesController(DCASContext context) => _context = context;

        // GET: /LatestUpdates/Create
        public IActionResult Create()
        {
            return View(new LatestUpdate { CreatedAt = DateTime.UtcNow });
        }

        // POST: /LatestUpdates/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LatestUpdate model, IFormFile? Image)
        {
            if (!ModelState.IsValid) return View(model);

            // Optional image validation
            if (Image != null && Image.Length > 0)
            {
                var allowed = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
                const long maxBytes = 5 * 1024 * 1024; // 5 MB
                if (!allowed.Contains(Image.ContentType))
                {
                    ModelState.AddModelError("Image", "Only JPG/PNG/GIF/WebP images are allowed.");
                    return View(model);
                }
                if (Image.Length > maxBytes)
                {
                    ModelState.AddModelError("Image", "Image is too large. Max 5 MB.");
                    return View(model);
                }

                using var ms = new MemoryStream();
                await Image.CopyToAsync(ms);
                model.Image = ms.ToArray();
                model.ImageContentType = Image.ContentType;
            }

            model.CreatedAt = DateTime.UtcNow;
            _context.Add(model);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Update posted.";
            return RedirectToAction("Index", "Home");
        }

        // Public: serve image bytes for a given update id
        [AllowAnonymous]
        public async Task<IActionResult> GetImage(int id)
        {
            var item = await _context.LatestUpdates.FindAsync(id);
            if (item == null || item.Image == null) return NotFound();
            var contentType = string.IsNullOrEmpty(item.ImageContentType) ? "image/jpeg" : item.ImageContentType;
            return File(item.Image, contentType);
        }
    }
}