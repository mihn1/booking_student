using B2S.Core.Entities;
using B2S.Core.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace B2S.Web.ViewModels
{
    public class CalendarCellViewModel
    {
        public string BookingId { get; set; }

        public Booking Booking { get; set; }

        public DateTime BookingFrom { get; set; }

        public string BedId { get; set; }

        public string HTMLClass { get; set; }

        public string HTMLInner { get; set; }

        public bool IsHeader { get; set; }

        public BookingStatus Status { get; set; }

        public string Gender { get; set; }

        public bool IsGreenTick { get; set; }

        public bool IsTwoBooking { get; set; }
    }
}
