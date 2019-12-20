using System.ComponentModel.DataAnnotations;

namespace B2S.Core.Enums
{
    public enum DamageType
    {
        [Display(Name = "Minor")]
        Minor = 1,
        [Display(Name = "Major")]
        Major = 2,
        [Display(Name = "Destroyed")]
        Destroyed = 3
    }
}
