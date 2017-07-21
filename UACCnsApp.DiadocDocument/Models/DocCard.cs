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
		/// Ид шаблона согласования
		/// </summary>
		[Display(Name="Шаблон согласования")]
		[Required(ErrorMessage = "Не заполнено поле <strong>Шаблон согласования</strong>!")]
		public int DocTemplateId { get; set; }

		/// <summary>
		/// Ид типа документа
		/// </summary>
		[Display(Name = "Тип документа")]
		[Required(ErrorMessage = "Не заполнено поле <strong>Тип документа</strong>!")]
		public int DocumentTypeId { get; set; }

		/// <summary>
		/// Ид вида основного документв
		/// </summary>
		[Display(Name = "Вид основного документа")]
        public int? MetaObjectId { get; set; }

		/// <summary>
		/// Ид основного документа
		/// </summary>
		[Display(Name = "Ид основного документа")]
        public int? ObjectId { get; set; }

		/// <summary>
		/// Номер документа
		/// </summary>
		[Display(Name = "Номер документа")]
        public string DocNumber { get; set; }

		/// <summary>
		/// Дата документа
		/// </summary>
		[Required(ErrorMessage = "Не заполнено поле <strong>Дата документа</strong>!")]
		[Display(Name = "Дата документа")]
        public DateTime DocDate { get; set; }

		/// <summary>
		/// Штрихкод
		/// </summary>
        [StringLength(50)]
		[Display(Name = "Штрихкод")]
		public string BarCode { get; set; }

		/// <summary>
		/// Комментарий
		/// </summary>
		[Display(Name = "Комментарий")]
		public string Comment { get; set; }

		/// <summary>
		/// Ид Валюты
		/// </summary>
		[Display(Name = "Валюта")]
        public int? CurrencyId { get; set; }

		/// <summary>
		/// Сумма документа
		/// </summary>
        [Column(TypeName = "numeric")]
		[Display(Name = "Сумма документа")]
        public decimal? Summa { get; set; }

		/// <summary>
		/// Юрлицо
		/// </summary>
		[Display(Name = "Юрлицо")]
		public int OwnerId { get; set; }

		/// <summary>
		/// Контрагент
		/// </summary>
		[Display(Name = "Контрагент")]
		public int? ContractorId { get; set; }

		/// <summary>
		/// Утверждающий
		/// </summary>
		[Display(Name = "Утверждающий")]
		public int? ApprovalUserId { get; set; }

		/// <summary>
		/// Инициатор
		/// </summary>
		[Display(Name = "Инициатор")]
		public int StarterUserId { get; set; }

		/// <summary>
		/// Департамент
		/// </summary>
		[Display(Name = "Департамент")]
        public int? DepartmentId { get; set; }

		/// <summary>
		/// Проект
		/// </summary>
		[Display(Name = "Проект")]
        public int? ProjectId { get; set; }

		/// <summary>
		/// ЦФО
		/// </summary>
		[Display(Name = "ЦФО")]
        public int? CenterFinancesResponsId { get; set; }

		/// <summary>
		/// Пакетный документ
		/// </summary>
		[Display(Name = "Пакетный документ")]
        public bool IsPacket { get; set; }

		/// <summary>
		/// Статус документа
		/// </summary>
		[Display(Name = "Статус документа")]
        public int DocStatusId { get; set; }

		/// <summary>
		/// Ответственный
		/// </summary>
		[Display(Name = "Ответственный")]
		[Required(ErrorMessage = "Не заполнено поле <strong>Ответственный</strong>!")]
		public int ResponsibleUserId { get; set; }

		/// <summary>
		/// Оповестить инициатора после полного окончания подписания
		/// </summary>
		[Display(Name = "Оповестить инициатора")]
		public bool NotifyStarter { get; set; }

		/// <summary>
		/// Оповестить ответственного после полного окончания подписания
		/// </summary>
		[Display(Name = "Оповестить ответственного")]
		public bool NotifyResponsible { get; set; }

		/// <summary>
		/// Подписание полностью завершено.
		/// Все этапы пройдены
		/// </summary>
		[Display(Name = "Подписание завершено")]
		public bool SigningCompleted { get; set; }

		/// <summary>
		/// Входящий номер
		/// </summary>
		public string IncomingNumber { get; set; }

		/// <summary>
		/// Входящая дата
		/// </summary>
		public DateTime? IncomingDate { get; set; }

		/// <summary>
		/// Начало
		/// </summary>
		public DateTime? DateBegin { get; set; }

		/// <summary>
		/// Окончание
		/// </summary>
		public DateTime? DateEnd { get; set; }

		/// <summary>
		/// Количество
		/// </summary>
		public int? Count { get; set; }

		/// <summary>
		/// Контактное лицо контрагента
		/// </summary>
		public int? ContactPersonId { get; set; }

		/// <summary>
		/// Полный текст документа 
		/// </summary>
		public string Text { get; set; }

		/// <summary>
		///Дополнительный текст документа 
		/// </summary>
		public string AddText { get; set; }

		/// <summary>
		/// Это электронный документ?
		/// </summary>
		public bool IsDigitalDoc { get; set; }

		/// <summary>
		/// Оператор
		/// </summary>
		public int? OperatorUserId { get; set; }

		/// <summary>
		/// Договор
		/// </summary>
		public int? AgreementId { get; set; }


		public int? ParentDocCardId { get; set; }

		/// <summary>
		/// Дата завершения согласования документа
		/// </summary>
		public DateTime? SigningCompletedDate { get; set; }

		/// <summary>
		/// Дата последнего изменения статуса 
		/// </summary>
		public DateTime? ChangeDocStatusDate { get; set; }

        /// <summary>
        /// Mаксимальная дата резолюции 
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime? LastSignDate { get; set; }

        /// <summary>
        /// ИД в 1с для интеграции
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
		/// Тип документа
		/// </summary>
		
		public virtual DocumentType DocumentType { get; set; }

		/// <summary>
		/// Вид документа
		/// </summary>
		
		public virtual MetaObject MetaObject { get; set; }


		/// <summary>
		/// Юрлицо
		/// </summary>
		
		public virtual Contractor Owner { get; set; }

		/// <summary>
		/// Контрагент
		/// </summary>
		
		public virtual Contractor Contractor { get; set; }

		/// <summary>
		/// Утверждающий
		/// </summary>
		
		public virtual MetaUser ApprovalUser { get; set; }

		/// <summary>
		/// Инициатор
		/// </summary>
		
		public virtual MetaUser StarterUser { get; set; }

		/// <summary>
		/// Ответственный
		/// </summary>
		
		public virtual MetaUser ResponsibleUser { get; set; }

		/// <summary>
		/// Департамент
		/// </summary>
		
		public virtual Department Department { get; set; }

		/// <summary>
		/// Контактное лицо
		/// </summary>
		
		public virtual ContactPerson ContactPerson { get; set; }
		
		/// <summary>
		/// Статус документа
		/// </summary>
		
		public virtual MetaObject DocStatus { get; set; }

		/// <summary>
		/// Оператор
		/// </summary>
		
		public virtual MetaUser OperatorUser { get; set; }

		/// <summary>
		/// Главный документ в пакете
		/// </summary>
		
		public virtual DocCard ParentDocCard { get; set; }

		/// <summary>
		/// return DocNumber + " от " + DocDate.ToShortDateString();
		/// </summary>
		
		public string RegDocum
		{
			get
			{
				return DocNumber + " от " + DocDate.ToShortDateString();
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
