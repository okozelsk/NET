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
            /// "Identity" activation function.
            /// </summary>
            Identity,
            /// <summary>"Sinusoid" activation function</summary>
            Sinusoid,
            /// <summary>"Elliot" activation function</summary>
            Elliot,
            /// <summary>"Tanh" activation function</summary>
            Tanh
        };

        /// <summary>Returns new instance of specified activation function</summary>
        /// <param name="type">Enumerated type of activation function</param>
        public static IActivationFunction CreateAF(ActivationType type, double p1 = double.NaN)
        {
            switch (type)
            {
                case ActivationType.Identity: return new IdentityAF();
                case ActivationType.Sinusoid: return new SinusoidAF();
                case ActivationType.Elliot: return new ElliotAF((double.IsNaN(p1) ? 1 : p1));
                case ActivationType.Tanh: return new TanhAF();
                default:
                    throw new ApplicationException("Unsupported activation function type: " + type.ToString());
            }
        }

        public static ActivationType ParseActivation(string code)
        {
            switch (code.ToUpper())
            {
                case "IDENTITY": return ActivationType.Identity;
                case "SINUSOID": return ActivationType.Sinusoid;
                case "ELLIOT": return ActivationType.Elliot;
                case "TANH": return ActivationType.Tanh;
                default:
                    throw new ApplicationException("Unsupported activation function code: " + code);
            }
        }

    }//ActivationFactory
}//Namespace
