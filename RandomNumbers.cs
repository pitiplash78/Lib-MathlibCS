using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathLibCS
{
    public class RandomNumber
    {
        private double initValue1 = 0;
        private double initValue2 = 0;

        public RandomNumber(double initValue1, double initValue2)
        {
            this.initValue1 = initValue1;
            this.initValue2 = initValue2;
        }

        public RandomNumber()
        {
            this.initValue1 = DateTime.Now.Ticks;
            this.initValue2 = DateTime.Now.Ticks;
        }

        public double Uniform1()
        {
            double tmp = 201.0 * initValue1 + 100000 / 3.0;
            initValue1 = tmp - (int)tmp;
            return initValue1;
        }

        public double Uniform1(double range)
        {
            double tmp = 201.0 * initValue1 + 100000 / 3.0;
            initValue1 = (tmp - (int)tmp) * range - range / 2.0;
            return initValue1;
        }

        public double Uniform2()
        {
            double tmp = 171.0 * initValue1 + 100000 / 3.0;
            initValue2 = tmp - (int)tmp;
            return initValue2;
        }

        public double Uniform2(double range)
        {
            double tmp = 171.0 * initValue2 + 100000 / 3.0;
            initValue2 = (tmp - (int)tmp) * range - range / 2.0;
            return initValue2;
        }

        public double NormalDistribution(double MeanValue, double variation)
        {
            return variation * Math.Sqrt(-2 * Math.Log(Uniform1(), 2)) * Math.Sin(2 * Math.PI * Uniform2()) + MeanValue;
        }

        public double NormalDistribution(double variation)
        {
            return variation * Math.Sqrt(-2 * Math.Log(Uniform1(), 2)) * Math.Sin(2 * Math.PI * Uniform2());
        }

        public double ExponetialDistribution(double lambda)
        {
            return -(1 / lambda) * Math.Log(Uniform1() + 1e-12, 2);
        }
    }
}
