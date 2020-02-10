using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Globalization;
using System.Reflection;
using RCNet.XmlTools;

namespace RCNet.Neural.Data.Filter
{
    /// <summary>
    /// Base class of feature filters setup
    /// </summary>
    [Serializable]
    public abstract class BaseFeatureFilterSettings : RCNetBaseSettings
    {
        //Constants

        //Attribute properties
        /// <summary>
        /// Feature type
        /// </summary>
        public BaseFeatureFilter.FeatureType Type { get; set; }

        //Constructors
        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="featureType">Feature type</param>
        public BaseFeatureFilterSettings(BaseFeatureFilter.FeatureType featureType)
        {
            Type = featureType;
            return;
        }

        /// <summary>
        /// Deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public BaseFeatureFilterSettings(BaseFeatureFilterSettings source)
        {
            Type = source.Type;
            return;
        }

        //Methods

    }//BaseFeatureFilterSettings

}//Namespace
