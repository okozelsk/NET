using System;
using System.Xml.Linq;
using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.MathTools.Differential;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Class mediates operations with activation functions and af configurations
    /// </summary>
    public static class ActivationFactory
    {
        //Constants
        /// <summary>
        /// Minimal initial voltage ratio of the spiking activation function
        /// </summary>
        private const double MinInitialVRatio = 0.05d;
        /// <summary>
        /// Maximal initial voltage ratio of the spiking activation function
        /// </summary>
        private const double MaxInitialVRatio = 0.95d;
        //Default values
        /// <summary>
        /// Default value of the refractory periods
        /// </summary>
        public const int DefaultRefractoryPeriods = 1;

        /// <summary>
        /// Default ODE numerical solver method
        /// </summary>
        public const ODENumSolver.Method DefaultSolverMethod = ODENumSolver.Method.Euler;

        /// <summary>
        /// Default ODE numerical solver computation steps
        /// </summary>
        public const int DefaultSolverCompSteps = 2;

        /// <summary>
        /// Default duration of the spiking neuron stimulation in ms.
        /// </summary>
        public const double DefaultStimuliDuration = 1;


        //Methods
        /// <summary>
        /// Loads configuration of the activation function from the given xml element
        /// </summary>
        /// <param name="settingsElem">XML element containing the configuration</param>
        public static IActivationSettings LoadSettings(XElement settingsElem)
        {
            switch (settingsElem.Name.LocalName)
            {
                case "activationAdExpIF":
                    return new AFSpikingAdExpIFSettings(settingsElem);
                case "activationSQNL":
                    return new AFAnalogSQNLSettings(settingsElem);
                case "activationBentIdentity":
                    return new AFAnalogBentIdentitySettings(settingsElem);
                case "activationElliot":
                    return new AFAnalogElliotSettings(settingsElem);
                case "activationExpIF":
                    return new AFSpikingExpIFSettings(settingsElem);
                case "activationIzhikevichIF":
                    return new AFSpikingIzhikevichIFSettings(settingsElem);
                case "activationAutoIzhikevichIF":
                    return new AFSpikingAutoIzhikevichIFSettings(settingsElem);
                case "activationGaussian":
                    return new AFAnalogGaussianSettings(settingsElem);
                case "activationIdentity":
                    return new AFAnalogIdentitySettings(settingsElem);
                case "activationISRU":
                    return new AFAnalogISRUSettings(settingsElem);
                case "activationLeakyIF":
                    return new AFSpikingLeakyIFSettings(settingsElem);
                case "activationLeakyReLU":
                    return new AFAnalogLeakyReLUSettings(settingsElem);
                case "activationSigmoid":
                    return new AFAnalogSigmoidSettings(settingsElem);
                case "activationSimpleIF":
                    return new AFSpikingSimpleIFSettings(settingsElem);
                case "activationSinc":
                    return new AFAnalogSincSettings(settingsElem);
                case "activationSinusoid":
                    return new AFAnalogSinusoidSettings(settingsElem);
                case "activationSoftExponential":
                    return new AFAnalogSoftExponentialSettings(settingsElem);
                case "activationSoftMax":
                    return new AFAnalogSoftMaxSettings(settingsElem);
                case "activationSoftPlus":
                    return new AFAnalogSoftPlusSettings(settingsElem);
                case "activationTanH":
                    return new AFAnalogTanHSettings(settingsElem);
                default:
                    throw new ArgumentException($"Unsupported activation function settings: {settingsElem.Name}", "settingsElem");
            }
        }

        /// <summary>
        /// Creates an instance of the activation function according to given configuration.
        /// </summary>
        /// <param name="settings">Configuration of the activation function</param>
        /// <param name="rand">Random object to be used for randomly generated parameters</param>
        public static IActivation CreateAF(IActivationSettings settings, Random rand)
        {
            IActivation af;
            Type settingsType = settings.GetType();
            if (settingsType == typeof(AFSpikingAdExpIFSettings))
            {
                AFSpikingAdExpIFSettings afs = (AFSpikingAdExpIFSettings)settings;
                af = new AFSpikingAdExpIF(rand.NextDouble(afs.TimeScale),
                                          rand.NextDouble(afs.Resistance),
                                          rand.NextDouble(afs.RestV),
                                          rand.NextDouble(afs.ResetV),
                                          rand.NextDouble(afs.RheobaseV),
                                          rand.NextDouble(afs.FiringThresholdV),
                                          rand.NextDouble(afs.SharpnessDeltaT),
                                          rand.NextDouble(afs.AdaptationVoltageCoupling),
                                          rand.NextDouble(afs.AdaptationTimeConstant),
                                          rand.NextDouble(afs.AdaptationSpikeTriggeredIncrement),
                                          afs.SolverMethod,
                                          afs.SolverCompSteps,
                                          afs.StimuliDuration,
                                          rand.NextRangedUniformDouble(MinInitialVRatio, MaxInitialVRatio)
                                          );
            }
            else if (settingsType == typeof(AFAnalogBentIdentitySettings))
            {
                af = new AFAnalogBentIdentity();
            }
            else if (settingsType == typeof(AFAnalogElliotSettings))
            {
                AFAnalogElliotSettings afs = (AFAnalogElliotSettings)settings;
                af = new AFAnalogElliot(rand.NextDouble(afs.Slope));
            }
            else if (settingsType == typeof(AFSpikingExpIFSettings))
            {
                AFSpikingExpIFSettings afs = (AFSpikingExpIFSettings)settings;
                af = new AFSpikingExpIF(rand.NextDouble(afs.TimeScale),
                                        rand.NextDouble(afs.Resistance),
                                        rand.NextDouble(afs.RestV),
                                        rand.NextDouble(afs.ResetV),
                                        rand.NextDouble(afs.RheobaseV),
                                        rand.NextDouble(afs.FiringThresholdV),
                                        rand.NextDouble(afs.SharpnessDeltaT),
                                        afs.RefractoryPeriods,
                                        afs.SolverMethod,
                                        afs.SolverCompSteps,
                                        afs.StimuliDuration,
                                        rand.NextRangedUniformDouble(MinInitialVRatio, MaxInitialVRatio)
                                        );
            }
            else if (settingsType == typeof(AFAnalogGaussianSettings))
            {
                af = new AFAnalogGaussian();
            }
            else if (settingsType == typeof(AFAnalogIdentitySettings))
            {
                af = new AFAnalogIdentity();
            }
            else if (settingsType == typeof(AFAnalogISRUSettings))
            {
                AFAnalogISRUSettings afs = (AFAnalogISRUSettings)settings;
                af = new AFAnalogISRU(rand.NextDouble(afs.Alpha));
            }
            else if (settingsType == typeof(AFSpikingIzhikevichIFSettings))
            {
                AFSpikingIzhikevichIFSettings afs = (AFSpikingIzhikevichIFSettings)settings;
                af = new AFSpikingIzhikevichIF(rand.NextDouble(afs.RecoveryTimeScale),
                                               rand.NextDouble(afs.RecoverySensitivity),
                                               rand.NextDouble(afs.RecoveryReset),
                                               rand.NextDouble(afs.RestV),
                                               rand.NextDouble(afs.ResetV),
                                               rand.NextDouble(afs.FiringThresholdV),
                                               afs.RefractoryPeriods,
                                               afs.SolverMethod,
                                               afs.SolverCompSteps,
                                               afs.StimuliDuration,
                                               rand.NextRangedUniformDouble(MinInitialVRatio, MaxInitialVRatio)
                                               );

            }
            else if (settingsType == typeof(AFSpikingAutoIzhikevichIFSettings))
            {
                double randomValue = rand.NextDouble().Power(2);
                AFSpikingAutoIzhikevichIFSettings afs = (AFSpikingAutoIzhikevichIFSettings)settings;
                //Ranges
                af = new AFSpikingIzhikevichIF(0.02,
                                               0.2,
                                               8 + (-6 * randomValue),
                                               -70,
                                               -65 + (15 * randomValue),
                                               30,
                                               afs.RefractoryPeriods,
                                               afs.SolverMethod,
                                               afs.SolverCompSteps,
                                               afs.StimuliDuration,
                                               rand.NextRangedUniformDouble(MinInitialVRatio, MaxInitialVRatio)
                                               );
            }
            else if (settingsType == typeof(AFSpikingLeakyIFSettings))
            {
                AFSpikingLeakyIFSettings afs = (AFSpikingLeakyIFSettings)settings;
                af = new AFSpikingLeakyIF(rand.NextDouble(afs.TimeScale),
                                          rand.NextDouble(afs.Resistance),
                                          rand.NextDouble(afs.RestV),
                                          rand.NextDouble(afs.ResetV),
                                          rand.NextDouble(afs.FiringThresholdV),
                                          afs.RefractoryPeriods,
                                          afs.SolverMethod,
                                          afs.SolverCompSteps,
                                          afs.StimuliDuration,
                                          rand.NextRangedUniformDouble(MinInitialVRatio, MaxInitialVRatio)
                                          );
            }
            else if (settingsType == typeof(AFAnalogLeakyReLUSettings))
            {
                AFAnalogLeakyReLUSettings afs = (AFAnalogLeakyReLUSettings)settings;
                af = new AFAnalogLeakyReLU(rand.NextDouble(afs.NegSlope));
            }
            else if (settingsType == typeof(AFAnalogSigmoidSettings))
            {
                af = new AFAnalogSigmoid();
            }
            else if (settingsType == typeof(AFSpikingSimpleIFSettings))
            {
                AFSpikingSimpleIFSettings afs = (AFSpikingSimpleIFSettings)settings;
                af = new AFSpikingSimpleIF(rand.NextDouble(afs.Resistance),
                                           rand.NextDouble(afs.DecayRate),
                                           rand.NextDouble(afs.ResetV),
                                           rand.NextDouble(afs.FiringThresholdV),
                                           afs.RefractoryPeriods,
                                           rand.NextRangedUniformDouble(MinInitialVRatio, MaxInitialVRatio)
                                           );
            }
            else if (settingsType == typeof(AFAnalogSincSettings))
            {
                af = new AFAnalogSinc();
            }
            else if (settingsType == typeof(AFAnalogSinusoidSettings))
            {
                af = new AFAnalogSinusoid();
            }
            else if (settingsType == typeof(AFAnalogSoftExponentialSettings))
            {
                AFAnalogSoftExponentialSettings afs = (AFAnalogSoftExponentialSettings)settings;
                af = new AFAnalogSoftExponential(rand.NextDouble(afs.Alpha));
            }
            else if (settingsType == typeof(AFAnalogSoftMaxSettings))
            {
                af = new AFAnalogSoftMax();
            }
            else if (settingsType == typeof(AFAnalogSoftPlusSettings))
            {
                af = new AFAnalogSoftPlus();
            }
            else if (settingsType == typeof(AFAnalogSQNLSettings))
            {
                af = new AFAnalogSQNL();
            }
            else if (settingsType == typeof(AFAnalogTanHSettings))
            {
                af = new AFAnalogTanH();
            }
            else
            {
                throw new ArgumentException($"Unsupported activation function configuration: {settingsType.Name}");
            }
            return af;
        }

    }//ActivationFactory

}//Namespace

