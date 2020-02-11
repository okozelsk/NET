using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Xml.Linq;
using Demo.DemoConsoleApp.Log;
using RCNet;
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
using RCNet.Neural.Network.SM.Preprocessing;
using RCNet.Queue;
using RCNet.CsvTools;
using RCNet.Neural.Data;

namespace Demo.DemoConsoleApp
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
                Console.WriteLine($"{i}, State {(af.TypeOfActivation == ActivationType.Spiking ? af.InternalState : signal)} signal {signal}");
            }
            Console.ReadLine();

            return;
        }

        public void Run()
        {
            SimpleIFSettings settings = new SimpleIFSettings(new RandomValueSettings(15, 15),
                                                             new RandomValueSettings(0.05, 0.05),
                                                             new RandomValueSettings(5, 5),
                                                             new RandomValueSettings(20, 20),
                                                             0
                                                             );
            IActivationFunction af = ActivationFactory.Create(settings, new Random(0));
            TestActivation(af, 800, 0.15, 10, 600);
            return;
        }



    }//Research
}
