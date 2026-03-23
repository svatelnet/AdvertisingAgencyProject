using AdvertisingAgencyProject.Data;
using AdvertisingAgencyProject.Models;
using AdvertisingAgencyProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AdvertisingAgencyProject.Controllers
{
    [Authorize(Roles = "Manager,Admin")]
    public class AdOrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdOrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            await UpdateOrderStatusesAsync();

            var orders = await _context.AdOrders
                .Include(o => o.Client)
                .Include(o => o.AdCategory)
                .OrderByDescending(o => o.StartDate)
                .ToListAsync();

            return View(orders);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var adOrder = await _context.AdOrders
                .Include(o => o.Client)
                .Include(o => o.AdCategory)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (adOrder == null)
            {
                return NotFound();
            }

            return View(adOrder);
        }

        public async Task<IActionResult> Create()
        {
            await LoadSelectListsAsync();

            return View(new AdOrder
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ClientId,AdCategoryId,Title,Description,StartDate,EndDate")] AdOrder adOrder)
        {
            var category = await _context.AdCategories.FindAsync(adOrder.AdCategoryId);
            var clientExists = await _context.Clients.AnyAsync(c => c.Id == adOrder.ClientId);

            ValidateOrder(adOrder, category, clientExists);

            if (!ModelState.IsValid)
            {
                await LoadSelectListsAsync(adOrder);
                return View(adOrder);
            }

            AdOrderWorkflowService.ApplyManagerChanges(adOrder, category!.BasePrice);

            _context.AdOrders.Add(adOrder);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

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

            await LoadSelectListsAsync(adOrder);
            return View(adOrder);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ClientId,AdCategoryId,Title,Description,StartDate,EndDate")] AdOrder formModel)
        {
            if (id != formModel.Id)
            {
                return NotFound();
            }

            var adOrder = await _context.AdOrders.FindAsync(id);
            if (adOrder == null)
            {
                return NotFound();
            }

            var category = await _context.AdCategories.FindAsync(formModel.AdCategoryId);
            var clientExists = await _context.Clients.AnyAsync(c => c.Id == formModel.ClientId);

            ValidateOrder(formModel, category, clientExists);

            if (!ModelState.IsValid)
            {
                await LoadSelectListsAsync(formModel);
                return View(formModel);
            }

            var keepSubmitted = adOrder.Status == OrderStatus.Submitted;

            adOrder.ClientId = formModel.ClientId;
            adOrder.AdCategoryId = formModel.AdCategoryId;
            adOrder.Title = formModel.Title;
            adOrder.Description = formModel.Description;
            adOrder.StartDate = formModel.StartDate;
            adOrder.EndDate = formModel.EndDate;

            if (keepSubmitted)
            {
                AdOrderWorkflowService.ApplyClientRequest(adOrder, category!.BasePrice);
            }
            else
            {
                AdOrderWorkflowService.ApplyManagerChanges(adOrder, category!.BasePrice);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var adOrder = await _context.AdOrders
                .Include(o => o.Client)
                .Include(o => o.AdCategory)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (adOrder == null)
            {
                return NotFound();
            }

            return View(adOrder);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var adOrder = await _context.AdOrders.FindAsync(id);
            if (adOrder != null)
            {
                _context.AdOrders.Remove(adOrder);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var adOrder = await _context.AdOrders
                .Include(o => o.AdCategory)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (adOrder == null)
            {
                return NotFound();
            }

            if (adOrder.AdCategory == null)
            {
                return BadRequest("Для заказа не найдена категория рекламы.");
            }

            adOrder.Status = OrderStatus.Approved;
            AdOrderWorkflowService.ApplyManagerChanges(adOrder, adOrder.AdCategory.BasePrice);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
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

        private void ValidateOrder(AdOrder adOrder, AdCategory? category, bool clientExists)
        {
            if (!clientExists)
            {
                ModelState.AddModelError(nameof(adOrder.ClientId), "Клиент не найден.");
            }

            if (category == null)
            {
                ModelState.AddModelError(nameof(adOrder.AdCategoryId), "Категория рекламы не найдена.");
            }

            if (adOrder.EndDate.Date < adOrder.StartDate.Date)
            {
                ModelState.AddModelError(nameof(adOrder.EndDate), "Дата окончания не может быть раньше даты начала.");
            }
        }

        private async Task LoadSelectListsAsync(AdOrder? adOrder = null)
        {
            ViewData["ClientId"] = new SelectList(
                await _context.Clients.OrderBy(c => c.FullName).ToListAsync(),
                "Id",
                "FullName",
                adOrder?.ClientId);

            ViewData["AdCategoryId"] = new SelectList(
                await _context.AdCategories.OrderBy(c => c.Name).ToListAsync(),
                "Id",
                "Name",
                adOrder?.AdCategoryId);
        }

        private async Task UpdateOrderStatusesAsync()
        {
            var orders = await _context.AdOrders.ToListAsync();
            var hasChanges = false;

            foreach (var order in orders)
            {
                if (AdOrderWorkflowService.RefreshStatus(order))
                {
                    hasChanges = true;
                }
            }

            if (hasChanges)
            {
                await _context.SaveChangesAsync();
            }
        }
    }
}