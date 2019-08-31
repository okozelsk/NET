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
using RCNet.Neural.Network.SM.Synapse;

namespace RCNet.Neural.Network.SM.Preprocessing
{
    /// <summary>
    /// Configuration of the availble predictors
    /// </summary>
    [Serializable]
    public class PredictorsSettings
    {
        //Attribute properties
        /// <summary>
        /// Current activation state
        /// </summary>
        public bool Activation { get; private set; }
        /// <summary>
        /// Squared current activation state
        /// </summary>
        public bool SquaredActivation { get; private set; }
        /// <summary>
        /// Exponentially weighted average firing rate within the last 64 cycles
        /// </summary>
        public bool ExpWAvgFiringRate64 { get; private set; }
        /// <summary>
        /// Fading number of firings
        /// </summary>
        public bool FadingNumOfFirings { get; private set; }
        /// <summary>
        /// Number of firings during the last 64 cycles
        /// </summary>
        public bool NumOfFirings64 { get; private set; }
        /// <summary>
        /// Binary (0/1) firing history of the last 32 cycles as an unsigned integer number
        /// </summary>
        public bool LastBin32FiringHist { get; private set; }
        /// <summary>
        /// Binary (0/1) firing history of the last 16 cycles as an unsigned integer number
        /// </summary>
        public bool LastBin16FiringHist { get; private set; }
        /// <summary>
        /// Binary (0/1) firing history of the last 8 cycles as an unsigned integer number
        /// </summary>
        public bool LastBin8FiringHist { get; private set; }
        /// <summary>
        /// Binary (0/1) indicator of the firing during the last cycle
        /// </summary>
        public bool LastBin1FiringHist { get; private set; }
        /// <summary>
        /// Number of enabled predictors
        /// </summary>
        public int NumOfEnabledPredictors { get; private set; }

