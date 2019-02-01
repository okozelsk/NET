using System;
using System.Collections.Generic;
using System.IO;
using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.CsvTools;

namespace RCNet.Neural.Data
{
    /// <summary>
    /// Bundle of pattern and desired output vector pairs
    /// </summary>
    [Serializable]
    public class PatternBundle
    {
        //Attributes
        /// <summary>
        /// Collection of input patterns
        /// </summary>
        public List<List<double[]>> InputPatternCollection { get; }
        /// <summary>
        /// Collection of output vectors (desired values)
        /// </summary>
        public List<double[]> OutputVectorCollection { get; }

        //Constructors
        /// <summary>
        /// Instantiates data bundle.
        /// Creates shallow copy of given lists
        /// </summary>
        /// <param name="inputPatternCollection">Collection of input patterns</param>
        /// <param name="outputVectorCollection">Collection of output vectors</param>
        public PatternBundle(List<List<double[]>> inputPatternCollection, List<double[]> outputVectorCollection)
        {
            InputPatternCollection = new List<List<double[]>>(inputPatternCollection);
            OutputVectorCollection = new List<double[]>(outputVectorCollection);
            return;
        }

        /// <summary>
        /// Instantiates data bundle
        /// </summary>
        public PatternBundle()
        {
            InputPatternCollection = new List<List<double[]>>();
            OutputVectorCollection = new List<double[]>();
            return;
        }

        /// <summary>
        /// Adds pattern/vector pair into the bundle
        /// </summary>
        /// <param name="pattern">Input pattern of vectors</param>
        /// <param name="outputVector">Output vector (ideal)</param>
        public void AddPair(List<double[]> pattern, double[] outputVector)
        {
            InputPatternCollection.Add(pattern);
            OutputVectorCollection.Add(outputVector);
            return;
        }

        /// <summary>
        /// Shuffles stored pairs
        /// </summary>
        /// <param name="rand">Random object</param>
        public void Shuffle(System.Random rand)
        {
            List<List<double[]>> l1 = new List<List<double[]>>(InputPatternCollection);
            List<double[]> l2 = new List<double[]>(OutputVectorCollection);
            InputPatternCollection.Clear();
            OutputVectorCollection.Clear();
            int[] shuffledIndices = new int[l2.Count];
            shuffledIndices.ShuffledIndices(rand);
            for (int i = 0; i < shuffledIndices.Length; i++)
            {
                InputPatternCollection.Add(l1[shuffledIndices[i]]);
                OutputVectorCollection.Add(l2[shuffledIndices[i]]);
            }
            return;
        }

