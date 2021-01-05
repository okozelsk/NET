using RCNet.MathTools;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Common interface of activation functions.
    /// </summary>
    public interface IActivation
    {
        //Properties
        /// <inheritdoc cref="ActivationType"/>
        ActivationType TypeOfActivation { get; }

        /// <summary>
        /// The output range of an activation function.
        /// </summary>
        Interval OutputRange { get; }

        /// <summary>
        /// Computes the activation.
        /// </summary>
        /// <param name="x">An activation input.</param>
        /// <returns>The computed activation.</returns>
        double Compute(double x);


    }//IActivation

}//Namespace
