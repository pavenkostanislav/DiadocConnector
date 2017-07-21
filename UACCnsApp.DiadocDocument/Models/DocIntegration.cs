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

        [Display(Name = "Тип документа")]
        public int DocumentTypeId { get; set; }

        [Display(Name = "текущая организация")]
        public int OwnerId { get; set; }

        [Display(Name = "Шаблон маршрута согласования")]
        public int? DocTemplateId { get; set; }

        [Display(Name = "Стороняя организация")]
        public int? ContractorId { get; set; }

        [Required(ErrorMessage = "Не заполнено поле <strong>Номер карточки реестра для которой необходимо согласование</strong>!")]
        [Display(Name = "Номер карточки реестра для которой необходимо согласование")]
        public string DocNumber { get; set; }

        [Display(Name = "Дата карточки реестра для которой необходимо согласование")]
        public DateTime DocDate { get; set; }

        [Display(Name = "Штрих-код")]
        [StringLength(50)]
        public string BarCode { get; set; }

        [Display(Name = "Сумма")]
        [Column(TypeName = "numeric")]
        public decimal? Summa { get; set; }

        [Display(Name = "Валюта")]
        public int? CurrencyId { get; set; }

        [Required(ErrorMessage = "Не заполнено поле <strong>Ответственный (Email)</strong>!")]
        [Display(Name = "Ответственный (Email)")]
        [StringLength(250)]
        public string ResponsibleUser { get; set; }

        [Display(Name = "Департамент ответственного")]
        public int? DepartmentId { get; set; }

        [Display(Name = "Проект")]
        public int? ProjectId { get; set; }

        [Display(Name = "ID CenterFinancesRespons")]
        public int? CenterFinancesResponsId { get; set; }

        [Display(Name = "Эксперт (Email)")]
        public string Experts { get; set; }

        [Display(Name = "Имя файла")]
        [StringLength(255)]
        public string FileName { get; set; }

        [Display(Name = "FileData")]
        public byte[] FileData { get; set; }

        [StringLength(50)]
        public string DocStatusName { get; set; }

        public DateTime? DocStatusDate { get; set; }

        [Display(Name = "Номер с карточкой документа")]
        public int? DocCardId { get; set; }

        [Display(Name = "Вариант активности (2)")]
        public int Action { get; set; }

        [Display(Name = "Ответ")]
        public string Response { get; set; }

        [Display(Name = "Коментарий")]
        public string Comment { get; set; }

        [Display(Name = "Таблица")]
        public int? MetaObjectId { get; set; }

        [Display(Name = "Карточка")]
        public int? ObjectId { get; set; }

        [Display(Name = "Утверждающий согласование")]
        public int? ApprovalUserId { get; set; }

        [Display(Name = "Инициатор подписания")]
        public int? StarterUserId { get; set; }

        [Display(Name = "Ответственный")]
        public int? ResponsibleUserId { get; set; }

        [Display(Name = "Эксперт (Email)")]
        public string ExpertId { get; set; }

        //[Display(Name = "Ошибка")]
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
