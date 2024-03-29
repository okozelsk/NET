﻿using RCNet.CsvTools;
using RCNet.Extensions;
using RCNet.MathTools;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace RCNet.Neural.Data
{
    /// <summary>
    /// Implements a bundle of input and output data vector pairs.
    /// </summary>
    [Serializable]
    public class VectorBundle
    {
        //Constants
        /// <summary>
        /// The maximum ratio of one data fold.
        /// </summary>
        public const double MaxRatioOfFoldData = 0.5d;


        //Attributes
        /// <summary>
        /// The collection of input vectors.
        /// </summary>
        public List<double[]> InputVectorCollection { get; }

        /// <summary>
        /// The collection of output vectors.
        /// </summary>
        public List<double[]> OutputVectorCollection { get; }

        //Constructors
        /// <summary>
        /// Creates an uninitialized instance.
        /// </summary>
        public VectorBundle()
        {
            InputVectorCollection = new List<double[]>();
            OutputVectorCollection = new List<double[]>();
            return;
        }


        /// <summary>
        /// Creates an uninitialized instance.
        /// </summary>
        /// <param name="expectedNumOfPairs">The expected number of vector pairs.</param>
        public VectorBundle(int expectedNumOfPairs)
        {
            InputVectorCollection = new List<double[]>(expectedNumOfPairs);
            OutputVectorCollection = new List<double[]>(expectedNumOfPairs);
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="inputVectorCollection">The collection of input vectors.</param>
        /// <param name="outputVectorCollection">The collection of output vectors.</param>
        public VectorBundle(IEnumerable<double[]> inputVectorCollection, IEnumerable<double[]> outputVectorCollection)
        {
            InputVectorCollection = new List<double[]>(inputVectorCollection);
            OutputVectorCollection = new List<double[]>(outputVectorCollection);
            return;
        }

        //Static methods
        /// <summary>
        /// Loads the vector bundle from the csv data (continuous input feeding).
        /// </summary>
        /// <param name="csvData">The csv data.</param>
        /// <param name="inputFieldNameCollection">The names of input fields.</param>
        /// <param name="outputFieldNameCollection">The names of output fields.</param>
        /// <param name="remainingInputVector">The last unused input vector.</param>
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
                //Check the number of fields
                if (csvData.ColNameCollection.NumOfStringValues < inputFieldNameCollection.Count)
                {
                    throw new ArgumentException("The number of column names in csv data is less than the number of the input fields.", "csvData");
                }
                //Collect indexes of allowed input fields
                foreach (string name in inputFieldNameCollection)
                {
                    int fieldIdx = csvData.ColNameCollection.IndexOf(name);
                    if (fieldIdx == -1)
                    {
                        throw new ArgumentException($"The input field name {name} was not found in the csv data column names.", "csvData");
                    }
                    inputFieldIndexes.Add(fieldIdx);
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
                int fieldIdx = csvData.ColNameCollection.IndexOf(outputFieldNameCollection[i]);
                if (fieldIdx == -1)
                {
                    throw new ArgumentException($"The output field name {outputFieldNameCollection[i]} was not found in the csv data column names.", "csvData");
                }
                outputFieldIndexes.Add(fieldIdx);
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
                    inputVector[j] = csvData.DataRowCollection[i].GetValueAt(inputFieldIndexes[j]).ParseDouble(true, $"Can't parse double value {csvData.DataRowCollection[i].GetValueAt(inputFieldIndexes[j])}.");
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
                        outputVector[j] = csvData.DataRowCollection[i].GetValueAt(outputFieldIndexes[j]).ParseDouble(true, $"Can't parse double value {csvData.DataRowCollection[i].GetValueAt(outputFieldIndexes[j])}.");
                    }
                    outputVectorCollection.Add(outputVector);
                }
            }
            //Create and return bundle
            return new VectorBundle(inputVectorCollection, outputVectorCollection);
        }

        /// <summary>
        /// Loads the vector bundle from the csv data (patterned input feeding).
        /// </summary>
        /// <param name="csvData">The csv data.</param>
        /// <param name="numOfOutputFields">The number of output fields.</param>
        public static VectorBundle Load(CsvDataHolder csvData, int numOfOutputFields)
        {
            VectorBundle bundle = new VectorBundle();
            foreach (DelimitedStringValues dataRow in csvData.DataRowCollection)
            {
                int numOfInputValues = dataRow.NumOfStringValues - numOfOutputFields;
                //Check data length
                if (numOfInputValues <= 0)
                {
                    throw new ArgumentException("Incorrect length of data row.", "csvData");
                }
                //Input data
                double[] inputData = new double[numOfInputValues];
                for (int i = 0; i < numOfInputValues; i++)
                {
                    inputData[i] = dataRow.GetValueAt(i).ParseDouble(true, $"Can't parse double data value {dataRow.GetValueAt(i)}.");
                }
                //Output data
                double[] outputData = new double[numOfOutputFields];
                for (int i = 0; i < numOfOutputFields; i++)
                {
                    outputData[i] = dataRow.GetValueAt(numOfInputValues + i).ParseDouble(true, $"Can't parse double data value {dataRow.GetValueAt(numOfInputValues + i)}.");
                }
                bundle.AddPair(inputData, outputData);
            }
            return bundle;
        }

        //Methods
        /// <summary>
        /// Adds the vector pair into the bundle.
        /// </summary>
        /// <param name="inputVector">The input vector.</param>
        /// <param name="outputVector">The output vector.</param>
        public void AddPair(double[] inputVector, double[] outputVector)
        {
            InputVectorCollection.Add(inputVector);
            OutputVectorCollection.Add(outputVector);
            return;
        }

        /// <summary>
        /// Adds all the vector pairs from another vector bundle.
        /// </summary>
        /// <param name="data">Another vector bundle.</param>
        public void Add(VectorBundle data)
        {
            InputVectorCollection.AddRange(data.InputVectorCollection);
            OutputVectorCollection.AddRange(data.OutputVectorCollection);
            return;
        }

        /// <summary>
        /// Shuffles the vector pairs.
        /// </summary>
        /// <param name="rand">The random object to be used.</param>
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

        /// <summary>
        /// Creates the shallow copy of this bundle.
        /// </summary>
        public VectorBundle CreateShallowCopy()
        {
            return new VectorBundle(new List<double[]>(InputVectorCollection), new List<double[]>(OutputVectorCollection));
        }

        /// <summary>
        /// Splits this bundle to a collection of smaller folds (sub-bundles) suitable for the cross-validation.
        /// </summary>
        /// <param name="foldDataRatio">The requested ratio of the samples constituting the single fold (sub-bundle).</param>
        /// <param name="binBorder">When the binBorder is specified then all the output features are considered as binary features within the one-takes-all group and function then keeps balanced ratios of 0 and 1 for every output feature and the fold.</param>
        /// <returns>A collection of the created folds.</returns>
        public List<VectorBundle> Folderize(double foldDataRatio, double binBorder = double.NaN)
        {
            if (OutputVectorCollection.Count < 2)
            {
                throw new InvalidOperationException($"Insufficient number of samples ({OutputVectorCollection.Count.ToString(CultureInfo.InvariantCulture)}).");
            }
            List<VectorBundle> foldCollection = new List<VectorBundle>();
            //Fold data ratio basic correction
            if (foldDataRatio > MaxRatioOfFoldData)
            {
                foldDataRatio = MaxRatioOfFoldData;
            }
            //Prelimitary fold size estimation
            int foldSize = Math.Max(1, (int)Math.Round(OutputVectorCollection.Count * foldDataRatio, 0));
            //Prelimitary number of folds
            int numOfFolds = (int)Math.Round((double)OutputVectorCollection.Count / foldSize);
            //Folds creation
            if (double.IsNaN(binBorder))
            {
                //No binary output -> simple split
                int samplesPos = 0;
                for (int foldIdx = 0; foldIdx < numOfFolds; foldIdx++)
                {
                    VectorBundle fold = new VectorBundle();
                    for (int i = 0; i < foldSize && samplesPos < OutputVectorCollection.Count; i++)
                    {
                        fold.InputVectorCollection.Add(InputVectorCollection[samplesPos]);
                        fold.OutputVectorCollection.Add(OutputVectorCollection[samplesPos]);
                        ++samplesPos;
                    }
                    foldCollection.Add(fold);
                }
                //Remaining samples
                for (int i = 0; i < OutputVectorCollection.Count - samplesPos; i++)
                {
                    int foldIdx = i % foldCollection.Count;
                    foldCollection[foldIdx].InputVectorCollection.Add(InputVectorCollection[samplesPos + i]);
                    foldCollection[foldIdx].OutputVectorCollection.Add(OutputVectorCollection[samplesPos + i]);
                }
            }//Indifferent output
            else
            {
                //Binary outputs -> keep balanced ratios of outputs
                int numOfOutputs = OutputVectorCollection[0].Length;
                if (numOfOutputs == 1)
                {
                    //Special case there is only one binary output
                    //Investigation of the output data metrics
                    BinDistribution refBinDistr = new BinDistribution(binBorder);
                    refBinDistr.Update(OutputVectorCollection, 0);
                    int min01 = Math.Min(refBinDistr.NumOf[0], refBinDistr.NumOf[1]);
                    if (min01 < 2)
                    {
                        throw new InvalidOperationException($"Insufficient bin 0 or 1 samples (less than 2).");
                    }
                    if (numOfFolds > min01)
                    {
                        numOfFolds = min01;
                    }
                    //Scan data
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
                    //Determine distributions of 0 and 1 for one fold
                    int bundleBin0Count = Math.Max(1, refBinDistr.NumOf[0] / numOfFolds);
                    int bundleBin1Count = Math.Max(1, refBinDistr.NumOf[1] / numOfFolds);
                    //Bundles creation
                    bin0SamplesPos = 0;
                    bin1SamplesPos = 0;
                    for (int foldIdx = 0; foldIdx < numOfFolds; foldIdx++)
                    {
                        VectorBundle fold = new VectorBundle();
                        //Bin 0
                        for (int i = 0; i < bundleBin0Count; i++)
                        {
                            fold.InputVectorCollection.Add(InputVectorCollection[bin0SampleIdxs[bin0SamplesPos]]);
                            fold.OutputVectorCollection.Add(OutputVectorCollection[bin0SampleIdxs[bin0SamplesPos]]);
                            ++bin0SamplesPos;
                        }
                        //Bin 1
                        for (int i = 0; i < bundleBin1Count; i++)
                        {
                            fold.InputVectorCollection.Add(InputVectorCollection[bin1SampleIdxs[bin1SamplesPos]]);
                            fold.OutputVectorCollection.Add(OutputVectorCollection[bin1SampleIdxs[bin1SamplesPos]]);
                            ++bin1SamplesPos;
                        }
                        foldCollection.Add(fold);
                    }
                    //Remaining samples
                    for (int i = 0; i < bin0SampleIdxs.Length - bin0SamplesPos; i++)
                    {
                        int foldIdx = i % foldCollection.Count;
                        foldCollection[foldIdx].InputVectorCollection.Add(InputVectorCollection[bin0SampleIdxs[bin0SamplesPos + i]]);
                        foldCollection[foldIdx].OutputVectorCollection.Add(OutputVectorCollection[bin0SampleIdxs[bin0SamplesPos + i]]);
                    }
                    for (int i = 0; i < bin1SampleIdxs.Length - bin1SamplesPos; i++)
                    {
                        int foldIdx = i % foldCollection.Count;
                        foldCollection[foldIdx].InputVectorCollection.Add(InputVectorCollection[bin1SampleIdxs[bin1SamplesPos + i]]);
                        foldCollection[foldIdx].OutputVectorCollection.Add(OutputVectorCollection[bin1SampleIdxs[bin1SamplesPos + i]]);
                    }
                }//Only 1 binary output
                else
                {
                    //There is more than 1 binary output - "one takes all approach"
                    //Investigation of the output data metrics
                    //Collect bin 1 sample indexes and check "one takes all" consistency for every output feature
                    List<int>[] outBin1SampleIdxs = new List<int>[numOfOutputs];
                    for (int i = 0; i < numOfOutputs; i++)
                    {
                        outBin1SampleIdxs[i] = new List<int>();
                    }
                    for (int sampleIdx = 0; sampleIdx < OutputVectorCollection.Count; sampleIdx++)
                    {
                        int numOf1 = 0;
                        for (int outFeatureIdx = 0; outFeatureIdx < numOfOutputs; outFeatureIdx++)
                        {
                            if (OutputVectorCollection[sampleIdx][outFeatureIdx] >= binBorder)
                            {
                                outBin1SampleIdxs[outFeatureIdx].Add(sampleIdx);
                                ++numOf1;
                            }
                        }
                        if (numOf1 != 1)
                        {
                            throw new ArgumentException($"Data are inconsistent on data index {sampleIdx.ToString(CultureInfo.InvariantCulture)}. Output vector has {numOf1.ToString(CultureInfo.InvariantCulture)} feature(s) having bin value 1.", "binBorder");
                        }
                    }
                    //Determine max possible number of folds
                    int maxNumOfFolds = OutputVectorCollection.Count;
                    for (int outFeatureIdx = 0; outFeatureIdx < numOfOutputs; outFeatureIdx++)
                    {
                        int outFeatureMaxFolds = Math.Min(outBin1SampleIdxs[outFeatureIdx].Count, OutputVectorCollection.Count - outBin1SampleIdxs[outFeatureIdx].Count);
                        maxNumOfFolds = Math.Min(outFeatureMaxFolds, maxNumOfFolds);
                    }
                    //Correct the number of folds to be created
                    if (numOfFolds > maxNumOfFolds)
                    {
                        numOfFolds = maxNumOfFolds;
                    }
                    //Create the folds
                    for (int foldIdx = 0; foldIdx < numOfFolds; foldIdx++)
                    {
                        foldCollection.Add(new VectorBundle());
                    }
                    //Samples distribution
                    for (int outFeatureIdx = 0; outFeatureIdx < numOfOutputs; outFeatureIdx++)
                    {
                        for (int bin1SampleRefIdx = 0; bin1SampleRefIdx < outBin1SampleIdxs[outFeatureIdx].Count; bin1SampleRefIdx++)
                        {
                            int foldIdx = bin1SampleRefIdx % foldCollection.Count;
                            int dataIdx = outBin1SampleIdxs[outFeatureIdx][bin1SampleRefIdx];
                            foldCollection[foldIdx].AddPair(InputVectorCollection[dataIdx], OutputVectorCollection[dataIdx]);
                        }
                    }
                }//More binary outputs
            }//Binary output

            return foldCollection;
        }

    }//VectorBundle

}//Namespace
