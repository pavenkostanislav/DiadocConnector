namespace UACCnsApp.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("diadoc.Entity")]
    [Serializable]
    public partial class Entity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ContractorId { get; set; }

        [Required]
        [StringLength(50)]
        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        [Required]
        [StringLength(50)]
        public string LastUpdatedBy { get; set; }

        public DateTime LastUpdatedDate { get; set; }


        public int? EntityType { get; set; }

        public Guid EntityId { get; set; }

        public Guid? ParentEntityId { get; set; }

        public string Content { get; set; }

        public int? AttachmentType { get; set; }

        public string FileName { get; set; }

        public bool? NeedRecipientSignature { get; set; }

        public Guid? SignerBoxId { get; set; }

        public Guid? NotDeliveredEventId { get; set; }

        public string DocumentInfo { get; set; }

        public DateTime? RawCreationDate { get; set; }

        public string ResolutionInfo { get; set; }

        public Guid? SignerDepartmentId { get; set; }

        public string ResolutionRequestInfo { get; set; }

        public string ResolutionRequestDenialInfo { get; set; }

        public bool? IsApprovementSignature { get; set; }

        public bool? IsEncryptedContent { get; set; }

        public string AttachmentVersion { get; set; }

        public string ResolutionRouteAssignmentInfo { get; set; }

        public string ResolutionRouteRemovalInfo { get; set; }
    }
}
