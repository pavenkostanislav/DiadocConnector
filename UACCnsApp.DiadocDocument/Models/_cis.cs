using System;
using System.Collections.Generic;
using System.Data;
using CISLibApp.Models;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using CISLibApp.Basic.Common;
using CISLibApp.Basic.Tools;
using CISLibApp.Common;
using Diadoc.Api;
using Diadoc.Api.Cryptography;
using Diadoc.Api.Proto;
using Diadoc.Api.Proto.Documents;
using Diadoc.Api.Proto.Events;
using Newtonsoft.Json;
using System.Reflection;

namespace UACCnsApp.Models
{

    public partial class Cis : DbContext
    {
        public Cis() : base("name=cis")
        {
#if DEBUG
            var methodBase = System.Reflection.MethodBase.GetCurrentMethod();
            Console.WriteLine("{0} {1} is calling at {2}", methodBase.MemberType, methodBase.DeclaringType.Name, DateTime.Now.ToString("dd.MM.yy HH:mm:ss"));
#endif
        }

        #region DbContext

        public virtual DbSet<Entity> Entities { get; set; }

        public virtual DbSet<BoxEvent> BoxEvents { get; set; }

        public virtual DbSet<CISLibApp.Models.Document> Documents { get; set; }

        public virtual DbSet<CISLibApp.Models.DocumentInitial> DocumentInitials { get; set; }

        public virtual DbSet<CISLibApp.Models.DocumentSubordinate> DocumentSubordinates { get; set; }

        public virtual DbSet<ExtIntegration> ExtIntegrations { get; set; }

        public virtual DbSet<ExtSystem> ExtSystems { get; set; }

        public virtual DbSet<DocCard> DocCards { get; set; }

        public virtual DbSet<DocIntegration> DocIntegrations { get; set; }

        public virtual DbSet<CISLibApp.Models.DocumentType> DocumentTypes { get; set; }

        public virtual DbSet<Contractor> Contractors { get; set; }

        public virtual DbSet<MetaUser> MetaUsers { get; set; }

        public virtual DbSet<AttachmentDescription> AttachmentDescriptions { get; set; }

        public virtual DbSet<AttachmentReference> AttachmentReferences { get; set; }

        public virtual DbSet<AttachmentsView> AttachmentsViews { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
#if DEBUG
                var methodBase = System.Reflection.MethodBase.GetCurrentMethod();
                Console.WriteLine("{0} {1}.{2} have been started executing at {3}", methodBase.MemberType, methodBase.DeclaringType.Name, methodBase.Name, DateTime.Now.ToString("dd.MM.yy HH:mm:ss"));
#endif
            // Инициализация базового блока
            CISLibApp.Basic.Models.BasicDbContext.ModelCreating(modelBuilder);
        }

        /// <summary>
        /// Перегружаем сохранение чтобы небыло ошибок на попытку сохранить виртуальные функции с подписанными к ним ключами
        /// </summary>
        /// <returns></returns>
        public override int SaveChanges()
        {
            var methodBase = System.Reflection.MethodBase.GetCurrentMethod();
            Console.WriteLine("{0} {1} is calling at {2}", methodBase.MemberType, methodBase.DeclaringType.Name, DateTime.Now.ToString("dd.MM.yy HH:mm:ss"));

            var userDiadoc = this.MetaUsers.FirstOrDefault(m => m.Name == "Diadoc");
            foreach (var item in base.ChangeTracker.Entries())
            {
                switch (item.State)
                {
                    case EntityState.Added:
                        if (item.CurrentValues.PropertyNames.Contains("CreatedBy"))
                        {
                            item.CurrentValues["CreatedBy"] = userDiadoc.Login;
                        }

                        if (item.CurrentValues.PropertyNames.Contains("CreatedDate"))
                        {
                            item.CurrentValues["CreatedDate"] = DateTime.Now;
                        }
                        if (item.CurrentValues.PropertyNames.Contains("LastUpdatedBy"))
                        {
                            item.CurrentValues["LastUpdatedBy"] = userDiadoc.Login;
                        }

                        if (item.CurrentValues.PropertyNames.Contains("LastUpdatedDate"))
                        {
                            item.CurrentValues["LastUpdatedDate"] = DateTime.Now;
                        }
                        break;
                    case EntityState.Modified:
                        if (item.CurrentValues.PropertyNames.Contains("LastUpdatedBy"))
                        {
                            item.CurrentValues["LastUpdatedBy"] = userDiadoc.Login;
                        }

                        if (item.CurrentValues.PropertyNames.Contains("LastUpdatedDate"))
                        {
                            item.CurrentValues["LastUpdatedDate"] = DateTime.Now;
                        }

                        break;
                }
            }
            return base.SaveChanges();
        }
        #endregion

        //#region Utilities

        #region ExtIntegration
        /// <summary>
        /// Получение ИД типа документа
        /// </summary>
        public ExtIntegration GetDiadocDocType(int documentDirection, int documentType)
        {
            var externalValue = documentDirection + "." + documentType;
            return this.ExtIntegrations.FirstOrDefault(
                                    m =>
                                        m.MetaObjectId == (int)CISLibApp.Common.Constant.Table.DocumentType &&
                                        m.ExtSystemId == (int)CISLibApp.Common.Constant.ExtSystem.Diadoc_DocumentTypeDotDocumentClass &&
                                        m.ExternalId == externalValue);
        }

        /// <summary>
        /// Получение ИД ящика из диадока по компании из КИС
        /// </summary>
        public string GetDiadocBoxId(int contractorId)
        {
            var box = this.ExtIntegrations.FirstOrDefault(
                        m => m.ExtSystemId == (int)Constant.ExtSystem.Diadoc_Box &&
                                m.MetaObjectId == (int)Constant.Table.Contractor &&
                                m.ObjectId == contractorId);
            if (box == null)
            {
                throw new Exception(string.Format("Не найден контрагент в системе ИД: {0}!", contractorId));
            }
            return box.ExternalId;
        }

        /// <summary>
        /// Получение ИД компании из КИС по ИД ящика из Диадока
        /// </summary>
        public int? GetCortractorId(string counteragentBoxId)
        {
            var contractor = this.ExtIntegrations.FirstOrDefault(
                                    ExtInt =>
                                        ExtInt.MetaObjectId == (int)Constant.Table.Contractor &&
                                        ExtInt.ExtSystemId == (int)Constant.ExtSystem.Diadoc_Box &&
                                        ExtInt.ExternalId == counteragentBoxId);
            if (contractor == null)
            {
                throw new Exception(string.Format("Не найдена организация с ящиком {0}!", counteragentBoxId));
            }
            return contractor.ObjectId;
        }

