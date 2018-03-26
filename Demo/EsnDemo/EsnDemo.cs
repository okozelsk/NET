using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using RCNet.MathTools;
using RCNet.Extensions;
using RCNet.CsvTools;
using RCNet.Neural.Network.Data;
using RCNet.Neural.Network.EchoState;
using RCNet.Demo.Log;


namespace RCNet.Demo
{
    /// <summary>
    /// Demonstrates the Esn usage.
    /// It performs training-->prediction operations sequence for each demo case defined in xml file.
    /// Input time series data has to be stored in a file (csv format).
	/// You can simply modify xml and configure your own training-->prediction sessions.
    /// </summary>
    public static class EsnDemo
    {
        /// <summary>
        /// Helper function.
        /// Loads the data from csv file, prepares output normalizer(s) and creates SamplesDataBundle object containing
        /// standardized and normalized data to be used for Esn training, testing and prediction.
        /// Note that all data is always normalized to the interval (-1.1), so only activation functions supporting
        /// this range should be used in demo case Esn settings.
        /// </summary>
        /// <param name="demoCaseParams">
        /// Demo case settings
        /// </param>
        /// <param name="predictionInputVector">
        /// Prepared input vector to be used for Esn lasting prediction (after training)
        /// </param>
        /// <param name="outputNormalizerCollection">
        /// Prepared normalizer(s) for Esn outputs denormalization
        /// </param>
        /// <returns>
        /// Prepared SamplesDataBundle object
        /// </returns>
        public static SamplesDataBundle SamplesDataBundleFromFile(EsnDemoSettings.EsnDemoCaseSettings demoCaseParams,
                                                                  out double[] predictionInputVector,
                                                                  out List<Normalizer> outputNormalizerCollection
                                                                  )
        {
            //Allocations
            //Returned data bundle
            SamplesDataBundle data = new SamplesDataBundle();
            //Allways normalize between -1 and 1
            Interval normalizationInterval = new Interval(-1, 1);
            //Mapped Esn input fields to csv columns
            List<int> inputFieldIdxCollection = new List<int>();
            //Normalizers for input fields
            List<Normalizer> inputNormalizerCollection = new List<Normalizer>(demoCaseParams.EsnConfiguration.InputFieldNameCollection.Count);
            //Used if single normalizer is required in the demo case settings
            Normalizer singleNormalizer = new Normalizer(normalizationInterval, demoCaseParams.NormalizerReserveRatio);
            //Mapped Esn output fields to Esn input fields
            List<int> outputFieldIdxCollection = new List<int>();
            //Output fields normalizers
            outputNormalizerCollection = new List<Normalizer>(demoCaseParams.EsnConfiguration.OutputFieldNameCollection.Count);
            //Collection of all available input vectors loaded from csv file
            List<double[]> inputVectorCollection = new List<double[]>();
            //Commonly used exception message
            string exCommonText = $"Can't parse data from file {demoCaseParams.CsvDataFileName}";

            //Open file and load the data
            using (StreamReader streamReader = new StreamReader(new FileStream(demoCaseParams.CsvDataFileName, FileMode.Open)))
            {
                //First row contains column names (data fields)
                string delimitedColumnNames = streamReader.ReadLine();
                //What data delimiter is used?
                char csvDelimiter = DelimitedStringValues.RecognizeDelimiter(delimitedColumnNames);
                //Split column names
                DelimitedStringValues columnNames = new DelimitedStringValues(csvDelimiter);
                columnNames.LoadFromString(delimitedColumnNames);
                //Check if the recognized data delimiter works properly
                if (columnNames.ValuesCount < demoCaseParams.EsnConfiguration.InputFieldNameCollection.Count)
                {
                    throw new Exception(exCommonText + " Unknown delimiter.");
                }
                //Input data fields and normalizers
                foreach (string fieldName in demoCaseParams.EsnConfiguration.InputFieldNameCollection)
                {
                    int fieldIdx = columnNames.IndexOf(fieldName);
                    inputFieldIdxCollection.Add(fieldIdx);
                    if (demoCaseParams.SingleNormalizer)
                    {
                        inputNormalizerCollection.Add(singleNormalizer);
                    }
                    else
                    {
                        inputNormalizerCollection.Add(new Normalizer(normalizationInterval, demoCaseParams.NormalizerReserveRatio));
                    }
                }
                //Output data fields and normalizers
                foreach (string fieldName in demoCaseParams.EsnConfiguration.OutputFieldNameCollection)
                {
                    int fieldIdx = demoCaseParams.EsnConfiguration.InputFieldNameCollection.IndexOf(fieldName);
                    if (fieldIdx == -1)
                    {
                        throw new Exception(exCommonText);
                    }
                    outputFieldIdxCollection.Add(fieldIdx);
                    outputNormalizerCollection.Add(inputNormalizerCollection[fieldIdx]);
                }
                //Collect input vectors and adjust normalizers
                DelimitedStringValues dataRow = new DelimitedStringValues(csvDelimiter);
                while (!streamReader.EndOfStream)
                {
                    dataRow.LoadFromString(streamReader.ReadLine());
                    double[] inputVector = new double[inputFieldIdxCollection.Count];
                    for (int i = 0; i < inputFieldIdxCollection.Count; i++)
                    {
                        double dataValue = dataRow.GetValue(inputFieldIdxCollection[i]).ParseDouble(true, exCommonText);
                        inputVector[i] = dataValue;
                        if (demoCaseParams.SingleNormalizer)
                        {
                            singleNormalizer.Adjust(dataValue);
                        }
                        else
                        {
                            inputNormalizerCollection[i].Adjust(dataValue);
                        }
                    }
                    inputVectorCollection.Add(inputVector);
                }
            }//Loading the file
            
            //SamplesDataBundle creation
            //Counts and positions
            int demoVectorsCount = demoCaseParams.NumOfBootSamples + demoCaseParams.MaxNumOfTrainingSamples + demoCaseParams.NumOfTestSamples;
            if(demoVectorsCount > inputVectorCollection.Count)
            {
                demoVectorsCount = inputVectorCollection.Count;
            }
            int firstVectorIdx = (inputVectorCollection.Count - demoVectorsCount) - 1;
            if (firstVectorIdx < 0)
            {
                firstVectorIdx = 0;
            }
            //Data normalization and division to input/output vectors
            predictionInputVector = null;
            for (int vectorIdx = firstVectorIdx; vectorIdx < inputVectorCollection.Count; vectorIdx++)
            {
                //Normalized input vector
                double[] inputVector = new double[inputFieldIdxCollection.Count];
                for(int i = 0; i < inputFieldIdxCollection.Count; i++)
                {
                    inputVector[i] = inputNormalizerCollection[i].Normalize(inputVectorCollection[vectorIdx][i]);
                }
                if(vectorIdx < inputVectorCollection.Count - 1)
                {
                    double[] outputVector = new double[outputFieldIdxCollection.Count];
                    for (int i = 0; i < outputFieldIdxCollection.Count; i++)
                    {
                        outputVector[i] = outputNormalizerCollection[i].Normalize(inputVectorCollection[vectorIdx + 1][outputFieldIdxCollection[i]]);
                    }
                    data.Inputs.Add(inputVector);
                    data.Outputs.Add(outputVector);
                }
                else
                {
                    predictionInputVector = inputVector;
                }
            }
            return data;
        }

