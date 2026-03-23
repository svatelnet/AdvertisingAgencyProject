using AdvertisingAgencyProject.Data;
using AdvertisingAgencyProject.Models;
using AdvertisingAgencyProject.Services;
using AdvertisingAgencyProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AdvertisingAgencyProject.Controllers
{
    [Authorize(Roles = "Client")]
    public class ClientCabinetController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ClientCabinetController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var client = await GetOrCreateCurrentClientAsync();
            if (client == null)
            {
                return NotFound("Не удалось определить текущего клиента.");
            }

            await UpdateMyOrdersStatusesAsync(client.Id);

            var recentOrders = await _context.AdOrders
                .Where(o => o.ClientId == client.Id)
                .Include(o => o.AdCategory)
                .OrderByDescending(o => o.StartDate)
                .Take(5)
                .ToListAsync();

            var model = new ClientDashboardViewModel
            {
                Client = client,
                RecentOrders = recentOrders
            };

            return View(model);
        }

        public async Task<IActionResult> Orders()
        {
            var client = await GetOrCreateCurrentClientAsync();
            if (client == null)
            {
                return NotFound();
            }

            await UpdateMyOrdersStatusesAsync(client.Id);

            var orders = await _context.AdOrders
                .Where(o => o.ClientId == client.Id)
                .Include(o => o.AdCategory)
                .OrderByDescending(o => o.StartDate)
                .ToListAsync();

            return View(orders);
        }

        public async Task<IActionResult> OrderDetails(int id)
        {
            var client = await GetOrCreateCurrentClientAsync();
            if (client == null)
            {
                return NotFound();
            }

            var order = await _context.AdOrders
                .Include(o => o.Client)
                .Include(o => o.AdCategory)
                .FirstOrDefaultAsync(o => o.Id == id && o.ClientId == client.Id);

            if (order == null)
            {
                return NotFound();
            }

            AdOrderWorkflowService.RefreshStatus(order);
            await _context.SaveChangesAsync();

            return View(order);
        }

        public async Task<IActionResult> EditProfile()
        {
            var client = await GetOrCreateCurrentClientAsync();
            if (client == null)
            {
                return NotFound();
            }

            var model = new ClientProfileViewModel
            {
                FullName = client.FullName,
                Email = client.Email ?? string.Empty,
                Phone = client.Phone,
                CompanyName = client.CompanyName
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(ClientProfileViewModel model)
        {
            var client = await GetOrCreateCurrentClientAsync();
            if (client == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                model.Email = client.Email ?? string.Empty;
                return View(model);
            }

            client.FullName = model.FullName.Trim();
            client.Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim();
            client.CompanyName = string.IsNullOrWhiteSpace(model.CompanyName) ? null : model.CompanyName.Trim();

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Профиль успешно обновлен.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> CreateRequest()
        {
            await LoadCategoriesAsync();

            return View(new ClientOrderRequestViewModel
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRequest(ClientOrderRequestViewModel model)
        {
            var client = await GetOrCreateCurrentClientAsync();
            if (client == null)
            {
                return NotFound();
            }

            var category = await _context.AdCategories.FindAsync(model.AdCategoryId);

            if (category == null)
            {
                ModelState.AddModelError(nameof(model.AdCategoryId), "Категория рекламы не найдена.");
            }

            if (model.EndDate.Date < model.StartDate.Date)
            {
                ModelState.AddModelError(nameof(model.EndDate), "Дата окончания не может быть раньше даты начала.");
            }

            if (!ModelState.IsValid)
            {
                await LoadCategoriesAsync(model.AdCategoryId);
                return View(model);
            }

            var order = new AdOrder
            {
                ClientId = client.Id,
                AdCategoryId = model.AdCategoryId,
                Title = model.Title.Trim(),
                Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim(),
                StartDate = model.StartDate,
                EndDate = model.EndDate
            };

            AdOrderWorkflowService.ApplyClientRequest(order, category!.BasePrice);

            _context.AdOrders.Add(order);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Заявка отправлена. После проверки менеджером она появится в работе.";
            return RedirectToAction(nameof(Orders));
        }

        private async Task<Client?> GetOrCreateCurrentClientAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || string.IsNullOrWhiteSpace(user.Email))
            {
                return null;
            }

            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Email == user.Email);
            if (client != null)
            {
                return client;
            }

            client = new Client
            {
                FullName = user.UserName ?? user.Email,
                Email = user.Email
            };

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            return client;
        }

        private async Task LoadCategoriesAsync(int? selectedId = null)
        {
            ViewData["AdCategoryId"] = new SelectList(
                await _context.AdCategories.OrderBy(c => c.Name).ToListAsync(),
                "Id",
                "Name",
                selectedId);
        }

        private async Task UpdateMyOrdersStatusesAsync(int clientId)
        {
            var orders = await _context.AdOrders
                .Where(o => o.ClientId == clientId)
                .ToListAsync();

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