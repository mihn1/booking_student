using B2S.Core.Entities;
using System;
using System.ComponentModel.DataAnnotations;

namespace B2S.Web.ViewModels
{
    public class BookingMovingViewModel
    {
        [StringLength(38)]
        [Required]
        [Display(Name = "Booking Id")]
        public string BookingId { get; set; }

        [Display(Name = "Move Date From")]
        [Required]
        public DateTime BookingFrom { get; set; }

        [Display(Name = "Move Date To")]
        [Required]
        public DateTime BookingTo { get; set; }

        [StringLength(38)]
        [Display(Name = "Move To Bed")]
        [Required]
        public string BedId { get; set; }
        [Display(Name = "Bed")]
        public Bed Bed { get; set; }
    }
}
