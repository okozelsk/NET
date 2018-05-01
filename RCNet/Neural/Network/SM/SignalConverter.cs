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
        private double _precision;
        private ulong _maxBuffValue;
        private int _pendingFractions;

        //Attribute properties
        /// <summary>
        /// Number of coding fractions
        /// </summary>
        public int NumOfCodingFractions { get; }

        /// <summary>
        /// Return number of pending fractions to be fetched.
        /// </summary>
        public int NumOfPendingFractions { get { return _pendingFractions; } }

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
            _pendingFractions = 0;
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
            _pendingFractions = NumOfCodingFractions;
            return;
        }

        /// <summary>
        /// Fetches next pending spike.
        /// </summary>
        /// <returns>0/1 or throws exception if there is no pending spikes.</returns>
        public int FetchSpike()
        {
            if (_pendingFractions > 0)
            {
                int spike = Bitwise.IsBitSet(_buffer, (uint)(NumOfCodingFractions - _pendingFractions)) ? 1 : 0;
                --_pendingFractions;
                return spike;
            }
            else
            {
                throw new Exception("No more spikes to be fetched.");
            }
        }

        /// <summary>
        /// Encodes spike train and prepares corresponding analog value to be fetched.
        /// </summary>
        /// <param name="spikeTrain">Spike train</param>
        public void EncodeSpikeTrain(ulong spikeTrain)
        {
            _buffer = spikeTrain & _maxBuffValue;
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

        /// <summary>
        /// Function mixes three analog values in the specified range into the one.
        /// Order of values: the first is the most important.
        /// </summary>
        /// <param name="analogRange">Range of analog values</param>
        /// <param name="a1">Analog value 1</param>
        /// <param name="a2">Analog value 2</param>
        /// <param name="a3">Analog value 3</param>
        /// <param name="s1">Spikes (bits) for analog value 1</param>
        /// <param name="s2">Spikes (bits) for analog value 2</param>
        /// <param name="s3">Spikes (bits) for analog value 3</param>
        /// <returns></returns>
        public static double Mix(Interval analogRange,
                                 double a1,
                                 double a2,
                                 double a3,
                                 int s1,
                                 int s2,
                                 int s3
                                 )
        {
            //Check spikes
            if(s1 + s2 + s3 > MaxCodingFractions)
            {
                throw new Exception("s1 + s2 +s3 > MaxCodingSpikes");
            }
            SignalConverter sc1 = new SignalConverter(analogRange, s1);
            SignalConverter sc2 = new SignalConverter(analogRange, s2);
            SignalConverter sc3 = new SignalConverter(analogRange, s3);
            SignalConverter scm = new SignalConverter(analogRange, s1 + s2 + s3);
            sc1.EncodeAnalogValue(a1);
            sc2.EncodeAnalogValue(a2);
            sc3.EncodeAnalogValue(a3);
            ulong buffer = 0;
            for(int i = 0; i < s1; i++)
            {
                buffer <<= 1;
                ulong bit = (ulong)sc1.FetchSpike();
                buffer |= bit;
            }
            for (int i = 0; i < s2; i++)
            {
                buffer <<= 1;
                ulong bit = (ulong)sc2.FetchSpike();
                buffer |= bit;
            }
            for (int i = 0; i < s3; i++)
            {
                buffer <<= 1;
                ulong bit = (ulong)sc3.FetchSpike();
                buffer |= bit;
            }
            scm.EncodeSpikeTrain(buffer);
            return scm.FetchAnalogValue();
        }

    }//SignalConverter

}//Namespace
