using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UACCnsApp.DiadocDocument.DiadocModels
{
    public class ZipArhiveDiadocModel
    {
        public ZipArhiveDiadocModel()
        {
#if DEBUG
        var methodBase = System.Reflection.MethodBase.GetCurrentMethod();
        Console.WriteLine("{0} {1} is calling at {2}", methodBase.MemberType, methodBase.DeclaringType.Name, DateTime.Now.ToString("dd.MM.yy HH:mm:ss"));
#endif
        }
        public int DocCardId { get; set; }

        public string CounteragentBoxId { get; set; }
        public Guid EntityId { get; set; }
        public Guid MessageId { get; set; }

        public string FileName { get; set; }
        public byte[] FileData { get; internal set; }
        public DateTime CreationTimestamp { get; set; }
    }
}
