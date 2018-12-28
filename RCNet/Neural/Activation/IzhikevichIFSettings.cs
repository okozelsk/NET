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
    /// Class encaptulates arguments of the IzhikevichIF activation function
    /// </summary>
    [Serializable]
    public class IzhikevichIFSettings
    {
        //Constants
        //Typical values
        /// <summary>
        /// Typical value of the parameter "a" in the original Izhikevich model
        /// </summary>
        public const double TypicalRecoveryTimeScale = 0.02;
        /// <summary>
        /// Typical value of the parameter "b" in the original Izhikevich model
        /// </summary>
        public const double TypicalRecoverySensitivity = 0.2;
        /// <summary>
        /// Typical value of the parameter "d" in the original Izhikevich model
        /// </summary>
        public const double TypicalRecoveryReset = 2;
        /// <summary>
        /// Typical value of the membrane resting potential
        /// </summary>
        public const double TypicalRestV = -70;
        /// <summary>
        /// Typical value of the parameter "c" in the original Izhikevich model
        /// </summary>
        public const double TypicalResetV = -65;
        /// <summary>
        /// Typical value of the membrane firing treshold
        /// </summary>
        public const double TypicalFiringThresholdV = 30;

        //Attribute properties
        /// <summary>
        /// Input stimuli coefficient (pA)
        /// </summary>
        public double StimuliCoeff { get; }
        /// <summary>
        /// Dimensionless. Describes the time scale of the recovery variable. Smaller values result in slower recovery.
        /// (parameter a in original model)
        /// </summary>
        public RandomValueSettings RecoveryTimeScale { get; }
        /// <summary>
        /// Dimensionless. Describes the sensitivity of the recovery variable to the subthreshold fluctuations of the membrane potential.
        /// (parameter b in original model)
        /// </summary>
        public RandomValueSettings RecoverySensitivity { get; }
        /// <summary>
        /// Dimensionless. Describes after-spike reset of the recovery variable.
        /// (parameter d in original model)
        /// </summary>
        public RandomValueSettings RecoveryReset { get; }
        /// <summary>
        /// Membrane rest potential (mV)
        /// </summary>
        public RandomValueSettings RestV { get; }
        /// <summary>
        /// Membrane reset potential (mV)
        /// (parameter c in original model)
        /// </summary>
        public RandomValueSettings ResetV { get; }
        /// <summary>
        /// Membrane firing threshold (mV)
        /// </summary>
        public RandomValueSettings FiringThresholdV { get; }
        /// <summary>
        /// Number of after spike computation cycles while an input stimuli is ignored (ms)
        /// </summary>
        public int RefractoryPeriods { get; }
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
        /// <param name="recoveryTimeScale">Time scale of the recovery variable</param>
        /// <param name="recoverySensitivity">Sensitivity of the recovery variable to the subthreshold fluctuations of the membrane potential</param>
        /// <param name="recoveryReset">After-spike reset of the recovery variable</param>
        /// <param name="restV">Membrane rest potential (mV)</param>
        /// <param name="resetV">Membrane reset potential (mV)</param>
        /// <param name="firingThresholdV">Membrane firing threshold (mV)</param>
        /// <param name="refractoryPeriods">Number of after spike computation cycles while an input stimuli is ignored (ms)</param>
        /// <param name="solverMethod">ODE numerical solver method</param>
        /// <param name="solverCompSteps">ODE numerical solver computation steps of the time step</param>
        public IzhikevichIFSettings(double stimuliCoeff,
                                    RandomValueSettings recoveryTimeScale,
                                    RandomValueSettings recoverySensitivity,
                                    RandomValueSettings recoveryReset,
                                    RandomValueSettings restV,
                                    RandomValueSettings resetV,
                                    RandomValueSettings firingThresholdV,
                                    int refractoryPeriods,
                                    ODENumSolver.Method solverMethod,
                                    int solverCompSteps
                                    )
        {
            StimuliCoeff = stimuliCoeff;
            RecoveryTimeScale = recoveryTimeScale.DeepClone();
            RecoverySensitivity = recoverySensitivity.DeepClone();
            RecoveryReset = recoveryReset.DeepClone();
            RestV = restV.DeepClone();
            ResetV = resetV.DeepClone();
            FiringThresholdV = firingThresholdV.DeepClone();
            RefractoryPeriods = refractoryPeriods;
            SolverMethod = solverMethod;
            SolverCompSteps = solverCompSteps;
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public IzhikevichIFSettings(IzhikevichIFSettings source)
        {
            StimuliCoeff = source.StimuliCoeff;
            RecoveryTimeScale = source.RecoveryTimeScale.DeepClone();
            RecoverySensitivity = source.RecoverySensitivity.DeepClone();
            RecoveryReset = source.RecoveryReset.DeepClone();
            RestV = source.RestV.DeepClone();
            ResetV = source.ResetV.DeepClone();
            FiringThresholdV = source.FiringThresholdV.DeepClone();
            RefractoryPeriods = source.RefractoryPeriods;
            SolverMethod = source.SolverMethod;
            SolverCompSteps = source.SolverCompSteps;
            return;
        }

        /// <summary>
        /// Creates an instance and initializes it from given xml element.
        /// </summary>
        /// <param name="elem">
        /// Xml data containing IzhikevichIF activation settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public IzhikevichIFSettings(XElement elem)
        {
            //Validation
            ElemValidator validator = new ElemValidator();
            Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.Neural.Activation.IzhikevichIFSettings.xsd");
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.RCNetTypes.xsd");
            XElement activationSettingsElem = validator.Validate(elem, "rootElem");
            //Parsing
            StimuliCoeff = double.Parse(activationSettingsElem.Attribute("stimuliCoeff").Value, CultureInfo.InvariantCulture);
            RecoveryTimeScale = new RandomValueSettings(activationSettingsElem.Descendants("recoveryTimeScale").FirstOrDefault());
            RecoverySensitivity = new RandomValueSettings(activationSettingsElem.Descendants("recoverySensitivity").FirstOrDefault());
            RecoveryReset = new RandomValueSettings(activationSettingsElem.Descendants("recoveryReset").FirstOrDefault());
            RestV = new RandomValueSettings(activationSettingsElem.Descendants("restV").FirstOrDefault());
            ResetV = new RandomValueSettings(activationSettingsElem.Descendants("resetV").FirstOrDefault());
            FiringThresholdV = new RandomValueSettings(activationSettingsElem.Descendants("firingThresholdV").FirstOrDefault());
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
            IzhikevichIFSettings cmpSettings = obj as IzhikevichIFSettings;
            if (StimuliCoeff != cmpSettings.StimuliCoeff ||
                !Equals(RecoveryTimeScale, cmpSettings.RecoveryTimeScale) ||
                !Equals(RecoverySensitivity, cmpSettings.RecoverySensitivity) ||
                !Equals(RecoveryReset, cmpSettings.RecoveryReset) ||
                !Equals(RestV, cmpSettings.RestV) ||
                !Equals(ResetV, cmpSettings.ResetV) ||
                !Equals(FiringThresholdV, cmpSettings.FiringThresholdV) ||
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
        public IzhikevichIFSettings DeepClone()
        {
            IzhikevichIFSettings clone = new IzhikevichIFSettings(this);
            return clone;
        }

    }//IzhikevichIFSettings

}//Namespace
