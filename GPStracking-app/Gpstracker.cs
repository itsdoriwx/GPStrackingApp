using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPStracking_app
{
    public class GpsTracker : ITracker
    {
        private Vehicle vehicle;
        private double destX, destY;
        private bool isOffRoute;
        private bool isSpeeding;

        public double SpeedLimit { get; set; } = 95.0;
        public double DeviationTolerance { get; set; } = 10.0;

        public event Action<string>? SpeedLimitExceeded;
        public event Action<string>? DeviationFromRoute;
        public event Action? ArrivedAtDestination;
        public event Action<double, double>? PositionChanged;

        public GpsTracker(Vehicle vehicle)
        {
            this.vehicle = vehicle;
        }

        public void SetDestination(double x, double y)
        {
            destX = x;
            destY = y;
        }

        public void Update(double currentX, double currentY, double plannedX, double plannedY, double speed)
        {
            vehicle.Speed = speed;
            PositionChanged?.Invoke(currentX, currentY);

            if (speed > SpeedLimit)
            {
                if (!isSpeeding)
                {
                    isSpeeding = true;
                    SpeedLimitExceeded?.Invoke($"ТРЕКЕР: Перевищення швидкості! {speed:F0} км/год");
                }
            }
            else
            {
                isSpeeding = false;
            }

            double deviation = Math.Abs(currentY - plannedY);
            if (deviation > DeviationTolerance)
            {
                if (!isOffRoute)
                {
                    isOffRoute = true;
                    DeviationFromRoute?.Invoke("ТРЕКЕР: Зафіксовано відхилення від планового треку!");
                }
            }
            else
            {
                isOffRoute = false;
            }

            if (destX > 0 && Math.Abs(currentX - destX) < 5 && Math.Abs(currentY - destY) < 5)
            {
                ArrivedAtDestination?.Invoke();
            }
        }

        public void Reset()
        {
            isOffRoute = false;
            isSpeeding = false;
        }
    }

}
