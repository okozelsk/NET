using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCNet.Neural.Network.SM
{
    [Serializable]
    class FiringRate
    {
        private const int SpikeBuffLength = 32;

        //Static members
        private static double[] _spikeValueCache;
        private static double _sumOfSpikeValues;

        //Instance members
        private uint _spikes;

        //Static constructor
        static FiringRate()
        {
            _sumOfSpikeValues = 0d;
            _spikeValueCache = new double[SpikeBuffLength];
            for(int i = 0; i < SpikeBuffLength; i++)
            {
                double val = Math.Exp(-i);
                _spikeValueCache[i] = val;
                _sumOfSpikeValues += val;
            }
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
            _spikes |= (uint)(spike ? 0x0001 : 0x0000);
            return;
        }

        public double GetRate()
        {
            double rate = 0;
            uint localCopy = _spikes;
            for(int i = 0; i < SpikeBuffLength; i++)
            {
                if((localCopy & 1) > 0)
                {
                    rate += _spikeValueCache[i];
                }
                localCopy >>= 1;
            }
            //Rescale between 0-1
            return rate / _sumOfSpikeValues;
        }


    }//FiringRate
}//Namespace
