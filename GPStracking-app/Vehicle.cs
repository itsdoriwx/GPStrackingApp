using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPStracking_app
{
        public abstract class Vehicle
        {
            public string Name { get; }
            public double X { get; set; }
            public double Y { get; set; }
            public double Speed { get; set; }

            protected Vehicle(string name, double speed = 0)
            {
                Name = name;
                Speed = speed;
            }
        }
    
}
