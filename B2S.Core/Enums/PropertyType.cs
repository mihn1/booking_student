using System.ComponentModel.DataAnnotations;

namespace B2S.Core.Enums
{
    public enum PropertyType
    {
        [Display(Name = "House")]
        House = 1,
        [Display(Name = "Apartment")]
        Apartment = 2,
        [Display(Name = "Townhouse")]
        Townhouse = 3,
        [Display(Name = "Other")]
        Other = 4
    }
}
