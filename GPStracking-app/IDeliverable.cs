using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPStracking_app
{
    public interface IDeliverable // повертає текстовий статус про процес доставки
    {
        string Deliver();
    }
}
