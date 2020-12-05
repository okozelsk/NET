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
    /// Example code shows how to setup StateMachine as a pure LSM for classification using StateMachineDesigner and various
    /// ways of input encoding for LSM spiking hidden neurons.
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
    public class LibrasClassificationLSMDesigner : ExampleBase
    {
        /// <summary>
        /// Runs the example code.
        /// </summary>
        public void Run(InputEncoder.SpikingInputEncodingRegime spikesEncodingRegime)
        {
            //Create StateMachine configuration
            //Simplified input configuration and homogenous excitability
            InputEncoderSettings inputCfg;
            HomogenousExcitabilitySettings homogenousExcitability;
            switch (spikesEncodingRegime)
            {
                /*
                 * Horizontal spikes encoding means that every spike position in the spike-train has related its own input neuron.
                 * So all the spikes are encoded at once, during one reservoir computation cycle.
                 */
                case InputEncoder.SpikingInputEncodingRegime.Horizontal:
                    inputCfg = StateMachineDesigner.CreateInputCfg(new FeedingPatternedSettings(1, NeuralPreprocessor.BidirProcessing.Continuous, RCNet.Neural.Data.InputPattern.VariablesSchema.Groupped, new UnificationSettings(false, false)),
                                                                   //136 spiking input neurons per input field - coding at once
                                                                   new InputSpikesCoderSettings(InputEncoder.SpikingInputEncodingRegime.Horizontal,
                                                                                                new A2SCoderSignalStrengthSettings(8), //8 neurons (spike-train length = 1)
                                                                                                new A2SCoderUpDirArrowsSettings(8, 8), //64 neurons (spike-train length = 1)
                                                                                                new A2SCoderDownDirArrowsSettings(8, 8) //64 neurons (spike-train length = 1)
                                                                                                ),
                                                                   true, //Route the input pattern as the predictors to a readout layer
                                                                   new ExternalFieldSettings("coord_abcissa", new RealFeatureFilterSettings(), true),
                                                                   new ExternalFieldSettings("coord_ordinate", new RealFeatureFilterSettings(), true)
                                                                   );
                    homogenousExcitability = new HomogenousExcitabilitySettings(1d, 0.7d, 0.2d); 
                    break;
                /*
                 * Vertical spikes encoding means that every coder generating spike-train has related its own input neuron.
                 * So all the spikes are encoded in several reservoir computation cycles, depending on largest coder's code (number of code time-points).
                 */
                case InputEncoder.SpikingInputEncodingRegime.Vertical:
                    inputCfg = StateMachineDesigner.CreateInputCfg(new FeedingPatternedSettings(1, NeuralPreprocessor.BidirProcessing.Continuous, RCNet.Neural.Data.InputPattern.VariablesSchema.Groupped),
                                                                   //17 spiking input neurons per input field- coding in 10 cycles
                                                                   new InputSpikesCoderSettings(InputEncoder.SpikingInputEncodingRegime.Vertical,
                                                                                                new A2SCoderSignalStrengthSettings(10), //1 neuron (spike-train length = 10)
                                                                                                new A2SCoderUpDirArrowsSettings(8, 10), //8 neurons (spike-train length = 10)
                                                                                                new A2SCoderDownDirArrowsSettings(8, 10) //8 neurons (spike-train length = 10)
                                                                                                ),
                                                                   true, //Route the input pattern as the predictors to a readout layer
                                                                   new ExternalFieldSettings("coord_abcissa", new RealFeatureFilterSettings(), true),
                                                                   new ExternalFieldSettings("coord_ordinate", new RealFeatureFilterSettings(), true)
                                                                   );
                    homogenousExcitability = new HomogenousExcitabilitySettings(1d, 0.7d, 0.2d);
                    break;
                /*
                 * Forbidden spikes encoding means no input spikes. Analog values from input fields are directly routed through synapses to hidden neurons.
                 * So all the input values are encoded at once, during one reservoir computation cycle.
                 */
                default:
                    //1 analog input neuron per input field
                    inputCfg = StateMachineDesigner.CreateInputCfg(new FeedingPatternedSettings(1, NeuralPreprocessor.BidirProcessing.Continuous, RCNet.Neural.Data.InputPattern.VariablesSchema.Groupped, new UnificationSettings(false, false)),
                                                                   new InputSpikesCoderSettings(InputEncoder.SpikingInputEncodingRegime.Forbidden),
                                                                   true, //Route the input pattern as the predictors to a readout layer
                                                                   new ExternalFieldSettings("coord_abcissa", new RealFeatureFilterSettings(), true),
                                                                   new ExternalFieldSettings("coord_ordinate", new RealFeatureFilterSettings(), true)
                                                                   );
                    homogenousExcitability = new HomogenousExcitabilitySettings(0.25, 0.7, 0.2d);
                    break;
            }

            //Simplified readout layer configuration
            ReadoutLayerSettings readoutCfg = StateMachineDesigner.CreateClassificationReadoutCfg(new CrossvalidationSettings(0.0825d, 0, 1),
                                                                                                  StateMachineDesigner.CreateMultiLayerRegrNet(10, new AFAnalogLeakyReLUSettings(), 2, 5, 400),
                                                                                                  "Hand movement",
                                                                                                  null,
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
            StateMachineSettings stateMachineCfg = smd.CreatePureLSMCfg(272, //Total size of the reservoir
                                                                        new AFSpikingAdExpIFSettings(), //Activation
                                                                        homogenousExcitability, //Homogenous excitability
                                                                        1, //Input connection density
                                                                        0, //Input max delay
                                                                        0.1d, //Interconnection density
                                                                        0, //Internal synapses max delay
                                                                        0, //Steady bias
                                                                        new PredictorsProviderSettings(new PredictorFiringTraceSettings(0.0005, 0))
                                                                        );

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

    }//LibrasClassificationLSMDesigner

}//Namespace
