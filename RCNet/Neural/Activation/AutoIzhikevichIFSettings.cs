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
    /// Class encaptulates arguments of the IzhikevichIF activation function
    /// </summary>
    [Serializable]
    public class AutoIzhikevichIFSettings
    {
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
        /// <summary>
        /// Determines what variant of ODENumSolver method to use.
        /// Gradual solving can lead to shorter computation time (computation stops immediately the firing threshold is met) but
        /// it impacts membrane current potential prediction power.
        /// </summary>
        public bool SolveGradually { get; }


        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="role">Role of the neuron (excitatory or inhibitory)</param>
        /// <param name="refractoryPeriods">Number of after spike computation cycles while an input stimuli is ignored (ms)</param>
        /// <param name="solverMethod">ODE numerical solver method</param>
        /// <param name="solverCompSteps">ODE numerical solver computation steps of the time step</param>
        /// <param name="solveGradually">Determines what variant of ODENumSolver method to use. Gradual solving can lead to shorter computation time (computation stops immediately the firing threshold is met) but it impacts membrane current potential prediction power.</param>
        public AutoIzhikevichIFSettings(NeuronCommon.NeuronRole role,
                                        int refractoryPeriods = 1,
                                        ODENumSolver.Method solverMethod = ODENumSolver.Method.Euler,
                                        int solverCompSteps = 2,
                                        bool solveGradually = false
                                        )
        {
            Role = role;
            RefractoryPeriods = refractoryPeriods;
            SolverMethod = solverMethod;
            SolverCompSteps = solverCompSteps;
            SolveGradually = solveGradually;
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
            SolveGradually = source.SolveGradually;
            return;
        }

        /// <summary>
        /// Creates an instance and initializes it from given xml element.
        /// </summary>
        /// <param name="elem">
        /// Xml data containing IzhikevichIF activation settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public AutoIzhikevichIFSettings(XElement elem)
        {
            //Validation
            ElemValidator validator = new ElemValidator();
            Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.Neural.Activation.AutoIzhikevichIFSettings.xsd");
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.RCNetTypes.xsd");
            XElement activationSettingsElem = validator.Validate(elem, "rootElem");
            //Parsing
            Role = NeuronCommon.ParseNeuronRole(activationSettingsElem.Attribute("role").Value);
            RefractoryPeriods = int.Parse(activationSettingsElem.Attribute("refractoryPeriods").Value, CultureInfo.InvariantCulture);
            SolverMethod = ODENumSolver.ParseComputationMethodType(activationSettingsElem.Attribute("solverMethod").Value);
            SolverCompSteps = int.Parse(activationSettingsElem.Attribute("solverCompSteps").Value, CultureInfo.InvariantCulture);
            SolveGradually = bool.Parse(activationSettingsElem.Attribute("solveGradually").Value);
            return;
        }

        //Methods
        /// <summary>
        /// See the base.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            AutoIzhikevichIFSettings cmpSettings = obj as AutoIzhikevichIFSettings;
            if (Role != cmpSettings.Role ||
                RefractoryPeriods != cmpSettings.RefractoryPeriods ||
                SolverMethod != cmpSettings.SolverMethod ||
                SolverCompSteps != cmpSettings.SolverCompSteps ||
                SolveGradually != cmpSettings.SolveGradually
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
        public AutoIzhikevichIFSettings DeepClone()
        {
            AutoIzhikevichIFSettings clone = new AutoIzhikevichIFSettings(this);
            return clone;
        }

    }//AutoIzhikevichIFSettings

}//Namespace
