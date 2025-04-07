using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventEase.Models;
using EventEase.Data;

namespace EventEase.Controllers
{
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Bookings
        public async Task<IActionResult> Index()
        {
            var bookings = await _context.Bookings
                .Include(b => b.Venue)
                .Include(b => b.Event)
                .ToListAsync();
            return View(bookings);
        }

        // GET: Bookings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.Venue)
                .Include(b => b.Event)
                .FirstOrDefaultAsync(m => m.BookingId == id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // GET: Bookings/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Venues = await _context.Venues.ToListAsync();
            ViewBag.Events = await _context.Events.ToListAsync();
            return View();
        }

        // POST: Bookings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookingId,StartTime,EndTime,VenueId,EventId")] Booking booking)
        {
            if (ModelState.IsValid)
            {
                // Check for booking conflicts
                bool isConflict = await _context.Bookings
                    .AnyAsync(b => b.VenueId == booking.VenueId &&
                                 ((booking.StartTime >= b.StartTime && booking.StartTime < b.EndTime) ||
                                  (booking.EndTime > b.StartTime && booking.EndTime <= b.EndTime) ||
                                  (booking.StartTime <= b.StartTime && booking.EndTime >= b.EndTime)));

                if (isConflict)
                {
                    ModelState.AddModelError(string.Empty, "The selected time slot conflicts with an existing booking for this venue.");
                    ViewBag.Venues = await _context.Venues.ToListAsync();
                    ViewBag.Events = await _context.Events.ToListAsync();
                    return View(booking);
                }

                // Generate booking reference
                booking.BookingReference = Guid.NewGuid().ToString();

                _context.Add(booking);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Venues = await _context.Venues.ToListAsync();
            ViewBag.Events = await _context.Events.ToListAsync();
            return View(booking);
        }

        // GET: Bookings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            ViewBag.Venues = await _context.Venues.ToListAsync();
            ViewBag.Events = await _context.Events.ToListAsync();
            return View(booking);
        }

        // POST: Bookings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookingId,StartTime,EndTime,VenueId,EventId,BookingReference")] Booking booking)
        {
            if (id != booking.BookingId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Check for booking conflicts (excluding current booking)
                    bool isConflict = await _context.Bookings
                        .AnyAsync(b => b.BookingId != booking.BookingId &&
                                     b.VenueId == booking.VenueId &&
                                     ((booking.StartTime >= b.StartTime && booking.StartTime < b.EndTime) ||
                                      (booking.EndTime > b.StartTime && booking.EndTime <= b.EndTime) ||
                                      (booking.StartTime <= b.StartTime && booking.EndTime >= b.EndTime)));

                    if (isConflict)
                    {
                        ModelState.AddModelError(string.Empty, "The selected time slot conflicts with an existing booking for this venue.");
                        ViewBag.Venues = await _context.Venues.ToListAsync();
                        ViewBag.Events = await _context.Events.ToListAsync();
                        return View(booking);
                    }

                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.BookingId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Venues = await _context.Venues.ToListAsync();
            ViewBag.Events = await _context.Events.ToListAsync();
            return View(booking);
        }

        // GET: Bookings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.Venue)
                .Include(b => b.Event)
                .FirstOrDefaultAsync(m => m.BookingId == id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // POST: Bookings/Delete/5
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