using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Xml.Linq;
using System.Reflection;
using RCNet.Extensions;
using RCNet.RandomValue;
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
        //Constants
        //Typical values
        public const double TypicalTimeScale = 5;
        public const double TypicalResistance = 500;
        public const double TypicalRestV = -70;
        public const double TypicalResetV = -51;
        public const double TypicalRheobaseV = -50;
        public const double TypicalFiringThresholdV = -30;
        public const double TypicalSharpnessDeltaT = 2;
        public const double TypicalAdaptationVoltageCoupling = 0.5;
        public const double TypicalAdaptationTimeConstant = 100;
        public const double TypicalAdaptationSpikeTriggeredIncrement = 7;

        //Attribute properties
        /// <summary>
        /// Input stimuli coefficient (pA)
        /// </summary>
        public double StimuliCoeff { get; }
        /// <summary>
        /// Membrane time scale (ms)
        /// </summary>
        public RandomValueSettings TimeScale { get; }
        /// <summary>
        /// Membrane resistance (Mohm)
        /// </summary>
        public RandomValueSettings Resistance { get; }
        /// <summary>
        /// Membrane rest potential (mV)
        /// </summary>
        public RandomValueSettings RestV { get; }
        /// <summary>
        /// Membrane reset potential (mV)
        /// </summary>
        public RandomValueSettings ResetV { get; }
        /// <summary>
        /// Membrane rheobase threshold (mV)
        /// </summary>
        public RandomValueSettings RheobaseV { get; }
        /// <summary>
        /// Membrane firing threshold (mV)
        /// </summary>
        public RandomValueSettings FiringThresholdV { get; }
        /// <summary>
        /// Sharpness of membrane potential change (mV)
        /// </summary>
        public RandomValueSettings SharpnessDeltaT { get; }
        /// <summary>
        /// Adaptation voltage coupling (nS)
        /// </summary>
        public RandomValueSettings AdaptationVoltageCoupling { get; }
        /// <summary>
        /// Adaptation time constant (ms)
        /// </summary>
        public RandomValueSettings AdaptationTimeConstant { get; }
        /// <summary>
        /// Spike triggered adaptation increment (pA)
        /// </summary>
        public RandomValueSettings AdaptationSpikeTriggeredIncrement { get; }
        /// <summary>
        /// ODE numerical solver method
        /// </summary>
        public ODENumSolver.Method SolverMethod { get; }
        /// <summary>
        /// ODE numerical solver computation steps of the time step 
        /// </summary>
        public int SolverCompSteps { get; }


        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="stimuliCoeff">Input stimuli coefficient (pA)</param>
        /// <param name="timeScale">Membrane time scale (ms)</param>
        /// <param name="resistance">Membrane resistance (Mohm)</param>
        /// <param name="restV">Membrane rest potential (mV)</param>
        /// <param name="resetV">Membrane reset potential (mV)</param>
        /// <param name="rheobaseV">Membrane rheobase threshold (mV)</param>
        /// <param name="firingThresholdV">Membrane firing threshold (mV)</param>
        /// <param name="sharpnessDeltaT">Sharpness of membrane potential change (mV)</param>
        /// <param name="adaptationVoltageCoupling">Adaptation voltage coupling (nS)</param>
        /// <param name="adaptationTimeConstant">Adaptation time constant (ms)</param>
        /// <param name="adaptationSpikeTriggeredIncrement">Spike triggered adaptation increment (pA)</param>
        /// <param name="solverMethod">ODE numerical solver method</param>
        /// <param name="solverCompSteps">ODE numerical solver computation steps of the time step</param>
        public AdExpIFSettings(double stimuliCoeff,
                               RandomValueSettings timeScale,
                               RandomValueSettings resistance,
                               RandomValueSettings restV,
                               RandomValueSettings resetV,
                               RandomValueSettings rheobaseV,
                               RandomValueSettings firingThresholdV,
                               RandomValueSettings sharpnessDeltaT,
                               RandomValueSettings adaptationVoltageCoupling,
                               RandomValueSettings adaptationTimeConstant,
                               RandomValueSettings adaptationSpikeTriggeredIncrement,
                               ODENumSolver.Method solverMethod,
                               int solverCompSteps
                               )
        {
            StimuliCoeff = stimuliCoeff;
            TimeScale = timeScale.DeepClone();
            Resistance = resistance.DeepClone();
            RestV = restV.DeepClone();
            ResetV = resetV.DeepClone();
            RheobaseV = rheobaseV.DeepClone();
            FiringThresholdV = firingThresholdV.DeepClone();
            SharpnessDeltaT = sharpnessDeltaT.DeepClone();
            AdaptationVoltageCoupling = adaptationVoltageCoupling.DeepClone();
            AdaptationTimeConstant = adaptationTimeConstant.DeepClone();
            AdaptationSpikeTriggeredIncrement = adaptationSpikeTriggeredIncrement.DeepClone();
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
            TimeScale = source.TimeScale.DeepClone();
            Resistance = source.Resistance.DeepClone();
            RestV = source.RestV.DeepClone();
            ResetV = source.ResetV.DeepClone();
            RheobaseV = source.RheobaseV.DeepClone();
            FiringThresholdV = source.FiringThresholdV.DeepClone();
            SharpnessDeltaT = source.SharpnessDeltaT.DeepClone();
            AdaptationVoltageCoupling = source.AdaptationVoltageCoupling.DeepClone();
            AdaptationTimeConstant = source.AdaptationTimeConstant.DeepClone();
            AdaptationSpikeTriggeredIncrement = source.AdaptationSpikeTriggeredIncrement.DeepClone();
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
            TimeScale = new RandomValueSettings(activationSettingsElem.Descendants("timeScale").FirstOrDefault());
            Resistance = new RandomValueSettings(activationSettingsElem.Descendants("resistance").FirstOrDefault());
            RestV = new RandomValueSettings(activationSettingsElem.Descendants("restV").FirstOrDefault());
            ResetV = new RandomValueSettings(activationSettingsElem.Descendants("resetV").FirstOrDefault());
            RheobaseV = new RandomValueSettings(activationSettingsElem.Descendants("rheobaseV").FirstOrDefault());
            FiringThresholdV = new RandomValueSettings(activationSettingsElem.Descendants("firingThresholdV").FirstOrDefault());
            SharpnessDeltaT = new RandomValueSettings(activationSettingsElem.Descendants("sharpnessDeltaT").FirstOrDefault());
            AdaptationVoltageCoupling = new RandomValueSettings(activationSettingsElem.Descendants("adaptationVoltageCoupling").FirstOrDefault());
            AdaptationTimeConstant = new RandomValueSettings(activationSettingsElem.Descendants("adaptationTimeConstant").FirstOrDefault());
            AdaptationSpikeTriggeredIncrement = new RandomValueSettings(activationSettingsElem.Descendants("adaptationSpikeTriggeredIncrement").FirstOrDefault());
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
                !Equals(TimeScale, cmpSettings.TimeScale) ||
                !Equals(Resistance, cmpSettings.Resistance) ||
                !Equals(RestV, cmpSettings.RestV) ||
                !Equals(ResetV, cmpSettings.ResetV) ||
                !Equals(RheobaseV, cmpSettings.RheobaseV) ||
                !Equals(FiringThresholdV, cmpSettings.FiringThresholdV) ||
                !Equals(SharpnessDeltaT, cmpSettings.SharpnessDeltaT) ||
                !Equals(AdaptationVoltageCoupling, cmpSettings.AdaptationVoltageCoupling) ||
                !Equals(AdaptationTimeConstant, cmpSettings.AdaptationTimeConstant) ||
                !Equals(AdaptationSpikeTriggeredIncrement, cmpSettings.AdaptationSpikeTriggeredIncrement) ||
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
