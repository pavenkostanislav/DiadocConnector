using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CISLibApp.Basic.Common;
using CISLibApp.Common;
using Newtonsoft.Json;
using UACCnsApp.Models;
using System.Reflection;

namespace UACCnsApp.DiadocDocument
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Start executing aggregator...");
#if DEBUG
                var methodBase = System.Reflection.MethodBase.GetCurrentMethod();
                Console.WriteLine("{0} {1}.{2} have been started executing at {3}", methodBase.MemberType, methodBase.DeclaringType.Name, methodBase.Name, DateTime.Now.ToString("dd.MM.yy HH:mm:ss"));
#endif
                var aggregator = new Aggregators.DiadocAggregator();

                var organizationWidget = new Widgets.OrganizationWidget();
                var boxEventWidget = new Widgets.BoxEventWidget();
                var documentWidget = new Widgets.DocumentWidget();

                aggregator.BoxesChecked += new Aggregators.BoxesCheckedEventHandler(organizationWidget.Update);
                aggregator.BoxesChecked += new Aggregators.BoxesCheckedEventHandler(boxEventWidget.Update);
                aggregator.BoxesChecked += new Aggregators.BoxesCheckedEventHandler(documentWidget.Update);

                aggregator.Start();

                #region OLD load
                //using (var db = new Cis())
                //{
                //    db.LoadBoxes();

                //    var ownerBox = db.GetDiadocBoxId(Constant.OwnerId);
                //    var authTokenLogin = db.DiadocApi.Authenticate(DiadocConstants.DiadocLogin, DiadocConstants.DiadocPassword);

                //    var filter = "Any." + Diadoc.Api.Com.DocumentDirection.Inbound;
                //    db.LoadDocuments(filter, authTokenLogin, ownerBox);
                //    db.LoadBoxEvents(filter, authTokenLogin, ownerBox);

                //    filter = "Any." + Diadoc.Api.Com.DocumentDirection.Outbound;
                //    db.LoadDocuments(filter, authTokenLogin, ownerBox);
                //    db.LoadBoxEvents(filter, authTokenLogin, ownerBox);

                //    var dt = DateTime.Now.AddHours(-60);
                //    var listDoc = db.Documents.Where(
                //                        m =>
                //                            (m.LastUpdatedDate >= dt || m.CreationTimestamp >= dt
                //                                ||
                //                                db.BoxEvents.Any(
                //                                    be =>
                //                                        be.MessageId == m.MessageId &&
                //                                        (be.LastUpdatedDate >= dt || be.Timestamp >= dt))
                //                            ) &&
                //                            m.DocCardId > 0 &&
                //                            m.Active)
                //                    .ToList();
                //    db.LoadZip(listDoc, authTokenLogin, ownerBox);
                //}
                #endregion

#if DEBUG
                Console.ReadKey();
#endif
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex.ToString());
#endif
                Tools.DiadocTools.InsertSendMail(ex.ToString(), System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
    }
}