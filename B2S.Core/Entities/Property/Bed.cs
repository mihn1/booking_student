using B2S.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace B2S.Core.Entities
{
    public class Bed : BaseEntity
    {
        public Bed()
        {
            this.CreatedAt = DateTime.UtcNow;
        }

        [StringLength(38)]
        [Key]
        [Display(Name = "Bed Id")]
        public string BedId { get; set; }

        [StringLength(250)]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [StringLength(38)]
        [Display(Name = "Room")]
        [Required]
        public string RoomId { get; set; }
        public Room Room { get; set; }

        [Display(Name = "Status")]
        public BedStatus Status { get; set; }

    }
}
