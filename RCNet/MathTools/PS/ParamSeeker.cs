using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.MathTools;

namespace RCNet.MathTools.PS
{
    /// <summary>
    /// The class implements an result driven iterative search for the optimal value of a given parameter
    /// </summary>
    [Serializable]
    public class ParamSeeker
    {
        //Attributes
        private readonly ParamSeekerSettings _settings;
        private readonly ValueError[] _plannedValues;
        private int _planIdx;

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="settings">Configuration parameters</param>
        public ParamSeeker(ParamSeekerSettings settings)
        {
            _settings = settings.DeepClone();
            _plannedValues = new ValueError[_settings.NumOfSteps + 1];
            Replan(_settings.Min, double.NaN, _settings.Max, double.NaN);
            return;
        }

        //Properties
        /// <summary>
        /// Next value to be tested.
        /// If NaN is returned, seeker has finished and there is no next value to be tested.
        /// </summary>
        public double Next
        {
            get
            {
                return _plannedValues[_planIdx].Value;
            }
        }

        //Methods
        /// <summary>
        /// Plans set of values to be tested.
        /// </summary>
        /// <param name="min">Min value</param>
        /// <param name="minErr">Error corresponding to min value</param>
        /// <param name="max">Min value</param>
        /// <param name="maxErr">Error corresponding to max value</param>
        private void Replan(double min, double minErr, double max, double maxErr)
        {
            double stepSize = (max - min) / _settings.NumOfSteps;
            double value = min;
            _planIdx = 0;
            for(int i = 0; i <= _settings.NumOfSteps; i++)
            {
                _plannedValues[i] = new ValueError { Value = value, Error = double.NaN };
                if(i == 0 && !double.IsNaN(minErr))
                {
                    //Error is already known
                    _plannedValues[i].Error = minErr;
                    _planIdx = 1;
                }
                else if(i == _settings.NumOfSteps && !double.IsNaN(maxErr))
                {
                    //Error is already known
                    _plannedValues[i].Error = maxErr;
                }
                value += stepSize;
            }
            return;
        }

        /// <summary>
        /// Evaluates given error and prepares next value to be tested
        /// </summary>
        /// <param name="error">Result error of the last tested value</param>
        public void ProcessError(double error)
        {
            //Store _plannedValues[_planIdx].Value resulting error  
            _plannedValues[_planIdx].Error = error;
            //Move to next
            ++_planIdx;
            if(_planIdx == _plannedValues.Length || (_planIdx == (_plannedValues.Length - 1) && !double.IsNaN(_plannedValues[_planIdx].Error)))
            {
                //We have evaluated all planed values
                //Find index of value with the smallest associated error
                int bestIdx = 0;
                for(int i = 1; i < _plannedValues.Length; i++)
                {
                    if(_plannedValues[i].Error < _plannedValues[bestIdx].Error)
                    {
                        bestIdx = i;
                    }
                }
                //Narrow the searched interval
                //Determine first (min value) and last (max value) indexes to be used as the edge
                int firstIdx = bestIdx > 0 ? bestIdx - 1 : bestIdx;
                int lastIdx = bestIdx < _plannedValues.Length - 1 ? bestIdx + 1 : bestIdx;
                if(firstIdx == 0 && lastIdx == _plannedValues.Length - 1)
                {
                    //Avoid repeating the same cycle
                    if(_plannedValues[firstIdx].Error < _plannedValues[lastIdx].Error)
                    {
                        lastIdx = bestIdx;
                    }
                    else
                    {
                        firstIdx = bestIdx;
                    }
                }
                Replan(_plannedValues[firstIdx].Value,
                       _plannedValues[firstIdx].Error,
                       _plannedValues[lastIdx].Value,
                       _plannedValues[lastIdx].Error
                       );
            }
            return;
        }

        //Inner classes
        [Serializable]
        private class ValueError
        {
            public double Value { get; set; }
            public double Error { get; set; }
        }

    }//ParamSeeker
}//Namespace
