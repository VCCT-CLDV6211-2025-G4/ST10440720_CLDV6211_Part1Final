using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventEase.Models;
using EventEase.Data;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using Azure.Storage.Blobs.Models;
using System.Linq;

namespace EventEase.Controllers
{
    public class VenuesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<VenuesController> _logger;
        private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB

        public VenuesController(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<VenuesController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Venues.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venues.FirstOrDefaultAsync(m => m.VenueId == id);
            if (venue == null) return NotFound();

            return View(venue);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VenueId,Name,Location,Capacity,Description")] Venue venue, IFormFile imageFile)
        {
            ValidateImageFile(imageFile);

            if (ModelState.IsValid)
            {
                try
                {
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        venue.ImageUrl = await UploadImageToBlobAsync(imageFile);
                    }
                    else
                    {
                        venue.ImageUrl = "/images/venue-placeholder.jpg";
                    }

                    _context.Add(venue);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating venue");
                    ModelState.AddModelError("", $"An error occurred: {GetUserFriendlyErrorMessage(ex)}");
                }
            }

            return View(venue);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venues.FindAsync(id);
            if (venue == null) return NotFound();

            return View(venue);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("VenueId,Name,Location,Capacity,ImageUrl,Description")] Venue venue, IFormFile? imageFile)
        {
            if (id != venue.VenueId) return NotFound();

            ValidateImageFile(imageFile, false);

            if (ModelState.IsValid)
            {
                try
                {
                    string originalImageUrl = venue.ImageUrl;

                    if (imageFile != null && imageFile.Length > 0)
                    {
                        venue.ImageUrl = await UploadImageToBlobAsync(imageFile);

                        // Delete old image after successful upload
                        if (!string.IsNullOrEmpty(originalImageUrl) &&
                            !originalImageUrl.Contains("venue-placeholder.jpg"))
                        {
                            await TryDeleteImageAsync(originalImageUrl);
                        }
                    }

                    _context.Update(venue);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VenueExists(venue.VenueId)) return NotFound();
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error editing venue");
                    ModelState.AddModelError("", $"An error occurred: {GetUserFriendlyErrorMessage(ex)}");
                }
            }

            return View(venue);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venues
                .Include(v => v.Bookings)
                .FirstOrDefaultAsync(m => m.VenueId == id);

            if (venue == null) return NotFound();

            if (venue.Bookings?.Any() == true)
            {
                ViewBag.ErrorMessage = "Cannot delete this venue as it has associated bookings.";
                return View("DeleteError");
            }

            return View(venue);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venue = await _context.Venues
                .Include(v => v.Bookings)
                .FirstOrDefaultAsync(v => v.VenueId == id);

            if (venue == null) return NotFound();

            if (venue.Bookings?.Any() == true)
            {
                ViewBag.ErrorMessage = "Cannot delete this venue as it has associated bookings.";
                return View("DeleteError");
            }

            try
            {
                if (!string.IsNullOrEmpty(venue.ImageUrl) &&
                    !venue.ImageUrl.Contains("venue-placeholder.jpg"))
                {
                    await TryDeleteImageAsync(venue.ImageUrl);
                }

                _context.Venues.Remove(venue);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting venue");
                ViewBag.ErrorMessage = $"An error occurred: {GetUserFriendlyErrorMessage(ex)}";
                return View("DeleteError");
            }
        }

        private bool VenueExists(int id)
        {
            return _context.Venues.Any(e => e.VenueId == id);
        }

        private async Task<string> UploadImageToBlobAsync(IFormFile file)
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("AzureBlobStorage")
                                    ?? _configuration["AzureBlobStorage:ConnectionString"];
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
                _logger.LogError(ex, "Error uploading image to blob storage");
                throw new ApplicationException("Failed to upload image. Please try again with a different file.", ex);
            }
        }

        private async Task TryDeleteImageAsync(string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl)) return;

                var connectionString = _configuration.GetConnectionString("AzureBlobStorage")
                                    ?? _configuration["AzureBlobStorage:ConnectionString"];
                var containerName = _configuration["AzureBlobStorage:ContainerName"];

                if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(containerName))
                    return;

                var blobServiceClient = new BlobServiceClient(connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

                var blobName = new Uri(imageUrl).Segments.Last();
                await containerClient.GetBlobClient(blobName).DeleteIfExistsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image from blob storage");
                // Continue with the operation even if deletion fails
            }
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

        private string GetUserFriendlyErrorMessage(Exception ex)
        {
            return ex switch
            {
                ApplicationException appEx => appEx.Message,
                _ => "An unexpected error occurred. Please try again."
            };
        }
    }
}