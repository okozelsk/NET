using System;
using RCNet.Neural.Activation;
using RCNet.Neural.Data.Coders.AnalogToSpiking;
using RCNet.Neural.Data.Filter;
using RCNet.Neural.Network.NonRecurrent;
using RCNet.Neural.Network.SM;
using RCNet.Neural.Network.SM.Preprocessing;
using RCNet.Neural.Network.SM.Preprocessing.Input;
using RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Pool.NeuronGroup;
using RCNet.Neural.Network.SM.Readout;

namespace Demo.DemoConsoleApp.Examples
{
    /// <summary>
    /// Example code shows how to setup StateMachine having bypassed neural preprocessor.
    /// Example uses LibrasMovement_train.csv and LibrasMovement_verify.csv from ./Data subfolder.
    /// The dataset is from "Anthony Bagnall, Jason Lines, William Vickers and Eamonn Keogh, The UEA & UCR Time Series Classification Repository, www.timeseriesclassification.com"
    /// https://timeseriesclassification.com/description.php?Dataset=Libras
    /// and contains 15 classes of 24 instances each, where
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
    public class LibrasClassificationNPBypassedDesigner : ExampleBase
    {
        /// <summary>
        /// Runs the example code.
        /// </summary>
        public void Run()
        {
            //Create StateMachine configuration
            //Simplified readout layer configuration using FF-network having 2 hidden layers as the classifier
            ReadoutLayerSettings readoutCfg = StateMachineDesigner.CreateClassificationReadoutCfg(new CrossvalidationSettings(0.0825d, 0, 1),
                                                                                                  StateMachineDesigner.CreateMultiLayerRegrNet(10, new AFAnalogLeakyReLUSettings(), 2, 5, 400),
                                                                                                  "Hand movement",
                                                                                                  new NetworkClusterSecondLevelCompSettings(new CrossvalidationSettings(0.25d, CrossvalidationSettings.AutoFolds, 2),
                                                                                                                                            StateMachineDesigner.CreateMultiLayerRegrNet(10, new AFAnalogLeakyReLUSettings(), 1, 5, 400),
                                                                                                                                            TrainedNetworkCluster.SecondLevelCompMode.SecondLevelOutputOnly
                                                                                                                                            ),
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
            StateMachineDesigner smd = new StateMachineDesigner(readoutCfg);
            //Create StateMachine configuration without preprocessing
            StateMachineSettings stateMachineCfg = smd.CreateBypassedCfg();

            //Display StateMachine xml configuration
            string xmlConfig = stateMachineCfg.GetXml(true).ToString();
            _log.Write("StateMachine configuration xml:");
            _log.Write("-------------------------------");
            _log.Write(xmlConfig);
            _log.Write(string.Empty);
            _log.Write("Press Enter to continue (StateMachine training and verification)...");
            _log.Write(string.Empty);
            Console.ReadLine();

            //Instantiation and training
            _log.Write("StateMachine training:");
            _log.Write("----------------------");
            _log.Write(string.Empty);
            //StateMachine instance
            StateMachine stateMachine = new StateMachine(stateMachineCfg);
            //StateMachine training
            TrainStateMachine(stateMachine, "./Data/LibrasMovement_train.csv", out _);
            _log.Write(string.Empty);
            //StateMachine verification
            _log.Write("StateMachine verification:");
            _log.Write("--------------------------");
            _log.Write(string.Empty);
            VerifyStateMachine(stateMachine, "./Data/LibrasMovement_verify.csv", null, out _);
            _log.Write(string.Empty);

            return;
        }

    }//LibrasClassificationNPBypassedDesigner

}//Namespace
