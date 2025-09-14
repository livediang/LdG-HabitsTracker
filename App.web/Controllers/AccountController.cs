using App.web.Services.Interfaces;
using App.web.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace App.web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;

        public AccountController(IAuthService authService, IUserService userService, ITokenService tokenService, IEmailService emailService)
        {
            _authService = authService;
            _userService = userService;
            _tokenService = tokenService;
            _emailService = emailService;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register() => View();

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Message"] = "Please fix the highlighted errors.";
                TempData["MessageType"] = "error";
                return View(model);
            }

            try
            {
                var register = await _authService.RegisterAsync(model.FullName, model.Email, model.Password);

                if (register.Success != false)
                {
                    var token = _tokenService.GenerateEmailConfirmationToken(register.User.UserId, register.User.Email);
                    var encodedToken = System.Net.WebUtility.UrlEncode(token);

                    var confirmUrl = Url.Action("ConfirmEmail", "Account", new { token = encodedToken }, Request.Scheme);
                    await _emailService.SendConfirmEmailAsync(model.Email, model.FullName, confirmUrl);

                    TempData["Message"] = register.Message;
                    TempData["MessageType"] = "success";
                    return View(model);
                }
                TempData["Message"] = register.Message;
                TempData["MessageType"] = "error";
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex;
                TempData["MessageType"] = "error";
                return View(model);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string token)
        {
            if (string.IsNullOrEmpty(token)) return BadRequest("InvalidToken.");

            var decodedToken = System.Net.WebUtility.UrlDecode(token);
            var result = _tokenService.ValidateEmailConfirmationToken(decodedToken);
            if (!result.IsValid) return View("InvalidToken");

            var user = await _userService.FindByIdAndEmail(result.UserId, result.Email);
            if (user == null) return View("InvalidToken");

            await _userService.ConfirmEmail(user);
            return View("ConfirmEmailSuccess");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login() => View();

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _authService.LoginAsync(model.Email, model.Password);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                return View(model);
            }

            var user = result.User;
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("FullName", user.Profile?.FullName ?? "")
            };

            foreach (var role in user.UserRoles?
                .Where(r => r?.Role?.Name != null)
                .Select(r => r.Role.Name) ?? Enumerable.Empty<string>())
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
                });

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            var user = await _userService.FindByEmail(model.Email);
            if (user == null)
            {
                TempData["Message"] = "Email not registered.";
                return View();
            }

            var token = _tokenService.GeneratePasswordResetToken(user.UserId, user.Email);
            var encodedToken = System.Net.WebUtility.UrlEncode(token);
            var resetUrl = Url.Action("ResetPassword", "Account", new { token = encodedToken }, Request.Scheme);

            await _emailService.SendPasswordResetAsync(user.Email, resetUrl);

            TempData["Message"] = "Check your email to reset your password.";
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token)) return View("InvalidToken");

            return View(new ResetPasswordViewModel { Token = token });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var decodedToken = System.Net.WebUtility.UrlDecode(model.Token);
            var result = _tokenService.ValidatePasswordResetToken(decodedToken);
            if (!result.IsValid) return View("InvalidToken");

            var user = await _userService.FindByIdAndEmail(result.UserId, result.Email);
            if (user == null) return View("InvalidToken");

            await _authService.UpdatePasswordAsync(user, model.NewPassword);

            return View("ResetPasswordSuccess");
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied() => View();
    }
}
