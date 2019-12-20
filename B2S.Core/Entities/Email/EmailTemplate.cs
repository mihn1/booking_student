using System;
using System.ComponentModel.DataAnnotations;

namespace B2S.Core.Entities
{
    public class EmailTemplate : BaseEntity
    {
        public EmailTemplate()
        {
            this.CreatedAt = DateTime.UtcNow;
        }

        [StringLength(38)]
        [Display(Name = "Template Id")]
        public string EmailTemplateId { get; set; }

        [StringLength(250)]
        [Display(Name = "Name")]
        [Required]
        public string TemplatName { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Subject")]
        [StringLength(250)]
        public string Subject { get; set; }

        [Display(Name = "Published")]
        public bool IsPublished { get; set; }

        [Display(Name = "Body Text")]
        public string BodyText { get; set; }

        [Display(Name = "Body Text")]
        public string BodyHTML { get; set; }
    }
}
