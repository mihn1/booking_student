using System.ComponentModel.DataAnnotations;

namespace B2S.Core.Enums
{
    public enum DamageStatus
    {
        [Display(Name = "New")]
        New = 1,
        [Display(Name = "In progress")]
        Inprogress = 2,    
        [Display(Name = "Closed")]
        Closed = 3
    }
}
