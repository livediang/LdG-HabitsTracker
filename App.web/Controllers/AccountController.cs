using App.web.Data;
using App.web.Models;
using App.web.Services;
using App.web.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace App.web.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AuthService _authService;
        private readonly TokenService _tokenService;
        private readonly EmailService _emailService;

        public AccountController(ApplicationDbContext context, AuthService authService, TokenService tokenService, EmailService emailService)
        {
            _context = context;
            _authService = authService;
            _tokenService = tokenService;
            _emailService = emailService;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _authService.RegisterAsync(model.FullName, model.Email, model.Password);

                    var token = _tokenService.GenerateEmailConfirmationToken(user.UserId, user.Email);

                    var encodedToken = System.Web.HttpUtility.UrlEncode(token);

                    var confirmUrl = Url.Action("ConfirmEmail", "Account", new { token = encodedToken }, Request.Scheme);

                    await _emailService.SendAsync(
                        user.Email,
                        "Confirm your account",
                        $@"
                            <h2>Welcome, {model.FullName}!</h2>
                            <p>Thank you for registering with <strong>Habits Tracker</strong>.</p>
                            <p>Please confirm your email address by clicking the button below:</p>
                            <p>
                                <a href='{confirmUrl}' 
                                   style='display:inline-block;padding:10px 20px;
                                          background-color:#4CAF50;color:white;
                                          text-decoration:none;border-radius:5px;'>
                                    Confirm Email
                                </a>
                            </p>
                            <p>If you didn’t create this account, you can safely ignore this email.</p>
                            <p>– The Habits Tracker Team</p>
                        "
                    );

                    ViewBag.Message = "A link has been sent to your email to confirm your email account.";

                    return RedirectToAction("Login");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ConfirmEmail(string token)
        {
            if (string.IsNullOrEmpty(token)) return BadRequest("Required Token.");

            try
            {
                var decodedToken = System.Web.HttpUtility.UrlDecode(token);

                var (userId, email, created) = _tokenService.ValidateToken(decodedToken);

                if (created.AddHours(24) < DateTime.UtcNow) return BadRequest("Expired Token.");

                var user = _context.Users.FirstOrDefault(u => u.UserId == userId && u.Email == email);
                if (user == null) return BadRequest("Invalid Token.");

                user.EmailConfirmed = true;
                _context.SaveChanges();

                return View("ConfirmEmailSuccess");
            }
            catch
            {
                return BadRequest("Invalid Token.");
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _authService.LoginAsync(model.Email, model.Password);

                if (!result.Success)
                {
                    ModelState.AddModelError("", result.Message);
                    return View(model);
                }

                var user = result.User;

                // Cookie Auth Scheme Claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Email, user.Email)
                };

                if (user.Profile != null)
                {
                    claims.Add(new Claim("FullName", user.Profile.FullName));
                }

                foreach (var role in user.UserRoles.Select(ur => ur.Role.Name))
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // Config cookie
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
                };

                // In this point... build the cookie
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties
                );

                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult LoginReset() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> LoginReset(string mailUser)
        {
            var userReset = _context.Users.FirstOrDefault(u => u.Email == mailUser);
            if (userReset == null)
            {
                ViewBag.Message = "Email not register.";
                return View();
            }

            var token = _tokenService.GenerateEmailConfirmationToken(userReset.UserId, userReset.Email);

            var encodedToken = System.Web.HttpUtility.UrlEncode(token);

            string resetLink = Url.Action("ResetPassword", "Account", new { token = encodedToken }, Request.Scheme);

            await _emailService.SendAsync(
                userReset.Email,
                "Reset your Habits Tracker password",
                $@"
                    <h2>Password Reset Request</h2>
                    <p>Hello,</p>
                    <p>We received a request to reset the password for your <strong>Habits Tracker</strong> account.</p>
                    <p>Please click the button below to set a new password:</p>
                    <p>
                        <a href='{resetLink}'
                           style='display:inline-block;padding:10px 20px;
                                  background-color:#007BFF;color:white;
                                  text-decoration:none;border-radius:5px;'>
                            Reset Password
                        </a>
                    </p>
                    <p>If you did not request this, you can safely ignore this email.</p>
                    <p>– The Habits Tracker Team</p>
                "
            );

            ViewBag.Message = "A link has been sent to your email to reset your password.";
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
                return BadRequest("Invalid password reset token.");

            var model = new ResetPasswordViewModel { Token = token };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var (userId, email, created) = _tokenService.ValidateToken(model.Token);

                var user = _context.Users.FirstOrDefault(u => u.UserId == userId && u.Email == email);
                if (user == null)
                    return BadRequest("Invalid token.");

                user.PasswordHash = _authService.HashPassword(model.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;

                _context.SaveChanges();

                TempData["Message"] = "Your password has been reset successfully. Please login.";
                return RedirectToAction("Login");
            }
            catch
            {
                return BadRequest("Invalid or expired token.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