        /// <summary>
        /// This is the control function of the regression process and is called
        /// after the completion of each regression training epoch.
        /// The goal of the regression process is for each Esn output field to train a feed forward network
        /// that will give good results both on the training data and the test data.
        /// Esn.EsnRegressionControlInArgs object passed to the function contains the best error statistics so far
        /// and the latest statistics. The primary purpose of the function is to decide whether the latest statistics
        /// are better than the best statistics so far.
        /// Here is used simply outArgs.Best = (inArgs.CurrRegrData.CombinedError LT inArgs.BestRegrData.CombinedError), but
        /// the real logic could be much more complex.
        /// The function can also tell the regression process that it does not make any sense to continue the regression.
        /// It can terminate the current regression attempt or whole output field regression process.
        /// The reservoir statistics are also available in the Esn.EsnRegressionControlInArgs object, which should be
        /// monitored to ensure that the neurons of the reservoirs have not been oversaturated.
        /// </summary>
        /// <param name="inArgs">Contains all the necessary information to control the progress of the regression.</param>
        /// <returns>Instructions for the regression process.</returns>
        public static Esn.EsnRegressionControlOutArgs ESNRegressionControl(Esn.EsnRegressionControlInArgs inArgs)
        {
            //Report reservoirs statistics in case of the first call
            if (inArgs.OutputFieldIdx == 0 && inArgs.RegrAttemptNumber == 1 && inArgs.Epoch == 1)
            {
                for (int resIdx = 0; resIdx < inArgs.ReservoirsStatistics.Count; resIdx++)
                {
                    ((IOutputLog)inArgs.ControllerData).Write($"    Neurons states statistics of reservoir instance {inArgs.ReservoirsStatistics[resIdx].ReservoirInstanceName} ", false);
                    ((IOutputLog)inArgs.ControllerData).Write("          ABS-MAX Avg, Max, Min, SDdev: " + inArgs.ReservoirsStatistics[resIdx].NeuronsMaxAbsStatesStat.ArithAvg.ToString(CultureInfo.InvariantCulture) + " " + inArgs.ReservoirsStatistics[resIdx].NeuronsMaxAbsStatesStat.Max.ToString(CultureInfo.InvariantCulture) + " " + inArgs.ReservoirsStatistics[resIdx].NeuronsMaxAbsStatesStat.Min.ToString(CultureInfo.InvariantCulture) + " " + inArgs.ReservoirsStatistics[resIdx].NeuronsMaxAbsStatesStat.StdDev.ToString(CultureInfo.InvariantCulture), false);
                    ((IOutputLog)inArgs.ControllerData).Write("              RMS Avg, Max, Min, SDdev: " + inArgs.ReservoirsStatistics[resIdx].NeuronsRMSStatesStat.ArithAvg.ToString(CultureInfo.InvariantCulture) + " " + inArgs.ReservoirsStatistics[resIdx].NeuronsRMSStatesStat.Max.ToString(CultureInfo.InvariantCulture) + " " + inArgs.ReservoirsStatistics[resIdx].NeuronsRMSStatesStat.Min.ToString(CultureInfo.InvariantCulture) + " " + inArgs.ReservoirsStatistics[resIdx].NeuronsRMSStatesStat.StdDev.ToString(CultureInfo.InvariantCulture), false);
                    ((IOutputLog)inArgs.ControllerData).Write("             SPAN Avg, Max, Min, SDdev: " + inArgs.ReservoirsStatistics[resIdx].NeuronsStateSpansStat.ArithAvg.ToString(CultureInfo.InvariantCulture) + " " + inArgs.ReservoirsStatistics[resIdx].NeuronsStateSpansStat.Max.ToString(CultureInfo.InvariantCulture) + " " + inArgs.ReservoirsStatistics[resIdx].NeuronsStateSpansStat.Min.ToString(CultureInfo.InvariantCulture) + " " + inArgs.ReservoirsStatistics[resIdx].NeuronsStateSpansStat.StdDev.ToString(CultureInfo.InvariantCulture), false);
                    ((IOutputLog)inArgs.ControllerData).Write("             Context neuron states RMS: " + inArgs.ReservoirsStatistics[resIdx].CtxNeuronStatesRMS.ToString(CultureInfo.InvariantCulture), false);
                }
                ((IOutputLog)inArgs.ControllerData).Write("    Regression:", false);
            }
            //Instantiate output object.
            Esn.EsnRegressionControlOutArgs outArgs = new Esn.EsnRegressionControlOutArgs();
            //Evaluate statistics and decide if the latest statistics are the best.
            outArgs.Best = (inArgs.RegrCurrResult.CombinedError < inArgs.RegrBestResult.CombinedError);
            //Report the progress
            int reportInterval = Math.Max(inArgs.MaxEpoch / 100, 1);
            if (outArgs.Best || (inArgs.Epoch % reportInterval) == 0 || inArgs.Epoch == inArgs.MaxEpoch || (inArgs.Epoch == 1 && inArgs.RegrAttemptNumber == 1))
            {
                ((IOutputLog)inArgs.ControllerData).Write(
                    "      OutputField: " + inArgs.OutputFieldName +
                    ", Attempt/Epoch: " + inArgs.RegrAttemptNumber.ToString().PadLeft(inArgs.RegrMaxAttempt.ToString().Length, '0') + "/" + inArgs.Epoch.ToString().PadLeft(inArgs.MaxEpoch.ToString().Length, '0') +
                    ", DSet-Sizes: (" + inArgs.RegrCurrResult.TrainingErrorStat.SamplesCount.ToString() + ", " + inArgs.RegrCurrResult.TestingErrorStat.SamplesCount.ToString() + ")" +
                    ", Best-Train: " + (outArgs.Best ? inArgs.RegrCurrResult.TrainingErrorStat : inArgs.RegrBestResult.TrainingErrorStat).ArithAvg.ToString("E3", CultureInfo.InvariantCulture) +
                    ", Best-Test: " + (outArgs.Best ? inArgs.RegrCurrResult.TestingErrorStat : inArgs.RegrBestResult.TestingErrorStat).ArithAvg.ToString("E3", CultureInfo.InvariantCulture) +
                    ", Curr-Train: " + inArgs.RegrCurrResult.TrainingErrorStat.ArithAvg.ToString("E3", CultureInfo.InvariantCulture) +
                    ", Curr-Test: " + inArgs.RegrCurrResult.TestingErrorStat.ArithAvg.ToString("E3", CultureInfo.InvariantCulture)
                    , !(inArgs.Epoch == 1 && inArgs.RegrAttemptNumber == 1));
            }
            return outArgs;
        }

