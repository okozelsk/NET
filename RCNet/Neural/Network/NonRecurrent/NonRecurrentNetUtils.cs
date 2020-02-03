using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using RCNet.Neural.Network.NonRecurrent.FF;
using RCNet.Neural.Network.NonRecurrent.PP;

namespace RCNet.Neural.Network.NonRecurrent
{
    /// <summary>
    /// Static helper methods related to non-recurrent-networks
    /// </summary>
    public static class NonRecurrentNetUtils
    {
        /// <summary>
        /// Determines if given settings object is FeedForwardNetworkSettings
        /// </summary>
        /// <param name="settings">Non-recurrent-network settings</param>
        public static bool IsFF(object settings)
        {
            return (settings.GetType() == typeof(FeedForwardNetworkSettings));
        }

        /// <summary>
        /// Determines if given configuration element is FeedForwardNetworkSettings element
        /// </summary>
        /// <param name="cfgElem">Configuration element</param>
        public static bool IsFFElem(XElement cfgElem)
        {
            return (cfgElem.Name.LocalName == "ff");
        }

        /// <summary>
        /// Determines if given settings object is ParallelPerceptronSettings
        /// </summary>
        /// <param name="settings">Non-recurrent-network settings</param>
        public static bool IsPP(object settings)
        {
            return (settings.GetType() == typeof(ParallelPerceptronSettings));
        }

        /// <summary>
        /// Determines if given configuration element is ParallelPerceptronSettings element
        /// </summary>
        /// <param name="cfgElem">Configuration element</param>
        public static bool IsPPElem(XElement cfgElem)
        {
            return (cfgElem.Name.LocalName == "pp");
        }



        /// <summary>
        /// Deeply clones given settings
        /// </summary>
        /// <param name="settings">Non-recurrent-network settings</param>
        public static object CloneSettings(object settings)
        {
            if (IsFF(settings))
            {
                return ((FeedForwardNetworkSettings)(settings)).DeepClone();
            }
            else if(IsPP(settings))
            {
                return ((ParallelPerceptronSettings)(settings)).DeepClone();
            }
            else
            {
                throw new Exception($"Unsupported type of given settings: {settings.GetType().ToString()}");
            }
        }

        /// <summary>
        /// Instantiates appropriate non-recurrent-network settings from given xml element.
        /// Type of settings is determined using element local name.
        /// </summary>
        /// <param name="cfgElem">XML element containing settings</param>
        public static object InstantiateSettings(XElement cfgElem)
        {
            if(IsFFElem(cfgElem))
            {
                return new FeedForwardNetworkSettings(cfgElem);
            }
            else if(IsPPElem(cfgElem))
            {
                return new ParallelPerceptronSettings(cfgElem);
            }
            else
            {
                throw new Exception($"Unsupported cfgElem name: {cfgElem.Name.ToString()}");
            }
        }

        /// <summary>
        /// Loads collection of instantiated non-recurrent-network settings under given root element.
        /// If rootElem is null then empty collection is returned
        /// </summary>
        /// <param name="rootElem">Root element</param>
        public static List<object> LoadSettingsCollection(XElement rootElem)
        {
            List<object> settingsCollection = new List<object>();
            if (rootElem != null)
            {
                foreach (XElement cfgElem in rootElem.Descendants())
                {
                    if (IsFFElem(cfgElem) || IsPPElem(cfgElem))
                    {
                        settingsCollection.Add(InstantiateSettings(cfgElem));
                    }
                }
            }
            return settingsCollection;
        }


    }//NonRecurrentNetUtils
}//Namespace
