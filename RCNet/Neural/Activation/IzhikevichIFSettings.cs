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
    /// Class encaptulates arguments of the IzhikevichIF activation function.
    /// Arguments are in RandomValue form to allow their dynamic random initialization within the specified ranges.
    /// </summary>
    [Serializable]
    public class IzhikevichIFSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "ActivationIzhikevichIFCfgType";

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
        /// <param name="recoveryTimeScale">Time scale of the recovery variable</param>
        /// <param name="recoverySensitivity">Sensitivity of the recovery variable to the subthreshold fluctuations of the membrane potential</param>
        /// <param name="recoveryReset">After-spike reset of the recovery variable</param>
        /// <param name="restV">Membrane rest potential (mV)</param>
        /// <param name="resetV">Membrane reset potential (mV)</param>
        /// <param name="firingThresholdV">Membrane firing threshold (mV)</param>
        /// <param name="refractoryPeriods">Number of after spike computation cycles while an input stimuli is ignored (ms)</param>
        /// <param name="solverMethod">ODE numerical solver method</param>
        /// <param name="solverCompSteps">ODE numerical solver computation steps of the time step</param>
        public IzhikevichIFSettings(RandomValueSettings recoveryTimeScale = null,
                                    RandomValueSettings recoverySensitivity = null,
                                    RandomValueSettings recoveryReset = null,
                                    RandomValueSettings restV = null,
                                    RandomValueSettings resetV = null,
                                    RandomValueSettings firingThresholdV = null,
                                    int refractoryPeriods = 1,
                                    ODENumSolver.Method solverMethod = ODENumSolver.Method.Euler,
                                    int solverCompSteps = 2
                                    )
        {
            RecoveryTimeScale = RandomValueSettings.CloneOrDefault(recoveryTimeScale, TypicalRecoveryTimeScale);
            RecoverySensitivity = RandomValueSettings.CloneOrDefault(recoverySensitivity, TypicalRecoverySensitivity);
            RecoveryReset = RandomValueSettings.CloneOrDefault(recoveryReset, TypicalRecoveryReset);
            RestV = RandomValueSettings.CloneOrDefault(restV, TypicalRestV);
            ResetV = RandomValueSettings.CloneOrDefault(resetV, TypicalResetV);
            FiringThresholdV = RandomValueSettings.CloneOrDefault(firingThresholdV, TypicalFiringThresholdV);
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
            XElement activationSettingsElem = Validate(elem, XsdTypeName);
            //Parsing
            RecoveryTimeScale = RandomValueSettings.LoadOrDefault(activationSettingsElem, "recoveryTimeScale", TypicalRecoveryTimeScale);
            RecoverySensitivity = RandomValueSettings.LoadOrDefault(activationSettingsElem, "recoverySensitivity", TypicalRecoverySensitivity);
            RecoveryReset = RandomValueSettings.LoadOrDefault(activationSettingsElem, "recoveryReset", TypicalRecoveryReset);
            RestV = RandomValueSettings.LoadOrDefault(activationSettingsElem, "restV", TypicalRestV);
            ResetV = RandomValueSettings.LoadOrDefault(activationSettingsElem, "resetV", TypicalResetV);
            FiringThresholdV = RandomValueSettings.LoadOrDefault(activationSettingsElem, "firingThresholdV", TypicalFiringThresholdV);
            RefractoryPeriods = int.Parse(activationSettingsElem.Attribute("refractoryPeriods").Value, CultureInfo.InvariantCulture);
            SolverMethod = ODENumSolver.ParseComputationMethodType(activationSettingsElem.Attribute("solverMethod").Value);
            SolverCompSteps = int.Parse(activationSettingsElem.Attribute("solverCompSteps").Value, CultureInfo.InvariantCulture);
            return;
        }

        //Methods
        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public IzhikevichIFSettings DeepClone()
        {
            return new IzhikevichIFSettings(this);
        }

    }//IzhikevichIFSettings

}//Namespace
