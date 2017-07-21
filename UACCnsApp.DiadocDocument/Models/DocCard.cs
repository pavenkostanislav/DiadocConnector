namespace UACCnsApp.Models
{
	using System;
	using CISLibApp.Models;
	using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
	using CISLibApp.Interfaces;

    [Table("DocCard")]
	[Serializable]
	public partial class DocCard : IDisplayName
    {
		[Key]
		[ScaffoldColumn(false)]
        public int Id { get; set; }

		/// <summary>
		/// �� ������� ������������
		/// </summary>
		[Display(Name="������ ������������")]
		[Required(ErrorMessage = "�� ��������� ���� <strong>������ ������������</strong>!")]
		public int DocTemplateId { get; set; }

		/// <summary>
		/// �� ���� ���������
		/// </summary>
		[Display(Name = "��� ���������")]
		[Required(ErrorMessage = "�� ��������� ���� <strong>��� ���������</strong>!")]
		public int DocumentTypeId { get; set; }

		/// <summary>
		/// �� ���� ��������� ���������
		/// </summary>
		[Display(Name = "��� ��������� ���������")]
        public int? MetaObjectId { get; set; }

		/// <summary>
		/// �� ��������� ���������
		/// </summary>
		[Display(Name = "�� ��������� ���������")]
        public int? ObjectId { get; set; }

		/// <summary>
		/// ����� ���������
		/// </summary>
		[Display(Name = "����� ���������")]
        public string DocNumber { get; set; }

		/// <summary>
		/// ���� ���������
		/// </summary>
		[Required(ErrorMessage = "�� ��������� ���� <strong>���� ���������</strong>!")]
		[Display(Name = "���� ���������")]
        public DateTime DocDate { get; set; }

		/// <summary>
		/// ��������
		/// </summary>
        [StringLength(50)]
		[Display(Name = "��������")]
		public string BarCode { get; set; }

		/// <summary>
		/// �����������
		/// </summary>
		[Display(Name = "�����������")]
		public string Comment { get; set; }

		/// <summary>
		/// �� ������
		/// </summary>
		[Display(Name = "������")]
        public int? CurrencyId { get; set; }

		/// <summary>
		/// ����� ���������
		/// </summary>
        [Column(TypeName = "numeric")]
		[Display(Name = "����� ���������")]
        public decimal? Summa { get; set; }

		/// <summary>
		/// ������
		/// </summary>
		[Display(Name = "������")]
		public int OwnerId { get; set; }

		/// <summary>
		/// ����������
		/// </summary>
		[Display(Name = "����������")]
		public int? ContractorId { get; set; }

		/// <summary>
		/// ������������
		/// </summary>
		[Display(Name = "������������")]
		public int? ApprovalUserId { get; set; }

		/// <summary>
		/// ���������
		/// </summary>
		[Display(Name = "���������")]
		public int StarterUserId { get; set; }

		/// <summary>
		/// �����������
		/// </summary>
		[Display(Name = "�����������")]
        public int? DepartmentId { get; set; }

		/// <summary>
		/// ������
		/// </summary>
		[Display(Name = "������")]
        public int? ProjectId { get; set; }

		/// <summary>
		/// ���
		/// </summary>
		[Display(Name = "���")]
        public int? CenterFinancesResponsId { get; set; }

		/// <summary>
		/// �������� ��������
		/// </summary>
		[Display(Name = "�������� ��������")]
        public bool IsPacket { get; set; }

		/// <summary>
		/// ������ ���������
		/// </summary>
		[Display(Name = "������ ���������")]
        public int DocStatusId { get; set; }

		/// <summary>
		/// �������������
		/// </summary>
		[Display(Name = "�������������")]
		[Required(ErrorMessage = "�� ��������� ���� <strong>�������������</strong>!")]
		public int ResponsibleUserId { get; set; }

		/// <summary>
		/// ���������� ���������� ����� ������� ��������� ����������
		/// </summary>
		[Display(Name = "���������� ����������")]
		public bool NotifyStarter { get; set; }

		/// <summary>
		/// ���������� �������������� ����� ������� ��������� ����������
		/// </summary>
		[Display(Name = "���������� ��������������")]
		public bool NotifyResponsible { get; set; }

		/// <summary>
		/// ���������� ��������� ���������.
		/// ��� ����� ��������
		/// </summary>
		[Display(Name = "���������� ���������")]
		public bool SigningCompleted { get; set; }

		/// <summary>
		/// �������� �����
		/// </summary>
		public string IncomingNumber { get; set; }

		/// <summary>
		/// �������� ����
		/// </summary>
		public DateTime? IncomingDate { get; set; }

		/// <summary>
		/// ������
		/// </summary>
		public DateTime? DateBegin { get; set; }

		/// <summary>
		/// ���������
		/// </summary>
		public DateTime? DateEnd { get; set; }

		/// <summary>
		/// ����������
		/// </summary>
		public int? Count { get; set; }

		/// <summary>
		/// ���������� ���� �����������
		/// </summary>
		public int? ContactPersonId { get; set; }

		/// <summary>
		/// ������ ����� ��������� 
		/// </summary>
		public string Text { get; set; }

		/// <summary>
		///�������������� ����� ��������� 
		/// </summary>
		public string AddText { get; set; }

		/// <summary>
		/// ��� ����������� ��������?
		/// </summary>
		public bool IsDigitalDoc { get; set; }

		/// <summary>
		/// ��������
		/// </summary>
		public int? OperatorUserId { get; set; }

		/// <summary>
		/// �������
		/// </summary>
		public int? AgreementId { get; set; }


		public int? ParentDocCardId { get; set; }

		/// <summary>
		/// ���� ���������� ������������ ���������
		/// </summary>
		public DateTime? SigningCompletedDate { get; set; }

		/// <summary>
		/// ���� ���������� ��������� ������� 
		/// </summary>
		public DateTime? ChangeDocStatusDate { get; set; }

        /// <summary>
        /// M����������� ���� ��������� 
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime? LastSignDate { get; set; }

        /// <summary>
        /// �� � 1� ��� ����������
        /// </summary>
        [ScaffoldColumn(false)]
		public byte[] Id1Cbuh { get; set; }

		[ScaffoldColumn(false)]
		public string CreatedBy { get; set; }

		[ScaffoldColumn(false)]
		public DateTime CreatedDate { get; set; }

		[ScaffoldColumn(false)]
		public string LastUpdatedBy { get; set; }

		[ScaffoldColumn(false)]
		public DateTime LastUpdatedDate { get; set; }


		/// <summary>
		/// ��� ���������
		/// </summary>
		
		public virtual DocumentType DocumentType { get; set; }

		/// <summary>
		/// ��� ���������
		/// </summary>
		
		public virtual MetaObject MetaObject { get; set; }


		/// <summary>
		/// ������
		/// </summary>
		
		public virtual Contractor Owner { get; set; }

		/// <summary>
		/// ����������
		/// </summary>
		
		public virtual Contractor Contractor { get; set; }

		/// <summary>
		/// ������������
		/// </summary>
		
		public virtual MetaUser ApprovalUser { get; set; }

		/// <summary>
		/// ���������
		/// </summary>
		
		public virtual MetaUser StarterUser { get; set; }

		/// <summary>
		/// �������������
		/// </summary>
		
		public virtual MetaUser ResponsibleUser { get; set; }

		/// <summary>
		/// �����������
		/// </summary>
		
		public virtual Department Department { get; set; }

		/// <summary>
		/// ���������� ����
		/// </summary>
		
		public virtual ContactPerson ContactPerson { get; set; }
		
		/// <summary>
		/// ������ ���������
		/// </summary>
		
		public virtual MetaObject DocStatus { get; set; }

		/// <summary>
		/// ��������
		/// </summary>
		
		public virtual MetaUser OperatorUser { get; set; }

		/// <summary>
		/// ������� �������� � ������
		/// </summary>
		
		public virtual DocCard ParentDocCard { get; set; }

		/// <summary>
		/// return DocNumber + " �� " + DocDate.ToShortDateString();
		/// </summary>
		
		public string RegDocum
		{
			get
			{
				return DocNumber + " �� " + DocDate.ToShortDateString();
			}
		}

		/// <summary>
		/// return RegDocum;
		/// </summary>
		
		public string DisplayName
		{
			get { return RegDocum; }
		}
	}
}
