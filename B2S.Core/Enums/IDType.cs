using System.ComponentModel.DataAnnotations;

namespace B2S.Core.Enums
{
    public enum IDType
    {
        [Display(Name = "Driver's License")]
        DriverLicense = 1,
        [Display(Name = "Student Card")]
        StudentCard = 2,
        [Display(Name = "Passport")]
        Passport = 3,
        [Display(Name = "Other")]
        Other = 4
    }
}
