using System;
using System.Globalization;
using System.Xml.Linq;
using RCNet.MathTools.Differential;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Configuration of the AFSpikingIzhikevichIF activation function in automatic rich behaviour mode
    /// </summary>
    [Serializable]
    public class AFSpikingAutoIzhikevichIFSettings : RCNetBaseSettings, IActivationSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "ActivationAutoIzhikevichIFType";

        //Attribute properties
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
        /// <summary>
        /// Duration of the stimulation
        /// </summary>
        public double StimuliDuration { get; }


        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="refractoryPeriods">Number of after spike computation cycles while an input stimuli is ignored (ms)</param>
        /// <param name="solverMethod">ODE numerical solver method</param>
        /// <param name="solverCompSteps">ODE numerical solver computation steps of the time step</param>
        /// <param name="stimuliDuration">Duration of the stimulation</param>
        public AFSpikingAutoIzhikevichIFSettings(int refractoryPeriods = ActivationFactory.DefaultRefractoryPeriods,
                                        ODENumSolver.Method solverMethod = ActivationFactory.DefaultSolverMethod,
                                        int solverCompSteps = ActivationFactory.DefaultSolverCompSteps,
                                        double stimuliDuration = ActivationFactory.DefaultStimuliDuration
                                        )
        {
            RefractoryPeriods = refractoryPeriods;
            SolverMethod = solverMethod;
            SolverCompSteps = solverCompSteps;
            StimuliDuration = stimuliDuration;
            Check();
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public AFSpikingAutoIzhikevichIFSettings(AFSpikingAutoIzhikevichIFSettings source)
        {
            RefractoryPeriods = source.RefractoryPeriods;
            SolverMethod = source.SolverMethod;
            SolverCompSteps = source.SolverCompSteps;
            StimuliDuration = source.StimuliDuration;
            return;
        }

        /// <summary>
        /// Creates an instance and initializes it from given xml element.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
        public AFSpikingAutoIzhikevichIFSettings(XElement elem)
        {
            //Validation
            XElement activationSettingsElem = Validate(elem, XsdTypeName);
            //Parsing
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
        /// Checks the defaults
        /// </summary>
        public bool IsDefaultRefractoryPeriods { get { return (RefractoryPeriods == ActivationFactory.DefaultRefractoryPeriods); } }

        /// <summary>
        /// Checks the defaults
        /// </summary>
        public bool IsDefaultSolverMethod { get { return (SolverMethod == ActivationFactory.DefaultSolverMethod); } }

        /// <summary>
        /// Checks the defaults
        /// </summary>
        public bool IsDefaultSolverCompSteps { get { return (SolverCompSteps == ActivationFactory.DefaultSolverCompSteps); } }

        /// <summary>
        /// Checks the defaults
        /// </summary>
        public bool IsDefaultStimuliDuration { get { return (StimuliDuration == ActivationFactory.DefaultStimuliDuration); } }

        /// <inheritdoc />
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return IsDefaultRefractoryPeriods &&
                       IsDefaultSolverMethod &&
                       IsDefaultSolverCompSteps &&
                       IsDefaultStimuliDuration;
            }
        }

        //Methods
        /// <inheritdoc />
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

        /// <inheritdoc />
        public override RCNetBaseSettings DeepClone()
        {
            return new AFSpikingAutoIzhikevichIFSettings(this);
        }

        /// <inheritdoc />
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

            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc />
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("activationAutoIzhikevichIF", suppressDefaults);
        }

    }//AFSpikingAutoIzhikevichIFSettings

}//Namespace
