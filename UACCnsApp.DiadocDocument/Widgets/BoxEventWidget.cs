using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UACCnsApp.DiadocDocument.Aggregators;
using UACCnsApp.DiadocDocument.Interfaces;

namespace UACCnsApp.DiadocDocument.Widgets
{
    class BoxEventWidget : IObserver, IWidget
    {
        private List<Diadoc.Api.Proto.Events.BoxEvent> _boxEventsDiadoc { get; set; }
        private List<Models.BoxEvent> _boxEvents { get; set; }

        public BoxEventWidget()
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

            _boxEventsDiadoc = e.BoxEvents;
            using (var db = new Models.Cis())
            {
                var serializeObject = Newtonsoft.Json.JsonConvert.SerializeObject(_boxEventsDiadoc);
                _boxEvents = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Models.BoxEvent>>(serializeObject, CISLibApp.Basic.Common.JsonSettings.RuDateTimeFormat);

                db.BoxEvents.AddRange(_boxEvents);
                if (_boxEvents.Any())
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
                }
            }
            Display();
        }

        public void Display()
        {
            Console.WriteLine("Created {0}/{1} boxes events links", _boxEvents.Count, _boxEventsDiadoc.Count);
        }
    }
}
