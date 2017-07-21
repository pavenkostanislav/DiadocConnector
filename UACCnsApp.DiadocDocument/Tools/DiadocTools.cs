using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using CISLibApp.Basic.Models;
using CISLibApp.Basic.NoDbModels;
using System.Xml.Linq;
using CISLibApp.Common;
using UACCnsApp.Models;

namespace UACCnsApp.Tools
{
    /// <summary>
    /// Класс статических методов
    /// </summary>
	public static class DiadocTools
    {
//        /// <summary>
//        /// Загрузка вложения к объекту-источнику
//        /// </summary>
//        /// <param name="fileStream">Содежимое файла</param>
//        /// <param name="fileName">Имя файла без типа</param>
//        /// <param name="fileDate">Дата события по котороому получен файл</param>
//        /// <param name="docCardId">Номер карточки СЭД</param>
//        public static AttachmentReference AddAttachment(byte[] fileStream, string fileName, DateTime fileDate, int docCardId)
//        {
//#if DEBUG
//            var methodBase = System.Reflection.MethodBase.GetCurrentMethod();
//            Console.WriteLine("{0} {1}.{2} have been started executing at {3}", methodBase.MemberType, methodBase.DeclaringType.Name, methodBase.Name, DateTime.Now.ToString("dd.MM.yy HH:mm:ss"));
//#endif
//            if (fileStream != null)
//            {

//                var contentType = "application/zip";
//                var fileNameType = string.Format("{0}.zip", fileName);

//                using (var db = new Cis())
//                {
//                    var fileSize = fileStream.Length;
//                    var docCard = db.DocCards.FirstOrDefault(dc => dc.Id == docCardId);
//                    if (docCard == null)
//                    {
//                        Console.WriteLine("Не найдена карточка");
//                        return null;
//                    }
//                    if (docCard.SigningCompleted)
//                    {
//                        Console.WriteLine("В карточке {0} завершён документооборот!", docCard.Id);
//                        return null;
//                    }
//                    var isAttachmentReferencesAny = db.AttachmentReferences.Any(
//                                                    ar =>
//                                                        ar.MetaObjectId == (int)Constant.Table.DocCard &&
//                                                        ar.ObjectId == docCardId &&
//                                                        ar.AttachmentDescription.DocumentTypeId == (int)Constant.DocumentTypes.AttachDiadoc &&
//                                                        ar.AttachmentDescription.FileName == fileNameType &&
//                                                        ar.AttachmentDescription.FileSize == fileSize);
                    
//                    if (isAttachmentReferencesAny)
//                    {
//                        Console.WriteLine("Существует архив \"{0}\" ({1}b) \n\tс типом \"вложение из диадока\"", fileNameType, fileSize);
//                        return null;
//                    }

//                   var oldAttachmentReferences = db.AttachmentReferences.Include("AttachmentDescription").FirstOrDefault(
//                                        m =>
//                                            m.MetaObjectId == (int)Constant.Table.DocCard &&
//                                            m.ObjectId == docCardId &&
//                                            m.AttachmentDescription.DocumentTypeId == (int)Constant.DocumentTypes.AttachDiadoc);

//                    #region Удаление прежнего вложения

//                    //var file =
//                    //    db.AttachmentReferences
//                    //    .Include("AttachmentDescription")
//                    //        .AsQueryable()
//                    //        .FirstOrDefault(
//                    //            m =>
//                    //                m.MetaObjectId == (int) Constant.Table.DocCard && 
//                    //                m.ObjectId == objId &&
//                    //                m.AttachmentDescription.DocumentTypeId == (int)Constant.DocumentTypes.AttachDiadoc);

//                    //if (file != null)
//                    //{
//                    //    Console.WriteLine("Remove attach {0}", file.AttachmentDescription.FileName);
//                    //    var attachmentDescriptionRemove = file.AttachmentDescription;
//                    //    var attachmentsViewRemove = attachmentDescriptionRemove.AttachmentsView;

//                    //    db.AttachmentReferences.Remove(file);
//                    //    db.AttachmentDescriptions.Remove(attachmentDescriptionRemove);
//                    //    db.AttachmentsViews.Remove(attachmentsViewRemove);
//                    //    db.SaveChanges();
//                    //}

//                    #endregion


//                    var temp = db.AttachmentDescriptions;
//                    var streamId = Guid.NewGuid();

//                    #region новое вложение AttachmentsView

//                    var attachmentsView = new AttachmentsView
//                    {
//                        Id = streamId,
//                        FileStream = fileStream,
//                        FileName = streamId.ToString() + fileNameType
//                    };

//                    attachmentsView = db.AttachmentsViews.Add(attachmentsView);

//                    db.SaveChanges();

//                    #endregion

//                    var attachmentDescription = new AttachmentDescription
//                    {
//                        Name = fileName,
//                        ContentType = contentType,
//                        AttachmentId = attachmentsView.Id,
//                        DocumentTypeId = (int)Constant.DocumentTypes.AttachDiadoc,
//                        Version = 1,
//                        FileName = fileNameType,
//                        FileSize = fileStream.Length,
//                        CreatedDate = fileDate
//                    };

//                    #region добавляем описание файла AttachmentDescription

//                    if (oldAttachmentReferences != null && oldAttachmentReferences.AttachmentDescription != null)
//                    {
//                        attachmentDescription.FirstVersionId = (oldAttachmentReferences.AttachmentDescription.FirstVersionId == 0 ? oldAttachmentReferences.AttachmentDescription.Id : oldAttachmentReferences.AttachmentDescription.FirstVersionId);
//                        attachmentDescription.Version = oldAttachmentReferences.AttachmentDescription.Version + 1;
//                    }

//                    attachmentDescription = db.AttachmentDescriptions.Add(attachmentDescription);

//                    db.SaveChanges();

//                    #endregion

//                    #region Ссылка на объект AttachmentReference

//                    if (oldAttachmentReferences == null)
//                    {
//                        oldAttachmentReferences = new AttachmentReference
//                        {
//                            AttachmentDescriptionId = attachmentDescription.Id,
//                            MetaObjectId = docCard.MetaObjectId ?? (int)Constant.Table.DocCard,
//                            ObjectId = docCard.ObjectId ?? docCardId
//                        };
//                    }

//                    oldAttachmentReferences.AttachmentDescriptionId = attachmentDescription.Id;
//                    if (db.AttachmentReferences.Any(m => m.MetaObjectId == (int)Constant.Table.DocCard && m.ObjectId == docCardId))
//                    {
//                        oldAttachmentReferences.OrderNumber =
//                            db.AttachmentReferences.Where(
//                                m => m.MetaObjectId == (int)Constant.Table.DocCard && m.ObjectId == docCardId)
//                                .Max(m => m.OrderNumber) + 1;
//                    }
//                    else
//                    {
//                        oldAttachmentReferences.OrderNumber = 1;
//                    }

//                    oldAttachmentReferences = db.AttachmentReferences.Add(oldAttachmentReferences);
//                    db.SaveChanges();

//                    #endregion

//                    Console.WriteLine("File loaded successfully!");

//                    InsertSendMail(string.Format("Загрузка архива {1} в документ №{0}", docCardId, fileNameType), docCardId);
//                    return oldAttachmentReferences;
//                }
//            }
//            return null;
//        }


