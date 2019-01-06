using System;
using System.Collections.Generic;
using System.IO;
using RCNet.MathTools;
using RCNet.Extensions;
using RCNet.CsvTools;

namespace RCNet.Neural.Data
{
    /// <summary>
    /// Bundle of input vector and desired output vector
    /// </summary>
    [Serializable]
    public class TimeSeriesBundle
    {
        //Attributes
        /// <summary>
        /// Collection of input vectors
        /// </summary>
        public List<double[]> InputVectorCollection { get; }
        
        /// <summary>
        /// Collection of output vectors (desired values)
        /// </summary>
        public List<double[]> OutputVectorCollection { get; }

        //Constructors
        /// <summary>
        /// Instantiates data bundle
        /// </summary>
        public TimeSeriesBundle()
        {
            InputVectorCollection = new List<double[]>();
            OutputVectorCollection = new List<double[]>();
            return;
        }

        //Constructors
        /// <summary>
        /// Instantiates data bundle.
        /// Creates shallow copy of given lists
        /// </summary>
        /// <param name="inputVectorCollection">Collection of input vectors</param>
        /// <param name="outputVectorCollection">Collection of output vectors</param>
        public TimeSeriesBundle(List<double[]> inputVectorCollection, List<double[]> outputVectorCollection)
        {
            InputVectorCollection = new List<double[]>(inputVectorCollection);
            OutputVectorCollection = new List<double[]>(outputVectorCollection);
            return;
        }

        /// <summary>
        /// Instantiates data bundle
        /// </summary>
        /// <param name="expectedNumOfPairs">Expected number of sample pairs</param>
        public TimeSeriesBundle(int expectedNumOfPairs)
        {
            InputVectorCollection = new List<double[]>(expectedNumOfPairs);
            OutputVectorCollection = new List<double[]>(expectedNumOfPairs);
            return;
        }

        /// <summary>
        /// Adds sample data pair into the bundle
        /// </summary>
        /// <param name="inputVector">Input vector</param>
        /// <param name="outputVector">Output vector (ideal)</param>
        public void AddPair(double[] inputVector, double[] outputVector)
        {
            InputVectorCollection.Add(inputVector);
            OutputVectorCollection.Add(outputVector);
            return;
        }

        /// <summary>
        /// Shuffles stored pairs
        /// </summary>
        /// <param name="rand">Random object</param>
        public void Shuffle(Random rand)
        {
            List<double[]> l1 = new List<double[]>(InputVectorCollection);
            List<double[]> l2 = new List<double[]>(OutputVectorCollection);
            InputVectorCollection.Clear();
            OutputVectorCollection.Clear();
            int[] shuffledIndices = new int[l2.Count];
            shuffledIndices.ShuffledIndices(rand);
            for (int i = 0; i < shuffledIndices.Length; i++)
            {
                InputVectorCollection.Add(l1[shuffledIndices[i]]);
                OutputVectorCollection.Add(l2[shuffledIndices[i]]);
            }
            return;
        }

