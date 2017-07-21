namespace UACCnsApp.Models
{
	using System;
	using CISLibApp.Models;
	using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    [Table("AttachmentReference")]
	[Serializable]
    public partial class AttachmentReference
    {
        public int Id { get; set; }

        public int AttachmentDescriptionId { get; set; }

        public int MetaObjectId { get; set; }

        public int? ObjectId { get; set; }

        public int OrderNumber { get; set; }

        [Required]
        [StringLength(50)]
        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        [Required]
        [StringLength(50)]
        public string LastUpdatedBy { get; set; }

        public DateTime LastUpdatedDate { get; set; }

		public virtual AttachmentDescription AttachmentDescription { get; set; }
		public virtual MetaObject MetaObject { get; set; }
    }
}
