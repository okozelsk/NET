using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Extensions;

namespace RCNet.Neural.Network.SM.Neuron
{
    /// <summary>
    /// Class converts recent spikes to an exponentially weighted firing rate (0-1)
    /// </summary>
    [Serializable]
    class FiringRate
    {
        //Constants
        private const int SpikeBuffLength = sizeof(ulong) * 8;

        //Static members
        private static readonly decimal[] _spikeValueCache;
        private static readonly decimal[] _sumOfSpikeValues;
        private static readonly ulong _highestBitMask;

        //Instance members
        //Attribute properties
        /// <summary>
        /// Returns current number of spikes within the sliding buffer
        /// </summary>
        public int NumOfRecentSpikes { get; private set; }
        //Attributes
        private ulong _spikes;
        private int _numOfBufferedData;

        /// <summary>
        /// Static constructor
        /// </summary>
        static FiringRate()
        {
            _sumOfSpikeValues = new decimal[SpikeBuffLength];
            _sumOfSpikeValues.Populate(0);
            _spikeValueCache = new decimal[SpikeBuffLength];
            for(int i = 0; i < SpikeBuffLength; i++)
            {
                decimal val = (decimal)Math.Exp(-i);
                _spikeValueCache[i] = val;
                _sumOfSpikeValues[i] = (i > 0 ? _sumOfSpikeValues[i - 1] : 0) + val;
            }

            _highestBitMask = 1;
            _highestBitMask <<= (SpikeBuffLength - 1);
            return;
        }

        /// <summary>
        /// Creates new initialized instance
        /// </summary>
        public FiringRate()
        {
            Reset();
            return;
        }

        //Methods
        /// <summary>
        /// Resets internal state to initial state
        /// </summary>
        public void Reset()
        {
            _spikes = 0;
            _numOfBufferedData = 0;
            NumOfRecentSpikes = 0;
            return;
        }

        /// <summary>
        /// Updates internal state
        /// </summary>
        /// <param name="spike"></param>
        public void Update(bool spike)
        {
            NumOfRecentSpikes -= (_spikes & _highestBitMask) > 0 ? 1 : 0;
            _spikes <<= 1;
            if (spike)
            {
                _spikes |= 1;
                ++NumOfRecentSpikes;
            }
            if (_numOfBufferedData < SpikeBuffLength)
            {
                ++_numOfBufferedData;
            }
            return;
        }

        /// <summary>
        /// Returns recent exponentially weighted firing rate
        /// </summary>
        /// <returns>Average exponentially weighted firing rate between 0 and 1</returns>
        public double GetRecentExpWRate()
        {
            if(_numOfBufferedData == 0)
            {
                return 0;
            }
            decimal rate = 0;
            ulong localCopy = _spikes;
            for(int i = 0; i < _numOfBufferedData; i++)
            {
                if((localCopy & 1) > 0)
                {
                    rate += _spikeValueCache[i];
                }
                localCopy >>= 1;
            }
            //Average rate
            return (double)(rate / _sumOfSpikeValues[_numOfBufferedData - 1]);
        }

        /// <summary>
        /// Returns recent firing frequency
        /// </summary>
        /// <returns>Firing frequency between 0 and 1</returns>
        public double GetRecentFrequency()
        {
            if (_numOfBufferedData == 0)
            {
                return 0;
            }
            return (double)NumOfRecentSpikes / _numOfBufferedData;
        }

        /// <summary>
        /// Returns last spikes as an integer number where bits are in order that the higher bit represents the more recent spike
        /// </summary>
        /// <returns>Last spikes history as an integer</returns>
        public ulong GetLastSpikes(int histLength)
        {
            if (_numOfBufferedData == 0 || histLength < 1)
            {
                return 0;
            }
            if(histLength > _numOfBufferedData)
            {
                histLength = _numOfBufferedData;
            }
            ulong localCopy = _spikes;
            ulong result = 0;
            for (int i = 0; i < histLength; i++)
            {
                result <<= 1;
                result += localCopy & 1;
                localCopy >>= 1;
            }
            return result;
        }

    }//FiringRate

}//Namespace
