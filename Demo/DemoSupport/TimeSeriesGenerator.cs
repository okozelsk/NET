using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;

namespace RCNet.Demo
{
    /// <summary>
    /// Tool for generation of several usual time series types
    /// </summary>
    public static class TimeSeriesGenerator
    {
        /// <summary>
        /// Generates random time series of numbers between 0 and 1.
        /// </summary>
        /// <param name="length">Required series length</param>
        /// <param name="randSeek">Specify less than 0 to obtain different series each call</param>
        /// <returns>List of generated values</returns>
        public static List<double> GenRandomTimeSeries(int length, int randSeek = -1)
        {
            Random rand = (randSeek < 0) ? new Random() : new Random(randSeek);
            List<double> values = new List<double>(length);
            for (int i = 0; i < length; i++)
            {
                values.Add(rand.NextDouble());
            }
            return values;
        }

        /// <summary>
        /// Generates sinusoid time series
        /// </summary>
        /// <param name="length">Required series length</param>
        /// <returns>List of generated values</returns>
        public static List<double> GenSinusoidTimeSeries(int length)
        {
            List<double> values = new List<double>(length);
            for (int i = 0; i < length; i++)
            {
                double sinVal = Math.Sin(Math.PI * i / 180.0);
                values.Add(sinVal);
            }
            return values;
        }

        /// <summary>
        /// Generates MackeyGlass time series
        /// </summary>
        /// <param name="length">Required series length</param>
        /// <returns>List of generated values</returns>
        public static List<double> GenMackeyGlassTimeSeries(int length)
        {
            double[] genInitValues = { 0.9697, 0.9699, 0.9794, 1.0003, 1.0319, 1.0703, 1.1076, 1.1352, 1.1485, 1.1482, 1.1383, 1.1234, 1.1072, 1.0928, 1.0820, 1.0756, 1.0739, 1.0759 };
            int tau = genInitValues.Length; //18
            double b = 0.1d, c = 0.2d;
            List<double> values = new List<double>(genInitValues);
            for (int i = genInitValues.Length; i < genInitValues.Length + length; i++)
            {
                double refMgV = values[i - tau];
                double lastMgV = values.Last();
                double newMgV = lastMgV - b * lastMgV + c * refMgV / (1 + Math.Pow(refMgV, 10));
                values.Add(newMgV);
            }
            values.RemoveRange(0, tau);
            return values;
        }

        public static void StoreTimeSeriesAsCSV(string fileName, string columnName, List<double> data)
        {
            StreamWriter stream = new StreamWriter(new FileStream(fileName, FileMode.Create));
            stream.WriteLine(columnName);
            foreach (double value in data)
            {
                stream.WriteLine(value.ToString("F20", CultureInfo.CurrentCulture));
            }
            stream.Close();
            stream.Dispose();
            return;
        }

        public static void GenerateStandardCSVFiles(string dir, int dataLength = 10000)
        {
            StoreTimeSeriesAsCSV(dir + "\\" + "Random.csv", "Value", GenRandomTimeSeries(dataLength));
            StoreTimeSeriesAsCSV(dir + "\\" + "Sinusoid.csv", "Value", GenSinusoidTimeSeries(dataLength));
            StoreTimeSeriesAsCSV(dir + "\\" + "MackeyGlass.csv", "Value", GenMackeyGlassTimeSeries(dataLength));
            return;
        }
    }//TimeSeriesGenerator


}//Namespace
