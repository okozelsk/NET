using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Xml.Linq;
using System.Reflection;
using RCNet.Extensions;
using RCNet.XmlTools;
using RCNet.RandomValue;
using RCNet.Neural.Data;
using RCNet.Neural.Data.Filter;
using RCNet.Neural.Data.Generators;
using RCNet.Neural.Network.SM.Preprocessing.Input;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir;

namespace RCNet.Neural.Network.SM.Preprocessing
{
    /// <summary>
    /// Neural Preprocessor configuration parameters
    /// </summary>
    [Serializable]
    public class NeuralPreprocessorSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "NPType";
        //Default values
        /// <summary>
        /// Default value of parameter specifying how many predictors having smallest rescalled range to be disabled
        /// </summary>
        public const double DefaultPredictorsReductionRatio = 0d;

        //Attribute properties
        /// <summary>
        /// Configuration of the preprocessor input
        /// </summary>
        public InputSettings InputCfg { get; }

        /// <summary>
        /// Configuration of reservoir structures
        /// </summary>
        public ReservoirStructuresSettings ReservoirStructuresCfg { get; }

        /// <summary>
        /// Configuration of reservoir instances
        /// </summary>
        public ReservoirInstancesSettings ReservoirInstancesCfg { get; }

        /// <summary>
        /// Specifies how many predictors having smallest rescalled range to be disabled
        /// </summary>
        public double PredictorsReductionRatio { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="inputCfg">Configuration of the preprocessor input</param>
        /// <param name="reservoirStructuresCfg">Configuration of reservoir structures</param>
        /// <param name="reservoirInstancesCfg">Configuration of reservoir instances</param>
        /// <param name="predictorsReductionRatio">Specifies how many predictors having smallest rescalled range to be disabled</param>
        public NeuralPreprocessorSettings(InputSettings inputCfg,
                                          ReservoirStructuresSettings reservoirStructuresCfg,
                                          ReservoirInstancesSettings reservoirInstancesCfg,
                                          double predictorsReductionRatio = DefaultPredictorsReductionRatio
                                          )
        {
            InputCfg = (InputSettings)inputCfg.DeepClone();
            ReservoirStructuresCfg = (ReservoirStructuresSettings)reservoirStructuresCfg.DeepClone();
            ReservoirInstancesCfg = (ReservoirInstancesSettings)reservoirInstancesCfg.DeepClone();
            PredictorsReductionRatio = predictorsReductionRatio;
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">Source instance</param>
        public NeuralPreprocessorSettings(NeuralPreprocessorSettings source)
            : this(source.InputCfg, source.ReservoirStructuresCfg, source.ReservoirInstancesCfg, source.PredictorsReductionRatio)
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
        public NeuralPreprocessorSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            InputCfg = new InputSettings(settingsElem.Elements("input").First());
            ReservoirStructuresCfg = new ReservoirStructuresSettings(settingsElem.Elements("reservoirStructures").First());
            ReservoirInstancesCfg = new ReservoirInstancesSettings(settingsElem.Elements("reservoirInstances").First());
            PredictorsReductionRatio = double.Parse(settingsElem.Attribute("predictorsReductionRatio").Value, CultureInfo.InvariantCulture);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultPredictorsReductionRatio { get { return (PredictorsReductionRatio == DefaultPredictorsReductionRatio); } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults { get { return false; } }

        //Methods
        /// <summary>
        /// Checks validity
        /// </summary>
        private void Check()
        {
            if (PredictorsReductionRatio < 0 || PredictorsReductionRatio >= 1)
            {
                throw new Exception($"Invalid PredictorsReductionRatio {PredictorsReductionRatio.ToString(CultureInfo.InvariantCulture)}. PredictorsReductionRatio must be GE to 0 and LT 1.");
            }
            //Reservoir instances consistency
            foreach(ReservoirInstanceSettings resInstCfg in ReservoirInstancesCfg.ReservoirInstanceCfgCollection)
            {
                resInstCfg.CheckConsistency(InputCfg, ReservoirStructuresCfg.GetReservoirStructureCfg(resInstCfg.StructureCfgName));
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new NeuralPreprocessorSettings(this);
        }

        /// <summary>
        /// Generates xml element containing the settings.
        /// </summary>
        /// <param name="rootElemName">Name to be used as a name of the root element.</param>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName,
                                             InputCfg.GetXml(suppressDefaults),
                                             ReservoirStructuresCfg.GetXml(suppressDefaults),
                                             ReservoirInstancesCfg.GetXml(suppressDefaults)
                                             );
            if (!suppressDefaults || !IsDefaultPredictorsReductionRatio)
            {
                rootElem.Add(new XAttribute("predictorsReductionRatio", PredictorsReductionRatio.ToString(CultureInfo.InvariantCulture)));
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
            return GetXml("neuralPreprocessor", suppressDefaults);
        }

    }//NeuralPreprocessorSettings

}//Namespace
