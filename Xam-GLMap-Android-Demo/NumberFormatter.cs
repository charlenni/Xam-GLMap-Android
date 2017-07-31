using System;

namespace Xam_GLMap_Android_Demo
{
    public class NumberFormatter
    {
        public static String NiceDoubleToString(double val)
        {
            if (double.IsNaN(val))
            {
                return "---";
            }

            if (val < 10)
            {
                return string.Format("{0:0.00}", val);
            }
            else if (val < 100)
            {
                return string.Format("{0:0.0}", val);
            }
            return string.Format("{0:0}", val);
        }

        public static string FormatSize(long val)
        {
            double sizeInMB = (double)val / (1000 * 1000);
            return string.Format("{0} {1}", NiceDoubleToString(sizeInMB), "MB");
        }
    }
}