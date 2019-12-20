using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace B2S.Core.Entities
{
    public class BookingData
    {
        [StringLength(38)]
        [Key]
        [Display(Name = "Booking Id")]
        public string BookingId { get; set; }

        [Display(Name = "Customer Name")]
        [StringLength(200)]
        public string CustomerName { get; set; }

        [Display(Name = "Country")]
        [StringLength(20)]
        public string Country { get; set; }

        [Display(Name = "Gender")]
        [StringLength(20)]
        public string Gender { get; set; }

        [Display(Name = "Property")]
        [StringLength(200)]
        public string Property { get; set; }

        [Display(Name = "Room")]
        [StringLength(20)]
        public string Room { get; set; }

        [Display(Name = "Bed")]
        [StringLength(20)]
        public string Bed { get; set; }

        [Display(Name = "Agent")]
        [StringLength(20)]
        public string Agent { get; set; }

        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Display(Name = "Finish Date")]
        public DateTime FinishDate { get; set; }


    }
}
