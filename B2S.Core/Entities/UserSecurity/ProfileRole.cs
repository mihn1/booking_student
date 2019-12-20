using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace B2S.Core.Entities
{
    public class ProfileRole : BaseEntity
    {
        public ProfileRole()
        {
            this.CreatedAt = DateTime.UtcNow;
        }

        [StringLength(38)]
        [Key]
        [Display(Name = "User Profile Id")]
        public string UserProfileId { get; set; }
        public UserProfile UserProfile { get; set; }

        [StringLength(450)]
        [Key]
        [Display(Name = "Role Id")]
        public string RoleId { get; set; }
        public IdentityRole Role { get; set; }
    }
}
