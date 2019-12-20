using System.ComponentModel.DataAnnotations;

namespace B2S.Core.Enums
{
    public enum UserType
    {
        [Display(Name = "Agent")]
        Agent = 1,
        [Display(Name = "Customer")]
        Customer = 2,
        [Display(Name = "Vendor")]
        Vendor = 3,
        [Display(Name = "Admin")]
        Admin = 4

    }
}
