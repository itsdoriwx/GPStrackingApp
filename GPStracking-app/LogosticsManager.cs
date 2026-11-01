using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPStracking_app
{
    public class LogisticsManager
    {
        public event Action<string>? MessageGenerated;
        public event Action? TripDispatchRequested; // подія - запуск рейсу

        public void RegisterShipment() 
        {
            MessageGenerated?.Invoke("МЕНЕДЖЕР: Посилку зареєстровано в системі.");
        }

        public void DispatchTrip(string transportName = "транспорт")
        {
            MessageGenerated?.Invoke($"МЕНЕДЖЕР: Надано дозвіл на виїзд. {transportName} готується до відправлення.");
            TripDispatchRequested?.Invoke();
        }

        public void OnDeviation()
        {
            MessageGenerated?.Invoke("МЕНЕДЖЕР: Відхилення від маршруту! Зв'язуюсь з водієм..");
        }
    }
}
