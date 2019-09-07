using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCNet.RandomValue
{
    public interface IDistrSettings
    {
        /// <summary>
        /// Creates deep clone
        /// </summary>
        /// <returns></returns>
        IDistrSettings DeepClone();

    }//IDistrSettings
}
