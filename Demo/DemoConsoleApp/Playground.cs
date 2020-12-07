using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Linq;
using RCNet.Extensions;
using RCNet.Neural.Activation;
using RCNet.Neural.Data.Transformers;
using RCNet.Neural.Data.Generators;
using RCNet.Neural.Data.Coders.AnalogToSpiking;
using RCNet.CsvTools;
using RCNet.MathTools;
using RCNet.Neural.Data.Filter;
using RCNet.Neural.Data;
using RCNet.Neural.Network.NonRecurrent;
using RCNet.Neural.Network.NonRecurrent.FF;

namespace Demo.DemoConsoleApp
{
    /// <summary>
    /// This class is a free "playground", the place where it is possible to test new concepts or somewhat else
    /// </summary>
    class Playground
    {
        //Attributes
        private readonly Random _rand;

        //Constructor
        public Playground()
        {
            _rand = new Random();
            return;
        }

        //Methods
        private void TestSpikingAF(AFSpikingBase af, int simLength, double constCurrent, int from, int count)
        {
            for (int i = 1; i <= simLength; i++)
            {
                double signal;
                double input;
                if (i >= from && i < from + count)
                {
                    input = double.IsNaN(constCurrent) ? _rand.NextDouble() : constCurrent;
                }
                else
                {
                    input = 0d;
                }
                signal = af.Compute(input);
                Console.WriteLine($"{af.GetType().Name} step {i}, State {(af.TypeOfActivation == ActivationType.Spiking ? af.InternalState : signal)} signal {signal}");
            }
            Console.ReadLine();

            return;
        }

        private void TestSingleFieldTransformer(ITransformer transformer)
        {
            double[] inputValues = new double[1];
            inputValues[0] = double.MinValue;
            Console.WriteLine($"{transformer.GetType().Name} Input {inputValues[0]} Output {transformer.Transform(inputValues)}");
            for (double input = -5d; input <= 5d; input += 0.1d)
            {
                input = Math.Round(input, 1);
                inputValues[0] = input;
                Console.WriteLine($"{transformer.GetType().Name} Input {input} Output {transformer.Transform(inputValues)}");
            }
            inputValues[0] = double.MaxValue;
            Console.WriteLine($"{transformer.GetType().Name} Input {inputValues[0]} Output {transformer.Transform(inputValues)}");
            Console.ReadLine();
            return;
        }

        private void TestTwoFieldsTransformer(ITransformer transformer)
        {
            double[] inputValues = new double[2];
            inputValues[0] = double.MinValue;
            inputValues[1] = double.MinValue;
            Console.WriteLine($"{transformer.GetType().Name} Inputs [{inputValues[0]}, {inputValues[1]}] Output {transformer.Transform(inputValues)}");

            for (double input1 = -5d; input1 <= 5d; input1 += 0.5d)
            {
                input1 = Math.Round(input1, 1);
                for (double input2 = -5d; input2 <= 5d; input2 += 0.5d)
                {
                    input2 = Math.Round(input2, 1);
                    inputValues[0] = input1;
                    inputValues[1] = input2;
                    Console.WriteLine($"{transformer.GetType().Name} Inputs [{inputValues[0]}, {inputValues[1]}] Output {transformer.Transform(inputValues)}");
                }
            }
            inputValues[0] = double.MaxValue;
            inputValues[1] = double.MaxValue;
            Console.WriteLine($"{transformer.GetType().Name} Inputs [{inputValues[0]}, {inputValues[1]}] Output {transformer.Transform(inputValues)}");
            Console.ReadLine();
            return;
        }

