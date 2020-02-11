using System;
using System.Collections.Generic;
using System.Text;

namespace MathLibCS
{
    public class Statistics
    {
        public static double meanValue(double[] data)
        {
            double mw = 0d; // Mittelwert
            int c = 0;

            double u = 0d;
            for (int i = 0; i < data.Length; i++)
            {
                u = data[i];
                if (!double.IsNaN(u))
                {
                    mw += u;
                    c++;
                }
            }
            mw /= (double)c;

            return mw;
        }

        public static double[] meanValue(double[,] data)
        {
            double[] mw = new double[data.GetLength(1)]; // Mittelwert
            for (int j = 0; j < data.GetLength(1); j++)
            {
                int c = 0;

                double u = 0d;
                for (int i = 0; i < data.Length; i++)
                {
                    u = data[i,j];
                    if (!double.IsNaN(u))
                    {
                        mw[j] += u;
                        c++; 
                    }
                }
                mw[j] /= (double)c;
            }
            return mw;
        }
    }
}
