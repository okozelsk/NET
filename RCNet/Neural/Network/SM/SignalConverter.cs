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
    /// Class converts a normalized analog value to a spike train series and vice versa.
    /// Spike train series is a sequence of 0/1 values.
    /// </summary>
    [Serializable]
    public class SignalConverter
    {
        //Constants
        /// <summary>
        /// Min number of coding fractions
        /// </summary>
        public const int MinCodingFractions = 1;
        /// <summary>
        /// Max number of coding fractions
        /// </summary>
        public const int MaxCodingFractions = 53;

        //Attributes
        private Interval _analogRange;
        private ulong _buffer;
        private readonly double _precision;
        private readonly ulong _maxBuffValue;

        //Attribute properties
        /// <summary>
        /// Number of coding fractions
        /// </summary>
        public int NumOfCodingFractions { get; }

        /// <summary>
        /// Return number of pending fractions to be fetched.
        /// </summary>
        public int NumOfPendingFractions { get; private set; }

        /// <summary>
        /// Constructs an initialized instance.
        /// </summary>
        /// <param name="analogRange">
        /// Normalized range of analog values.
        /// </param>
        /// <param name="numOfCodingFractions">
        /// Number of coding bits.
        /// As more coding bits as higher accuracy.
        /// One bit can differentiate 2 values, two bits 4 values, 3 bits 8 values, 4 bits 16 values ....
        /// </param>
        public SignalConverter(Interval analogRange, int numOfCodingFractions)
        {
            _analogRange = analogRange.DeepClone();
            NumOfCodingFractions = Math.Max(Math.Min(numOfCodingFractions, MaxCodingFractions), MinCodingFractions);
            _precision = (1d / Math.Pow(2, NumOfCodingFractions));
            _maxBuffValue = (ulong)Math.Pow(2, numOfCodingFractions) - 1;
            _buffer = 0;
            NumOfPendingFractions = 0;
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
            NumOfPendingFractions = NumOfCodingFractions;
            return;
        }

        /// <summary>
        /// Fetches next pending spike.
        /// </summary>
        /// <returns>0/1 or throws exception if there is no pending spikes.</returns>
        public int FetchSpike()
        {
            if (NumOfPendingFractions > 0)
            {
                int spike = Bitwise.IsBitSet(_buffer, (uint)(NumOfCodingFractions - NumOfPendingFractions)) ? 1 : 0;
                --NumOfPendingFractions;
                return spike;
            }
            else
            {
                throw new Exception("No more spikes to be fetched.");
            }
        }

    }//SignalConverter

}//Namespace
