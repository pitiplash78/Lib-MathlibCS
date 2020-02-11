using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Windows.Forms;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

namespace MathLibCS
{
    public class Filter
    {
        // Creation of Filter Elements for FIR-Filters
        /// <summary>
        /// Calculates FIR LowPass Filter Coefficients.
        /// </summary>
        /// <param name="coefficients">Coeffiecient Array which is returned.</param>
        /// <param name="samplrate">Samplerate of the origin time series in seconds.</param>
        /// <param name="CutOffFrequency">Cutofffrequency in Hz.</param>
        /// <param name="pbar">Forms.Progressbar</param>
        public static void calcFilterOp_LowPass(ref double[] coefficients, double samplrate, double CutOffFrequency, ProgressBar pbar)
        {
            double b = Math.PI * 2d * CutOffFrequency;
            int l2 = (coefficients.Length - 1) / 2;
            coefficients[l2] += (b / Math.PI);

            for (double i = 1d, T = samplrate; i <= (double)l2; i++, T += samplrate)
            {
                double tmp = Math.Sin(b * T) / (Math.PI * T);
                coefficients[l2 - (int)i] += tmp;
                coefficients[l2 + (int)i] += tmp;
                pbar.Value += 2;
            }
        }

        /// <summary>
        /// Calculates FIR HighPass Filter Coefficients.
        /// </summary>
        /// <param name="coefficients">Coeffiecient Array which is returned.</param>
        /// <param name="samplrate">Samplerate of the origin time series in seconds.</param>
        /// <param name="CutOffFrequency">Cutofffrequency in Hz.</param>
        /// <param name="pbar">Forms.Progressbar</param>
        public static void calcFilterOp_HighPass(ref double[] coefficients, double samplrate, double CutOffFrequency, ProgressBar pbar)
        {
            double b = Math.PI * 2d * CutOffFrequency;
            int l2 = (coefficients.Length - 1) / 2;
            coefficients[l2] += 1d + (b / Math.PI);

            for (double i = 1d, T = samplrate; i <= (double)l2; i++, T += samplrate)
            {
                double tmp = -(Math.Sin(b * T) / (Math.PI * T));
                coefficients[l2 - (int)i] += tmp;
                coefficients[l2 + (int)i] += tmp;
                pbar.Value += 2;
            }
        }

        /// <summary>
        /// Calculates FIR BandPass Filter Coefficients.
        /// </summary>
        /// <param name="_operator">Coeffiecient Array which is returned.</param>
        /// <param name="samplrate">Samplerate of the origin time series in seconds.</param>
        /// <param name="CutOffLow">Lower Cutofffrequency in Hz.</param>
        /// <param name="CutOffHigh">Higher Cutofffrequency in Hz.</param>
        /// <param name="pbar">Forms.Progressbar</param>
        public static void calcFilterOp_BandPass(ref double[] _operator, double samplrate, double CutOffLow, double CutOffHigh, ProgressBar pbar)
        {
            double a = Math.PI * 2 * CutOffLow;
            double b = Math.PI * 2 * CutOffHigh;
            int l2 = (_operator.Length - 1) / 2;
            _operator[l2] += ((a - b) / Math.PI);

            for (double i = 1d, T = samplrate; i <= l2; i++, T += samplrate)
            {
                double tmp = (Math.Sin(a * T) - Math.Sin(b * T)) / (Math.PI * T);
                _operator[l2 - (int)i] += tmp;
                _operator[l2 + (int)i] += tmp;
                pbar.Value += 2;
            }
        }

