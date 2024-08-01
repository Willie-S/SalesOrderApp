using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SalesOrderApp.Interfaces;
using SalesOrderApp.Models;
using SalesOrderApp.Repositories;
using SalesOrderApp.ViewModels;
using System.Security.Claims;

namespace SalesOrderApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserRepository _userRepository;
        private readonly XmlUserRepository _xmlUserRepository;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ILogger<AuthController> _logger;

        public AuthController(UserRepository userRepository, XmlUserRepository xmlUserRepository, IPasswordHasher<User> passwordHasher, ILogger<AuthController> logger)
        {
            _userRepository = userRepository;
            _xmlUserRepository = xmlUserRepository;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        public IActionResult Signup()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Signup(SignupViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                if (await _userRepository.EmailExistsAsync(model.Email))
                {
                    ModelState.AddModelError(string.Empty, "Email already exists.");
                    return View(model);
                }

                var user = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    UserRoleId = (int)model.UserRole
                };

                user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);

                try
                {
                    await _userRepository.AddUserAsync(user);
                    await _xmlUserRepository.AddUserAsync(user);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to add user to the DB.");
                    ModelState.AddModelError(string.Empty, "An error occurred while saving the user data.");
                    return View(model);
                }

                TempData["SignupSuccess"] = "Signup successful.";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during signup.");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
                return View(model);
            }
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                var user = await _userRepository.GetUserByEmailAsync(model.Email);

                if (user == null || _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password) != PasswordVerificationResult.Success)
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }

                // Set up authentication
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Email),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                TempData["LoginSuccess"] = "Login successful.";
                return RedirectToAction("Index", "Orders");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during login.");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
                return View();
            }
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
