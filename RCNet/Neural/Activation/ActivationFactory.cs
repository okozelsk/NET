﻿using RCNet.Extensions;
using RCNet.MathTools.Differential;
using System;
using System.Xml.Linq;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Mediates the operations with activation functions and their configurations.
    /// </summary>
    public static class ActivationFactory
    {
        //Constants
        /// <summary>
        /// The minimal initial voltage ratio of the spiking activation function.
        /// </summary>
        private const double MinInitialVRatio = 0.05d;
        /// <summary>
        /// The maximal initial voltage ratio of the spiking activation function.
        /// </summary>
        private const double MaxInitialVRatio = 0.95d;
        //Default values
        /// <summary>
        /// The default number of refractory periods of the spiking activation function.
        /// </summary>
        public const int DefaultRefractoryPeriods = 1;

        /// <summary>
        /// The default ODE numerical solving method.
        /// </summary>
        public const ODENumSolver.Method DefaultSolverMethod = ODENumSolver.Method.Euler;

        /// <summary>
        /// The default number of computation steps of the ODE numerical solver.
        /// </summary>
        public const int DefaultSolverCompSteps = 2;

        /// <summary>
        /// The default duration of the spiking neuron stimulation (in ms).
        /// </summary>
        public const double DefaultStimuliDuration = 1;


        //Methods
        /// <summary>
        /// Loads the configuration of the activation function.
        /// </summary>
        /// <param name="settingsElem">A xml element containing the configuration data.</param>
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
        /// Creates the instance of the activation function.
        /// </summary>
        /// <param name="cfg">The configuration.</param>
        /// <param name="rand">A random object to be used for randomly generated parameters.</param>
        public static IActivation CreateAF(IActivationSettings cfg, Random rand)
        {
            IActivation af;
            Type settingsType = cfg.GetType();
            if (settingsType == typeof(AFSpikingAdExpIFSettings))
            {
                AFSpikingAdExpIFSettings afs = (AFSpikingAdExpIFSettings)cfg;
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
                AFAnalogElliotSettings afs = (AFAnalogElliotSettings)cfg;
                af = new AFAnalogElliot(rand.NextDouble(afs.Slope));
            }
            else if (settingsType == typeof(AFSpikingExpIFSettings))
            {
                AFSpikingExpIFSettings afs = (AFSpikingExpIFSettings)cfg;
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
                AFAnalogISRUSettings afs = (AFAnalogISRUSettings)cfg;
                af = new AFAnalogISRU(rand.NextDouble(afs.Alpha));
            }
            else if (settingsType == typeof(AFSpikingIzhikevichIFSettings))
            {
                AFSpikingIzhikevichIFSettings afs = (AFSpikingIzhikevichIFSettings)cfg;
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
                AFSpikingAutoIzhikevichIFSettings afs = (AFSpikingAutoIzhikevichIFSettings)cfg;
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
                AFSpikingLeakyIFSettings afs = (AFSpikingLeakyIFSettings)cfg;
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
                AFAnalogLeakyReLUSettings afs = (AFAnalogLeakyReLUSettings)cfg;
                af = new AFAnalogLeakyReLU(rand.NextDouble(afs.NegSlope));
            }
            else if (settingsType == typeof(AFAnalogSigmoidSettings))
            {
                af = new AFAnalogSigmoid();
            }
            else if (settingsType == typeof(AFSpikingSimpleIFSettings))
            {
                AFSpikingSimpleIFSettings afs = (AFSpikingSimpleIFSettings)cfg;
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
                AFAnalogSoftExponentialSettings afs = (AFAnalogSoftExponentialSettings)cfg;
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

