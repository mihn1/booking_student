using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.ComponentModel.DataAnnotations;

namespace B2S.Core.Entities
{
    public class BaseEntity
    {
        [Editable(false)]
        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Editable(false)]
        [Display(Name = "Created By")]
        [StringLength(38)]
        public string CreatedBy { get; set; }

        [Editable(false)]
        [Display(Name = "Modified At")]
        public DateTime ModifiedAt { get; set; }

        [Editable(false)]
        [Display(Name = "ModifiedBy")]
        [StringLength(38)]
        public string ModifiedBy { get; set; }

        [Editable(false)]
        [Display(Name = "Deleted At")]
        public DateTime DeletedAt { get; set; }

        [Editable(false)]
        [Display(Name = "DeletedBy")]
        [StringLength(38)]
        public string DeletedBy { get; set; }

        [Editable(false)]
        [Display(Name = "IsDeleted")]
        public bool IsDeleted { get; set; } = false;
    }
}
