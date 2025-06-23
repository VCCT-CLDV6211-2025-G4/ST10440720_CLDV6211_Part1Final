using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;

namespace EventBookingSystem.Controllers
{
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Bookings (with Advanced Filtering)
        public async Task<IActionResult> Index(string searchString, int? eventTypesId, DateTime? startDate, DateTime? endDate, bool? availableOnly = false)
        {
            var bookings = _context.Bookings
                .Include(b => b.Event)
                    .ThenInclude(e => e.EventTypes)
                .Include(b => b.Venue)
                .AsQueryable();

            // Filter by search string (CustomerName or Venue Name)
            if (!string.IsNullOrEmpty(searchString))
            {
                bookings = bookings.Where(b =>
                    b.CustomerName.Contains(searchString) ||
                    b.Venue.Name.Contains(searchString));
            }

            // Filter by EventTypesId
            if (eventTypesId.HasValue)
            {
                bookings = bookings.Where(b => b.Event.EventTypesId == eventTypesId);
            }

            // Filter by Date Range
            if (startDate.HasValue && endDate.HasValue)
            {
                bookings = bookings.Where(b => b.BookingDate >= startDate && b.BookingDate <= endDate);
            }

            // Filter by Venue Availability (using IsAvailable field)
            if (availableOnly == true)
            {
                bookings = bookings.Where(b => b.Venue.IsAvailable);
            }

            // Provide data to View for filters dropdown
            ViewBag.EventTypes = new SelectList(await _context.EventTypes.ToListAsync(), "EventTypesId", "Name", eventTypesId);
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.AvailableOnly = availableOnly;

            return View(await bookings.ToListAsync());
        }

        // GET: Bookings/Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);

            if (booking == null)
                return NotFound();

            return View(booking);
        }

        // GET: Bookings/Create
        public IActionResult Create()
        {
            ViewBag.Venues = _context.Venues.ToList();
            ViewBag.Events = _context.Events.Include(e => e.EventTypes).ToList();
            return View();
        }

        // POST: Bookings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookingId,CustomerName,EventId,VenueId,BookingDate,StartTime,EndTime")] Booking booking)
        {
            if (ModelState.IsValid)
            {
                _context.Add(booking);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Booking successfully created.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Venues = _context.Venues.ToList();
            ViewBag.Events = _context.Events.Include(e => e.EventTypes).ToList();
            return View(booking);
        }

        // GET: Bookings/Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
                return NotFound();

            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "Name", booking.EventId);
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "Name", booking.VenueId);
            return View(booking);
        }

        // POST: Bookings/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookingId,CustomerName,EventId,VenueId,BookingDate,StartTime,EndTime")] Booking booking)
        {
            if (id != booking.BookingId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.BookingId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "Name", booking.EventId);
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "Name", booking.VenueId);
            return View(booking);
        }

        // GET: Bookings/Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);

            if (booking == null)
                return NotFound();

            return View(booking);
        }

        // POST: Bookings/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);

            if (booking != null)
            {
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.BookingId == id);
        }
    }
}
