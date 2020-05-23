using RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.PM
{
    /// <summary>
    /// Collection of predictors mapper's allowed predictors settings
    /// </summary>
    [Serializable]
    public class AllowedPredictorsSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "SMMapperAllowedPredictorsType";

        //Attribute properties
        /// <summary>
        /// Collection of predictors settings
        /// </summary>
        public List<AllowedPredictorSettings> AllowedPredictorCfgCollection { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        private AllowedPredictorsSettings()
        {
            AllowedPredictorCfgCollection = new List<AllowedPredictorSettings>();
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="allowedPredictorCfgCollection">Allowed predictor settings collection</param>
        public AllowedPredictorsSettings(IEnumerable<AllowedPredictorSettings> allowedPredictorCfgCollection)
            : this()
        {
            AddAllowedPredictors(allowedPredictorCfgCollection);
            Check();
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="allowedPredictorCfgCollection">Allowed predictor settings collection</param>
        public AllowedPredictorsSettings(params AllowedPredictorSettings[] allowedPredictorCfgCollection)
            : this()
        {
            AddAllowedPredictors(allowedPredictorCfgCollection);
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public AllowedPredictorsSettings(AllowedPredictorsSettings source)
            : this()
        {
            AddAllowedPredictors(source.AllowedPredictorCfgCollection);
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings.</param>
        public AllowedPredictorsSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            AllowedPredictorCfgCollection = new List<AllowedPredictorSettings>();
            foreach (XElement predictorElem in settingsElem.Elements("predictor"))
            {
                AllowedPredictorCfgCollection.Add(new AllowedPredictorSettings(predictorElem));
            }
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults { get { return false; } }

        //Methods
        /// <summary>
        /// Checks consistency
        /// </summary>
        protected override void Check()
        {
            if (AllowedPredictorCfgCollection.Count == 0)
            {
                throw new ArgumentException($"At least one allowed predictor configuration must be specified.", "AllowedPredictorCfgCollection");
            }
            //Uniqueness of predictor ID
            string[] names = new string[AllowedPredictorCfgCollection.Count];
            names[0] = AllowedPredictorCfgCollection[0].PredictorID.ToString();
            for (int i = 1; i < AllowedPredictorCfgCollection.Count; i++)
            {
                if (names.Contains(AllowedPredictorCfgCollection[i].PredictorID.ToString()))
                {
                    throw new ArgumentException($"Predictor {AllowedPredictorCfgCollection[i].PredictorID} is not unique.", "AllowedPredictorCfgCollection");
                }
                names[i] = AllowedPredictorCfgCollection[i].PredictorID.ToString();
            }
            return;
        }

        /// <summary>
        /// Adds cloned allowed predictor configurations from given collection into the internal collection
        /// </summary>
        /// <param name="allowedPredictorCfgCollection">Allowed predictor settings collection</param>
        private void AddAllowedPredictors(IEnumerable<AllowedPredictorSettings> allowedPredictorCfgCollection)
        {
            foreach (AllowedPredictorSettings allowedPredictorCfg in allowedPredictorCfgCollection)
            {
                AllowedPredictorCfgCollection.Add((AllowedPredictorSettings)allowedPredictorCfg.DeepClone());
            }
            return;
        }

        /// <summary>
        /// Check if specified predictor is allowed
        /// </summary>
        /// <param name="predictorID">Predictor ID</param>
        public bool IsAllowed(PredictorsProvider.PredictorID predictorID)
        {
            foreach (AllowedPredictorSettings predictorCfg in AllowedPredictorCfgCollection)
            {
                if (predictorCfg.PredictorID == predictorID)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new AllowedPredictorsSettings(this);
        }

        /// <summary>
        /// Generates xml element containing the settings.
        /// </summary>
        /// <param name="rootElemName">Name to be used as a name of the root element.</param>
        /// <param name="suppressDefaults">Specifies whether to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            foreach (AllowedPredictorSettings allowedPredictorCfg in AllowedPredictorCfgCollection)
            {
                rootElem.Add(allowedPredictorCfg.GetXml(suppressDefaults));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <summary>
        /// Generates default named xml element containing the settings.
        /// </summary>
        /// <param name="suppressDefaults">Specifies whether to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("allowedPredictors", suppressDefaults);
        }

    }//AllowedPredictorsSettings

}//Namespace
