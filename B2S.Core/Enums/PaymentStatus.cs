using System.ComponentModel.DataAnnotations;

namespace B2S.Core.Enums
{
    public enum PaymentStatus
    {
        [Display(Name = "Pending")]
        Pending = 1,
        [Display(Name = "Due")]
        Due = 2,
        [Display(Name = "Sent")]
        Sent = 3,
        [Display(Name = "Paid")]
        Paid = 4
    }
}
