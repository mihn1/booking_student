using System.ComponentModel.DataAnnotations;

namespace B2S.Core.Enums
{
    public enum DocumentCategory
    {
        [Display(Name = "CheckIn")]
        CheckIn = 1,
        [Display(Name = "CheckOut")]
        CheckOut = 2,
        [Display(Name = "Booking")]
        Booking = 3,
        [Display(Name = "Other")]
        Other = 4
    }
}
