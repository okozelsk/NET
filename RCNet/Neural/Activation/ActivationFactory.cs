using System;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Xml.Linq;
using RCNet.Extensions;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Class mediates operations with activation functions
    /// </summary>
    public static class ActivationFactory
    {
        /// <summary>
        /// Returns the instance of the activation function settings
        /// </summary>
        /// <param name="settingsElem">
        /// XML element containing specific activation settings
        /// </param>
        public static Object LoadSettings(XElement settingsElem)
        {
            switch (settingsElem.Name.LocalName)
            {
                case "activationAdExpIF":
                    return new AdExpIFSettings(settingsElem);
                case "activationBentIdentity":
                    return new BentIdentitySettings(settingsElem);
                case "activationElliot":
                    return new ElliotSettings(settingsElem);
                case "activationExpIF":
                    return new ExpIFSettings(settingsElem);
                case "activationIzhikevichIF":
                    return new IzhikevichIFSettings(settingsElem);
                case "activationAutoIzhikevichIF":
                    return new AutoIzhikevichIFSettings(settingsElem);
                case "activationGaussian":
                    return new GaussianSettings(settingsElem);
                case "activationIdentity":
                    return new IdentitySettings(settingsElem);
                case "activationISRU":
                    return new ISRUSettings(settingsElem);
                case "activationLeakyIF":
                    return new LeakyIFSettings(settingsElem);
                case "activationLeakyReLU":
                    return new LeakyReLUSettings(settingsElem);
                case "activationSigmoid":
                    return new SigmoidSettings(settingsElem);
                case "activationSimpleIF":
                    return new SimpleIFSettings(settingsElem);
                case "activationSinc":
                    return new SincSettings(settingsElem);
                case "activationSinusoid":
                    return new SinusoidSettings(settingsElem);
                case "activationSoftExponential":
                    return new SoftExponentialSettings(settingsElem);
                case "activationSoftPlus":
                    return new SoftPlusSettings(settingsElem);
                case "activationTanH":
                    return new TanHSettings(settingsElem);
                default:
                    throw new ArgumentException($"Unsupported activation function settings: {settingsElem.Name}");
            }
        }

        /// <summary>
        /// Creates an instance of the activation function according to given settings.
        /// </summary>
        /// <param name="settings">Specific activation function settings </param>
        /// <param name="rand">Random object to be used for randomly generated parameters</param>
        public static IActivationFunction Create(Object settings, Random rand)
        {
            Type settingsType = settings.GetType();
            if (settingsType == typeof(AdExpIFSettings))
            {
                AdExpIFSettings afs = (AdExpIFSettings)settings;
                return new AdExpIF(afs.StimuliCoeff,
                                   rand.NextDouble(afs.TimeScale),
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
                                   afs.SolverCompSteps
                                  );
            }
            else if (settingsType == typeof(BentIdentitySettings))
            {
                return new BentIdentity();
            }
            else if (settingsType == typeof(ElliotSettings))
            {
                ElliotSettings afs = (ElliotSettings)settings;
                return new Elliot(rand.NextDouble(afs.Slope));
            }
            else if (settingsType == typeof(ExpIFSettings))
            {
                //return new ExpIF((ExpIFSettings)settings, rand);
                ExpIFSettings afs = (ExpIFSettings)settings;
                return new ExpIF(afs.StimuliCoeff,
                                 rand.NextDouble(afs.TimeScale),
                                 rand.NextDouble(afs.Resistance),
                                 rand.NextDouble(afs.RestV),
                                 rand.NextDouble(afs.ResetV),
                                 rand.NextDouble(afs.RheobaseV),
                                 rand.NextDouble(afs.FiringThresholdV),
                                 rand.NextDouble(afs.SharpnessDeltaT),
                                 afs.RefractoryPeriods,
                                 afs.SolverMethod,
                                 afs.SolverCompSteps
                                 );
            }
            else if (settingsType == typeof(GaussianSettings))
            {
                return new Gaussian();
            }
            else if (settingsType == typeof(IdentitySettings))
            {
                return new Identity();
            }
            else if (settingsType == typeof(ISRUSettings))
            {
                ISRUSettings afs = (ISRUSettings)settings;
                return new ISRU(rand.NextDouble(afs.Alpha));
            }
            else if (settingsType == typeof(IzhikevichIFSettings))
            {
                IzhikevichIFSettings afs = (IzhikevichIFSettings)settings;
                return new IzhikevichIF(afs.StimuliCoeff,
                                        rand.NextDouble(afs.RecoveryTimeScale),
                                        rand.NextDouble(afs.RecoverySensitivity),
                                        rand.NextDouble(afs.RecoveryReset),
                                        rand.NextDouble(afs.RestV),
                                        rand.NextDouble(afs.ResetV),
                                        rand.NextDouble(afs.FiringThresholdV),
                                        afs.RefractoryPeriods,
                                        afs.SolverMethod,
                                        afs.SolverCompSteps
                                        );

            }
            else if (settingsType == typeof(AutoIzhikevichIFSettings))
            {
                double randomValue = rand.NextBoundedUniformDouble(0, 1);
                AutoIzhikevichIFSettings afs = (AutoIzhikevichIFSettings)settings;
                if (afs.Role == CommonEnums.NeuronRole.Excitatory)
                {
                    //Excitatory ranges
                    return new IzhikevichIF(afs.StimuliCoeff,
                                            0.02,
                                            0.2,
                                            8 + (-6 * randomValue.Power(2)),
                                            -70,
                                            -65 + (15 * randomValue.Power(2)),
                                            30,
                                            afs.RefractoryPeriods,
                                            afs.SolverMethod,
                                            afs.SolverCompSteps
                                            );
                }
                else
                {
                    //Inhibitory ranges
                    return new IzhikevichIF(afs.StimuliCoeff,
                                            0.02 + 0.08 * randomValue,
                                            0.25 - 0.05 * randomValue,
                                            2,
                                            -70,
                                            -65,
                                            30,
                                            afs.RefractoryPeriods,
                                            afs.SolverMethod,
                                            afs.SolverCompSteps
                                            );
                }
            }
            else if (settingsType == typeof(LeakyIFSettings))
            {
                LeakyIFSettings afs = (LeakyIFSettings)settings;
                return new LeakyIF(afs.StimuliCoeff,
                                   rand.NextDouble(afs.TimeScale),
                                   rand.NextDouble(afs.Resistance),
                                   rand.NextDouble(afs.RestV),
                                   rand.NextDouble(afs.ResetV),
                                   rand.NextDouble(afs.FiringThresholdV),
                                   afs.RefractoryPeriods,
                                   afs.SolverMethod,
                                   afs.SolverCompSteps
                                   );
            }
            else if (settingsType == typeof(LeakyReLUSettings))
            {
                LeakyReLUSettings afs = (LeakyReLUSettings)settings;
                return new LeakyReLU(rand.NextDouble(afs.NegSlope));
            }
            else if (settingsType == typeof(SigmoidSettings))
            {
                return new Sigmoid();
            }
            else if (settingsType == typeof(SimpleIFSettings))
            {
                SimpleIFSettings afs = (SimpleIFSettings)settings;
                return new SimpleIF(afs.StimuliCoeff,
                                    rand.NextDouble(afs.Resistance),
                                    rand.NextDouble(afs.DecayRate),
                                    rand.NextDouble(afs.ResetV),
                                    rand.NextDouble(afs.FiringThresholdV),
                                    afs.RefractoryPeriods
                                    );
            }
            else if (settingsType == typeof(SincSettings))
            {
                return new Sinc();
            }
            else if (settingsType == typeof(SinusoidSettings))
            {
                return new Sinusoid();
            }
            else if (settingsType == typeof(SoftExponentialSettings))
            {
                SoftExponentialSettings afs = (SoftExponentialSettings)settings;
                return new SoftExponential(rand.NextDouble(afs.Alpha));
            }
            else if (settingsType == typeof(SoftPlusSettings))
            {
                return new SoftPlus();
            }
            else if (settingsType == typeof(TanHSettings))
            {
                return new TanH();
            }
            else
            {
                throw new ArgumentException($"Unsupported activation function settings: {settingsType.Name}");
            }
        }

        /// <summary>
        /// Returns the deep clone of the activation function settings
        /// </summary>
        /// <param name="settings">
        /// Specific activation function settings
        /// </param>
        public static Object DeepCloneActivationSettings(Object settings)
        {
            Type settingsType = settings.GetType();
            if (settingsType == typeof(AdExpIFSettings))
            {
                return ((AdExpIFSettings)settings).DeepClone();
            }
            else if (settingsType == typeof(BentIdentitySettings))
            {
                return ((BentIdentitySettings)settings).DeepClone();
            }
            else if (settingsType == typeof(ElliotSettings))
            {
                return ((ElliotSettings)settings).DeepClone();
            }
            else if (settingsType == typeof(ExpIFSettings))
            {
                return ((ExpIFSettings)settings).DeepClone();
            }
            else if (settingsType == typeof(GaussianSettings))
            {
                return ((GaussianSettings)settings).DeepClone();
            }
            else if (settingsType == typeof(IdentitySettings))
            {
                return ((IdentitySettings)settings).DeepClone();
            }
            else if (settingsType == typeof(ISRUSettings))
            {
                return ((ISRUSettings)settings).DeepClone();
            }
            else if (settingsType == typeof(IzhikevichIFSettings))
            {
                return ((IzhikevichIFSettings)settings).DeepClone();
            }
            else if (settingsType == typeof(AutoIzhikevichIFSettings))
            {
                return ((AutoIzhikevichIFSettings)settings).DeepClone();
            }
            else if (settingsType == typeof(LeakyIFSettings))
            {
                return ((LeakyIFSettings)settings).DeepClone();
            }
            else if (settingsType == typeof(LeakyReLUSettings))
            {
                return ((LeakyReLUSettings)settings).DeepClone();
            }
            else if (settingsType == typeof(LeakyReLUSettings))
            {
                return ((LeakyReLUSettings)settings).DeepClone();
            }
            else if (settingsType == typeof(SigmoidSettings))
            {
                return ((SigmoidSettings)settings).DeepClone();
            }
            else if (settingsType == typeof(SimpleIFSettings))
            {
                return ((SimpleIFSettings)settings).DeepClone();
            }
            else if (settingsType == typeof(SincSettings))
            {
                return ((SincSettings)settings).DeepClone();
            }
            else if (settingsType == typeof(SinusoidSettings))
            {
                return ((SinusoidSettings)settings).DeepClone();
            }
            else if (settingsType == typeof(SoftExponentialSettings))
            {
                return ((SoftExponentialSettings)settings).DeepClone();
            }
            else if (settingsType == typeof(SoftPlusSettings))
            {
                return ((SoftPlusSettings)settings).DeepClone();
            }
            else if (settingsType == typeof(TanHSettings))
            {
                return ((TanHSettings)settings).DeepClone();
            }
            else
            {
                throw new ArgumentException($"Unsupported activation function settings: {settingsType.Name}");
            }
        }

    }//ActivationFactory

}//Namespace