        /// <summary>
        /// Calculates FIR BandStop Filter Coefficients.
        /// </summary>
        /// <param name="_operator">Coeffiecient Array which is returned.</param>
        /// <param name="samplrate">Samplerate of the origin time series in seconds.</param>
        /// <param name="CutOffLow">Lower Cutofffrequency in Hz.</param>
        /// <param name="CutOffHigh">Higher Cutofffrequency in Hz.</param>
        /// <param name="pbar">Forms.Progressbar</param>
        public static void calcFilterOp_BandStopp(ref double[] _operator, double samplrate, double CutOffLow, double CutOffHigh, ProgressBar pbar)
        {
            double a = Math.PI * 2 * CutOffLow;
            double b = Math.PI * 2 * CutOffHigh;
            int l2 = (_operator.Length - 1) / 2;
            _operator[l2] += (1d + ((a - b) / Math.PI));

            for (double i = 1d, T = samplrate; i <= l2; i++, T += samplrate)
            {
                double tmp = (Math.Sin(a * T) - Math.Sin(b * T)) / (Math.PI * T);
                _operator[l2 - (int)i] += tmp;
                _operator[l2 + (int)i] += tmp;
                pbar.Value += 2;
            }
        }

        public static void calcFilterOp_FlankeLowPass(ref double[] coefficients, double samplrate, double CutOffFrequency, double delta, ProgressBar pbar)
        {
            int l2 = (coefficients.Length - 1) / 2;
            coefficients[l2] += (-delta * Math.PI);

            double F1 = CutOffFrequency + delta;

            for (double i = 1d, T = samplrate; i <= (double)l2; i++, T += samplrate)
            {
                double F2 = CutOffFrequency * 2d * Math.PI * T;
                double F3 = 1d / (delta * 3 * Math.PI * T);
                double F4 = F1 * T;

                double tmp = F3 / T * (Math.Cos(F2) - Math.Cos(F4)) - 1d / (Math.PI * T) * Math.Sin(F2);
                coefficients[l2 - (int)i] += tmp;
                coefficients[l2 + (int)i] += tmp;
                pbar.Value += 2;
            }
        }

        // Calculation of Frequency Response of an Filteroperator

        /// <summary>
        /// Normalized Spectrum of a Filteroperator
        /// </summary>
        /// <param name="inputdata">Filtercoeffisients</param>
        /// <param name="sampleRate">Samplerate of origin data.</param>
        /// <param name="pbar">Forms.Prgogressbar</param>
        /// <param name="FreqReturn">Calculated Frequency and,..</param>
        /// <param name="AmpReturn">calculated Amplitude Response of the Filter.</param>
        public static void normalizedFrequencyResponse(double[] inputdata, double sampleRate, ProgressBar pbar, ref double[] FreqReturn, ref double[] AmpReturn)
        {
            double dF = 1d / ((double)inputdata.Length);                 // Distance between the frequency values
            int NF = (int)(0.5d * inputdata.Length);                    // number of frequency values
            double dAng = 2d * Math.PI / (double)inputdata.Length;       // step size of the angle
            double ang = 0d * dAng;          // start value for the angle
            double[] Ak = new double[NF + 1];             // Fourier coefficients of the cosine terms
            double[] Bk = new double[NF + 1];             // Fourier coefficients of the sine terms
            double[] F = new double[NF + 1];              // frequency values

            for (long k = 0; k <= NF; k++)
            {
                double cos = Math.Cos(ang);
                double sin = Math.Sin(ang);

                double A = 0d;
                double B = 0d;

                for (long i = 0; i < inputdata.Length; i++)
                {
                    double tmpA = A;
                    double tmpB = B;
                    A = cos * tmpA - sin * tmpB + inputdata[(inputdata.Length - 1) - i];
                    B = sin * tmpA + cos * tmpB;
                }
                F[k] = (double)k * dF / sampleRate;
                Ak[k] = A * 2d / (double)inputdata.Length;
                Bk[k] = B * 2d / (double)inputdata.Length;

                ang += dAng;
                pbar.Value += 1;
            }

            FreqReturn = new double[NF];
            AmpReturn = new double[NF];

            double min = double.MaxValue;
            double max = double.MinValue;

            for (long k = 0; k < NF; k++)
            {
                long l = NF - k;
                FreqReturn[k] = 1d / F[l];

                double amptmp = Math.Sqrt(Math.Pow(Ak[l], 2) + Math.Pow(Bk[l], 2));  // amplitudes
                min = Math.Min(min, amptmp);
                max = Math.Max(max, amptmp);
                AmpReturn[k] = amptmp;
            }

            // amplitude auf 0-1 normalisieren
            double fac = 1d / (max - min);
            for (int k = 0; k < NF; k++)
                AmpReturn[k] = (AmpReturn[k] - min) * fac;
        }


