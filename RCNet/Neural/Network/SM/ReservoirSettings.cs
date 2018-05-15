using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Xml.Linq;
using System.IO;
using RCNet.Extensions;
using RCNet.XmlTools;
using RCNet.RandomValue;
using RCNet.Neural.Activation;

namespace RCNet.Neural.Network.SM
{
    /// <summary>
    /// The class contains reservoir configuration parameters and also contains
    /// internal logic so it is not just a container of parameters. To create the proper instance by hand is not
    /// a trivial task.
    /// The easiest and safest way to create an instance is to use the xml constructor.
    /// </summary>
    [Serializable]
    public class ReservoirSettings
    {
        //Attribute properties
        /// <summary>
        /// Name of this configuration
        /// </summary>
        public string SettingsName { get; set; }
        /// <summary>
        /// Specifies how will input value be presented to reservoir
        /// </summary>
        public CommonEnums.InputCodingType InputCoding { get; set; }
        /// <summary>
        /// In case of analog input coding this specifies number of ms of the sustain analog signal.
        /// In case of spike train coding thes specifies how many spikes code the input value (each spike takes 1 ms).
        /// </summary>
        public int InputDuration { get; set; }
        /// <summary>
        /// Spectral radius.
        /// </summary>
        public double SpectralRadius { get; set; }
        /// <summary>
        /// Collection of neural pools to be instantiated within the reservoir
        /// </summary>
        public List<PoolSettings> PoolSettingsCollection { get; set; }
        /// <summary>
        /// Collection of pools interconnection settings
        /// </summary>
        public List<PoolsInterconnection> PoolsInterconnectionCollection { get; set; }

