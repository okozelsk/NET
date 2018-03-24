using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OKOSW.Neural.Activation
{
    public static class ActivationFactory
    {
        //Constants
        /// <summary>Supported types of neuron activation function</summary>
        public enum ActivationType
        {
            /// <summary>
            /// "Identity" (aka Linear) activation function.
            /// </summary>
            Identity,
            /// <summary>"Sinusoid" activation function</summary>
            Sinusoid,
            /// <summary>"Elliot" (aka Softsign) activation function</summary>
            Elliot,
            /// <summary>"TanH" activation function</summary>
            TanH,
            /// <summary>"Inverse Square Root Unit" activation function</summary>
            ISRU,
            /// <summary>"Sigmoid" (aka Logistic, Softstep) activation function</summary>
            Sigmoid,
            /// <summary>"Gaussian" activation function</summary>
            Gaussian
        };

        /// <summary>Returns new instance of specified activation function</summary>
        /// <param name="type">Enumerated type of activation function</param>
        public static IActivationFunction CreateActivationFunction(ActivationType type, double p1 = double.NaN, double p2 = double.NaN)
        {
            switch (type)
            {
                case ActivationType.Identity: return new IdentityAF();
                case ActivationType.Sinusoid: return new SinusoidAF();
                case ActivationType.Elliot: return new ElliotAF((double.IsNaN(p1) ? 1 : p1));
                case ActivationType.TanH: return new TanhAF();
                case ActivationType.ISRU: return new InverseSquareRootUnitAF((double.IsNaN(p1) ? 1 : p1));
                case ActivationType.Sigmoid: return new SigmoidAF();
                case ActivationType.Gaussian: return new GaussianAF();
                default:
                    throw new ArgumentException("Unsupported activation function type: " + type.ToString());
            }
        }

        public static ActivationType ParseActivation(string code)
        {
            switch (code.ToUpper())
            {
                case "IDENTITY": return ActivationType.Identity;
                case "SINUSOID": return ActivationType.Sinusoid;
                case "ELLIOT": return ActivationType.Elliot;
                case "TANH": return ActivationType.TanH;
                case "ISRU": return ActivationType.ISRU;
                case "SIGMOID": return ActivationType.Sigmoid;
                case "GAUSSIAN": return ActivationType.Gaussian;
                default:
                    throw new ArgumentException("Unsupported activation function code: " + code);
            }
        }

    }//ActivationFactory
}//Namespace
