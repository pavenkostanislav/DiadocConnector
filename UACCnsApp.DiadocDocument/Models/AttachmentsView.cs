namespace UACCnsApp.Models
{
	using System;
	using System.ComponentModel.DataAnnotations;
	using System.ComponentModel.DataAnnotations.Schema;
    
    [Table("AttachmentsView")]
	[Serializable]
    public partial class AttachmentsView
    {
        [Key]
		public Guid Id { get; set; }

        [StringLength(255)]
        public string FileName { get; set; }

        public byte[] FileStream { get; set; }

    }
}
