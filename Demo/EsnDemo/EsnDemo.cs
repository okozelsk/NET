using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
using System.Xml;
using OKOSW.MathTools;
using OKOSW.Extensions;
using OKOSW.CsvTools;
using OKOSW.Neural.Networks.EchoState;
using OKOSW.Demo;
using OKOSW.Demo.Log;


namespace OKOSW.Demo
{
    /// <summary>
    /// Demonstrates ESN usage
    /// </summary>
    public static class EsnDemo
    {
        /// <summary>
        /// Loads data, prepares output normalizer(s) and creates ESN required DataBundle object containing
        /// standardized and normalized data to be used for network training and testing.
        /// </summary>
        /// <param name="demoCaseParams">Demo case settings</param>
        /// <param name="predictionInputVector">OUT prepared input vector to be used for ESN further prediction (after training)</param>
        /// <param name="outputNormalizers">OUT prepared normalizer(s) for ESN outputs denormalization</param>
        /// <returns></returns>
        public static Esn.DataBundle ESNDataBundleFromFile(EsnDemoSettings.DemoCaseParams demoCaseParams, out double[] predictionInputVector, out List<Normalizer> outputNormalizers)
        {
            Interval normalizationInterval = new Interval(-1, 1);
            Esn.DataBundle data = new Esn.DataBundle(2000);
            StreamReader stream = new StreamReader(new FileStream(demoCaseParams.CSVDataFileName, FileMode.Open));
            string exCommonText = "Can't parse data from file " + demoCaseParams.CSVDataFileName;
            //All data fields names
            string allColumnsLine = stream.ReadLine();
            char csv_delimiter = DelimitedStringValues.RecognizeDelimiter(allColumnsLine);
            DelimitedStringValues allColumnsDsv = new DelimitedStringValues(csv_delimiter);
            allColumnsDsv.LoadFromString(allColumnsLine);
            if (allColumnsDsv.ValuesCount < demoCaseParams.ESNCfg.InputFieldsNames.Count)
            {
                throw new Exception(exCommonText + " Unknown delimiter.");
            }
            //Input data fields indexes and normalizers allocation
            List<int> inputFieldsIdxs = new List<int>();
            List<Normalizer> inputNormalizers = new List<Normalizer>(demoCaseParams.ESNCfg.InputFieldsNames.Count);
            Normalizer singleNormalizer = new Normalizer(normalizationInterval, demoCaseParams.NormalizerReserveRatio);
            foreach (string fieldName in demoCaseParams.ESNCfg.InputFieldsNames)
            {
                int fieldIdx = allColumnsDsv.IndexOf(fieldName);
                inputFieldsIdxs.Add(fieldIdx);
                if (demoCaseParams.SingleNormalizer)
                {
                    inputNormalizers.Add(singleNormalizer);
                }
                else
                {
                    inputNormalizers.Add(new Normalizer(normalizationInterval, demoCaseParams.NormalizerReserveRatio));
                }
            }
            //Output data fields indexes within input fields and normalizers allocation
            List<int> outputFieldsIdxs = new List<int>();
            outputNormalizers = new List<Normalizer>(demoCaseParams.OutputFieldsNames.Count);
            foreach (string fieldName in demoCaseParams.OutputFieldsNames)
            {
                int fieldIdx = demoCaseParams.ESNCfg.InputFieldsNames.IndexOf(fieldName);
                if(fieldIdx == -1)
                {
                    throw new Exception(exCommonText);
                }
                outputFieldsIdxs.Add(fieldIdx);
                outputNormalizers.Add(inputNormalizers[fieldIdx]);
            }
            //All input vectors with natural data and normalizers adjustment
            List<double[]> allInputVectors = new List<double[]>();
            DelimitedStringValues dataRowDsv = new DelimitedStringValues(csv_delimiter);
            while (!stream.EndOfStream)
            {
                dataRowDsv.LoadFromString(stream.ReadLine());
                double[] inputVector = new double[inputFieldsIdxs.Count];
                for(int i = 0; i < inputFieldsIdxs.Count; i++)
                {
                    double dataValue = dataRowDsv.GetValue(inputFieldsIdxs[i]).ParseDouble(true, exCommonText);
                    inputVector[i] = dataValue;
                    if (demoCaseParams.SingleNormalizer)
                    {
                        singleNormalizer.Adjust(dataValue);
                    }
                    else
                    {
                        inputNormalizers[i].Adjust(dataValue);
                    }
                }
                allInputVectors.Add(inputVector);
            }
            stream.Close();
            //DataBundle creation
            int demoVectorsCount = demoCaseParams.BootSeqMinLength + demoCaseParams.TrainingSeqMaxLength + demoCaseParams.TestingSeqLength;
            if(demoVectorsCount > allInputVectors.Count)
            {
                demoVectorsCount = allInputVectors.Count;
            }
            int firstVectorIdx = (allInputVectors.Count - demoVectorsCount) - 1;
            if (firstVectorIdx < 0)
            {
                firstVectorIdx = 0;
            }
            predictionInputVector = null;
            for (int vectorIdx = firstVectorIdx; vectorIdx < allInputVectors.Count; vectorIdx++)
            {
                //Normalized input vector
                double[] inputVector = new double[inputFieldsIdxs.Count];
                for(int i = 0; i < inputFieldsIdxs.Count; i++)
                {
                    inputVector[i] = inputNormalizers[i].Normalize(allInputVectors[vectorIdx][i]);
                }
                if(vectorIdx < allInputVectors.Count - 1)
                {
                    double[] outputVector = new double[outputFieldsIdxs.Count];
                    for (int i = 0; i < outputFieldsIdxs.Count; i++)
                    {
                        outputVector[i] = outputNormalizers[i].Normalize(allInputVectors[vectorIdx + 1][outputFieldsIdxs[i]]);
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
        /// This is a callback control function called from the regression process after each epoch.
        /// Primary purpose is to identify the best network (weights).
        /// Here is used simply outArgs.Best = (inArgs.CurrRegrData.CombinedError LT inArgs.BestRegrData.CombinedError), but
        /// the real logic could be much more complex.
        /// Secondary purpose is to inform the caller about the progress and to give a chance to control the progress
        /// (to stop current attempt or to stop whole regression).
        /// </summary>
        /// <param name="inArgs">Contains current/best error statistics, reservoir(s) statistics and the user object</param>
        /// <returns>Instructions for the calling regression process.</returns>
        public static RegressionControlOutArgs ESNRegressionControl(RegressionControlInArgs inArgs)
        {
            //Report reservoirs statistics in case of the first call
            if (inArgs.RegrValID == 0 && inArgs.RegrAttemptNumber == 1 && inArgs.Epoch == 1)
            {
                for (int resIdx = 0; resIdx < inArgs.ReservoirsStatistics.Count; resIdx++)
                {
                    ((IOutputLog)inArgs.ControllerData).Write("    Reservoir neurons statistics for ResID " + inArgs.ReservoirsStatistics[resIdx].ResID, false);
                    ((IOutputLog)inArgs.ControllerData).Write("          ABS-MAX Avg, Max, Min, SDdev: " + inArgs.ReservoirsStatistics[resIdx].NeuronsMaxAbsStatesStat.ArithAvg.ToString(CultureInfo.InvariantCulture) + " " + inArgs.ReservoirsStatistics[resIdx].NeuronsMaxAbsStatesStat.Max.ToString(CultureInfo.InvariantCulture) + " " + inArgs.ReservoirsStatistics[resIdx].NeuronsMaxAbsStatesStat.Min.ToString(CultureInfo.InvariantCulture) + " " + inArgs.ReservoirsStatistics[resIdx].NeuronsMaxAbsStatesStat.StdDev.ToString(CultureInfo.InvariantCulture), false);
                    ((IOutputLog)inArgs.ControllerData).Write("              RMS Avg, Max, Min, SDdev: " + inArgs.ReservoirsStatistics[resIdx].NeuronsRMSStatesStat.ArithAvg.ToString(CultureInfo.InvariantCulture) + " " + inArgs.ReservoirsStatistics[resIdx].NeuronsRMSStatesStat.Max.ToString(CultureInfo.InvariantCulture) + " " + inArgs.ReservoirsStatistics[resIdx].NeuronsRMSStatesStat.Min.ToString(CultureInfo.InvariantCulture) + " " + inArgs.ReservoirsStatistics[resIdx].NeuronsRMSStatesStat.StdDev.ToString(CultureInfo.InvariantCulture), false);
                    ((IOutputLog)inArgs.ControllerData).Write("             SPAN Avg, Max, Min, SDdev: " + inArgs.ReservoirsStatistics[resIdx].NeuronsStateSpansStat.ArithAvg.ToString(CultureInfo.InvariantCulture) + " " + inArgs.ReservoirsStatistics[resIdx].NeuronsStateSpansStat.Max.ToString(CultureInfo.InvariantCulture) + " " + inArgs.ReservoirsStatistics[resIdx].NeuronsStateSpansStat.Min.ToString(CultureInfo.InvariantCulture) + " " + inArgs.ReservoirsStatistics[resIdx].NeuronsStateSpansStat.StdDev.ToString(CultureInfo.InvariantCulture), false);
                    ((IOutputLog)inArgs.ControllerData).Write("             Context neuron states RMS: " + inArgs.ReservoirsStatistics[resIdx].CtxNeuronStatesRMS.ToString(CultureInfo.InvariantCulture), false);
                    ((IOutputLog)inArgs.ControllerData).Write(" ", false);
                }
            }
            RegressionControlOutArgs outArgs = new RegressionControlOutArgs();
            outArgs.Best = (inArgs.CurrRegrData.CombinedError < inArgs.BestRegrData.CombinedError);
            //Progress prompt
            ((IOutputLog)inArgs.ControllerData).Write(
                "    OutFieldNbr: " + inArgs.RegrValID.ToString() +
                ", Attempt/Epoch: " + inArgs.RegrAttemptNumber.ToString().PadLeft(2, '0') + "/" + inArgs.Epoch.ToString().PadLeft(5, '0') +
                ", DSet-Sizes: (" + inArgs.CurrRegrData.TrainingErrorStat.SamplesCount.ToString() + ", " + inArgs.CurrRegrData.TestingErrorStat.SamplesCount.ToString() + ")" +
                ", Best-Train: " + (outArgs.Best ? inArgs.CurrRegrData.TrainingErrorStat : inArgs.BestRegrData.TrainingErrorStat).ArithAvg.ToString("E3", CultureInfo.InvariantCulture) +
                ", Best-Test: " + (outArgs.Best ? inArgs.CurrRegrData.TestingErrorStat : inArgs.BestRegrData.TestingErrorStat).ArithAvg.ToString("E3", CultureInfo.InvariantCulture) +
                ", Curr-Train: " + inArgs.CurrRegrData.TrainingErrorStat.ArithAvg.ToString("E3", CultureInfo.InvariantCulture) +
                ", Curr-Test: " + inArgs.CurrRegrData.TestingErrorStat.ArithAvg.ToString("E3", CultureInfo.InvariantCulture)
                , !(inArgs.Epoch == 1 && inArgs.RegrAttemptNumber == 1));
            return outArgs;
        }

        /// <summary>
        /// Executes one specified demo case.
        /// Loads and prepares sample data, trains network and display results and prediction.
        /// </summary>
        /// <param name="log">Into this interface demo writes output to be displayed</param>
        /// <param name="demoCaseParams">Demo case settings to be executed</param>
        /// <returns></returns>
        public static double[] PerformDemoCase(IOutputLog log, EsnDemoSettings.DemoCaseParams demoCaseParams)
        {
            log.Write("  Performing demo case " + demoCaseParams.Name, false);
            //Load of data bundle for ESN training
            double[] predictionInputVector;
            List<Normalizer> outputNormalizers;
            Esn.DataBundle data = ESNDataBundleFromFile(demoCaseParams, out predictionInputVector, out outputNormalizers);
            //ESN training
            Esn esn = new Esn(demoCaseParams.ESNCfg, demoCaseParams.OutputFieldsNames.Count);
            Esn.EsnTestSamplesSelectorCallbackDelegate samplesSelector = esn.SelectRandomTestSamples;
            if (demoCaseParams.TestSamplesSelection == "SEQUENCE") samplesSelector = esn.SelectSequenceTestSamples;
            RegressionData[] regrOuts = esn.Train(data,
                                                  demoCaseParams.BootSeqMinLength,
                                                  demoCaseParams.TestingSeqLength,
                                                  samplesSelector,
                                                  ESNRegressionControl,
                                                  log
                                                  );
            //Next future values prediction
            double[] outputVector = esn.PredictNext(predictionInputVector);
            for(int i = 0; i < outputVector.Length; i++)
            {
                outputVector[i] = outputNormalizers[i].Naturalize(outputVector[i]);
            }
            //Report regression information
            for (int outputIdx = 0; outputIdx < regrOuts.Length; outputIdx++)
            {
                log.Write("    " + demoCaseParams.OutputFieldsNames[outputIdx], false);
                log.Write("       Predicted next: " + outputVector[outputIdx].ToString(CultureInfo.InvariantCulture), false);
                log.Write("     Regr weights stat", false);
                log.Write("        Min, Max, Avg: " + regrOuts[outputIdx].OutputWeightsStat.Min.ToString(CultureInfo.InvariantCulture) + " " + regrOuts[outputIdx].OutputWeightsStat.Max.ToString(CultureInfo.InvariantCulture) + " " + regrOuts[outputIdx].OutputWeightsStat.ArithAvg.ToString(CultureInfo.InvariantCulture), false);
                log.Write("        Upd, Cnt, Zrs: " + regrOuts[outputIdx].BestUpdatesCount.ToString() + " " + regrOuts[outputIdx].OutputWeightsStat.SamplesCount.ToString() + " " + (regrOuts[outputIdx].OutputWeightsStat.SamplesCount - regrOuts[outputIdx].OutputWeightsStat.NonzeroSamplesCount).ToString(), false);
                log.Write("            Error stat", false);
                log.Write("    Train set samples: " + regrOuts[outputIdx].TrainingErrorStat.SamplesCount.ToString(), false);
                log.Write("    Train set Avg Err: " + regrOuts[outputIdx].TrainingErrorStat.ArithAvg.ToString(CultureInfo.InvariantCulture), false);
                log.Write("     Test set samples: " + regrOuts[outputIdx].TestingErrorStat.SamplesCount.ToString(), false);
                log.Write("     Test set Avg Err: " + regrOuts[outputIdx].TestingErrorStat.ArithAvg.ToString(CultureInfo.InvariantCulture), false);
                log.Write("    Test Max Real Err: " + (outputNormalizers[outputIdx].ComputeNaturalError(regrOuts[outputIdx].TestingErrorStat.Max)).ToString(CultureInfo.InvariantCulture), false);
                log.Write("    Test Avg Real Err: " + (outputNormalizers[outputIdx].ComputeNaturalError(regrOuts[outputIdx].TestingErrorStat.ArithAvg)).ToString(CultureInfo.InvariantCulture), false);
                log.Write("    (Note that displayed errors are not MSE. MSE errors would be much smaller)", false);
            }
            log.Write(" ", false);
            return outputVector;
        }

        /// <summary>
        /// Runs ESN demo. Calls PerformDemoCase for each demo case defined in demoSettingsFile XML.
        /// </summary>
        /// <param name="log">Into this interface demo writes output to be displayed</param>
        /// <param name="demoSettingsFile">XML file name which contains defined ESN demo cases</param>
        public static void RunDemo(IOutputLog log, string demoSettingsFile)
        {
            log.Write("ESN demo started", false);
            EsnDemoSettings demoSettings = new EsnDemoSettings(demoSettingsFile);
            //TimeSeriesGenerator.GenerateStandardCSVFiles(demoSettings.DataDir);
            foreach(EsnDemoSettings.DemoCaseParams demoCaseParams in demoSettings.DemoCases)
            {
                double[] predictions = PerformDemoCase(log, demoCaseParams);
            }
            log.Write("ESN demo finished", false);
            return;
        }

    }//ESNDemo
}//Namespace
