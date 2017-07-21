using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UACCnsApp.DiadocDocument.Aggregators;
using UACCnsApp.DiadocDocument.Interfaces;

namespace UACCnsApp.DiadocDocument.Widgets
{
    class OrganizationWidget : IObserver, IWidget
    {
        private List<Diadoc.Api.Proto.Organization> _organizations;

        private int _organizationCount;

        public OrganizationWidget()
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
                _organizations = e.Organizations;
                foreach(var _organization in _organizations)
                {
                    var contractor = db.Contractors.FirstOrDefault(m=>m.INN == _organization.Inn);

                    #region Validation contractor

                    if (contractor == null)
                    {
                        Console.WriteLine("По ИНН: " + _organization.Inn + " и КПП: " + _organization.Kpp + " компания не найдена");
                        continue;
                    }

                    #endregion

                    #region Validation box
                    //1 _firstbox == null
                    var _firstbox = _organization.Boxes.FirstOrDefault();
                    if (_firstbox == null)
                    {
                        continue;
                    }
                    //2 ExtIntegrations.Any
                    if (    db.ExtIntegrations.Any(
                                m =>
                                    m.MetaObjectId == (int)CISLibApp.Common.Constant.Table.Contractor &&
                                    m.ObjectId == contractor.Id &&
                                    m.ExtSystemId == (int)CISLibApp.Common.Constant.ExtSystem.Diadoc_Box &&
                                    m.ExternalId == _firstbox.BoxId )   )
                    {
                        continue;
                    }

                    #endregion

                    #region add new ExtIntegration Diadoc Contractor Box

                    var temp = new CISLibApp.Models.ExtIntegration
                    {
                        ExtSystemId = (int)CISLibApp.Common.Constant.ExtSystem.Diadoc_Box,
                        ExternalId = _firstbox.BoxId,

                        MetaObjectId = (int)CISLibApp.Common.Constant.Table.Contractor,
                        ObjectId = contractor.Id,

                        Name = contractor.Name,
                        Description = _organization.FullName
                    };

                    db.ExtIntegrations.Add(temp);
                    _organizationCount++;

                    #endregion

                }
                if (_organizationCount > 0)
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
            Console.WriteLine("Created {0}/{1} organisations links", _organizationCount, _organizations.Count);
        }
    }
}
