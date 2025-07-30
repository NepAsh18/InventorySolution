using InventorySolution.Models.Entities;
using InventorySolution.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventorySolution.Controllers;

[Authorize(Roles = "Admin")]
public class AdminCustomerController(
    UserManager<AppUser> userManager,
    RoleManager<IdentityRole> roleManager) : Controller
{
    // GET: AdminCustomer
    public async Task<IActionResult> Index()
    {
        // Get all users first
        var allUsers = userManager.Users.ToList();

        // Filter users in "User" role in memory
        var usersInRole = new List<AppUser>();
        foreach (var user in allUsers)
        {
            if (await userManager.IsInRoleAsync(user, "User"))
            {
                usersInRole.Add(user);
            }
        }

        var model = usersInRole.Select(u => new CustomerViewModel
        {
            Id = u.Id,
            FullName = u.FullName,
            Email = u.Email,
            SecurityQuestion = u.SecurityQuestion
        }).ToList();

        return View(model);
    }

    // GET: AdminCustomer/Details/5
    public async Task<IActionResult> Details(string id)
    {
        if (string.IsNullOrEmpty(id)) return NotFound();

        var user = await userManager.FindByIdAsync(id);
        if (user == null || !await userManager.IsInRoleAsync(user, "User"))
            return NotFound();

        return View(new CustomerDetailViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            SecurityQuestion = user.SecurityQuestion
        });
    }

    // GET: AdminCustomer/Create
    public IActionResult Create() => View(new AdminCreateCustomerViewModel
    {
        AvailableSecurityQuestions = AccountController.GetSecurityQuestions()
    });

    // POST: AdminCustomer/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminCreateCustomerViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableSecurityQuestions = AccountController.GetSecurityQuestions();
            return View(model);
        }

        var user = new AppUser
        {
            FullName = model.FullName,
            UserName = model.Email,
            Email = model.Email,
            SecurityQuestion = model.SecurityQuestion,
            SecurityAnswer = AccountController.HashSecurityAnswer(model.SecurityAnswer)
        };

        var result = await userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            model.AvailableSecurityQuestions = AccountController.GetSecurityQuestions();
            return View(model);
        }

        if (!await roleManager.RoleExistsAsync("User"))
            await roleManager.CreateAsync(new IdentityRole("User"));

        await userManager.AddToRoleAsync(user, "User");
        return RedirectToAction(nameof(Index));
    }

    // GET: AdminCustomer/Edit/5
    public async Task<IActionResult> Edit(string id)
    {
        if (string.IsNullOrEmpty(id)) return NotFound();

        var user = await userManager.FindByIdAsync(id);
        if (user == null || !await userManager.IsInRoleAsync(user, "User"))
            return NotFound();

        return View(new AdminEditCustomerViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            SecurityQuestion = user.SecurityQuestion,
            AvailableSecurityQuestions = AccountController.GetSecurityQuestions()
        });
    }

    // POST: AdminCustomer/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AdminEditCustomerViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableSecurityQuestions = AccountController.GetSecurityQuestions();
            return View(model);
        }

        var user = await userManager.FindByIdAsync(model.Id);
        if (user == null || !await userManager.IsInRoleAsync(user, "User"))
            return NotFound();

        user.FullName = model.FullName;
        user.Email = model.Email;
        user.UserName = model.Email;
        user.SecurityQuestion = model.SecurityQuestion;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            model.AvailableSecurityQuestions = AccountController.GetSecurityQuestions();
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: AdminCustomer/Delete/5
    public async Task<IActionResult> Delete(string id)
    {
        if (string.IsNullOrEmpty(id)) return NotFound();

        var user = await userManager.FindByIdAsync(id);
        if (user == null || !await userManager.IsInRoleAsync(user, "User"))
            return NotFound();

        return View(new CustomerViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email
        });
    }

    // POST: AdminCustomer/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        try
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "User ID is required.";
                return RedirectToAction(nameof(Index));
            }

            var user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            if (!await userManager.IsInRoleAsync(user, "User"))
            {
                TempData["ErrorMessage"] = "User is not in the required role.";
                return RedirectToAction(nameof(Index));
            }

            var result = await userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                // Aggregate errors
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                TempData["ErrorMessage"] = $"Deletion failed: {errors}";
                return RedirectToAction(nameof(Delete), new { id });
            }

            TempData["SuccessMessage"] = "User deleted successfully.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
            return RedirectToAction(nameof(Delete), new { id });
        }

        return RedirectToAction(nameof(Index));
    }
}