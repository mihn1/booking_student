using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace B2S.Core.Entities
{
    public class UserProfile : BaseEntity
    {
        public UserProfile()
        {
            this.CreatedAt = DateTime.UtcNow;
        }

        [StringLength(38)]
        [Key]
        [Display(Name = "User Profile Id")]
        public string UserProfileId { get; set; }

        [StringLength(50)]
        [Display(Name = "User Profile Name")]
        [Required]
        public string UserProfileName { get; set; }

        [StringLength(250)]
        [Display(Name = "Description")]
        public string Description { get; set; }
    }
}
