using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Globalization;
using RCNet.RandomValue;
using RCNet.Neural.Data.Modulation;

namespace RCNet.DemoConsoleApp
{
    /// <summary>
    /// A tool for generating some frequently used time series and saving them to a csv file
    /// </summary>
    public static class TimeSeriesGenerator
    {
        /// <summary>
        /// Generates time series of specified length based on specified signal modulator.
        /// </summary>
        /// <param name="modulator">One of the implemented modulators</param>
        /// <param name="length">Time series target length</param>
        /// <returns></returns>
        public static List<double> GenTimeSeries(IModulator modulator, int length)
        {
            List<double> dataCollection = new List<double>(length);
            for (int i = 0; i < length; i++)
            {
                dataCollection.Add(modulator.Next());
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
            RandomValueSettings settings = new RandomValueSettings(0, 1, false, Extensions.RandomClassExtensions.DistributionType.Uniform);
            RandomModulator modulator = new RandomModulator(settings, seek);
            return GenTimeSeries(modulator, length);
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
            SinusoidalModulatorSettings settings = new SinusoidalModulatorSettings(phase, freq, ampl);
            SinusoidalModulator modulator = new SinusoidalModulator(settings);
            return GenTimeSeries(modulator, length);
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
            MackeyGlassModulatorSettings settings = new MackeyGlassModulatorSettings(tau, b, c);
            MackeyGlassModulator modulator = new MackeyGlassModulator(settings);
            return GenTimeSeries(modulator, length);
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
