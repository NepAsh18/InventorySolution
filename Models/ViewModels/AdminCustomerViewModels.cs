// Models/ViewModels/AdminCustomerViewModels.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InventorySolution.Models.ViewModels
{
    public class CustomerViewModel
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string SecurityQuestion { get; set; }
    }

    public class CustomerDetailViewModel : CustomerViewModel
    {
        // Additional properties for details if needed
    }

    public class AdminCreateCustomerViewModel
    {
        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [Display(Name = "Security Question")]
        public string SecurityQuestion { get; set; }

        [Required]
        [Display(Name = "Security Answer")]
        public string SecurityAnswer { get; set; }

        public List<string> AvailableSecurityQuestions { get; set; }
    }

    public class AdminEditCustomerViewModel
    {
        public string Id { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Security Question")]
        public string SecurityQuestion { get; set; }

        public List<string> AvailableSecurityQuestions { get; set; }
    }

    public class EditMyProfileViewModel
    {
        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Security Question")]
        public string SecurityQuestion { get; set; }

        [Display(Name = "Security Answer")]
        public string SecurityAnswer { get; set; }

        public List<string> AvailableSecurityQuestions { get; set; }
    }
}