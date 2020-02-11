using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathLibCS
{
    public class Transform2Channels
    {
        public static bool Transform(ref DoubleArray[] inputArray,
                                     out DoubleArray[] outputArray,
                                     double Azimuth)
        {
            outputArray = null;

            if (inputArray == null)
                return false;
          
            outputArray = new DoubleArray[inputArray.Length];

            int length = 0;
            for (int i = 0; i < inputArray.Length; i++)
            {
                if (i == 0)
                    length = inputArray[i].Length;
                else
                {
                    if (length != inputArray[i].Length)
                        return false;
                }
            }

            /*
             * SN =  X * Math.Sin(azimut) + Y * Math.Cos(azimut);
             * EW = (X * Math.Cos(azimut) - Y * Math.Sin(azimut)) * (-1);
             */

            double sa = Math.Sin(Azimuth);
            double ca = Math.Cos(Azimuth);
            double X = double.NaN;
            double Y = double.NaN;

            double[] X_ = new double[length];
            double[] Y_ = new double[length];

            for (int i = 0; i < length; i++)
            {
                X = inputArray[0][i];
                Y = inputArray[1][i];

                X_[i] = X * ca + Y * sa;
                Y_[i] = -X * sa + Y * ca;
            }

            outputArray[0] = new DoubleArray(X_);
            outputArray[1] = new DoubleArray(Y_);
            
            return true;
        }

    }
}
