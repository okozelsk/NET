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
using RCNet.Neural.Data.Filter;
using System.Globalization;

namespace RCNet.DemoConsoleApp
{
    /// <summary>
    /// This class is a free "playground", the place where it is possible to test new concepts or somewhat else
    /// </summary>
    class Research
    {
        //Attributes
        private readonly Random _rand;

        //Constructor
        public Research()
        {
            _rand = new Random();
            return;
        }

        //Methods
        public void Run()
        {


            IActivationFunction testAF = ActivationFactory.Create(new AutoIzhikevichIFSettings(Neural.Network.SM.Neuron.NeuronCommon.NeuronRole.Excitatory), new Random(0));
            TestActivation(testAF, 200, 0.47, 10, 170);

            AutoIzhikevichIFSettings setup = new AutoIzhikevichIFSettings(Neural.Network.SM.Neuron.NeuronCommon.NeuronRole.Excitatory);
            FindAFInputBorders(ActivationFactory.Create(setup, new Random(0)), -0.1, 20);


            Console.ReadLine();


            //Filter test
            RealFeatureFilter rff = new RealFeatureFilter(new Interval(-1, 1));
            for(int i = 1; i <= 1500; i++)
            {
                rff.Update(_rand.NextDouble() * _rand.Next(0, 10000));
            }
            double featureValue = 0.5;
            double filterValue = rff.ApplyFilter(featureValue);
            double reverseValue = rff.ApplyReverse(filterValue);
            Console.WriteLine($"Feature: {featureValue} Filter: {filterValue} Reverse: {reverseValue}");





            //Pulse generator test
            BasicStat sampleStat = new BasicStat();
            sampleStat.Reset();
            PulseGeneratorSettings modSettings = new PulseGeneratorSettings(1, 1.5, PulseGeneratorSettings.TimingMode.Poisson);
            IGenerator generator = new PulseGenerator(modSettings);

            int steps = 10000;
            double period = 0;
            for (int i = 0; i < steps; i++)
            {
                ++period;
                double sample = generator.Next();
                //Console.WriteLine(sample);
                if(sample != 0)
                {
                    sampleStat.AddSampleValue(period);
                    period = 0;
                }
            }
            Console.WriteLine($"Mean: {sampleStat.ArithAvg} StdDev: {sampleStat.StdDev} Min: {sampleStat.Min} Max: {sampleStat.Max}");
            Console.ReadLine();



            //Random distributions test
            BasicStat rStat = new BasicStat();
            for(int i = 0; i < 200; i++)
            {
                double r = _rand.NextFilterredGaussianDouble(0.5, 1, -0.5, 1);
                rStat.AddSampleValue(r);
                Console.WriteLine(r);
            }
            Console.WriteLine($"Mean: {rStat.ArithAvg} StdDev: {rStat.StdDev} Min: {rStat.Min} Max: {rStat.Max}");
            Console.ReadLine();



            //Activation tests
            double fadingSum = 0;
            for(int i = 0; i < 1000; i++)
            {
                fadingSum *= (1d - 0.1);
                fadingSum += 1d;
                Console.WriteLine(fadingSum);
            }

            Console.ReadLine();



            /*
            //Linear algebra test
            double[] flatData = {
                                  0.2, 5, 17.3, 1.01, 54, 7,
                                  2.2, 5.5, 12.13, 11.57, 5.71, -85,
                                  -70.1, 15, -18.3, 0.3, 42, -6.25,
                                  0.042, 1, 7.75, -81.01, -21.29, 5.44,
                                  0.1, 4, -4.3, 18.01, 7.12, -3.14,
                                  -80.1, 24.4, 4.3, 12.03, 2.789, -13
                                 };
            Matrix testM = new Matrix(6, 6, flatData);

            //Inversion test
            Matrix resultM = new Matrix(testM);
            resultM.SingleThreadInverse();
            */
            /*
            //Transpose test
            Matrix resultM = testM.Transpose();
            */

            /*
            //Multiply test
            Matrix resultM = Matrix.Multiply(testM, testM);
            for (int i = 0; i < resultM.NumOfRows; i++)
            {
                Console.WriteLine($"{resultM.Data[i][0]}; {resultM.Data[i][1]}; {resultM.Data[i][2]}; {resultM.Data[i][3]}; {resultM.Data[i][4]}; {resultM.Data[i][5]}");
            }
            */



            ;



            int numOfweights = 3;
            int xIdx, dIdx = 0;
            double[][] data = new double[3][];
            data[dIdx] = new double[numOfweights];
            xIdx = -1;
            data[dIdx][++xIdx] = 2;
            data[dIdx][++xIdx] = 1;
            data[dIdx][++xIdx] = 3;
            ++dIdx;
            data[dIdx] = new double[numOfweights];
            xIdx = -1;
            data[dIdx][++xIdx] = 1;
            data[dIdx][++xIdx] = 3;
            data[dIdx][++xIdx] = -3;
            ++dIdx;
            data[dIdx] = new double[numOfweights];
            xIdx = -1;
            data[dIdx][++xIdx] = -2;
            data[dIdx][++xIdx] = 4;
            data[dIdx][++xIdx] = 4;

            //Matrix M = new Matrix(data, true);
            //Matrix I = M.Inverse(false);
            //Matrix identity = M * I; //Must lead to identity matrix


            Matrix I = new Matrix(3, 3);
            I.AddScalarToDiagonal(1);
            Matrix X = new Matrix(I);
            X.Multiply(0.1);

            Matrix XT = X.Transpose();
            Matrix R = XT * X;


            Console.ReadLine();





            ///*
            SimpleIFSettings settings = new SimpleIFSettings(new RandomValueSettings(15, 15),
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
                double signal;
                if (i >= from && i < from + count)
                {
                    double input = double.IsNaN(constCurrent) ? rand.NextDouble() : constCurrent;
                    signal = af.Compute(input);
                }
                else
                {
                    signal = af.Compute(0);
                }
                Console.WriteLine($"{i}, State {(af.TypeOfActivation == Neural.Activation.ActivationType.Spiking ? af.InternalState : signal)} signal {signal}");
            }
            Console.ReadLine();

            return;
        }

        private double FindCurrent(IActivationFunction af, double targetResponse, double tolerance)
        {
            double lo = -100, hi = 100;
            while (Math.Abs(lo - hi) > 1e-4)
            {
                af.Reset();
                double current = (lo + (hi - lo) / 2);
                double response = af.Compute(current);
                if (af.TypeOfActivation == Neural.Activation.ActivationType.Spiking)
                {
                    response = af.InternalState;
                }
                Console.CursorLeft = 0;
                Console.Write($"lo {lo}, hi {hi}, current {current}, response {response}".PadRight(150, ' '));
                if (Math.Abs(response - targetResponse) <= tolerance)
                {
                    Console.WriteLine();
                    return current;
                }
                if (response > targetResponse)
                {
                    hi = current;
                }
                else if (response < targetResponse)
                {
                    lo = current;
                }
            }
            return double.NaN;
        }

        private void FindAFInputBorders(IActivationFunction af, double minResponse, double maxResponse)
        {
            const double tolerance = 1e-6;
            Console.WriteLine($"AF: {af.ToString()} minResponse {minResponse} maxResponse {maxResponse}");
            Console.WriteLine($"Found min current: {FindCurrent(af, minResponse, tolerance)}");
            Console.WriteLine($"Found max current: {FindCurrent(af, maxResponse, tolerance)}");
            Console.ReadLine();
            return;
        }


    }//Research
}
