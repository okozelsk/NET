using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace RCNet.Extensions
{
    /// <summary>
    /// Useful extensions of RNGCryptoServiceProvider class.
    /// </summary>
    public static class RNGCryptoServiceProviderClassExtension
    {
        /// <summary>
        /// Generates next random byte.
        /// </summary>
        public static byte NextByte(this RNGCryptoServiceProvider rndGen)
        {
            byte[] rndByte = new byte[1];
            rndGen.GetBytes(rndByte);
            return rndByte[0];
        }

        /// <summary>
        /// Generates next random nonzero byte.
        /// </summary>
        public static byte NextNonZeroByte(this RNGCryptoServiceProvider rndGen)
        {
            byte[] rndByte = new byte[1];
            rndGen.GetNonZeroBytes(rndByte);
            return rndByte[0];
        }

        /// <summary>
        /// Generates next random UInt32.
        /// </summary>
        public static UInt32 NextUInt32(this RNGCryptoServiceProvider rndGen)
        {
            byte[] rndBytes = new byte[4];
            rndGen.GetBytes(rndBytes);
            return BitConverter.ToUInt32(rndBytes, 0);
        }

        /// <summary>
        /// Generates next random Int32.
        /// </summary>
        public static Int32 NextInt32(this RNGCryptoServiceProvider rndGen)
        {
            byte[] rndBytes = new byte[4];
            rndGen.GetBytes(rndBytes);
            return BitConverter.ToInt32(rndBytes, 0);
        }

        /// <summary>
        /// Generates next random UInt64.
        /// </summary>
        public static UInt64 NextUInt64(this RNGCryptoServiceProvider rndGen)
        {
            byte[] rndBytes = new byte[8];
            rndGen.GetBytes(rndBytes);
            return BitConverter.ToUInt64(rndBytes, 0);
        }

        /// <summary>
        /// Generates next random Int64.
        /// </summary>
        public static Int64 NextInt64(this RNGCryptoServiceProvider rndGen)
        {
            byte[] rndBytes = new byte[8];
            rndGen.GetBytes(rndBytes);
            return BitConverter.ToInt64(rndBytes, 0);
        }

        /// <summary>
        /// Generates next random UInt32 between 0 and maxValue .
        /// </summary>
        public static UInt32 NextUInt32(this RNGCryptoServiceProvider rndGen, UInt32 maxValue)
        {
            UInt32 genNumber = rndGen.NextUInt32();
            while (genNumber == UInt32.MaxValue)
            {
                genNumber = rndGen.NextUInt32();
            }
            return (UInt32)((maxValue + 1) * ((double)genNumber / (double)UInt32.MaxValue));
        }

        /// <summary>
        /// Generates next random Int32 between minValue and maxValue .
        /// </summary>
        public static Int32 NextInt32(this RNGCryptoServiceProvider rndGen, Int32 minValue, Int32 maxValue)
        {
            return (Int32)Math.Round(rndGen.NextDouble(minValue, maxValue));
        }

        /// <summary>
        /// Generates next random UInt64 between 0 and maxValue .
        /// </summary>
        public static UInt64 NextUInt64(this RNGCryptoServiceProvider rndGen, UInt64 maxValue)
        {
            UInt64 genNumber = rndGen.NextUInt64();
            while (genNumber == UInt64.MaxValue)
            {
                genNumber = rndGen.NextUInt64();
            }
            return (UInt64)((maxValue + 1) * ((decimal)genNumber / (decimal)UInt64.MaxValue));
        }

        /// <summary>
        /// Generates next random double between 0 and 1.
        /// </summary>
        public static double NextDouble(this RNGCryptoServiceProvider rndGen)
        {
            return (double)((decimal)rndGen.NextUInt64() / (decimal)UInt64.MaxValue);
        }

        /// <summary>
        /// Generates next random double between min and max.
        /// </summary>
        public static double NextDouble(this RNGCryptoServiceProvider rndGen, double min, double max)
        {
            return min + (max - min) * rndGen.NextDouble();
        }

    }//RNGCryptoServiceProviderClassExtension

}//Namespace