        public string GetDiadocFileName(CISLibApp.Models.Document item)
        {
            var docType = this.GetDiadocDocType((int)item.DocumentDirection, item.DocumentType);

            string fileName = item.FileName.Split('.')[0];

            if (!string.IsNullOrEmpty(item.DocumentNumber) && docType != null)
            {
                fileName = docType.Description + " №" + item.DocumentNumber;
                if (!string.IsNullOrEmpty(item.DocumentDate))
                {
                    fileName += " от " + item.DocumentDate;
                }
            }

            return System.IO.Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c, '_'));
        }
        #endregion

        //public void NewMetaDataComparison(Diadoc.Api.Proto.Documents.Document source, CISLibApp.Models.Document target)
        //{
        //    var сontractorId = this.GetCortractorId(source.CounteragentBoxId);

        //    if (сontractorId.HasValue)
        //    {
        //        target.ContractorId = сontractorId.Value;
        //    }
        //    else
        //    {
        //        throw new Exception("Нет соответствия компании в системе СЭД!");
        //    }

        //    var docType = this.GetDiadocDocType((int)source.DocumentType, (int)source.DocumentDirection);
        //    target.DocTypeId = docType != null ? docType.ObjectId : (int)CISLibApp.Common.Constant.DocumentTypes.DocCardDiadoc;

        //    if (source.ResolutionStatus != null)
        //    {
        //        target.ResolutionStatusType = Convert.ToInt32(source.ResolutionStatus.Type);
        //    }

        //    target.LastModificationTimestamp = new DateTime(source.LastModificationTimestampTicks);

        //    if (target.PacketId == null && !string.IsNullOrEmpty(source.PacketId))
        //    {
        //        target.PacketId = new Guid(source.PacketId);
        //    }

        //    if (target.MessageId == null && !string.IsNullOrEmpty(source.MessageId))
        //    {
        //        target.MessageId = new Guid(source.MessageId);
        //    }

        //    if (target.EntityId == null && !string.IsNullOrEmpty(source.EntityId))
        //    {
        //        target.EntityId = new Guid(source.EntityId);
        //    }


        //    //дополнительные атрибуты специфичные для неформализованных документов.
        //    if (source.NonformalizedDocumentMetadata != null)
        //    {
        //        target.ReceiptStatus = (int)source.NonformalizedDocumentMetadata.ReceiptStatus;

        //        target.DocumentStatus = (int)source.NonformalizedDocumentMetadata.DocumentStatus;
        //        target.Status = (int)source.NonformalizedDocumentMetadata.Status;
        //    }
        //    //дополнительные атрибуты специфичные для протоколов согласования цены.
        //    if (source.PriceListAgreementMetadata != null)
        //    {
        //        target.ReceiptStatus = (int)source.PriceListAgreementMetadata.ReceiptStatus;

        //        target.DocumentStatus = (int)source.PriceListAgreementMetadata.DocumentStatus;
        //        target.Status = (int)source.PriceListAgreementMetadata.Status;
        //    }
        //    //дополнительные атрибуты специфичные для реестров сертификатов.
        //    if (source.CertificateRegistryMetadata != null)
        //    {
        //        target.ReceiptStatus = (int)source.CertificateRegistryMetadata.ReceiptStatus;

        //        target.DocumentStatus = (int)source.CertificateRegistryMetadata.DocumentStatus;
        //        target.Status = (int)source.CertificateRegistryMetadata.Status;
        //    }



        //    //дополнительные атрибуты специфичные для счетов-фактур.
        //    if (source.InvoiceMetadata != null)
        //    {
        //        target.AmendmentFlags = (int)source.InvoiceMetadata.AmendmentFlags;
        //        target.Vat = source.InvoiceMetadata.Vat;
        //        target.Total = source.InvoiceMetadata.Total;
        //        target.Currency = source.InvoiceMetadata.Currency;

        //        target.Status = (int)source.InvoiceMetadata.Status;
        //    }
        //    //дополнительные атрибуты специфичные для исправлений счетов  != null) {//фактур.
        //    if (source.InvoiceRevisionMetadata != null)
        //    {
        //        target.AmendmentFlags = (int)source.InvoiceRevisionMetadata.AmendmentFlags;
        //        target.Vat = source.InvoiceRevisionMetadata.Vat;
        //        target.Total = source.InvoiceRevisionMetadata.Total;
        //        target.Currency = source.InvoiceRevisionMetadata.Currency;

        //        target.Status = (int)source.InvoiceRevisionMetadata.Status;
        //    }
        //    //дополнительные атрибуты специфичные для корректировочных счетов  != null) {//фактур.
        //    if (source.InvoiceCorrectionMetadata != null)
        //    {
        //        target.AmendmentFlags = (int)source.InvoiceCorrectionMetadata.AmendmentFlags;
        //        target.VatInc = source.InvoiceCorrectionMetadata.VatInc;
        //        target.VatDec = source.InvoiceCorrectionMetadata.VatDec;
        //        target.TotalInc = source.InvoiceCorrectionMetadata.TotalInc;
        //        target.TotalDec = source.InvoiceCorrectionMetadata.TotalDec;
        //        target.Currency = source.InvoiceCorrectionMetadata.Currency;

        //        target.Status = (int)source.InvoiceCorrectionMetadata.Status;
        //    }
        //    //дополнительные атрибуты специфичные для исправлений корректировочных счетов-фактур.
        //    if (source.InvoiceCorrectionRevisionMetadata != null)
        //    {
        //        target.VatInc = source.InvoiceCorrectionRevisionMetadata.VatInc;
        //        target.VatDec = source.InvoiceCorrectionRevisionMetadata.VatDec;
        //        target.TotalInc = source.InvoiceCorrectionRevisionMetadata.TotalInc;
        //        target.TotalDec = source.InvoiceCorrectionRevisionMetadata.TotalDec;
        //        target.Currency = source.InvoiceCorrectionRevisionMetadata.Currency;

        //        target.Status = (int)source.InvoiceCorrectionRevisionMetadata.Status;
        //    }


        //    //дополнительные атрибуты специфичные для документов типа TrustConnectionRequest.
        //    if (source.TrustConnectionRequestMetadata != null)
        //    {
        //        target.Status = (int)source.TrustConnectionRequestMetadata.Status;
        //    }


        //    //дополнительные атрибуты специфичные для актов сверки.
        //    if (source.ReconciliationActMetadata != null)
        //    {
        //        target.ReceiptStatus = (int)source.ReconciliationActMetadata.ReceiptStatus;

        //        target.DocumentStatus = (int)source.ReconciliationActMetadata.DocumentStatus;
        //    }
        //    //дополнительные атрибуты специфичные для типа документа дополнительное соглашение к договору.
        //    if (source.SupplementaryAgreementMetadata != null)
        //    {
        //        target.ReceiptStatus = (int)source.SupplementaryAgreementMetadata.ReceiptStatus;
        //        target.ContractType = source.SupplementaryAgreementMetadata.ContractType;
        //        target.ContractNumber = source.SupplementaryAgreementMetadata.ContractNumber;
        //        target.ContractDate = source.SupplementaryAgreementMetadata.ContractDate;
        //        target.Total = source.SupplementaryAgreementMetadata.Total;

        //        target.DocumentStatus = (int)source.SupplementaryAgreementMetadata.DocumentStatus;
        //    }


        //    //дополнительные атрибуты специфичные для актов о выполнении работ(оказании услуг).
        //    if (source.AcceptanceCertificateMetadata != null)
        //    {
        //        target.ReceiptStatus = (int)source.AcceptanceCertificateMetadata.ReceiptStatus;
        //        target.Vat = source.AcceptanceCertificateMetadata.Vat;
        //        target.Total = source.AcceptanceCertificateMetadata.Total;
        //        target.Grounds = source.AcceptanceCertificateMetadata.Grounds;

        //        target.DocumentStatus = (int)source.AcceptanceCertificateMetadata.DocumentStatus;
        //        target.Status = (int)source.AcceptanceCertificateMetadata.Status;
        //    }
        //    //дополнительные атрибуты специфичные для счетов на оплату.
        //    if (source.ProformaInvoiceMetadata != null)
        //    {
        //        target.ReceiptStatus = (int)source.ProformaInvoiceMetadata.ReceiptStatus;
        //        target.Vat = source.ProformaInvoiceMetadata.Vat;
        //        target.Total = source.ProformaInvoiceMetadata.Total;
        //        target.Grounds = source.ProformaInvoiceMetadata.Grounds;

        //        target.DocumentStatus = (int)source.ProformaInvoiceMetadata.DocumentStatus;
        //        target.Status = (int)source.ProformaInvoiceMetadata.Status;
        //    }
        //    //дополнительные атрибуты специфичные для товарных накладных ТОРГ-12.
        //    if (source.Torg12Metadata != null)
        //    {
        //        target.ReceiptStatus = (int)source.Torg12Metadata.ReceiptStatus;
        //        target.Vat = source.Torg12Metadata.Vat;
        //        target.Total = source.Torg12Metadata.Total;
        //        target.Grounds = source.Torg12Metadata.Grounds;

        //        target.DocumentStatus = (int)source.Torg12Metadata.DocumentStatus;
        //        target.Status = (int)source.Torg12Metadata.Status;
        //    }
        //    //дополнительные атрибуты специфичные для товарных накладных ТОРГ-12 в XML-формате.
        //    if (source.XmlTorg12Metadata != null)
        //    {
        //        target.ReceiptStatus = (int)source.XmlTorg12Metadata.ReceiptStatus;
        //        target.Vat = source.XmlTorg12Metadata.Vat;
        //        target.Total = source.XmlTorg12Metadata.Total;
        //        target.Grounds = source.XmlTorg12Metadata.Grounds;

        //        target.DocumentStatus = (int)source.XmlTorg12Metadata.DocumentStatus;
        //        target.Status = (int)source.XmlTorg12Metadata.Status;
        //    }
        //    //дополнительные атрибуты специфичные для актов о выполнении работ(оказании услуг) в XML-формате.
        //    if (source.XmlAcceptanceCertificateMetadata != null)
        //    {
        //        target.ReceiptStatus = (int)source.XmlAcceptanceCertificateMetadata.ReceiptStatus;
        //        target.Vat = source.XmlAcceptanceCertificateMetadata.Vat;
        //        target.Total = source.XmlAcceptanceCertificateMetadata.Total;
        //        target.Grounds = source.XmlAcceptanceCertificateMetadata.Grounds;

        //        target.DocumentStatus = (int)source.XmlAcceptanceCertificateMetadata.DocumentStatus;
        //        target.Status = (int)source.XmlAcceptanceCertificateMetadata.Status;
        //    }
        //    //дополнительные атрибуты специфичные для ценовых листов.
        //    if (source.PriceListMetadata != null)
        //    {
        //        target.ReceiptStatus = (int)source.PriceListMetadata.ReceiptStatus;

        //        target.DocumentStatus = (int)source.PriceListMetadata.DocumentStatus;
        //        target.Status = (int)source.PriceListMetadata.Status;
        //    }
        //    //дополнительные атрибуты специфичные для договоров.
        //    if (source.ContractMetadata != null)
        //    {
        //        target.ReceiptStatus = (int)source.ContractMetadata.ReceiptStatus;
        //        target.ContractPrice = source.ContractMetadata.ContractPrice;
        //        target.ContractType = source.ContractMetadata.ContractType;

        //        target.DocumentStatus = (int)source.ContractMetadata.DocumentStatus;
        //        target.Status = (int)source.ContractMetadata.Status;
        //    }
        //    //дополнительные атрибуты специфичные для накладных ТОРГ  != null) {//13.
        //    if (source.Torg13Metadata != null)
        //    {
        //        target.ReceiptStatus = (int)source.Torg13Metadata.ReceiptStatus;
        //        target.Vat = source.Torg13Metadata.Vat;
        //        target.Total = source.Torg13Metadata.Total;
        //        target.Grounds = source.Torg13Metadata.Grounds;

        //        target.DocumentStatus = (int)source.Torg13Metadata.DocumentStatus;
        //        target.Status = (int)source.Torg13Metadata.Status;
        //    }


        //    //дополнительные атрибуты специфичные для детализаций.
        //    if (source.ServiceDetailsMetadata != null)
        //    {
        //        target.ReceiptStatus = (int)source.ServiceDetailsMetadata.ReceiptStatus;

        //        target.DocumentStatus = (int)source.ServiceDetailsMetadata.DocumentStatus;
        //        target.Status = (int)source.ServiceDetailsMetadata.Status;
        //    }
        //    //дополнительные атрибуты, специфичные для УПД
        //    if (source.UniversalTransferDocumentMetadata != null)
        //    {
        //        target.AmendmentFlags = (int)source.UniversalTransferDocumentMetadata.AmendmentFlags;
        //        target.DocumentFunction = source.UniversalTransferDocumentMetadata.DocumentFunction;
        //        target.Vat = source.UniversalTransferDocumentMetadata.Vat;
        //        target.Total = source.UniversalTransferDocumentMetadata.Total;
        //        target.Currency = source.UniversalTransferDocumentMetadata.Currency;
        //        target.Grounds = source.UniversalTransferDocumentMetadata.Grounds;

        //        target.DocumentStatus = (int)source.UniversalTransferDocumentMetadata.DocumentStatus;
        //        target.Status = (int)source.UniversalTransferDocumentMetadata.Status;
        //    }
        //    //дополнительные атрибуты, специфичные для исправлений УПД
        //    if (source.UniversalTransferDocumentRevisionMetadata != null)
        //    {
        //        target.AmendmentFlags = (int)source.UniversalTransferDocumentRevisionMetadata.AmendmentFlags;
        //        target.DocumentFunction = source.UniversalTransferDocumentRevisionMetadata.DocumentFunction;
        //        target.Vat = source.UniversalTransferDocumentRevisionMetadata.Vat;
        //        target.Total = source.UniversalTransferDocumentRevisionMetadata.Total;
        //        target.Currency = source.UniversalTransferDocumentRevisionMetadata.Currency;
        //        target.Grounds = source.UniversalTransferDocumentRevisionMetadata.Grounds;

        //        target.DocumentStatus = (int)source.UniversalTransferDocumentRevisionMetadata.DocumentStatus;
        //        target.Status = (int)source.UniversalTransferDocumentRevisionMetadata.Status;
        //    }
        //    //дополнительные атрибуты, специфичные для УКД
        //    if (source.UniversalCorrectionDocumentMetadata != null)
        //    {
        //        target.AmendmentFlags = (int)source.UniversalCorrectionDocumentMetadata.AmendmentFlags;
        //        target.DocumentFunction = source.UniversalCorrectionDocumentMetadata.DocumentFunction;
        //        target.VatInc = source.UniversalCorrectionDocumentMetadata.VatInc;
        //        target.VatDec = source.UniversalCorrectionDocumentMetadata.VatDec;
        //        target.TotalInc = source.UniversalCorrectionDocumentMetadata.TotalInc;
        //        target.TotalDec = source.UniversalCorrectionDocumentMetadata.TotalDec;
        //        target.Currency = source.UniversalCorrectionDocumentMetadata.Currency;
        //        target.Grounds = source.UniversalCorrectionDocumentMetadata.Grounds;

        //        target.DocumentStatus = (int)source.UniversalCorrectionDocumentMetadata.DocumentStatus;
        //        target.Status = (int)source.UniversalCorrectionDocumentMetadata.Status;

        //    }
        //    //дополнительные атрибуты, специфичные для исправлений УКД
        //    if (source.UniversalCorrectionDocumentRevisionMetadata != null)
        //    {
        //        target.AmendmentFlags = (int)source.UniversalCorrectionDocumentRevisionMetadata.AmendmentFlags;
        //        target.DocumentFunction = source.UniversalCorrectionDocumentRevisionMetadata.DocumentFunction;
        //        target.VatInc = source.UniversalCorrectionDocumentRevisionMetadata.VatInc;
        //        target.VatDec = source.UniversalCorrectionDocumentRevisionMetadata.VatDec;
        //        target.TotalInc = source.UniversalCorrectionDocumentRevisionMetadata.TotalInc;
        //        target.TotalDec = source.UniversalCorrectionDocumentRevisionMetadata.TotalDec;
        //        target.Currency = source.UniversalCorrectionDocumentRevisionMetadata.Currency;
        //        target.Grounds = source.UniversalCorrectionDocumentRevisionMetadata.Grounds;

        //        target.DocumentStatus = (int)source.UniversalCorrectionDocumentRevisionMetadata.DocumentStatus;
        //        target.Status = (int)source.UniversalCorrectionDocumentRevisionMetadata.Status;
        //    }
        //}


        //private CISLibApp.Models.Document NewStatusesComparison(Diadoc.Api.Proto.Documents.Document source, CISLibApp.Models.Document target)
        //{
        //    //дополнительные атрибуты специфичные для неформализованных документов.
        //    if (source.NonformalizedDocumentMetadata != null)
        //    {
        //        target.DocumentStatus = (int)source.NonformalizedDocumentMetadata.DocumentStatus;
        //        target.Status = (int)source.NonformalizedDocumentMetadata.Status;
        //    }
        //    //дополнительные атрибуты специфичные для протоколов согласования цены.
        //    if (source.PriceListAgreementMetadata != null)
        //    {
        //        target.DocumentStatus = (int)source.PriceListAgreementMetadata.DocumentStatus;
        //        target.Status = (int)source.PriceListAgreementMetadata.Status;
        //    }
        //    //дополнительные атрибуты специфичные для реестров сертификатов.
        //    if (source.CertificateRegistryMetadata != null)
        //    {
        //        target.DocumentStatus = (int)source.CertificateRegistryMetadata.DocumentStatus;
        //        target.Status = (int)source.CertificateRegistryMetadata.Status;
        //    }

        //    //дополнительные атрибуты специфичные для счетов-фактур.
        //    if (source.InvoiceMetadata != null)
        //    {
        //        target.Status = (int)source.InvoiceMetadata.Status;
        //    }
        //    //дополнительные атрибуты специфичные для исправлений счетов  != null) {//фактур.
        //    if (source.InvoiceRevisionMetadata != null)
        //    {
        //        target.Status = (int)source.InvoiceRevisionMetadata.Status;
        //    }
        //    //дополнительные атрибуты специфичные для корректировочных счетов  != null) {//фактур.
        //    if (source.InvoiceCorrectionMetadata != null)
        //    {
        //        target.Status = (int)source.InvoiceCorrectionMetadata.Status;
        //    }
        //    //дополнительные атрибуты специфичные для исправлений корректировочных счетов-фактур.
        //    if (source.InvoiceCorrectionRevisionMetadata != null)
        //    {
        //        target.Status = (int)source.InvoiceCorrectionRevisionMetadata.Status;
        //    }

        //    //дополнительные атрибуты специфичные для документов типа TrustConnectionRequest.
        //    if (source.TrustConnectionRequestMetadata != null)
        //    {
        //        target.Status = (int)source.TrustConnectionRequestMetadata.Status;
        //    }


        //    //дополнительные атрибуты специфичные для актов сверки.
        //    if (source.ReconciliationActMetadata != null)
        //    {
        //        target.DocumentStatus = (int)source.ReconciliationActMetadata.DocumentStatus;
        //    }
        //    //дополнительные атрибуты специфичные для типа документа дополнительное соглашение к договору.
        //    if (source.SupplementaryAgreementMetadata != null)
        //    {
        //        target.DocumentStatus = (int)source.SupplementaryAgreementMetadata.DocumentStatus;
        //    }


        //    //дополнительные атрибуты специфичные для актов о выполнении работ(оказании услуг).
        //    if (source.AcceptanceCertificateMetadata != null)
        //    {
        //        target.DocumentStatus = (int)source.AcceptanceCertificateMetadata.DocumentStatus;
        //        target.Status = (int)source.AcceptanceCertificateMetadata.Status;
        //    }
        //    //дополнительные атрибуты специфичные для счетов на оплату.
        //    if (source.ProformaInvoiceMetadata != null)
        //    {
        //        target.DocumentStatus = (int)source.ProformaInvoiceMetadata.DocumentStatus;
        //        target.Status = (int)source.ProformaInvoiceMetadata.Status;
        //    }
        //    //дополнительные атрибуты специфичные для товарных накладных ТОРГ-12.
        //    if (source.Torg12Metadata != null)
        //    {
        //        target.DocumentStatus = (int)source.Torg12Metadata.DocumentStatus;
        //        target.Status = (int)source.Torg12Metadata.Status;
        //    }
        //    //дополнительные атрибуты специфичные для товарных накладных ТОРГ-12 в XML-формате.
        //    if (source.XmlTorg12Metadata != null)
        //    {
        //        target.DocumentStatus = (int)source.XmlTorg12Metadata.DocumentStatus;
        //        target.Status = (int)source.XmlTorg12Metadata.Status;
        //    }
        //    //дополнительные атрибуты специфичные для актов о выполнении работ(оказании услуг) в XML-формате.
        //    if (source.XmlAcceptanceCertificateMetadata != null)
        //    {
        //        target.DocumentStatus = (int)source.XmlAcceptanceCertificateMetadata.DocumentStatus;
        //        target.Status = (int)source.XmlAcceptanceCertificateMetadata.Status;
        //    }
        //    //дополнительные атрибуты специфичные для ценовых листов.
        //    if (source.PriceListMetadata != null)
        //    {
        //        target.DocumentStatus = (int)source.PriceListMetadata.DocumentStatus;
        //        target.Status = (int)source.PriceListMetadata.Status;
        //    }
        //    //дополнительные атрибуты специфичные для договоров.
        //    if (source.ContractMetadata != null)
        //    {
        //        target.DocumentStatus = (int)source.ContractMetadata.DocumentStatus;
        //        target.Status = (int)source.ContractMetadata.Status;
        //    }
        //    //дополнительные атрибуты специфичные для накладных ТОРГ  != null) {//13.
        //    if (source.Torg13Metadata != null)
        //    {
        //        target.DocumentStatus = (int)source.Torg13Metadata.DocumentStatus;
        //        target.Status = (int)source.Torg13Metadata.Status;
        //    }


        //    //дополнительные атрибуты специфичные для детализаций.
        //    if (source.ServiceDetailsMetadata != null)
        //    {
        //        target.DocumentStatus = (int)source.ServiceDetailsMetadata.DocumentStatus;
        //        target.Status = (int)source.ServiceDetailsMetadata.Status;
        //    }
        //    //дополнительные атрибуты, специфичные для УПД
        //    if (source.UniversalTransferDocumentMetadata != null)
        //    {
        //        target.DocumentStatus = (int)source.UniversalTransferDocumentMetadata.DocumentStatus;
        //        target.Status = (int)source.UniversalTransferDocumentMetadata.Status;
        //    }
        //    //дополнительные атрибуты, специфичные для исправлений УПД
        //    if (source.UniversalTransferDocumentRevisionMetadata != null)
        //    {
        //        target.DocumentStatus = (int)source.UniversalTransferDocumentRevisionMetadata.DocumentStatus;
        //        target.Status = (int)source.UniversalTransferDocumentRevisionMetadata.Status;
        //    }
        //    //дополнительные атрибуты, специфичные для УКД
        //    if (source.UniversalCorrectionDocumentMetadata != null)
        //    {
        //        target.DocumentStatus = (int)source.UniversalCorrectionDocumentMetadata.DocumentStatus;
        //        target.Status = (int)source.UniversalCorrectionDocumentMetadata.Status;

        //    }
        //    //дополнительные атрибуты, специфичные для исправлений УКД
        //    if (source.UniversalCorrectionDocumentRevisionMetadata != null)
        //    {
        //        target.DocumentStatus = (int)source.UniversalCorrectionDocumentRevisionMetadata.DocumentStatus;
        //        target.Status = (int)source.UniversalCorrectionDocumentRevisionMetadata.Status;
        //    }
        //    return target;
        //}

        ///// <summary>
        ///// Экземпляр класса DiadocApi, проксирующий работу с веб-сервисом Диадок
        ///// Крипто-API, предоставляемое операционной системой (доступно через класс WinApiCrypt)
        ///// </summary>
        //public readonly DiadocApi DiadocApi = new DiadocApi(DiadocConstants.DiadocClientId, DiadocConstants.DiadocApiUrl, new WinApiCrypt());

        //#endregion

        //#region boxes

        ///// <summary>
        ///// Загрузка связок ящиков из диадок
        ///// </summary>
        //public void LoadBoxes()
        //{
        //    Console.WriteLine("Start {2} {0} {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString(), System.Reflection.MethodBase.GetCurrentMethod().Name);
        //    var count = 0;

        //    var list = this.Contractors.Where(
        //        m =>
        //            m.INN != null &&
        //            !this.ExtIntegrations.Any(
        //                b =>
        //                    b.ExtSystemId == (int)Constant.ExtSystem.Diadoc_Box &&
        //                    b.MetaObjectId == (int)Constant.Table.Contractor &&
        //                    b.ObjectId == m.Id)).ToList();

        //    foreach (var item in list)
        //    {
        //        #region get organisation from diadoc

        //        Organization org = null;
        //        try
        //        {
        //            org = this.DiadocApi.GetOrganizationByInnKpp(item.INN, item.KPP);
        //            if (org == null)
        //            {
        //                throw new Exception("Не найдена организация по INN!");
        //            }
        //        }
        //        catch (Exception)
        //        {
        //            continue;
        //        }

        //        #endregion

        //        #region #region load box from diadoc

        //        var orgBoxes = org.Boxes.FirstOrDefault();
        //        if (orgBoxes == null)
        //        {
        //            continue;
        //        }

        //        if (this.ExtIntegrations.Any(m => m.MetaObjectId == (int)Constant.Table.Contractor &&
        //                                            m.ObjectId == item.Id &&
        //                                            m.ExtSystemId == (int)Constant.ExtSystem.Diadoc_Box &&
        //                                            m.ExternalId == orgBoxes.BoxId))
        //        {
        //            continue;
        //        }

        //        #endregion

        //        #region add new ExtIntegration Diadoc Contractor Box

        //        var temp = new ExtIntegration
        //        {
        //            ExtSystemId = (int)Constant.ExtSystem.Diadoc_Box,
        //            ExternalId = orgBoxes.BoxId,

        //            MetaObjectId = (int)Constant.Table.Contractor,
        //            ObjectId = item.Id,

        //            Name = item.Name,
        //            Description = orgBoxes.Title
        //        };

        //        this.ExtIntegrations.Add(temp);

        //        #endregion

        //        count++;
        //    }
        //    if (count > 0)
        //    {
        //        try
        //        {
        //            this.SaveChanges();
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine(BasicTools.GetErrorMessage(ex));
        //            Tools.DiadocTools.InsertSendMail(ex.ToString(), System.Reflection.MethodBase.GetCurrentMethod().Name);
        //        }
        //        Console.WriteLine("Load {0} boxes", count);
        //    }
        //    Console.WriteLine("End {2} {0} {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString(), System.Reflection.MethodBase.GetCurrentMethod().Name);
        //}

        //#endregion

        //#region box events

        ///// <summary>
        ///// Загрузка списка событий из Диадок в СЭД
        ///// </summary>
        ///// <param name="filter">Параметры фильтр</param>
        ///// <returns>Список событий из СЭД</returns>
        //public List<BoxEvent> LoadBoxEvents(string filter, string authTokenLogin, string ownerBox)
        //{
        //    Console.WriteLine("Start {2} {0} {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString(), System.Reflection.MethodBase.GetCurrentMethod().Name);

        //    #region get collection from diadoc

        //    var boxesEvents = new List<BoxEvent>();

        //    var firstOrDefault = this.BoxEvents.OrderByDescending(m => m.Timestamp).FirstOrDefault();
        //    string AfterIndexKey = firstOrDefault != null ? firstOrDefault.EventId.ToString() : null;
        //    while (true)
        //    {
        //        var response = this.DiadocApi.GetNewEvents(authTokenLogin, ownerBox, AfterIndexKey);
        //        if (response == null || !response.Events.Any() || AfterIndexKey == response.Events.Last().EventId)
        //        {
        //            break;
        //        }

        //        AfterIndexKey = response.Events.Last().EventId;
        //        var objStr = JsonConvert.SerializeObject(response.Events);
        //        boxesEvents.AddRange(JsonConvert.DeserializeObject<List<BoxEvent>>(objStr, JsonSettings.RuDateTimeFormat));
        //    }

        //    #endregion

        //    if (boxesEvents.Any())
        //    {
        //        this.BoxEvents.AddRange(boxesEvents);
        //        try
        //        {
        //            this.SaveChanges();
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine(BasicTools.GetErrorMessage(ex));
        //            Tools.DiadocTools.InsertSendMail(ex.ToString(), System.Reflection.MethodBase.GetCurrentMethod().Name);
        //            return null;
        //        }
        //        Console.WriteLine("Load {0} box events", boxesEvents.Count);

        //        this.UpdateStatusDocuments(boxesEvents, authTokenLogin, ownerBox);
        //    }
        //    Console.WriteLine("End {2} {0} {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString(), System.Reflection.MethodBase.GetCurrentMethod().Name);
        //    return boxesEvents;

        //}

        //#endregion

        //#region documents

        ///// <summary>
        ///// Загрузка списка документов из Диадок в СЭД
        ///// </summary>
        ///// <param name="filter">Параметры фильтр</param>
        ///// <returns>Список документов из СЭД</returns>
        //public List<CISLibApp.Models.Document> LoadDocuments(string filter, string authTokenLogin, string ownerBox)
        //{
        //    Console.WriteLine("Start {2} {0} {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString(), System.Reflection.MethodBase.GetCurrentMethod().Name);

        //    var allDocuments = GetDocumentList(filter, ownerBox, authTokenLogin);
        //    if (allDocuments.Any())
        //    {
        //        allDocuments = this.Documents.AddRange(allDocuments).ToList();
        //        try
        //        {
        //            this.SaveChanges();
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine(BasicTools.GetErrorMessage(ex));
        //            Tools.DiadocTools.InsertSendMail(ex.ToString(), System.Reflection.MethodBase.GetCurrentMethod().Name);
        //            return null;
        //        }
        //        //Load linking
        //        this.LoadLinkDocument(allDocuments, authTokenLogin, ownerBox);

        //        Console.WriteLine("Load {0} documents", allDocuments.Count);
        //        //Tools.Tools.InsertSendMail(string.Format("LoadDocuments.  {0}", string.Join(", ", allDocuments.Select(s => s.Id))));
        //    }
        //    Console.WriteLine("End {2} {0} {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString(), System.Reflection.MethodBase.GetCurrentMethod().Name);
        //    return allDocuments;
        //}

        //public void LoadLinkDocument(List<CISLibApp.Models.Document> documents, string authTokenLogin, string ownerBox)
        //{
        //    Console.WriteLine("Start {2} {0} {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString(), System.Reflection.MethodBase.GetCurrentMethod().Name);
        //    foreach (var document in documents)
        //    {
        //        var doc = this.DiadocApi.GetDocument(authTokenLogin, ownerBox, document.MessageId.ToString(), document.EntityId.ToString());
        //        if (doc.SubordinateDocumentIdsList.Count > 0)
        //        {
        //            var objStr = JsonConvert.SerializeObject(doc.SubordinateDocumentIds);
        //            var tempList = JsonConvert.DeserializeObject<List<DocumentSubordinate>>(objStr, JsonSettings.RuDateTimeFormat);

        //            foreach (var item in tempList)
        //            {
        //                item.DocumentId = document.Id;
        //            }
        //            this.DocumentSubordinates.AddRange(
        //                tempList.Where(
        //                    m =>
        //                        !this.DocumentSubordinates.Any(
        //                            id =>
        //                                id.DocumentId == m.DocumentId && id.MessageId == m.MessageId &&
        //                                id.EntityId == m.MessageId)));
        //        }

        //        if (doc.InitialDocumentIdsList.Count > 0)
        //        {
        //            var objStr = JsonConvert.SerializeObject(doc.InitialDocumentIds);
        //            var tempList = JsonConvert.DeserializeObject<List<DocumentInitial>>(objStr, JsonSettings.RuDateTimeFormat);
        //            foreach (var item in tempList)
        //            {
        //                item.DocumentId = document.Id;
        //            }
        //            this.DocumentInitials.AddRange(
        //                tempList.Where(
        //                    m =>
        //                        !this.DocumentInitials.Any(
        //                            id =>
        //                                id.DocumentId == m.DocumentId && id.MessageId == m.MessageId &&
        //                                id.EntityId == m.MessageId)));
        //        }
        //    }
        //    Console.WriteLine("SaveChange");
        //    try
        //    {
        //        this.SaveChanges();
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(BasicTools.GetErrorMessage(ex));
        //        Tools.DiadocTools.InsertSendMail(ex.ToString(), System.Reflection.MethodBase.GetCurrentMethod().Name);
        //    }
        //    Console.WriteLine("End {2} {0} {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString(), System.Reflection.MethodBase.GetCurrentMethod().Name);
        //}

        ///// <summary>
        ///// Получение списка документов из СЭД
        ///// </summary>
        ///// <param name="filter">Параметры фильтр</param>
        ///// <param name="box">Связка ящика в диадоке и компании в СЭД</param>
        ///// <param name="ownerBox">ИД ящика в диадоке</param>
        ///// <param name="authTokenLogin">Ключ подключения к апи</param>
        ///// <returns>Список документов из СЭД</returns>
        //public List<CISLibApp.Models.Document> GetDocumentList(string filter, string ownerBox, string authTokenLogin, bool IsUsedAfterIndexKey = true, string counteragentBoxId = null, DateTime? dtFrom = null, DateTime? dtTo = null)
        //{
        //    Console.WriteLine("Start {2} {0} {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString(), System.Reflection.MethodBase.GetCurrentMethod().Name);
        //    try
        //    {
        //        var diadocDocuments = GetDiadocDocuments(filter, ownerBox, authTokenLogin, counteragentBoxId, IsUsedAfterIndexKey, dtFrom, dtTo);
        //        var objStr = JsonConvert.SerializeObject(diadocDocuments.Where(m => this.ExtIntegrations.Any(
        //                                                                                ExtInt =>
        //                                                                                    ExtInt.MetaObjectId == (int)Constant.Table.Contractor &&
        //                                                                                    ExtInt.ExtSystemId == (int)Constant.ExtSystem.Diadoc_Box &&
        //                                                                                    ExtInt.ExternalId == m.CounteragentBoxId)
        //                                                                            && !this.Documents.Any(d => d.MessageId.ToString() == m.MessageId && d.EntityId.ToString() == m.EntityId)
        //                                                                            ));
        //        var documents = JsonConvert.DeserializeObject<List<CISLibApp.Models.Document>>(objStr, JsonSettings.RuDateTimeFormat);

        //        Console.WriteLine("Load {0} documents", documents.Count);
        //        foreach (var target in documents)
        //        {
        //            try
        //            {
        //                var source = diadocDocuments.FirstOrDefault(m => m.MessageId == target.MessageId.ToString() && m.EntityId == target.EntityId.ToString());
        //                this.NewMetaDataComparison(source, target);
        //            }
        //            catch (Exception ex)
        //            {
        //                Console.WriteLine(BasicTools.GetErrorMessage(ex));
        //                Tools.DiadocTools.InsertSendMail(ex.ToString(), System.Reflection.MethodBase.GetCurrentMethod().Name);
        //                continue;
        //            }
        //        }
        //        this.SaveChanges();

        //        Console.WriteLine("End {2} {0} {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString(), System.Reflection.MethodBase.GetCurrentMethod().Name);
        //        return documents;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(BasicTools.GetErrorMessage(ex));
        //        Tools.DiadocTools.InsertSendMail(ex.ToString(), System.Reflection.MethodBase.GetCurrentMethod().Name);
        //        return null;
        //    }
        //}

        //public List<Diadoc.Api.Proto.Documents.Document> GetDiadocDocuments(string filter, string ownerBox, string authTokenLogin, string counteragentBoxId, bool IsUsedAfterIndexKey = false, DateTime? dtFrom = null, DateTime? dtTo = null)
        //{
        //    Console.WriteLine("Start {2} {0} {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString(), System.Reflection.MethodBase.GetCurrentMethod().Name);
        //    #region get collection from diadoc
        //    string inbound = Diadoc.Api.Com.DocumentDirection.Inbound.ToString();
        //    string outbound = Diadoc.Api.Com.DocumentDirection.Outbound.ToString();

        //    var firstOrDefault = this.Documents.Where(
        //                                            m =>
        //                                                (filter.Contains(inbound) && m.DocumentDirection == (int)Diadoc.Api.Com.DocumentDirection.Inbound) ||
        //                                                (filter.Contains(outbound) && m.DocumentDirection == (int)Diadoc.Api.Com.DocumentDirection.Outbound)
        //                                        )
        //                                        .OrderByDescending(m => m.CreationTimestamp)
        //                                        .FirstOrDefault();

        //    var filterCategory = new Diadoc.Api.DocumentsFilter
        //    {
        //        BoxId = ownerBox,
        //        FilterCategory = filter,
        //        CounteragentBoxId = counteragentBoxId,
        //        TimestampFrom = dtFrom,
        //        TimestampTo = dtTo,
        //        AfterIndexKey = IsUsedAfterIndexKey && firstOrDefault != null ? firstOrDefault.IndexKey : null
        //    };

        //    var diadocDocuments = new List<Diadoc.Api.Proto.Documents.Document>();

        //    while (true)
        //    {
        //        var response = this.DiadocApi.GetDocuments(authTokenLogin, filterCategory);
        //        if (response.Documents == null || !response.Documents.Any())
        //        {
        //            break;
        //        }
        //        filterCategory.AfterIndexKey = response.Documents.Last().IndexKey;
        //        diadocDocuments.AddRange(response.Documents);
        //    }

        //    #endregion
        //    Console.WriteLine("TotalCount {0}/{1}", diadocDocuments.Count( m=> this.Documents.Any(d => d.MessageId.ToString() == m.MessageId && d.EntityId.ToString() == m.EntityId )), diadocDocuments.Count());
        //    Console.WriteLine("End {2} {0} {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString(), System.Reflection.MethodBase.GetCurrentMethod().Name);
        //    return diadocDocuments.Where(m => !this.Documents.Any(d => d.MessageId.ToString() == m.MessageId && d.EntityId.ToString() == m.EntityId)).ToList(); ;
        //}

        ///// <summary>
        ///// Обновление данных по документам из СЭД
        ///// </summary>
        ///// <param name="boxesEvents">Связка ящика в диадоке и компании в СЭД</param>
        ///// <param name="authTokenLogin">Ключ подключения к апи</param>
        ///// <param name="ownerBox">ИД ящика в диадоке</param>
        ///// <returns>Обновление полей статусов документов</returns>
        //public void UpdateStatusDocuments(List<BoxEvent> boxesEvents, string authTokenLogin, string ownerBox)
        //{
        //    Console.WriteLine("Start {2} {0} {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString(), System.Reflection.MethodBase.GetCurrentMethod().Name);

        //    foreach (var target in this.Documents.ToList())
        //    {
        //        try
        //        {
        //            if (boxesEvents.Any(b => b.MessageId == target.MessageId))
        //            {
        //                var source = this.DiadocApi.GetDocument(authTokenLogin, ownerBox, target.MessageId.ToString(), target.EntityId.ToString());
        //                this.NewMetaDataComparison(source, target);
        //            }
        //        }
        //        catch (Exception)
        //        {
        //            continue;
        //        }
        //    }
        //    Console.WriteLine("SaveChanges");
        //    try
        //    {
        //        this.SaveChanges();
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(BasicTools.GetErrorMessage(ex));
        //        Tools.DiadocTools.InsertSendMail(ex.ToString(), System.Reflection.MethodBase.GetCurrentMethod().Name);
        //    }
        //    Console.WriteLine("End {2} {0} {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString(), System.Reflection.MethodBase.GetCurrentMethod().Name);
        //}

        ///// <summary>
        ///// Обновление данных по документам из СЭД
        ///// </summary>
        ///// <param name="sources">Список документов из дидок</param>
        //public void UpdateStatusDocuments(List<Diadoc.Api.Proto.Documents.Document> sources)
        //{
        //    Console.WriteLine("Start {2} {0} {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString(), System.Reflection.MethodBase.GetCurrentMethod().Name);

        //    foreach (var source in sources)
        //    {
        //        try
        //        {
        //            var target = this.Documents.FirstOrDefault(m => m.MessageId.ToString() == source.MessageId && m.EntityId .ToString() == source.EntityId);
        //            this.NewMetaDataComparison(source, target);
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine(BasicTools.GetErrorMessage(ex));
        //            Tools.DiadocTools.InsertSendMail(ex.ToString(), System.Reflection.MethodBase.GetCurrentMethod().Name);
        //            continue;
        //        }
        //    }
        //    this.SaveChanges();

        //    Console.WriteLine("End {2} {0} {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString(), System.Reflection.MethodBase.GetCurrentMethod().Name);
        //}


        //#endregion

        //#region zip

        ///// <summary>
        ///// Обновление архивов в разрезе компании
        ///// </summary>
        ///// <param name="authTokenLogin">Ключ подключения к апи</param>
        ///// <param name="ownerBox">ИД ящика в диадоке</param>
        ///// <returns>Количестко загруженных архивов</returns>
        //public int LoadZip(List<CISLibApp.Models.Document> documents, string authTokenLogin, string ownerBox)
        //{
        //    Console.WriteLine("Start {2} {0} {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString(), System.Reflection.MethodBase.GetCurrentMethod().Name);
        //    var count = 0;

        //    foreach (var item in documents)
        //    {
        //        try
        //        {
        //            if (item.DocCardId.HasValue)
        //            {
        //                var fileName = this.GetDiadocFileName(item);

        //                #region Generate document zip

        //                var file = this.DiadocApi.GenerateDocumentZip(authTokenLogin, ownerBox, item.MessageId.ToString(), item.EntityId.ToString(), true);
        //                Console.WriteLine("Waiting {2} link file from shelf \"{0}.zip\" for DocCard №{1} ", fileName, item.DocCardId, file.RetryAfter);
        //                while (file.RetryAfter > 0)
        //                {
        //                    System.Threading.Thread.Sleep(file.RetryAfter * 1000);
        //                    file = this.DiadocApi.GenerateDocumentZip(authTokenLogin, ownerBox, item.MessageId.ToString(), item.EntityId.ToString(), true);
        //                }
        //                Console.WriteLine("Waiting {2} file \"{0}.zip\" for DocCard №{1} ", fileName, item.DocCardId, file.RetryAfter);
        //                var fileStream = this.DiadocApi.GetFileFromShelf(authTokenLogin, file.ZipFileNameOnShelf);
        //                #endregion

        //                #region Add attachment to СЭД

        //                if (Tools.DiadocTools.AddAttachment(fileStream, fileName, item.CreationTimestamp, item.DocCardId.Value) != null)
        //                {
        //                    count++;
        //                }

        //                #endregion
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine(BasicTools.GetErrorMessage(ex));
        //            Tools.DiadocTools.InsertSendMail(ex.ToString(), System.Reflection.MethodBase.GetCurrentMethod().Name);
        //            continue;
        //        }
        //    }
        //    if (count > 0)
        //    {
        //        Console.WriteLine("Count documents {0}", documents.Count());
        //    }
        //    Console.WriteLine("End {2} {0} {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString(), System.Reflection.MethodBase.GetCurrentMethod().Name);
        //    return count;
        //}

        //#endregion
    }
}
