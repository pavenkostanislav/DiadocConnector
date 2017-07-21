using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UACCnsApp.DiadocDocument.Interfaces
{
    public interface IObserver
    {
        void Update(object sender, Aggregators.BoxesEventArgs e);
    }
}