        //Constructors
        /// <summary>
        /// Creates instance having switches initialized as a result of neuron group and reservoir instance predictors settings
        /// </summary>
        /// <param name="groupPredictorsSettings">Neuron group predictors settings</param>
        /// <param name="reservoirPredictorsSettings">Reservoir predictors settings</param>
        public PredictorsSettings(PredictorsSettings groupPredictorsSettings, PredictorsSettings reservoirPredictorsSettings)
        {
            Activation = (groupPredictorsSettings.Activation && reservoirPredictorsSettings.Activation);
            SquaredActivation = (groupPredictorsSettings.SquaredActivation && reservoirPredictorsSettings.SquaredActivation);
            ExpWAvgFiringRate64 = (groupPredictorsSettings.ExpWAvgFiringRate64 && reservoirPredictorsSettings.ExpWAvgFiringRate64);
            FadingNumOfFirings = (groupPredictorsSettings.FadingNumOfFirings && reservoirPredictorsSettings.FadingNumOfFirings);
            NumOfFirings64 = (groupPredictorsSettings.NumOfFirings64 && reservoirPredictorsSettings.NumOfFirings64);
            LastBin32FiringHist = (groupPredictorsSettings.LastBin32FiringHist && reservoirPredictorsSettings.LastBin32FiringHist);
            LastBin16FiringHist = (groupPredictorsSettings.LastBin16FiringHist && reservoirPredictorsSettings.LastBin16FiringHist);
            LastBin8FiringHist = (groupPredictorsSettings.LastBin8FiringHist && reservoirPredictorsSettings.LastBin8FiringHist);
            LastBin1FiringHist = (groupPredictorsSettings.LastBin1FiringHist && reservoirPredictorsSettings.LastBin1FiringHist);
            NumOfEnabledPredictors = GetNumOfEnabledPredictors();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public PredictorsSettings(PredictorsSettings source)
        {
            Activation = source.Activation;
            SquaredActivation = source.SquaredActivation;
            ExpWAvgFiringRate64 = source.ExpWAvgFiringRate64;
            FadingNumOfFirings = source.FadingNumOfFirings;
            NumOfFirings64 = source.NumOfFirings64;
            LastBin32FiringHist = source.LastBin32FiringHist;
            LastBin16FiringHist = source.LastBin16FiringHist;
            LastBin8FiringHist = source.LastBin8FiringHist;
            LastBin1FiringHist = source.LastBin1FiringHist;
            NumOfEnabledPredictors = source.NumOfEnabledPredictors;
            return;
        }

        /// <summary>
        /// Creates the instance and initialize it from given xml element.
        /// </summary>
        /// <param name="elem">
        /// Xml element containing the settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public PredictorsSettings(XElement elem)
        {
            //Validation
            ElemValidator validator = new ElemValidator();
            Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.Neural.Network.SM.Preprocessing.PredictorsSettings.xsd");
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.RCNetTypes.xsd");
            XElement PredictorsSettingsElem = validator.Validate(elem, "rootElem");
            //Parsing
            Activation = bool.Parse(PredictorsSettingsElem.Attribute("activation").Value);
            SquaredActivation = bool.Parse(PredictorsSettingsElem.Attribute("squaredActivation").Value);
            ExpWAvgFiringRate64 = bool.Parse(PredictorsSettingsElem.Attribute("expWAvgFiringRate64").Value);
            FadingNumOfFirings = bool.Parse(PredictorsSettingsElem.Attribute("fadingNumOfFirings").Value);
            NumOfFirings64 = bool.Parse(PredictorsSettingsElem.Attribute("numOfFirings64").Value);
            LastBin32FiringHist = bool.Parse(PredictorsSettingsElem.Attribute("lastBin32FiringHist").Value);
            LastBin16FiringHist = bool.Parse(PredictorsSettingsElem.Attribute("lastBin16FiringHist").Value);
            LastBin8FiringHist = bool.Parse(PredictorsSettingsElem.Attribute("lastBin8FiringHist").Value);
            LastBin1FiringHist = bool.Parse(PredictorsSettingsElem.Attribute("lastBin1FiringHist").Value);
            NumOfEnabledPredictors = GetNumOfEnabledPredictors();
            return;
        }

        //Properties
        //Methods
        /// <summary>
        /// Determines number of enabled predictors
        /// </summary>
        private int GetNumOfEnabledPredictors()
        {
            int count = 0;
            count += Activation ? 1 : 0;
            count += SquaredActivation ? 1 : 0;
            count += ExpWAvgFiringRate64 ? 1 : 0;
            count += FadingNumOfFirings ? 1 : 0;
            count += NumOfFirings64 ? 1 : 0;
            count += LastBin32FiringHist ? 1 : 0;
            count += LastBin16FiringHist ? 1 : 0;
            count += LastBin8FiringHist ? 1 : 0;
            count += LastBin1FiringHist ? 1 : 0;
            return count;
        }
        /// <summary>
        /// See the base.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            PredictorsSettings cmpSettings = obj as PredictorsSettings;
            if (Activation != cmpSettings.Activation ||
                SquaredActivation != cmpSettings.SquaredActivation ||
                ExpWAvgFiringRate64 != cmpSettings.ExpWAvgFiringRate64 ||
                FadingNumOfFirings != cmpSettings.FadingNumOfFirings ||
                NumOfFirings64 != cmpSettings.NumOfFirings64 ||
                LastBin32FiringHist != cmpSettings.LastBin32FiringHist ||
                LastBin16FiringHist != cmpSettings.LastBin16FiringHist ||
                LastBin8FiringHist != cmpSettings.LastBin8FiringHist ||
                LastBin1FiringHist != cmpSettings.LastBin1FiringHist ||
                NumOfEnabledPredictors != cmpSettings.NumOfEnabledPredictors
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
        public PredictorsSettings DeepClone()
        {
            PredictorsSettings clone = new PredictorsSettings(this);
            return clone;
        }

    }//PredictorsSettings

}//Namespace

