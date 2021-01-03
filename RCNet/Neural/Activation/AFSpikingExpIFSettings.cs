using RCNet.MathTools.Differential;
using RCNet.RandomValue;
using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Configuration of the AFSpikingExpIF activation function.
    /// </summary>
    [Serializable]
    public class AFSpikingExpIFSettings : RCNetBaseSettings, IActivationSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "ActivationExpIFType";

        //Typical values
        /// <summary>
        /// The typical value of time scale.
        /// </summary>
        public const double TypicalTimeScale = 12;
        /// <summary>
        /// The typical value of resistance.
        /// </summary>
        public const double TypicalResistance = 20;
        /// <summary>
        /// The typical value of resting voltage.
        /// </summary>
        public const double TypicalRestV = -65;
        /// <summary>
        /// The typical value of reset voltage.
        /// </summary>
        public const double TypicalResetV = -60;
        /// <summary>
        /// The typical value of rheobase.
        /// </summary>
        public const double TypicalRheobaseV = -55;
        /// <summary>
        /// The typical value of firing voltage.
        /// </summary>
        public const double TypicalFiringThresholdV = -30;
        /// <summary>
        /// The typical value of sharpness delta.
        /// </summary>
        public const double TypicalSharpnessDeltaT = 2;

        //Attribute properties
        /// <summary>
        /// The membrane time scale (ms).
        /// </summary>
        public URandomValueSettings TimeScale { get; }
        /// <summary>
        /// The membrane resistance (Mohm).
        /// </summary>
        public URandomValueSettings Resistance { get; }
        /// <summary>
        /// The membrane rest potential (mV).
        /// </summary>
        public RandomValueSettings RestV { get; }
        /// <summary>
        /// The membrane reset potential (mV).
        /// </summary>
        public RandomValueSettings ResetV { get; }
        /// <summary>
        /// The membrane rheobase threshold (mV).
        /// </summary>
        public RandomValueSettings RheobaseV { get; }
        /// <summary>
        /// The membrane firing threshold (mV).
        /// </summary>
        public RandomValueSettings FiringThresholdV { get; }
        /// <summary>
        /// The sharpness of membrane potential change (mV).
        /// </summary>
        public URandomValueSettings SharpnessDeltaT { get; }
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
        /// <param name="timeScale">The membrane time scale (ms).</param>
        /// <param name="resistance">The membrane resistance (Mohm).</param>
        /// <param name="restV">The membrane rest potential (mV).</param>
        /// <param name="resetV">The membrane reset potential (mV).</param>
        /// <param name="rheobaseV">The membrane rheobase threshold (mV).</param>
        /// <param name="firingThresholdV">The membrane firing threshold (mV).</param>
        /// <param name="sharpnessDeltaT">The sharpness of membrane potential change (mV).</param>
        /// <param name="refractoryPeriods">The number of after-spike computation cycles while an input stimuli to be ignored (cycles).</param>
        /// <param name="solverMethod">The ODE numerical solver method to be used.</param>
        /// <param name="solverCompSteps">The number of computation sub-steps of the ODE numerical solver.</param>
        /// <param name="stimuliDuration">The duration of the membrane stimulation (ms).</param>
        public AFSpikingExpIFSettings(URandomValueSettings timeScale = null,
                                      URandomValueSettings resistance = null,
                                      RandomValueSettings restV = null,
                                      RandomValueSettings resetV = null,
                                      RandomValueSettings rheobaseV = null,
                                      RandomValueSettings firingThresholdV = null,
                                      URandomValueSettings sharpnessDeltaT = null,
                                      int refractoryPeriods = ActivationFactory.DefaultRefractoryPeriods,
                                      ODENumSolver.Method solverMethod = ActivationFactory.DefaultSolverMethod,
                                      int solverCompSteps = ActivationFactory.DefaultSolverCompSteps,
                                      double stimuliDuration = ActivationFactory.DefaultStimuliDuration
                                      )
        {
            TimeScale = URandomValueSettings.CloneOrCreate(timeScale, TypicalTimeScale);
            Resistance = URandomValueSettings.CloneOrCreate(resistance, TypicalResistance);
            RestV = RandomValueSettings.CloneOrCreate(restV, TypicalRestV);
            ResetV = RandomValueSettings.CloneOrCreate(resetV, TypicalResetV);
            RheobaseV = RandomValueSettings.CloneOrCreate(rheobaseV, TypicalRheobaseV);
            FiringThresholdV = RandomValueSettings.CloneOrCreate(firingThresholdV, TypicalFiringThresholdV);
            SharpnessDeltaT = URandomValueSettings.CloneOrCreate(sharpnessDeltaT, TypicalSharpnessDeltaT);
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
        public AFSpikingExpIFSettings(AFSpikingExpIFSettings source)
        {
            TimeScale = (URandomValueSettings)source.TimeScale.DeepClone();
            Resistance = (URandomValueSettings)source.Resistance.DeepClone();
            RestV = (RandomValueSettings)source.RestV.DeepClone();
            ResetV = (RandomValueSettings)source.ResetV.DeepClone();
            RheobaseV = (RandomValueSettings)source.RheobaseV.DeepClone();
            FiringThresholdV = (RandomValueSettings)source.FiringThresholdV.DeepClone();
            SharpnessDeltaT = (URandomValueSettings)source.SharpnessDeltaT.DeepClone();
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
        public AFSpikingExpIFSettings(XElement elem)
        {
            //Validation
            XElement activationSettingsElem = Validate(elem, XsdTypeName);
            //Parsing
            TimeScale = URandomValueSettings.LoadOrCreate(activationSettingsElem, "timeScale", TypicalTimeScale);
            Resistance = URandomValueSettings.LoadOrCreate(activationSettingsElem, "resistance", TypicalResistance);
            RestV = RandomValueSettings.LoadOrCreate(activationSettingsElem, "restV", TypicalRestV);
            ResetV = RandomValueSettings.LoadOrCreate(activationSettingsElem, "resetV", TypicalResetV);
            RheobaseV = RandomValueSettings.LoadOrCreate(activationSettingsElem, "rheobaseV", TypicalRheobaseV);
            FiringThresholdV = RandomValueSettings.LoadOrCreate(activationSettingsElem, "firingThresholdV", TypicalFiringThresholdV);
            SharpnessDeltaT = URandomValueSettings.LoadOrCreate(activationSettingsElem, "sharpnessDeltaT", TypicalSharpnessDeltaT);
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
        public bool IsDefaultTimeScale { get { return (TimeScale.Min == TypicalTimeScale && TimeScale.Max == TypicalTimeScale && TimeScale.DistrType == RandomCommon.DistributionType.Uniform); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultResistance { get { return (Resistance.Min == TypicalResistance && Resistance.Max == TypicalResistance && Resistance.DistrType == RandomCommon.DistributionType.Uniform); } }

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
        public bool IsDefaultRheobaseV { get { return (RheobaseV.Min == TypicalRheobaseV && RheobaseV.Max == TypicalRheobaseV && !RheobaseV.RandomSign && RheobaseV.DistrType == RandomCommon.DistributionType.Uniform); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultFiringThresholdV { get { return (FiringThresholdV.Min == TypicalFiringThresholdV && FiringThresholdV.Max == TypicalFiringThresholdV && !FiringThresholdV.RandomSign && FiringThresholdV.DistrType == RandomCommon.DistributionType.Uniform); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultSharpnessDeltaT { get { return (SharpnessDeltaT.Min == TypicalSharpnessDeltaT && SharpnessDeltaT.Max == TypicalSharpnessDeltaT && SharpnessDeltaT.DistrType == RandomCommon.DistributionType.Uniform); } }

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
                return IsDefaultTimeScale &&
                       IsDefaultResistance &&
                       IsDefaultRestV &&
                       IsDefaultResetV &&
                       IsDefaultRheobaseV &&
                       IsDefaultFiringThresholdV &&
                       IsDefaultSharpnessDeltaT &&
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
            return new AFSpikingExpIFSettings(this);
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
            if (!suppressDefaults || !IsDefaultTimeScale)
            {
                rootElem.Add(TimeScale.GetXml("timeScale", suppressDefaults));
            }
            if (!suppressDefaults || !IsDefaultResistance)
            {
                rootElem.Add(Resistance.GetXml("resistance", suppressDefaults));
            }
            if (!suppressDefaults || !IsDefaultRestV)
            {
                rootElem.Add(RestV.GetXml("restV", suppressDefaults));
            }
            if (!suppressDefaults || !IsDefaultResetV)
            {
                rootElem.Add(ResetV.GetXml("resetV", suppressDefaults));
            }
            if (!suppressDefaults || !IsDefaultRheobaseV)
            {
                rootElem.Add(RheobaseV.GetXml("rheobaseV", suppressDefaults));
            }
            if (!suppressDefaults || !IsDefaultFiringThresholdV)
            {
                rootElem.Add(FiringThresholdV.GetXml("firingThresholdV", suppressDefaults));
            }
            if (!suppressDefaults || !IsDefaultSharpnessDeltaT)
            {
                rootElem.Add(SharpnessDeltaT.GetXml("sharpnessDeltaT", suppressDefaults));
            }

            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("activationExpIF", suppressDefaults);
        }

    }//AFSpikingExpIFSettings

}//Namespace