        /// <summary>
        /// Отправка уведомления на почту
        /// </summary>
        /// <param name="body">Тело сообщения</param>
        /// <param name="objectId">Номер карточки</param>
        /// <returns>null при успешном выполнении или сообщение об ошибке</returns>
        public static string InsertSendMail(string body, int objectId)
        {

            var mail = new SendMailEasy
            {
                recipients = "support@kpma.ru",
                head = "UACCnsApp.DiadocDocument Интеграция c диадок. Загрузка новых архивов из диадок",
                body = body,
                MetaObjectId = (int)Constant.Table.DocCard,
                ObjectId = objectId
            };

            return InsertSendMail(mail);
        }

        /// <summary>
        /// Отправка уведомления на почту
        /// </summary>
        /// <param name="body">Тело сообщения</param>
        /// <returns>null при успешном выполнении или сообщение об ошибке</returns>
        public static string InsertSendMail(string body, string head = null)
        {
            if (head == null) head = string.Empty;
            var mail = new SendMailEasy
            {
                recipients = "s.pavenko@kpma.ru",
                head = "UACCnsApp.DiadocDocument "+ head +" Интеграция c диадок",
                body = body
            };

            return InsertSendMail(mail);
        }


        /// <summary>
        /// Отправка уведомления на почту
        /// </summary>
        /// <returns>null при успешном выполнении или сообщение об ошибке</returns>
        public static string InsertSendMail(SendMailEasy mail)
        {
	        try
	        {
                if (mail == null) { throw new Exception("Не найдено сообщение"); }
                if (mail.recipients == null) { throw new Exception("Не указан не один получатель"); }
                if (mail.head == null) { throw new Exception("Не заполнена тема сообщения"); }
                if (mail.body == null) { throw new Exception("Не заполнено тело сообщения"); }

                using (var db = new Cis())
                {
                    var userDiadoc = db.MetaUsers.FirstOrDefault(m => m.Name == "Diadoc");
                    if (userDiadoc == null)
                    {
                        throw new Exception("Не заполнено userDiadoc");
                    }

                    if (mail.body == null)
                    {
                        throw new Exception("Не заполнено тело сообщения");
                    }


                    var recipients = new SqlParameter("@Recipients", mail.recipients);
                    var head = new SqlParameter("@Head", mail.head);
                    var body = new SqlParameter("@Body", mail.body);
                    var createdBy = new SqlParameter("@CreatedBy", userDiadoc.Login);

                    var foot = new SqlParameter("@Foot", SqlDbType.NVarChar)
                    {
                        Value = (object)mail.foot ?? DBNull.Value
                    };
                    var subject = new SqlParameter("@Subject", SqlDbType.NVarChar)
                    {
                        Value = (object)mail.subject ?? DBNull.Value
                    };
                    var metaObjectId = new SqlParameter("@MetaObjectId", mail.MetaObjectId)
                    {
                        Value = (object)mail.MetaObjectId ?? DBNull.Value
                    };
                    var objectId = new SqlParameter("@ObjectId", mail.ObjectId)
                    {
                        Value = (object)mail.ObjectId ?? DBNull.Value
                    };

                    db.Database.ExecuteSqlCommand(
                        "exec [dbo].[sp_InsertSendMail] @Recipients, @Head,	@Body, @CreatedBy, @MetaObjectId, @ObjectId, @Subject,	@Foot",
                        recipients, head, body, createdBy, metaObjectId, objectId, subject, foot);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex.ToString());
#endif
            }
            return null;
        }


    }
}
