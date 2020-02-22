using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Xml.Linq;
using System.Xml.XPath;
using System.IO;
using RCNet.Extensions;
using RCNet.XmlTools;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Space3D;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Pool.NeuronGroup;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Neuron.Predictor;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.Pool
{
    /// <summary>
    /// Configuration of a neural pool.
    /// </summary>
    [Serializable]
    public class PoolSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "PoolType";

        //Attribute properties
        /// <summary>
        /// Pool name
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Pool dimensions
        /// </summary>
        public ProportionsSettings ProportionsCfg { get; }
        /// <summary>
        /// Pool coordinates within the 3D space
        /// </summary>
        public CoordinatesSettings CoordinatesCfg { get; }
        /// <summary>
        /// Settings of the neuron groups in the pool
        /// </summary>
        public NeuronGroupsSettings NeuronGroupsCfg { get; }
        /// <summary>
        /// Configuration of the pool's neurons interconnection
        /// </summary>
        public InterconnSettings InterconnectionCfg { get; }
        /// <summary>
        /// Configuration of the predictors
        /// </summary>
        public PredictorsSettings PredictorsCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="name">Pool name</param>
        /// <param name="proportionsCfg">Pool dimensions</param>
        /// <param name="neuronGroupsCfg">Settings of the neuron groups in the pool</param>
        /// <param name="interconnectionCfg">Configuration of the pool's neurons interconnection</param>
        /// <param name="predictorsCfg">Configuration of the predictors</param>
        /// <param name="coordinatesCfg">Pool coordinates within the 3D space</param>
        public PoolSettings(string name,
                            ProportionsSettings proportionsCfg,
                            NeuronGroupsSettings neuronGroupsCfg,
                            InterconnSettings interconnectionCfg,
                            PredictorsSettings predictorsCfg = null,
                            CoordinatesSettings coordinatesCfg = null
                            )
        {
            Name = name;
            ProportionsCfg = (ProportionsSettings)proportionsCfg.DeepClone();
            NeuronGroupsCfg = (NeuronGroupsSettings)neuronGroupsCfg.DeepClone();
            InterconnectionCfg = (InterconnSettings)interconnectionCfg.DeepClone();
            PredictorsCfg = predictorsCfg == null ? null : (PredictorsSettings)predictorsCfg.DeepClone();
            CoordinatesCfg = coordinatesCfg == null ? new CoordinatesSettings() : (CoordinatesSettings)coordinatesCfg.DeepClone();
            CheckAndComplete();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public PoolSettings(PoolSettings source)
            :this(source.Name, source.ProportionsCfg, source.NeuronGroupsCfg, source.InterconnectionCfg, source.PredictorsCfg, source.CoordinatesCfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance from the given xml element.
        /// </summary>
        /// <param name="elem">
        /// Xml data containing the settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public PoolSettings(XElement elem)
        {
            //Validation
            XElement poolSettingsElem = Validate(elem, XsdTypeName);
            //Parsing
            Name = poolSettingsElem.Attribute("name").Value;
            ProportionsCfg = new ProportionsSettings(poolSettingsElem.Descendants("proportions").First());
            NeuronGroupsCfg = new NeuronGroupsSettings(poolSettingsElem.Descendants("neuronGroups").First());
            InterconnectionCfg = new InterconnSettings(poolSettingsElem.Descendants("interconnection").First());
            //Predictors
            XElement predictorsElem = poolSettingsElem.Descendants("predictors").FirstOrDefault();
            if (predictorsElem != null)
            {
                PredictorsCfg = new PredictorsSettings(predictorsElem);
            }
            //Coordinates
            XElement coordinatesElem = poolSettingsElem.Descendants("coordinates").FirstOrDefault();
            CoordinatesCfg = coordinatesElem == null ? new CoordinatesSettings() : new CoordinatesSettings(coordinatesElem);
            CheckAndComplete();
            return;
        }

        //Properties
        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return false;
            }
        }


        //Methods
        /// <summary>
        /// Checks validity and completes object
        /// </summary>
        private void CheckAndComplete()
        {
            if (Name.Length == 0)
            {
                throw new Exception($"Name can not be empty.");
            }
            NeuronGroupsCfg.SetGrpNeuronsSubCounts(ProportionsCfg.Size);
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new PoolSettings(this);
        }

        /// <summary>
        /// Generates xml element containing the settings.
        /// </summary>
        /// <param name="rootElemName">Name to be used as a name of the root element.</param>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName, new XAttribute("name", Name),
                                                           ProportionsCfg.GetXml(suppressDefaults));
            if (!suppressDefaults || !CoordinatesCfg.ContainsOnlyDefaults)
            {
                rootElem.Add(CoordinatesCfg.GetXml(suppressDefaults));
            }
            rootElem.Add(NeuronGroupsCfg.GetXml(suppressDefaults));
            rootElem.Add(InterconnectionCfg.GetXml(suppressDefaults));
            if (PredictorsCfg != null && !PredictorsCfg.ContainsOnlyDefaults)
            {
                rootElem.Add(PredictorsCfg.GetXml(suppressDefaults));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <summary>
        /// Generates default named xml element containing the settings.
        /// </summary>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("pool", suppressDefaults);
        }

    }//PoolSettings

}//Namespace

