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
