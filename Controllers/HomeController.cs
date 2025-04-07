using EventEase.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace EventEase.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var model = new HomeViewModel
            {
                // Basic counts
                VenueCount = await _context.Venues.CountAsync(),
                EventCount = await _context.Events.CountAsync(),
                BookingCount = await _context.Bookings.CountAsync(),
                UpcomingBookingCount = await _context.Bookings
                    .CountAsync(b => b.StartTime > DateTime.Now),
                PastBookingCount = await _context.Bookings
                    .CountAsync(b => b.StartTime <= DateTime.Now),

                // Recent activity
                RecentBookings = await _context.Bookings
                    .Include(b => b.Event)
                    .Include(b => b.Venue)
                    .OrderByDescending(b => b.StartTime)
                    .Take(5)
                    .ToListAsync(),

                UpcomingEvents = await _context.Events
                    .OrderBy(e => e.Name)
                    .Take(3)
                    .ToListAsync(),

                PopularVenues = await _context.Venues
                    .OrderByDescending(v => v.Bookings.Count)
                    .Take(3)
                    .ToListAsync(),

                // System status
                DatabaseConnected = await _context.Database.CanConnectAsync(),
                LastUpdate = DateTime.Now.ToString("MMMM dd, yyyy")
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }

    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}