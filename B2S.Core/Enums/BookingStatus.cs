using System.ComponentModel.DataAnnotations;

namespace B2S.Core.Enums
{
    public enum BookingStatus
    {
        [Display(Name = "#3c8dbc")]
        New = 1, // When booking is created
        [Display(Name = "#f39c12")]
        Confirmed = 2, // When booking is confirmed by student and agent get paid
        [Display(Name = "#f39c12")]
        CheckIn = 3, // When student check in
        [Display(Name = "#00a65a")]
        Paid = 4, // When payment was genereated
        [Display(Name = "#00a65a")]
        CheckOut = 5 // When student check out
    }
}
