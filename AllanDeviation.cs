using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace MathLibCS
{
    public class AllanVariance
    {
        /// <summary>
        /// Enumerator for the selection of method for calculation the tau's
        /// </summary>
        public enum TauCalculation
        {
            none,
            optimizedTau,
            decadeTau,
            octaveTau,
        }

        /// <summary>
        ///  Calculates the AllanVariance.
        /// </summary>
        /// <param name="Values">Double array containing the whole datavalues.</param>
        /// <param name="Samplerate">Samplerate of the Data in seconds.</param>
        /// <param name="progressBar">System.Window.Forms</param>
        /// <returns>Returns a 2d double array (first column: tau [s], second coloumn: according Value)</returns>
        public static double[,] AllanVarianceCalc(double[] Values, int Samplerate, TauCalculation taucalculation, ProgressBar progressBar)
        {
            ArrayList list = new ArrayList(); // liste für; paketgröße / anzahl der pakete / tau

            if (taucalculation == TauCalculation.optimizedTau)
            {
                int max = (Values.Length - (Values.Length % 2) / 2);  // miximale anzahl von rechnungen, wenn paketgröße linear um 1 zunimmt
                list.Add(new long[3] { 1, max, 1 * Samplerate }); // paketgröße 1 --> rechnungsbeginn

                //berechnung der liste für sinnvoll maximal enthaltene paketgrößen
                int prev = 1;
                for (int i = max - 1; i >= 2; i--)
                {
                    double pd = Values.Length / i;
                    int pi = (int)Math.Floor(pd);
                    if (pi > prev)
                    {
                        int member = Values.Length / pi;
                        list.Add(new int[3] { pi, member, pi * Samplerate });
                    }
                    prev = pi;
                }
            }
            else if(taucalculation == TauCalculation.decadeTau)
            {
                int stellen = (int)Math.Ceiling(Math.Log10(Values.Length)) * 3;
                int state = 1;
                double order = 1;
                int pi = 1;
                
                // berechnung der Liste für decadisches tau nach art von 'Stable32'
                for (int i = 0; i < stellen; i++)
                {
                    if (pi / (4 * (int)order) == 1)
                    {
                        order = Math.Pow(10, i / 3);
                        state = 1;
                    }
                    else if (i != 0)
                        state *= 2;

                    pi = state * (int)order;
                    int member = Values.Length / pi;
                    if (member >= 4)
                        list.Add(new int[3] { pi, member, pi * Samplerate });
                    else
                        break;

                }
            }
            else if (taucalculation == TauCalculation.octaveTau)
            {
                int pi = 1;
                int member = Values.Length / pi;

                // berechnung der Liste für octavem tau nach art von 'Stable32'
                while (member >= 4)
                {
                    list.Add(new int[3] { pi, member, pi * Samplerate });
                    pi = pi << 1;
                    member = Values.Length / pi;
                }
            }
            else
            {
                // --> ERROR
            }

            int length = list.Count - 1;
            progressBar.Maximum = length;

            // anlegen von rückgabearray mit [zeit [s], value, std]
            double[,] ret = new double[length, 3];

            // für paketgröße = 1 --> sonderbehandlung == geht schneller
            double meandiff = 0;
            for (long i = 0; i < Values.Length-1; i++)
                meandiff += Math.Pow(Values[i + 1] - Values[i], 2); // summe über differenzen ins quadrat

            //  Berechung der Werte 
            ret[0, 0] = Samplerate; // tau
            ret[0, 1] = Math.Sqrt(0.5f * (meandiff / (double)(Values.Length - 1))); // Allan deviation über pakete
            ret[0, 2] = ret[0, 1] / Math.Sqrt(Values.Length);// Fehlerabschätzung

            progressBar.Value = 1;

            for (int j = 1; j < length; j++)
            {
                int p = ((int[])list[j])[0]; // paketgröe
                int dim = ((int[])list[j])[1]; // anzahl der pakete
                double[] D = new double[dim]; // für bestimmung der mittelwerte der pakete

                for (int i = 0; i < D.Length; i++) // päcken bestimmen und mittelwert bilden
                {
                    for (int c = p * i; c < p * i + p; c++)
                        D[i] += Values[c]; // summe der einzelpakete
                    D[i] /= p; // mittelwerte der pakete
                }

                meandiff = 0;
                for (int i = 0; i < D.Length - 1; i++)
                    meandiff += Math.Pow(D[i + 1] - D[i], 2); // summe über differenzen ins quadrat

                //  Berechung der Werte 
                ret[j, 0] = ((int[])list[j])[2]; //tau
                ret[j, 1] = Math.Sqrt(0.5f * (meandiff / (double)(D.Length - 1))); // allan deviation
                ret[j, 2] = ret[j, 1] / Math.Sqrt(D.Length);// Math.Sqrt(meandiffst);// Standardabweichung 

                progressBar.Value = j + 1;
            }
            progressBar.Value = 0;

            return ret;
        }
    }
}
