using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using RCNet.MathTools;
using RCNet.Extensions;
using RCNet.Neural.Network.Data;

namespace RCNet.CsvTools
{
    public static class TimeSeriesDataLoader
    {
        public static VectorsPairBundle Load(string fileName,
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
            VectorsPairBundle bundle = null;
            bundleNormalizer = new BundleNormalizer(normRange, normReserveRatio, dataStandardization, dataStandardization);
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
