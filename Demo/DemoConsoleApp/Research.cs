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
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Synapse;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Synapse.Dynamics;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Pool;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Pool.NeuronGroup;
using RCNet.Neural.Network.SM.Readout;

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

        private void TestXmlSettings()
        {
            RCNetBaseSettings settings = null;
            settings = new RandomValueSettings(-1, 1);
            Console.WriteLine(settings.GetXml("root", true));
            Console.WriteLine(settings.GetXml("root", false));
            Console.WriteLine();

            settings = new ParamSeekerSettings(0.1, 0.5, 10);
            Console.WriteLine(settings.GetXml("root", true));
            Console.WriteLine(settings.GetXml("root", false));
            Console.WriteLine();

            settings = new AdExpIFSettings();
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            settings = new AutoIzhikevichIFSettings(NeuronCommon.NeuronRole.Excitatory);
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            settings = new BentIdentitySettings();
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            settings = new ElliotSettings();
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            settings = new ExpIFSettings();
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            settings = new GaussianSettings();
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            settings = new IdentitySettings();
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            settings = new ISRUSettings();
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            settings = new IzhikevichIFSettings();
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            settings = new LeakyIFSettings();
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            settings = new LeakyReLUSettings();
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            settings = new SigmoidSettings();
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            settings = new SimpleIFSettings();
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            settings = new SincSettings();
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            settings = new SinusoidSettings();
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            settings = new SoftExponentialSettings();
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            settings = new SoftPlusSettings();
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            settings = new SQNLSettings();
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            settings = new TanHSettings();
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            settings = new BinFeatureFilterSettings();
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            settings = new EnumFeatureFilterSettings(5);
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            settings = new RealFeatureFilterSettings(true, true, false);
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            settings = new MackeyGlassGeneratorSettings();
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            settings = new PulseGeneratorSettings(0.5, 3, PulseGenerator.TimingMode.Constant);
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            settings = new SinusoidalGeneratorSettings();
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            settings = new ElasticRegrTrainerSettings(1000);
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            settings = new QRDRegrTrainerSettings(3, 1000);
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            settings = new RidgeRegrTrainerSettings(1000);
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            settings = new RPropTrainerSettings(3, 1000);
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            settings = new PDeltaRuleTrainerSettings(3, 1000);
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            FeedForwardNetworkSettings ffMLNetCfg = new FeedForwardNetworkSettings(new ElliotSettings(),
                                                                                   new HiddenLayersSettings(new HiddenLayerSettings(10, new TanHSettings()),
                                                                                                            new HiddenLayerSettings(10, new LeakyReLUSettings())
                                                                                                            ),
                                                                                   new RPropTrainerSettings(3, 1000)
                                                                                   );

            FeedForwardNetworkSettings ffSLNetCfg = new FeedForwardNetworkSettings(new ElliotSettings(),
                                                                                   null,
                                                                                   new RPropTrainerSettings(3, 400)
                                                                                   );


            settings = ffMLNetCfg;
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            ParallelPerceptronSettings ppNetCfg = new ParallelPerceptronSettings(3, 2, new PDeltaRuleTrainerSettings(3, 1000));

            settings = ppNetCfg;
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            PredictorsParamsSettings predictorsParamsCfg = new PredictorsParamsSettings(new FiringFadingSumSettings(0.001));
            
            settings = predictorsParamsCfg;
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();


            PredictorsSettings predictorsCfg = new PredictorsSettings(false,
                                                                           false,
                                                                           false,
                                                                           false,
                                                                           true,
                                                                           false,
                                                                           false,
                                                                           false,
                                                                           predictorsParamsCfg
                                                                           );

            settings = predictorsCfg;
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            settings = new InputUnitSettings("inputField1", new InputUnitConnsSettings(new InputUnitConnSettings("Pool1", 0.5)));
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            settings = new SpikingTargetSettings();
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            settings = new AnalogTargetSettings();
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            settings = new InputSynapseSettings();
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            settings = new InternalSynapseSettings(new S2SSynapseE2EDynamicsSettings(0.4), new S2ASynapseE2EDynamicsSettings(0.8));
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            ClassificationNetworksSettings clNetsCfg = new ClassificationNetworksSettings(ffMLNetCfg, ppNetCfg);

            settings = clNetsCfg;
            Console.WriteLine(settings.GetXml("networks", true));
            Console.WriteLine(settings.GetXml("networks", false));
            Console.WriteLine();

            ForecastNetworksSettings fcNetsCfg = new ForecastNetworksSettings(ffMLNetCfg, ffSLNetCfg);

            settings = fcNetsCfg;
            Console.WriteLine(settings.GetXml("networks", true));
            Console.WriteLine(settings.GetXml("networks", false));
            Console.WriteLine();

            DefaultNetworksSettings defNetsCfg = new DefaultNetworksSettings(clNetsCfg, fcNetsCfg);

            settings = defNetsCfg;
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            ForecastTaskSettings fcTaskCfg = new ForecastTaskSettings(new RealFeatureFilterSettings(), fcNetsCfg);

            settings = fcTaskCfg;
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            ClassificationTaskSettings clTaskCfg = new ClassificationTaskSettings(ClassificationTaskSettings.DefaultOneWinnerGroupName, null);

            settings = clTaskCfg;
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            ReadoutUnitSettings ru0Cfg = new ReadoutUnitSettings("field1", fcTaskCfg);
            
            settings = ru0Cfg;
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            ReadoutUnitSettings ru1Cfg = new ReadoutUnitSettings("field2", clTaskCfg);

            ReadoutUnitsSettings roUnitsCfg = new ReadoutUnitsSettings(ru0Cfg, ru1Cfg);

            settings = roUnitsCfg;
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            ReadoutLayerSettings rlCfg = new ReadoutLayerSettings(roUnitsCfg, 0.1, 0, 1, defNetsCfg);

            settings = rlCfg;
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            ChainSchemaSettings chainSchemaCfg = new ChainSchemaSettings(0.5);

            settings = chainSchemaCfg;
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            RandomSchemaSettings randSchemaCfg = new RandomSchemaSettings(new ConnDistrFlatSettings(),  0.2);

            settings = randSchemaCfg;
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            InterconnSettings pInterconnCfg = new InterconnSettings(randSchemaCfg, chainSchemaCfg);

            settings = pInterconnCfg;
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            SpikingNeuronGroupSettings psngExcitatoryCfg = new SpikingNeuronGroupSettings("ana-exc-grp1",
                                                                                                  NeuronCommon.NeuronRole.Excitatory,
                                                                                                  1,
                                                                                                  new SimpleIFSettings(),
                                                                                                  0.5,
                                                                                                  new RandomValueSettings(-0.1, 0.1),
                                                                                                  predictorsCfg
                                                                                                  );

            settings = psngExcitatoryCfg;
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            AnalogRetainmentSettings anRetainmentCfg = new AnalogRetainmentSettings(1d, new URandomValueSettings(0.1d, 0.75d));
            settings = anRetainmentCfg;
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            AnalogNeuronGroupSettings pangExcitatoryCfg = new AnalogNeuronGroupSettings("spiking-exc-grp1",
                                                                                                NeuronCommon.NeuronRole.Excitatory,
                                                                                                1,
                                                                                                new TanHSettings(),
                                                                                                0.0015,
                                                                                                NeuronCommon.NeuronSignalingRestrictionType.AnalogOnly,
                                                                                                0.5,
                                                                                                new RandomValueSettings(-0.1, 0.1),
                                                                                                new AnalogRetainmentSettings(1, new URandomValueSettings(0.25, 0.75)),
                                                                                                predictorsCfg
                                                                                                );

            settings = pangExcitatoryCfg;
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            NeuronGroupsSettings groupsCfg = new NeuronGroupsSettings(psngExcitatoryCfg, pangExcitatoryCfg);

            settings = groupsCfg;
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            ProportionsSettings proportionsCfg = new ProportionsSettings(5, 5, 5);

            settings = proportionsCfg;
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            PoolSettings pool1Cfg = new PoolSettings("Pool1", proportionsCfg, groupsCfg, pInterconnCfg, predictorsCfg);

            settings = pool1Cfg;
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            PoolSettings pool2Cfg = new PoolSettings("Pool2", proportionsCfg, groupsCfg, pInterconnCfg, predictorsCfg);

            PoolsSettings poolsCfg = new PoolsSettings(pool1Cfg, pool2Cfg);

            settings = poolsCfg;
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            InterPoolConnSettings interPoolConnCfg = new InterPoolConnSettings(new ConnDistrFlatSettings(),
                                                                                                                               "Pool1",
                                                                                                                               0.5,
                                                                                                                               "Pool2",
                                                                                                                               0.05
                                                                                                                               );
            settings = interPoolConnCfg;
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            InterPoolConnsSettings interPoolConnsCfg = new InterPoolConnsSettings(interPoolConnCfg);

            settings = interPoolConnsCfg;
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();

            ReservoirStructureSettings resStructCfg = new ReservoirStructureSettings("MainCfg", poolsCfg, interPoolConnsCfg);

            settings = resStructCfg;
            Console.WriteLine(settings.GetXml(true));
            Console.WriteLine(settings.GetXml(false));
            Console.WriteLine();


            Console.ReadLine();
            

            return;
        }

        public void Run()
        {
            
            TestXmlSettings();

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
