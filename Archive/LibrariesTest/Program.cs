using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using OKOSW.Extensions;
using OKOSW.CSVTools;
using OKOSW.MathTools;
using OKOSW.Neural.Networks.FF.Basic;
using OKOSW.Neural.Activation;


namespace OKOSW.LibrariesTest
{
    class Program
    {
        static void Main(string[] args)
        {
            int numInput = 500;
            int numHidden = 10;
            int numOutput = 3;
            int numRows = 1000;
            int seed = 0;
            int maxEpochs = 500;

            //Data preparation for both networks
            double[][] allData = MakeAllData(numInput, numHidden, numOutput, numRows, seed);
            double[][] trainData;
            double[][] testData;
            MakeTrainTest(allData, 0.80, seed, out trainData, out testData);
            List<double[]> trainInputs = new List<double[]>(trainData.Length);
            List<double[]> trainOutputs = new List<double[]>(trainData.Length);
            foreach (double[] rec in trainData)
            {
                double[] inp = new double[numInput];
                Array.Copy(rec, 0, inp, 0, numInput);
                trainInputs.Add(inp);
                double[] outp = new double[numOutput];
                Array.Copy(rec, numInput, outp, 0, numOutput);
                trainOutputs.Add(outp);
            }

            //OKOSW network
            Console.WriteLine("OKOSW BasicNetwork training:");
            BasicNetwork net = new BasicNetwork(numInput, numOutput);
            net.AddLayer(numHidden, new TanhAF());
            net.FinalizeStructure(new TanhAF());
            Random rand = new Random(seed);
            net.RandomizeWeights(rand);
            RPropTrainer train = new RPropTrainer(net, trainInputs, trainOutputs);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 1; i <= maxEpochs; i++)
            {
                train.Iteration();
                if (i % 100 == 0)
                {
                    double rmse = net.ComputeBatchErrorStat(trainInputs, trainOutputs).RootMeanSquare;
                    Console.Write("Epoch: " + i.ToString() + " RMSE: " + rmse.ToString() + "\r");
                }
            }
            sw.Stop();
            Console.WriteLine("\nElapsed time: " + sw.Elapsed.ToString());


            //Original network
            Console.WriteLine("\nORG neural network training:");
            BasicNetworkOrg nn = new BasicNetworkOrg(numInput, numHidden, numOutput);
            sw.Reset();
            sw.Start();
            double[] weights = nn.TrainRPROP(trainData, maxEpochs); // RPROP
            sw.Stop();
            Console.WriteLine("\nElapsed time: " + sw.Elapsed.ToString());


            Console.ReadLine();
        } // Main

        static double[][] MakeAllData(int numInput, int numHidden, int numOutput,
            int numRows, int seed)
        {
            Random rnd = new Random(seed + 1000);
            int numWeights = (numInput * numHidden) + numHidden +
            (numHidden * numOutput) + numOutput;
            double[] weights = new double[numWeights]; // actually weights & biases
            for (int i = 0; i < numWeights; ++i)
                weights[i] = 2.0 * rnd.NextDouble() - 1.0; // [-1.0 to 1.0]

            Console.WriteLine("Generating weights:");

            double[][] result = new double[numRows][]; // allocate return-result matrix
            for (int i = 0; i < numRows; ++i)
                result[i] = new double[numInput + numOutput]; // 1-of-N Y in last column

            BasicNetworkOrg gnn = new BasicNetworkOrg(numInput, numHidden, numOutput); // generating NN
            gnn.SetWeights(weights);

            for (int r = 0; r < numRows; ++r) // for each row
            {
                // generate random inputs
                double[] inputs = new double[numInput];
                for (int i = 0; i < numInput; ++i)
                    inputs[i] = 2.0 * rnd.NextDouble() - 1.0; // [-1.0 to 1.0]

                // compute outputs
                double[] outputs = gnn.ComputeOutputs(inputs);
                // translate outputs to 1-of-N
                double[] oneOfN = new double[numOutput]; // all 0.0
                int maxIndex = 0;
                double maxValue = outputs[0];
                for (int i = 0; i < numOutput; ++i)
                {
                    if (outputs[i] > maxValue)
                    {
                        maxIndex = i;
                        maxValue = outputs[i];
                    }
                }
                oneOfN[maxIndex] = 1.0;

                // place inputs and 1-of-N output values into curr row
                int c = 0; // column into result[][]
                for (int i = 0; i < numInput; ++i) // inputs
                    result[r][c++] = inputs[i];
                for (int i = 0; i < numOutput; ++i) // outputs
                    result[r][c++] = oneOfN[i];
            } // each row
            return result;
        } // MakeAllData

        static void MakeTrainTest(double[][] allData, double trainPct, int seed,
            out double[][] trainData, out double[][] testData)
        {
            Random rnd = new Random(seed);
            int totRows = allData.Length;
            int numTrainRows = (int)(totRows * trainPct); // usually 0.80
            int numTestRows = totRows - numTrainRows;
            trainData = new double[numTrainRows][];
            testData = new double[numTestRows][];

            double[][] copy = new double[allData.Length][]; // ref copy of all data
            for (int i = 0; i < copy.Length; ++i)
                copy[i] = allData[i];

            for (int i = 0; i < copy.Length; ++i) // scramble order
            {
                int r = rnd.Next(i, copy.Length); // use Fisher-Yates
                double[] tmp = copy[r];
                copy[r] = copy[i];
                copy[i] = tmp;
            }
            for (int i = 0; i < numTrainRows; ++i)
                trainData[i] = copy[i];

            for (int i = 0; i < numTestRows; ++i)
                testData[i] = copy[i + numTrainRows];
        } // MakeTrainTest


    }//Program
}//Namespace
