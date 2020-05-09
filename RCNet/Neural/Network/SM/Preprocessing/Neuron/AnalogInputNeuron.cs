using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.Neural.Activation;
using RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Pool.NeuronGroup;

namespace RCNet.Neural.Network.SM.Preprocessing.Neuron
{
    /// <summary>
    /// Analog input neuron is the special type of a neuron without accosiated activation function. Its purpose is only to mediate
    /// external input analog value for a synapse in appropriate form and to provide predictors.
    /// </summary>
    [Serializable]
    public class AnalogInputNeuron : INeuron
    {
        //Static attributes
        private static readonly Interval _normalizedOutputRange = new Interval(0, 1);

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
        private readonly Interval _inputRange;
        private double _stimuli;
        private double _output;
        private double _normalizedOutput;
        private readonly double _analogFiringThreshold;
        private readonly PredictorsProvider _predictors;

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="location">Neuron's location</param>
        /// <param name="inputRange">Range of input value</param>
        /// <param name="predictorsCfg">Configuration of neuron's predictors</param>
        /// <param name="analogFiringThreshold">A number between 0 and 1 (LT1). Every time the new activation value is higher than the previous activation value by at least the threshold, it is evaluated as a firing event. Ignored in case of spiking activation.</param>
        public AnalogInputNeuron(NeuronLocation location,
                                 Interval inputRange,
                                 PredictorsSettings predictorsCfg,
                                 double analogFiringThreshold = AnalogNeuronGroupSettings.DefaultFiringThreshold
                                 )
        {
            Location = location;
            _inputRange = inputRange.DeepClone();
            Statistics = new NeuronStatistics();
            _predictors = predictorsCfg != null ? new PredictorsProvider(predictorsCfg) : null;
            _analogFiringThreshold = analogFiringThreshold;
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
        public ActivationType TypeOfActivation { get { return ActivationType.Analog; } }

        /// <summary>
        /// Output signaling restriction
        /// </summary>
        public NeuronCommon.NeuronSignalingRestrictionType SignalingRestriction { get { return NeuronCommon.NeuronSignalingRestrictionType.AnalogOnly; } }

        /// <summary>
        /// Computation cycles gone from the last emitted spike or start (if no spike emitted before current computation cycle)
        /// </summary>
        public int SpikeLeak { get { return 0; } }

        /// <summary>
        /// Specifies, if neuron has already emitted spike before current computation cycle
        /// </summary>
        public bool AfterFirstSpike { get { return false; } }

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
            _stimuli = 0;
            _output = 0;
            _normalizedOutput = 0;
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
        /// <param name="iStimuli">External input analog signal</param>
        /// <param name="rStimuli">Parameter is ignored. Stimulation comming from reservoir neurons is irrelevant. </param>
        public void NewStimulation(double iStimuli, double rStimuli)
        {
            _stimuli = iStimuli.Bound();
            return;
        }

        /// <summary>
        /// Prepares new output signal (input for hidden neurons).
        /// </summary>
        /// <param name="collectStatistics">Specifies whether to update internal statistics</param>
        public void Recompute(bool collectStatistics)
        {
            _output = _stimuli;
            //Normalized stimuli between 0 and 1
            double prevNormalizedStimuli = _normalizedOutput;
            _normalizedOutput = _normalizedOutputRange.Rescale(_stimuli, _inputRange);
            bool firingEvent = (_normalizedOutput - prevNormalizedStimuli) > _analogFiringThreshold;
            //Update predictors
            _predictors?.Update(_stimuli, _normalizedOutput.Bound(0, 1), firingEvent);
            //Statistics
            if (collectStatistics)
            {
                Statistics.Update(_stimuli, 0d, _stimuli, _stimuli, _stimuli, firingEvent ? 1d : 0d);
            }
            return;
        }

        /// <summary>
        /// Returns input for hidden neuron having activation of specified type.
        /// </summary>
        /// <param name="targetActivationType">Specifies what type of the signal is required.</param>
        public double GetSignal(ActivationType targetActivationType)
        {
            return targetActivationType == ActivationType.Spiking ? _normalizedOutput : _output;
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


    }//AnalogInputNeuron

}//Namespace