        //Static methods
        /// <summary>
        /// Loads the data and prepares TimeSeriesBundle.
        /// The first line of the csv file must be field names. These field names must
        /// match the names of the input and output fields.
        /// </summary>
        /// <param name="fileName"> Data file name </param>
        /// <param name="inputFieldNameCollection"> Input field names </param>
        /// <param name="outputFieldNameCollection"> Output field names </param>
        /// <param name="outputFieldTaskCollection">
        /// Neural task related to output field.
        /// Classification task means the output field contains binary value so data
        /// standardization and normalizer reserve are suppressed.
        /// </param>
        /// <param name="normRange"> Range of normalized values </param>
        /// <param name="normReserveRatio">
        /// Reserve held by a normalizer to cover cases where future data exceeds a known range of sample data.
        /// </param>
        /// <param name="dataStandardization"> Specifies whether to apply data standardization </param>
        /// <param name="bundleNormalizer"> Returned initialized instance of BundleNormalizer </param>
        /// <param name="remainingInputVector"> Returned the last input vector unused in the bundle </param>
        public static TimeSeriesBundle LoadFromCsv(string fileName,
                                                   List<string> inputFieldNameCollection,
                                                   List<string> outputFieldNameCollection,
                                                   List<CommonEnums.TaskType> outputFieldTaskCollection,
                                                   Interval normRange,
                                                   double normReserveRatio,
                                                   bool dataStandardization,
                                                   out BundleNormalizer bundleNormalizer,
                                                   out double[] remainingInputVector
                                                   )
        {
            TimeSeriesBundle bundle = null;
            remainingInputVector = null;
            bundleNormalizer = new BundleNormalizer(normRange);
            using (StreamReader streamReader = new StreamReader(new FileStream(fileName, FileMode.Open)))
            {
                List<int> inputFieldIndexes = new List<int>();
                List<int> outputFieldIndexes = new List<int>();
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
                        bundleNormalizer.DefineField(name, name, normReserveRatio, dataStandardization);
                        inputFieldIndexes.Add(columnNames.IndexOf(name));
                    }
                    bundleNormalizer.DefineInputField(name);
                }
                for (int i = 0; i < outputFieldNameCollection.Count; i++)
                {
                    if (!bundleNormalizer.IsFieldDefined(outputFieldNameCollection[i]))
                    {
                        bundleNormalizer.DefineField(outputFieldNameCollection[i],
                                                     outputFieldNameCollection[i],
                                                     outputFieldTaskCollection[i] == CommonEnums.TaskType.Classification ? 0 : normReserveRatio,
                                                     outputFieldTaskCollection[i] == CommonEnums.TaskType.Classification ? false : dataStandardization
                                                     );
                    }
                    outputFieldIndexes.Add(columnNames.IndexOf(outputFieldNameCollection[i]));
                    bundleNormalizer.DefineOutputField(outputFieldNameCollection[i]);
                }
                //Finalize structure
                bundleNormalizer.FinalizeStructure();
                //Load full data in string form
                List<DelimitedStringValues> fullData = new List<DelimitedStringValues>();
                while (!streamReader.EndOfStream)
                {
                    DelimitedStringValues row = new DelimitedStringValues(csvDelimiter);
                    row.LoadFromString(streamReader.ReadLine());
                    fullData.Add(row);
                }
                //Prepare input and output vectors
                List<double[]> inputVectorCollection = new List<double[]>(fullData.Count);
                List<double[]> outputVectorCollection = new List<double[]>(fullData.Count);
                for (int i = 0; i < fullData.Count; i++)
                {
                    //Input vector
                    double[] inputVector = new double[inputFieldIndexes.Count];
                    for(int j = 0; j < inputFieldIndexes.Count; j++)
                    {
                        inputVector[j] = fullData[i].GetValue(inputFieldIndexes[j]).ParseDouble(true, $"Can't parse double value {fullData[i].GetValue(inputFieldIndexes[j])}.");
                    }
                    if (i < fullData.Count - 1)
                    {
                        //Within the bundle
                        inputVectorCollection.Add(inputVector);
                    }
                    else
                    {
                        //remaining input vector out of the bundle
                        remainingInputVector = inputVector;
                    }
                    if (i > 0)
                    {
                        //Output vector
                        double[] outputVector = new double[outputFieldIndexes.Count];
                        for (int j = 0; j < outputFieldIndexes.Count; j++)
                        {
                            outputVector[j] = fullData[i].GetValue(outputFieldIndexes[j]).ParseDouble(true, $"Can't parse double value {fullData[i].GetValue(outputFieldIndexes[j])}.");
                        }
                        outputVectorCollection.Add(outputVector);
                    }
                }
                //Create bundle
                bundle = new TimeSeriesBundle(inputVectorCollection, outputVectorCollection);
                //Normalize bundle and remaining input vector
                bundleNormalizer.Normalize(bundle);
                bundleNormalizer.NormalizeInputVector(remainingInputVector);
            }
            return bundle;
        }//LoadFromCsv

    }//TimeSeriesBundle

}//Namespace
