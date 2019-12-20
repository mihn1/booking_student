using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace B2S.Core.Entities
{
    public class BuildingImage : BaseEntity
    {
        public BuildingImage()
        {
            this.CreatedAt = DateTime.UtcNow;
        }

        [StringLength(38)]
        [Key]
        [Display(Name = "Image Id")]
        public string ImageId { get; set; }

        [Display(Name = "Image File")]
        public byte[] ImageFile { get; set; }

        [StringLength(38)]
        [Display(Name = "Building Id")]
        [Required]
        public string BuildingId { get; set; }
        [Display(Name = "Building")]
        public Building Building { get; set; }

        [Display(Name = "IsDefault")]
        public bool IsDefault { get; set; }
    }
}
