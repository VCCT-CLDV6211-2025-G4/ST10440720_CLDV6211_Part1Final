using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEase.Models;
using EventEase.Data;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using System;
using Azure.Storage.Blobs.Models;

namespace EventEase.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB

        public EventsController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            var events = await _context.Events.Include(e => e.Venue).ToListAsync();
            return View(events);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var @event = await _context.Events
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(m => m.EventId == id);

            if (@event == null)
                return NotFound();

            return View(@event);
        }

        public IActionResult Create()
        {
            PopulateVenuesDropDown();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EventId,Name,Title,Date,VenueId,Description")] Event @event, IFormFile imageFile)
        {
            ValidateImageFile(imageFile);

            if (ModelState.IsValid)
            {
                try
                {
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        @event.ImageUrl = await UploadImageToBlobAsync(imageFile);
                    }

                    _context.Add(@event);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                }
            }

            PopulateVenuesDropDown(@event.VenueId);
            return View(@event);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
                return NotFound();

            PopulateVenuesDropDown(@event.VenueId);
            return View(@event);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EventId,Name,Title,Date,VenueId,Description,ImageUrl")] Event @event, IFormFile? imageFile)
        {
            if (id != @event.EventId)
                return NotFound();

            ValidateImageFile(imageFile, false);

            if (ModelState.IsValid)
            {
                try
                {
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        @event.ImageUrl = await UploadImageToBlobAsync(imageFile);
                    }

                    _context.Update(@event);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(@event.EventId))
                        return NotFound();
                    throw;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                }
            }

            PopulateVenuesDropDown(@event.VenueId);
            return View(@event);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var @event = await _context.Events
                .Include(e => e.Venue)
                .Include(e => e.Bookings)
                .FirstOrDefaultAsync(m => m.EventId == id);

            if (@event == null)
                return NotFound();

            ViewBag.HasBookings = @event.Bookings?.Any() ?? false;
            ViewBag.BookingCount = @event.Bookings?.Count ?? 0;

            return View(@event);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @event = await _context.Events
                .Include(e => e.Bookings)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (@event == null)
                return NotFound();

            try
            {
                // Delete associated bookings first if they exist
                if (@event.Bookings?.Any() == true)
                {
                    _context.Bookings.RemoveRange(@event.Bookings);
                }

                _context.Events.Remove(@event);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");

                // Re-populate view data if deletion fails
                ViewBag.HasBookings = @event.Bookings?.Any() ?? false;
                ViewBag.BookingCount = @event.Bookings?.Count ?? 0;

                return View("Delete", @event);
            }
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.EventId == id);
        }

        private void PopulateVenuesDropDown(object? selectedVenueId = null)
        {
            ViewBag.VenueId = new SelectList(_context.Venues.OrderBy(v => v.Name), "VenueId", "Name", selectedVenueId);
        }

        private void ValidateImageFile(IFormFile file, bool isRequired = false)
        {
            if (file == null || file.Length == 0)
            {
                if (isRequired)
                {
                    ModelState.AddModelError("imageFile", "Please upload an image file.");
                }
                return;
            }

            if (!file.ContentType.StartsWith("image/"))
            {
                ModelState.AddModelError("imageFile", "Only image files are allowed.");
            }

            if (file.Length > _maxFileSize)
            {
                ModelState.AddModelError("imageFile", $"File size must be less than {_maxFileSize / (1024 * 1024)}MB.");
            }
        }

        private async Task<string> UploadImageToBlobAsync(IFormFile file)
        {
            try
            {
                var connectionString = _configuration["AzureBlobStorage:ConnectionString"];
                var containerName = _configuration["AzureBlobStorage:ContainerName"];

                if (string.IsNullOrEmpty(connectionString))
                    throw new ApplicationException("Azure Storage connection is not configured");

                if (string.IsNullOrEmpty(containerName))
                    throw new ApplicationException("Azure Storage container name is not configured");

                var blobServiceClient = new BlobServiceClient(connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

                await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var blobClient = containerClient.GetBlobClient(fileName);

                using (var stream = file.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream, new BlobHttpHeaders
                    {
                        ContentType = file.ContentType
                    }, conditions: null);
                }

                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed to upload image. Please try again with a different file.", ex);
            }
        }
    }
}