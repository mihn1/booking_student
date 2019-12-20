using B2S.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace B2S.Core.Entities
{
    public class Booking : BaseEntity
    {

        public Booking()
        {
            this.CreatedAt = DateTime.UtcNow;
        }

        [StringLength(38)]
        [Key]
        [Display(Name = "Booking Id")]
        public string BookingId { get; set; }

        [Display(Name = "First Name")]
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [Display(Name = "Gender")]
        [Required]
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
        [EmailAddress]
        [Required]
        public string Email { get; set; }

        [Display(Name = "Status")]
        public BookingStatus Status { get; set; }

        [Display(Name = "Nationality")]
        [StringLength(30)]
        [Required]
        public string Nationality { get; set; }

        [StringLength(38)]
        [Display(Name = "Agent Id")]
        public string AgentId { get; set; }
        [Display(Name = "Agent")]
        public Agent Agent { get; set; }

        [StringLength(38)]
        [Display(Name = "Customer Id")]
        public string CustomerId { get; set; }
        [Display(Name = "Customer")]
        public Customer Customer { get; set; }

        [Display(Name = "Booking From")]
        public DateTime BookingFrom { get; set; }

        [Display(Name = "Booking To")]
        public DateTime BookingTo { get; set; }

        [StringLength(38)]
        [Display(Name = "Bed")]
        [Required]
        public string BedId { get; set; }
        [Display(Name = "Bed")]
        public Bed Bed { get; set; }

        [Editable(false)]
        [Display(Name = "Price")]
        public decimal Price { get; set; }
        [Editable(false)]
        [Display(Name = "Discount Amount")]
        public decimal DiscountAmount { get; set; }
        [Editable(false)]
        [Display(Name = "Deposit Amount")]
        public decimal DepositAmount { get; set; }

        [Display(Name = "Confirm OA")]
        [Required(ErrorMessage = "You need to confirm Occupancy Agreement.")]
        public bool IsConfirmOA { get; set; }

        [Display(Name = "Confirm T&C")]
        [Required(ErrorMessage = "You need to confirm Terms and Conditions.")]
        public bool IsConfirmTC { get; set; }
    }
}
