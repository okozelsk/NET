using RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.PM
{
    /// <summary>
    /// Configuration of the predictors mapper's allowed predictors.
    /// </summary>
    [Serializable]
    public class AllowedPredictorsSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "SMMapperAllowedPredictorsType";

        //Attribute properties
        /// <summary>
        /// The collection of the allowed predictor configurations.
        /// </summary>
        public List<AllowedPredictorSettings> AllowedPredictorCfgCollection { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        private AllowedPredictorsSettings()
        {
            AllowedPredictorCfgCollection = new List<AllowedPredictorSettings>();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="allowedPredictorCfgCollection">The collection of the allowed predictor configurations.</param>
        public AllowedPredictorsSettings(IEnumerable<AllowedPredictorSettings> allowedPredictorCfgCollection)
            : this()
        {
            AddAllowedPredictors(allowedPredictorCfgCollection);
            Check();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="allowedPredictorCfgCollection">The allowed predictor configurations.</param>
        public AllowedPredictorsSettings(params AllowedPredictorSettings[] allowedPredictorCfgCollection)
            : this()
        {
            AddAllowedPredictors(allowedPredictorCfgCollection);
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public AllowedPredictorsSettings(AllowedPredictorsSettings source)
            : this()
        {
            AddAllowedPredictors(source.AllowedPredictorCfgCollection);
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
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
        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults { get { return false; } }

        //Methods
        /// <inheritdoc/>
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
        /// Adds the allowed predictor configurations from the specified collection into the internal collection.
        /// </summary>
        /// <param name="allowedPredictorCfgCollection">The allowed predictor configurations.</param>
        private void AddAllowedPredictors(IEnumerable<AllowedPredictorSettings> allowedPredictorCfgCollection)
        {
            foreach (AllowedPredictorSettings allowedPredictorCfg in allowedPredictorCfgCollection)
            {
                AllowedPredictorCfgCollection.Add((AllowedPredictorSettings)allowedPredictorCfg.DeepClone());
            }
            return;
        }

        /// <summary>
        /// Checks whether the specified predictor is allowed.
        /// </summary>
        /// <param name="predictorID">An identifier of the predictor.</param>
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

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new AllowedPredictorsSettings(this);
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("allowedPredictors", suppressDefaults);
        }

    }//AllowedPredictorsSettings

}//Namespace
