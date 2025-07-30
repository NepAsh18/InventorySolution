using InventorySolution.Models.Entities;
using InventorySolution.Models.ViewModels;
using InventorySolution.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace InventorySolution.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<AppUser> signInManager;
        private readonly UserManager<AppUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly INotificationService _notificationService;

        public AccountController(
            SignInManager<AppUser> signInManager,
            UserManager<AppUser> userManager,
            INotificationService notificationService,
            RoleManager<IdentityRole> roleManager)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.roleManager = roleManager;
            _notificationService = notificationService;
        }

        // 🔐 Security Questions List
        public static List<string> GetSecurityQuestions() => new()
        {
            "What's your pet name?",
            "What is your favourite book?",
            "Which is the most exciting place you have traveled to recently?",
            "What is your mother’s maiden name?",
            "What was your childhood nickname?"
        };

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Invalid Login Attempt.");
                return View(model);
            }
            if (TempData.ContainsKey("DirectOrder_ProductId"))
            {
                TempData.Keep("DirectOrder_ProductId");
                TempData.Keep("DirectOrder_Quantity");
            }

            var user = await userManager.FindByEmailAsync(model.Email);
            var roles = await userManager.GetRolesAsync(user);

            // Store user information in session
            HttpContext.Session.SetString("UserId", user.Id);
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetString("UserRoles", string.Join(",", roles));

            return roles.Contains("Admin")
                ? RedirectToAction("Dashboard", "Admin")
                : RedirectToAction("Index", "CustomerProduct");
        }

        [HttpGet]
        public IActionResult Register() => View(new RegisterViewModel
        {
            AvailableSecurityQuestions = GetSecurityQuestions()
        });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableSecurityQuestions = GetSecurityQuestions();
                return View(model);
            }

            var user = new AppUser
            {
                FullName = model.Name,
                UserName = model.Email,
                Email = model.Email,
                SecurityQuestion = model.SecurityQuestion,
                SecurityAnswer = HashSecurityAnswer(model.SecurityAnswer)
            };

            var result = await userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                model.AvailableSecurityQuestions = GetSecurityQuestions();
                return View(model);
            }

            if (!await roleManager.RoleExistsAsync("User"))

                await _notificationService.CreateNotificationAsync(
       $"New customer registered: {model.Email}",
       "NewCustomer",
        relatedEntityId: null
   );
            await roleManager.CreateAsync(new IdentityRole("User"));

            await userManager.AddToRoleAsync(user, "User");
            await signInManager.SignInAsync(user, isPersistent: false);

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult VerifyEmail() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyEmail(VerifyEmailViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "User not found!");
                return View(model);
            }

            var answerMatches = user.SecurityAnswer == HashSecurityAnswer(model.SecurityAnswer);
            if (user.SecurityQuestion == model.SecurityQuestion && answerMatches)
                return RedirectToAction("ChangePassword", new { email = user.Email });

            ModelState.AddModelError("", "Security question or answer is incorrect.");
            return View(model);
        }

        [HttpGet]
        public IActionResult ChangePassword(string email) =>
            string.IsNullOrEmpty(email)
                ? RedirectToAction("VerifyEmail")
                : View(new ChangePasswordViewModel { Email = email });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Please correct the errors");
                return View(model);
            }

            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "User not found!");
                return View(model);
            }

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var result = await userManager.ResetPasswordAsync(user, token, model.NewPassword);

            if (result.Succeeded)
                return RedirectToAction("Login");

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Clear all session data
            HttpContext.Session.Clear();

            // Sign out the user
            await signInManager.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }

        public static string HashSecurityAnswer(string answer)
        {
            if (string.IsNullOrEmpty(answer)) return string.Empty;
            var bytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(answer));
            return Convert.ToBase64String(bytes);
        }

        
    }
}