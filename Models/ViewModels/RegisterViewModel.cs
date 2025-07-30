using System.ComponentModel.DataAnnotations;

namespace InventorySolution.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(40, MinimumLength = 8, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.")]
        [DataType(DataType.Password)]
        [Compare("ConfirmPassword", ErrorMessage = "Password does not match.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }

        // 🔐 Add this for displaying the dropdown
        public List<string> AvailableSecurityQuestions { get; set; }

        [Required(ErrorMessage = "Please select a security question.")]
        [Display(Name = "Security Question")]
        public string SecurityQuestion { get; set; }

        [Required(ErrorMessage = "Security answer is required.")]
        [Display(Name = "Security Answer")]
        public string SecurityAnswer { get; set; }
    }
}
