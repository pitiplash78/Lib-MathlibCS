using System;
using System.Collections.Generic;
using System.Text;

namespace MathLibCS
{
    public class IntegfrationDifferentiation
    {
        /// <summary>
        /// Numerical integration by summing the values.
        /// </summary>
        /// <param name="data">Double Array containing the data to be integrated.</param>
        /// <param name="meanValue">Will be substracted by the integration.</param>
        /// <param name="sampleRate">Sample rate of the time series.</param>
        /// <param name="startTime">Adjusted start time of the time series.</param>
        /// <returns>The intedrated values as double array.</returns>
        public static double[] integration(double[] data,double  meanValue, TimeSpan sampleRate, ref DateTime startTime)
        {
            double[] values = new double[data.Length - 1];

            double u = data[0];
            double fx = 0d;

            if (!double.IsNaN(u))
                fx += (u - meanValue) * sampleRate.TotalSeconds;

            values[0] = fx;

            int j = 0;

            for (int i = 1; i < data.Length - 1; i++)
            {
                u = data[i];
                if (!double.IsNaN(u))
                {
                    fx += (u - meanValue) * (i - j) * sampleRate.TotalSeconds;
                    values[i] = fx;
                    j = i;
                }
                else
                    values[i] = double.NaN;
            }
            startTime = startTime.AddSeconds(sampleRate.TotalSeconds / 2d);

            return values;
        }
        
        /// <summary>
        /// Numerical differentiation of the time series by using two point. 
        /// </summary>
        /// <param name="data">Double Array containing the data to be differentiated.</param>
        /// <param name="y0">Will be added after differentiation to each point.</param>
        /// <param name="sampleRate">Sample rate of the time series.</param>
        /// <param name="startTime">Adjusted start time of the time series.</param>
        /// <returns>The intedrated values as double array.</returns>
        public static double[] differentation(double[] data, double y0, TimeSpan sampleRate, ref DateTime startTime)
        {
            double[] values = new double[data.Length - 1];

            for (int i = 0; i < data.Length - 1; i++)
            {
                double u = data[i];
                double v = data[i + 1];
                values[i] = ((v - u) / 2d) + y0;
            }
            startTime = startTime.AddSeconds(sampleRate.TotalSeconds / 2d);

            return values;
        }

        /// <summary>
        /// Resampling by linear interpolation to the original time stamps after integration/differentiation.
        /// </summary>
        /// <param name="data">Data to be resampled.</param>
        /// <param name="sampleRate">Sample rate of the time series.</param>
        /// <param name="startTime">Adjusted start time of the time series.</param>
        /// <returns>The intedrated values as double array.</returns>
        public static double[] resample(double[] data, TimeSpan sampleRate, ref DateTime startTime)
        {
            double[] values = new double[data.Length - 1];

            for (int i = 0; i < data.Length - 1; i++)
            {
                double u = data[i];
                double v = data[i + 1];
                values[i] = u + ((v - u) / 2d);
            }

            startTime = startTime.AddSeconds(sampleRate.TotalSeconds / 2);

            return values;
        }
    }
}
