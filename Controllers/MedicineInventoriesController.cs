using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Net;
using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DCAS.Data;
using DCAS.Models;

namespace DCAS.Controllers
{
    public class MedicineInventoriesController : Controller
    {
        private readonly DCASContext _context;

        public MedicineInventoriesController(DCASContext context)
        {
            _context = context;
        }

        // GET: MedicineInventories
        public async Task<IActionResult> Index()
        {
            return View(await _context.MedicineInventory.ToListAsync());
        }

        public async Task<IActionResult> GetImage(int id)
        {
            var medicineInventory = await _context.MedicineInventory
                .FirstOrDefaultAsync(m => m.Id == id);

            if (medicineInventory == null || medicineInventory.Image == null || medicineInventory.Image.Length == 0)
            {
                // Return a default "no image" or 404
                return NotFound();
            }

            // Detect content type from the image bytes
            string contentType = GetImageContentType(medicineInventory.Image);

            return File(medicineInventory.Image, contentType);
        }

        // Helper method to detect image content type
        private string GetImageContentType(byte[] imageBytes)
        {
            if (imageBytes == null || imageBytes.Length < 4)
                return "image/jpeg"; // default

            // Check the first few bytes to determine image type
            // JPEG
            if (imageBytes.Length >= 2 && imageBytes[0] == 0xFF && imageBytes[1] == 0xD8)
                return "image/jpeg";

            // PNG
            if (imageBytes.Length >= 8 &&
                imageBytes[0] == 0x89 && imageBytes[1] == 0x50 &&
                imageBytes[2] == 0x4E && imageBytes[3] == 0x47)
                return "image/png";

            // GIF
            if (imageBytes.Length >= 6 &&
                imageBytes[0] == 0x47 && imageBytes[1] == 0x49 && imageBytes[2] == 0x46)
                return "image/gif";

            // WebP
            if (imageBytes.Length >= 12 &&
                imageBytes[8] == 0x57 && imageBytes[9] == 0x45 &&
                imageBytes[10] == 0x42 && imageBytes[11] == 0x50)
                return "image/webp";

            // Default to JPEG
            return "image/jpeg";
        }

        // GET: MedicineInventories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: MedicineInventories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,MedicineName,Price,Quantity,Miligram,Description,ExpiryDate")] MedicineInventory medicineInventory, IFormFile Image)
        {
            // Existing create logic preserved
            if (ModelState.IsValid)
            {
                try
                {
                    if (Image != null && Image.Length > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await Image.CopyToAsync(memoryStream);
                            medicineInventory.Image = memoryStream.ToArray();
                        }
                    }

                    _context.Add(medicineInventory);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Medicine inventory created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"An unexpected error occurred while saving the data: {ex.Message}");
                }
            }

