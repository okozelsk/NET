using System;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Class mediates operations with activation functions
    /// </summary>
    public static class ActivationFactory
    {
        //Constants
        /// <summary>
        /// Supported types of neuron activation function
        /// </summary>
        public enum Function
        {
            /// <summary>
            /// Analog "BentIdentity" activation function.
            /// </summary>
            BentIdentity,
            /// <summary>
            /// Analog "Elliot" (aka Softsign) activation function
            /// </summary>
            Elliot,
            /// <summary>
            /// Analog "Gaussian" activation function
            /// </summary>
            Gaussian,
            /// <summary>
            /// Analog "Identity" (aka Linear) activation function.
            /// </summary>
            Identity,
            /// <summary>
            /// Analog "ISRU" (Inverse Square Root Unit) activation function
            /// </summary>
            ISRU,
            /// <summary>
            /// Analog "Leaky ReLU" activation function.
            /// ReLU function is a specific case of this LeakyReLU having negSlope = 0.
            /// Identity function is a specific case of this LeakyReLU having negSlope = 1.
            /// </summary>
            LeakyReLU,
            /// <summary>
            /// Leaky integrate and fire
            /// </summary>
            LIF,
            /// <summary>
            /// Analog "Sigmoid" activation function
            /// </summary>
            Sigmoid,
            /// <summary>
            /// Analog "Sinc" activation function
            /// </summary>
            Sinc,
            /// <summary>
            /// Analog "Sinusoid" activation function
            /// </summary>
            Sinusoid,
            /// <summary>
            /// Analog "SoftExponential" activation function
            /// </summary>
            SoftExponential,
            /// <summary>
            /// Analog "SoftPlus" activation function
            /// </summary>
            SoftPlus,
            /// <summary>
            /// Analog "TanH" activation function
            /// </summary>
            TanH
        };

        /// <summary>
        /// Activation function output
        /// </summary>
        public enum FunctionOutputType
        {
            /// <summary>
            /// Function spikes
            /// </summary>
            Spike,
            /// <summary>
            /// Function has analog output
            /// </summary>
            Analog
        };

        /// <summary>
        /// Returns the new instance of the activation function
        /// </summary>
        /// <param name="settings">
        /// Specific activation settings
        /// </param>
        /// <param name="rand">
        /// Random generator
        /// </param>
        public static IActivationFunction Create(ActivationSettings settings, Random rand = null)
        {
            switch (settings.FunctionType)
            {
                case Function.BentIdentity:
                    return new BentIdentity();
                case Function.Elliot:
                    return new Elliot((double.IsNaN(settings.Arg1) ? 1 : settings.Arg1));
                case Function.Gaussian:
                    return new Gaussian();
                case Function.Identity:
                    return new Identity();
                case Function.ISRU:
                    return new ISRU((double.IsNaN(settings.Arg1) ? 1 : settings.Arg1));
                case Function.LeakyReLU:
                    return new LeakyReLU((double.IsNaN(settings.Arg1) ? 0.01 : settings.Arg1));
                case Function.LIF:
                    return new LIF(double.IsNaN(settings.Arg1) ? 15 : settings.Arg1, //membraneResistance (MOhm)
                                   double.IsNaN(settings.Arg2) ? 0.05 : settings.Arg2, //membrane decay rate
                                   double.IsNaN(settings.Arg3) ? 5 : settings.Arg3, //resetV (mV)
                                   double.IsNaN(settings.Arg4) ? 20 : settings.Arg4, //firingTresholdV (mV)
                                   double.IsNaN(settings.Arg5) ? 0 : settings.Arg5 //refractoryPeriods (ms)
                                   );
                case Function.Sigmoid:
                    return new Sigmoid();
                case Function.Sinc:
                    return new Sinc();
                case Function.Sinusoid:
                    return new Sinusoid();
                case Function.SoftExponential:
                    return new SoftExponential((double.IsNaN(settings.Arg1) ? 0 : settings.Arg1));
                case Function.SoftPlus:
                    return new SoftPlus();
                case Function.TanH:
                    return new TanH();
                default:
                    throw new ArgumentException($"Unsupported activation function type: {settings.FunctionType}");
            }
        }

        /// <summary>
        /// Parses given string code of the activation function.
        /// </summary>
        /// <param name="code">A code of the activation function.</param>
        public static Function ParseActivationFunctionType(string code)
        {
            switch (code.ToUpper())
            {
                case "BENTIDENTITY": return Function.BentIdentity;
                case "ELLIOT": return Function.Elliot;
                case "GAUSSIAN": return Function.Gaussian;
                case "IDENTITY": return Function.Identity;
                case "ISRU": return Function.ISRU;
                case "LEAKYRELU": return Function.LeakyReLU;
                case "LIF": return Function.LIF;
                case "SIGMOID": return Function.Sigmoid;
                case "SINC": return Function.Sinc;
                case "SINUSOID": return Function.Sinusoid;
                case "SOFTEXPONENTIAL": return Function.SoftExponential;
                case "SOFTPLUS": return Function.SoftPlus;
                case "TANH": return Function.TanH;
                default:
                    throw new ArgumentException($"Unsupported activation function code: {code}");
            }
        }

    }//ActivationFactory

}//Namespace

