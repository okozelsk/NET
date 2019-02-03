using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCNet.Neural
{
    /// <summary>
    /// Common interface of setting classes for non-recurrent trainers
    /// </summary>
    public interface INonRecurrentNetworkTrainerSettings
    {
        //Properties
        /// <summary>
        /// Maximum number of attempts
        /// </summary>
        int NumOfAttempts { get; }
        /// <summary>
        /// Maximum number of epochs within the one attempt
        /// </summary>
        int NumOfAttemptEpochs { get; }

        //Methods
        /// <summary>
        /// Creates a deep copy
        /// </summary>
        /// <returns></returns>
        INonRecurrentNetworkTrainerSettings DeepClone();

    }//INonRecurrentNetworkTrainerSettings

}//Namespace
