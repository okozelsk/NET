using System;

namespace RCNet.Demo.Log
{
    /// <summary>
    /// A simple output log interface
    /// </summary>
    public interface IOutputLog
    {
        /// <summary>
        /// Writes the given message to output.
        /// </summary>
        /// <param name="message">The message to be written to output</param>
        /// <param name="replaceLastMessage">Indicates if to replace a text of the last message by the new one.</param>
        void Write(string message, bool replaceLastMessage = false);

    }//IOutputLog

}//Namespace
