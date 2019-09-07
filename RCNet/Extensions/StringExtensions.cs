using System;
using System.Globalization;

namespace RCNet.Extensions
{
    /// <summary>
    /// Useful String class extensions
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Parses double value. Function tries parsing with the InvariantCulture and if fails, tries the CurrentCulture.
        /// If fails both of them function throws an exception or returns the double.NaN value.
        /// The behaviour depends on given arguments.
        /// </summary>
        public static double ParseDouble(this string x, bool throwEx, string exText = "")
        {
            if (!double.TryParse(x, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
            {
                if (!double.TryParse(x, NumberStyles.Float, CultureInfo.CurrentCulture, out value))
                {
                    if (throwEx)
                    {
                        throw new Exception(exText);
                    }
                    else
                    {
                        value = double.NaN;
                    }
                }
            }
            return value;
        }

        /// <summary>
        /// Parses DateTime value. Function tries parsing with the InvariantCulture and if fails, tries the CurrentCulture.
        /// If fails both of them function throws an exception or returns the DateTime.MinValue.
        /// The behaviour depends on given arguments.
        /// </summary>
        public static DateTime ParseDateTime(this string x, bool throwEx, string exText = "")
        {
            if (!DateTime.TryParse(x, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out DateTime value))
            {
                if (!DateTime.TryParse(x, CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces, out value))
                {
                    if (throwEx)
                    {
                        throw new Exception(exText);
                    }
                    else
                    {
                        value = DateTime.MinValue;
                    }
                }
            }
            return value;
        }

        /// <summary>
        /// Parses int value. Function tries parsing with the InvariantCulture and if fails, tries the CurrentCulture.
        /// If fails both of them function throws an exception or returns the int.MinValue.
        /// The behaviour depends on given arguments.
        /// </summary>
        public static int ParseInt(this string x, bool throwEx, string exText = "")
        {
            if (!int.TryParse(x, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
            {
                if (!int.TryParse(x, NumberStyles.Integer, CultureInfo.CurrentCulture, out value))
                {
                    if (throwEx)
                    {
                        throw new Exception(exText);
                    }
                    else
                    {
                        value = int.MinValue;
                    }
                }
            }
            return value;
        }

    }//StringExtensions

}//Namespace