            return View(medicineInventory);
        }

        // Details for modal
        public async Task<IActionResult> GetDetails(int id)
        {
            var medicine = await _context.MedicineInventory.FindAsync(id);
            if (medicine == null) return NotFound();

            // build safe pieces first
            var imageUrl = (medicine.Image != null && medicine.Image.Length > 0)
                ? Url.Action("GetImage", "MedicineInventories", new { id = medicine.Id })
                : null;

            var sb = new StringBuilder();
            sb.AppendLine("<div class='row'>");
            sb.AppendLine("  <div class='col-md-6 text-center'>");
            if (!string.IsNullOrEmpty(imageUrl))
            {
                sb.AppendFormat("    <img src='{0}' class='img-fluid rounded shadow-sm mb-3' />", imageUrl).AppendLine();
            }
            else
            {
                sb.AppendLine("    <i class='fas fa-image fa-5x text-muted'></i><p class='text-muted'>No image available</p>");
            }
            sb.AppendLine("  </div>");
            sb.AppendLine("  <div class='col-md-6'>");
            sb.AppendFormat("    <p><strong>💊 Name:</strong> {0}</p>", WebUtility.HtmlEncode(medicine.MedicineName)).AppendLine();
            sb.AppendFormat("    <p><strong>💰 Price:</strong> ₱{0:N2}</p>", medicine.Price).AppendLine();
            sb.AppendFormat("    <p><strong>📦 Quantity:</strong> {0}</p>", medicine.Quantity).AppendLine();
            sb.AppendFormat("    <p><strong>⚖️ Strength:</strong> {0}</p>", WebUtility.HtmlEncode(medicine.Miligram)).AppendLine();
            sb.AppendFormat("    <p><strong>📝 Description:</strong> {0}</p>", WebUtility.HtmlEncode(medicine.Description)).AppendLine();
            sb.AppendFormat("    <p><strong>📅 Expiry:</strong> {0:MMM dd, yyyy}</p>", medicine.ExpiryDate).AppendLine();
            sb.AppendLine("  </div>");
            sb.AppendLine("</div>");

            return Content(sb.ToString(), "text/html");
        }

        // Edit form for modal
        public async Task<IActionResult> GetEditForm(int id)
        {
            var medicine = await _context.MedicineInventory.FindAsync(id);
            if (medicine == null) return NotFound();

            var encName = WebUtility.HtmlEncode(medicine.MedicineName ?? string.Empty);
            var encPrice = medicine.Price.ToString(CultureInfo.InvariantCulture);
            var encQuantity = medicine.Quantity.ToString(CultureInfo.InvariantCulture);
            var encMiligram = WebUtility.HtmlEncode(medicine.Miligram ?? string.Empty);
            var encDescription = WebUtility.HtmlEncode(medicine.Description ?? string.Empty);
            var encExpiry = medicine.ExpiryDate.ToString("yyyy-MM-dd");

            var postAction = Url.Action("EditModal", "MedicineInventories");
            var imageUrl = (medicine.Image != null && medicine.Image.Length > 0)
                ? Url.Action("GetImage", "MedicineInventories", new { id = medicine.Id })
                : null;

            var sb = new StringBuilder();
            sb.AppendFormat("<form id='editForm' method='post' action='{0}' enctype='multipart/form-data'>", postAction).AppendLine();
            sb.AppendFormat("  <input type='hidden' name='Id' value='{0}' />", medicine.Id).AppendLine();
            sb.AppendLine("  <div class='mb-3 text-center'>");
            if (!string.IsNullOrEmpty(imageUrl))
            {
                sb.AppendFormat("    <img src='{0}' class='img-fluid rounded mb-2' style='max-height:200px;' />", imageUrl).AppendLine();
            }
            else
            {
                sb.AppendLine("    <div class='text-muted mb-2'><i class='fas fa-image fa-3x'></i><p>No image</p></div>");
            }
            sb.AppendLine("    <input type='file' class='form-control' name='Image' accept='image/*' />");
            sb.AppendLine("  </div>");

            sb.AppendLine("  <div class='mb-3'>");
            sb.AppendLine("    <label class='form-label'>Medicine Name</label>");
            sb.AppendFormat("    <input type='text' class='form-control' name='MedicineName' value='{0}' />", encName).AppendLine();
            sb.AppendLine("  </div>");

            sb.AppendLine("  <div class='mb-3'>");
            sb.AppendLine("    <label class='form-label'>Price</label>");
            sb.AppendFormat("    <input type='number' step='0.01' class='form-control' name='Price' value='{0}' />", encPrice).AppendLine();
            sb.AppendLine("  </div>");

            sb.AppendLine("  <div class='mb-3'>");
            sb.AppendLine("    <label class='form-label'>Quantity</label>");
            sb.AppendFormat("    <input type='number' class='form-control' name='Quantity' value='{0}' />", encQuantity).AppendLine();
            sb.AppendLine("  </div>");

            sb.AppendLine("  <div class='mb-3'>");
            sb.AppendLine("    <label class='form-label'>Strength</label>");
            sb.AppendFormat("    <input type='text' class='form-control' name='Miligram' value='{0}' />", encMiligram).AppendLine();
            sb.AppendLine("  </div>");

            sb.AppendLine("  <div class='mb-3'>");
            sb.AppendLine("    <label class='form-label'>Description</label>");
            sb.AppendFormat("    <textarea class='form-control' name='Description'>{0}</textarea>", encDescription).AppendLine();
            sb.AppendLine("  </div>");

            sb.AppendLine("  <div class='mb-3'>");
            sb.AppendLine("    <label class='form-label'>Expiry Date</label>");
            sb.AppendFormat("    <input type='date' class='form-control' name='ExpiryDate' value='{0}' />", encExpiry).AppendLine();
            sb.AppendLine("  </div>");

            sb.AppendLine("  <button type='submit' class='btn btn-primary'>");
            sb.AppendLine("    <i class='fas fa-save'></i> Save Changes");
            sb.AppendLine("  </button>");
            sb.AppendLine("</form>");

            return Content(sb.ToString(), "text/html");
        }

        // Save edits from modal
        [HttpPost]
        public async Task<IActionResult> EditModal(MedicineInventory medicineInventory, IFormFile Image)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"EditModal called. Incoming Id={medicineInventory?.Id}, Name='{medicineInventory?.MedicineName}', Quantity={medicineInventory?.Quantity}");

                if (medicineInventory == null || medicineInventory.Id == 0)
                {
                    if (Request?.Form != null && Request.Form.ContainsKey("Id") && int.TryParse(Request.Form["Id"], out var fallbackId))
                    {
                        medicineInventory = medicineInventory ?? new MedicineInventory();
                        medicineInventory.Id = fallbackId;
                        System.Diagnostics.Debug.WriteLine($"Recovered Id from form: {fallbackId}");
                    }
                    else
                    {
                        var errObj = new { success = false, error = "Invalid data posted." };
                        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                            return Json(errObj);
                        TempData["ErrorMessage"] = "Invalid data posted.";
                        return RedirectToAction(nameof(Index));
                    }
                }

                var existing = await _context.MedicineInventory.FindAsync(medicineInventory.Id);
                if (existing == null)
                {
                    var notFoundObj = new { success = false, error = "Item not found." };
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        return Json(notFoundObj);
                    TempData["ErrorMessage"] = "Item not found.";
                    return RedirectToAction(nameof(Index));
                }

                existing.MedicineName = medicineInventory.MedicineName;
                existing.Price = medicineInventory.Price;
                existing.Quantity = medicineInventory.Quantity;
                existing.Miligram = medicineInventory.Miligram;
                existing.Description = medicineInventory.Description;
                existing.ExpiryDate = medicineInventory.ExpiryDate;

                if (Image != null && Image.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await Image.CopyToAsync(memoryStream);
                        existing.Image = memoryStream.ToArray();
                    }
                }

                var saveResult = await _context.SaveChangesAsync();
                System.Diagnostics.Debug.WriteLine($"EditModal: SaveChangesAsync affected {saveResult} rows for Id={existing.Id}");

                // If request is AJAX, return JSON (existing frontend expects JSON).
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = true });

                // Non-AJAX fallback: redirect to Index so the user sees the updated list instead of raw JSON.
                TempData["SuccessMessage"] = "Medicine inventory updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EditModal Exception: {ex}");
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, error = ex.Message });

                TempData["ErrorMessage"] = "Error saving changes. " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // Delete with confirm
        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var medicineInventory = await _context.MedicineInventory.FindAsync(id);
            if (medicineInventory != null)
            {
                _context.MedicineInventory.Remove(medicineInventory);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
    }
}
