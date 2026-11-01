namespace GPStracking_app.Tests
{
    [TestClass]
    public class VehicleTests
    {
        [TestMethod] 
        public void Truck_ImplementsThreeInterfaces_Successfully()
        {
            var truck = new Truck();

            Assert.IsInstanceOfType(truck, typeof(IGpsTrackable));
            Assert.IsInstanceOfType(truck, typeof(IDeliverable));
            Assert.IsInstanceOfType(truck, typeof(IMonitored));
        }

        [TestMethod] 
        public void Vehicle_NameProperties_ReturnCorrectCapitalizedNames()
        {
            Vehicle truck = new Truck();
            Vehicle train = new Train();

            Assert.AreEqual("ВАНТАЖІВКА", truck.Name);
            Assert.AreEqual("ПОТЯГ", train.Name);
        }

        [TestMethod] 
        public void Truck_InterfaceMethods_ReturnValidStrings()
        {
            var truck = new Truck();

            string deliveryStatus = truck.Deliver();
            string monitorStatus = truck.GetStatus();

            Assert.IsFalse(string.IsNullOrWhiteSpace(deliveryStatus));
            Assert.IsFalse(string.IsNullOrWhiteSpace(monitorStatus));
        }

    }
}
