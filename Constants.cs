using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace MathLibCS
{
     /// <summary>
    /// Class conaining all needed constant values.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Numberformat for english notification (. as decimal separator).
        /// </summary>
        public static System.Globalization.NumberFormatInfo NumberFormatEN = new System.Globalization.CultureInfo("en-US", false).NumberFormat;

        /// <summary>
        /// Numberformat for german notification (, as decimal separator).
        /// </summary>
        public static System.Globalization.NumberFormatInfo NumberFormatDE = new System.Globalization.CultureInfo("de-DE", false).NumberFormat;
    }
}
