﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor
{
    /// <summary>
    /// Configuration of the PredictorsProvider
    /// </summary>
    [Serializable]
    public class PredictorsProviderSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "PredictorsType";

        //Attribute properties
        /// <summary>
        ///Collection of predictors configurations
        /// </summary>
        public List<IPredictorSettings> PredictorCfgCollection { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="predictorsCfg">Collection of predictors configurations</param>
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
        /// Creates an initialized instance
        /// </summary>
        /// <param name="predictorsCfg">Predictors configurations</param>
        public PredictorsProviderSettings(params IPredictorSettings[] predictorsCfg)
            : this(predictorsCfg.AsEnumerable())
        {
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public PredictorsProviderSettings(PredictorsProviderSettings source)
            : this(source.PredictorCfgCollection)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
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
        /// Total number of predictors
        /// </summary>
        public int NumOfPredictors
        {
            get
            {
                return PredictorCfgCollection.Count;
            }
        }

        /// <summary>
        /// Specifies necessary size of the moving window of activations
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
        /// Specifies necessary size of the moving window of firings
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
            foreach(IPredictorSettings predictorCfg in PredictorCfgCollection)
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

