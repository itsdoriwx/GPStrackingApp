using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPStracking_app
{
   public class Train : Vehicle, IGpsTrackable, IMonitored
   {
        public Train() : base("ПОТЯГ", 80) { }
        public (double X, double Y) GetPosition() => (X, Y);                       // IGpsTrackable
        public string GetStatus() => $"Рух за розкладом. Швидкість: {Speed} км/год"; // IMonitored
   }
}
