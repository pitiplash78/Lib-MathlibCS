using System;
using System.Collections.Generic;
using System.Text;

namespace MathLibCS
{
    public class Correlation
    {
        public static double PearsonCorrelate(ref double[] x, ref double[] y, int n)
        {
            double result = 0;
            int i = 0;
            double xmean = 0;
            double ymean = 0;
            double s = 0;
            double xv = 0;
            double yv = 0;

            double t1 = 0;
            double t2 = 0;
            xv = 0;
            yv = 0;
            if (n <= 1)
            {
                result = 0;
                return result;
            }

            xmean = 0;
            ymean = 0;
            for (i = 0; i <= n - 1; i++)
            {
                xmean = xmean + x[i];
                ymean = ymean + y[i];
            }
            xmean = xmean / n;
            ymean = ymean / n;

            s = 0;
            xv = 0;
            yv = 0;
            for (i = 0; i <= n - 1; i++)
            {
                t1 = x[i] - xmean;
                t2 = y[i] - ymean;
                xv = xv + (t1 * t1);
                yv = yv + (t2 * t2);
                s = s + t1 * t2;
            }
            if (xv == 0 | yv == 0)
            {
                result = 0;
            }
            else
            {
                result = s / (Math.Sqrt(xv) * Math.Sqrt(yv));

            }
            return result;
        }
        public static double Spearmanrankcorrelate(double[] x, double[] y, int n)
        {
            double result = 0;

            x = (double[])x.Clone();
            y = (double[])y.Clone();

            rankx(ref x, n);
            rankx(ref y, n);
            result = PearsonCorrelate(ref x, ref y, n);
            return result;
        }
        private static void rankx(ref double[] x, int n)
        {
            int i = 0;
            int j = 0;
            int k = 0;
            int t = 0;
            double tmp = 0;
            int tmpi = 0;
            double[] r = new double[0];
            int[] c = new int[0];


            //
            // Prepare
            //
            if (n <= 1)
            {
                x[0] = 1;
                return;
            }
            r = new double[n - 1 + 1];
            c = new int[n - 1 + 1];
            for (i = 0; i <= n - 1; i++)
            {
                r[i] = x[i];
                c[i] = i;
            }

            //
            // sort {R, C}
            //
            if (n != 1)
            {
                i = 2;
                do
                {
                    t = i;
                    while (t != 1)
                    {
                        k = t / 2;
                        if (r[k - 1] >= r[t - 1])
                        {
                            t = 1;
                        }
                        else
                        {
                            tmp = r[k - 1];
                            r[k - 1] = r[t - 1];
                            r[t - 1] = tmp;
                            tmpi = c[k - 1];
                            c[k - 1] = c[t - 1];
                            c[t - 1] = tmpi;
                            t = k;
                        }
                    }
                    i = i + 1;
                }
                while (i <= n);
                i = n - 1;
                do
                {
                    tmp = r[i];
                    r[i] = r[0];
                    r[0] = tmp;
                    tmpi = c[i];
                    c[i] = c[0];
                    c[0] = tmpi;
                    t = 1;
                    while (t != 0)
                    {
                        k = 2 * t;
                        if (k > i)
                        {
                            t = 0;
                        }
                        else
                        {
                            if (k < i)
                            {
                                if (r[k] > r[k - 1])
                                {
                                    k = k + 1;
                                }
                            }
                            if (r[t - 1] >= r[k - 1])
                            {
                                t = 0;
                            }
                            else
                            {
                                tmp = r[k - 1];
                                r[k - 1] = r[t - 1];
                                r[t - 1] = tmp;
                                tmpi = c[k - 1];
                                c[k - 1] = c[t - 1];
                                c[t - 1] = tmpi;
                                t = k;
                            }
                        }
                    }
                    i = i - 1;
                }
                while (i >= 1);
            }

            //
            // compute tied ranks
            //
            i = 0;
            while (i <= n - 1)
            {
                j = i + 1;
                while (j <= n - 1)
                {
                    if (r[j] != r[i])
                    {
                        break;
                    }
                    j = j + 1;
                }
                for (k = i; k <= j - 1; k++)
                {
                    r[k] = 1 + ((double)(i + j - 1)) / (double)(2);
                }
                i = j;
            }

            //
            // back to x
            //
            for (i = 0; i <= n - 1; i++)
            {
                x[c[i]] = r[i];
            }
        }
    }
}
