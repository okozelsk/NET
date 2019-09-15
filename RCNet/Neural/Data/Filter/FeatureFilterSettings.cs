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
    public abstract class FeatureFilterSettings
    {
        //Constants

        //Attribute properties
        /// <summary>
        /// Feature type
        /// </summary>
        public FeatureFilter.FeatureType Type { get; set; }

        //Constructors
        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="featureType">Feature type</param>
        public FeatureFilterSettings(FeatureFilter.FeatureType featureType)
        {
            Type = featureType;
            return;
        }

        /// <summary>
        /// Deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public FeatureFilterSettings(FeatureFilterSettings source)
        {
            Type = source.Type;
            return;
        }

        //Methods
        /// <summary>
        /// See the base.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            FeatureFilterSettings cmpSettings = obj as FeatureFilterSettings;
            if (Type != cmpSettings.Type)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// See the base.
        /// </summary>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

    }//FeatureFilterSettings

}//Namespace
