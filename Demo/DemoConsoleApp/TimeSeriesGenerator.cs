using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Globalization;
using RCNet.RandomValue;
using RCNet.Neural.Data.Generators;

namespace Demo.DemoConsoleApp
{
    /// <summary>
    /// A tool for generating some frequently used time series and saving them to a csv file
    /// </summary>
    public static class TimeSeriesGenerator
    {
        /// <summary>
        /// Generates time series of specified length based on specified signal generator.
        /// </summary>
        /// <param name="generator">One of the implemented generators</param>
        /// <param name="length">Time series target length</param>
        /// <returns></returns>
        public static List<double> GenTimeSeries(IGenerator generator, int length)
        {
            List<double> dataCollection = new List<double>(length);
            for (int i = 0; i < length; i++)
            {
                dataCollection.Add(generator.Next());
            }
            return dataCollection;
        }

        /// <summary>
        /// Generates a random time series of the numbers between 0 and 1.
        /// </summary>
        /// <param name="length">The required length</param>
        /// <param name="seek">
        /// Initial seek of the random generator.
        /// Specify seek less than 0 to obtain different initialization each time this function is invoked.
        /// </param>
        /// <returns>A collection of random values</returns>
        public static List<double> GenRandomTimeSeries(int length, int seek = -1)
        {
            RandomValueSettings settings = new RandomValueSettings(0, 1, false);
            RandomGenerator generator = new RandomGenerator(settings, seek);
            return GenTimeSeries(generator, length);
        }

        /// <summary>
        /// Generates a sinusoidal time series
        /// </summary>
        /// <param name="length">The required length</param>
        /// <param name="phase">Phase shift</param>
        /// <param name="freq">Frequency coefficient</param>
        /// <param name="ampl">Amplitude coefficient</param>
        /// <returns>A collection of sinusoidal values</returns>
        public static List<double> GenSinusoidTimeSeries(int length, double phase = 0d, double freq = 1d, double ampl = 1d)
        {
            SinusoidalGeneratorSettings settings = new SinusoidalGeneratorSettings(phase, freq, ampl);
            SinusoidalGenerator generator = new SinusoidalGenerator(settings);
            return GenTimeSeries(generator, length);
        }

        /// <summary>
        /// Generates the Mackey-Glass time series
        /// </summary>
        /// <param name="length">The required length</param>
        /// <param name="tau">Tau (backward deepness 2-18)</param>
        /// <param name="b">b coefficient</param>
        /// <param name="c">c coefficient</param>
        /// <returns>A collection of Mackey-Glass values</returns>
        public static List<double> GenMackeyGlassTimeSeries(int length, int tau = 18, double b = 0.1, double c = 0.2)
        {
            MackeyGlassGeneratorSettings settings = new MackeyGlassGeneratorSettings(tau, b, c);
            MackeyGlassGenerator generator = new MackeyGlassGenerator(settings);
            return GenTimeSeries(generator, length);
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
            using (StreamWriter streamWriter = new StreamWriter(new FileStream(fileName, FileMode.Create)))
            {
                streamWriter.WriteLine(valueColumnName);
                foreach (double value in dataCollection)
                {
                    streamWriter.WriteLine(value.ToString("F20", cultureInfo));
                }
            }
            return;
        }

        /// <summary>
        /// Function generates and saves three types of time series in csv format for demo purposes.
        /// (Random, Sinusoid and Mackey Glass)
        /// </summary>
        /// <param name="dir">The output directory</param>
        /// <param name="cultureInfo">The culture info object to be used</param>
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
