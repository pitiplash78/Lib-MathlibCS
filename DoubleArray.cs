using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

public sealed class DoubleArray : IEquatable<DoubleArray>, IEnumerable<double>
{
    private static System.Globalization.NumberFormatInfo NumberFormat = new System.Globalization.CultureInfo("en-US", false).NumberFormat;

    private /* readonly */ double[] data;

    public double this[int index]
    {
        get
        {
            return data[index];
        }
        set
        {
            data[index] = value;
        }
    }

    public DoubleArray(params double[] data)
    {
        if (data == null)
            throw new ArgumentNullException("data");

        this.data = (double[])data.Clone();
    }

    public DoubleArray(int Length)
    {
        this.data = new double[Length];
    }

    public double[] Data
    {
        get
        {
            return (double[])data.Clone();
        }
        set
        {
            data = (double[])value.Clone();
        }
    }

    private double? hash;
    public override int GetHashCode()
    {
        if (hash == null)
        {
            double result = 13;
            for (int i = 0; i < data.Length; i++)
            {
                result = (result * 7) + data[i];
            }
            hash = result;
        }
        return (int)hash.GetValueOrDefault();
    }

    public int Length { get { return data.Length; } }

    public IEnumerator<double> GetEnumerator()
    {
        for (int i = 0; i < data.Length; i++)
        {
            yield return data[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public override bool Equals(object obj)
    {
        return this == (obj as DoubleArray);
    }

    public bool Equals(DoubleArray obj)
    {
        return this == obj;
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder("[");
        if (data.Length > 0) 
            sb.Append(data[0].ToString(NumberFormat));
        for (int i = 1; i < data.Length; i++)
        {
            sb.Append(", ").Append(data[i].ToString(NumberFormat));
        }
        sb.Append(']');
        return sb.ToString();
    }

    public static bool operator ==(DoubleArray x, DoubleArray y)
    {
        if (ReferenceEquals(x, y))
            return true;

        if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
            return false;

        if (x.hash.HasValue && y.hash.HasValue && // exploit known different hash
            x.hash.GetValueOrDefault() != y.hash.GetValueOrDefault())
            return false;

        double[] xdata = x.data;
        double[] ydata = y.data;

        if (xdata.Length != ydata.Length)
            return false;

        for (int i = 0; i < xdata.Length; i++)
        {
            if (xdata[i] != ydata[i])
                return false;
        }
        return true;
    }

    public static bool operator !=(DoubleArray x, DoubleArray y)
    {
        return !(x == y);
    }

    // Addition
    public static DoubleArray operator +(DoubleArray x, DoubleArray y)
    {
        if (x == null || y == null)
            throw new ArgumentNullException();

        double[] xdata = x.data;
        double[] ydata = y.data;

        if (xdata.Length != ydata.Length)
            throw new InvalidOperationException("Length mismatch");

        double[] result = new double[xdata.Length];
        for (int i = 0; i < xdata.Length; i++)
        {
            result[i] = xdata[i] + ydata[i];
        }
        return new DoubleArray(result);
    }
    public static DoubleArray operator +(DoubleArray x, double y)
    {
        if (x == null)
            throw new ArgumentNullException();

        double[] xdata = x.data;
        double ydata = y;

        double[] result = new double[xdata.Length];
        for (int i = 0; i < xdata.Length; i++)
        {
            result[i] = xdata[i] + ydata;
        }
        return new DoubleArray(result);
    }
    public static DoubleArray operator +(double x, DoubleArray y)
    {
        if (y == null)
            throw new ArgumentNullException();

        double[] xdata = y.data;
        double ydata = x;

        double[] result = new double[xdata.Length];
        for (int i = 0; i < xdata.Length; i++)
        {
            result[i] = xdata[i] + ydata;
        }
        return new DoubleArray(result);
    }

    // Substraction
    public static DoubleArray operator -(DoubleArray x, DoubleArray y)
    {
        if (x == null || y == null)
            throw new ArgumentNullException();

        double[] xdata = x.data;
        double[] ydata = y.data;

        if (xdata.Length != ydata.Length)
            throw new InvalidOperationException("Length mismatch");

        double[] result = new double[xdata.Length];
        for (int i = 0; i < xdata.Length; i++)
        {
            result[i] = xdata[i] - ydata[i];
        }
        return new DoubleArray(result);
    }
    public static DoubleArray operator -(DoubleArray x, double y)
    {
        if (x == null)
            throw new ArgumentNullException();

        double[] xdata = x.data;
        double ydata = y;

        double[] result = new double[xdata.Length];
        for (int i = 0; i < xdata.Length; i++)
        {
            result[i] = xdata[i] - ydata;
        }
        return new DoubleArray(result);
    }
    public static DoubleArray operator -(double x, DoubleArray y)
    {
        if (y == null)
            throw new ArgumentNullException();

        double[] xdata = y.data;
        double ydata = x;

        double[] result = new double[xdata.Length];
        for (int i = 0; i < xdata.Length; i++)
        {
            result[i] = xdata[i] - ydata;
        }
        return new DoubleArray(result);
    }

    // Multiplication
    public static DoubleArray operator *(DoubleArray x, DoubleArray y)
    {
        if (x == null || y == null)
            throw new ArgumentNullException();

        double[] xdata = x.data;
        double[] ydata = y.data;

        if (xdata.Length != ydata.Length)
            throw new InvalidOperationException("Length mismatch");

        double[] result = new double[xdata.Length];
        for (int i = 0; i < xdata.Length; i++)
        {
            result[i] = xdata[i] * ydata[i];
        }
        return new DoubleArray(result);
    }
    public static DoubleArray operator *(DoubleArray x, double y)
    {
        if (x == null)
            throw new ArgumentNullException();

        double[] xdata = x.data;
        double ydata = y;

        double[] result = new double[xdata.Length];
        for (int i = 0; i < xdata.Length; i++)
        {
            result[i] = xdata[i] * ydata;
        }
        return new DoubleArray(result);
    }
    public static DoubleArray operator *(double x, DoubleArray y)
    {
        if (y == null)
            throw new ArgumentNullException();

        double[] xdata = y.data;
        double ydata = x;

        double[] result = new double[xdata.Length];
        for (int i = 0; i < xdata.Length; i++)
        {
            result[i] = xdata[i] * ydata;
        }
        return new DoubleArray(result);
    }

    // Division
    public static DoubleArray operator /(DoubleArray x, DoubleArray y)
    {
        if (x == null || y == null)
            throw new ArgumentNullException();

        double[] xdata = x.data;
        double[] ydata = y.data;

        if (xdata.Length != ydata.Length)
            throw new InvalidOperationException("Length mismatch");

        double[] result = new double[xdata.Length];
        for (int i = 0; i < xdata.Length; i++)
        {
            result[i] = xdata[i] / ydata[i];
        }
        return new DoubleArray(result);
    }
    public static DoubleArray operator /(DoubleArray x, double y)
    {
        if (x == null)
            throw new ArgumentNullException();

        double[] xdata = x.data;
        double ydata = y;

        double[] result = new double[xdata.Length];
        for (int i = 0; i < xdata.Length; i++)
        {
            result[i] = xdata[i] / ydata;
        }
        return new DoubleArray(result);
    }
    public static DoubleArray operator /(double x, DoubleArray y)
    {
        if (y == null)
            throw new ArgumentNullException();

        double xdata = x;
        double[] ydata = y.data;

        double[] result = new double[ydata.Length];
        for (int i = 0; i < ydata.Length; i++)
        {
            result[i] = xdata / ydata[i];
        }
        return new DoubleArray(result);
    }

    // Math functions
    public static DoubleArray Abs(DoubleArray x)
    {
        if (x == null)
            throw new ArgumentNullException();

        double[] xdata = x.data;
        double[] result = new double[xdata.Length];

        for (int i = 0; i < result.Length; i++)
        {
            result[i] = Math.Abs(xdata[i]);
        }

        return new DoubleArray(result);
    }
    public static DoubleArray Acos(DoubleArray x)
    {
        if (x == null)
            throw new ArgumentNullException();

        double[] xdata = x.data;
        double[] result = new double[xdata.Length];

        for (int i = 0; i < result.Length; i++)
        {
            result[i] = Math.Acos(xdata[i]);
        }

        return new DoubleArray(result);
    }
    public static DoubleArray Asin(DoubleArray x)
    {
        if (x == null)
            throw new ArgumentNullException();

        double[] xdata = x.data;
        double[] result = new double[xdata.Length];

        for (int i = 0; i < result.Length; i++)
        {
            result[i] = Math.Asin(xdata[i]);
        }

        return new DoubleArray(result);
    }
    public static DoubleArray Atan(DoubleArray x)
    {
        if (x == null)
            throw new ArgumentNullException();

        double[] xdata = x.data;
        double[] result = new double[xdata.Length];

        for (int i = 0; i < result.Length; i++)
        {
            result[i] = Math.Atan(xdata[i]);
        }

        return new DoubleArray(result);
    }
    public static DoubleArray Atan2(DoubleArray x, DoubleArray y)
    {
        if (x == null || y == null)
            throw new ArgumentNullException();

        double[] xdata = x.data;
        double[] ydata = y.data;

        if (xdata.Length != ydata.Length)
            throw new InvalidOperationException("Length mismatch");

        double[] result = new double[xdata.Length];

        for (int i = 0; i < result.Length; i++)
        {
            result[i] = Math.Atan2(xdata[i], ydata[i]);
        }

        return new DoubleArray(result);
    }
    public static DoubleArray Atan2(DoubleArray x, double y)
    {
        if (x == null)
            throw new ArgumentNullException();

        double[] xdata = x.data;
        double ydata = y;
        double[] result = new double[xdata.Length];

        for (int i = 0; i < result.Length; i++)
        {
           result[i] = Math.Atan2(xdata[i], ydata);
        }

        return new DoubleArray(result);
    }
    public static DoubleArray Atan2(double x, DoubleArray y)
    {
        if (y == null)
            throw new ArgumentNullException();

        double[] xdata = y.data;
        double ydata = x;

        double[] result = new double[xdata.Length];

        for (int i = 0; i < result.Length; i++)
        {
            result[i] = Math.Atan2(ydata, xdata[i]);
        }

        return new DoubleArray(result);
    }
    public static DoubleArray Ceiling(DoubleArray x)
    {
        if (x == null)
            throw new ArgumentNullException();

        double[] xdata = x.data;
        double[] result = new double[xdata.Length];

        for (int i = 0; i < result.Length; i++)
        {
            result[i] = Math.Ceiling(xdata[i]);
        }

        return new DoubleArray(result);
    }
    public static DoubleArray Cos(DoubleArray x)
    {
        if (x == null)
            throw new ArgumentNullException();

        double[] xdata = x.data;
        double[] result = new double[xdata.Length];

        for (int i = 0; i < result.Length; i++)
        {
            result[i] = Math.Cos(xdata[i]);
        }

        return new DoubleArray(result);
    }
    public static DoubleArray Cosh(DoubleArray x)
    {
        if (x == null)
            throw new ArgumentNullException();

        double[] xdata = x.data;
        double[] result = new double[xdata.Length];

        for (int i = 0; i < result.Length; i++)
        {
            result[i] = Math.Cosh(xdata[i]);
        }

        return new DoubleArray(result);
    }
    public static DoubleArray Exp(DoubleArray x)
    {
        if (x == null)
            throw new ArgumentNullException();

        double[] xdata = x.data;
        double[] result = new double[xdata.Length];

        for (int i = 0; i < result.Length; i++)
        {
            result[i] = Math.Exp(xdata[i]);
        }

        return new DoubleArray(result);
    }
    public static DoubleArray Floor(DoubleArray x)
    {
        if (x == null)
            throw new ArgumentNullException();

        double[] xdata = x.data;
        double[] result = new double[xdata.Length];

        for (int i = 0; i < result.Length; i++)
        {
            result[i] = Math.Floor(xdata[i]);
        }

        return new DoubleArray(result);
    }
    public static DoubleArray Log(DoubleArray x)
    {
        if (x == null)
            throw new ArgumentNullException();

        double[] xdata = x.data;
        double[] result = new double[xdata.Length];

        for (int i = 0; i < result.Length; i++)
        {
            result[i] = Math.Log(xdata[i]);
        }

        return new DoubleArray(result);
    }
    public static DoubleArray Log(DoubleArray x, double newBase)
    {
        if (x == null)
            throw new ArgumentNullException();

        double[] xdata = x.data;
        double[] result = new double[xdata.Length];

        for (int i = 0; i < result.Length; i++)
        {
            result[i] = Math.Log(xdata[i], newBase);
        }

        return new DoubleArray(result);
    }
    public static DoubleArray Log10(DoubleArray x)
    {
        if (x == null)
            throw new ArgumentNullException();

        double[] xdata = x.data;
        double[] result = new double[xdata.Length];

        for (int i = 0; i < result.Length; i++)
        {
            result[i] = Math.Log10(xdata[i]);
        }

        return new DoubleArray(result);
    }
    public static DoubleArray Pow(DoubleArray x, DoubleArray y)
    {
        if (x == null || y == null)
            throw new ArgumentNullException();

        double[] xdata = x.data;
        double[] ydata = y.data;

        if (xdata.Length != ydata.Length)
            throw new InvalidOperationException("Length mismatch");

        if (x == null)
            throw new ArgumentNullException();

        double[] result = new double[xdata.Length];

        for (int i = 0; i < result.Length; i++)
        {
            result[i] = Math.Pow(xdata[i], ydata[i]);
        }

        return new DoubleArray(result);
    }
    public static DoubleArray Pow(DoubleArray x, double y)
    {
        if (x == null)
            throw new ArgumentNullException();

        double[] xdata = x.data;
        double[] result = new double[xdata.Length];

        for (int i = 0; i < result.Length; i++)
        {
            result[i] = Math.Pow(xdata[i], y);
        }

        return new DoubleArray(result);
    }
    public static DoubleArray Pow(double x, DoubleArray y)
    {
        if (y == null)
            throw new ArgumentNullException();

        double xdata = x;
        double[] ydata = y.data;
        
        double[] result = new double[ydata.Length];

        for (int i = 0; i < result.Length; i++)
        {
            result[i] = Math.Pow(x, ydata[i]);
        }

        return new DoubleArray(result);
    }
    public static DoubleArray Round(DoubleArray x)
    {
        if (x == null)
            throw new ArgumentNullException();

        double[] xdata = x.data;
        double[] result = new double[xdata.Length];

        for (int i = 0; i < result.Length; i++)
        {
            result[i] = Math.Round(xdata[i]);
        }

        return new DoubleArray(result);
    }
    public static DoubleArray Round(DoubleArray x, int digits)
    {
        if (x == null)
            throw new ArgumentNullException();

        double[] xdata = x.data;
        double[] result = new double[xdata.Length];

        for (int i = 0; i < result.Length; i++)
        {
            result[i] = Math.Round(xdata[i], digits);
        }

        return new DoubleArray(result);
    }
    public static DoubleArray Sin(DoubleArray x)
    {
        if (x == null)
            throw new ArgumentNullException();

        double[] xdata = x.data;
        double[] result = new double[xdata.Length];

        for (int i = 0; i < result.Length; i++)
        {
            result[i] = Math.Sin(xdata[i]);
        }

        return new DoubleArray(result);
    }
    public static DoubleArray Sinh(DoubleArray x)
    {
        if (x == null)
            throw new ArgumentNullException();

        double[] xdata = x.data;
        double[] result = new double[xdata.Length];

        for (int i = 0; i < result.Length; i++)
        {
            result[i] = Math.Sinh(xdata[i]);
        }

        return new DoubleArray(result);
    }
    public static DoubleArray Sqrt(DoubleArray x)
    {
        if (x == null)
            throw new ArgumentNullException();

        double[] xdata = x.data;
        double[] result = new double[xdata.Length];

        for (int i = 0; i < result.Length; i++)
        {
            result[i] = Math.Sqrt(xdata[i]);
        }

        return new DoubleArray(result);
    }
    public static DoubleArray Tan(DoubleArray x)
    {
        if (x == null)
            throw new ArgumentNullException();

        double[] xdata = x.data;
        double[] result = new double[xdata.Length];

        for (int i = 0; i < result.Length; i++)
        {
            result[i] = Math.Tan(xdata[i]);
        }

        return new DoubleArray(result);
    }
    public static DoubleArray Tanh(DoubleArray x)
    {
        if (x == null)
            throw new ArgumentNullException();

        double[] xdata = x.data;
        double[] result = new double[xdata.Length];

        for (int i = 0; i < result.Length; i++)
        {
            result[i] = Math.Tanh(xdata[i]);
        }

        return new DoubleArray(result);
    }
    public static DoubleArray Truncate(DoubleArray x)
    {
        if (x == null)
            throw new ArgumentNullException();

        double[] xdata = x.data;
        double[] result = new double[xdata.Length];

        for (int i = 0; i < result.Length; i++)
        {
            result[i] = Math.Truncate(xdata[i]);
        }

        return new DoubleArray(result);
    }

    public static DoubleArray DegreeToRadian(DoubleArray x)
    {
        if (x == null)
            throw new ArgumentNullException();

        double[] xdata = x.data;
        double[] result = new double[xdata.Length];

        for (int i = 0; i < result.Length; i++)
        {
            result[i] = xdata[i] / Math.PI * 180.0;
        }

        return new DoubleArray(result);
    }
    public static DoubleArray RadianToDegree(DoubleArray x)
    {
        if (x == null)
            throw new ArgumentNullException();

        double[] xdata = x.data;
        double[] result = new double[xdata.Length];

        for (int i = 0; i < result.Length; i++)
        {
            result[i] = xdata[i] * Math.PI / 180.0; ;
        }

        return new DoubleArray(result);
    }

    public static double Sum(DoubleArray x)
    {
        if (x == null)
            throw new ArgumentNullException();

        double[] xdata = x.data;
        double result = 0.0;
        double tmp = 0.0;

        for (int i = 0; i < xdata.Length; i++)
        {
            tmp = xdata[i];
            if(!Double.IsNaN(tmp))
            result += tmp;
        }

        return result;
    }

    public static double MeanValue(DoubleArray x)
    {
        if (x == null)
            throw new ArgumentNullException();

        double[] xdata = x.data;
        double result = 0.0;
        double tmp = 0.0;
        int count = 0;
        for (int i = 0; i < xdata.Length; i++)
        {
            tmp = xdata[i];
            if (!Double.IsNaN(tmp))
            {
                result += tmp;
                count++;
            }
        }

        return result/(double)count;
    }
}
