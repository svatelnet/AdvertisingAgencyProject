using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AdvertisingAgencyProject.Data;
using AdvertisingAgencyProject.Models;

namespace AdvertisingAgencyProject.Controllers
{
    public class AdOrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdOrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: AdOrders
        public async Task<IActionResult> Index()
        {
            await UpdateExpiredOrdersAsync();
            var applicationDbContext = _context.AdOrders
                .Include(a => a.Client)
                .Include(a => a.AdCategory);

            return View(await applicationDbContext.ToListAsync());
        }

        // GET: AdOrders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var adOrder = await _context.AdOrders
                .Include(a => a.AdCategory)
                .Include(a => a.Client)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (adOrder == null)
            {
                return NotFound();
            }

            return View(adOrder);
        }

        // GET: AdOrders/Create
        public IActionResult Create()
        {
            ViewData["AdCategoryId"] = new SelectList(_context.AdCategories, "Id", "Name");
            ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "FullName");
            return View();
        }

        // POST: AdOrders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ClientId,AdCategoryId,Title,Description,StartDate,EndDate")] AdOrder adOrder)
        {
            if (adOrder.EndDate < adOrder.StartDate)
            {
                ModelState.AddModelError("", "Дата окончания не может быть раньше даты начала.");
            }

            var category = await _context.AdCategories.FindAsync(adOrder.AdCategoryId);
            if (category == null)
            {
                ModelState.AddModelError("", "Категория рекламы не найдена.");
            }

            if (ModelState.IsValid)
            {
                int days = (adOrder.EndDate - adOrder.StartDate).Days + 1;

                adOrder.TotalPrice = category!.BasePrice * days;
                adOrder.Status = OrderStatus.Submitted;
                adOrder.IsActive = false;

                _context.Add(adOrder);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["AdCategoryId"] = new SelectList(_context.AdCategories, "Id", "Name", adOrder.AdCategoryId);
            ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "FullName", adOrder.ClientId);
            return View(adOrder);
        }

        // GET: AdOrders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var adOrder = await _context.AdOrders.FindAsync(id);
            if (adOrder == null)
            {
                return NotFound();
            }
            ViewData["AdCategoryId"] = new SelectList(_context.AdCategories, "Id", "Name", adOrder.AdCategoryId);
            ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "FullName", adOrder.ClientId);
            return View(adOrder);
        }

        // POST: AdOrders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ClientId,AdCategoryId,Title,Description,StartDate,EndDate,TotalPrice,Status,IsActive")] AdOrder adOrder)
        {
            if (id != adOrder.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(adOrder);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AdOrderExists(adOrder.Id))
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
            ViewData["AdCategoryId"] = new SelectList(_context.AdCategories, "Id", "Name", adOrder.AdCategoryId);
            ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "FullName", adOrder.ClientId);
            return View(adOrder);
        }

        // GET: AdOrders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var adOrder = await _context.AdOrders
                .Include(a => a.AdCategory)
                .Include(a => a.Client)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (adOrder == null)
            {
                return NotFound();
            }

            return View(adOrder);
        }

        // POST: AdOrders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var adOrder = await _context.AdOrders.FindAsync(id);
            if (adOrder != null)
            {
                _context.AdOrders.Remove(adOrder);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Complete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var adOrder = await _context.AdOrders.FindAsync(id);
            if (adOrder == null)
            {
                return NotFound();
            }

            adOrder.Status = OrderStatus.Completed;
            adOrder.IsActive = false;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task UpdateExpiredOrdersAsync()
        {
            var expiredOrders = await _context.AdOrders
                .Where(o => o.IsActive && o.EndDate < DateTime.Today)
                .ToListAsync();

            foreach (var order in expiredOrders)
            {
                order.IsActive = false;
                order.Status = OrderStatus.Expired;
            }

            await _context.SaveChangesAsync();
        }

        private bool AdOrderExists(int id)
        {
            return _context.AdOrders.Any(e => e.Id == id);
        }
    }
}
