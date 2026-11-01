using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPStracking_app
{
    public class Warehouse 
    {
        public event Action<string>? MessageGenerated; 

        public void PrepareUnloading()
        {
            MessageGenerated?.Invoke("СКЛАД: Транспорт прибув! Розвантажуємо..");
        }

        public void OnDeviation() // DeviationFromRoute
        {
            MessageGenerated?.Invoke("СКЛАД: Транспорт відхилився від маршруту! Коригуємо час розвантаження..");
        }
    }
}

