using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Extensions;

namespace RCNet.Neural.Data.Generators
{
    /// <summary>
    /// Generates constant pulses
    /// </summary>
    [Serializable]
    public class PulseGenerator : IGenerator
    {
        //Enums
        /// <summary>
        /// Method of the pulse timing
        /// </summary>
        public enum TimingMode
        {
            /// <summary>
            /// Period of the pulses is constant
            /// </summary>
            Constant,
            /// <summary>
            /// Period of the pulses follows the Uniform distribution
            /// </summary>
            Uniform,
            /// <summary>
            /// Period of the pulses follows the Gaussian distribution
            /// </summary>
            Gaussian,
            /// <summary>
            /// Period of the pulses follows the Poisson (Exponential) distribution
            /// </summary>
            Poisson
        }

        //Attributes
        private readonly double _signal;
        private readonly double _avgPeriod;
        private readonly TimingMode _mode;
        private Random _rand;
        private int _t;
        private int _nextPulseTime;

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="signal">Pulse signal value</param>
        /// <param name="avgPeriod">Pulse average leak</param>
        /// <param name="mode">Pulse timing mode</param>
        public PulseGenerator(double signal, double avgPeriod, TimingMode mode)
        {
            _signal = signal;
            _avgPeriod = Math.Abs(avgPeriod);
            _mode = mode;
            Reset();
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="settings">Configuration</param>
        public PulseGenerator(PulseGeneratorSettings settings)
        {
            _signal = settings.Signal;
            _avgPeriod = settings.AvgPeriod;
            _mode = settings.Mode;
            Reset();
            return;
        }

        //Methods
        /// <summary>
        /// Schedules next pulse time
        /// </summary>
        private void ScheduleNextPulse()
        {
            double minPeriod = 1d;
            double maxPeriod = 1d + 2d * (_avgPeriod - 1d);
            double spanPeriod = maxPeriod - minPeriod;
            int timeIncrement;
            switch(_mode)
            {
                case TimingMode.Constant:
                    timeIncrement = (_t == 0) ? 1 : (int)Math.Round(_avgPeriod);
                    break;
                case TimingMode.Uniform:
                    timeIncrement = (int)Math.Round(_rand.NextRangedUniformDouble(minPeriod, maxPeriod));
                    break;
                case TimingMode.Gaussian:
                    timeIncrement = (int)Math.Round(_rand.NextFilterredGaussianDouble(_avgPeriod, spanPeriod / 6d, minPeriod, maxPeriod));
                    break;
                case TimingMode.Poisson:
                    timeIncrement = (int)Math.Round(_rand.NextExponentialDouble(_avgPeriod));
                    break;
                default:
                    timeIncrement = 0;
                    break;
            }
            if(timeIncrement <= 0)
            {
                timeIncrement = 1;
            }
            _nextPulseTime = _t + timeIncrement;
            return;
        }

        /// <summary>
        /// Resets generator to its initial state
        /// </summary>
        public void Reset()
        {
            _rand = new Random(0);
            _t = 0;
            ScheduleNextPulse();
            return;
        }

        /// <summary>
        /// Returns next signal value
        /// </summary>
        public double Next()
        {
            ++_t;
            if(_t == _nextPulseTime)
            {
                ScheduleNextPulse();
                return _signal;
            }
            else
            {
                return 0;
            }
        }

    }//PulseGenerator
}//Namespace
