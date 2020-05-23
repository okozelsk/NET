using System;
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
        public VectorBundle(IEnumerable<double[]> inputVectorCollection, IEnumerable<double[]> outputVectorCollection)
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
        /// Loads the data and prepares VectorBundle for continuous feeding.
        /// The first line of the csv file must contain field names. These field names must
        /// match the names of the input and output fields.
        /// </summary>
        /// <param name="csvData"> Data in csv format</param>
        /// <param name="inputFieldNameCollection"> Input fields to be extracted from a file</param>
        /// <param name="outputFieldNameCollection"> Output fields to be extracted from a file</param>
        /// <param name="remainingInputVector"> Returned the last input vector unused in the bundle </param>
        public static VectorBundle Load(CsvDataHolder csvData,
                                        List<string> inputFieldNameCollection,
                                        List<string> outputFieldNameCollection,
                                        out double[] remainingInputVector
                                        )
        {
            remainingInputVector = null;
            List<int> inputFieldIndexes = new List<int>();
            List<int> outputFieldIndexes = new List<int>();
            if (inputFieldNameCollection != null)
            {
                //Check if the recognized data delimiter works properly
                if (csvData.ColNameCollection.NumOfStringValues < inputFieldNameCollection.Count)
                {
                    throw new FormatException("1st row of the file doesn't contain delimited column names or the value delimiter was not properly recognized.");
                }
                //Collect indexes of allowed input fields
                foreach (string name in inputFieldNameCollection)
                {
                    inputFieldIndexes.Add(csvData.ColNameCollection.IndexOf(name));
                }
            }
            else
            {
                int[] indexes = new int[csvData.ColNameCollection.NumOfStringValues];
                indexes.Indices();
                inputFieldIndexes = new List<int>(indexes);
            }
            for (int i = 0; i < outputFieldNameCollection.Count; i++)
            {
                outputFieldIndexes.Add(csvData.ColNameCollection.IndexOf(outputFieldNameCollection[i]));
            }
            //Prepare input and output vectors
            List<double[]> inputVectorCollection = new List<double[]>(csvData.DataRowCollection.Count);
            List<double[]> outputVectorCollection = new List<double[]>(csvData.DataRowCollection.Count);
            for (int i = 0; i < csvData.DataRowCollection.Count; i++)
            {
                //Input vector
                double[] inputVector = new double[inputFieldIndexes.Count];
                for (int j = 0; j < inputFieldIndexes.Count; j++)
                {
                    inputVector[j] = csvData.DataRowCollection[i].GetValue(inputFieldIndexes[j]).ParseDouble(true, $"Can't parse double value {csvData.DataRowCollection[i].GetValue(inputFieldIndexes[j])}.");
                }
                if (i < csvData.DataRowCollection.Count - 1)
                {
                    //Within the bundle
                    inputVectorCollection.Add(inputVector);
                }
                else
                {
                    //Remaining input vector out of the bundle
                    remainingInputVector = inputVector;
                }
                if (i > 0)
                {
                    //Output vector
                    double[] outputVector = new double[outputFieldIndexes.Count];
                    for (int j = 0; j < outputFieldIndexes.Count; j++)
                    {
                        outputVector[j] = csvData.DataRowCollection[i].GetValue(outputFieldIndexes[j]).ParseDouble(true, $"Can't parse double value {csvData.DataRowCollection[i].GetValue(outputFieldIndexes[j])}.");
                    }
                    outputVectorCollection.Add(outputVector);
                }
            }
            //Create and return bundle
            return new VectorBundle(inputVectorCollection, outputVectorCollection);
        }

        /// <summary>
        /// Loads the data and prepares VectorBundle for patterned feeding.
        /// The data row must begin with at least one complete set of values for defined repetitive variables.
        /// The data row must end with values of defined output variables.
        /// </summary>
        /// <param name="csvData"> Data in csv format</param>
        /// <param name="numOfOutputVariables"> Num of output variables (fields)</param>
        public static VectorBundle Load(CsvDataHolder csvData, int numOfOutputVariables)
        {
            VectorBundle bundle = new VectorBundle();
            foreach (DelimitedStringValues dataRow in csvData.DataRowCollection)
            {
                int numOfInputValues = dataRow.NumOfStringValues - numOfOutputVariables;
                //Check data length
                if (numOfInputValues <= 0)
                {
                    throw new FormatException("Incorrect length of data row.");
                }
                //Input data
                double[] inputData = new double[numOfInputValues];
                for (int i = 0; i < numOfInputValues; i++)
                {
                    inputData[i] = dataRow.GetValue(i).ParseDouble(true, $"Can't parse double data value {dataRow.GetValue(i)}.");
                }
                //Output data
                double[] outputData = new double[numOfOutputVariables];
                for (int i = 0; i < numOfOutputVariables; i++)
                {
                    outputData[i] = dataRow.GetValue(numOfInputValues + i).ParseDouble(true, $"Can't parse double data value {dataRow.GetValue(numOfInputValues + i)}.");
                }
                bundle.AddPair(inputData, outputData);
            }
            return bundle;
        }

        //Methods
        /// <summary>
        /// Splits this bundle to a collection of smaller bundles.
        /// Method expects length of the output vectors = 1.
        /// </summary>
        /// <param name="subBundleSize">Sub-bundle size</param>
        /// <param name="binBorder">If specified and there is only one output value, method will keep balanced number of output values GE to binBorder in the each sub-bundle</param>
        /// <returns>Collection of extracted sub-bundles</returns>
        public List<VectorBundle> Split(int subBundleSize, double binBorder = double.NaN)
        {
            int numOfBundles = OutputVectorCollection.Count / subBundleSize;
            List<VectorBundle> bundleCollection = new List<VectorBundle>(numOfBundles);
            if (!double.IsNaN(binBorder) && OutputVectorCollection[0].Length == 1)
            {
                BinDistribution refBinDistr = new BinDistribution(binBorder);
                refBinDistr.Update(OutputVectorCollection, 0);
                //Scan
                int[] bin0SampleIdxs = new int[refBinDistr.NumOf[0]];
                int bin0SamplesPos = 0;
                int[] bin1SampleIdxs = new int[refBinDistr.NumOf[1]];
                int bin1SamplesPos = 0;
                for (int i = 0; i < OutputVectorCollection.Count; i++)
                {
                    if (OutputVectorCollection[i][0] >= refBinDistr.BinBorder)
                    {
                        bin1SampleIdxs[bin1SamplesPos++] = i;
                    }
                    else
                    {
                        bin0SampleIdxs[bin0SamplesPos++] = i;
                    }
                }
                //Division
                int bundleBin0Count = Math.Max(1, refBinDistr.NumOf[0] / numOfBundles);
                int bundleBin1Count = Math.Max(1, refBinDistr.NumOf[1] / numOfBundles);
                if (bundleBin0Count * numOfBundles > bin0SampleIdxs.Length)
                {
                    throw new InvalidOperationException($"Insufficient bin 0 samples");
                }
                if (bundleBin1Count * numOfBundles > bin1SampleIdxs.Length)
                {
                    throw new InvalidOperationException($"Insufficient bin 1 samples");
                }
                //Bundles creation
                bin0SamplesPos = 0;
                bin1SamplesPos = 0;
                for (int bundleNum = 0; bundleNum < numOfBundles; bundleNum++)
                {
                    VectorBundle bundle = new VectorBundle();
                    //Bin 0
                    for (int i = 0; i < bundleBin0Count; i++)
                    {
                        bundle.InputVectorCollection.Add(InputVectorCollection[bin0SampleIdxs[bin0SamplesPos]]);
                        bundle.OutputVectorCollection.Add(OutputVectorCollection[bin0SampleIdxs[bin0SamplesPos]]);
                        ++bin0SamplesPos;
                    }
                    //Bin 1
                    for (int i = 0; i < bundleBin1Count; i++)
                    {
                        bundle.InputVectorCollection.Add(InputVectorCollection[bin1SampleIdxs[bin1SamplesPos]]);
                        bundle.OutputVectorCollection.Add(OutputVectorCollection[bin1SampleIdxs[bin1SamplesPos]]);
                        ++bin1SamplesPos;
                    }
                    bundleCollection.Add(bundle);
                }
                //Remaining samples
                for (int i = 0; i < bin0SampleIdxs.Length - bin0SamplesPos; i++)
                {
                    int bundleIdx = i % bundleCollection.Count;
                    bundleCollection[bundleIdx].InputVectorCollection.Add(InputVectorCollection[bin0SampleIdxs[bin0SamplesPos + i]]);
                    bundleCollection[bundleIdx].OutputVectorCollection.Add(OutputVectorCollection[bin0SampleIdxs[bin0SamplesPos + i]]);
                }
                for (int i = 0; i < bin1SampleIdxs.Length - bin1SamplesPos; i++)
                {
                    int bundleIdx = i % bundleCollection.Count;
                    bundleCollection[bundleIdx].InputVectorCollection.Add(InputVectorCollection[bin1SampleIdxs[bin1SamplesPos + i]]);
                    bundleCollection[bundleIdx].OutputVectorCollection.Add(OutputVectorCollection[bin1SampleIdxs[bin1SamplesPos + i]]);
                }
            }
            else
            {
                //Bundles creation
                int samplesPos = 0;
                for (int bundleNum = 0; bundleNum < numOfBundles; bundleNum++)
                {
                    VectorBundle bundle = new VectorBundle();
                    for (int i = 0; i < subBundleSize && samplesPos < OutputVectorCollection.Count; i++)
                    {
                        bundle.InputVectorCollection.Add(InputVectorCollection[samplesPos]);
                        bundle.OutputVectorCollection.Add(OutputVectorCollection[samplesPos]);
                        ++samplesPos;
                    }
                    bundleCollection.Add(bundle);
                }
                //Remaining samples
                for (int i = 0; i < OutputVectorCollection.Count - samplesPos; i++)
                {
                    int bundleIdx = i % bundleCollection.Count;
                    bundleCollection[bundleIdx].InputVectorCollection.Add(InputVectorCollection[samplesPos + i]);
                    bundleCollection[bundleIdx].OutputVectorCollection.Add(OutputVectorCollection[samplesPos + i]);
                }
            }
            return bundleCollection;
        }

        /// <summary>
        /// Adds data from given bundle into this bundle
        /// </summary>
        /// <param name="data">Data to be added</param>
        public void Add(VectorBundle data)
        {
            InputVectorCollection.AddRange(data.InputVectorCollection);
            OutputVectorCollection.AddRange(data.OutputVectorCollection);
            return;
        }

    }//VectorBundle

}//Namespace
