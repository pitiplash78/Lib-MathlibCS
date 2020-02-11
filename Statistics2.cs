using System;


namespace MathLibCS
{
    public class Statistic2
    {
        //    double[] x = new double[abtastwerte];

        private int count = 0;
        public double miWert1 = 0;
        public double miWert2 = 0;
        public double median = 0;
        public double spannw = 0;
        public double delta = 0;
        public double streu1 = 0;
        public double streu2 = 0;
        public double varianz1 = 0;
        public double varianz2 = 0;
        public double schiefe = 0;
        public double kurtosis = 0;


        public Statistic2()
        { }


        public void CalculateStatistics(ref double[] x)
        {
            count = 0;

            //Mittelwert
            miWert1 = 0;
            for (int i = 0; i < x.Length; i++)
            {
                if (!double.IsNaN(x[i]))
                {
                    miWert1 += x[i];
                    count++;
                }
            }
            miWert1 /= count;

            // median
            double[] sort = new double[count];
            median = Median(count, x, out sort);

            // Spannweite
            spannw = sort[count - 1] - sort[0];

            //mittlere absolute Abweichung vom Mittelwert
            delta = 0;
            for (int i = 0; i < x.Length; i++)
                if (!double.IsNaN(x[i]))
                    delta += Math.Abs(x[i] - miWert1);
            delta /= count;

            if (count > 1)
            {
                varianz1 = 0;
                varianz2 = 0;
                schiefe = 0;
                kurtosis = 0;
                for (int i = 0; i < x.Length; i++)
                {
                    if (!double.IsNaN(x[i]))
                    {
                        varianz1 += (x[i] - miWert1) * (x[i] - miWert1);
                        varianz2 += x[i] * x[i];
                        schiefe += Math.Pow(x[i] - miWert1, 3);
                        kurtosis += Math.Pow(x[i] - miWert1, 4);
                    }
                }
                varianz1 /= (count - 1);
                varianz2 /= (count - 1);

                // Streuung
                streu1 = Math.Sqrt(varianz1);
                streu2 = Math.Sqrt(varianz2);

                schiefe /= count * Math.Pow(streu1, 3);
                kurtosis /= count * Math.Pow(streu1, 4);
            }
        }

        private double Median(int k, double[] x, out double[] sort)
        {
            double median = 0;
            sort = new double[x.Length];

            for (int i = 0; i < x.Length; i++)
                sort[i] = x[i];

            QuickSort(0, k - 1, ref sort);

            if (k % 2 == 0)
                median = (sort[k / 2 - 1] + sort[k / 2]) / 2.0;
            else
                median = sort[(k - 1) / 2];

            return median;
        }

        private void QuickSort(int start, int stop, ref double[] sort)
        {
            int a = start;
            int b = stop;

            double u = sort[(int)((a + b) / 2.0)];

            do
            {
                while (sort[a] < u) a++;
                while (sort[b] > u) b--;

                if (a <= b)
                {
                    double tmp = sort[a];
                    sort[a] = sort[b];
                    sort[b] = tmp;
                    a++;
                    b--;
                }
            } while (a <= b);

            if (start < b) QuickSort(start, b, ref sort);
            if (stop > a) QuickSort(a, stop, ref sort);
        }
    }
}