        private void TestTransformers()
        {
            List<string> singleFieldList = new List<string>() { "f1" };
            List<string> twoFieldsList = new List<string>() { "f1", "f2" };
            ITransformer transformer;
            //Difference transformer
            transformer = new DiffTransformer(singleFieldList, new DiffTransformerSettings(singleFieldList[0], 2));
            TestSingleFieldTransformer(transformer);
            //CDiv transformer
            transformer = new CDivTransformer(singleFieldList, new CDivTransformerSettings(singleFieldList[0], 1d));
            TestSingleFieldTransformer(transformer);
            //Log transformer
            transformer = new LogTransformer(singleFieldList, new LogTransformerSettings(singleFieldList[0], 10));
            TestSingleFieldTransformer(transformer);
            //Exp transformer
            transformer = new ExpTransformer(singleFieldList, new ExpTransformerSettings(singleFieldList[0]));
            TestSingleFieldTransformer(transformer);
            //Power transformer
            transformer = new PowerTransformer(singleFieldList, new PowerTransformerSettings(singleFieldList[0], 0.5d, true));
            TestSingleFieldTransformer(transformer);
            //YeoJohnson transformer
            transformer = new YeoJohnsonTransformer(singleFieldList, new YeoJohnsonTransformerSettings(singleFieldList[0], 0.5d));
            TestSingleFieldTransformer(transformer);
            //MWStat transformer
            transformer = new MWStatTransformer(singleFieldList, new MWStatTransformerSettings(singleFieldList[0], 5, BasicStat.OutputFeature.RootMeanSquare));
            TestSingleFieldTransformer(transformer);
            //Mul transformer
            transformer = new MulTransformer(twoFieldsList, new MulTransformerSettings(twoFieldsList[0], twoFieldsList[1]));
            TestTwoFieldsTransformer(transformer);
            //Div transformer
            transformer = new DivTransformer(twoFieldsList, new DivTransformerSettings(twoFieldsList[0], twoFieldsList[1]));
            TestTwoFieldsTransformer(transformer);
            //Linear transformer
            transformer = new LinearTransformer(twoFieldsList, new LinearTransformerSettings(twoFieldsList[0], twoFieldsList[1], 0.03, 0.2));
            TestTwoFieldsTransformer(transformer);
            return;
        }

        private void GenSteadyPatternedMGData(int minTau, int maxTau, int tauSamples, int patternLength, double verifyRatio, string path)
        {
            CsvDataHolder trainingData = new CsvDataHolder(DelimitedStringValues.DefaultDelimiter);
            CsvDataHolder verificationData = new CsvDataHolder(DelimitedStringValues.DefaultDelimiter);
            int verifyBorderIdx = (int)(tauSamples * verifyRatio);
            for (int tau = minTau; tau <= maxTau; tau++)
            {
                MackeyGlassGenerator mgg = new MackeyGlassGenerator(new MackeyGlassGeneratorSettings(tau));
                int neededDataLength = 1 + patternLength + (tauSamples - 1);
                double[] mggData = new double[neededDataLength];
                for(int i = 0; i < neededDataLength; i++)
                {
                    mggData[i] = mgg.Next();
                }
                for(int i = 0; i < tauSamples; i++)
                {
                    DelimitedStringValues patternData = new DelimitedStringValues();
                    //Steady data
                    patternData.AddValue(tau.ToString(CultureInfo.InvariantCulture));
                    //Varying data
                    for (int j = 0; j < patternLength; j++)
                    {
                        patternData.AddValue(mggData[i + j].ToString(CultureInfo.InvariantCulture));
                    }
                    //Desired data 1
                    patternData.AddValue(mggData[i + patternLength].ToString(CultureInfo.InvariantCulture));
                    //Desired data 2
                    patternData.AddValue(mggData[i + patternLength].ToString(CultureInfo.InvariantCulture));
                    //Add to a collections
                    if (i < verifyBorderIdx)
                    {
                        trainingData.DataRowCollection.Add(patternData);
                    }
                    else
                    {
                        verificationData.DataRowCollection.Add(patternData);
                    }
                }
            }
            //Save files
            trainingData.Save(Path.Combine(path, "SteadyMG_train.csv"));
            verificationData.Save(Path.Combine(path, "SteadyMG_verify.csv"));

            return;
        }

