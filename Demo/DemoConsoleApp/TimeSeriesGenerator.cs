using RCNet.Neural.Data.Generators;
using RCNet.RandomValue;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Demo.DemoConsoleApp
{
    /// <summary>
    /// Implements the helper methods to generate some frequently used time series and store them as a csv.
    /// </summary>
    public static class TimeSeriesGenerator
    {
        /// <summary>
        /// Generates the time series of specified length using specified generator.
        /// </summary>
        /// <param name="generator">The generator to be used.</param>
        /// <param name="length">The length.</param>
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
        /// Generates a time series of random numbers between 0 and 1.
        /// </summary>
        /// <param name="length">The required length.</param>
        /// <param name="seek">
        /// The initial seek of the random generator.
        /// Specify seek less than 0 to obtain different initialization each time this function is invoked.
        /// </param>
        public static List<double> GenRandomTimeSeries(int length, int seek = -1)
        {
            RandomValueSettings settings = new RandomValueSettings(0, 1, false);
            RandomGenerator generator = new RandomGenerator(settings, seek);
            return GenTimeSeries(generator, length);
        }

        /// <summary>
        /// Generates a sinusoidal time series.
        /// </summary>
        /// <param name="length">The required length.</param>
        /// <param name="phase">The phase shift.</param>
        /// <param name="freq">The frequency coefficient.</param>
        /// <param name="ampl">The amplitude coefficient.</param>
        public static List<double> GenSinusoidTimeSeries(int length, double phase = 0d, double freq = 1d, double ampl = 1d)
        {
            SinusoidalGeneratorSettings settings = new SinusoidalGeneratorSettings(phase, freq, ampl);
            SinusoidalGenerator generator = new SinusoidalGenerator(settings);
            return GenTimeSeries(generator, length);
        }

        /// <summary>
        /// Generates the Mackey-Glass time series
        /// </summary>
        /// <param name="length">The required length.</param>
        /// <param name="tau">The tau (backward deepness 2-18).</param>
        /// <param name="b">The b coefficient.</param>
        /// <param name="c">The c coefficient.</param>
        public static List<double> GenMackeyGlassTimeSeries(int length, int tau = 18, double b = 0.1, double c = 0.2)
        {
            MackeyGlassGeneratorSettings settings = new MackeyGlassGeneratorSettings(tau, b, c);
            MackeyGlassGenerator generator = new MackeyGlassGenerator(settings);
            return GenTimeSeries(generator, length);
        }

        /// <summary>
        /// Stores the time series in a csv file.
        /// </summary>
        /// <param name="fileName">The name of the output csv file.</param>
        /// <param name="valueColumnName">The name of the value column.</param>
        /// <param name="dataCollection">The data.</param>
        /// <param name="cultureInfo">The culture info object to be used.</param>
        public static void SaveTimeSeriesToCsvFile(string fileName, string valueColumnName, List<double> dataCollection, CultureInfo cultureInfo)
        {
            using StreamWriter streamWriter = new StreamWriter(new FileStream(fileName, FileMode.Create));
            streamWriter.WriteLine(valueColumnName);
            foreach (double value in dataCollection)
            {
                streamWriter.WriteLine(value.ToString("F20", cultureInfo));
            }
            return;
        }

    }//TimeSeriesGenerator

}//Namespace
