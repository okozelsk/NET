using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.DemoConsoleApp.Log;
using RCNet.Neural.Activation;
using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.MathTools.MatrixMath;
using RCNet.Neural.Network.SM;
using RCNet.MathTools.Differential;
using RCNet.MathTools.VectorMath;
using RCNet.RandomValue;
using RCNet.Neural.Data.Generators;
using System.Globalization;

namespace RCNet.DemoConsoleApp
{
    /// <summary>
    /// This class is a free "playground", the place where it is possible to test new concepts or somewhat else
    /// </summary>
    class Research
    {
        //Attributes

        //Constructor
        public Research()
        {
            return;
        }

        //Methods
        public void Run()
        {

            //Linear algebra test
            int numOfweights = 4;
            int xIdx, dIdx = 0;
            double[][] data = new double[5][];
            data[dIdx] = new double[numOfweights];
            xIdx = -1;
            data[dIdx][++xIdx] = 1; //Bias
            data[dIdx][++xIdx] = 2;
            data[dIdx][++xIdx] = 1;
            data[dIdx][++xIdx] = 3;
            ++dIdx;
            data[dIdx] = new double[numOfweights];
            xIdx = -1;
            data[dIdx][++xIdx] = 1; //Bias
            data[dIdx][++xIdx] = 1;
            data[dIdx][++xIdx] = 3;
            data[dIdx][++xIdx] = -3;
            ++dIdx;
            data[dIdx] = new double[numOfweights];
            xIdx = -1;
            data[dIdx][++xIdx] = 1; //Bias
            data[dIdx][++xIdx] = -2;
            data[dIdx][++xIdx] = 4;
            data[dIdx][++xIdx] = 4;
            ++dIdx;
            data[dIdx] = new double[numOfweights];
            xIdx = -1;
            data[dIdx][++xIdx] = 1; //Bias
            data[dIdx][++xIdx] = -5;
            data[dIdx][++xIdx] = 7;
            data[dIdx][++xIdx] = 6;
            ++dIdx;
            data[dIdx] = new double[numOfweights];
            xIdx = -1;
            data[dIdx][++xIdx] = 1; //Bias
            data[dIdx][++xIdx] = 1;
            data[dIdx][++xIdx] = 12;
            data[dIdx][++xIdx] = 5;

            Matrix M = new Matrix(data, true);
            Vector desired = new Vector(5);
            dIdx = -1;
            desired.Data[++dIdx] = 8;
            desired.Data[++dIdx] = 13;
            desired.Data[++dIdx] = 5;
            desired.Data[++dIdx] = 7;
            desired.Data[++dIdx] = 10;

            Vector weights = Matrix.RidgeRegression(M, desired, 0);

            //Display results
            for(int i = 0; i < data.Length; i++)
            {
                double result = 0;
                for(int j = 0; j < weights.Length; j++)
                {
                    result += data[i][j] * weights[j];
                }
                Console.WriteLine($"Computed {result}, Desired {desired.Data[i]}");
            }
            Console.WriteLine();

            //QRD
            QRD decomposition = new QRD(M);
            Matrix B = new Matrix(desired.Length, 1, desired.Data);
            Matrix W = decomposition.Solve(B);
            //Display results
            for (int i = 0; i < data.Length; i++)
            {
                double result = 0;
                for (int j = 0; j < W.Data.Length; j++)
                {
                    result += data[i][j] * W.Data[j][0];
                }
                Console.WriteLine($"Computed {result}, Desired {desired.Data[i]}");

            }



            ;




            //TimeSeriesGenerator.SaveTimeSeriesToCsvFile("MackeyGlass_big.csv", "Value", TimeSeriesGenerator.GenMackeyGlassTimeSeries(16000), CultureInfo.InvariantCulture);
            MackeyGlassGeneratorSettings modSettings = new MackeyGlassGeneratorSettings(18, 0.1, 0.2);
            IGenerator generator = new MackeyGlassGenerator(modSettings);

            int steps = 100;
            for (int i = 0; i < steps; i++)
            {
                Console.WriteLine(generator.Next());
            }
            Console.ReadLine();
            generator.Reset();
            for (int i = 0; i < steps; i++)
            {
                Console.WriteLine(generator.Next());
            }
            Console.ReadLine();

            ///*
            SimpleIFSettings settings = new SimpleIFSettings(1,
                                                             new RandomValueSettings(15, 15),
                                                             new RandomValueSettings(0.05, 0.05),
                                                             new RandomValueSettings(5, 5),
                                                             new RandomValueSettings(20, 20),
                                                             0
                                                             );
            IActivationFunction af = ActivationFactory.Create(settings, new Random(0));
            //*/
            TestActivation(af, 800, 0.15, 10, 600);
            return;
        }

        private void TestActivation(IActivationFunction af, int simLength, double constCurrent, int from, int count)
        {
            Random rand = new Random();
            for (int i = 1; i <= simLength; i++)
            {
                double signal = 0;
                if (i >= from && i < from + count)
                {
                    double input = double.IsNaN(constCurrent) ? rand.NextDouble(0, 1, false, RandomClassExtensions.DistributionType.Uniform) : constCurrent;
                    signal = af.Compute(input);
                }
                else
                {
                    signal = af.Compute(0);
                }
                Console.WriteLine($"{i}, State {af.InternalState} signal {signal}");
            }
            Console.ReadLine();

            return;
        }


    }//Research
}
