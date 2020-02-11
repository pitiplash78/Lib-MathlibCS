using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PreAnalyseExtended
{
    // Todo mehr oder weniger überall einbauen & summaries tun, wie mit common time series umgehen

    public class Data
    {
        public int FileIndex { get; set; }
        public int ColumnIndex { get; set; }

        public string ColumnName { get; set; }
        public Units.Units.UnitNamesEnum ColumnUnit { get; set; }
        public Station.SensorEntry Sensor { get; set; }

        public DateTime StartTime { get; set; }
        public TimeSpan SampleRate { get; set; }

        public FilePath FilePath { get; set; }

        public DoubleArray Values;

        public double[] Time; // TODO gebraucht

        public double MeanValue = double.NaN;
    }

    public class CombinedData
    {
        public Data[] BaseData = null;

        public DateTime StartTime;
        public int SampleRate = -1;
        public int DataLength = -1;

        public FilePath FilePath { get; set; }

        public DoubleArray[] OriginalData = null;
        public double[] TimeZGC;
        public double[] TimeMJD;

        public Station.SensorEntry SensorResultingData { get; set; }
       
        public DoubleArray ResultingData = null;

        public double MeanValue = double.NaN;

        public CombinedData()
        { }
    }
}
