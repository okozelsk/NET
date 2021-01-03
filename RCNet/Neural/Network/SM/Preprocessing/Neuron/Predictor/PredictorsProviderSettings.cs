using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor
{
    /// <summary>
    /// Configuration of the PredictorsProvider.
    /// </summary>
    [Serializable]
    public class PredictorsProviderSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "PredictorsType";

        //Attribute properties
        /// <summary>
        ///The collection of the predictor computer configurations.
        /// </summary>
        public List<IPredictorSettings> PredictorCfgCollection { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="predictorsCfg">The collection of the predictor computer configurations.</param>
        public PredictorsProviderSettings(IEnumerable predictorsCfg = null)
        {
            PredictorCfgCollection = new List<IPredictorSettings>();
            foreach (IPredictorSettings predictorCfg in predictorsCfg)
            {
                if (predictorCfg != null)
                {
                    PredictorCfgCollection.Add((IPredictorSettings)((RCNetBaseSettings)predictorCfg).DeepClone());
                }
            }
            Check();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="predictorsCfg">The predictor computer configurations.</param>
        public PredictorsProviderSettings(params IPredictorSettings[] predictorsCfg)
            : this(predictorsCfg.AsEnumerable())
        {
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">The source instance.</param>
        public PredictorsProviderSettings(PredictorsProviderSettings source)
            : this(source.PredictorCfgCollection)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public PredictorsProviderSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Allocation
            PredictorCfgCollection = new List<IPredictorSettings>();
            //Parsing
            foreach (XElement predictorElem in settingsElem.Elements())
            {
                IPredictorSettings predictorCfg = PredictorFactory.LoadPredictorSettings(predictorElem);
                if (predictorCfg != null)
                {
                    PredictorCfgCollection.Add(predictorCfg);
                }
            }
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// The total number of predictors.
        /// </summary>
        public int NumOfPredictors
        {
            get
            {
                return PredictorCfgCollection.Count;
            }
        }

        /// <summary>
        /// The necessary size of the moving data window of the activations.
        /// </summary>
        public int RequiereActivationMDWSize
        {
            get
            {
                int size = 0;
                foreach (IPredictorSettings predictorCfg in PredictorCfgCollection)
                {
                    if (predictorCfg != null)
                    {
                        size = Math.Max(size, predictorCfg.RequiredWndSizeOfActivations);
                    }
                }
                return size;
            }
        }

        /// <summary>
        /// The necessary size of the moving data window of the firings.
        /// </summary>
        public int RequiereFiringMDWSize
        {
            get
            {
                int size = 0;
                foreach (IPredictorSettings predictorCfg in PredictorCfgCollection)
                {
                    if (predictorCfg != null)
                    {
                        size = Math.Max(size, predictorCfg.RequiredWndSizeOfFirings);
                    }
                }
                return size;
            }
        }

        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return (NumOfPredictors == 0);
            }
        }

        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            return;
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new PredictorsProviderSettings(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            foreach (IPredictorSettings predictorCfg in PredictorCfgCollection)
            {
                rootElem.Add(((RCNetBaseSettings)predictorCfg).GetXml(suppressDefaults));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("predictors", suppressDefaults);
        }

    }//PredictorsProviderSettings

}//Namespace

