using System;
using System.Collections.Generic;
using System.Text;

namespace MathLibCS
{
    public class AutomaticalInterpolation
    {
        public static double[] Interpolation(double[] values, 
            int InterpolationCount, bool SmothInterpolation, short InterpolationNeighbourhods)
        {
            double currentValue = 0f;
            double passedValue = 0f;

            for (long i = 0; i < values.Length; i++)
            {
                passedValue = values[i];//Wert  lesen
                if (i == 0 && double.IsNaN(passedValue)) //Falls erster Wert Leerstelle ist
                    while (double.IsNaN(passedValue))
                    {
                        passedValue = values[i];
                        i++;
                        if (i == values.Length)
                            break;
                    }

                if (double.IsNaN(passedValue)) // Leerstelle suchen
                {
                    long s = 0;
                    currentValue = passedValue;
                    passedValue = values[i - 1];
                    //Endpunkt der Lücke suchen und Bereichstest
                    while (double.IsNaN(currentValue) && (i + s) < values.Length)
                    {
                        currentValue = values[i + s]; //Wert Lesen
                        s++; //Länge der Lücke
                    }
                    s -= 1;
                    if (s < InterpolationCount && (i + s) < values.Length)// Test ob Lücke klein genug ist
                    {
                        //Lineare Regression mit Bereichstest
                        if (i > InterpolationNeighbourhods && (i + s) < values.Length - InterpolationNeighbourhods &&
                            SmothInterpolation)
                        {
                            //Anfangswert bestimmen mit angegebenen Nebenstellen
                            double sy = 0;
                            double sxy = 0;
                            short c = 1;
                            double sx = 0;
                            double sx2 = 0;
                            bool empty = false;
                            for (long a = i - InterpolationNeighbourhods; a < i; a++)
                            {
                                double y = values[a];
                                if (double.IsNaN(y)) //Test ob Regression sinnvoll ist
                                    empty = true;
                                sy += y; sxy += c * y; sx += c; sx2 += c * c; c++;
                            }
                            if (!empty) //Extrapolation des nächsten Wertes bei positivem Test
                            {
                                c -= 1;
                                double B = (c * sxy - sx * sy) / (c * sx2 - sx * sx);
                                double A = (sy - B * sx) / c;
                                passedValue = A + B * (c);
                                empty = false;
                            }
                            c = 1; sy = 0; sxy = 0; sx = 0; sx2 = 0;
                            // Endwert bestimmen mit angegebenen Nebenstellen
                            for (long b = i + s; b <= i + s + InterpolationNeighbourhods; b++)
                            {
                                double y = values[b];
                                if (double.IsNaN(y)) //Test ob Regression sinnvoll ist
                                    empty = true;
                                sy += y; sxy += c * y; sx += c; sx2 += c * c; c++;
                            }
                            if (!empty) //Extrapolation des nächsten Wertes bei positivem Test
                            {
                                c -= 1;
                                double B = (c * sxy - sx * sy) / (c * sx2 - sx * sx);
                                double A = (sy - B * sx) / c;
                                currentValue = A;
                                empty = false;
                            }
                        }
                        //Interpolation mit den bestimmten Werten
                        double dValue = (currentValue - passedValue) / (s + 1);
                        for (long inter = i; inter <= (i + s - 1); inter++)
                        {
                            double u = values[inter];
                            double v = passedValue + dValue * (inter - i + 1);
                            values[inter] = v;
                        }
                    }
                    i += s;
                }
            }

            return values;
        }
    }
}
