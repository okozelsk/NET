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
using OKOSW.CSVTools;
using OKOSW.Neural.Networks.EchoState;
using OKOSW.Demo;
using OKOSW.Demo.Log;


namespace OKOSW.Demo
{

    /// <summary>
    /// Demonstrates ESN usage
    /// </summary>
    public static class ESNDemo
    {
        /// <summary>
        /// Creates ESN required DataBundle object containing normalized data for training
        /// </summary>
        /// <param name="demoCaseParams">Particular demo case parameters</param>
        /// <param name="predictionInputVector">OUT input vector to be used for ESN further prediction</param>
        /// <param name="outputNormalizer">OUT normalizer object for ESN outputs denormalization</param>
        /// <returns></returns>
        public static ESN.DataBundle ESNDataBundleFromFile(ESNDemoSettings.DemoCaseParams demoCaseParams, out double[] predictionInputVector, out List<Normalizer> outputNormalizers)
        {
            Interval normalizationInterval = new Interval(-1, 1);
            ESN.DataBundle data = new ESN.DataBundle(2000);
            StreamReader stream = new StreamReader(new FileStream(demoCaseParams.CSVDataFileName, FileMode.Open));
            string exCommonText = "Can't parse data from file " + demoCaseParams.CSVDataFileName;
            //All data fields names
            string allColumnsLine = stream.ReadLine();
            char csv_delimiter = DelimitedStringValues.DetermineDelimiter(allColumnsLine);
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
                int fieldIdx = allColumnsDsv.FindValueIndex(fieldName);
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
        /// Regression callback control function
        /// </summary>
        /// <param name="inArgs"></param>
        /// <returns></returns>
        public static RegressionCallBackOutArgs ESNTraininigControl(RegressionCallBackInArgs inArgs)
        {
            //Report reservoirs statistics in case of the first call
            if (inArgs.RegrValID == 0 && inArgs.RegrAttemptNumber == 1 && inArgs.Epoch == 1)
            {
                for (int resIdx = 0; resIdx < inArgs.ReservoirsStatistics.Count; resIdx++)
                {
                    ((IOutputLog)inArgs.ControllerData).Write("    Stats of the reservoir neurons " + inArgs.ReservoirsStatistics[resIdx].ResID, false);
                    ((IOutputLog)inArgs.ControllerData).Write("      MAX States  Avg, Max, Min, SDdev: " + inArgs.ReservoirsStatistics[resIdx].NeuronsMaxAbsStatesStat.ArithAvg.ToString() + " " + inArgs.ReservoirsStatistics[resIdx].NeuronsMaxAbsStatesStat.Max.ToString() + " " + inArgs.ReservoirsStatistics[resIdx].NeuronsMaxAbsStatesStat.Min.ToString() + " " + inArgs.ReservoirsStatistics[resIdx].NeuronsMaxAbsStatesStat.StdDev.ToString(), false);
                    ((IOutputLog)inArgs.ControllerData).Write("      AVG States  Avg, Max, Min, SDdev: " + inArgs.ReservoirsStatistics[resIdx].NeuronsGeoAvgStatesStat.ArithAvg.ToString() + " " + inArgs.ReservoirsStatistics[resIdx].NeuronsGeoAvgStatesStat.Max.ToString() + " " + inArgs.ReservoirsStatistics[resIdx].NeuronsGeoAvgStatesStat.Min.ToString() + " " + inArgs.ReservoirsStatistics[resIdx].NeuronsGeoAvgStatesStat.StdDev.ToString(), false);
                    ((IOutputLog)inArgs.ControllerData).Write("      States SPAN Avg, Max, Min, SDdev: " + inArgs.ReservoirsStatistics[resIdx].NeuronsStateSpansStat.ArithAvg.ToString() + " " + inArgs.ReservoirsStatistics[resIdx].NeuronsStateSpansStat.Max.ToString() + " " + inArgs.ReservoirsStatistics[resIdx].NeuronsStateSpansStat.Min.ToString() + " " + inArgs.ReservoirsStatistics[resIdx].NeuronsStateSpansStat.StdDev.ToString(), false);
                    ((IOutputLog)inArgs.ControllerData).Write(" ", false);
                }
            }
            RegressionCallBackOutArgs outArgs = new RegressionCallBackOutArgs();
            outArgs.Best = (inArgs.CurrRegrData.CombinedError < inArgs.BestRegrData.CombinedError);
            //Progress prompt
            ((IOutputLog)inArgs.ControllerData).Write(
                "    OutFieldNbr: " + inArgs.RegrValID.ToString() +
                ", Attempt/Epoch: " + inArgs.RegrAttemptNumber.ToString().PadLeft(2, '0') + "/" + inArgs.Epoch.ToString().PadLeft(5, '0') +
                ", DSet-Sizes: (" + inArgs.CurrRegrData.TrainingErrorStat.SamplesCount.ToString() + ", " + inArgs.CurrRegrData.TestingErrorStat.SamplesCount.ToString() + ")" +
                ", B-TrainA: " + (outArgs.Best ? inArgs.CurrRegrData.TrainingErrorStat : inArgs.BestRegrData.TrainingErrorStat).ArithAvg.ToString("E3", CultureInfo.InvariantCulture) +
                ", B-TestA: " + (outArgs.Best ? inArgs.CurrRegrData.TestingErrorStat : inArgs.BestRegrData.TestingErrorStat).ArithAvg.ToString("E3", CultureInfo.InvariantCulture) +
                ", B-TestM: " + (outArgs.Best ? inArgs.CurrRegrData.TestingErrorStat : inArgs.BestRegrData.TestingErrorStat).Min.ToString("E3", CultureInfo.InvariantCulture) +
                ", C-TrainA: " + inArgs.CurrRegrData.TrainingErrorStat.ArithAvg.ToString("E3", CultureInfo.InvariantCulture) +
                ", C-TestA: " + inArgs.CurrRegrData.TestingErrorStat.ArithAvg.ToString("E3", CultureInfo.InvariantCulture) +
                ", C-TestM: " + inArgs.CurrRegrData.TestingErrorStat.Min.ToString("E3", CultureInfo.InvariantCulture)
                , !(inArgs.Epoch == 1 && inArgs.RegrAttemptNumber == 1));
            return outArgs;
        }

        public static double[] PerformDemoCase(IOutputLog log, ESNDemoSettings.DemoCaseParams demoCaseParams)
        {
            log.Write("  Performing demo case " + demoCaseParams.Name, false);
            //Load of data bundle for ESN training
            double[] predictionInputVector;
            List<Normalizer> outputNormalizers;
            ESN.DataBundle data = ESNDataBundleFromFile(demoCaseParams, out predictionInputVector, out outputNormalizers);
            //ESN training
            ESN esn = new ESN(demoCaseParams.ESNCfg, demoCaseParams.OutputFieldsNames.Count);
            ESN.TestSamplesSelector samplesSelector = esn.SelectTestSamples_Rnd;
            if (demoCaseParams.TestSamplesSelection == "SEQUENCE") samplesSelector = esn.SelectTestSamples_Seq;
            RegressionData[] regrOuts = esn.Train(data,
                                                  demoCaseParams.BootSeqMinLength,
                                                  demoCaseParams.TestingSeqLength,
                                                  samplesSelector,
                                                  ESNTraininigControl,
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
                log.Write("    Output Field: " + demoCaseParams.OutputFieldsNames[outputIdx], false);
                log.Write("       Predicion: " + outputVector[outputIdx].ToString(), false);
                log.Write("      Out weights", false);
                log.Write("        Min, Max, Avg: " + regrOuts[outputIdx].OutputWeightsStat.Min.ToString() + " " + regrOuts[outputIdx].OutputWeightsStat.Max.ToString() + " " + regrOuts[outputIdx].OutputWeightsStat.ArithAvg.ToString(), false);
                log.Write("        Upd, Cnt, Zrs: " + regrOuts[outputIdx].BestUpdatesCount.ToString() + " " + regrOuts[outputIdx].OutputWeightsStat.SamplesCount.ToString() + " " + (regrOuts[outputIdx].OutputWeightsStat.SamplesCount - regrOuts[outputIdx].OutputWeightsStat.NonzeroSamplesCount).ToString(), false);
                log.Write("      Errors", false);
                log.Write("        TrainE Avg: " + regrOuts[outputIdx].TrainingErrorStat.ArithAvg.ToString(), false);
                log.Write("        Test   Len: " + demoCaseParams.TestingSeqLength.ToString(), false);
                log.Write("        TestE  Avg: " + regrOuts[outputIdx].TestingErrorStat.ArithAvg.ToString(), false);
                log.Write("        Test Max NaturalE (+/-): " + (outputNormalizers[outputIdx].ComputeNaturalError(regrOuts[outputIdx].TestingErrorStat.Max)).ToString(), false);
                log.Write("        Test Avg NaturalE (+/-): " + (outputNormalizers[outputIdx].ComputeNaturalError(regrOuts[outputIdx].TestingErrorStat.ArithAvg)).ToString(), false);
            }
            log.Write(" ", false);
            return outputVector;
        }

        /// <summary>
        /// Runs ESN demos. Function prepares and runs different data sets and related ESN settings.
        /// </summary>
        /// <param name="log">Logging object to be used as demo messages output</param>
        /// <param name="demoSettingsFile">XML file name containing settings for ESN demo</param>
        public static void RunDemo(IOutputLog log, string demoSettingsFile)
        {
            log.Write("ESN demo started", false);
            ESNDemoSettings demoSettings = new ESNDemoSettings(demoSettingsFile);
            TimeSeriesGenerator.GenerateStandardCSVFiles(demoSettings.DataDir);
            foreach(ESNDemoSettings.DemoCaseParams demoCaseParams in demoSettings.DemoCases)
            {
                double[] predictions = PerformDemoCase(log, demoCaseParams);
            }
            log.Write("ESN demo finished", false);
            return;
        }

    }//ESNDemo
}//Namespace
