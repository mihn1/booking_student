using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace B2S.Core.Enums
{
    public enum EventType
    {
        [Display(Name = "Check In")]
        CheckIn = 1,
        [Display(Name = "Check Out")]
        CheckOut = 2,
      
    }
}
