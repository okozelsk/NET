using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OKOSW.Demo.Log
{
    /// <summary>
    /// Simple output log interface
    /// </summary>
    public interface IOutputLog
    {
        /// <summary>
        /// Writes message to output.
        /// </summary>
        /// <param name="message">Message to be written to output</param>
        /// <param name="replaceLastMessage">Indicates if to replace last written message by this new message</param>
        void Write(string message, bool replaceLastMessage = false);
    }//IOutputLog
}//Namespace
