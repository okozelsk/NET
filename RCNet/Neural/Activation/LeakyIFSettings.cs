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
    /// Class encaptulates arguments of the LeakyIF activation function
    /// </summary>
    [Serializable]
    public class LeakyIFSettings
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
        /// Membrane firing threshold (mV)
        /// </summary>
        public double FiringThresholdV { get; set; }
        /// <summary>
        /// Number of after spike computation cycles while an input stimuli is ignored (ms)
        /// </summary>
        public int RefractoryPeriods { get; set; }
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
        /// <param name="firingThresholdV">Membrane firing threshold (mV)</param>
        /// <param name="refractoryPeriods">Number of after spike computation cycles while an input stimuli is ignored (ms)</param>
        /// <param name="solverMethod">ODE numerical solver method</param>
        /// <param name="solverCompSteps">ODE numerical solver computation steps of the time step</param>
        public LeakyIFSettings(double stimuliCoeff,
                             double timeScale,
                             double resistance,
                             double restV,
                             double resetV,
                             double firingThresholdV,
                             int refractoryPeriods,
                             ODENumSolver.Method solverMethod,
                             int solverCompSteps
                             )
        {
            StimuliCoeff = stimuliCoeff;
            TimeScale = timeScale;
            Resistance = resistance;
            RestV = restV;
            ResetV = resetV;
            FiringThresholdV = firingThresholdV;
            RefractoryPeriods = refractoryPeriods;
            SolverMethod = solverMethod;
            SolverCompSteps = solverCompSteps;
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public LeakyIFSettings(LeakyIFSettings source)
        {
            StimuliCoeff = source.StimuliCoeff;
            TimeScale = source.TimeScale;
            Resistance = source.Resistance;
            RestV = source.RestV;
            ResetV = source.ResetV;
            FiringThresholdV = source.FiringThresholdV;
            RefractoryPeriods = source.RefractoryPeriods;
            SolverMethod = source.SolverMethod;
            SolverCompSteps = source.SolverCompSteps;
            return;
        }

        /// <summary>
        /// Creates an instance and initializes it from given xml element.
        /// </summary>
        /// <param name="elem">
        /// Xml data containing LeakyIF activation settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public LeakyIFSettings(XElement elem)
        {
            //Validation
            ElemValidator validator = new ElemValidator();
            Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.Neural.Activation.LeakyIFSettings.xsd");
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.RCNetTypes.xsd");
            XElement activationSettingsElem = validator.Validate(elem, "rootElem");
            //Parsing
            StimuliCoeff = double.Parse(activationSettingsElem.Attribute("stimuliCoeff").Value, CultureInfo.InvariantCulture);
            TimeScale = double.Parse(activationSettingsElem.Attribute("timeScale").Value, CultureInfo.InvariantCulture);
            Resistance = double.Parse(activationSettingsElem.Attribute("resistance").Value, CultureInfo.InvariantCulture);
            RestV = double.Parse(activationSettingsElem.Attribute("restV").Value, CultureInfo.InvariantCulture);
            ResetV = double.Parse(activationSettingsElem.Attribute("resetV").Value, CultureInfo.InvariantCulture);
            FiringThresholdV = double.Parse(activationSettingsElem.Attribute("firingThresholdV").Value, CultureInfo.InvariantCulture);
            RefractoryPeriods = int.Parse(activationSettingsElem.Attribute("refractoryPeriods").Value, CultureInfo.InvariantCulture);
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
            LeakyIFSettings cmpSettings = obj as LeakyIFSettings;
            if (StimuliCoeff != cmpSettings.StimuliCoeff ||
                TimeScale != cmpSettings.TimeScale ||
                Resistance != cmpSettings.Resistance ||
                RestV != cmpSettings.RestV ||
                ResetV != cmpSettings.ResetV ||
                FiringThresholdV != cmpSettings.FiringThresholdV ||
                RefractoryPeriods != cmpSettings.RefractoryPeriods ||
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
        public LeakyIFSettings DeepClone()
        {
            LeakyIFSettings clone = new LeakyIFSettings(this);
            return clone;
        }

    }//LeakyIFSettings

}//Namespace
