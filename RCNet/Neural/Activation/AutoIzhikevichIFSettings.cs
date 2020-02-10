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
using RCNet.Neural.Network.SM.Neuron;
using RCNet.XmlTools;
using RCNet.MathTools.Differential;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Class encaptulates arguments for the setup of IzhikevichIF activation function in automatic role-driven mode
    /// </summary>
    [Serializable]
    public class AutoIzhikevichIFSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "ActivationAutoIzhikevichIFCfgType";

        //Attribute properties
        /// <summary>
        /// Role of the neuron (excitatory or inhibitory)
        /// </summary>
        public NeuronCommon.NeuronRole Role { get; }
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
        /// <param name="role">Role of the neuron (excitatory or inhibitory)</param>
        /// <param name="refractoryPeriods">Number of after spike computation cycles while an input stimuli is ignored (ms)</param>
        /// <param name="solverMethod">ODE numerical solver method</param>
        /// <param name="solverCompSteps">ODE numerical solver computation steps of the time step</param>
        public AutoIzhikevichIFSettings(NeuronCommon.NeuronRole role,
                                        int refractoryPeriods = 1,
                                        ODENumSolver.Method solverMethod = ODENumSolver.Method.Euler,
                                        int solverCompSteps = 2
                                        )
        {
            Role = role;
            RefractoryPeriods = refractoryPeriods;
            SolverMethod = solverMethod;
            SolverCompSteps = solverCompSteps;
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public AutoIzhikevichIFSettings(AutoIzhikevichIFSettings source)
        {
            Role = source.Role;
            RefractoryPeriods = source.RefractoryPeriods;
            SolverMethod = source.SolverMethod;
            SolverCompSteps = source.SolverCompSteps;
            return;
        }

        /// <summary>
        /// Creates an instance and initializes it from given xml element.
        /// </summary>
        /// <param name="elem">
        /// Xml data containing activation settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public AutoIzhikevichIFSettings(XElement elem)
        {
            //Validation
            XElement activationSettingsElem = Validate(elem, XsdTypeName);
            //Parsing
            Role = NeuronCommon.ParseNeuronRole(activationSettingsElem.Attribute("role").Value);
            RefractoryPeriods = int.Parse(activationSettingsElem.Attribute("refractoryPeriods").Value, CultureInfo.InvariantCulture);
            SolverMethod = ODENumSolver.ParseComputationMethodType(activationSettingsElem.Attribute("solverMethod").Value);
            SolverCompSteps = int.Parse(activationSettingsElem.Attribute("solverCompSteps").Value, CultureInfo.InvariantCulture);
            return;
        }

        //Methods
        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public AutoIzhikevichIFSettings DeepClone()
        {
            return new AutoIzhikevichIFSettings(this);
        }

    }//AutoIzhikevichIFSettings

}//Namespace
