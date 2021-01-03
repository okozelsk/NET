using System;
using System.Globalization;

namespace RCNet.Extensions
{
    /// <summary>
    /// Implements useful extensions of a string.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Parses a double value.
        ///</summary>
        /// <remarks>
        /// Function tries to parse a value using the CultureInfo.InvariantCulture and if it fails, tries to use the CultureInfo.CurrentCulture.
        /// If it fails at all, function's behavior then depends on specified parameters.
        /// It can throw the InvalidOperationException exception or return the double.NaN value.
        /// </remarks>
        /// <param name="throwEx">Specifies whether to throw the InvalidOperationException exception in case the parsing fails.</param>
        /// <param name="exText">The specific text of the InvalidOperationException exception.</param>
        /// <param name="str"></param>
        public static double ParseDouble(this string str, bool throwEx, string exText = "")
        {
            if (!double.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
            {
                if (!double.TryParse(str, NumberStyles.Float, CultureInfo.CurrentCulture, out value))
                {
                    if (throwEx)
                    {
                        throw new InvalidOperationException(exText);
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
        /// Parses a DateTime value.
        ///</summary>
        /// <remarks>
        /// Function tries to parse a value using the CultureInfo.InvariantCulture and if it fails, tries to use the CultureInfo.CurrentCulture.
        /// If it fails at all, function's behavior then depends on specified parameters.
        /// It can throw the InvalidOperationException exception or return the DateTime.MinValue.
        /// </remarks>
        /// <param name="throwEx">Specifies whether to throw the InvalidOperationException exception in case the parsing fails.</param>
        /// <param name="exText">The specific text of the InvalidOperationException exception.</param>
        /// <param name="str"></param>
        public static DateTime ParseDateTime(this string str, bool throwEx, string exText = "")
        {
            if (!DateTime.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out DateTime value))
            {
                if (!DateTime.TryParse(str, CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces, out value))
                {
                    if (throwEx)
                    {
                        throw new InvalidOperationException(exText);
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
        /// Parses an int value.
        ///</summary>
        /// <remarks>
        /// Function tries to parse a value using the CultureInfo.InvariantCulture and if it fails, tries to use the CultureInfo.CurrentCulture.
        /// If it fails at all, function's behavior then depends on specified parameters.
        /// It can throw the InvalidOperationException exception or return the int.MinValue.
        /// </remarks>
        /// <param name="throwEx">Specifies whether to throw the InvalidOperationException exception in case the parsing fails.</param>
        /// <param name="exText">The specific text of the InvalidOperationException exception.</param>
        /// <param name="str"></param>
        public static int ParseInt(this string str, bool throwEx, string exText = "")
        {
            if (!int.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
            {
                if (!int.TryParse(str, NumberStyles.Integer, CultureInfo.CurrentCulture, out value))
                {
                    if (throwEx)
                    {
                        throw new InvalidOperationException(exText);
                    }
                    else
                    {
                        value = int.MinValue;
                    }
                }
            }
            return value;
        }

        /// <summary>
        /// Parses a bool value.
        ///</summary>
        /// <remarks>
        /// Function tries to parse a bool value.
        /// If it fails, function's behavior then depends on specified parameters.
        /// It can throw the InvalidOperationException exception or return the false value.
        /// </remarks>
        /// <param name="throwEx">Specifies whether to throw the InvalidOperationException exception in case the parsing fails.</param>
        /// <param name="exText">The specific text of the InvalidOperationException exception.</param>
        /// <param name="str"></param>
        public static bool ParseBool(this string str, bool throwEx, string exText = "")
        {
            if (!bool.TryParse(str, out bool value))
            {
                if (throwEx)
                {
                    throw new InvalidOperationException(exText);
                }
                else
                {
                    value = false;
                }
            }
            return value;
        }

    }//StringExtensions

}//Namespace

