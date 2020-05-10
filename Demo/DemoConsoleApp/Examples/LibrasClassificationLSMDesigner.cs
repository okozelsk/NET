using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using RCNet.RandomValue;
using RCNet.Extensions;
using RCNet.Neural.Data.Filter;
using RCNet.Neural.Activation;
using RCNet.Neural.Network.NonRecurrent.FF;
using RCNet.Neural.Network.SM.Preprocessing.Input;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Pool;
using RCNet.Neural.Network.SM.Preprocessing.Neuron;
using RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Space3D;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Pool.NeuronGroup;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS;
using RCNet.Neural.Network.SM.Preprocessing;
using RCNet.Neural.Network.SM.Readout;
using RCNet.Neural.Network.SM;

namespace Demo.DemoConsoleApp.Examples
{
    /// <summary>
    /// Example code shows how to setup StateMachine as a pure LSM for classification using StateMachineDesigner.
    /// Example uses LibrasMovement_train.csv and LibrasMovement_verify.csv from ./Data subfolder.
    /// The dataset is from https://archive.ics.uci.edu/ml/datasets/Libras+Movement and contains 15 classes of 24 instances each, where
    /// each class references to a hand movement type in LIBRAS.The hand movement is represented as a bidimensional curve performed
    /// by the hand in a period of time.The curves were obtained from videos of hand movements, with the Libras performance from 4 
    /// different people, during 2 sessions.Each video corresponds to only one hand movement and has about 7 seconds.
    /// Each video corresponds to a function F in a functions space which is the continual version of the input dataset.
    /// In the video pre-processing, a time normalization is carried out selecting 45 frames from each video, in according
    /// to an uniform distribution.In each frame, the centroid pixels of the segmented objects (the hand) are found, which
    /// compose the discrete version of the curve F with 45 points.All curves are normalized in the unitary space.
    /// Each curve F is mapped in a representation with 90 features, with representing the coordinates of movement.
    /// Each instance represents 45 points on a bi-dimensional space, which can be plotted in an ordered way (from 1 through
    /// 45 as the X co-ordinate) in order to draw the path of the movement.
    /// </summary>
    public class LibrasClassificationLSMDesigner : BaseExample
    {
        /// <summary>
        /// Runs the example code.
        /// </summary>
        public void Run()
        {
            //Create StateMachine configuration
            //Simplified input configuration
            InputEncoderSettings inputCfg = StateMachineDesigner.CreateInputCfg(new FeedingPatternedSettings(true, false, RCNet.Neural.Data.InputPattern.VariablesSchema.Groupped),
                                                                         new PredictorsSettings(false, false, false, false, true, false, true, false),
                                                                         new ExternalFieldSettings("coord_abcissa", new RealFeatureFilterSettings(), true, new SpikeCodeSettings(4, Math.E, true, true)),
                                                                         new ExternalFieldSettings("coord_ordinate", new RealFeatureFilterSettings(), true, new SpikeCodeSettings(4, Math.E, true, true))
                                                                         );
            //Simplified readout layer configuration
            ReadoutLayerSettings readoutCfg = StateMachineDesigner.CreateClassificationReadoutCfg(StateMachineDesigner.CreateSingleLayerRegrNet(new ElliotSettings(), 5, 400),
                                                                                                  0.0825d,
                                                                                                  1,
                                                                                                  "Hand movement",
                                                                                                  "curved swing",
                                                                                                  "horizontal swing",
                                                                                                  "vertical swing",
                                                                                                  "anti-clockwise arc",
                                                                                                  "clockwise arc",
                                                                                                  "circle",
                                                                                                  "horizontal straight-line",
                                                                                                  "vertical straight-line",
                                                                                                  "horizontal zigzag",
                                                                                                  "vertical zigzag",
                                                                                                  "horizontal wavy",
                                                                                                  "vertical wavy",
                                                                                                  "face-up curve",
                                                                                                  "face-down curve",
                                                                                                  "tremble"
                                                                                                  );
            //Create designer instance
            StateMachineDesigner smd = new StateMachineDesigner(inputCfg, readoutCfg);
            //Create pure LSM fashioned StateMachine configuration
            StateMachineSettings stateMachineCfg = smd.CreatePureLSMCfg(new ProportionsSettings(6, 5, 5), //Proportions (it also determines total size)
                                                                        new AdExpIFSettings(), //Activation
                                                                        new HomogenousExcitabilitySettings(0.5, 0.5, 0.25),
                                                                        0.5, //Prime neurons ratio
                                                                        1d, //Input connection density
                                                                        0, //Input max delay
                                                                        0.1d, //Interconnection density
                                                                        0, //Internal synapses max delay
                                                                        0, //Steady bias
                                                                        new PredictorsParamsSettings(new FiringCountSettings(45)),
                                                                        PredictorsProvider.PredictorID.FiringCount,
                                                                        PredictorsProvider.PredictorID.FiringFadingSum
                                                                        );

            //Display StateMachine xml configuration
            string xmlConfig = stateMachineCfg.GetXml(true).ToString();
            _log.Write("StateMachine configuration xml:");
            _log.Write("-------------------------------");
            _log.Write(xmlConfig);
            _log.Write(string.Empty);
            _log.Write("Pres Enter to continue (StateMachine training and verification)...");
            _log.Write(string.Empty);
            Console.ReadLine();

            //Instantiation and training
            _log.Write("StateMachine training:");
            _log.Write("----------------------");
            _log.Write(string.Empty);
            //StateMachine instance
            StateMachine stateMachine = new StateMachine(stateMachineCfg);
            //StateMachine training
            TrainStateMachine(stateMachine, ".\\Data\\LibrasMovement_train.csv", out _);
            _log.Write(string.Empty);
            //StateMachine verification
            _log.Write("StateMachine verification:");
            _log.Write("--------------------------");
            _log.Write(string.Empty);
            VerifyStateMachine(stateMachine, ".\\Data\\LibrasMovement_verify.csv", null, out _);
            _log.Write(string.Empty);

            return;
        }

    }//LibrasClassificationLSMDesigner

}//Namespace
