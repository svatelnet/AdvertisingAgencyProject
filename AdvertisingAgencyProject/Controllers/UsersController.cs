using AdvertisingAgencyProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AdvertisingAgencyProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private static readonly string[] AppRoles = new[] { "Admin", "Manager", "Client" };

        private readonly UserManager<IdentityUser> _userManager;

        public UsersController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users
                .OrderBy(u => u.Email)
                .ToList();

            var model = new List<UserWithRolesViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                model.Add(new UserWithRolesViewModel
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    UserName = user.UserName ?? string.Empty,
                    Roles = roles.ToList(),
                    CurrentRole = GetCurrentRole(roles)
                });
            }

            return View(model);
        }

        public IActionResult CreateManager()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateManager(CreateManagerViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError(string.Empty, "Пользователь с таким email уже существует.");
                return View(model);
            }

            var user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Manager");

                TempData["SuccessMessage"] = "Менеджер успешно создан.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRole(string id, string role)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(role) || !AppRoles.Contains(role))
            {
                TempData["ErrorMessage"] = "Выбрана некорректная роль.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var currentRoles = (await _userManager.GetRolesAsync(user))
                .Where(r => AppRoles.Contains(r))
                .ToList();

            var currentUserId = _userManager.GetUserId(User);

            if (user.Id == currentUserId && role != "Admin")
            {
                TempData["ErrorMessage"] = "Нельзя изменить свою роль администратора на другую.";
                return RedirectToAction(nameof(Index));
            }

            if (currentRoles.Contains("Admin") && role != "Admin")
            {
                var admins = await _userManager.GetUsersInRoleAsync("Admin");
                if (admins.Count <= 1)
                {
                    TempData["ErrorMessage"] = "Нельзя изменить роль последнего администратора.";
                    return RedirectToAction(nameof(Index));
                }
            }

            if (currentRoles.Count == 1 && currentRoles.Contains(role))
            {
                TempData["SuccessMessage"] = "У пользователя уже установлена эта роль.";
                return RedirectToAction(nameof(Index));
            }

            var rolesToRemove = currentRoles
                .Where(r => r != role)
                .ToList();

            if (rolesToRemove.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if (!removeResult.Succeeded)
                {
                    TempData["ErrorMessage"] = "Не удалось обновить роль пользователя.";
                    return RedirectToAction(nameof(Index));
                }
            }

            if (!currentRoles.Contains(role))
            {
                var addResult = await _userManager.AddToRoleAsync(user, role);
                if (!addResult.Succeeded)
                {
                    TempData["ErrorMessage"] = "Не удалось назначить новую роль.";
                    return RedirectToAction(nameof(Index));
                }
            }

            TempData["SuccessMessage"] = "Роль пользователя обновлена.";
            return RedirectToAction(nameof(Index));
        }

        private static string GetCurrentRole(IList<string> roles)
        {
            if (roles.Contains("Admin"))
            {
                return "Admin";
            }

            if (roles.Contains("Manager"))
            {
                return "Manager";
            }

            if (roles.Contains("Client"))
            {
                return "Client";
            }

            return string.Empty;
        }
    }
}