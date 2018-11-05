using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Xml.Linq;
using System.Reflection;
using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.XmlTools;
using RCNet.MathTools.Differential;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Class encaptulates arguments of the AdExpIF activation function
    /// </summary>
    [Serializable]
    public class AdExpIFSettings
    {
        //Attribute properties
        /// <summary>
        /// Input stimuli coefficient (pA)
        /// </summary>
        public double StimuliCoeff { get; set; }
        /// <summary>
        /// Membrane time scale (ms)
        /// </summary>
        public double TimeScale { get; set; }
        /// <summary>
        /// Membrane resistance (Mohm)
        /// </summary>
        public double Resistance { get; set; }
        /// <summary>
        /// Membrane rest potential (mV)
        /// </summary>
        public double RestV { get; set; }
        /// <summary>
        /// Membrane reset potential (mV)
        /// </summary>
        public double ResetV { get; set; }
        /// <summary>
        /// Membrane rheobase threshold (mV)
        /// </summary>
        public double RheobaseThresholdV { get; set; }
        /// <summary>
        /// Membrane firing threshold (mV)
        /// </summary>
        public double FiringThresholdV { get; set; }
        /// <summary>
        /// Sharpness of membrane potential change (mV)
        /// </summary>
        public double SharpnessDeltaT { get; set; }
        /// <summary>
        /// Adaptation voltage coupling (nS)
        /// </summary>
        public double AdaptationVoltageCoupling { get; set; }
        /// <summary>
        /// Adaptation time constant (ms)
        /// </summary>
        public double AdaptationTimeConstant { get; set; }
        /// <summary>
        /// Spike triggered adaptation increment (pA)
        /// </summary>
        public double SpikeTriggeredAdaptationIncrement { get; set; }
        /// <summary>
        /// ODE numerical solver method
        /// </summary>
        public ODENumSolver.Method SolverMethod { get; set; }
        /// <summary>
        /// ODE numerical solver computation steps of the time step 
        /// </summary>
        public int SolverCompSteps { get; set; }


        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="stimuliCoeff">Input stimuli coefficient (pA)</param>
        /// <param name="timeScale">Membrane time scale (ms)</param>
        /// <param name="resistance">Membrane resistance (Mohm)</param>
        /// <param name="restV">Membrane rest potential (mV)</param>
        /// <param name="resetV">Membrane reset potential (mV)</param>
        /// <param name="rheobaseThresholdV">Membrane rheobase threshold (mV)</param>
        /// <param name="firingThresholdV">Membrane firing threshold (mV)</param>
        /// <param name="sharpnessDeltaT">Sharpness of membrane potential change (mV)</param>
        /// <param name="adaptationVoltageCoupling">Adaptation voltage coupling (nS)</param>
        /// <param name="adaptationTimeConstant">Adaptation time constant (ms)</param>
        /// <param name="spikeTriggeredAdaptationIncrement">Spike triggered adaptation increment (pA)</param>
        /// <param name="solverMethod">ODE numerical solver method</param>
        /// <param name="solverCompSteps">ODE numerical solver computation steps of the time step</param>
        public AdExpIFSettings(double stimuliCoeff,
                               double timeScale,
                               double resistance,
                               double restV,
                               double resetV,
                               double rheobaseThresholdV,
                               double firingThresholdV,
                               double sharpnessDeltaT,
                               double adaptationVoltageCoupling,
                               double adaptationTimeConstant,
                               double spikeTriggeredAdaptationIncrement,
                               ODENumSolver.Method solverMethod,
                               int solverCompSteps
                               )
        {
            StimuliCoeff = stimuliCoeff;
            TimeScale = timeScale;
            Resistance = resistance;
            RestV = restV;
            ResetV = resetV;
            RheobaseThresholdV = rheobaseThresholdV;
            FiringThresholdV = firingThresholdV;
            SharpnessDeltaT = sharpnessDeltaT;
            AdaptationVoltageCoupling = adaptationVoltageCoupling;
            AdaptationTimeConstant = adaptationTimeConstant;
            SpikeTriggeredAdaptationIncrement = spikeTriggeredAdaptationIncrement;
            SolverMethod = solverMethod;
            SolverCompSteps = solverCompSteps;
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public AdExpIFSettings(AdExpIFSettings source)
        {
            StimuliCoeff = source.StimuliCoeff;
            TimeScale = source.TimeScale;
            Resistance = source.Resistance;
            RestV = source.RestV;
            ResetV = source.ResetV;
            RheobaseThresholdV = source.RheobaseThresholdV;
            FiringThresholdV = source.FiringThresholdV;
            SharpnessDeltaT = source.SharpnessDeltaT;
            AdaptationVoltageCoupling = source.AdaptationVoltageCoupling;
            AdaptationTimeConstant = source.AdaptationTimeConstant;
            SpikeTriggeredAdaptationIncrement = source.SpikeTriggeredAdaptationIncrement;
            SolverMethod = source.SolverMethod;
            SolverCompSteps = source.SolverCompSteps;
            return;
        }

        /// <summary>
        /// Creates an instance and initializes it from given xml element.
        /// </summary>
        /// <param name="elem">
        /// Xml data containing AdExpIF activation settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public AdExpIFSettings(XElement elem)
        {
            //Validation
            ElemValidator validator = new ElemValidator();
            Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.Neural.Activation.AdExpIFSettings.xsd");
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.RCNetTypes.xsd");
            XElement activationSettingsElem = validator.Validate(elem, "rootElem");
            //Parsing
            StimuliCoeff = double.Parse(activationSettingsElem.Attribute("stimuliCoeff").Value, CultureInfo.InvariantCulture);
            TimeScale = double.Parse(activationSettingsElem.Attribute("timeScale").Value, CultureInfo.InvariantCulture);
            Resistance = double.Parse(activationSettingsElem.Attribute("resistance").Value, CultureInfo.InvariantCulture);
            RestV = double.Parse(activationSettingsElem.Attribute("restV").Value, CultureInfo.InvariantCulture);
            ResetV = double.Parse(activationSettingsElem.Attribute("resetV").Value, CultureInfo.InvariantCulture);
            RheobaseThresholdV = double.Parse(activationSettingsElem.Attribute("rheobaseV").Value, CultureInfo.InvariantCulture);
            FiringThresholdV = double.Parse(activationSettingsElem.Attribute("firingThresholdV").Value, CultureInfo.InvariantCulture);
            SharpnessDeltaT = double.Parse(activationSettingsElem.Attribute("sharpnessDeltaT").Value, CultureInfo.InvariantCulture);
            AdaptationVoltageCoupling = double.Parse(activationSettingsElem.Attribute("adaptationVoltageCoupling").Value, CultureInfo.InvariantCulture);
            AdaptationTimeConstant = double.Parse(activationSettingsElem.Attribute("adaptationTimeConstant").Value, CultureInfo.InvariantCulture);
            SpikeTriggeredAdaptationIncrement = double.Parse(activationSettingsElem.Attribute("spikeTriggeredAdaptationIncrement").Value, CultureInfo.InvariantCulture);
            SolverMethod = ODENumSolver.ParseComputationMethodType(activationSettingsElem.Attribute("solverMethod").Value);
            SolverCompSteps = int.Parse(activationSettingsElem.Attribute("solverCompSteps").Value, CultureInfo.InvariantCulture);
            return;
        }

        //Methods
        /// <summary>
        /// See the base.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            AdExpIFSettings cmpSettings = obj as AdExpIFSettings;
            if (StimuliCoeff != cmpSettings.StimuliCoeff ||
                TimeScale != cmpSettings.TimeScale ||
                Resistance != cmpSettings.Resistance ||
                RestV != cmpSettings.RestV ||
                ResetV != cmpSettings.ResetV ||
                RheobaseThresholdV != cmpSettings.RheobaseThresholdV ||
                FiringThresholdV != cmpSettings.FiringThresholdV ||
                SharpnessDeltaT != cmpSettings.SharpnessDeltaT ||
                AdaptationVoltageCoupling != cmpSettings.AdaptationVoltageCoupling ||
                AdaptationTimeConstant != cmpSettings.AdaptationTimeConstant ||
                SpikeTriggeredAdaptationIncrement != cmpSettings.SpikeTriggeredAdaptationIncrement ||
                SolverMethod != cmpSettings.SolverMethod ||
                SolverCompSteps != cmpSettings.SolverCompSteps
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
        public AdExpIFSettings DeepClone()
        {
            AdExpIFSettings clone = new AdExpIFSettings(this);
            return clone;
        }

    }//AdExpIFSettings

}//Namespace
