using B2S.Core.Entities;
using B2S.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace B2S.Web.ViewModels
{
    public class BedBookingViewModel
    {
        [StringLength(38)]
        [Display(Name = "Bed Id")]
        public string BedId { get; set; }
        public Bed Bed { get; set; }

        [StringLength(38)]
        [Display(Name = "Customer Id")]
        public string CustomerId { get; set; }
        [Display(Name = "Customer")]
        public Customer Customer { get; set; }

        [Display(Name = "Booking From")]
        public DateTime BookingFrom { get; set; }

        [Display(Name = "Booking To")]
        public DateTime BookingTo { get; set; }

        public CalendarHeaderType HeaderType { get; set; }

        public bool IsHeader { get; set; }
    }
}
