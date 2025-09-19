using App.web.Data;
using App.web.Models;
using App.web.Services;
using App.web.Services.Interfaces;
using App.web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace App.web.Controllers
{
    public class AdministrationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _authService;

        public AdministrationController(ApplicationDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        [Authorize]
        private User? GetCurrentUser()
        {
            var idUser = User.FindFirst(ClaimTypes.NameIdentifier);

            if (idUser == null) return null;

            Guid idUserPortal = Guid.Parse(idUser.Value);

            return _context.Users.Include(u => u.UserRoles).FirstOrDefault(u => u.UserId == idUserPortal);
        }

        // GET: Users
        [Authorize]
        public async Task<IActionResult> Index(string searchTerm, int page = 1, int pageSize = 4)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(u => u.Email.Contains(searchTerm) || u.Profile.FullName.Contains(searchTerm));
            }

            int totalRecords = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            var users = await query
                .Include(u => u.Profile)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .OrderBy(u => u.UserId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new AdministrationViewModel
            {
                Users = users,
                SearchTerm = searchTerm,
                CurrentPage = page,
                TotalPages = totalPages
            };

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_UsersTable", viewModel);
            }

            return View(viewModel);
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            if (id == Guid.Empty) return NotFound();

            var user = await _context.Users
                .Include(u => u.Profile)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(m => m.UserId == id);

            if (user == null) return NotFound();

            return PartialView("_UsersDetails", user);
        }

        // GET: Users/Edit/{id}
        public async Task<IActionResult> Edit(Guid id)
        {
            var user = await _context.Users
                .Include(u => u.Profile)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null) return NotFound();

            var vm = new UserEditViewModel
            {
                UserId = user.UserId,
                Email = user.Email,
                IsActive = user.IsActive,
                EmailConfirmed = user.EmailConfirmed,
                FullName = user.Profile?.FullName,
                PhotoUrl = user.Profile?.PhotoUrl,
                TimeZone = user.Profile?.TimeZone,
                Language = user.Profile?.Language,
                SelectedRoles = user.UserRoles.Select(ur => ur.RoleId).ToList()
            };

            ViewBag.AllRoles = await _context.Roles.ToListAsync();

            return PartialView("_UsersEdit", vm);
        }

        // POST: Users/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, UserEditViewModel model)
        {
            if (id != model.UserId) return BadRequest();

            if (ModelState.IsValid)
            {
                try
                {
                    var userDb = await _context.Users
                        .Include(u => u.Profile)
                        .Include(u => u.UserRoles)
                        .FirstOrDefaultAsync(u => u.UserId == id);

                    if (userDb == null) return NotFound();

                    userDb.Email = model.Email;
                    userDb.IsActive = model.IsActive;
                    userDb.EmailConfirmed = model.EmailConfirmed;
                    userDb.UpdatedAt = DateTime.UtcNow;

                    if (userDb.Profile != null)
                    {
                        userDb.Profile.FullName = model.FullName;
                        userDb.Profile.PhotoUrl = model.PhotoUrl;
                        userDb.Profile.TimeZone = model.TimeZone;
                        userDb.Profile.Language = model.Language;
                    }
                    else
                        userDb.Profile = new UserProfile
                        {
                            ProfileId = Guid.NewGuid(),
                            UserId = userDb.UserId,
                            FullName = model.FullName
                        };

                    userDb.UserRoles.Clear();
                    if (model.SelectedRoles != null && model.SelectedRoles.Any())
                    {
                        foreach (var roleId in model.SelectedRoles)
                        {
                            userDb.UserRoles.Add(new UserRole
                            {
                                UserId = userDb.UserId,
                                RoleId = roleId
                            });
                        }
                    }

                    await _context.SaveChangesAsync();

                    var users = await _context.Users
                        .Include(u => u.Profile)
                        .Include(u => u.UserRoles)
                            .ThenInclude(ur => ur.Role)
                        .ToListAsync();

                    var viewModel = new AdministrationViewModel
                    {
                        Users = users,
                        SearchTerm = "",
                        CurrentPage = 1,
                        TotalPages = 1
                    };

                    return Json(new
                    {
                        success = true,
                        html = await this.RenderViewAsync("_UsersTable", viewModel, true)
                    });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Users.Any(e => e.UserId == model.UserId))
                        return NotFound();

                    throw;
                }
            }

            ViewBag.AllRoles = await _context.Roles.ToListAsync();

            return Json(new
            {
                success = false,
                html = await this.RenderViewAsync("_UsersEdit", model, true)
            });
        }

        // GET: Users/Delete/{id}
        public async Task<IActionResult> Delete(Guid id)
        {
            var user = await _context.Users
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
                return NotFound();

            return PartialView("_UsersDelete", user);
        }

        // POST: Users/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }

            var users = await _context.Users
                .Include(u => u.Profile)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .OrderBy(u => u.UserId)
                .ToListAsync();

            var viewModel = new AdministrationViewModel
            {
                Users = users,
                SearchTerm = "",
                CurrentPage = 1,
                TotalPages = 1
            };

            return Json(new
            {
                success = true,
                html = await this.RenderViewAsync("_UsersTable", viewModel, true)
            });
        }

        // GET: Users/Create/{id}
        public async Task<IActionResult> Create()
        {
            ViewBag.AllRoles = await _context.Roles.ToListAsync();

            return PartialView("_UsersCreate");
        }

        // POST: Users/Create/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var register = await _authService.RegisterAsync(model.FullName, model.Email, model.Password);

                var users = await _context.Users
                    .Include(u => u.Profile)
                    .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                    .ToListAsync();

                var viewModel = new AdministrationViewModel
                {
                    Users = users,
                    SearchTerm = "",
                    CurrentPage = 1,
                    TotalPages = 1
                };

                return Json(new
                {
                    success = true,
                    html = await this.RenderViewAsync("_UsersTable", viewModel, true)
                });
            }

            ViewBag.AllRoles = await _context.Roles.ToListAsync();

            return Json(new
            {
                success = false,
                html = await this.RenderViewAsync("_UsersCreate", model, true)
            });
        }
    }
}
