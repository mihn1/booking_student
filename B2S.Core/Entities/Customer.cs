using B2S.Core.Enums;
using B2S.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace B2S.Core.Entities
{
    public class Customer : BaseEntity, IBaseAddress
    {
        public Customer()
        {
            this.CreatedAt = DateTime.UtcNow;
        }

        [StringLength(38)]
        [Key]
        [Display(Name = "Customer Id")]
        public string CustomerId { get; set; }

        [Display(Name = "First Name")]
        [Required]
        [StringLength(20)]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [Required]
        [StringLength(20)]
        public string LastName { get; set; }

        [Display(Name = "Gender")]
        public Gender Gender { get; set; }

        [Display(Name = "ID Type")]
        public IDType IDType { get; set; }

        [Display(Name = "ID Number")]
        [StringLength(50)]
        public string IDNumber { get; set; }

        [Display(Name = "Phone")]
        [StringLength(20)]
        public string Phone { get; set; }

        [Display(Name = "Email")]
        [StringLength(50)]
        public string Email { get; set; }

        [Display(Name = "Address")]
        [StringLength(250)]
        public string Address { get; set; }

        [Display(Name = "City")]
        [StringLength(30)]
        public string City { get; set; }

        [Display(Name = "Province")]
        [StringLength(30)]
        public string Province { get; set; }

        [Display(Name = "Postcode")]
        [StringLength(10)]
        public string Postcode { get; set; }

        [Display(Name = "Country")]
        [StringLength(30)]
        public string Country { get; set; }
    }
}
