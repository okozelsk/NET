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
            /// Bidirectional Exponential Integrate and Fire
            /// </summary>
            BiExpIF,
            /// <summary>
            /// Bidirectional Leaky Integrate and Fire
            /// </summary>
            BiLIF,
            /// <summary>
            /// Analog "Elliot" (aka Softsign) activation function
            /// </summary>
            Elliot,
            /// <summary>
            /// Exponential Integrate and Fire
            /// </summary>
            ExpIF,
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
            /// Leaky Integrate and Fire
            /// </summary>
            LIF,
            /// <summary>
            /// Analog "Leaky ReLU" activation function.
            /// ReLU function is a specific case of this LeakyReLU having negSlope = 0.
            /// Identity function is a specific case of this LeakyReLU having negSlope = 1.
            /// </summary>
            LeakyReLU,
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
        public static IActivationFunction Create(ActivationSettings settings)
        {
            switch (settings.FunctionType)
            {
                case Function.BentIdentity:
                    return new BentIdentity();
                case Function.BiExpIF:
                    return new BiExpIF(double.IsNaN(settings.Arg1) ? 12 : settings.Arg1,
                                       double.IsNaN(settings.Arg2) ? 20 : settings.Arg2,
                                       double.IsNaN(settings.Arg4) ? 5 : settings.Arg4,
                                       double.IsNaN(settings.Arg5) ? 10 : settings.Arg5,
                                       double.IsNaN(settings.Arg6) ? 35 : settings.Arg6,
                                       double.IsNaN(settings.Arg7) ? 2 : settings.Arg7,
                                       double.IsNaN(settings.Arg8) ? 1 : settings.Arg8
                                       );
                case Function.BiLIF:
                    return new BiLIF(double.IsNaN(settings.Arg1) ? 8 : settings.Arg1,
                                     double.IsNaN(settings.Arg2) ? 10 : settings.Arg2,
                                     double.IsNaN(settings.Arg4) ? 5 : settings.Arg4,
                                     double.IsNaN(settings.Arg5) ? 20 : settings.Arg5,
                                     double.IsNaN(settings.Arg6) ? 2 : settings.Arg6
                                     );
                case Function.Elliot:
                    return new Elliot((double.IsNaN(settings.Arg1) ? 1 : settings.Arg1));
                case Function.ExpIF:
                    return new ExpIF(double.IsNaN(settings.Arg1) ? 12 : settings.Arg1,
                                     double.IsNaN(settings.Arg2) ? 20 : settings.Arg2,
                                     double.IsNaN(settings.Arg3) ? -65 : settings.Arg3,
                                     double.IsNaN(settings.Arg4) ? -60 : settings.Arg4,
                                     double.IsNaN(settings.Arg5) ? -55 : settings.Arg5,
                                     double.IsNaN(settings.Arg6) ? -30 : settings.Arg6,
                                     double.IsNaN(settings.Arg7) ? 2 : settings.Arg7,
                                     double.IsNaN(settings.Arg8) ? 1 : settings.Arg8
                                     );
                case Function.Gaussian:
                    return new Gaussian();
                case Function.Identity:
                    return new Identity();
                case Function.ISRU:
                    return new ISRU((double.IsNaN(settings.Arg1) ? 1 : settings.Arg1));
                case Function.LeakyReLU:
                    return new LeakyReLU((double.IsNaN(settings.Arg1) ? 0.01 : settings.Arg1));
                case Function.LIF:
                    return new LIF(double.IsNaN(settings.Arg1) ? 8 : settings.Arg1,
                                     double.IsNaN(settings.Arg2) ? 10 : settings.Arg2,
                                     double.IsNaN(settings.Arg3) ? -70 : settings.Arg3,
                                     double.IsNaN(settings.Arg4) ? -65 : settings.Arg4,
                                     double.IsNaN(settings.Arg5) ? -50 : settings.Arg5,
                                     double.IsNaN(settings.Arg6) ? 2 : settings.Arg6
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
                case "BIEXPIF": return Function.BiExpIF;
                case "BILIF": return Function.BiLIF;
                case "ELLIOT": return Function.Elliot;
                case "EXPIF": return Function.ExpIF;
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

