using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCNet.RandomValue
{
    /// <summary>
    /// Technical interface for random distributions
    /// </summary>
    public interface IDistrSettings
    {
        /// <summary>
        /// Creates deep clone
        /// </summary>
        /// <returns></returns>
        IDistrSettings DeepClone();

    }//IDistrSettings
}
