using RCNet.Neural.Network.SM.Preprocessing.Input;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir;
using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

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
        /// Default value of parameter determining how many predictors having smallest value-span to be disabled
        /// </summary>
        public const double DefaultPredictorsReductionRatio = 0d;
        /// <summary>
        /// Default value of parameter specifying minimum acceptable predictor's value-span
        /// </summary>
        public const double DefaultPredictorValueMinSpan = 1e-6d;

        //Attribute properties
        /// <summary>
        /// Configuration of the preprocessor's input encoder
        /// </summary>
        public InputEncoderSettings InputEncoderCfg { get; }

        /// <summary>
        /// Configuration of reservoir structures
        /// </summary>
        public ReservoirStructuresSettings ReservoirStructuresCfg { get; }

        /// <summary>
        /// Configuration of reservoir instances
        /// </summary>
        public ReservoirInstancesSettings ReservoirInstancesCfg { get; }

        /// <summary>
        /// Determines how many predictors having smallest value-span to be disabled
        /// </summary>
        public double PredictorsReductionRatio { get; }

        /// <summary>
        /// Specifies minimum acceptable predictor's value-span
        /// </summary>
        public double PredictorValueMinSpan { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="inputEncoderCfg">Configuration of the preprocessor's input encoder</param>
        /// <param name="reservoirStructuresCfg">Configuration of reservoir structures</param>
        /// <param name="reservoirInstancesCfg">Configuration of reservoir instances</param>
        /// <param name="predictorsReductionRatio">Determines how many predictors having smallest value-span to be disabled</param>
        /// <param name="predictorValueMinSpan">Specifies minimum acceptable predictor's value-span</param>
        public NeuralPreprocessorSettings(InputEncoderSettings inputEncoderCfg,
                                          ReservoirStructuresSettings reservoirStructuresCfg,
                                          ReservoirInstancesSettings reservoirInstancesCfg,
                                          double predictorsReductionRatio = DefaultPredictorsReductionRatio,
                                          double predictorValueMinSpan = DefaultPredictorValueMinSpan
                                          )
        {
            InputEncoderCfg = (InputEncoderSettings)inputEncoderCfg.DeepClone();
            ReservoirStructuresCfg = (ReservoirStructuresSettings)reservoirStructuresCfg.DeepClone();
            ReservoirInstancesCfg = (ReservoirInstancesSettings)reservoirInstancesCfg.DeepClone();
            PredictorsReductionRatio = predictorsReductionRatio;
            PredictorValueMinSpan = predictorValueMinSpan;
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">Source instance</param>
        public NeuralPreprocessorSettings(NeuralPreprocessorSettings source)
            : this(source.InputEncoderCfg, source.ReservoirStructuresCfg, source.ReservoirInstancesCfg,
                   source.PredictorsReductionRatio, source.PredictorValueMinSpan)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance from the given xml element.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
        public NeuralPreprocessorSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            InputEncoderCfg = new InputEncoderSettings(settingsElem.Elements("inputEncoder").First());
            ReservoirStructuresCfg = new ReservoirStructuresSettings(settingsElem.Elements("reservoirStructures").First());
            ReservoirInstancesCfg = new ReservoirInstancesSettings(settingsElem.Elements("reservoirInstances").First());
            PredictorsReductionRatio = double.Parse(settingsElem.Attribute("predictorsReductionRatio").Value, CultureInfo.InvariantCulture);
            PredictorValueMinSpan = double.Parse(settingsElem.Attribute("predictorValueMinSpan").Value, CultureInfo.InvariantCulture);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultPredictorsReductionRatio { get { return (PredictorsReductionRatio == DefaultPredictorsReductionRatio); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultPredictorValueMinSpan { get { return (PredictorValueMinSpan == DefaultPredictorValueMinSpan); } }

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
            if (PredictorsReductionRatio < 0 || PredictorsReductionRatio >= 1)
            {
                throw new ArgumentException($"Invalid PredictorsReductionRatio {PredictorsReductionRatio.ToString(CultureInfo.InvariantCulture)}. PredictorsReductionRatio must be GE to 0 and LT 1.", "PredictorsReductionRatio");
            }
            if (PredictorValueMinSpan <= 0)
            {
                throw new ArgumentException($"Invalid PredictorValueMinSpan {PredictorValueMinSpan.ToString(CultureInfo.InvariantCulture)}. PredictorValueMinSpan must be GT 0.", "PredictorValueMinSpan");
            }
            //Reservoir instances consistency
            foreach (ReservoirInstanceSettings resInstCfg in ReservoirInstancesCfg.ReservoirInstanceCfgCollection)
            {
                resInstCfg.CheckConsistency(InputEncoderCfg, ReservoirStructuresCfg.GetReservoirStructureCfg(resInstCfg.StructureCfgName));
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
        /// <param name="suppressDefaults">Specifies whether to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName,
                                             InputEncoderCfg.GetXml(suppressDefaults),
                                             ReservoirStructuresCfg.GetXml(suppressDefaults),
                                             ReservoirInstancesCfg.GetXml(suppressDefaults)
                                             );
            if (!suppressDefaults || !IsDefaultPredictorsReductionRatio)
            {
                rootElem.Add(new XAttribute("predictorsReductionRatio", PredictorsReductionRatio.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultPredictorValueMinSpan)
            {
                rootElem.Add(new XAttribute("predictorValueMinSpan", PredictorValueMinSpan.ToString(CultureInfo.InvariantCulture)));
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
            return GetXml("neuralPreprocessor", suppressDefaults);
        }

    }//NeuralPreprocessorSettings

}//Namespace
