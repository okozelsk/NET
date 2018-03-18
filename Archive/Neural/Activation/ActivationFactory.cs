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
        /// <summary>Type of supported neuron activation function</summary>
        public enum EnumActivationType
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
        public static IActivationFunction CreateAF(EnumActivationType type, double p1 = double.NaN)
        {
            switch (type)
            {
                case EnumActivationType.Identity: return new IdentityAF();
                case EnumActivationType.Sinusoid: return new SinusoidAF();
                case EnumActivationType.Elliot: return new ElliotAF((double.IsNaN(p1) ? 1 : p1));
                case EnumActivationType.Tanh: return new TanhAF();
                default:
                    throw new ApplicationException("Unsupported activation function type: " + type.ToString());
            }
        }

        public static EnumActivationType ParseActivation(string code)
        {
            switch (code.ToUpper())
            {
                case "IDENTITY": return EnumActivationType.Identity;
                case "SINUSOID": return EnumActivationType.Sinusoid;
                case "ELLIOT": return EnumActivationType.Elliot;
                case "TANH": return EnumActivationType.Tanh;
                default:
                    throw new ApplicationException("Unsupported activation function code: " + code);
            }
        }

    }//ActivationFactory
}//Namespace
