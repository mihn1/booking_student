using B2S.Core.Enums;
using System;

namespace B2S.Web.ViewModels
{
    public class EventViewModel
    {
        public string BookingId { get; set; }

        public string Title { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string BackgroundColor { get; set; }

        public string BorderColor { get; set; }

        public string Description { get; set; }

        public EventType EventType { get; set; }

    }
}
