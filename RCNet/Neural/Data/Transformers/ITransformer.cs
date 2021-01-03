namespace RCNet.Neural.Data.Transformers
{
    /// <summary>
    /// The common interface of all input data transformers.
    /// </summary>
    public interface ITransformer
    {
        /// <summary>
        /// Resets the transformer to its initial state.
        /// </summary>
        void Reset();

        /// <summary>
        /// Computes the transformed value.
        /// </summary>
        /// <param name="data">The collection of already known input fields values.</param>
        double Transform(double[] data);

    }//ITransformer

}//Namespace
