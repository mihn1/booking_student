using B2S.Core.Enums;
using B2S.Core.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;

namespace B2S.Core.Entities
{
    public class UserAccount : BaseEntity
    {
        public UserAccount()
        {
            this.CreatedAt = DateTime.UtcNow;
        }

        [StringLength(38)]
        [Key]
        [Display(Name = "UserId")]
        public string UserId { get; set; }
        
        [StringLength(38)]
        [Key]
        [Display(Name = "AccountId")]
        public string AccountId { get; set; }
        

    }
}
