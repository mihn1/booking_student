using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace B2S.Core.Entities
{
    public class Room : BaseEntity
    {
        public Room()
        {
            this.CreatedAt = DateTime.UtcNow;
        }

        [StringLength(38)]
        [Key]
        [Display(Name = "Room Id")]
        public string RoomId { get; set; }

        [StringLength(250)]
        [Display(Name = "Room Name")]
        public string RoomName { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [StringLength(38)]
        [Display(Name = "Property Id")]
        [Required]
        public string PropertyId { get; set; }
        [Display(Name = "Property")]
        public Property Property { get; set; }


        [StringLength(38)]
        [Display(Name = "Building Id")]      
        public string BuildingId { get; set; }
        [Display(Name = "Building")]
        public Building Building { get; set; }


        [StringLength(38)]
        [Display(Name = "RoomType")]
        public string RoomTypeId { get; set; }
        [Display(Name = "RoomType")]
        public RoomType RoomType { get; set; }
    }
}
