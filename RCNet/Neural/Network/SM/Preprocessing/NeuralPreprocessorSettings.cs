﻿using RCNet.Neural.Network.SM.Preprocessing.Input;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir;
using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing
{
    /// <summary>
    /// Configuration of the neural preprocessor.
    /// </summary>
    [Serializable]
    public class NeuralPreprocessorSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "NPType";
        //Default values
        /// <summary>
        /// The default value of the parameter determining how many predictors having smallest value-span to be disabled.
        /// </summary>
        public const double DefaultPredictorsReductionRatio = 0d;
        /// <summary>
        /// The default value of the parameter specifying the minimum acceptable predictor's value-span.
        /// </summary>
        public const double DefaultPredictorValueMinSpan = 1e-6d;

        //Attribute properties
        /// <summary>
        /// The configuration of the input encoder.
        /// </summary>
        public InputEncoderSettings InputEncoderCfg { get; }

        /// <summary>
        /// The configuration of the reservoir structures.
        /// </summary>
        public ReservoirStructuresSettings ReservoirStructuresCfg { get; }

        /// <summary>
        /// The configuration of the reservoir instances.
        /// </summary>
        public ReservoirInstancesSettings ReservoirInstancesCfg { get; }

        /// <summary>
        /// Determines how many predictors having smallest value-span to be disabled.
        /// </summary>
        public double PredictorsReductionRatio { get; }

        /// <summary>
        /// Specifies the minimum acceptable predictor's value-span.
        /// </summary>
        public double PredictorValueMinSpan { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="inputEncoderCfg">The configuration of the input encoder.</param>
        /// <param name="reservoirStructuresCfg">The configuration of the reservoir structures.</param>
        /// <param name="reservoirInstancesCfg">The configuration of the reservoir instances.</param>
        /// <param name="predictorsReductionRatio">Determines how many predictors having smallest value-span to be disabled.</param>
        /// <param name="predictorValueMinSpan">Specifies the minimum acceptable predictor's value-span.</param>
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
        /// <param name="source">The source instance.</param>
        public NeuralPreprocessorSettings(NeuralPreprocessorSettings source)
            : this(source.InputEncoderCfg, source.ReservoirStructuresCfg, source.ReservoirInstancesCfg,
                   source.PredictorsReductionRatio, source.PredictorValueMinSpan)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
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
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultPredictorsReductionRatio { get { return (PredictorsReductionRatio == DefaultPredictorsReductionRatio); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultPredictorValueMinSpan { get { return (PredictorValueMinSpan == DefaultPredictorValueMinSpan); } }

        /// <inheritdoc />
        public override bool ContainsOnlyDefaults { get { return false; } }

        //Methods
        /// <inheritdoc />
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

        /// <inheritdoc />
        public override RCNetBaseSettings DeepClone()
        {
            return new NeuralPreprocessorSettings(this);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("neuralPreprocessor", suppressDefaults);
        }

    }//NeuralPreprocessorSettings

}//Namespace