        //Static methods
        /// <summary>
        /// Loads the data and prepares PatternBundle.
        /// 1st row of the file must start with the #RepetitiveGroupOfAttributes keyword followed by
        /// attribute names.
        /// 2nd row of the file must start with the #Outputs keyword followed by
        /// output field names.
        /// 3rd+ rows are the data rows.
        /// The data row must begin with at least one complete set of values for defined repetitive attributes.
        /// The data row must end with values of defined output fields.
        /// </summary>
        /// <param name="fileName"> Data file name </param>
        /// <param name="inputFieldNameCollection"> Input fields to be extracted from a file</param>
        /// <param name="outputFieldNameCollection"> Output fields to be extracted from a file</param>
        public static PatternBundle LoadFromCsv(string fileName,
                                                List<string> inputFieldNameCollection,
                                                List<string> outputFieldNameCollection
                                                )
        {
            PatternBundle bundle = new PatternBundle();
            using (StreamReader streamReader = new StreamReader(new FileStream(fileName, FileMode.Open)))
            {
                List<int> inputFieldGrpIndexes = new List<int>();
                List<int> outputFieldIndexes = new List<int>();
                //The first row contains the "#RepetitiveGroupOfAttributes" keyword followed by name(s) of attribute(s)
                string delimitedRepetitiveGroupOfAttributes = streamReader.ReadLine();
                if (!delimitedRepetitiveGroupOfAttributes.StartsWith("#RepetitiveGroupOfAttributes"))
                {
                    throw new FormatException("1st row of the file doesn't start with the #RepetitiveGroupOfAttributes keyword.");
                }
                //What data delimiter is used?
                char csvDelimiter = DelimitedStringValues.RecognizeDelimiter(delimitedRepetitiveGroupOfAttributes);
                //Split column names
                DelimitedStringValues repetitiveGroupOfAttributes = new DelimitedStringValues(csvDelimiter);
                repetitiveGroupOfAttributes.LoadFromString(delimitedRepetitiveGroupOfAttributes);
                repetitiveGroupOfAttributes.RemoveTrailingWhites();
                //Check if the recognized data delimiter works properly
                if (repetitiveGroupOfAttributes.NumOfStringValues < 2)
                {
                    throw new FormatException("The value delimiter was not recognized or missing repetitive attribute(s) name(s).");
                }
                //Remove the #RepetitiveGroupOfAttributes keyword from the collection
                repetitiveGroupOfAttributes.RemoveAt(0);
                //Check if attribute names match with the input fields collection
                if (repetitiveGroupOfAttributes.NumOfStringValues < inputFieldNameCollection.Count)
                {
                    throw new FormatException("Inconsistent number of attributes in the file and number of specified input fields.");
                }
                foreach (string inputFieldName in inputFieldNameCollection)
                {
                    int index = repetitiveGroupOfAttributes.IndexOf(inputFieldName);
                    if (index < 0)
                    {
                        throw new FormatException($"Input field name {inputFieldName} was not found among the repetitive attributes specified in the file.");
                    }
                    inputFieldGrpIndexes.Add(index);
                }
                //The second row contains the "#Outputs" keyword followed by name(s) of output class(es) or values(s)
                string delimitedOutputNames = streamReader.ReadLine();
                if (!delimitedOutputNames.StartsWith("#Outputs"))
                {
                    throw new FormatException("2nd row of the file doesn't start with the #Outputs keyword.");
                }
                DelimitedStringValues outputNames = new DelimitedStringValues(csvDelimiter);
                outputNames.LoadFromString(delimitedOutputNames);
                outputNames.RemoveTrailingWhites();
                //Remove the #Outputs keyword from the collection
                outputNames.RemoveAt(0);
                //Check if the there is at least one output name
                if (outputNames.NumOfStringValues < 1)
                {
                    throw new FormatException("Missing output name(es).");
                }
                //Check if output names match with the output fields collection
                if (outputNames.NumOfStringValues < outputFieldNameCollection.Count)
                {
                    throw new FormatException("Inconsistent number of outputs in the file and number of specified output fields.");
                }
                foreach (string outputFieldName in outputFieldNameCollection)
                {
                    int index = outputNames.IndexOf(outputFieldName);
                    if (index < 0)
                    {
                        throw new FormatException($"Output field name {outputFieldName} was not found among the outputs specified in the file.");
                    }
                    outputFieldIndexes.Add(index);
                }
                //Load data
                DelimitedStringValues dataRow = new DelimitedStringValues(csvDelimiter);
                while (!streamReader.EndOfStream)
                {
                    dataRow.LoadFromString(streamReader.ReadLine());
                    dataRow.RemoveTrailingWhites();
                    //Check data length
                    if (dataRow.NumOfStringValues < repetitiveGroupOfAttributes.NumOfStringValues + outputNames.NumOfStringValues ||
                       ((dataRow.NumOfStringValues - outputNames.NumOfStringValues) % repetitiveGroupOfAttributes.NumOfStringValues) != 0)
                    {
                        throw new FormatException("Incorrect length of data row.");
                    }
                    //Pattern data
                    List<double[]> patternData = new List<double[]>();
                    for (int grpIdx = 0; grpIdx < (dataRow.NumOfStringValues - outputNames.NumOfStringValues) / repetitiveGroupOfAttributes.NumOfStringValues; grpIdx++)
                    {
                        double[] inputVector = new double[inputFieldGrpIndexes.Count];
                        for(int i = 0; i < inputFieldGrpIndexes.Count; i++)
                        {
                            inputVector[i] = dataRow.GetValue(grpIdx * repetitiveGroupOfAttributes.NumOfStringValues + inputFieldGrpIndexes[i]).ParseDouble(true, $"Can't parse double data value {dataRow.GetValue(grpIdx * repetitiveGroupOfAttributes.NumOfStringValues + inputFieldGrpIndexes[i])}.");
                        }
                        patternData.Add(inputVector);
                    }//grpIdx
                    //Output data
                    double[] outputVector = new double[outputFieldIndexes.Count];
                    int dataRowStartIdx = dataRow.NumOfStringValues - outputNames.NumOfStringValues;
                    for (int i = 0; i < outputFieldIndexes.Count; i++)
                    {
                        outputVector[i] = dataRow.GetValue(dataRowStartIdx + outputFieldIndexes[i]).ParseDouble(true, $"Can't parse double value {dataRow.GetValue(dataRowStartIdx + outputFieldIndexes[i])}.");
                    }
                    bundle.AddPair(patternData, outputVector);
                }//while !EOF
            }//using streamReader
            return bundle;
        }//LoadFromCsv

    }//PatternBundle

}//Namespace
