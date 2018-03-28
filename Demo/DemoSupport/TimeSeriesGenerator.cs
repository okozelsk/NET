using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Globalization;

namespace RCNet.Demo
{
    /// <summary>
    /// A tool for generating some frequently used time series and saving them to a csv file
    /// </summary>
    public static class TimeSeriesGenerator
    {
        /// <summary>
        /// Generates a random time series of the numbers between 0 and 1.
        /// </summary>
        /// <param name="length">The required length</param>
        /// <param name="randSeek">The random generator seek. Specify a value less than zero to obtain different results when you recall the function.</param>
        /// <returns>The collection of generated values</returns>
        public static List<double> GenRandomTimeSeries(int length, int randSeek = -1)
        {
            Random rand = (randSeek < 0) ? new Random() : new Random(randSeek);
            List<double> dataCollection = new List<double>(length);
            for (int i = 0; i < length; i++)
            {
                dataCollection.Add(rand.NextDouble());
            }
            return dataCollection;
        }

        /// <summary>
        /// Generates a sinusoid time series
        /// </summary>
        /// <param name="length">The required length</param>
        /// <returns>The collection of generated values</returns>
        public static List<double> GenSinusoidTimeSeries(int length)
        {
            List<double> dataCollection = new List<double>(length);
            for (int i = 0; i < length; i++)
            {
                double sinVal = Math.Sin(Math.PI * i / 180.0);
                dataCollection.Add(sinVal);
            }
            return dataCollection;
        }

        /// <summary>
        /// Generates the Mackey Glass time series
        /// </summary>
        /// <param name="length">The required length</param>
        /// <returns>The collection of generated values</returns>
        public static List<double> GenMackeyGlassTimeSeries(int length)
        {
            double[] genInitValues = { 0.9697, 0.9699, 0.9794, 1.0003, 1.0319, 1.0703, 1.1076, 1.1352, 1.1485, 1.1482, 1.1383, 1.1234, 1.1072, 1.0928, 1.0820, 1.0756, 1.0739, 1.0759 };
            int tau = genInitValues.Length; //18
            double b = 0.1d, c = 0.2d;
            List<double> dataCollection = new List<double>(genInitValues);
            for (int i = genInitValues.Length; i < genInitValues.Length + length; i++)
            {
                double refMGV = dataCollection[i - tau];
                double lastMGV = dataCollection.Last();
                double newMGV = lastMGV - b * lastMGV + c * refMGV / (1 + Math.Pow(refMGV, 10));
                dataCollection.Add(newMGV);
            }
            dataCollection.RemoveRange(0, tau);
            return dataCollection;
        }

        /// <summary>
        /// Function saves given time series in a csv file.
        /// </summary>
        /// <param name="fileName">Name of the output csv file.</param>
        /// <param name="valueColumnName">Name of the value column</param>
        /// <param name="dataCollection">Data</param>
        /// <param name="cultureInfo">Culture info to be used</param>
        public static void SaveTimeSeriesToCsvFile(string fileName, string valueColumnName, List<double> dataCollection, CultureInfo cultureInfo)
        {
            using (StreamWriter stream = new StreamWriter(new FileStream(fileName, FileMode.Create)))
            {
                stream.WriteLine(valueColumnName);
                foreach (double value in dataCollection)
                {
                    stream.WriteLine(value.ToString("F20", cultureInfo));
                }
            }
            return;
        }

        /// <summary>
        /// Function generates ans saves three types of time series in csv format for demo purposes.
        /// (Random, Sinusoid and Mackey Glass)
        /// </summary>
        /// <param name="dir">The output directory</param>
        /// <param name="cultureInfo">The culture info to be used</param>
        /// <param name="timeSeriesLength">The required length of the generated time series</param>
        public static void PrepareDemoTimeSeriesCsvFiles(string dir, CultureInfo cultureInfo, int timeSeriesLength = 10000)
        {
            SaveTimeSeriesToCsvFile(dir + "\\" + "Random.csv", "Value", GenRandomTimeSeries(timeSeriesLength), cultureInfo);
            SaveTimeSeriesToCsvFile(dir + "\\" + "Sinusoid.csv", "Value", GenSinusoidTimeSeries(timeSeriesLength), cultureInfo);
            SaveTimeSeriesToCsvFile(dir + "\\" + "MackeyGlass.csv", "Value", GenMackeyGlassTimeSeries(timeSeriesLength), cultureInfo);
            return;
        }

    }//TimeSeriesGenerator

}//Namespace
