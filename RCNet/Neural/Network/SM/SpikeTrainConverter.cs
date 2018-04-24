using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Extensions;
using RCNet.MathTools;

namespace RCNet.Neural.Network.SM
{
    /// <summary>
    /// Class converts normalized an analog value to a spike train time series and vice versa.
    /// Spike train time series is a sequence of 0/1 values.
    /// </summary>
    [Serializable]
    public class SpikeTrainConverter
    {
        //Constants
        /// <summary>
        /// Min number of coding spikes
        /// </summary>
        public const int MinCodingSpikes = 1;
        /// <summary>
        /// Max number of coding spikes
        /// </summary>
        public const int MaxCodingSpikes = 53;

        //Attributes
        private Interval _analogRange;
        private ulong _buffer;
        private double _precision;
        private double _maxBuffValue;
        private int _pendingSpikes;

        //Attribute properties
        /// <summary>
        /// Number of coding spikes
        /// </summary>
        public int NumOfCodingSpikes { get; }

        /// <summary>
        /// Return number of pending spikes to be fetched.
        /// </summary>
        public int NumOfPendingSpikes { get { return _pendingSpikes; } }

        /// <summary>
        /// Constructs an initialized instance.
        /// </summary>
        /// <param name="analogRange">
        /// Normalized range of analog values.
        /// </param>
        /// <param name="numOfCodingSpikes">
        /// Number of coding spikes.
        /// As more coding spikes as higher accuracy.
        /// One spike can differentiate 2 values, two spikes 4 values, 3 spikes 8 values, 4 spikes 16 values ....
        /// </param>
        public SpikeTrainConverter(Interval analogRange, int numOfCodingSpikes)
        {
            _analogRange = analogRange.DeepClone();
            NumOfCodingSpikes = Math.Max(Math.Min(numOfCodingSpikes, MaxCodingSpikes), MinCodingSpikes);
            _precision = (1d / Math.Pow(2, NumOfCodingSpikes));
            _maxBuffValue = Math.Pow(2, numOfCodingSpikes) - 1;
            _buffer = 0;
            _pendingSpikes = 0;
            return;
        }

        /// <summary>
        /// Encodes analog value and prepares spikes to be fetched.
        /// </summary>
        /// <param name="analogValue">Analog value</param>
        public void EncodeAnalogValue(double analogValue)
        {
            double rescaledAnalog = ((analogValue - _analogRange.Min) / _analogRange.Span).Bound(0, 1);
            double pieces = Math.Min(rescaledAnalog / _precision, _maxBuffValue);
            _buffer = (ulong)Math.Floor(pieces);
            _pendingSpikes = NumOfCodingSpikes;
            return;
        }

        /// <summary>
        /// Fetches next pending spike.
        /// </summary>
        /// <returns>0/1 or -1 if there is no pending spikes.</returns>
        public int FetchSpike()
        {
            int spike = -1;
            if (_pendingSpikes > 0)
            {
                spike = (int)(_buffer & 0x01);
                _buffer >>= 1;
            }
            return spike;
        }

        /// <summary>
        /// Encodes spike train and prepares corresponding analog value to be fetched.
        /// </summary>
        /// <param name="spikeTrain">Spike train</param>
        public void EncodeSpikeTrain(ulong spikeTrain)
        {
            _buffer = (ulong)Math.Min(spikeTrain, _maxBuffValue);
            return;
        }

        /// <summary>
        /// Fetches analog value according to encoded spikes.
        /// </summary>
        /// <returns>Analog value within the analogRange</returns>
        public double FetchAnalogValue()
        {
            return (_buffer * _precision) * _analogRange.Span + _analogRange.Min;
        }

    }//SpikeTrainConverter

}//Namespace
