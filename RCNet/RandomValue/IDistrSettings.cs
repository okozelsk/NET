using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RCNet.RandomValue
{
    /// <summary>
    /// Technical interface for random distributions
    /// </summary>
    public interface IDistrSettings
    {
        /// <summary>
        /// Type of random distribution
        /// </summary>
        RandomCommon.DistributionType Type { get; }

    }//IDistrSettings
}
