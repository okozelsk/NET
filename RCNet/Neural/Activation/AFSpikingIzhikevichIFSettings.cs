using RCNet.MathTools.Differential;
using RCNet.RandomValue;
using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Configuration of the AFSpikingIzhikevichIF activation function.
    /// </summary>
    [Serializable]
    public class AFSpikingIzhikevichIFSettings : RCNetBaseSettings, IActivationSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "ActivationIzhikevichIFType";

        //Typical values
        /// <summary>
        /// The typical value of the parameter "a" in the original Izhikevich model.
        /// </summary>
        public const double TypicalRecoveryTimeScale = 0.02;
        /// <summary>
        /// The typical value of the parameter "b" in the original Izhikevich model.
        /// </summary>
        public const double TypicalRecoverySensitivity = 0.2;
        /// <summary>
        /// The typical value of the parameter "d" in the original Izhikevich model.
        /// </summary>
        public const double TypicalRecoveryReset = 2;
        /// <summary>
        /// The typical value of the membrane resting potential.
        /// </summary>
        public const double TypicalRestV = -70;
        /// <summary>
        /// The typical value of the parameter "c" in the original Izhikevich model.
        /// </summary>
        public const double TypicalResetV = -65;
        /// <summary>
        /// The typical value of the membrane firing threshold.
        /// </summary>
        public const double TypicalFiringThresholdV = 30;

        //Attribute properties
        /// <summary>
        /// The dimensionless parameter "a" in the original Izhikevich model. Describes the time scale of the recovery variable. Smaller values result in slower recovery.
        /// </summary>
        public URandomValueSettings RecoveryTimeScale { get; }
        /// <summary>
        /// The dimensionless parameter "b" in the original Izhikevich model. Describes the sensitivity of the recovery variable to the subthreshold fluctuations of the membrane potential.
        /// </summary>
        public URandomValueSettings RecoverySensitivity { get; }
        /// <summary>
        /// The dimensionless parameter "d" in the original Izhikevich model. Describes after-spike reset of the recovery variable.
        /// </summary>
        public URandomValueSettings RecoveryReset { get; }
        /// <summary>
        /// The membrane rest potential (mV).
        /// </summary>
        public RandomValueSettings RestV { get; }
        /// <summary>
        /// The membrane reset potential (mV). The parameter "c" in the original Izhikevich model.
        /// </summary>
        public RandomValueSettings ResetV { get; }
        /// <summary>
        /// The membrane firing threshold (mV).
        /// </summary>
        public RandomValueSettings FiringThresholdV { get; }
        /// <summary>
        /// The number of after-spike computation cycles while an input stimuli to be ignored (cycles).
        /// </summary>
        public int RefractoryPeriods { get; }
        /// <summary>
        /// The ODE numerical solver method.
        /// </summary>
        public ODENumSolver.Method SolverMethod { get; }
        /// <summary>
        /// The number of computation sub-steps of the ODE numerical solver.
        /// </summary>
        public int SolverCompSteps { get; }
        /// <summary>
        /// The duration of the membrane stimulation (ms).
        /// </summary>
        public double StimuliDuration { get; }


        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <remarks>
        /// The arguments are in RandomValue form to allow their dynamic random initialization within the specified ranges.
        /// </remarks>
        /// <param name="recoveryTimeScale">The time scale of the recovery variable.</param>
        /// <param name="recoverySensitivity">The sensitivity of the recovery variable to the sub-threshold fluctuations of the membrane potential.</param>
        /// <param name="recoveryReset">The after-spike reset of the recovery variable.</param>
        /// <param name="restV">The membrane rest potential (mV).</param>
        /// <param name="resetV">The membrane reset potential (mV).</param>
        /// <param name="firingThresholdV">The membrane firing threshold (mV).</param>
        /// <param name="refractoryPeriods">The number of after-spike computation cycles while an input stimuli to be ignored (cycles).</param>
        /// <param name="solverMethod">The ODE numerical solver method to be used.</param>
        /// <param name="solverCompSteps">The number of computation sub-steps of the ODE numerical solver.</param>
        /// <param name="stimuliDuration">The duration of the membrane stimulation (ms).</param>
        public AFSpikingIzhikevichIFSettings(URandomValueSettings recoveryTimeScale = null,
                                             URandomValueSettings recoverySensitivity = null,
                                             URandomValueSettings recoveryReset = null,
                                             RandomValueSettings restV = null,
                                             RandomValueSettings resetV = null,
                                             RandomValueSettings firingThresholdV = null,
                                             int refractoryPeriods = ActivationFactory.DefaultRefractoryPeriods,
                                             ODENumSolver.Method solverMethod = ActivationFactory.DefaultSolverMethod,
                                             int solverCompSteps = ActivationFactory.DefaultSolverCompSteps,
                                             double stimuliDuration = ActivationFactory.DefaultStimuliDuration
                                             )
        {
            RecoveryTimeScale = URandomValueSettings.CloneOrCreate(recoveryTimeScale, TypicalRecoveryTimeScale);
            RecoverySensitivity = URandomValueSettings.CloneOrCreate(recoverySensitivity, TypicalRecoverySensitivity);
            RecoveryReset = URandomValueSettings.CloneOrCreate(recoveryReset, TypicalRecoveryReset);
            RestV = RandomValueSettings.CloneOrCreate(restV, TypicalRestV);
            ResetV = RandomValueSettings.CloneOrCreate(resetV, TypicalResetV);
            FiringThresholdV = RandomValueSettings.CloneOrCreate(firingThresholdV, TypicalFiringThresholdV);
            RefractoryPeriods = refractoryPeriods;
            SolverMethod = solverMethod;
            SolverCompSteps = solverCompSteps;
            StimuliDuration = stimuliDuration;
            Check();
            return;
        }

        /// <summary>
        /// The copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public AFSpikingIzhikevichIFSettings(AFSpikingIzhikevichIFSettings source)
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
            StimuliDuration = source.StimuliDuration;
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public AFSpikingIzhikevichIFSettings(XElement elem)
        {
            //Validation
            XElement activationSettingsElem = Validate(elem, XsdTypeName);
            //Parsing
            RecoveryTimeScale = URandomValueSettings.LoadOrCreate(activationSettingsElem, "recoveryTimeScale", TypicalRecoveryTimeScale);
            RecoverySensitivity = URandomValueSettings.LoadOrCreate(activationSettingsElem, "recoverySensitivity", TypicalRecoverySensitivity);
            RecoveryReset = URandomValueSettings.LoadOrCreate(activationSettingsElem, "recoveryReset", TypicalRecoveryReset);
            RestV = RandomValueSettings.LoadOrCreate(activationSettingsElem, "restV", TypicalRestV);
            ResetV = RandomValueSettings.LoadOrCreate(activationSettingsElem, "resetV", TypicalResetV);
            FiringThresholdV = RandomValueSettings.LoadOrCreate(activationSettingsElem, "firingThresholdV", TypicalFiringThresholdV);
            RefractoryPeriods = int.Parse(activationSettingsElem.Attribute("refractoryPeriods").Value, CultureInfo.InvariantCulture);
            SolverMethod = (ODENumSolver.Method)Enum.Parse(typeof(ODENumSolver.Method), activationSettingsElem.Attribute("solverMethod").Value, true);
            SolverCompSteps = int.Parse(activationSettingsElem.Attribute("solverCompSteps").Value, CultureInfo.InvariantCulture);
            StimuliDuration = double.Parse(activationSettingsElem.Attribute("stimuliDuration").Value, CultureInfo.InvariantCulture);
            Check();
            return;
        }

        //Properties
        /// <inheritdoc/>
        public ActivationType TypeOfActivation { get { return ActivationType.Spiking; } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultRecoveryTimeScale { get { return (RecoveryTimeScale.Min == TypicalRecoveryTimeScale && RecoveryTimeScale.Max == TypicalRecoveryTimeScale && RecoveryTimeScale.DistrType == RandomCommon.DistributionType.Uniform); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultRecoverySensitivity { get { return (RecoverySensitivity.Min == TypicalRecoverySensitivity && RecoverySensitivity.Max == TypicalRecoverySensitivity && RecoverySensitivity.DistrType == RandomCommon.DistributionType.Uniform); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultRecoveryReset { get { return (RecoveryReset.Min == TypicalRecoveryReset && RecoveryReset.Max == TypicalRecoveryReset && RecoveryReset.DistrType == RandomCommon.DistributionType.Uniform); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultRestV { get { return (RestV.Min == TypicalRestV && RestV.Max == TypicalRestV && !RestV.RandomSign && RestV.DistrType == RandomCommon.DistributionType.Uniform); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultResetV { get { return (ResetV.Min == TypicalResetV && ResetV.Max == TypicalResetV && !ResetV.RandomSign && ResetV.DistrType == RandomCommon.DistributionType.Uniform); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultFiringThresholdV { get { return (FiringThresholdV.Min == TypicalFiringThresholdV && FiringThresholdV.Max == TypicalFiringThresholdV && !FiringThresholdV.RandomSign && FiringThresholdV.DistrType == RandomCommon.DistributionType.Uniform); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultRefractoryPeriods { get { return (RefractoryPeriods == ActivationFactory.DefaultRefractoryPeriods); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultSolverMethod { get { return (SolverMethod == ActivationFactory.DefaultSolverMethod); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultSolverCompSteps { get { return (SolverCompSteps == ActivationFactory.DefaultSolverCompSteps); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultStimuliDuration { get { return (StimuliDuration == ActivationFactory.DefaultStimuliDuration); } }

        /// <inheritdoc/>
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
                       IsDefaultSolverCompSteps &&
                       IsDefaultStimuliDuration;
            }
        }

        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            if (RefractoryPeriods < 0)
            {
                throw new ArgumentException($"Invalid RefractoryPeriods {RefractoryPeriods.ToString(CultureInfo.InvariantCulture)}. RefractoryPeriods must be GE to 0.", "RefractoryPeriods");
            }
            if (SolverCompSteps < 1)
            {
                throw new ArgumentException($"Invalid SolverCompSteps {SolverCompSteps.ToString(CultureInfo.InvariantCulture)}. SolverCompSteps must be GE to 1.", "SolverCompSteps");
            }
            if (StimuliDuration <= 0)
            {
                throw new ArgumentException($"Invalid StimuliDuration {StimuliDuration.ToString(CultureInfo.InvariantCulture)}. StimuliDuration must be GT 0.", "StimuliDuration");
            }
            return;
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new AFSpikingIzhikevichIFSettings(this);
        }

        /// <inheritdoc/>
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
            if (!suppressDefaults || !IsDefaultStimuliDuration)
            {
                rootElem.Add(new XAttribute("stimuliDuration", StimuliDuration.ToString(CultureInfo.InvariantCulture)));
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

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("activationIzhikevichIF", suppressDefaults);
        }

    }//AFSpikingIzhikevichIFSettings

}//Namespace
