using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using RCNet.MathTools;
using RCNet.Extensions;
using RCNet.CsvTools;

namespace RCNet.Neural.Data
{
    /// <summary>
    /// The class allows to upload sample data for a Prediction task from a csv file.
    /// </summary>
    public static class TimeSeriesDataLoader
    {
        /// <summary>
        /// Loads the data and prepares PredictionBundle.
        /// The first line of the csv file must be field names. These field names must
        /// match the names of the input and output fields.
        /// </summary>
        /// <param name="fileName">
        /// Data file name
        /// </param>
        /// <param name="inputFieldNameCollection">
        /// Input fields
        /// </param>
        /// <param name="outputFieldNameCollection">
        /// Output fields
        /// </param>
        /// <param name="normRange">
        /// Range of normalized values
        /// </param>
        /// <param name="normReserveRatio">
        /// Reserve held by a normalizer to cover cases where future data exceeds a known range of sample data.
        /// </param>
        /// <param name="dataStandardization">
        /// Specifies whether to apply data standardization
        /// </param>
        /// <param name="singleNormalizer">
        /// Use true if all input and output fields are about the same range of values.
        /// </param>
        /// <param name="bundleNormalizer">
        /// Returned initialized instance of BundleNormalizer.
        /// </param>
        /// <param name="remainingInputVector">
        /// Returned the last input vector unused in the bundle.
        /// </param>
        public static TimeSeriesBundle Load(string fileName,
                                             List<string> inputFieldNameCollection,
                                             List<string> outputFieldNameCollection,
                                             Interval normRange,
                                             double normReserveRatio,
                                             bool dataStandardization,
                                             bool singleNormalizer,
                                             out BundleNormalizer bundleNormalizer,
                                             out double[] remainingInputVector
                                             )
        {
            TimeSeriesBundle bundle = null;
            bundleNormalizer = new BundleNormalizer(normRange, normReserveRatio, dataStandardization, normReserveRatio, dataStandardization);
            using (StreamReader streamReader = new StreamReader(new FileStream(fileName, FileMode.Open)))
            {
                List<int> fieldIndexes = new List<int>();
                List<double[]> allData = new List<double[]>();
                //First row contains column names (data fields)
                string delimitedColumnNames = streamReader.ReadLine();
                //What data delimiter is used?
                char csvDelimiter = DelimitedStringValues.RecognizeDelimiter(delimitedColumnNames);
                //Split column names
                DelimitedStringValues columnNames = new DelimitedStringValues(csvDelimiter);
                columnNames.LoadFromString(delimitedColumnNames);
                //Check if the recognized data delimiter works properly
                if (columnNames.NumOfStringValues < inputFieldNameCollection.Count)
                {
                    throw new FormatException("1st row of the file doesn't contain delimited column names or the value delimiter was not properly recognized.");
                }
                //Define fields
                foreach (string name in inputFieldNameCollection)
                {
                    if (!bundleNormalizer.IsFieldDefined(name))
                    {
                        bundleNormalizer.DefineField(name, singleNormalizer ? "COMMON" : name);
                        fieldIndexes.Add(columnNames.IndexOf(name));
                    }
                    bundleNormalizer.DefineInputField(name);
                }
                foreach (string name in outputFieldNameCollection)
                {
                    if (!bundleNormalizer.IsFieldDefined(name))
                    {
                        bundleNormalizer.DefineField(name, singleNormalizer ? "COMMON" : name);
                        fieldIndexes.Add(columnNames.IndexOf(name));
                    }
                    bundleNormalizer.DefineOutputField(name);
                }
                //Finalize structure
                bundleNormalizer.FinalizeStructure();
                //Load all relevant data
                DelimitedStringValues dataRow = new DelimitedStringValues(csvDelimiter);
                while (!streamReader.EndOfStream)
                {
                    dataRow.LoadFromString(streamReader.ReadLine());
                    double[] vector = new double[fieldIndexes.Count];
                    for (int i = 0; i < fieldIndexes.Count; i++)
                    {
                        vector[i] = dataRow.GetValue(fieldIndexes[i]).ParseDouble(true, $"Can't parse double value {dataRow.GetValue(fieldIndexes[i])}.");
                    }
                    allData.Add(vector);
                }
                //Create data bundle
                remainingInputVector = bundleNormalizer.CreateBundleFromVectorCollection(allData, true, out bundle);
            }
            return bundle;
        }//Load

    }//TimeSeriesDataLoader

}//Namespace
