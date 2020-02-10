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
    /// Class encaptulates arguments of the AdExpIF activation function.
    /// Arguments are in RandomValue form to allow their dynamic random initialization within the specified ranges.
    /// </summary>
    [Serializable]
    public class AdExpIFSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "ActivationAdExpIFCfgType";

        //Typical values
        /// <summary>
        /// Typical value of time scale
        /// </summary>
        public const double TypicalTimeScale = 5;
        /// <summary>
        /// Typical value of resistance
        /// </summary>
        public const double TypicalResistance = 500;
        /// <summary>
        /// Typical value of resting voltage
        /// </summary>
        public const double TypicalRestV = -70;
        /// <summary>
        /// Typical value of reset voltage
        /// </summary>
        public const double TypicalResetV = -51;
        /// <summary>
        /// Typical value of rheobase
        /// </summary>
        public const double TypicalRheobaseV = -50;
        /// <summary>
        /// Typical value of firing voltage
        /// </summary>
        public const double TypicalFiringThresholdV = -30;
        /// <summary>
        /// Typical value of sharpness delta
        /// </summary>
        public const double TypicalSharpnessDeltaT = 2;
        /// <summary>
        /// Typical value of adaptation voltage coupling
        /// </summary>
        public const double TypicalAdaptationVoltageCoupling = 0.5;
        /// <summary>
        /// Typical value of adaptation time constant
        /// </summary>
        public const double TypicalAdaptationTimeConstant = 100;
        /// <summary>
        /// Typical value of spike triggered increment
        /// </summary>
        public const double TypicalAdaptationSpikeTriggeredIncrement = 7;


        //Attribute properties
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
        public AdExpIFSettings(RandomValueSettings timeScale = null,
                               RandomValueSettings resistance = null,
                               RandomValueSettings restV = null,
                               RandomValueSettings resetV = null,
                               RandomValueSettings rheobaseV = null,
                               RandomValueSettings firingThresholdV = null,
                               RandomValueSettings sharpnessDeltaT = null,
                               RandomValueSettings adaptationVoltageCoupling = null,
                               RandomValueSettings adaptationTimeConstant = null,
                               RandomValueSettings adaptationSpikeTriggeredIncrement = null,
                               ODENumSolver.Method solverMethod = ODENumSolver.Method.Euler,
                               int solverCompSteps = 2
                               )
        {
            TimeScale = RandomValueSettings.CloneOrDefault(timeScale, TypicalTimeScale);
            Resistance = RandomValueSettings.CloneOrDefault(resistance, TypicalResistance);
            RestV = RandomValueSettings.CloneOrDefault(restV, TypicalRestV);
            ResetV = RandomValueSettings.CloneOrDefault(resetV, TypicalResetV);
            RheobaseV = RandomValueSettings.CloneOrDefault(rheobaseV, TypicalRheobaseV);
            FiringThresholdV = RandomValueSettings.CloneOrDefault(firingThresholdV, TypicalFiringThresholdV);
            SharpnessDeltaT = RandomValueSettings.CloneOrDefault(sharpnessDeltaT, TypicalSharpnessDeltaT);
            AdaptationVoltageCoupling = RandomValueSettings.CloneOrDefault(adaptationVoltageCoupling, TypicalAdaptationVoltageCoupling);
            AdaptationTimeConstant = RandomValueSettings.CloneOrDefault(adaptationTimeConstant, TypicalAdaptationTimeConstant);
            AdaptationSpikeTriggeredIncrement = RandomValueSettings.CloneOrDefault(adaptationSpikeTriggeredIncrement, TypicalAdaptationSpikeTriggeredIncrement);
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
            XElement activationSettingsElem = Validate(elem, XsdTypeName);
            //Parsing
            TimeScale = RandomValueSettings.LoadOrDefault(activationSettingsElem, "timeScale", TypicalTimeScale);
            Resistance = RandomValueSettings.LoadOrDefault(activationSettingsElem, "resistance", TypicalResistance);
            RestV = RandomValueSettings.LoadOrDefault(activationSettingsElem, "restV", TypicalRestV);
            ResetV = RandomValueSettings.LoadOrDefault(activationSettingsElem, "resetV", TypicalResetV);
            RheobaseV = RandomValueSettings.LoadOrDefault(activationSettingsElem, "rheobaseV", TypicalRheobaseV);
            FiringThresholdV = RandomValueSettings.LoadOrDefault(activationSettingsElem, "firingThresholdV", TypicalFiringThresholdV);
            SharpnessDeltaT = RandomValueSettings.LoadOrDefault(activationSettingsElem, "sharpnessDeltaT", TypicalSharpnessDeltaT);
            AdaptationVoltageCoupling = RandomValueSettings.LoadOrDefault(activationSettingsElem, "adaptationVoltageCoupling", TypicalAdaptationVoltageCoupling);
            AdaptationTimeConstant = RandomValueSettings.LoadOrDefault(activationSettingsElem, "adaptationTimeConstant", TypicalAdaptationTimeConstant);
            AdaptationSpikeTriggeredIncrement = RandomValueSettings.LoadOrDefault(activationSettingsElem, "adaptationSpikeTriggeredIncrement", TypicalAdaptationSpikeTriggeredIncrement);
            SolverMethod = ODENumSolver.ParseComputationMethodType(activationSettingsElem.Attribute("solverMethod").Value);
            SolverCompSteps = int.Parse(activationSettingsElem.Attribute("solverCompSteps").Value, CultureInfo.InvariantCulture);
            return;
        }

        //Methods
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
