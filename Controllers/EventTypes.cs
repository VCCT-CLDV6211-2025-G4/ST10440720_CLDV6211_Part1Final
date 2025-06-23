using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;

namespace EventEase.Controllers
{
    public class EventTypesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventTypesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: EventTypes
        public async Task<IActionResult> Index()
        {
            return View(await _context.EventTypes.ToListAsync());
        }

        // GET: EventTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var eventType = await _context.EventTypes.FirstOrDefaultAsync(m => m.EventTypesId == id);  // ✅ FIXED HERE
            if (eventType == null) return NotFound();

            return View(eventType);
        }

        // GET: EventTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: EventTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EventTypesId,Name")] EventTypes eventType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(eventType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(eventType);
        }

        // GET: EventTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var eventType = await _context.EventTypes.FindAsync(id);
            if (eventType == null) return NotFound();

            return View(eventType);
        }

        // POST: EventTypes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EventTypesId,Name")] EventTypes eventType)
        {
            if (id != eventType.EventTypesId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(eventType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventTypesExists(eventType.EventTypesId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(eventType);
        }

        // GET: EventTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var eventType = await _context.EventTypes.FirstOrDefaultAsync(m => m.EventTypesId == id);  // ✅ FIXED HERE
            if (eventType == null) return NotFound();

            return View(eventType);
        }

        // POST: EventTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var eventType = await _context.EventTypes.FindAsync(id);
            _context.EventTypes.Remove(eventType);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EventTypesExists(int id)
        {
            return _context.EventTypes.Any(e => e.EventTypesId == id);
        }
    }
}
