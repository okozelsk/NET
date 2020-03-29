using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.Neural.Activation;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Neuron.Predictor;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Pool.NeuronGroup;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.Neuron
{
    /// <summary>
    /// Spiking input neuron is the special type of a neuron without accosiated activation function. Its purpose is only to mediate
    /// external input spike value for a synapse in appropriate form and to provide predictors.
    /// </summary>
    [Serializable]
    public class SpikingInputNeuron : INeuron
    {
        //Attribute properties
        /// <summary>
        /// Information about a neuron location within the neural preprocessor
        /// Note that Input neuron home PoolID is always -1 because Input neurons do not belong to a standard pools.
        /// </summary>
        public NeuronLocation Location { get; }

        /// <summary>
        /// Neuron's key statistics
        /// </summary>
        public NeuronStatistics Statistics { get; }

        //Attributes
        private double _inputSpike;
        private double _outputSpike;
        private readonly PredictorsProvider _predictors;

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="reservoirID">Reservoir ID</param>
        /// <param name="inputEntryPoint">Input entry point coordinates within the 3D space</param>
        /// <param name="flatIdx">Index of this neuron among all input neurons.</param>
        /// <param name="predictorsCfg">Configuration of neuron's predictors</param>
        public SpikingInputNeuron(int reservoirID,
                                  int[] inputEntryPoint,
                                  int flatIdx,
                                  PredictorsSettings predictorsCfg
                                  )
        {
            Location = new NeuronLocation(reservoirID, flatIdx, - 1, flatIdx, 0, inputEntryPoint[0], inputEntryPoint[1], inputEntryPoint[2]);
            Statistics = new NeuronStatistics();
            _predictors = predictorsCfg != null ? new PredictorsProvider(predictorsCfg) : null;
            Reset(false);
            return;
        }

        //Properties
        /// <summary>
        /// Neuron type
        /// </summary>
        public NeuronCommon.NeuronType Type { get { return NeuronCommon.NeuronType.Input; } }

        /// <summary>
        /// Type of the activation function
        /// </summary>
        public ActivationType TypeOfActivation { get { return ActivationType.Spiking; } }

        /// <summary>
        /// Output signaling restriction
        /// </summary>
        public NeuronCommon.NeuronSignalingRestrictionType SignalingRestriction { get { return NeuronCommon.NeuronSignalingRestrictionType.SpikingOnly; } }

        /// <summary>
        /// Computation cycles gone from the last emitted spike or start (if no spike emitted before current computation cycle)
        /// </summary>
        public int SpikeLeak { get; private set; }

        /// <summary>
        /// Specifies, if neuron has already emitted spike before current computation cycle
        /// </summary>
        public bool AfterFirstSpike { get; private set; }

        /// <summary>
        /// Number of provided predictors
        /// </summary>
        public int NumOfEnabledPredictors { get { return _predictors == null ? 0 : _predictors.NumOfEnabledPredictors; } }

        //Methods
        /// <summary>
        /// Resets neuron to its initial state
        /// </summary>
        /// <param name="statistics">Specifies whether to reset internal statistics</param>
        public void Reset(bool statistics)
        {
            _inputSpike = 0;
            _outputSpike = 0;
            SpikeLeak = 0;
            AfterFirstSpike = false;
            _predictors?.Reset();
            if (statistics)
            {
                Statistics.Reset();
            }
            return;
        }

        /// <summary>
        /// Stores new incoming stimulation.
        /// </summary>
        /// <param name="iStimuli">External input spike</param>
        /// <param name="rStimuli">Parameter is ignored. Stimulation comming from reservoir neurons is irrelevant. </param>
        public void NewStimulation(double iStimuli, double rStimuli)
        {
            _inputSpike = iStimuli > 0 ? 1d : 0d;
            return;
        }

        /// <summary>
        /// Prepares new output signal (input for hidden neurons).
        /// </summary>
        /// <param name="collectStatistics">Specifies whether to update internal statistics</param>
        public void Recompute(bool collectStatistics)
        {
            if (_outputSpike > 0)
            {
                //Spike during previous cycle, so reset the counter
                AfterFirstSpike = true;
                SpikeLeak = 0;
            }
            ++SpikeLeak;
            _outputSpike = _inputSpike;
            bool firing = _outputSpike > 0;
            //Update predictors
            _predictors?.Update(_outputSpike, _outputSpike, firing);
            //Statistics
            if (collectStatistics)
            {
                Statistics.Update(_inputSpike, 0d, _inputSpike, _inputSpike, 0, _outputSpike);
            }
            return;
        }

        /// <summary>
        /// Checks if given predictor is enabled
        /// </summary>
        /// <param name="predictorID">Identificator of the predictor</param>
        public bool IsPredictorEnabled(PredictorsProvider.PredictorID predictorID)
        {
            if (_predictors != null && _predictors.IsPredictorEnabled(predictorID))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns input for hidden neuron having activation of specified type.
        /// </summary>
        /// <param name="targetActivationType">Specifies what type of the signal is required.</param>
        public double GetSignal(ActivationType targetActivationType)
        {
            return _outputSpike;
        }

        /// <summary>
        /// Copies values of enabled predictors to a given buffer starting from specified position (idx)
        /// </summary>
        /// <param name="predictors">Buffer where to be copied enabled predictors</param>
        /// <param name="idx">Starting position index</param>
        public int CopyPredictorsTo(double[] predictors, int idx)
        {
            if (_predictors == null)
            {
                return 0;
            }
            else
            {
                return _predictors.CopyPredictorsTo(predictors, idx);
            }
        }

        /// <summary>
        /// Returns array containing values of enabled predictors
        /// </summary>
        public double[] GetPredictors()
        {
            return _predictors?.GetPredictors();
        }


    }//SpikingInputNeuron

}//Namespace
