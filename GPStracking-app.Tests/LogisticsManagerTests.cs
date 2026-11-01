using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPStracking_app;

namespace GPStracking_app.Tests
{
    [TestClass]
    public class LogisticsManagerTests
    {
        [TestMethod] // чек генерації повідомлення реєстрації
        public void LogisticsManager_RegisterShipment_GeneratesMessage()
        {
            var manager = new LogisticsManager();
            string managerLog = string.Empty;

            manager.MessageGenerated += msg => managerLog = msg;
            manager.RegisterShipment();

            Assert.IsFalse(string.IsNullOrEmpty(managerLog));
            Assert.IsTrue(managerLog.Contains("зареєстровано"));
        }

        [TestMethod] // чек запиту на виїзд
        public void LogisticsManager_DispatchTrip_RaisesTripDispatchRequestedEvent()
        {
            var manager = new LogisticsManager();
            bool isDispatchRequested = false;

            manager.TripDispatchRequested += () => isDispatchRequested = true;
            manager.DispatchTrip("ПОТЯГ");

            Assert.IsTrue(isDispatchRequested);
        }

        [TestMethod] // чек отримання клієнтом повідомлення про прибуття
        public void Customer_PickUpParcel_FiresParcelReceivedEvent()
        {
            var customer = new Customer();
            string receivedMsg = string.Empty;

            customer.ParcelReceivedByCustomer += msg => receivedMsg = msg;
            customer.PickUpParcel();

            Assert.IsFalse(string.IsNullOrEmpty(receivedMsg));
            Assert.IsTrue(receivedMsg.Contains("забрав посилку"));
        }
    }
}