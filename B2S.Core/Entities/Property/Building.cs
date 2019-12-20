using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace B2S.Core.Entities
{
    public class Building : BaseEntity
    {
        public Building()
        {
            this.CreatedAt = DateTime.UtcNow;
        }

        [StringLength(38)]
        [Key]
        [Display(Name = "Building Id")]
        public string BuildingId { get; set; }

        [StringLength(250)]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [StringLength(38)]
        [Display(Name = "Property Id")]
        [Required]
        public string PropertyId { get; set; }
        [Display(Name = "Property")]
        public Property Property { get; set; }
    }
}
