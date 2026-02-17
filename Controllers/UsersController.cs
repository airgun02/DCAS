using BCrypt.Net;
using DCAS.Data;
using DCAS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace DCAS.Controllers
{
    public class UsersController : Controller
    {
        private readonly DCASContext _context;

        public UsersController(DCASContext context)
        {
            _context = context;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                                        .OrderByDescending(u => u.StartDate)
                                        .ToListAsync();
            return View(users);
        }

        public async Task<IActionResult> GetImage(int id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.UsersId == id);

            if (user == null || user.Profile == null || user.Profile.Length == 0)
            {
                // Redirect to static placeholder in wwwroot/images/
                return Redirect(Url.Content("~/images/avatar-placeholder.png"));
            }

            string contentType = GetImageContentType(user.Profile);
            return File(user.Profile, contentType);
        }

        // Helper method to detect image content type
        private string GetImageContentType(byte[] imageBytes)
        {
            if (imageBytes == null || imageBytes.Length < 4)
                return "image/jpeg";

            if (imageBytes.Length >= 2 && imageBytes[0] == 0xFF && imageBytes[1] == 0xD8)
                return "image/jpeg";

            if (imageBytes.Length >= 8 &&
                imageBytes[0] == 0x89 && imageBytes[1] == 0x50 &&
                imageBytes[2] == 0x4E && imageBytes[3] == 0x47)
                return "image/png";

            if (imageBytes.Length >= 6 &&
                imageBytes[0] == 0x47 && imageBytes[1] == 0x49 && imageBytes[2] == 0x46)
                return "image/gif";

            if (imageBytes.Length >= 12 &&
                imageBytes[8] == 0x57 && imageBytes[9] == 0x45 &&
                imageBytes[10] == 0x42 && imageBytes[11] == 0x50)
                return "image/webp";

            return "image/jpeg";
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            return View(new Users());
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("UsersId,Name,Address,Email,PhoneNumber,Role,JobTitle,Specialization,Gender,Nationality,Position,WorkStatus,Age,BirthDate,StartDate,Profile,Username,Password")] Users users,
            IFormFile? Image)
        {
            if (!ModelState.IsValid)
            {
                return View(users);
            }

            try
            {
                // Hash password
                users.Password = BCrypt.Net.BCrypt.HashPassword(users.Password, workFactor: 17);

                // Handle image if provided
                if (Image != null && Image.Length > 0)
                {
                    using var memoryStream = new MemoryStream();
                    await Image.CopyToAsync(memoryStream);
                    users.Profile = memoryStream.ToArray();
                }

                _context.Add(users);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "User created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred while saving the data: {ex.Message}");
                return View(users);
            }
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var users = await _context.Users.FindAsync(id);
            if (users == null) return NotFound();

            return View(users);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, IFormFile? Image, [Bind("UsersId,Name,Address,Email,PhoneNumber,Role,JobTitle,Specialization,Gender,Nationality,Position,WorkStatus,Age,BirthDate,StartDate,Username,Password")] Users userUpdateData)
        {
            if (id != userUpdateData.UsersId) return NotFound();
            if (!ModelState.IsValid) return View(userUpdateData);

            try
            {
                var userToUpdate = await _context.Users.FindAsync(id);
                if (userToUpdate == null) return NotFound();

                userToUpdate.Name = userUpdateData.Name;
                userToUpdate.Address = userUpdateData.Address;
                userToUpdate.Email = userUpdateData.Email;
                userToUpdate.PhoneNumber = userUpdateData.PhoneNumber;
                userToUpdate.Role = userUpdateData.Role;
                userToUpdate.JobTitle = userUpdateData.JobTitle;
                userToUpdate.Specialization = userUpdateData.Specialization;
                userToUpdate.Gender = userUpdateData.Gender;
                userToUpdate.Nationality = userUpdateData.Nationality;
                userToUpdate.Position = userUpdateData.Position;
                userToUpdate.WorkStatus = userUpdateData.WorkStatus;
                userToUpdate.Age = userUpdateData.Age;
                userToUpdate.BirthDate = userUpdateData.BirthDate;
                userToUpdate.StartDate = userUpdateData.StartDate;
                userToUpdate.Username = userUpdateData.Username;

                if (!string.IsNullOrEmpty(userUpdateData.Password))
                {
                    userToUpdate.Password = BCrypt.Net.BCrypt.HashPassword(userUpdateData.Password, workFactor: 17);
                }

                // Use the incoming 'Image' (matches your form input name)
                if (Image != null && Image.Length > 0)
                {
                    using var memoryStream = new MemoryStream();
                    await Image.CopyToAsync(memoryStream);
                    userToUpdate.Profile = memoryStream.ToArray();
                }

                _context.Update(userToUpdate);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "User updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsersExists(userUpdateData.UsersId)) return NotFound();
                throw;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred while saving the data: {ex.Message}");
                return View(userUpdateData);
            }
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var users = await _context.Users.FirstOrDefaultAsync(m => m.UsersId == id);
            if (users == null) return NotFound();

            return View(users);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var users = await _context.Users.FindAsync(id);
            if (users != null) _context.Users.Remove(users);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UsersExists(int id)
        {
            return _context.Users.Any(e => e.UsersId == id);
        }
    }
}
