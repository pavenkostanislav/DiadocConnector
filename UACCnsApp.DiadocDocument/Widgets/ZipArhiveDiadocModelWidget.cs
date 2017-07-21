using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UACCnsApp.DiadocDocument.Aggregators;
using UACCnsApp.DiadocDocument.Interfaces;

namespace UACCnsApp.DiadocDocument.Widgets
{
    class ZipArhiveDiadocModelWidget : IObserver, IWidget
    {
        private List<UACCnsApp.DiadocDocument.DiadocModels.ZipArhiveDiadocModel> _zipArhiveDiadocModels { get; set; }

        public ZipArhiveDiadocModelWidget()
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

            using (var db = new Models.Cis())
            {
                _zipArhiveDiadocModels = e.ZipArhiveDiadocModels;
                foreach (var zip in _zipArhiveDiadocModels)
                {
                    if (zip.FileData != null)
                    {

                        var contentType = "application/zip";
                        var fileNameType = string.Format("{0}.zip", zip.FileName);

                        var fileSize = zip.FileData.Length;
                        var docCard = db.DocCards.FirstOrDefault(dc => dc.Id == zip.DocCardId);
                        if (docCard == null)
                        {
#if DEBUG
                            Console.WriteLine("zipitem.DocCardId is not found");
#endif
                            continue;
                        }
                        if (docCard.SigningCompleted)
                        {
#if DEBUG
                            Console.WriteLine("DocCardId: {0} had checked singing completed field", docCard.Id);
#endif
                            continue;
                        }
                        var isAttachmentReferencesAny = db.AttachmentReferences.Any(
                                                        ar =>
                                                            ar.MetaObjectId == (int)CISLibApp.Common.Constant.Table.DocCard &&
                                                            ar.ObjectId == zip.DocCardId &&
                                                            ar.AttachmentDescription.DocumentTypeId == (int)CISLibApp.Common.Constant.DocumentTypes.AttachDiadoc &&
                                                            ar.AttachmentDescription.FileName == fileNameType &&
                                                            ar.AttachmentDescription.FileSize == fileSize);

                        if (isAttachmentReferencesAny)
                        {
#if DEBUG
                            Console.WriteLine("Violation of UNIQUE KEY constraint. Is created zip arhive \"{0}\" (size: {1}, type: {2})", fileNameType, fileSize, CISLibApp.Common.Constant.DocumentTypes.AttachDiadoc.ToString());
#endif
                            continue;
                        }

                        var oldAttachmentReferences = db.AttachmentReferences.Include("AttachmentDescription").FirstOrDefault(
                                             m =>
                                                 m.MetaObjectId == (int)CISLibApp.Common.Constant.Table.DocCard &&
                                                 m.ObjectId == zip.DocCardId &&
                                                 m.AttachmentDescription.DocumentTypeId == (int)CISLibApp.Common.Constant.DocumentTypes.AttachDiadoc);
                        
                        var streamId = Guid.NewGuid();

#region новое вложение AttachmentsView

                        var attachmentsView = new Models.AttachmentsView
                        {
                            Id = streamId,
                            FileStream = zip.FileData,
                            FileName = streamId.ToString() + fileNameType
                        };

                        attachmentsView = db.AttachmentsViews.Add(attachmentsView);

                        db.SaveChanges();

#endregion

#region добавляем описание файла AttachmentDescription

                        var attachmentDescription = new Models.AttachmentDescription
                        {
                            Name = zip.FileName,
                            ContentType = contentType,
                            AttachmentId = attachmentsView.Id,
                            DocumentTypeId = (int)CISLibApp.Common.Constant.DocumentTypes.AttachDiadoc,
                            Version = 1,
                            FileName = fileNameType,
                            FileSize = zip.FileData.Length,
                            CreatedDate = zip.CreationTimestamp
                        };


                        if (oldAttachmentReferences != null && oldAttachmentReferences.AttachmentDescription != null)
                        {
                            attachmentDescription.FirstVersionId = (oldAttachmentReferences.AttachmentDescription.FirstVersionId == 0 ? oldAttachmentReferences.AttachmentDescription.Id : oldAttachmentReferences.AttachmentDescription.FirstVersionId);
                            attachmentDescription.Version = oldAttachmentReferences.AttachmentDescription.Version + 1;
                        }

                        attachmentDescription = db.AttachmentDescriptions.Add(attachmentDescription);

                        db.SaveChanges();

#endregion

#region Ссылка на объект AttachmentReference

                        if (oldAttachmentReferences == null)
                        {
                            oldAttachmentReferences = new Models.AttachmentReference
                            {
                                AttachmentDescriptionId = attachmentDescription.Id,
                                MetaObjectId = docCard.MetaObjectId ?? (int)CISLibApp.Common.Constant.Table.DocCard,
                                ObjectId = docCard.ObjectId ?? zip.DocCardId
                            };
                        }

                        oldAttachmentReferences.AttachmentDescriptionId = attachmentDescription.Id;
                        if (db.AttachmentReferences.Any(m => m.MetaObjectId == (int)CISLibApp.Common.Constant.Table.DocCard && m.ObjectId == zip.DocCardId))
                        {
                            oldAttachmentReferences.OrderNumber =
                                db.AttachmentReferences.Where(
                                    m => m.MetaObjectId == (int)CISLibApp.Common.Constant.Table.DocCard && m.ObjectId == zip.DocCardId)
                                    .Max(m => m.OrderNumber) + 1;
                        }
                        else
                        {
                            oldAttachmentReferences.OrderNumber = 1;
                        }

                        oldAttachmentReferences = db.AttachmentReferences.Add(oldAttachmentReferences);
                        db.SaveChanges();

#endregion
                    }
                }
            }
            Display();
        }

        public void Display()
        {
            Console.WriteLine("Download {0} zip files", _zipArhiveDiadocModels.Count);
        }
    }
}
