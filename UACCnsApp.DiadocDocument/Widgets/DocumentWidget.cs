using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UACCnsApp.DiadocDocument.Aggregators;
using UACCnsApp.DiadocDocument.Interfaces;

namespace UACCnsApp.DiadocDocument.Widgets
{
    class DocumentWidget : IObserver, IWidget
    {
        private List<Diadoc.Api.Proto.Documents.Document> _documentsDiadoc { get; set; }
        private List<CISLibApp.Models.Document> _documents { get; set; }

        public DocumentWidget()
        {
#if DEBUG
            var methodBase = System.Reflection.MethodBase.GetCurrentMethod();
            Console.WriteLine("{0} {1} is calling at {2}", methodBase.MemberType, methodBase.DeclaringType.Name, DateTime.Now.ToString("dd.MM.yy HH:mm:ss"));
#endif
        }
        public void Update(object sender, BoxesEventArgs e)
        {
#if DEBUG
            var methodBase = System.Reflection.MethodBase.GetCurrentMethod();
            Console.WriteLine("{0} {1}.{2} have been started executing at {3}", methodBase.MemberType, methodBase.DeclaringType.Name, methodBase.Name, DateTime.Now.ToString("dd.MM.yy HH:mm:ss"));
#endif

            _documentsDiadoc = e.Documents;
            using (var db = new Models.Cis())
            {
                var serializeObject = Newtonsoft.Json.JsonConvert.SerializeObject(_documentsDiadoc.Where(m => db.ExtIntegrations.Any(
                                                                                       ExtInt =>
                                                                                           ExtInt.MetaObjectId == (int)CISLibApp.Common.Constant.Table.Contractor &&
                                                                                           ExtInt.ExtSystemId == (int)CISLibApp.Common.Constant.ExtSystem.Diadoc_Box &&
                                                                                           ExtInt.ExternalId == m.CounteragentBoxId)
                                                                                   && !db.Documents.Any(d => d.MessageId.ToString() == m.MessageId && d.EntityId.ToString() == m.EntityId)
                                                                                   ));
                _documents = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CISLibApp.Models.Document>>(serializeObject, CISLibApp.Basic.Common.JsonSettings.RuDateTimeFormat);

                foreach (var target in _documents)
                {
                    try
                    {
                        var source = _documentsDiadoc.FirstOrDefault(m => m.MessageId == target.MessageId.ToString() && m.EntityId == target.EntityId.ToString());

                        var сontractorId = db.GetCortractorId(source.CounteragentBoxId);

                        if (сontractorId.HasValue)
                        {
                            target.ContractorId = сontractorId.Value;
                        }
                        else
                        {
                            Console.WriteLine("Нет соответствия ящика {0} и компании в системе СЭД!", source.CounteragentBoxId);
                            continue;
                            //throw new Exception("Нет соответствия компании в системе СЭД!");
                        }

                        var docType = db.GetDiadocDocType((int)source.DocumentType, (int)source.DocumentDirection);
                        target.DocTypeId = docType != null ? docType.ObjectId : (int)CISLibApp.Common.Constant.DocumentTypes.DocCardDiadoc;

                        NewMetaDataComparison(source, target);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        Tools.DiadocTools.InsertSendMail(ex.ToString(), System.Reflection.MethodBase.GetCurrentMethod().Name);
                        continue;
                    }
                }
                db.Documents.AddRange(_documents);
                if (_documents.Any())
                {
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
#if DEBUG
                        Console.WriteLine(ex.ToString());
#endif
                        Tools.DiadocTools.InsertSendMail(ex.ToString(), System.Reflection.MethodBase.GetCurrentMethod().Name);
                    }
                    
                    var aggregator = new Aggregators.DiadocAggregator();
                    var zipArhiveDiadocModelWidgetWidget = new Widgets.ZipArhiveDiadocModelWidget();
                    aggregator.BoxesChecked += new Aggregators.BoxesCheckedEventHandler(zipArhiveDiadocModelWidgetWidget.Update);
                    aggregator.Start();
                }
            }
            Display();
        }

        public void Display()
        {
            Console.WriteLine("Created {0}/{1} documents links", _documents.Count, _documentsDiadoc.Count);
        }

