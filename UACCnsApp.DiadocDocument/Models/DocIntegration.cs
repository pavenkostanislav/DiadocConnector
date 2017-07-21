namespace UACCnsApp.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("DocIntegration")]
    [Serializable]
    public partial class DocIntegration
    {
        [ScaffoldColumn(false), Key]
        public int Id { get; set; }

        [Display(Name = "��� ���������")]
        public int DocumentTypeId { get; set; }

        [Display(Name = "������� �����������")]
        public int OwnerId { get; set; }

        [Display(Name = "������ �������� ������������")]
        public int? DocTemplateId { get; set; }

        [Display(Name = "�������� �����������")]
        public int? ContractorId { get; set; }

        [Required(ErrorMessage = "�� ��������� ���� <strong>����� �������� ������� ��� ������� ���������� ������������</strong>!")]
        [Display(Name = "����� �������� ������� ��� ������� ���������� ������������")]
        public string DocNumber { get; set; }

        [Display(Name = "���� �������� ������� ��� ������� ���������� ������������")]
        public DateTime DocDate { get; set; }

        [Display(Name = "�����-���")]
        [StringLength(50)]
        public string BarCode { get; set; }

        [Display(Name = "�����")]
        [Column(TypeName = "numeric")]
        public decimal? Summa { get; set; }

        [Display(Name = "������")]
        public int? CurrencyId { get; set; }

        [Required(ErrorMessage = "�� ��������� ���� <strong>������������� (Email)</strong>!")]
        [Display(Name = "������������� (Email)")]
        [StringLength(250)]
        public string ResponsibleUser { get; set; }

        [Display(Name = "����������� ��������������")]
        public int? DepartmentId { get; set; }

        [Display(Name = "������")]
        public int? ProjectId { get; set; }

        [Display(Name = "ID CenterFinancesRespons")]
        public int? CenterFinancesResponsId { get; set; }

        [Display(Name = "������� (Email)")]
        public string Experts { get; set; }

        [Display(Name = "��� �����")]
        [StringLength(255)]
        public string FileName { get; set; }

        [Display(Name = "FileData")]
        public byte[] FileData { get; set; }

        [StringLength(50)]
        public string DocStatusName { get; set; }

        public DateTime? DocStatusDate { get; set; }

        [Display(Name = "����� � ��������� ���������")]
        public int? DocCardId { get; set; }

        [Display(Name = "������� ���������� (2)")]
        public int Action { get; set; }

        [Display(Name = "�����")]
        public string Response { get; set; }

        [Display(Name = "����������")]
        public string Comment { get; set; }

        [Display(Name = "�������")]
        public int? MetaObjectId { get; set; }

        [Display(Name = "��������")]
        public int? ObjectId { get; set; }

        [Display(Name = "������������ ������������")]
        public int? ApprovalUserId { get; set; }

        [Display(Name = "��������� ����������")]
        public int? StarterUserId { get; set; }

        [Display(Name = "�������������")]
        public int? ResponsibleUserId { get; set; }

        [Display(Name = "������� (Email)")]
        public string ExpertId { get; set; }

        //[Display(Name = "������")]
        //public string Error { get; set; }

        [StringLength(50), ScaffoldColumn(false)]
        public string CreatedBy { get; set; }

        [ScaffoldColumn(false)]
        public DateTime? CreatedDate { get; set; }

		public bool? IsDigitalDoc { get; set; }

		public int? OperatorUserId { get; set; }

		public int? AgreementId { get; set; }

		public bool? NotifyResponsible { get; set; }

		public string RefBarCodes { get; set; }
    }
}
