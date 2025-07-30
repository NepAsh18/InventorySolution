using System.ComponentModel.DataAnnotations;

namespace InventorySolution.Models.ViewModels
{
    public class VerifyEmailViewModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Security question is required.")]
        [Display(Name = "Security Question")]
        public string SecurityQuestion { get; set; }

        [Required(ErrorMessage = "Answer is required.")]
        [Display(Name = "Your Answer")]
        public string SecurityAnswer { get; set; }
    }
}