        // FilterOperator class with all needed Information and Filtering

        /// <summary>
        /// class containing all FIR Filteroperator information.
        /// </summary>
        [XmlRoot("FilterOperator")]
        public class FilterOperator
        {
            [XmlElement("FilterLength")]
            public int Filterlength = 303;

            [XmlElement("SampleRate")]
            public int SampleRate;

            [XmlElement("SampleRateOut")]
            public int SampleRateOut = 60;

            [XmlElement("MaskedValues")]
            public int MaskedValues = 0;

            //[XmlElement("FilterData")]
            private ArrayList FilterData;

            [XmlElement("Comment")]
            public string Comment = null;

            public FilterOperator()
            {
                FilterData = new ArrayList();
            }

            [XmlElement("FilterEntry")]
            public FilterEntry[] FilterEntryItems
            {
                get
                {
                    FilterEntry[] items = new FilterEntry[FilterData.Count];
                    FilterData.CopyTo(items);
                    return items;
                }
                set
                {
                    if (value == null) return;
                    FilterEntry[] items = (FilterEntry[])value;
                    FilterData.Clear();
                    foreach (FilterEntry item in items)
                        FilterData.Add(item);
                }
            }

            public int AddFilterEntry(FilterEntry item)
            {
                return FilterData.Add(item);
            }
            //public void AddFilterEntryAt(int i, FilterEntry item)
            //{
            //    FilterData.Insert(i, item);
            //}
            public void RemoveFilterEntryAt(int i)
            {
                FilterData.RemoveAt(i);
            }

            [XmlElement("Window")]
            public Windowing.windowFunction Window = Windowing.windowFunction.none;

            [XmlElement("Coefficients")]
            public double[] Coefficients = null;

            [XmlElement("SpectralResponseFreq")]
            public double[] SpectralResponseFreq = null;
            [XmlElement("SpectralResponseGain")]
            public double[] SpectralResponseGain = null;
        }
        /// <summary>
        /// class (sub-) for the Filteroperator class containing the Filterelement information.
        /// </summary>
        public class FilterEntry
        {
            [XmlElement("filterFunction")]
            public FilterFunction filterfunction;
            [XmlElement("LowFrequency")]
            public double LowFrequency = double.NaN;
            [XmlElement("HighFrequency")]
            public double HighFrequency = double.NaN;

            public FilterEntry()
            { }

            public FilterEntry(FilterFunction _filterfunction, double _LowFrequency, double _HighFrequency)
            {
                this.filterfunction = _filterfunction;
                this.LowFrequency = _LowFrequency;
                this.HighFrequency = _HighFrequency;
            }
        }
        /// <summary>
        /// enum class containing the Filterelement members.
        /// </summary>
        public enum FilterFunction
        {
            [XmlEnum(Name = "LowPass")]
            LowPass,
            [XmlEnum(Name = "HighPass")]
            HighPass,
            [XmlEnum(Name = "BandPass")]
            BandPass,
            [XmlEnum(Name = "BandStop")]
            BandStop,
            [XmlEnum(Name = "FlankLP")]
            FlankLP,
        }

