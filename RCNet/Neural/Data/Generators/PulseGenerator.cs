using RCNet.Extensions;
using System;

namespace RCNet.Neural.Data.Generators
{
    /// <summary>
    /// Implements the constant pulse generator.
    /// </summary>
    [Serializable]
    public class PulseGenerator : IGenerator
    {
        //Enums
        /// <summary>
        /// The pulse timing mode.
        /// </summary>
        public enum TimingMode
        {
            /// <summary>
            /// The period of the pulses is constant.
            /// </summary>
            Constant,
            /// <summary>
            /// The period of the pulses follows the Uniform distribution.
            /// </summary>
            Uniform,
            /// <summary>
            /// The period of the pulses follows the Gaussian distribution.
            /// </summary>
            Gaussian,
            /// <summary>
            /// The period of the pulses follows the Poisson (Exponential) distribution.
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
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="signal">The pulse signal value.</param>
        /// <param name="avgPeriod">The pulse average leak.</param>
        /// <param name="mode">The pulse timing mode.</param>
        public PulseGenerator(double signal, double avgPeriod, TimingMode mode)
        {
            _signal = signal;
            _avgPeriod = Math.Abs(avgPeriod);
            _mode = mode;
            Reset();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="cfg">The configuration</param>
        public PulseGenerator(PulseGeneratorSettings cfg)
        {
            _signal = cfg.Signal;
            _avgPeriod = cfg.AvgPeriod;
            _mode = cfg.Mode;
            Reset();
            return;
        }

        //Methods
        /// <summary>
        /// Schedules the next pulse.
        /// </summary>
        private void ScheduleNextPulse()
        {
            double minPeriod = 1d;
            double maxPeriod = 1d + 2d * (_avgPeriod - 1d);
            double spanPeriod = maxPeriod - minPeriod;
            int timeIncrement;
            switch (_mode)
            {
                case TimingMode.Constant:
                    timeIncrement = (_t == 0) ? 1 : (int)Math.Round(_avgPeriod);
                    break;
                case TimingMode.Uniform:
                    timeIncrement = (int)Math.Round(_rand.NextRangedUniformDouble(minPeriod, maxPeriod));
                    break;
                case TimingMode.Gaussian:
                    timeIncrement = (int)Math.Round(_rand.NextRangedGaussianDouble(_avgPeriod, spanPeriod / 6d, minPeriod, maxPeriod));
                    break;
                case TimingMode.Poisson:
                    timeIncrement = (int)Math.Round(_rand.NextExponentialDouble(_avgPeriod));
                    break;
                default:
                    timeIncrement = 0;
                    break;
            }
            if (timeIncrement <= 0)
            {
                timeIncrement = 1;
            }
            _nextPulseTime = _t + timeIncrement;
            return;
        }

        /// <inheritdoc />
        public void Reset()
        {
            _rand = new Random(0);
            _t = 0;
            ScheduleNextPulse();
            return;
        }

        /// <inheritdoc />
        public double Next()
        {
            ++_t;
            if (_t == _nextPulseTime)
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
