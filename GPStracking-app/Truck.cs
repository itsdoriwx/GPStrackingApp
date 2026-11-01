using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPStracking_app
{
    public class Truck : Vehicle, IGpsTrackable, IDeliverable, IMonitored
    {
        public event Action<string>? MessageGenerated; 

        public Truck() : base("ВАНТАЖІВКА", 60) { }

        public (double X, double Y) GetPosition() => (X, Y); // IGpsTrackable
        public string Deliver() => "ВАНТАЖІВКА виконує рейс."; // IDeliverable
        public string GetStatus() => $"Двигун в нормі. Швидкість: {Speed} км/год"; // IMonitored

      
        public void OnArrived()   // ArrivedAtDestination
        {
            MessageGenerated?.Invoke("ВАНТАЖІВКА: Маршрут пройдено!");
        }
    }
}