        public static void NewMetaDataComparison(Diadoc.Api.Proto.Documents.Document source, CISLibApp.Models.Document target)
        {

            if (source.ResolutionStatus != null)
            {
                target.ResolutionStatusType = Convert.ToInt32(source.ResolutionStatus.Type);
            }

            if (target.PacketId == null && !string.IsNullOrEmpty(source.PacketId))
            {
                target.PacketId = new Guid(source.PacketId);
            }

            if (target.MessageId == null && !string.IsNullOrEmpty(source.MessageId))
            {
                target.MessageId = new Guid(source.MessageId);
            }

            if (target.EntityId == null && !string.IsNullOrEmpty(source.EntityId))
            {
                target.EntityId = new Guid(source.EntityId);
            }

            //дополнительные атрибуты специфичные для неформализованных документов.
            if (source.NonformalizedDocumentMetadata != null)
            {
                target.ReceiptStatus = (int)source.NonformalizedDocumentMetadata.ReceiptStatus;

                target.DocumentStatus = (int)source.NonformalizedDocumentMetadata.DocumentStatus;
                target.Status = (int)source.NonformalizedDocumentMetadata.Status;
            }
            //дополнительные атрибуты специфичные для протоколов согласования цены.
            if (source.PriceListAgreementMetadata != null)
            {
                target.ReceiptStatus = (int)source.PriceListAgreementMetadata.ReceiptStatus;

                target.DocumentStatus = (int)source.PriceListAgreementMetadata.DocumentStatus;
                target.Status = (int)source.PriceListAgreementMetadata.Status;
            }
            //дополнительные атрибуты специфичные для реестров сертификатов.
            if (source.CertificateRegistryMetadata != null)
            {
                target.ReceiptStatus = (int)source.CertificateRegistryMetadata.ReceiptStatus;

                target.DocumentStatus = (int)source.CertificateRegistryMetadata.DocumentStatus;
                target.Status = (int)source.CertificateRegistryMetadata.Status;
            }
            //дополнительные атрибуты специфичные для счетов-фактур.
            if (source.InvoiceMetadata != null)
            {
                target.AmendmentFlags = (int)source.InvoiceMetadata.AmendmentFlags;
                target.Vat = source.InvoiceMetadata.Vat;
                target.Total = source.InvoiceMetadata.Total;
                target.Currency = source.InvoiceMetadata.Currency;

                target.Status = (int)source.InvoiceMetadata.Status;
            }
            //дополнительные атрибуты специфичные для исправлений счетов  != null) {//фактур.
            if (source.InvoiceRevisionMetadata != null)
            {
                target.AmendmentFlags = (int)source.InvoiceRevisionMetadata.AmendmentFlags;
                target.Vat = source.InvoiceRevisionMetadata.Vat;
                target.Total = source.InvoiceRevisionMetadata.Total;
                target.Currency = source.InvoiceRevisionMetadata.Currency;

                target.Status = (int)source.InvoiceRevisionMetadata.Status;
            }
            //дополнительные атрибуты специфичные для корректировочных счетов  != null) {//фактур.
            if (source.InvoiceCorrectionMetadata != null)
            {
                target.AmendmentFlags = (int)source.InvoiceCorrectionMetadata.AmendmentFlags;
                target.VatInc = source.InvoiceCorrectionMetadata.VatInc;
                target.VatDec = source.InvoiceCorrectionMetadata.VatDec;
                target.TotalInc = source.InvoiceCorrectionMetadata.TotalInc;
                target.TotalDec = source.InvoiceCorrectionMetadata.TotalDec;
                target.Currency = source.InvoiceCorrectionMetadata.Currency;

                target.Status = (int)source.InvoiceCorrectionMetadata.Status;
            }
            //дополнительные атрибуты специфичные для исправлений корректировочных счетов-фактур.
            if (source.InvoiceCorrectionRevisionMetadata != null)
            {
                target.VatInc = source.InvoiceCorrectionRevisionMetadata.VatInc;
                target.VatDec = source.InvoiceCorrectionRevisionMetadata.VatDec;
                target.TotalInc = source.InvoiceCorrectionRevisionMetadata.TotalInc;
                target.TotalDec = source.InvoiceCorrectionRevisionMetadata.TotalDec;
                target.Currency = source.InvoiceCorrectionRevisionMetadata.Currency;

                target.Status = (int)source.InvoiceCorrectionRevisionMetadata.Status;
            }


            //дополнительные атрибуты специфичные для документов типа TrustConnectionRequest.
            if (source.TrustConnectionRequestMetadata != null)
            {
                target.Status = (int)source.TrustConnectionRequestMetadata.Status;
            }


            //дополнительные атрибуты специфичные для актов сверки.
            if (source.ReconciliationActMetadata != null)
            {
                target.ReceiptStatus = (int)source.ReconciliationActMetadata.ReceiptStatus;

                target.DocumentStatus = (int)source.ReconciliationActMetadata.DocumentStatus;
            }
            //дополнительные атрибуты специфичные для типа документа дополнительное соглашение к договору.
            if (source.SupplementaryAgreementMetadata != null)
            {
                target.ReceiptStatus = (int)source.SupplementaryAgreementMetadata.ReceiptStatus;
                target.ContractType = source.SupplementaryAgreementMetadata.ContractType;
                target.ContractNumber = source.SupplementaryAgreementMetadata.ContractNumber;
                target.ContractDate = source.SupplementaryAgreementMetadata.ContractDate;
                target.Total = source.SupplementaryAgreementMetadata.Total;

                target.DocumentStatus = (int)source.SupplementaryAgreementMetadata.DocumentStatus;
            }


            //дополнительные атрибуты специфичные для актов о выполнении работ(оказании услуг).
            if (source.AcceptanceCertificateMetadata != null)
            {
                target.ReceiptStatus = (int)source.AcceptanceCertificateMetadata.ReceiptStatus;
                target.Vat = source.AcceptanceCertificateMetadata.Vat;
                target.Total = source.AcceptanceCertificateMetadata.Total;
                target.Grounds = source.AcceptanceCertificateMetadata.Grounds;

                target.DocumentStatus = (int)source.AcceptanceCertificateMetadata.DocumentStatus;
                target.Status = (int)source.AcceptanceCertificateMetadata.Status;
            }
            //дополнительные атрибуты специфичные для счетов на оплату.
            if (source.ProformaInvoiceMetadata != null)
            {
                target.ReceiptStatus = (int)source.ProformaInvoiceMetadata.ReceiptStatus;
                target.Vat = source.ProformaInvoiceMetadata.Vat;
                target.Total = source.ProformaInvoiceMetadata.Total;
                target.Grounds = source.ProformaInvoiceMetadata.Grounds;

                target.DocumentStatus = (int)source.ProformaInvoiceMetadata.DocumentStatus;
                target.Status = (int)source.ProformaInvoiceMetadata.Status;
            }
            //дополнительные атрибуты специфичные для товарных накладных ТОРГ-12.
            if (source.Torg12Metadata != null)
            {
                target.ReceiptStatus = (int)source.Torg12Metadata.ReceiptStatus;
                target.Vat = source.Torg12Metadata.Vat;
                target.Total = source.Torg12Metadata.Total;
                target.Grounds = source.Torg12Metadata.Grounds;

                target.DocumentStatus = (int)source.Torg12Metadata.DocumentStatus;
                target.Status = (int)source.Torg12Metadata.Status;
            }
            //дополнительные атрибуты специфичные для товарных накладных ТОРГ-12 в XML-формате.
            if (source.XmlTorg12Metadata != null)
            {
                target.ReceiptStatus = (int)source.XmlTorg12Metadata.ReceiptStatus;
                target.Vat = source.XmlTorg12Metadata.Vat;
                target.Total = source.XmlTorg12Metadata.Total;
                target.Grounds = source.XmlTorg12Metadata.Grounds;

                target.DocumentStatus = (int)source.XmlTorg12Metadata.DocumentStatus;
                target.Status = (int)source.XmlTorg12Metadata.Status;
            }
            //дополнительные атрибуты специфичные для актов о выполнении работ(оказании услуг) в XML-формате.
            if (source.XmlAcceptanceCertificateMetadata != null)
            {
                target.ReceiptStatus = (int)source.XmlAcceptanceCertificateMetadata.ReceiptStatus;
                target.Vat = source.XmlAcceptanceCertificateMetadata.Vat;
                target.Total = source.XmlAcceptanceCertificateMetadata.Total;
                target.Grounds = source.XmlAcceptanceCertificateMetadata.Grounds;

                target.DocumentStatus = (int)source.XmlAcceptanceCertificateMetadata.DocumentStatus;
                target.Status = (int)source.XmlAcceptanceCertificateMetadata.Status;
            }
            //дополнительные атрибуты специфичные для ценовых листов.
            if (source.PriceListMetadata != null)
            {
                target.ReceiptStatus = (int)source.PriceListMetadata.ReceiptStatus;

                target.DocumentStatus = (int)source.PriceListMetadata.DocumentStatus;
                target.Status = (int)source.PriceListMetadata.Status;
            }
            //дополнительные атрибуты специфичные для договоров.
            if (source.ContractMetadata != null)
            {
                target.ReceiptStatus = (int)source.ContractMetadata.ReceiptStatus;
                target.ContractPrice = source.ContractMetadata.ContractPrice;
                target.ContractType = source.ContractMetadata.ContractType;

                target.DocumentStatus = (int)source.ContractMetadata.DocumentStatus;
                target.Status = (int)source.ContractMetadata.Status;
            }
            //дополнительные атрибуты специфичные для накладных ТОРГ  != null) {//13.
            if (source.Torg13Metadata != null)
            {
                target.ReceiptStatus = (int)source.Torg13Metadata.ReceiptStatus;
                target.Vat = source.Torg13Metadata.Vat;
                target.Total = source.Torg13Metadata.Total;
                target.Grounds = source.Torg13Metadata.Grounds;

                target.DocumentStatus = (int)source.Torg13Metadata.DocumentStatus;
                target.Status = (int)source.Torg13Metadata.Status;
            }


            //дополнительные атрибуты специфичные для детализаций.
            if (source.ServiceDetailsMetadata != null)
            {
                target.ReceiptStatus = (int)source.ServiceDetailsMetadata.ReceiptStatus;

                target.DocumentStatus = (int)source.ServiceDetailsMetadata.DocumentStatus;
                target.Status = (int)source.ServiceDetailsMetadata.Status;
            }
            //дополнительные атрибуты, специфичные для УПД
            if (source.UniversalTransferDocumentMetadata != null)
            {
                target.AmendmentFlags = (int)source.UniversalTransferDocumentMetadata.AmendmentFlags;
                target.DocumentFunction = source.UniversalTransferDocumentMetadata.DocumentFunction;
                target.Vat = source.UniversalTransferDocumentMetadata.Vat;
                target.Total = source.UniversalTransferDocumentMetadata.Total;
                target.Currency = source.UniversalTransferDocumentMetadata.Currency;
                target.Grounds = source.UniversalTransferDocumentMetadata.Grounds;

                target.DocumentStatus = (int)source.UniversalTransferDocumentMetadata.DocumentStatus;
                target.Status = (int)source.UniversalTransferDocumentMetadata.Status;
            }
            //дополнительные атрибуты, специфичные для исправлений УПД
            if (source.UniversalTransferDocumentRevisionMetadata != null)
            {
                target.AmendmentFlags = (int)source.UniversalTransferDocumentRevisionMetadata.AmendmentFlags;
                target.DocumentFunction = source.UniversalTransferDocumentRevisionMetadata.DocumentFunction;
                target.Vat = source.UniversalTransferDocumentRevisionMetadata.Vat;
                target.Total = source.UniversalTransferDocumentRevisionMetadata.Total;
                target.Currency = source.UniversalTransferDocumentRevisionMetadata.Currency;
                target.Grounds = source.UniversalTransferDocumentRevisionMetadata.Grounds;

                target.DocumentStatus = (int)source.UniversalTransferDocumentRevisionMetadata.DocumentStatus;
                target.Status = (int)source.UniversalTransferDocumentRevisionMetadata.Status;
            }
            //дополнительные атрибуты, специфичные для УКД
            if (source.UniversalCorrectionDocumentMetadata != null)
            {
                target.AmendmentFlags = (int)source.UniversalCorrectionDocumentMetadata.AmendmentFlags;
                target.DocumentFunction = source.UniversalCorrectionDocumentMetadata.DocumentFunction;
                target.VatInc = source.UniversalCorrectionDocumentMetadata.VatInc;
                target.VatDec = source.UniversalCorrectionDocumentMetadata.VatDec;
                target.TotalInc = source.UniversalCorrectionDocumentMetadata.TotalInc;
                target.TotalDec = source.UniversalCorrectionDocumentMetadata.TotalDec;
                target.Currency = source.UniversalCorrectionDocumentMetadata.Currency;
                target.Grounds = source.UniversalCorrectionDocumentMetadata.Grounds;

                target.DocumentStatus = (int)source.UniversalCorrectionDocumentMetadata.DocumentStatus;
                target.Status = (int)source.UniversalCorrectionDocumentMetadata.Status;

            }
            //дополнительные атрибуты, специфичные для исправлений УКД
            if (source.UniversalCorrectionDocumentRevisionMetadata != null)
            {
                target.AmendmentFlags = (int)source.UniversalCorrectionDocumentRevisionMetadata.AmendmentFlags;
                target.DocumentFunction = source.UniversalCorrectionDocumentRevisionMetadata.DocumentFunction;
                target.VatInc = source.UniversalCorrectionDocumentRevisionMetadata.VatInc;
                target.VatDec = source.UniversalCorrectionDocumentRevisionMetadata.VatDec;
                target.TotalInc = source.UniversalCorrectionDocumentRevisionMetadata.TotalInc;
                target.TotalDec = source.UniversalCorrectionDocumentRevisionMetadata.TotalDec;
                target.Currency = source.UniversalCorrectionDocumentRevisionMetadata.Currency;
                target.Grounds = source.UniversalCorrectionDocumentRevisionMetadata.Grounds;

                target.DocumentStatus = (int)source.UniversalCorrectionDocumentRevisionMetadata.DocumentStatus;
                target.Status = (int)source.UniversalCorrectionDocumentRevisionMetadata.Status;
            }

            target.LastModificationTimestamp = new DateTime(source.LastModificationTimestampTicks);
        }

    }
}
