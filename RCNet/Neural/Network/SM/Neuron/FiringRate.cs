using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.MathTools;

namespace RCNet.Neural.Network.SM.Neuron
{
    /// <summary>
    /// Class converts recent spikes to a number representing weighted firing rate
    /// </summary>
    [Serializable]
    class FiringRate
    {
        //Constants
        private const int SpikeBuffLength = sizeof(ulong) * 8;

        //Static members
        private static readonly Normalizer _outputNormalizer;
        private static readonly decimal[] _spikeValueCache;
        private static readonly decimal _sumOfSpikeValues;

        //Instance members
        private ulong _spikes;

        //Static constructor
        static FiringRate()
        {
            _sumOfSpikeValues = 0;
            _spikeValueCache = new decimal[SpikeBuffLength];
            for(int i = 0; i < SpikeBuffLength; i++)
            {
                decimal val = (decimal)Math.Exp(-i);
                _spikeValueCache[i] = val;
                _sumOfSpikeValues += val;
            }
            _outputNormalizer = new Normalizer(new Interval(-1, 1), 0, false);
            _outputNormalizer.Adjust(0);
            _outputNormalizer.Adjust(1);
            return;
        }

        //Constructor
        public FiringRate()
        {
            Reset();
            return;
        }

        //Methods
        public void Reset()
        {
            _spikes = 0;
            return;
        }

        public void Update(bool spike)
        {
            _spikes <<= 1;
            if (spike)
            {
                _spikes |= 1;
            }
            return;
        }

        public double GetRate()
        {
            decimal rate = 0;
            ulong localCopy = _spikes;
            for(int i = 0; i < SpikeBuffLength; i++)
            {
                if((localCopy & 1) > 0)
                {
                    rate += _spikeValueCache[i];
                }
                localCopy >>= 1;
            }
            //Normalize between -1 and 1
            return _outputNormalizer.Normalize((double)(rate / _sumOfSpikeValues));
        }


    }//FiringRate
}//Namespace
