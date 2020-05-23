namespace Demo.DemoConsoleApp.Log
{
    /// <summary>
    /// Interface for a simple output journal
    /// </summary>
    public interface IOutputLog
    {
        /// <summary>
        /// Writes the given message to output.
        /// </summary>
        /// <param name="message">The message to be written to output</param>
        /// <param name="replaceLastMessage">Indicates whether to replace a text of the last message by the new one.</param>
        void Write(string message, bool replaceLastMessage = false);

    }//IOutputLog

}//Namespace
