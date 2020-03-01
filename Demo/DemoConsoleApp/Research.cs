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
using RCNet.MathTools.Differential;
using RCNet.MathTools.VectorMath;
using RCNet.MathTools.PS;
using RCNet.RandomValue;
using RCNet.Queue;
using RCNet.CsvTools;
using RCNet.Neural.Data;
using RCNet.Neural.Data.Generators;
using RCNet.Neural.Data.Filter;
using RCNet.Neural.Network.NonRecurrent.FF;
using RCNet.Neural.Network.NonRecurrent.PP;
using RCNet.Neural.Network.NonRecurrent;
using RCNet.Neural.Network.SM;
using RCNet.Neural.Network.SM.Preprocessing;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Space3D;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Neuron;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Neuron.Predictor;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Pool;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Pool.NeuronGroup;
using RCNet.Neural.Network.SM.Readout;
using RCNet.Neural.Network.SM.Preprocessing.Input;

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
            InputSettings inputCfg = StateMachineDesigner.CreateInputCfg(new FeedingContinuousSettings(FeedingContinuousSettings.AutoBootCyclesNum, true),
                                                                         new ExternalFieldSettings("High", new RealFeatureFilterSettings()),
                                                                         new ExternalFieldSettings("Low", new RealFeatureFilterSettings()),
                                                                         new ExternalFieldSettings("Adj Close", new RealFeatureFilterSettings())
                                                                         );

            ReadoutLayerSettings readoutCfg = StateMachineDesigner.CreateForecastReadoutCfg(StateMachineDesigner.CreateIdentityRegrNet(2, 800),
                                                                                            0.1d,
                                                                                            "High",
                                                                                            "Low",
                                                                                            "Adj Close"
                                                                                            );

            StateMachineDesigner smd = new StateMachineDesigner(inputCfg, readoutCfg);

            StateMachineSettings esnCfg = smd.CreatePureESNCfg(200, 1, 0, 0.1d, 0, 0.05d, 0.75d, PredictorsProvider.PredictorID.Activation, PredictorsProvider.PredictorID.ActivationSquare);

            Console.WriteLine(esnCfg.GetXml(true).ToString());
            Console.ReadLine();

            //string name = smd.GetResInstName(StateMachineDesigner.ResDesign.Hybrid, 0);

            ;

            SimpleIFSettings settings = new SimpleIFSettings(new URandomValueSettings(15, 15),
                                                             new URandomValueSettings(0.05, 0.05),
                                                             new URandomValueSettings(5, 5),
                                                             new URandomValueSettings(20, 20),
                                                             0
                                                             );
            IActivationFunction af = ActivationFactory.Create(settings, new Random(0));
            TestActivation(af, 800, 0.15, 10, 600);
            return;
        }



    }//Research
}
