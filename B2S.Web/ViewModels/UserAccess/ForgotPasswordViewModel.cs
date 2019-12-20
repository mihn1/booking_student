using System.ComponentModel.DataAnnotations;

namespace B2S.Web.ViewModels.UserAccess
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
