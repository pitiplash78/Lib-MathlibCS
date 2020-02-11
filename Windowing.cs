using System;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace MathLibCS
{
    public class Windowing
    {
        /// <summary>
        /// Data Window application on a time series.
        /// </summary>
        /// <param name="data">Data to applicate the Window</param>
        /// <param name="_window">class windowfuction member</param>
        /// <param name="pbar">Forms.Prgogressbar</param>
        public static double[] Window(double[] data, windowFunction _window, ProgressBar pbar)
        {

            if (_window == windowFunction.uniform || _window == windowFunction.none)
            {
                pbar.Value += (data.Length - 1);
                return data;
            }
            int l2 = (data.Length - 1) / 2;
            int l22 = l2 + l2;
            int M1 = l2 + 1;
            int M2 = l2 - 1;

            data[0] = 0d;
            data[data.Length - 1] = 0d;
            double F = 0d;

            for (int i = l2; i < l22; i++)
            {
                double t = (l2 - (double)i) / l2;

                if (_window == windowFunction.Barlett)
                    F = 1 + t; // 1- math.abs(t);
                if (_window == windowFunction.TurkeyHann)
                    F = 0.5d * (1d + Math.Cos(Math.PI * t));
                if (_window == windowFunction.Hamming)
                    F = 0.54d + 0.46d * Math.Cos(Math.PI * t);

                data[l2 + (i - l2)] *= F;
                data[l2 - (i - l2)] *= F;
                pbar.Value += 2;
            }
            return data;
        }

        /// <summary>
        /// Data Window application on a time series.
        /// </summary>
        /// <param name="index">Position for Calculation the window values </param>
        /// <param name="datalenght">Length of the Data array, where should be applied the window</param>
        /// <param name="_window">class windowfuction member</param>
        /// <param name="gaussA">Parameter for the Gauss window function if used</param>
        /// <returns>The calculated Window Value, for given position.</returns>
        public static double WindowAtIndex(int index, int datalenght, windowFunction _window, double gaussA)
        {
            double win = 1;         // weight factor according to the data                   

            if (_window == windowFunction.uniform || _window == windowFunction.none)
                win = 1d;
            else if (_window == windowFunction.Barlett)
                win = 1d - Math.Abs((2d * (double)index / datalenght) - 1d);
            else if (_window == windowFunction.TurkeyHann)
                win = 0.5d + 0.5d * Math.Cos(2d * Math.PI * (((double)index / (double)datalenght) - 0.5d));
            else if (_window == windowFunction.Hamming)
                win = 0.54d + 0.46d * Math.Cos(2d * Math.PI * (((double)index / (double)datalenght) - 0.5d));
            else if (_window == windowFunction.Gauss)
                win = Math.Exp(-0.5d * Math.Pow(2d * gaussA * (((double)index / (double)datalenght) - 0.5d), 2d));

            return win;
        }

        /// <summary>
        /// Compensates the Energy on Fourier analysis after applying a Data window
        /// </summary>
        /// <param name="_window">class windowfuction member</param>
        /// <param name="DataLength">Length of the Data array, where should be applied the window enerdy decompensation.</param>
        /// <param name="GaussA">Parameter for the Gauss window function if used</param>
        /// <returns>The calculated Energy lost for decompensation.</returns>
        public static double WindowEnergyLostDecompensation(windowFunction _window, int DataLength, double GaussA )
        {
            double fac = 1d;                                                  // factor

            if (_window == windowFunction.Barlett)
                fac = 2d;
            else if (_window == windowFunction.TurkeyHann)
                fac = 2d;
            else if (_window == windowFunction.Hamming)
            {
                double win = 1d;
                for (double i = 0; i < (double)DataLength; i++)
                {
                    win = 0.54d + 0.46d * Math.Cos(2d * Math.PI * ((i / ((double)DataLength - 1d)) - 0.5d));
                    fac += win;
                }
                fac = ((double)DataLength - 1d) / fac;
            }
            else if (_window == windowFunction.Gauss)
            {
                double win = 1d;                
                for (double i = 0; i < (double)DataLength; i++)
                {
                    win = Math.Exp(-0.5d * Math.Pow(2d * GaussA * ((i / ((double)DataLength - 1d)) - 0.5d), 2d));
                    fac += win;
                }
                fac = ((double)DataLength - 1d) / fac;
            }
            else
                fac = 1;

            return fac;
        }

        /// <summary>
        /// Data window enum class for application and control of windowing.
        /// </summary>
        public enum windowFunction
        {
            [XmlEnum(Name = "none")]
            none,
            [XmlEnum(Name = "uniform")]
            uniform,
            [XmlEnum(Name = "Barlett")]
            Barlett,
            [XmlEnum(Name = "TurkeyHann")]
            TurkeyHann,
            [XmlEnum(Name = "Hamming")]
            Hamming,
            [XmlEnum(Name = "Gauss")]
            Gauss,
        }
    }
}
