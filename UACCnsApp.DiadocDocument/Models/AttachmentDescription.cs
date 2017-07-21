namespace UACCnsApp.Models
{
	using System;
	using CISLibApp.Models;
	using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    [Table("AttachmentDescription")]
	[Serializable]
    public partial class AttachmentDescription
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        public int DocumentTypeId { get; set; }

        public Guid AttachmentId { get; set; }

		public string ContentType { get; set; }

        [StringLength(50)]
        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

		public int Version { get; set; }

		[StringLength(255)]
		public string FileName { get; set; }

		public int FileSize { get; set; }

		public int FirstVersionId { get; set; }

		public bool IsPrivate { get; set; }

		public bool IsClosed { get; set; }
		
		[StringLength(50)]
        public string LastUpdatedBy { get; set; }

        public DateTime LastUpdatedDate { get; set; }

		public virtual DocumentType DocumentType { get; set; }

		[ForeignKey("AttachmentId")]
		public virtual AttachmentsView AttachmentsView { get; set; }

    }
}
