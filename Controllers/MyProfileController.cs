using InventorySolution.Models.Entities;
using InventorySolution.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace InventorySolution.Controllers;

[Authorize(Roles = "User")]
public class MyProfileController(UserManager<AppUser> userManager) : Controller
{
    public async Task<IActionResult> Index()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        return View(new EditMyProfileViewModel
        {
            FullName = user.FullName,
            Email = user.Email,
            SecurityQuestion = user.SecurityQuestion,
            AvailableSecurityQuestions = AccountController.GetSecurityQuestions()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(EditMyProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableSecurityQuestions = AccountController.GetSecurityQuestions();
            return View(model);
        }

        var user = await userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        user.FullName = model.FullName;
        user.Email = model.Email;
        user.UserName = model.Email;
        user.SecurityQuestion = model.SecurityQuestion;

        if (!string.IsNullOrEmpty(model.SecurityAnswer))
        {
            user.SecurityAnswer = AccountController.HashSecurityAnswer(model.SecurityAnswer);
        }

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            model.AvailableSecurityQuestions = AccountController.GetSecurityQuestions();
            return View(model);
        }

        ViewBag.SuccessMessage = "Profile updated successfully!";
        model.AvailableSecurityQuestions = AccountController.GetSecurityQuestions();
        return View(model);
    }
}