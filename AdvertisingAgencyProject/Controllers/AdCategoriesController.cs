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
    public class AdCategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdCategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: AdCategories
        public async Task<IActionResult> Index()
        {
            return View(await _context.AdCategories.ToListAsync());
        }

        // GET: AdCategories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var adCategory = await _context.AdCategories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (adCategory == null)
            {
                return NotFound();
            }

            return View(adCategory);
        }

        // GET: AdCategories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AdCategories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,BasePrice")] AdCategory adCategory)
        {
            if (ModelState.IsValid)
            {
                _context.Add(adCategory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(adCategory);
        }

        // GET: AdCategories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var adCategory = await _context.AdCategories.FindAsync(id);
            if (adCategory == null)
            {
                return NotFound();
            }
            return View(adCategory);
        }

        // POST: AdCategories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,BasePrice")] AdCategory adCategory)
        {
            if (id != adCategory.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(adCategory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AdCategoryExists(adCategory.Id))
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
            return View(adCategory);
        }

        // GET: AdCategories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var adCategory = await _context.AdCategories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (adCategory == null)
            {
                return NotFound();
            }

            return View(adCategory);
        }

        // POST: AdCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var adCategory = await _context.AdCategories.FindAsync(id);
            if (adCategory != null)
            {
                _context.AdCategories.Remove(adCategory);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AdCategoryExists(int id)
        {
            return _context.AdCategories.Any(e => e.Id == id);
        }
    }
}
