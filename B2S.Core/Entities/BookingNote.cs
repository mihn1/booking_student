using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace B2S.Core.Entities
{
    public class BookingNote : BaseEntity
    {
        public BookingNote()
        {
            this.CreatedAt = DateTime.UtcNow;
        }

        [StringLength(38)]
        [Key]
        [Display(Name = "Note Id")]
        public string NoteId { get; set; }

        [StringLength(38)]
        [Required]
        [Display(Name = "Booking Id")]
        public string BookingId { get; set; }

        [StringLength(250)]
        [Display(Name = "Title")]
        public string Title { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        
    }
}
