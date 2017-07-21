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
        
    }
}
