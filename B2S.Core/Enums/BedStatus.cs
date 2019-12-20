using System.ComponentModel.DataAnnotations;

namespace B2S.Core.Enums
{
    public enum BedStatus
    {
        [Display(Name = "Ready")]
        Ready = 1,
        [Display(Name = "Cleanup")]
        Cleanup = 2
    }
}
