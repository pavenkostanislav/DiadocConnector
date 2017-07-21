namespace UACCnsApp.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("diadoc.BoxEvent")]
    [Serializable]
    public partial class BoxEvent
    {
        [Key]
        public int Id { get; set; }
        

        [Required]
        [StringLength(50)]
        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        [Required]
        [StringLength(50)]
        public string LastUpdatedBy { get; set; }

        public DateTime LastUpdatedDate { get; set; }
        
        public Guid EventId { get; set; }

        public Guid? MessageId { get; set; }

        public Guid? MessagePatchId { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
