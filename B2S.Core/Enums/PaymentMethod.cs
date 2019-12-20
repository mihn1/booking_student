using System.ComponentModel.DataAnnotations;

namespace B2S.Core.Enums
{
    public enum PaymentMethod
    {
        [Display(Name = "Cash")]
        Cash = 1,
        [Display(Name = "Cheque")]
        Cheque = 2,
        [Display(Name = "Bank Transfer")]
        BankTransfer = 3,
        [Display(Name = "PayPal")]
        PayPal = 4
    }
}
