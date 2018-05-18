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
            /// Adaptive exponential integrate and fire activation function
            /// </summary>
            AdExpIF,
            /// <summary>
            /// Adaptive simple form of leaky integrate and fire
            /// </summary>
            AdSimpleIF,
            /// <summary>
            /// Analog "BentIdentity" activation function.
            /// </summary>
            BentIdentity,
            /// <summary>
            /// Analog "Elliot" (aka Softsign) activation function
            /// </summary>
            Elliot,
            /// <summary>
            /// Exponential integrate and fire activation function
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
            /// Leaky integrate and fire
            /// </summary>
            LeakyIF,
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
            /// Simple form of leaky integrate and fire
            /// </summary>
            SimpleIF,
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
        /// Returns the new instance of the activation function
        /// </summary>
        /// <param name="settings">
        /// Specific activation settings
        /// </param>
        public static IActivationFunction Create(ActivationSettings settings)
        {
            switch (settings.FunctionType)
            {
                case Function.AdExpIF:
                    return new AdExpIF(double.IsNaN(settings.Arg1) ? 5 : settings.Arg1, //membraneTimeScale (ms)
                                       double.IsNaN(settings.Arg2) ? 500 : settings.Arg2, //membraneResistance (MOhm)
                                       double.IsNaN(settings.Arg3) ? -70 : settings.Arg3, //restV (mV)
                                       double.IsNaN(settings.Arg4) ? -51 : settings.Arg4, //resetV (mV)
                                       double.IsNaN(settings.Arg5) ? -50 : settings.Arg5, //rheobaseV (mV)
                                       double.IsNaN(settings.Arg6) ? -30 : settings.Arg6, //firingThresholdV (mV)
                                       double.IsNaN(settings.Arg7) ? 2 : settings.Arg7, //sharpnessDeltaT (mV)
                                       double.IsNaN(settings.Arg8) ? 0.5 : settings.Arg8, //adaptationVoltageCoupling (nS)
                                       double.IsNaN(settings.Arg9) ? 100 : settings.Arg9, //adaptationTimeConstant (ms)
                                       double.IsNaN(settings.Arg10) ? 7 : settings.Arg10, //spikeTriggeredAdaptationIncrement (pA)
                                       double.IsNaN(settings.Arg11) ? 200 : settings.Arg11 //stimuliCoeff (pA)
                                       );
                case Function.AdSimpleIF:
                    return new AdSimpleIF(double.IsNaN(settings.Arg1) ? 15 : settings.Arg1, //membraneResistance (MOhm)
                                          double.IsNaN(settings.Arg2) ? 0.1 : settings.Arg2, //membrane decay rate
                                          double.IsNaN(settings.Arg3) ? 5 : settings.Arg3, //resetV (mV)
                                          double.IsNaN(settings.Arg4) ? 20 : settings.Arg4, //firingThresholdV (mV)
                                          double.IsNaN(settings.Arg5) ? 1 : settings.Arg5 //initial stimuli coeff
                                          );
                case Function.BentIdentity:
                    return new BentIdentity();
                case Function.Elliot:
                    return new Elliot((double.IsNaN(settings.Arg1) ? 1 : settings.Arg1));
                case Function.ExpIF:
                    return new ExpIF(double.IsNaN(settings.Arg1) ? 12 : settings.Arg1, //membraneTimeScale (ms)
                                     double.IsNaN(settings.Arg2) ? 20 : settings.Arg2, //membraneResistance (MOhm)
                                     double.IsNaN(settings.Arg3) ? -65 : settings.Arg3, //restV (mV)
                                     double.IsNaN(settings.Arg4) ? -60 : settings.Arg4, //resetV (mV)
                                     double.IsNaN(settings.Arg5) ? -55 : settings.Arg5, //rheobaseV (mV)
                                     double.IsNaN(settings.Arg6) ? -30 : settings.Arg6, //firingThresholdV (mV)
                                     double.IsNaN(settings.Arg7) ? 2 : settings.Arg7, //sharpnessDeltaT (mV)
                                     double.IsNaN(settings.Arg8) ? 1 : settings.Arg8, //refractoryPeriods (ms)
                                     double.IsNaN(settings.Arg9) ? 5.5 : settings.Arg9 //stimuli coeff
                                     );
                case Function.Gaussian:
                    return new Gaussian();
                case Function.Identity:
                    return new Identity();
                case Function.ISRU:
                    return new ISRU((double.IsNaN(settings.Arg1) ? 1 : settings.Arg1));
                case Function.LeakyIF:
                    return new LeakyIF(double.IsNaN(settings.Arg1) ? 8 : settings.Arg1, //membraneTimeScale (ms)
                                       double.IsNaN(settings.Arg2) ? 10 : settings.Arg2, //membraneResistance (MOhm)
                                       double.IsNaN(settings.Arg3) ? -70 : settings.Arg3, //restV (mV)
                                       double.IsNaN(settings.Arg4) ? -65 : settings.Arg4, //resetV (mV)
                                       double.IsNaN(settings.Arg5) ? -50 : settings.Arg5, //firingThresholdV (mV)
                                       double.IsNaN(settings.Arg6) ? 1 : settings.Arg6, //refractoryPeriods (ms)
                                       double.IsNaN(settings.Arg7) ? 5.5 : settings.Arg7 //stimuli coeff
                                       );
                case Function.LeakyReLU:
                    return new LeakyReLU((double.IsNaN(settings.Arg1) ? 0.01 : settings.Arg1));
                case Function.Sigmoid:
                    return new Sigmoid();
                case Function.SimpleIF:
                    return new SimpleIF(double.IsNaN(settings.Arg1) ? 15 : settings.Arg1, //membraneResistance (MOhm)
                                        double.IsNaN(settings.Arg2) ? 0.05 : settings.Arg2, //membrane decay rate
                                        double.IsNaN(settings.Arg3) ? 5 : settings.Arg3, //resetV (mV)
                                        double.IsNaN(settings.Arg4) ? 20 : settings.Arg4, //firingThresholdV (mV)
                                        double.IsNaN(settings.Arg5) ? 0 : settings.Arg5, //refractoryPeriods (ms)
                                        double.IsNaN(settings.Arg6) ? 1 : settings.Arg6 //stimuli coeff
                                        );
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
                case "ADEXPIF": return Function.AdExpIF;
                case "ADSIMPLEIF": return Function.AdSimpleIF;
                case "BENTIDENTITY": return Function.BentIdentity;
                case "ELLIOT": return Function.Elliot;
                case "EXPIF": return Function.ExpIF;
                case "GAUSSIAN": return Function.Gaussian;
                case "IDENTITY": return Function.Identity;
                case "ISRU": return Function.ISRU;
                case "LEAKYIF": return Function.LeakyIF;
                case "LEAKYRELU": return Function.LeakyReLU;
                case "SIGMOID": return Function.Sigmoid;
                case "SIMPLEIF": return Function.SimpleIF;
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

