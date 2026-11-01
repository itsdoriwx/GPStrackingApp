using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPStracking_app
{
    public interface IGpsTrackable 
    {
        (double X, double Y) GetPosition();
    }
}
