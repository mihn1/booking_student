using B2S.Core.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace B2S.Core.Entities
{
    public class PropertyDocument : BaseEntity
    {

        public PropertyDocument()
        {
            this.CreatedAt = DateTime.UtcNow;
        }

        [StringLength(38)]
        [Key]
        [Display(Name = "Document Id")]
        public string DocumentId { get; set; }

        [StringLength(250)]
        [Display(Name = "Document Name")]
        [Required]
        public string DocumentName { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [StringLength(38)]
        [Display(Name = "Property Id")]
        [Required]
        public string PropertyId { get; set; }
        [Display(Name = "Property")]
        public Property Property { get; set; }

        [Display(Name = "Category")]
        public DocumentCategory Category { get; set; }

        [StringLength(30)]
        [Display(Name = "File Type")]
        public string FileType { get; set; }

        [StringLength(250)]
        [Display(Name = "File Name")]
        public string FileName { get; set; }

        [Display(Name = "File Body")]
        public byte[] FileBody { get; set; }
    }
}
