using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPStracking_app;

namespace GPStracking_app.Tests
{
    [TestClass]
    public class GpsTrackerTests
    {
        [TestMethod] 
        public void GpsTracker_ArrivedAtDestination_NotifiesAllSubscribers()
        {
            var truck = new Truck();
            var tracker = new GpsTracker(truck);
            var warehouse = new Warehouse();
            var customer = new Customer();

            bool warehouseNotified = false;
            bool customerNotified = false;
            bool truckNotified = false;

            tracker.ArrivedAtDestination += () => warehouseNotified = true;
            tracker.ArrivedAtDestination += () => customerNotified = true;
            tracker.ArrivedAtDestination += truck.OnArrived;
            truck.MessageGenerated += _ => truckNotified = true;

            tracker.SetDestination(100, 100);
            tracker.Update(100, 100, 100, 100, 0);

            Assert.IsTrue(warehouseNotified, "Склад має отримати сигнал про прибуття.");
            Assert.IsTrue(customerNotified, "Клієнт має отримати сигнал про прибуття.");
            Assert.IsTrue(truckNotified, "Вантажівка має відреагувати на прибуття.");
        }

        [TestMethod] // чек генерації повідомлення при перевищенні швидкості
        public void GpsTracker_SpeedLimitExceeded_FiresEventWithSpeedMessage()
        {
            var truck = new Truck();
            var tracker = new GpsTracker(truck) { SpeedLimit = 90.0 };
            string receivedMsg = string.Empty;

            tracker.SpeedLimitExceeded += msg => receivedMsg = msg;
            tracker.Update(10, 10, 10, 10, 105.0);

            Assert.IsTrue(receivedMsg.Contains("ТРЕКЕР: Перевищення швидкості!"));
            Assert.IsTrue(receivedMsg.Contains("105"));
        }

        [TestMethod] 
        public void GpsTracker_NormalSpeed_DoesNotTriggerSpeedingEvent()
        {
            var truck = new Truck();
            var tracker = new GpsTracker(truck) { SpeedLimit = 90.0 };
            bool isSpeedingFired = false;

            tracker.SpeedLimitExceeded += _ => isSpeedingFired = true;
            tracker.Update(10, 10, 10, 10, 80.0);

            Assert.IsFalse(isSpeedingFired);
        }

        [TestMethod] // чек широкомовної події відхилення від маршруту
        public void GpsTracker_DeviationFromRoute_TriggersEventForMultipleSubscribers()
        {
            var truck = new Truck();
            var tracker = new GpsTracker(truck) { DeviationTolerance = 5.0 };
            var manager = new LogisticsManager();
            var warehouse = new Warehouse();

            bool trackerEventFired = false;
            bool managerNotified = false;
            bool warehouseNotified = false;

            tracker.DeviationFromRoute += _ => trackerEventFired = true;
            tracker.DeviationFromRoute += _ => manager.OnDeviation();
            tracker.DeviationFromRoute += _ => warehouse.OnDeviation();

            manager.MessageGenerated += _ => managerNotified = true;
            warehouse.MessageGenerated += _ => warehouseNotified = true;

            tracker.Update(10, 30, 10, 10, 50.0);

            Assert.IsTrue(trackerEventFired);
            Assert.IsTrue(managerNotified);
            Assert.IsTrue(warehouseNotified);
        }

        [TestMethod] 
        public void GpsTracker_Reset_ClearsStateAndAllowsReTrigger()
        {
            var truck = new Truck();
            var tracker = new GpsTracker(truck);

            tracker.Update(10, 50, 10, 10, 120.0);
            tracker.Reset();

            int eventCount = 0;
            tracker.SpeedLimitExceeded += _ => eventCount++;
            tracker.Update(10, 10, 10, 10, 120.0);

            Assert.AreEqual(1, eventCount);
        }
    }
}