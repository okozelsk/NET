using System;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Class mediates operations with activation functions
    /// </summary>
    public static class ActivationFactory
    {
        //Constants
        /// <summary>
        /// Activation function output signal
        /// </summary>
        public enum FunctionOutputSignalType
        {
            /// <summary>
            /// Function fires spikes when firing condition is met
            /// </summary>
            Spike,
            /// <summary>
            /// Function has continuous analog output
            /// </summary>
            Analog
        };

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
                case "activationAdSimpleIF":
                    return new AdSimpleIFSettings(settingsElem);
                case "activationBentIdentity":
                    return new BentIdentitySettings(settingsElem);
                case "activationElliot":
                    return new ElliotSettings(settingsElem);
                case "activationExpIF":
                    return new ExpIFSettings(settingsElem);
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
        /// Returns the new instance of the activation function
        /// </summary>
        /// <param name="settings">
        /// Specific activation function settings
        /// </param>
        public static IActivationFunction Create(Object settings)
        {
            Type settingsType = settings.GetType();
            if(settingsType == typeof(AdExpIFSettings))
            {
                return new AdExpIF((AdExpIFSettings)settings);
            }
            else if(settingsType == typeof(AdSimpleIFSettings))
            {
                return new AdSimpleIF((AdSimpleIFSettings)settings);
            }
            else if (settingsType == typeof(BentIdentitySettings))
            {
                return new BentIdentity((BentIdentitySettings)settings);
            }
            else if (settingsType == typeof(ElliotSettings))
            {
                return new Elliot((ElliotSettings)settings);
            }
            else if (settingsType == typeof(ExpIFSettings))
            {
                return new ExpIF((ExpIFSettings)settings);
            }
            else if (settingsType == typeof(GaussianSettings))
            {
                return new Gaussian((GaussianSettings)settings);
            }
            else if (settingsType == typeof(IdentitySettings))
            {
                return new Identity((IdentitySettings)settings);
            }
            else if (settingsType == typeof(ISRUSettings))
            {
                return new ISRU((ISRUSettings)settings);
            }
            else if (settingsType == typeof(LeakyIFSettings))
            {
                return new LeakyIF((LeakyIFSettings)settings);
            }
            else if (settingsType == typeof(LeakyReLUSettings))
            {
                return new LeakyReLU((LeakyReLUSettings)settings);
            }
            else if (settingsType == typeof(LeakyReLUSettings))
            {
                return new LeakyReLU((LeakyReLUSettings)settings);
            }
            else if (settingsType == typeof(SigmoidSettings))
            {
                return new Sigmoid((SigmoidSettings)settings);
            }
            else if (settingsType == typeof(SimpleIFSettings))
            {
                return new SimpleIF((SimpleIFSettings)settings);
            }
            else if (settingsType == typeof(SincSettings))
            {
                return new Sinc((SincSettings)settings);
            }
            else if (settingsType == typeof(SinusoidSettings))
            {
                return new Sinusoid((SinusoidSettings)settings);
            }
            else if (settingsType == typeof(SoftExponentialSettings))
            {
                return new SoftExponential((SoftExponentialSettings)settings);
            }
            else if (settingsType == typeof(SoftPlusSettings))
            {
                return new SoftPlus((SoftPlusSettings)settings);
            }
            else if (settingsType == typeof(TanHSettings))
            {
                return new TanH((TanHSettings)settings);
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
        public static Object GetDeepClone(Object settings)
        {
            Type settingsType = settings.GetType();
            if (settingsType == typeof(AdExpIFSettings))
            {
                return ((AdExpIFSettings)settings).DeepClone();
            }
            else if (settingsType == typeof(AdSimpleIFSettings))
            {
                return ((AdSimpleIFSettings)settings).DeepClone();
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