        /// <summary>
        /// Filtering of time series by FIR-operator.
        /// </summary>
        /// <param name="sender">sender object of the backgroundworker</param>
        /// <param name="e">DoWorkEventArgs of the backgroundworker</param>
        /// <param name="Values">Values, which should be filtered.</param>
        /// <param name="StartTime">Start time of the 'Values', which should be filtered.</param>
        /// <param name="SampleRate">Samplerate of the 'Values', which should be filtered.</param>
        /// <param name="FirstIndex">Index, where the filtering should begin. Determined with 'bool GetFirstTimeIndex(ref int Start...)'</param>
        /// <param name="ValuesFiltered">Returns the filtered values.</param>
        /// <param name="ValuesResidual">Returns the residuals of the filtered values.</param>
        /// <param name="StartTimeNew">Returns the start time of the filtered values.</param>
        /// <param name="SampleRateNew">Returns the samplerate of the filtered values.</param>
        /// <param name="Mirroring">Mirroring (true) of the values at the beginning, end, and gaps.</param>
        /// <param name="Filter">'FilterOperator' (class) of the used filter.</param>
        /// <param name="progressbar">Sysem.Windows.Forms</param>
        /// <param name="partsToFilter">Gives the number of time series to be filtered in one step.</param>
        /// <param name="runOfFiltering">Gives the number of the time series, which is currently filtered.</param>
        /// <returns>Success of filtering (true)</returns>
        public static bool filternbyFIRoperator(
                ref object sender,ref System.ComponentModel.DoWorkEventArgs e,
                double[] Values, DateTime StartTime, TimeSpan SampleRate, int FirstIndex,
                out double[] ValuesFiltered, out double[] ValuesResidual,
                out DateTime StartTimeNew, out TimeSpan SampleRateNew,
                bool Mirroring,
                FilterOperator Filter,
                int partsToFilter, int runOfFiltering)
        {
            {  // Initialisierung
                int LengthValuesFilteres = (int)Math.Ceiling((double)(Values.Length - FirstIndex) /
                                                             (double)(Filter.SampleRateOut /
                                                                      (int)SampleRate.TotalSeconds));
                ValuesFiltered = new double[LengthValuesFilteres];
                ValuesResidual = new double[LengthValuesFilteres];

                StartTimeNew = StartTime.AddSeconds(FirstIndex * SampleRate.TotalSeconds);
                SampleRateNew = new TimeSpan(0, 0, Filter.SampleRateOut);
            }

            // Längenstest, ob gefiltert werden kann
            if (Filter.Coefficients.Length >= Values.Length - FirstIndex)
            {
                MessageBox.Show("Filtering of the time series imposible. Time series is to short!",
                    "PreAnalyseExtended", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Filterlängehalbe
            int FL2 = (Filter.Coefficients.Length - 1) / 2;

            // Maskierparameter
            int gca = 0;
            int gcb = FL2 / (Filter.SampleRateOut / (int)SampleRate.TotalSeconds) + 1;

            // Laufvariable zu filternde Werte
            int index = FirstIndex; 

            int probressbaroffset = (int)Math.Floor((double)Values.Length/(double)partsToFilter*(double)runOfFiltering);

            while (index < Values.Length)
            {
                if (((System.ComponentModel.BackgroundWorker)sender).CancellationPending)
                {
                    e.Cancel = true;
                    return false;
                }

                double fv = double.NaN; // gefilterter Wert

                bool gg = true;  // Wert auf welchen Zeitpunkt gerade gefiltert wird ist keine Lücke
                bool ga = false; // Lückenmarker
                bool gb = false; // Lückenmarker
                int j = 0; // untere Werteindex für Filteroperator
                int k = 0; // oberer Werteindex für Filteroperator

                if (index >= FL2 && index + FL2 < Values.Length) // normale Berechnung mit test auf Lücke
                {
                    for (int i = 0; i <= FL2; i++)
                    {
                        if (i == 0)
                        {
                            if (double.IsNaN(Values[index]))
                            {
                                gg = false;
                                break; // Schleife beenden für schnelleren Prozess
                            }
                            else
                                fv = Values[index] * Filter.Coefficients[FL2];
                        }
                        else
                        {
                            j = index - i; // untere Werteindex für Filteroperator
                            k = index + i; // oberer Werteindex für Filteroperator

                            if (double.IsNaN(Values[j]))
                                ga = true; // Filteroperator rutscht aus Lücke raus für Rückwärstspiegelung
                            if (double.IsNaN(Values[k]))
                                gb = true; // Filteroperator rutscht in Lücke rein für Vorwärstspiegelung
                            if (!gg && ga && gb)
                                break; // Schleife beenden für schnelleren Prozess
                            fv += (Values[j] + Values[k]) * Filter.Coefficients[FL2 - i];
                        }
                    }
                }

                // Behandlung von ausnahmen: Anfang Daten, Lücken, Ende Daten
                if ((index < FL2 && Mirroring) ||  // Daten Anfang
                    (Mirroring && ga && !gb && gg)) // Anfang Lückenfrei --> Rückwärtsspiegeln
                {
                    bool mirror = false; // Flag, das gespiegelt wird
                    int l = 0; // Shiftindex für Spiegelung
                    int m = 1; // Laufindex für Spiegelung

                    for (int i = 0; i <= FL2; i++)
                    {
                        if (i == 0)
                        {
                            if (double.IsNaN(Values[index])) // Trifft nur für Daten am Anfang zu
                            {
                                gg = false;
                                break; // Schleife beenden für schnelleren Prozess
                            }
                            else
                                fv = Values[index] * Filter.Coefficients[FL2];
                        }
                        else
                        {
                            // Indezies ohne ausnahme
                            j = index - i; // untere Werteindex für Filteroperator
                            k = index + i; // oberer Werteindex für Filteroperator

                            if (!mirror && (j >= 0) && double.IsNaN(Values[j]))
                            {
                                l = i - 1; // Shift Variable für Spiegelung im Datensatz
                                mirror = true;
                                // Test ob Filteroperator nach Lücke komplett bis zur nächsten anwendbar
                                for (int n = 0; n <= FL2; n++)
                                    if (double.IsNaN(Values[j + FL2 + 1 + n]))
                                    {
                                        gg = false;
                                        break;
                                    }
                            }
                            else if (!mirror && j < 0)
                            {
                                j = m; // Spiegelung am Anfang wenn Value[0] != NaN
                                m++; // Laufindex für Spiegelung
                            }
                            if (mirror)
                            {
                                j = index - l + m; // Berechnung Spiegelung 
                                m++; // Laufindex für Spiegelung
                            }
                            fv += (Values[j] + Values[k]) * Filter.Coefficients[FL2 - i];
                        }
                    }
                    ga = true; // Marker nur für Anfang, sonst schon gesetz 
                }
                else if ((index + FL2 >= Values.Length && Mirroring) || // Daten Ende
                         (Mirroring && !ga && gb && gg)) // Ende Lückenfrei --> Vorwärtsspiegeln
                {
                    bool mirror = false; // Flag, das gespiegelt wird
                    int l = 0; // Shiftindex für Spiegelung
                    int m = 1; // Laufindex für Spiegelung
                    for (int i = 0; i <= FL2; i++)
                    {
                        if (i == 0)
                        {
                            if (double.IsNaN(Values[index])) // Trifft nur für Daten am Ende zu
                            {
                                gg = false;
                                break; // Schleife beenden für schnelleren Prozess
                            }
                            else
                                fv = Values[index] * Filter.Coefficients[FL2];
                        }
                        else
                        {
                            // Indezies ohne ausnahme
                            j = index - i; // untere Werteindex für Filteroperator
                            k = index + i; // oberer Werteindex für Filteroperator

                            if (!mirror && (k < Values.Length) && double.IsNaN(Values[k]))
                            {
                                l = i - 1; // Shift Variable für Spiegelung im Datensatz
                                mirror = true;
                                // Test ob Filteroperator nach Lücke komplett bis zur nächsten anwendbar
                                for (int n = 0; n <= FL2; n++)
                                    if (double.IsNaN(Values[k - FL2 - 1 - n]))
                                    {
                                        gg = false;
                                        break;
                                    }
                            }
                            else if (!mirror && k >= Values.Length)
                            {
                                m++; // Laufindex für Spiegelung
                                k = Values.Length - m;
                            }
                            if (mirror)
                            {
                                k = index + l - m; // Berechnung Spiegelung 
                                m++;  // Laufindex für Spiegelung
                            }
                            fv += (Values[j] + Values[k]) * Filter.Coefficients[FL2 - i];
                        }
                    }
                    gb = true; // Marker nur für Ende, sonst schon gesetz 
                }

                // für zusätzlich Lücke im Bereich der Filterlänge
                if (!gg || (ga && gb))
                    fv = double.NaN;

                // Maskieren Anfang Lückenfrei
                if (ga && !gb && gg)
                    gca++; // Hochzählen für Markercounter
                if (ga && !gb && gg && gca <= Filter.MaskedValues)
                    fv = double.NaN; // Maskieren von gefiltertem Wert
                if ((ga && gb || !gg) || !ga && !gb && gca >= Filter.MaskedValues)
                    gca = 0; // Zurücksetzen von Markercounter

                // Maskieren Ende Lückenfrei
                if (!ga && gb && gg)
                    gcb--; // Runterzählen für Markercounter
                if (!ga && gb && gcb <= Filter.MaskedValues)
                    fv = double.NaN; // Maskieren von gefiltertem Wert
                if (!ga && !gb && gcb <= 1) 
                    gcb = FL2 / (Filter.SampleRateOut / (int)SampleRate.TotalSeconds) + 1; // Zurücksetzen von Markercounter

                // Gefilterte Werte in Arrays legen
                int indexValuesFiltered = (index - FirstIndex) / (Filter.SampleRateOut / (int)SampleRate.TotalSeconds);
                ValuesFiltered[indexValuesFiltered] = fv;
                ValuesResidual[indexValuesFiltered] = Values[index] - fv;

                // Index verschieben für Filterzeitpunkt
                index += Filter.SampleRateOut / (int)SampleRate.TotalSeconds;
                int p = probressbaroffset + (int)Math.Floor((double)index / (double)partsToFilter);
                ((System.ComponentModel.BackgroundWorker)sender).ReportProgress(p);
            }

            return true;
        }

        /// <summary>
        /// Determines the first possible Index to filter on an adequate Starttime of filtered time series.
        /// </summary>
        /// <param name="FirstIndex">returns the determined first index </param>
        /// <param name="StartTime">Start time of the time series, which should be filtered.</param>
        /// <param name="SampleRate">Samplerate of the time series, which should be filtered.</param>
        /// <param name="Filter">'FilterOperator' (class) of the used filter.</param>
        /// <returns>True (false) for (not) determined an adequate Starttime of filtered time series.</returns>
        public static bool GetFirstTimeIndex(ref int FirstIndex, DateTime StartTime, TimeSpan SampleRate,
                                             FilterOperator Filter)
        {
            if ((Filter.SampleRateOut <= 3600) &&
                ((Filter.SampleRateOut * TimeSpan.TicksPerSecond) % SampleRate.Ticks == 0) &&
                ((int)StartTime.TimeOfDay.TotalSeconds % (int)SampleRate.TotalSeconds) == 0)
            {
                if (StartTime.Ticks % TimeSpan.TicksPerHour != 0)
                {
                    DateTime stnew = new DateTime(StartTime.Year, StartTime.Month, StartTime.Day, StartTime.Hour, 0, 0) +
                                           new TimeSpan(1, 0, 0);

                    FirstIndex = (int)(stnew - StartTime).TotalSeconds / (int)SampleRate.TotalSeconds;

                    return false;
                }
                else
                {
                    return true;
                }
            }
            return true;
        }
        
        /// <summary>
        /// Serialization of the Filteroperator class to an XML file.
        /// </summary>
        /// <param name="data">FilterOperator which has to be serializated</param>
        /// <param name="dataPath">Path of the XML file.</param>
        public static void XML_FilterOperator_Serialisieren(FilterOperator data, string dataPath)
        {
            XmlSerializer s = new XmlSerializer(typeof(FilterOperator));
            TextWriter w = new StreamWriter(dataPath);
            s.Serialize(w, data);
            w.Close();
        }

        /// <summary>
        /// Derialization of the Filteroperator class From an XML file.
        /// </summary>
        /// <param name="dataPath">Path of the XML file.</param>
        /// <returns>class FilterOperator </returns>
        public static FilterOperator XML_FilterOperator_Deserialisieren(string dataPath)
        {
            XmlSerializer s = new XmlSerializer(typeof(Filter.FilterOperator));
            Filter.FilterOperator filterop = new Filter.FilterOperator();
            TextReader r = new StreamReader(dataPath);
            filterop = (FilterOperator)s.Deserialize(r);
            r.Close();
            return filterop;
        }
    }
}
