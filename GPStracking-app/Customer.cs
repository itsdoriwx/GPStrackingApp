using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPStracking_app
{
    public class Customer
    {
        public event Action<string>? MessageGenerated;
        public event Action<string>? ParcelReceivedByCustomer;

        public void NotifyArrival()
        {
            MessageGenerated?.Invoke("Ваше замовлення прибуло у відділення!");
        }

        public void PickUpParcel()
        {
            MessageGenerated?.Invoke("Посилку отримано. Дякуємо!");
            ParcelReceivedByCustomer?.Invoke("СИСТЕМА: Клієнт забрав посилку з відділення.");
        }
    }
}
