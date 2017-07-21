using CISLibApp.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UACCnsApp.DiadocDocument.Aggregators
{
    public class BoxesEventArgs
    {
        public List<Diadoc.Api.Proto.Organization> Organizations { get; private set; }
        public List<Diadoc.Api.Proto.Events.BoxEvent> BoxEvents { get; private set; }
        public List<Diadoc.Api.Proto.Documents.Document> Documents { get; private set; }
        public List<UACCnsApp.DiadocDocument.DiadocModels.ZipArhiveDiadocModel> ZipArhiveDiadocModels { get; private set; }
        public BoxesEventArgs(List<Diadoc.Api.Proto.Organization> organizations
                            , List<Diadoc.Api.Proto.Events.BoxEvent> boxEvents
                            , List<Diadoc.Api.Proto.Documents.Document> documents
                            , List<DiadocModels.ZipArhiveDiadocModel> zipArhiveDiadocModels)
        {
#if DEBUG
            var methodBase = System.Reflection.MethodBase.GetCurrentMethod();
            Console.WriteLine("{0} {1} is calling at {2}", methodBase.MemberType, methodBase.DeclaringType.Name, DateTime.Now.ToString("dd.MM.yy HH:mm:ss"));
#endif
            Organizations = organizations;
            BoxEvents = boxEvents;
            Documents = documents;
            ZipArhiveDiadocModels = zipArhiveDiadocModels;
        }
    }
    public delegate void BoxesCheckedEventHandler(object sender, BoxesEventArgs e);

    public class DiadocAggregator
    {
        /// <summary>
        /// Экземпляр класса DiadocApi, проксирующий работу с веб-сервисом Диадок
        /// Крипто-API, предоставляемое операционной системой (доступно через класс WinApiCrypt)
        /// </summary>
        private readonly Diadoc.Api.DiadocApi DiadocApi;
        private readonly string authTokenLogin;

        public DiadocAggregator()
        {
            DiadocApi = new Diadoc.Api.DiadocApi(DiadocConstants.DiadocClientId, DiadocConstants.DiadocApiUrl, new Diadoc.Api.Cryptography.WinApiCrypt());
            authTokenLogin = DiadocApi.Authenticate(DiadocConstants.DiadocLogin, DiadocConstants.DiadocPassword);
#if DEBUG
            var methodBase = System.Reflection.MethodBase.GetCurrentMethod();
            Console.WriteLine("{0} {1} is calling at {2}", methodBase.MemberType, methodBase.DeclaringType.Name, DateTime.Now.ToString("dd.MM.yy HH:mm:ss"));
#endif
        }

        public event BoxesCheckedEventHandler BoxesChecked;

        public void Start()
        {
#if DEBUG
                var methodBase = System.Reflection.MethodBase.GetCurrentMethod();
                Console.WriteLine("{0} {1}.{2} have been started executing at {3}", methodBase.MemberType, methodBase.DeclaringType.Name, methodBase.Name, DateTime.Now.ToString("dd.MM.yy HH:mm:ss"));
#endif
            List<Diadoc.Api.Proto.Organization> organizations = null;
            List<Diadoc.Api.Proto.Events.BoxEvent> boxEvents = null;
            List<Diadoc.Api.Proto.Documents.Document> documents = null;
            List<DiadocModels.ZipArhiveDiadocModel> zipArhiveDiadocModels = null;

            Delegate[] subscribers = this.BoxesChecked.GetInvocationList();
            foreach (var subscriber in subscribers)
            {
                switch (subscriber.Method.DeclaringType.Name)
                {
                    case "OrganizationWidget":
                        organizations = GetNewOrganization();
                        break;
                    case "BoxEventWidget":
                        boxEvents = GetNewBoxEvent();
                        break;
                    case "DocumentWidget":
                        documents = GetDocuments();
                        break;
                    case "ZipArhiveDiadocModelWidget":
                        var startDateTime = DateTime.Now.AddHours(-2);
                        zipArhiveDiadocModels = GetZipArhiveDiadocModels(startDateTime);
                        break;
                }
            }


            //Порождение события, если есть подписчики.
            BoxesChecked?.Invoke(this, new Aggregators.BoxesEventArgs(organizations, boxEvents, documents, zipArhiveDiadocModels));
        }

        private List<Diadoc.Api.Proto.Organization> GetNewOrganization()
        {
#if DEBUG
                var methodBase = System.Reflection.MethodBase.GetCurrentMethod();
                Console.WriteLine("{0} {1}.{2} have been started executing at {3}", methodBase.MemberType, methodBase.DeclaringType.Name, methodBase.Name, DateTime.Now.ToString("dd.MM.yy HH:mm:ss"));
#endif

            using (var db = new Models.Cis())
            {
                var organizations = new List<Diadoc.Api.Proto.Organization>();

                #region Get organization list

                foreach (var contractor in db.Contractors.Where(m => !string.IsNullOrEmpty(m.INN)).ToList())
                {
                    Diadoc.Api.Proto.Organization organization = null;
                    try
                    {
                        organization = DiadocApi.GetOrganizationByInnKpp(contractor.INN, contractor.KPP);
                    }
                    catch (Exception ex)
                    {
#if DEBUG
                        Console.WriteLine(ex.Message);
#endif
                        continue;
                    }
                    #region Validation

                    if (organization != null)
                    {
                        var boxes = organization.Boxes.FirstOrDefault();
                        var isExtIntegration = db.ExtIntegrations
                                            .Any(
                                                m =>
                                                    m.ExtSystemId == (int)Constant.ExtSystem.Diadoc_Box &&
                                                    boxes.BoxId == m.ExternalId &&

                                                    m.MetaObjectId == (int)Constant.Table.Contractor &&
                                                    m.ObjectId == contractor.Id
                                            );
                        #endregion

                        if (!isExtIntegration)
                        {
                            organizations.Add(organization);
                        }
                    }
                }

                #endregion

                Console.WriteLine("got {0} new organizations via diadoc operator", organizations.Count);
                return organizations;
            }
        }

        private List<Diadoc.Api.Proto.Events.BoxEvent> GetNewBoxEvent()
        {
#if DEBUG
                var methodBase = System.Reflection.MethodBase.GetCurrentMethod();
                Console.WriteLine("{0} {1}.{2} have been started executing at {3}", methodBase.MemberType, methodBase.DeclaringType.Name, methodBase.Name, DateTime.Now.ToString("dd.MM.yy HH:mm:ss"));
#endif

            using (var db = new Models.Cis())
            {
                var ownerBox = db.GetDiadocBoxId(Constant.OwnerId);
                var boxesEvents = new List<Diadoc.Api.Proto.Events.BoxEvent>();

                var firstOrDefault = db.BoxEvents.OrderByDescending(m => m.Timestamp).FirstOrDefault();
                string AfterIndexKey = firstOrDefault?.EventId.ToString();
                while (true)
                {
                    var response = DiadocApi.GetNewEvents(authTokenLogin, ownerBox, AfterIndexKey);
                    if (response == null || !response.Events.Any() || AfterIndexKey == response.Events.Last().EventId)
                    {
                        break;
                    }
                    boxesEvents.AddRange(response.Events.Where(m=> !db.BoxEvents.Any(e=>e.MessageId.ToString() == m.MessageId && e.EventId.ToString() == m.EventId)));
                    AfterIndexKey = response.Events.Last().EventId;
                }

                Console.WriteLine("got {0} new boxes events via diadoc operator", boxesEvents.Count);
                return boxesEvents;
            }
        }

        private List<Diadoc.Api.Proto.Documents.Document> GetDocuments(string counteragentBoxId = null, bool isNew = true, DateTime? dtFrom = null, DateTime? dtTo = null)
        {
#if DEBUG
                var methodBase = System.Reflection.MethodBase.GetCurrentMethod();
                Console.WriteLine("{0} {1}.{2} have been started executing at {3}", methodBase.MemberType, methodBase.DeclaringType.Name, methodBase.Name, DateTime.Now.ToString("dd.MM.yy HH:mm:ss"));
#endif

            using (var db = new Models.Cis())
            {
                var ownerBox = db.GetDiadocBoxId(Constant.OwnerId);
                var diadocDocuments = new List<Diadoc.Api.Proto.Documents.Document>();
                foreach (Diadoc.Api.Com.DocumentDirection documentDirection in Enum.GetValues(typeof(Diadoc.Api.Com.DocumentDirection)))
                {
                    if (documentDirection == Diadoc.Api.Com.DocumentDirection.UnknownDocumentDirection) { continue; }
                    string afterIndexKey = null;
                    
                    if (isNew)
                    {
                        switch (documentDirection)
                        {
                            case Diadoc.Api.Com.DocumentDirection.Inbound:
                                afterIndexKey = db.Documents?.Where(m => m.DocumentDirection == (int)Diadoc.Api.Com.DocumentDirection.Inbound)?.OrderByDescending(m => m.CreationTimestamp)?.FirstOrDefault()?.IndexKey;
                                break;
                            case Diadoc.Api.Com.DocumentDirection.Outbound:
                                afterIndexKey = db.Documents?.Where(m => m.DocumentDirection == (int)Diadoc.Api.Com.DocumentDirection.Outbound)?.OrderByDescending(m => m.CreationTimestamp)?.FirstOrDefault()?.IndexKey;
                                break;
                        }
                    }
                    var filterCategory = new Diadoc.Api.DocumentsFilter
                    {
                        BoxId = ownerBox,
                        FilterCategory = "Any." + documentDirection,
                        CounteragentBoxId = counteragentBoxId,
                        TimestampFrom = dtFrom,
                        TimestampTo = dtTo,
                        AfterIndexKey = afterIndexKey
                    };

                    while (true)
                    {
                        var response = this.DiadocApi.GetDocuments(authTokenLogin, filterCategory);
                        if (response.Documents == null || !response.Documents.Any())
                        {
                            break;
                        }
                        filterCategory.AfterIndexKey = response.Documents.Last().IndexKey;
                        diadocDocuments.AddRange(response.Documents);
                    }
                }
                Console.WriteLine("got {0} new documents via diadoc operator", diadocDocuments.Count);
                return diadocDocuments;
            }
        }

        private List<DiadocModels.ZipArhiveDiadocModel> GetZipArhiveDiadocModels(DateTime startdate)
        {
#if DEBUG
                var methodBase = System.Reflection.MethodBase.GetCurrentMethod();
                Console.WriteLine("{0} {1}.{2} have been started executing at {3}", methodBase.MemberType, methodBase.DeclaringType.Name, methodBase.Name, DateTime.Now.ToString("dd.MM.yy HH:mm:ss"));
#endif

            using (var db = new Models.Cis())
            {
                var zipArhiveDiadocModels = new List<DiadocModels.ZipArhiveDiadocModel>();
                var ownerBox = db.GetDiadocBoxId(Constant.OwnerId);

                var listDoc = db.Documents.Where(
                                    m =>
                                        (m.LastUpdatedDate >= startdate || m.CreationTimestamp >= startdate
                                            ||
                                            db.BoxEvents.Any(
                                                be =>
                                                    be.MessageId == m.MessageId &&
                                                    (be.LastUpdatedDate >= startdate || be.Timestamp >= startdate))
                                        ) &&
                                        m.DocCardId > 0 &&
                                        m.Active)
                                .ToList();

                foreach (var item in listDoc)
                {
                    try
                    {
                        if (item.DocCardId.HasValue)
                        {
                            var fileName = db.GetDiadocFileName(item);

                            #region Generate document zip

                            var file = DiadocApi.GenerateDocumentZip(authTokenLogin, ownerBox, item.MessageId.ToString(), item.EntityId.ToString(), true);
                            while (file.RetryAfter > 0)
                            {
                                Console.WriteLine("shelf file {0} sec waiting...", file.RetryAfter);
                                System.Threading.Thread.Sleep(file.RetryAfter * 1000);
                                file = DiadocApi.GenerateDocumentZip(authTokenLogin, ownerBox, item.MessageId.ToString(), item.EntityId.ToString(), true);
                            }
                            Console.WriteLine("file \"{0}.zip\" geting...", fileName);
                            var fileStream = this.DiadocApi.GetFileFromShelf(authTokenLogin, file.ZipFileNameOnShelf);
                            #endregion

                            zipArhiveDiadocModels.Add( new DiadocModels.ZipArhiveDiadocModel
                            {
                                DocCardId = item.DocCardId.Value,
                                CounteragentBoxId = item.CounteragentBoxId,
                                EntityId = item.MessageId,
                                MessageId = item.EntityId,
                                FileName = fileName,
                                FileData = fileStream,
                                CreationTimestamp = db.BoxEvents?.Where(be => be.MessageId == item.MessageId)?.Max(m => m.Timestamp) ?? item.CreationTimestamp
                            });
                        }
                    }
                    catch (Exception ex)
                    {
#if DEBUG
                        Console.WriteLine(ex.ToString());
#endif
                        Tools.DiadocTools.InsertSendMail(ex.ToString(), System.Reflection.MethodBase.GetCurrentMethod().Name);
                        continue;
                    }
                }
                Console.WriteLine("got {0} files via diadoc operator", zipArhiveDiadocModels.Count);
                return zipArhiveDiadocModels;
            }
        }
    }
}
