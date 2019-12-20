using System.ComponentModel.DataAnnotations;

namespace B2S.Core.Interfaces
{
    public interface IBaseAddress
    {
        [Display(Name = "Address")]
        [Required]
        [StringLength(250)]
        string Address { get; set; }

        [Display(Name = "City")]
        [StringLength(30)]
        string City { get; set; }

        [Display(Name = "Province")]
        [StringLength(30)]
        string Province { get; set; }

        [Display(Name = "Postcode")]
        [StringLength(10)]
        string Postcode { get; set; }

        [Display(Name = "Country")]
        [StringLength(30)]
        string Country { get; set; }
    }
}
