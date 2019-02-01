﻿using System;
using System.Collections.Generic;
using System.IO;
using RCNet.MathTools;
using RCNet.Extensions;
using RCNet.CsvTools;

namespace RCNet.Neural.Data
{
    /// <summary>
    /// Bundle of input vector and desired output vector pairs
    /// </summary>
    [Serializable]
    public class VectorBundle
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
        public VectorBundle()
        {
            InputVectorCollection = new List<double[]>();
            OutputVectorCollection = new List<double[]>();
            return;
        }

        //Constructors
        /// <summary>
        /// Creates shallow copy of given lists
        /// </summary>
        /// <param name="inputVectorCollection">Collection of input vectors</param>
        /// <param name="outputVectorCollection">Collection of output vectors</param>
        public VectorBundle(List<double[]> inputVectorCollection, List<double[]> outputVectorCollection)
        {
            InputVectorCollection = new List<double[]>(inputVectorCollection);
            OutputVectorCollection = new List<double[]>(outputVectorCollection);
            return;
        }

        /// <summary>
        /// Instantiates an empty bundle
        /// </summary>
        /// <param name="expectedNumOfPairs">Expected number of sample pairs</param>
        public VectorBundle(int expectedNumOfPairs)
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
        /// The first line of the csv file must contain field names. These field names must
        /// match the names of the input and output fields.
        /// </summary>
        /// <param name="fileName"> Data file name </param>
        /// <param name="inputFieldNameCollection"> Input fields to be extracted from a file</param>
        /// <param name="outputFieldNameCollection"> Output fields to be extracted from a file</param>
        /// <param name="remainingInputVector"> Returned the last input vector unused in the bundle </param>
        public static VectorBundle LoadFromCsv(string fileName,
                                               List<string> inputFieldNameCollection,
                                               List<string> outputFieldNameCollection,
                                               out double[] remainingInputVector
                                               )
        {
            VectorBundle bundle = null;
            remainingInputVector = null;
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
                //Collect indexes of allowed fields
                foreach (string name in inputFieldNameCollection)
                {
                    inputFieldIndexes.Add(columnNames.IndexOf(name));
                }
                for (int i = 0; i < outputFieldNameCollection.Count; i++)
                {
                    outputFieldIndexes.Add(columnNames.IndexOf(outputFieldNameCollection[i]));
                }
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
                bundle = new VectorBundle(inputVectorCollection, outputVectorCollection);
            }
            return bundle;
        }//LoadFromCsv

    }//VectorBundle

}//Namespace