        private string ByteArrayToString(byte[] arr)
        {
            StringBuilder builder = new StringBuilder(arr.Length);
            for(int i = 0; i < arr.Length; i++)
            {
                builder.Append(arr[i].ToString());
            }
            return builder.ToString();
        }

        private string ByteArraysToString(byte[][] arr)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < arr.Length; i++)
            {
                builder.Append(" ");
                builder.Append(ByteArrayToString(arr[i]));
            }
            return builder.ToString();
        }

        private void TestA2SCoder()
        {
            double[] orderedAnalogValues = { -1, -0.9, -0.8, -0.7, -0.6, -0.5, -0.4, -0.3, -0.2, -0.1, -0.05, -0.025, -0.0125, 0, 0.0125, 0.025, 0.05, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1 };
            double[] disorderedAnalogValues = { -1, 1, -0.8, -0.7, 0.8, 0.8, -0.4, 0.3, 0.2, 1, 0, 0.1, 0.2, 0.3, -0.5, 0.6, 0.9 };
            A2SCoderBase coder = null;

            //Gaussian
            coder = new A2SCoderGaussianReceptors(new A2SCoderGaussianReceptorsSettings(8, 10));
            Console.WriteLine($"{coder.GetType().Name}");
            foreach (double value in orderedAnalogValues)
            {
                Console.WriteLine($"    {value.ToString(CultureInfo.InvariantCulture),-10} {ByteArraysToString(coder.GetCode(value))}");
            }
            Console.ReadLine();

            //Signal strength
            coder = new A2SCoderSignalStrength(new A2SCoderSignalStrengthSettings(8));
            Console.WriteLine($"{coder.GetType().Name}");
            foreach (double value in orderedAnalogValues)
            {
                Console.WriteLine($"    {value.ToString(CultureInfo.InvariantCulture),-10} {ByteArraysToString(coder.GetCode(value))}");
            }
            Console.ReadLine();

            //UpDirArrows
            coder = new A2SCoderUpDirArrows(new A2SCoderUpDirArrowsSettings(16, 8));
            Console.WriteLine($"{coder.GetType().Name}");
            foreach (double value in disorderedAnalogValues)
            {
                Console.WriteLine($"    {value.ToString(CultureInfo.InvariantCulture),-10} {ByteArraysToString(coder.GetCode(value))}");
            }
            Console.ReadLine();

            //DownDirArrows
            coder = new A2SCoderDownDirArrows(new A2SCoderDownDirArrowsSettings(16, 8));
            Console.WriteLine($"{coder.GetType().Name}");
            foreach (double value in disorderedAnalogValues)
            {
                Console.WriteLine($"    {value.ToString(CultureInfo.InvariantCulture),-10} {ByteArraysToString(coder.GetCode(value))}");
            }
            Console.ReadLine();

            return;
        }

        private void TestEnumFeatureFilter()
        {
            int enumerations = 10;
            EnumFeatureFilter filter = new EnumFeatureFilter(Interval.IntZP1, new EnumFeatureFilterSettings(enumerations));
            Random rand = new Random();
            for(int i = 0; i < 200; i++)
            {
                filter.Update((double)rand.Next(1, enumerations));
            }

            Console.WriteLine($"{filter.GetType().Name} ApplyFilter");
            for (int i = 1; i <= 10; i++)
            {
                Console.WriteLine($"    {i.ToString(CultureInfo.InvariantCulture),-20} {filter.ApplyFilter(i)}");
            }

            Console.WriteLine($"{filter.GetType().Name} ApplyReverse");
            int pieces = 100;
            for(int i = 0; i <= pieces; i++)
            {
                double value = (double)i * (1d / pieces);
                Console.WriteLine($"    {value.ToString(CultureInfo.InvariantCulture),-20} {filter.ApplyReverse(value)}");
            }
            Console.ReadLine();

        }

        private void TestBinFeatureFilter()
        {
            BinFeatureFilter filter = new BinFeatureFilter(Interval.IntZP1, new BinFeatureFilterSettings());
            Random rand = new Random();
            for (int i = 0; i < 200; i++)
            {
                filter.Update((double)rand.Next(0, 1));
            }

            Console.WriteLine($"{filter.GetType().Name} ApplyFilter");
            for (int i = 0; i <= 1; i++)
            {
                Console.WriteLine($"    {i.ToString(CultureInfo.InvariantCulture),-20} {filter.ApplyFilter(i)}");
            }

            Console.WriteLine($"{filter.GetType().Name} ApplyReverse");
            int pieces = 10;
            for (int i = 0; i <= pieces; i++)
            {
                double value = (double)i * (1d / pieces);
                Console.WriteLine($"    {value.ToString(CultureInfo.InvariantCulture),-20} {filter.ApplyReverse(value)}");
            }
            Console.ReadLine();
        }

        private void TestVectorBundleFolderization(string dataFile, int numOfClasses)
        {
            //Load csv data
            CsvDataHolder csvData = new CsvDataHolder(dataFile);
            //Convert csv data to a VectorBundle
            VectorBundle vectorData = VectorBundle.Load(csvData, numOfClasses);
            double binBorder = 0.5d;
            double[] foldDataRatios = { -1d, 0d, 0.1d, 0.5d, 0.75d, 1d, 2d };
            Console.WriteLine($"Folderization test of {dataFile}. NumOfSamples={vectorData.InputVectorCollection.Count.ToString(CultureInfo.InvariantCulture)}, NumOfFoldDataRatios={foldDataRatios.Length.ToString(CultureInfo.InvariantCulture)}");
            foreach (double foldDataRatio in foldDataRatios)
            {
                Console.WriteLine($"  Testing fold data ratio = {foldDataRatio.ToString(CultureInfo.InvariantCulture)}");
                List<VectorBundle> folds = vectorData.Folderize(foldDataRatio, binBorder);
                Console.WriteLine($"    Number of resulting folds = {folds.Count.ToString(CultureInfo.InvariantCulture)}");
                for (int foldIdx = 0; foldIdx < folds.Count; foldIdx++)
                {
                    int numOfFoldSamples = folds[foldIdx].InputVectorCollection.Count;
                    Console.WriteLine($"      FoldIdx={foldIdx.ToString(CultureInfo.InvariantCulture),-4} FoldSize={numOfFoldSamples.ToString(CultureInfo.InvariantCulture),-4}");
                    int[] classesBin1Counts = new int[numOfClasses];
                    classesBin1Counts.Populate(0);
                    for (int sampleIdx = 0; sampleIdx < numOfFoldSamples; sampleIdx++)
                    {
                        for(int classIdx = 0; classIdx < numOfClasses; classIdx++)
                        {
                            if(folds[foldIdx].OutputVectorCollection[sampleIdx][classIdx] >= binBorder)
                            {
                                ++classesBin1Counts[classIdx];
                            }
                        }
                    }
                    Console.WriteLine($"        Number of positive samples per class");
                    for (int classIdx = 0; classIdx < numOfClasses; classIdx++)
                    {
                        Console.WriteLine($"          ClassID={classIdx.ToString(CultureInfo.InvariantCulture), -3}, Bin1Samples={classesBin1Counts[classIdx].ToString(CultureInfo.InvariantCulture)}");
                    }
                }
                Console.ReadLine();
            }
            return;
        }

        /// <summary>
        /// Displays information about the readout unit regression progress.
        /// </summary>
        /// <param name="buildingState">Current state of the regression process</param>
        /// <param name="foundBetter">Indicates that the best readout unit was changed as a result of the performed epoch</param>
        private void OnRegressionEpochDone(TrainedOneTakesAllNetworkBuilder.BuildingState buildingState, bool foundBetter)
        {
            int reportEpochsInterval = 5;
            //Progress info
            if (foundBetter ||
                (buildingState.Epoch % reportEpochsInterval) == 0 ||
                buildingState.Epoch == buildingState.MaxEpochs ||
                (buildingState.Epoch == 1 && buildingState.RegrAttemptNumber == 1)
                )
            {
                //Build progress report message
                string progressText = buildingState.GetProgressInfo(4);
                //Report the progress
                if((buildingState.Epoch == 1 && buildingState.RegrAttemptNumber == 1))
                {
                    Console.WriteLine();
                }
                Console.Write("\x0d" + progressText + "          ");
            }
            return;
        }


        private void TestTrainedOneTakesAllClusterAndBuilder(string trainDataFile, string verifyDataFile, int numOfClasses, double foldDataRatio = 0.1d)
        {
            Console.BufferWidth = 320;
            Console.WriteLine("One Takes All - Cluster and Cluster builder test");
            //Load csv data and create vector bundle
            Console.WriteLine($"Loading {trainDataFile}...");
            CsvDataHolder trainCsvData = new CsvDataHolder(trainDataFile);
            VectorBundle trainData = VectorBundle.Load(trainCsvData, numOfClasses);
            Console.WriteLine($"Loading {verifyDataFile}...");
            CsvDataHolder verifyCsvData = new CsvDataHolder(verifyDataFile);
            VectorBundle verifyData = VectorBundle.Load(verifyCsvData, numOfClasses);
            Console.WriteLine($"Cluster training on {trainDataFile}...");
            //TRAINING
            List<FeedForwardNetworkSettings> netCfgs = new List<FeedForwardNetworkSettings>();
            netCfgs.Add(new FeedForwardNetworkSettings(new AFAnalogSoftMaxSettings(),
                                                       new HiddenLayersSettings(new HiddenLayerSettings(30, new AFAnalogTanHSettings())),
                                                       new RPropTrainerSettings(5, 750)
                                                       )
                        );

            TrainedOneTakesAllNetworkClusterBuilder builder =
                new TrainedOneTakesAllNetworkClusterBuilder("Test",
                                                            netCfgs,
                                                            null,
                                                            null
                                                            );
            builder.RegressionEpochDone += OnRegressionEpochDone;
            TrainedOneTakesAllNetworkCluster tc = builder.Build(trainData, new CrossvalidationSettings(foldDataRatio));

            //VERIFICATION
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine($"Cluster verification on {verifyDataFile}...");
            Console.WriteLine();
            int numOfErrors = 0;
            for(int i = 0; i < verifyData.InputVectorCollection.Count; i++)
            {
                double[] computed = tc.Compute(verifyData.InputVectorCollection[i]);
                int computedWinnerIdx = computed.MaxIdx();
                int realWinnerIdx = verifyData.OutputVectorCollection[i].MaxIdx();
                if (computedWinnerIdx != realWinnerIdx) ++numOfErrors;
                Console.Write("\x0d" + $"({i+1}/{verifyData.InputVectorCollection.Count}) Errors:{numOfErrors}...");
            }
            Console.WriteLine();
            Console.WriteLine($"Accuracy {(1d - (double)numOfErrors/(double)verifyData.InputVectorCollection.Count).ToString(CultureInfo.InvariantCulture)}");
            Console.WriteLine();

            return;
        }

        /// <summary>
        /// Playground's entry point
        /// </summary>
        public void Run()
        {
            Console.Clear();
            //TODO - place your code here
            //TestVectorBundleFolderization("./Data/ProximalPhalanxOutlineAgeGroup_train.csv", 3);
            TestTrainedOneTakesAllClusterAndBuilder("./Data/LibrasMovement_train.csv", "./Data/LibrasMovement_verify.csv", 15, 0.1d);
            TestTrainedOneTakesAllClusterAndBuilder("./Data/ProximalPhalanxOutlineAgeGroup_train.csv", "./Data/ProximalPhalanxOutlineAgeGroup_verify.csv", 3, 0.1d);
            return;
        }


    }//Playground
}