        //Constructors
        /// <summary>
        /// Creates an uninitialized instance
        /// </summary>
        public ReservoirSettings()
        {
            SettingsName = string.Empty;
            InputCoding = CommonEnums.InputCodingType.Analog;
            InputDuration = 0;
            SpectralRadius = -1;
            PoolSettingsCollection = null;
            PoolsInterconnectionCollection = null;
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public ReservoirSettings(ReservoirSettings source)
        {
            SettingsName = source.SettingsName;
            InputCoding = source.InputCoding;
            InputDuration = source.InputDuration;
            SpectralRadius = source.SpectralRadius;
            PoolSettingsCollection = null;
            if(source.PoolSettingsCollection != null)
            {
                PoolSettingsCollection = new List<PoolSettings>(source.PoolSettingsCollection.Count);
                foreach(PoolSettings srcPoolSettings in source.PoolSettingsCollection)
                {
                    PoolSettingsCollection.Add(srcPoolSettings.DeepClone());
                }
            }
            PoolsInterconnectionCollection = null;
            if(source.PoolsInterconnectionCollection != null)
            {
                PoolsInterconnectionCollection = new List<PoolsInterconnection>(source.PoolsInterconnectionCollection.Count);
                foreach(PoolsInterconnection poolsInterConn in source.PoolsInterconnectionCollection)
                {
                    PoolsInterconnectionCollection.Add(poolsInterConn.DeepClone());
                }
            }
            return;
        }

        /// <summary>
        /// Creates the instance and initialize it from given xml element.
        /// This is the preferred way to instantiate reservoir settings.
        /// </summary>
        /// <param name="elem">
        /// Xml data containing reservoir settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public ReservoirSettings(XElement elem)
        {
            //Validation
            ElemValidator validator = new ElemValidator();
            Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.Neural.Network.SM.ReservoirSettings.xsd");
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.RCNetTypes.xsd");
            XElement reservoirSettingsElem = validator.Validate(elem, "rootElem");
            //Parsing
            SettingsName = reservoirSettingsElem.Attribute("name").Value;
            InputCoding = CommonEnums.ParseInputCodingType(reservoirSettingsElem.Attribute("inputCoding").Value);
            InputDuration = int.Parse(reservoirSettingsElem.Attribute("inputDuration").Value, CultureInfo.InvariantCulture);
            SpectralRadius = reservoirSettingsElem.Attribute("spectralRadius").Value == "NA" ? -1d : double.Parse(reservoirSettingsElem.Attribute("spectralRadius").Value, CultureInfo.InvariantCulture);
            //Pool settings collection
            PoolSettingsCollection = new List<PoolSettings>();
            foreach (XElement poolSettingsElem in reservoirSettingsElem.Descendants("pools").First().Descendants("pool"))
            {
                PoolSettingsCollection.Add(new PoolSettings(poolSettingsElem));
            }
            //Pools interconnection settings
            PoolsInterconnectionCollection = new List<PoolsInterconnection>();
            XElement pool2PoolConnContainerElem = reservoirSettingsElem.Descendants("pool2PoolConns").FirstOrDefault();
            if (pool2PoolConnContainerElem != null)
            {
                foreach (XElement poolsInterConnElem in pool2PoolConnContainerElem.Descendants("pool2PoolConn"))
                {
                    PoolsInterconnectionCollection.Add(new PoolsInterconnection(poolsInterConnElem, PoolSettingsCollection));
                }
            }
            return;
        }

        //Methods
        /// <summary>
        /// See the base.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            ReservoirSettings cmpSettings = obj as ReservoirSettings;
            if (SettingsName != cmpSettings.SettingsName ||
                InputCoding != cmpSettings.InputCoding ||
                InputDuration != cmpSettings.InputDuration ||
                SpectralRadius != cmpSettings.SpectralRadius ||
                (PoolSettingsCollection == null && cmpSettings.PoolSettingsCollection != null) ||
                (PoolSettingsCollection != null && cmpSettings.PoolSettingsCollection == null) ||
                (PoolSettingsCollection != null && PoolSettingsCollection.Count != cmpSettings.PoolSettingsCollection.Count) ||
                (PoolsInterconnectionCollection == null && cmpSettings.PoolsInterconnectionCollection != null) ||
                (PoolsInterconnectionCollection != null && cmpSettings.PoolsInterconnectionCollection == null) ||
                (PoolsInterconnectionCollection != null && PoolsInterconnectionCollection.Count != cmpSettings.PoolsInterconnectionCollection.Count)
                )
            {
                return false;
            }

            if (PoolSettingsCollection != null)
            {
                for (int i = 0; i < PoolSettingsCollection.Count; i++)
                {
                    if (!Equals(PoolSettingsCollection[i], cmpSettings.PoolSettingsCollection[i]))
                    {
                        return false;
                    }
                }
            }

            if (PoolsInterconnectionCollection != null)
            {
                for (int i = 0; i < PoolsInterconnectionCollection.Count; i++)
                {
                    if (!Equals(PoolsInterconnectionCollection[i], cmpSettings.PoolsInterconnectionCollection[i]))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// See the base.
        /// </summary>
        public override int GetHashCode()
        {
            return SettingsName.GetHashCode();
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public ReservoirSettings DeepClone()
        {
            ReservoirSettings clone = new ReservoirSettings(this);
            return clone;
        }


        //Inner classes
        /// <summary>
        /// Class specifies interconnection between two pools
        /// </summary>
        [Serializable]
        public class PoolsInterconnection
        {
            //Attributes
            /// <summary>
            /// Name of the target pool
            /// </summary>
            public string TargetPoolName { get; set; }
            /// <summary>
            /// Target pool ID
            /// </summary>
            public int TargetPoolID { get; }
            /// <summary>
            /// Determines how many randomly selected neurons in target pool will receive signal from source pool neurons
            /// Count = TargetPool.Dim.Size * TargetConnectionDensity
            /// </summary>
            public double TargetConnectionDensity { get; set; }
            /// <summary>
            /// Name of the source pool
            /// </summary>
            public string SourcePoolName { get; set; }
            /// <summary>
            /// Source pool ID
            /// </summary>
            public int SourcePoolID { get; }
            /// <summary>
            /// Determines how many randomly selected neurons in the source pool will be connected to one neuron in target pool.
            /// Count = SourcePool.Dim.Size * SourceConnectionDensity
            /// </summary>
            public double SourceConnectionDensity { get; set; }
            /// <summary>
            /// Weight of source pool neuron to target pool neuron synapse.
            /// </summary>
            public RandomValueSettings SynapseWeight { get; set; }

            //Constructors
            /// <summary>
            /// Creates an unitialized instance
            /// </summary>
            public PoolsInterconnection()
            {
                SourcePoolName = string.Empty;
                SourceConnectionDensity = 0;
                TargetPoolName = string.Empty;
                TargetConnectionDensity = 0;
                SynapseWeight = null;
                return;
            }

            /// <summary>
            /// Copy constructor
            /// </summary>
            /// <param name="source">Source instance</param>
            public PoolsInterconnection(PoolsInterconnection source)
            {
                SourcePoolName = source.SourcePoolName;
                SourcePoolID = source.SourcePoolID;
                SourceConnectionDensity = source.SourceConnectionDensity;
                TargetPoolName = source.TargetPoolName;
                TargetPoolID = source.TargetPoolID;
                TargetConnectionDensity = source.TargetConnectionDensity;
                SynapseWeight = null;
                if (source.SynapseWeight != null)
                {
                    SynapseWeight = source.SynapseWeight.DeepClone();
                }
                return;
            }

            /// <summary>
            /// Creates the instance itialized from xml
            /// </summary>
            public PoolsInterconnection(XElement interConnElem, List<PoolSettings> poolSettingsCollection)
            {
                TargetPoolName = interConnElem.Attribute("targetPool").Value;
                TargetPoolID = -1;
                //Find target pool ID (index)
                for (int idx = 0; idx < poolSettingsCollection.Count; idx++)
                {
                    if (poolSettingsCollection[idx].InstanceName == TargetPoolName)
                    {
                        TargetPoolID = idx;
                        break;
                    }
                }
                if (TargetPoolID == -1)
                {
                    throw new Exception($"Pool {TargetPoolName} was not found.");
                }
                TargetConnectionDensity = double.Parse(interConnElem.Attribute("targetConnDensity").Value, CultureInfo.InvariantCulture);
                SourcePoolName = interConnElem.Attribute("srcPool").Value;
                SourcePoolID = -1;
                //Find source pool ID (index)
                for (int idx = 0; idx < poolSettingsCollection.Count; idx++)
                {
                    if (poolSettingsCollection[idx].InstanceName == SourcePoolName)
                    {
                        SourcePoolID = idx;
                        break;
                    }
                }
                if (SourcePoolID == -1)
                {
                    throw new Exception($"Pool {SourcePoolName} was not found.");
                }
                SourceConnectionDensity = double.Parse(interConnElem.Attribute("srcConnDensity").Value, CultureInfo.InvariantCulture);
                if (SourcePoolID == TargetPoolID)
                {
                    throw new Exception($"Two different pools have to be specified for interpool connection.");
                }
                SynapseWeight = new RandomValueSettings(interConnElem.Descendants("weight").First());
                return;
            }

            //Methods
            /// <summary>
            /// See the base.
            /// </summary>
            public override bool Equals(object obj)
            {
                if (obj == null) return false;
                PoolsInterconnection cmpSettings = obj as PoolsInterconnection;
                if (SourcePoolName != cmpSettings.SourcePoolName ||
                    SourceConnectionDensity != cmpSettings.SourceConnectionDensity ||
                    TargetPoolName != cmpSettings.TargetPoolName ||
                    TargetConnectionDensity != cmpSettings.TargetConnectionDensity ||
                    !Equals(SynapseWeight, cmpSettings.SynapseWeight)
                    )
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

            /// <summary>
            /// Creates the deep copy instance of this instance
            /// </summary>
            public PoolsInterconnection DeepClone()
            {
                PoolsInterconnection clone = new PoolsInterconnection(this);
                return clone;
            }

        }//PoolsInterconnection

    }//ReservoirSettings

}//Namespace

