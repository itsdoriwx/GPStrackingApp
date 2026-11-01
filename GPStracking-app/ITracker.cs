using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPStracking_app
{
    public interface ITracker
    {
        void Update(double x, double y, double plannedX, double plannedY, double speed);
    }
}
