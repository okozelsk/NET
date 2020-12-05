using RCNet.MathTools;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Common interface of all activation functions
    /// </summary>
    public interface IActivation
    {
        //Properties
        /// <inheritdoc cref="ActivationType"/>
        ActivationType TypeOfActivation { get; }

        /// <summary>
        /// Output range of the activation function
        /// </summary>
        Interval OutputRange { get; }

        /// <summary>
        /// Computes the activation
        /// </summary>
        /// <param name="x">Activation input</param>
        /// <returns>Computed activation</returns>
        double Compute(double x);


    }//IActivation

}//Namespace
