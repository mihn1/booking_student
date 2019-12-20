using B2S.Core.Enums;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace B2S.Core.Entities
{
    // Add profile data for application users by adding properties to the User class
    public partial class User : IdentityUser
    {
        [Display(Name = "First Name")]
        [Required]
        [StringLength(10)]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [Required]
        [StringLength(10)]
        public string LastName { get; set; }
        public string ProfilePictureUrl { get; set; } = "/images/userprofile/empty_profile.png";
        public bool IsSuperAdmin { get; set; } = false;

        [Display(Name = "User Type")]
        public UserType UserType { get; set; }

        [StringLength(38)]
        [Display(Name = "User Profile Id")]
        public string UserProfileId { get; set; }
        public UserProfile UserProfile { get; set; }
    }
}