        /// <summary>
        /// Executes specified demo case.
        /// Loads and prepares sample data, trains Esn and displayes results plus prediction.
        /// </summary>
        /// <param name="log">Here demo writes its output messages</param>
        /// <param name="demoCaseParams">Demo case settings to be executed</param>
        /// <returns></returns>
        public static double[] PerformDemoCase(IOutputLog log, EsnDemoSettings.EsnDemoCaseSettings demoCaseParams)
        {
            log.Write("  Performing demo case " + demoCaseParams.Name, false);
            //Load of data bundle for Esv training
            double[] predictionInputVector;
            List<Normalizer> outputNormalizers;
            SamplesDataBundle data = SamplesDataBundleFromFile(demoCaseParams,
                                                               out predictionInputVector,
                                                               out outputNormalizers
                                                               );
            //Instantiate Esn
            Esn esn = new Esn(demoCaseParams.EsnConfiguration);
            //Select appropriate method for the test samples selection
            Esn.EsnTestSamplesSelectorCallbackDelegate samplesSelector = esn.SelectRandomTestSamples;
            if (demoCaseParams.TestSamplesSelectionMethod == "Sequential") samplesSelector = esn.SelectSequentialTestSamples;
            //Esn training (regression)
            Esn.EsnRegressionResult[] regrOuts = esn.Train(data,
                                                         demoCaseParams.NumOfBootSamples,
                                                         demoCaseParams.NumOfTestSamples,
                                                         samplesSelector,
                                                         ESNRegressionControl,
                                                         log
                                                         );
            //Next values prediction
            //Note that there is not necessary to call PushFeedback function immediately after training.
            //Feedback was already pushed during the Esn training.
            double[] outputVector = esn.Compute(predictionInputVector);
            //Values are normalized so they have to be denormalize
            for(int i = 0; i < outputVector.Length; i++)
            {
                outputVector[i] = outputNormalizers[i].Naturalize(outputVector[i]);
            }
            //Report training (regression) results and prediction
            log.Write("    Results", false);
            for (int outputIdx = 0; outputIdx < regrOuts.Length; outputIdx++)
            {
                log.Write("            OutputField: " + regrOuts[outputIdx].OutputFieldName, false);
                log.Write("         Predicted next: " + outputVector[outputIdx].ToString(CultureInfo.InvariantCulture), false);
                log.Write("      Trained weights stat", false);
                log.Write("          Min, Max, Avg: " + regrOuts[outputIdx].OutputWeightsStat.Min.ToString(CultureInfo.InvariantCulture) + " " + regrOuts[outputIdx].OutputWeightsStat.Max.ToString(CultureInfo.InvariantCulture) + " " + regrOuts[outputIdx].OutputWeightsStat.ArithAvg.ToString(CultureInfo.InvariantCulture), false);
                log.Write("          Upd, Cnt, Zrs: " + regrOuts[outputIdx].UpdateCounter.ToString() + " " + regrOuts[outputIdx].OutputWeightsStat.SamplesCount.ToString() + " " + (regrOuts[outputIdx].OutputWeightsStat.SamplesCount - regrOuts[outputIdx].OutputWeightsStat.NonzeroSamplesCount).ToString(), false);
                log.Write("              Error stat", false);
                log.Write("      Train set samples: " + regrOuts[outputIdx].TrainingErrorStat.SamplesCount.ToString(), false);
                log.Write("      Train set Avg Err: " + regrOuts[outputIdx].TrainingErrorStat.ArithAvg.ToString(CultureInfo.InvariantCulture), false);
                log.Write("       Test set samples: " + regrOuts[outputIdx].TestingErrorStat.SamplesCount.ToString(), false);
                log.Write("       Test set Avg Err: " + regrOuts[outputIdx].TestingErrorStat.ArithAvg.ToString(CultureInfo.InvariantCulture), false);
                log.Write("      Test Max Real Err: " + (outputNormalizers[outputIdx].ComputeNaturalError(regrOuts[outputIdx].TestingErrorStat.Max)).ToString(CultureInfo.InvariantCulture), false);
                log.Write("      Test Avg Real Err: " + (outputNormalizers[outputIdx].ComputeNaturalError(regrOuts[outputIdx].TestingErrorStat.ArithAvg)).ToString(CultureInfo.InvariantCulture), false);
            }
            log.Write(" ", false);
            log.Write(" ", false);
            return outputVector;
        }

        /// <summary>
        /// Runs ESN demo. This is the main function.
        /// For each demo case defined in demoSettingsXmlFile function calls PerformDemoCase.
        /// </summary>
        /// <param name="log">Into this interface demo writes output to be displayed</param>
        /// <param name="demoSettingsXmlFile">Xml file containing definitions of demo cases to be prformed</param>
        public static void RunDemo(IOutputLog log, string demoSettingsXmlFile)
        {
            log.Write("ESN demo started", false);
            //Instantiate demo settings from the xml file
            EsnDemoSettings demoSettings = new EsnDemoSettings(demoSettingsXmlFile);
            //Loop through the demo cases
            foreach(EsnDemoSettings.EsnDemoCaseSettings demoCaseParams in demoSettings.DemoCaseParamsCollection)
            {
                //Execute the demo case
                double[] predictions = PerformDemoCase(log, demoCaseParams);
            }
            log.Write("ESN demo finished", false);
            return;
        }

    }//ESNDemo
}//Namespace
