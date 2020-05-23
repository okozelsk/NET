namespace RCNet.Neural.Data.Generators
{
    /// <summary>
    /// Common interface for signal generators
    /// </summary>
    public interface IGenerator
    {
        /// <summary>
        /// Resets generator to its initial state
        /// </summary>
        void Reset();

        /// <summary>
        /// Returns next signal value
        /// </summary>
        double Next();

    }//IGenerator

}//Namespace
