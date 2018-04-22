using System;

namespace RCNet.Neural.Analog.Activation
{
    /// <summary>
    /// Class mediates operations with activation functions
    /// </summary>
    public static class AnalogActivationFactory
    {
        //Constants
        /// <summary>
        /// Supported types of neuron activation function
        /// </summary>
        public enum FunctionType
        {
            /// <summary>
            /// "BentIdentity" activation function.
            /// </summary>
            BentIdentity,
            /// <summary>
            /// "Elliot" (aka Softsign) activation function
            /// </summary>
            Elliot,
            /// <summary>
            /// "Gaussian" activation function
            /// </summary>
            Gaussian,
            /// <summary>
            /// "Identity" (aka Linear) activation function.
            /// </summary>
            Identity,
            /// <summary>
            /// "ISRU" (Inverse Square Root Unit) activation function
            /// </summary>
            ISRU,
            /// <summary>
            /// "Leaky ReLU" activation function.
            /// Pure ReLU is a specific case of Leaky ReLU having negSlope = 0.
            /// Leaky ReLU having negSlope = 1 is the same as Identity.
            /// </summary>
            LeakyReLU,
            /// <summary>
            /// "Sigmoid" activation function
            /// </summary>
            Sigmoid,
            /// <summary>
            /// "Sinc" activation function
            /// </summary>
            Sinc,
            /// <summary>
            /// "Sinusoid" activation function
            /// </summary>
            Sinusoid,
            /// <summary>
            /// "SoftExponential" activation function
            /// </summary>
            SoftExponential,
            /// <summary>
            /// "SoftPlus" activation function
            /// </summary>
            SoftPlus,
            /// <summary>
            /// "TanH" activation function
            /// </summary>
            TanH
        };

        /// <summary>
        /// Returns the new instance of the activation function
        /// </summary>
        /// <param name="settings">
        /// Specific activation settings
        /// </param>
        public static IAnalogActivationFunction Create(AnalogActivationSettings settings)
        {
            switch (settings.FunctionType)
            {
                case FunctionType.BentIdentity: return new BentIdentity();
                case FunctionType.Elliot: return new Elliot((double.IsNaN(settings.Arg1) ? 1 : settings.Arg1));
                case FunctionType.Gaussian: return new Gaussian();
                case FunctionType.Identity: return new Identity();
                case FunctionType.ISRU: return new ISRU((double.IsNaN(settings.Arg1) ? 1 : settings.Arg1));
                case FunctionType.LeakyReLU: return new LeakyReLU((double.IsNaN(settings.Arg1) ? 0.01 : settings.Arg1));
                case FunctionType.Sigmoid: return new Sigmoid();
                case FunctionType.Sinc: return new Sinc();
                case FunctionType.Sinusoid: return new Sinusoid();
                case FunctionType.SoftExponential: return new SoftExponential((double.IsNaN(settings.Arg1) ? 0 : settings.Arg1));
                case FunctionType.SoftPlus: return new SoftPlus();
                case FunctionType.TanH: return new TanH();
                default:
                    throw new ArgumentException($"Unsupported activation function type: {settings.FunctionType}");
            }
        }

        /// <summary>
        /// Parses given string code of the activation function type.
        /// </summary>
        /// <param name="code">A code of the activation function type.</param>
        /// <returns>Parsed activation function type.</returns>
        public static FunctionType ParseActivationFunctionType(string code)
        {
            switch (code.ToUpper())
            {
                case "BENTIDENTITY": return FunctionType.BentIdentity;
                case "ELLIOT": return FunctionType.Elliot;
                case "GAUSSIAN": return FunctionType.Gaussian;
                case "IDENTITY": return FunctionType.Identity;
                case "ISRU": return FunctionType.ISRU;
                case "LEAKYRELU": return FunctionType.LeakyReLU;
                case "SIGMOID": return FunctionType.Sigmoid;
                case "SINC": return FunctionType.Sinc;
                case "SINUSOID": return FunctionType.Sinusoid;
                case "SOFTEXPONENTIAL": return FunctionType.SoftExponential;
                case "SOFTPLUS": return FunctionType.SoftPlus;
                case "TANH": return FunctionType.TanH;
                default:
                    throw new ArgumentException($"Unsupported activation function code: {code}");
            }
        }

    }//AnalogActivationFactory

}//Namespace

