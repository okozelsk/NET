using System;
using System.Collections.Generic;
using System.IO;
using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.CsvTools;
using RCNet.Neural.Network.SM.Preprocessing;

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
        public PatternBundle(IEnumerable<List<double[]>> inputPatternCollection, IEnumerable<double[]> outputVectorCollection)
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
        /// Converts one dimensional data array to a pattern.
        /// </summary>
        /// <param name="data">Data array</param>
        /// <param name="numOfVariables">Number of variables at one timepoint</param>
        /// <param name="patternedDataOrganization">Type of variables organization: groupped [v1(t1),v2(t1),v1(t2),v2(t2),v1(t3),v2(t3)] or sequential [v1(t1),v1(t2),v1(t3),v2(t1),v2(t2),v2(t3)]</param>
        public static List<double[]> ArrayToPattern(double[] data, int numOfVariables, NeuralPreprocessorSettings.InputSettings.PatternedDataOrganization patternedDataOrganization)
        {
            //Check data length
            if (data.Length < numOfVariables || (data.Length % numOfVariables) != 0)
            {
                throw new FormatException("Incorrect length of data array.");
            }
            //Pattern data
            List<double[]> patternData = new List<double[]>(data.Length / numOfVariables);
            if (patternedDataOrganization == NeuralPreprocessorSettings.InputSettings.PatternedDataOrganization.Groupped)
            {
                //Groupped format
                for (int grpIdx = 0; grpIdx < data.Length / numOfVariables; grpIdx++)
                {
                    double[] inputVector = new double[numOfVariables];
                    for (int i = 0; i < numOfVariables; i++)
                    {
                        inputVector[i] = data[grpIdx * numOfVariables + i];
                    }
                    patternData.Add(inputVector);
                }//grpIdx
            }
            else
            {
                //Sequential format
                int timePoints = data.Length / numOfVariables;
                for(int timeIdx = 0; timeIdx < timePoints; timeIdx++)
                {
                    double[] inputVector = new double[numOfVariables];
                    for (int i = 0; i < numOfVariables; i++)
                    {
                        inputVector[i] = data[i * timePoints + timeIdx];
                    }
                    patternData.Add(inputVector);
                }
            }
            return patternData;
        }

        /// <summary>
        /// Loads the data and prepares PatternBundle.
        /// The data row must begin with at least one complete set of values for defined repetitive variables.
        /// The data row must end with values of defined output variables.
        /// </summary>
        /// <param name="fileName"> Data file name </param>
        /// <param name="numOfInputVariables"> Number of repetitive input variables (fields)</param>
        /// <param name="patternedDataOrganization">Type of input variables organization: groupped [v1(t1),v2(t1),v1(t2),v2(t2),v1(t3),v2(t3)] or sequential [v1(t1),v1(t2),v1(t3),v2(t1),v2(t2),v2(t3)]</param>
        /// <param name="numOfOutputVariables"> Num of output variables (fields)</param>
        public static PatternBundle LoadFromCsv(string fileName,
                                                int numOfInputVariables,
                                                NeuralPreprocessorSettings.InputSettings.PatternedDataOrganization patternedDataOrganization,
                                                int numOfOutputVariables
                                                )
        {
            PatternBundle bundle = new PatternBundle();
            CsvDataHolder cdh = new CsvDataHolder(fileName, false);
            foreach(DelimitedStringValues dataRow in cdh.DataRowCollection)
            {
                int numOfInputValues = dataRow.NumOfStringValues - numOfOutputVariables;
                //Check data length
                if (dataRow.NumOfStringValues < numOfInputVariables + numOfOutputVariables ||
                   (numOfInputValues % numOfInputVariables) != 0)
                {
                    throw new FormatException("Incorrect length of data row.");
                }
                //Input data
                double[] inputData = new double[numOfInputValues];
                for(int i = 0; i < numOfInputValues; i++)
                {
                    inputData[i] = dataRow.GetValue(i).ParseDouble(true, $"Can't parse double data value {dataRow.GetValue(i)}.");
                }
                //Output data
                double[] outputData = new double[numOfOutputVariables];
                for (int i = 0; i < numOfOutputVariables; i++)
                {
                    outputData[i] = dataRow.GetValue(numOfInputValues + i).ParseDouble(true, $"Can't parse double data value {dataRow.GetValue(numOfInputValues + i)}.");
                }
                //Convert to pattern
                List<double[]> patternData = ArrayToPattern(inputData, numOfInputVariables, patternedDataOrganization);
                bundle.AddPair(patternData, outputData);
            }
            return bundle;
        }//LoadFromCsv

    }//PatternBundle

}//Namespace
