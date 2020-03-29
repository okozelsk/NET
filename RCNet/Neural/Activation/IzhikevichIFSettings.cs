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
        public const string XsdTypeName = "ActivationIzhikevichIFType";

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
        public URandomValueSettings RecoveryTimeScale { get; }
        /// <summary>
        /// Dimensionless. Describes the sensitivity of the recovery variable to the subthreshold fluctuations of the membrane potential.
        /// (parameter b in original model)
        /// </summary>
        public URandomValueSettings RecoverySensitivity { get; }
        /// <summary>
        /// Dimensionless. Describes after-spike reset of the recovery variable.
        /// (parameter d in original model)
        /// </summary>
        public URandomValueSettings RecoveryReset { get; }
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
        public IzhikevichIFSettings(URandomValueSettings recoveryTimeScale = null,
                                    URandomValueSettings recoverySensitivity = null,
                                    URandomValueSettings recoveryReset = null,
                                    RandomValueSettings restV = null,
                                    RandomValueSettings resetV = null,
                                    RandomValueSettings firingThresholdV = null,
                                    int refractoryPeriods = ActivationFactory.DefaultRefractoryPeriods,
                                    ODENumSolver.Method solverMethod = ActivationFactory.DefaultSolverMethod,
                                    int solverCompSteps = ActivationFactory.DefaultSolverCompSteps
                                    )
        {
            RecoveryTimeScale = URandomValueSettings.CloneOrDefault(recoveryTimeScale, TypicalRecoveryTimeScale);
            RecoverySensitivity = URandomValueSettings.CloneOrDefault(recoverySensitivity, TypicalRecoverySensitivity);
            RecoveryReset = URandomValueSettings.CloneOrDefault(recoveryReset, TypicalRecoveryReset);
            RestV = RandomValueSettings.CloneOrDefault(restV, TypicalRestV);
            ResetV = RandomValueSettings.CloneOrDefault(resetV, TypicalResetV);
            FiringThresholdV = RandomValueSettings.CloneOrDefault(firingThresholdV, TypicalFiringThresholdV);
            RefractoryPeriods = refractoryPeriods;
            SolverMethod = solverMethod;
            SolverCompSteps = solverCompSteps;
            Check();
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public IzhikevichIFSettings(IzhikevichIFSettings source)
        {
            RecoveryTimeScale = (URandomValueSettings)source.RecoveryTimeScale.DeepClone();
            RecoverySensitivity = (URandomValueSettings)source.RecoverySensitivity.DeepClone();
            RecoveryReset = (URandomValueSettings)source.RecoveryReset.DeepClone();
            RestV = (RandomValueSettings)source.RestV.DeepClone();
            ResetV = (RandomValueSettings)source.ResetV.DeepClone();
            FiringThresholdV = (RandomValueSettings)source.FiringThresholdV.DeepClone();
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
            RecoveryTimeScale = URandomValueSettings.LoadOrDefault(activationSettingsElem, "recoveryTimeScale", TypicalRecoveryTimeScale);
            RecoverySensitivity = URandomValueSettings.LoadOrDefault(activationSettingsElem, "recoverySensitivity", TypicalRecoverySensitivity);
            RecoveryReset = URandomValueSettings.LoadOrDefault(activationSettingsElem, "recoveryReset", TypicalRecoveryReset);
            RestV = RandomValueSettings.LoadOrDefault(activationSettingsElem, "restV", TypicalRestV);
            ResetV = RandomValueSettings.LoadOrDefault(activationSettingsElem, "resetV", TypicalResetV);
            FiringThresholdV = RandomValueSettings.LoadOrDefault(activationSettingsElem, "firingThresholdV", TypicalFiringThresholdV);
            RefractoryPeriods = int.Parse(activationSettingsElem.Attribute("refractoryPeriods").Value, CultureInfo.InvariantCulture);
            SolverMethod = (ODENumSolver.Method)Enum.Parse(typeof(ODENumSolver.Method), activationSettingsElem.Attribute("solverMethod").Value, true);
            SolverCompSteps = int.Parse(activationSettingsElem.Attribute("solverCompSteps").Value, CultureInfo.InvariantCulture);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultRecoveryTimeScale { get { return (RecoveryTimeScale.Min == TypicalRecoveryTimeScale && RecoveryTimeScale.Max == TypicalRecoveryTimeScale && RecoveryTimeScale.DistrType == RandomCommon.DistributionType.Uniform); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultRecoverySensitivity { get { return (RecoverySensitivity.Min == TypicalRecoverySensitivity && RecoverySensitivity.Max == TypicalRecoverySensitivity && RecoverySensitivity.DistrType == RandomCommon.DistributionType.Uniform); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultRecoveryReset { get { return (RecoveryReset.Min == TypicalRecoveryReset && RecoveryReset.Max == TypicalRecoveryReset && RecoveryReset.DistrType == RandomCommon.DistributionType.Uniform); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultRestV { get { return (RestV.Min == TypicalRestV && RestV.Max == TypicalRestV && !RestV.RandomSign && RestV.DistrType == RandomCommon.DistributionType.Uniform); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultResetV { get { return (ResetV.Min == TypicalResetV && ResetV.Max == TypicalResetV && !ResetV.RandomSign && ResetV.DistrType == RandomCommon.DistributionType.Uniform); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultFiringThresholdV { get { return (FiringThresholdV.Min == TypicalFiringThresholdV && FiringThresholdV.Max == TypicalFiringThresholdV && !FiringThresholdV.RandomSign && FiringThresholdV.DistrType == RandomCommon.DistributionType.Uniform); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultRefractoryPeriods { get { return (RefractoryPeriods == ActivationFactory.DefaultRefractoryPeriods); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultSolverMethod { get { return (SolverMethod == ActivationFactory.DefaultSolverMethod); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultSolverCompSteps { get { return (SolverCompSteps == ActivationFactory.DefaultSolverCompSteps); } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return IsDefaultRecoveryTimeScale &&
                       IsDefaultRecoverySensitivity &&
                       IsDefaultRecoveryReset &&
                       IsDefaultRestV &&
                       IsDefaultResetV &&
                       IsDefaultFiringThresholdV &&
                       IsDefaultRefractoryPeriods &&
                       IsDefaultSolverMethod &&
                       IsDefaultSolverCompSteps;
            }
        }

        //Methods
        /// <summary>
        /// Checks validity
        /// </summary>
        private void Check()
        {
            if (RefractoryPeriods < 0)
            {
                throw new Exception($"Invalid RefractoryPeriods {RefractoryPeriods.ToString(CultureInfo.InvariantCulture)}. RefractoryPeriods must be GE to 0.");
            }
            if (SolverCompSteps < 1)
            {
                throw new Exception($"Invalid SolverCompSteps {SolverCompSteps.ToString(CultureInfo.InvariantCulture)}. SolverCompSteps must be GE to 1.");
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new IzhikevichIFSettings(this);
        }

        /// <summary>
        /// Generates xml element containing the settings.
        /// </summary>
        /// <param name="rootElemName">Name to be used as a name of the root element.</param>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            if (!suppressDefaults || !IsDefaultRefractoryPeriods)
            {
                rootElem.Add(new XAttribute("refractoryPeriods", RefractoryPeriods.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultSolverMethod)
            {
                rootElem.Add(new XAttribute("solverMethod", SolverMethod.ToString()));
            }
            if (!suppressDefaults || !IsDefaultSolverCompSteps)
            {
                rootElem.Add(new XAttribute("solverCompSteps", SolverCompSteps.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultRecoveryTimeScale)
            {
                rootElem.Add(RecoveryTimeScale.GetXml("recoveryTimeScale", suppressDefaults));
            }
            if (!suppressDefaults || !IsDefaultRecoverySensitivity)
            {
                rootElem.Add(RecoverySensitivity.GetXml("recoverySensitivity", suppressDefaults));
            }
            if (!suppressDefaults || !IsDefaultRecoveryReset)
            {
                rootElem.Add(RecoveryReset.GetXml("recoveryReset", suppressDefaults));
            }
            if (!suppressDefaults || !IsDefaultRestV)
            {
                rootElem.Add(RestV.GetXml("restV", suppressDefaults));
            }
            if (!suppressDefaults || !IsDefaultResetV)
            {
                rootElem.Add(ResetV.GetXml("resetV", suppressDefaults));
            }
            if (!suppressDefaults || !IsDefaultFiringThresholdV)
            {
                rootElem.Add(FiringThresholdV.GetXml("firingThresholdV", suppressDefaults));
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
            return GetXml("activationIzhikevichIF", suppressDefaults);
        }

    }//IzhikevichIFSettings

}//Namespace
