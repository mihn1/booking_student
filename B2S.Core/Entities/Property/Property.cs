using B2S.Core.Enums;
using B2S.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace B2S.Core.Entities
{
    public class Property : BaseEntity, IBaseAddress
    {
        public Property()
        {
            this.CreatedAt = DateTime.UtcNow;
        }

        [StringLength(38)]
        [Key]
        [Display(Name = "Property Id")]
        public string PropertyId { get; set; }

        [StringLength(250)]
        [Display(Name = "Property Name")]
        public string PropertyName { get; set; }

        [Display(Name = "Property Type")]
        public PropertyType PropertyType { get; set; }

        [StringLength(38)]
        [Display(Name = "Vendor")]
        [Required]
        public string VendorId { get; set; }
        [Display(Name = "Vendor")]
        public Vendor Vendor { get; set; }

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
