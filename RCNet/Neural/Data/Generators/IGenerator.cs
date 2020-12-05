namespace RCNet.Neural.Data.Generators
{
    /// <summary>
    /// Common interface for signal generators
    /// </summary>
    public interface IGenerator
    {
        /// <summary>
        /// Resets the generator to its initial state
        /// </summary>
        void Reset();

        /// <summary>
        /// Generates the next value
        /// </summary>
        double Next();

    }//IGenerator

}//Namespace
