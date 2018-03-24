using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace RCNet.Extensions
{
    /// <summary>
    /// Useful String class Extensions
    /// </summary>
    public static class StringClassExtensions
    {
        public static double ParseDouble(this string x, bool throwEx, string exText = "")
        {
            double value;
            if(!double.TryParse(x, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
            {
                if(!double.TryParse(x, NumberStyles.Float, CultureInfo.CurrentCulture, out value))
                {
                    if(throwEx)
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

        public static bool ParseBool(this string x, bool throwEx, string exText = "")
        {
            bool value;
            if (!bool.TryParse(x, out value))
            {
                if (throwEx)
                {
                    throw new Exception(exText);
                }
                else
                {
                    value = false;
                }
            }
            return value;
        }

        public static DateTime ParseDateTime(this string x, bool throwEx, string exText = "")
        {
            DateTime value;
            if (!DateTime.TryParse(x, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out value))
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

        public static int ParseInt(this string x, bool throwEx, string exText = "")
        {
            int value;
            if (!int.TryParse(x, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
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


    }//StringClassExtensions
}//Namespace
