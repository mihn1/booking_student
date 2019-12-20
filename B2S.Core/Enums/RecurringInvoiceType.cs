using System.ComponentModel.DataAnnotations;

namespace B2S.Core.Enums
{
    public enum RecurringInvoiceType
    {
        [Display(Name = "Days")]
        Days = 1,
        [Display(Name = "Weeks")]
        Weeks = 2,
        [Display(Name = "Months")]
        Months = 3
    }
}
