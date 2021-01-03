using System;

namespace RCNet.MathTools
{
    /// <summary>
    /// Implements a simple iterative error-driven search for the parameter's optimal value.
    /// </summary>
    [Serializable]
    public class ParamValFinder
    {
        //Constants
        /// <summary>
        /// The default number of inner points dividing the currently focused interval (relevant for nonzero span interval).
        /// </summary>
        public const int DefaultNumOfIntervalInnerPoints = 1;

        //Attributes
        private readonly int _numOfIntervalInnerPoints;
        private readonly TestCase[] _testCaseCollection;
        private int _currentTestCaseIdx;

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="cfg">The configuration.</param>
        public ParamValFinder(ParamValFinderSettings cfg)
        {
            if (cfg.NumOfSubIntervals == ParamValFinderSettings.AutoSubIntervalsNum)
            {
                if (cfg.Min == cfg.Max)
                {
                    _numOfIntervalInnerPoints = 0;
                    _testCaseCollection = new TestCase[1];
                    _testCaseCollection[0] = new TestCase() { ParamValue = cfg.Min, Error = double.NaN };
                    _currentTestCaseIdx = 0;
                    return;
                }
                else
                {
                    _numOfIntervalInnerPoints = DefaultNumOfIntervalInnerPoints;
                    _testCaseCollection = new TestCase[2 + _numOfIntervalInnerPoints];
                }

            }
            else
            {
                _numOfIntervalInnerPoints = cfg.NumOfSubIntervals;
                _testCaseCollection = new TestCase[2 + _numOfIntervalInnerPoints];
            }
            for (int i = 0; i < _testCaseCollection.Length; i++)
            {
                _testCaseCollection[i] = new TestCase() { ParamValue = double.NaN, Error = double.NaN };
            }
            Replan(cfg.Min, double.NaN, cfg.Max, double.NaN);
            return;
        }

        //Properties
        /// <summary>
        /// Gets the next value to be tested. NaN means that there is no next value to be tested.
        /// </summary>
        public double Next
        {
            get
            {
                return _testCaseCollection[_currentTestCaseIdx].Pending ? _testCaseCollection[_currentTestCaseIdx].ParamValue : double.NaN;
            }
        }

        //Methods
        /// <summary>
        /// Schedules new values for testing.
        /// </summary>
        /// <param name="min">The min value.</param>
        /// <param name="minErr">Error corresponding to min value.</param>
        /// <param name="max">The max value.</param>
        /// <param name="maxErr">Error corresponding to max value.</param>
        private void Replan(double min, double minErr, double max, double maxErr)
        {
            double stepSize = (max - min) / (_numOfIntervalInnerPoints + 1);
            //Focused interval boundaries
            _testCaseCollection[0].ParamValue = min;
            _testCaseCollection[0].Error = minErr;
            _testCaseCollection[_testCaseCollection.Length - 1].ParamValue = max;
            _testCaseCollection[_testCaseCollection.Length - 1].Error = maxErr;
            double paramValue = min + stepSize;
            for (int i = 1; i < _testCaseCollection.Length - 1; i++)
            {
                _testCaseCollection[i] = new TestCase { ParamValue = paramValue, Error = double.NaN };
                paramValue += stepSize;
            }
            _currentTestCaseIdx = _testCaseCollection[0].Done ? 1 : 0;
            return;
        }

        /// <summary>
        /// Evaluates an error and prepares the next value for testing.
        /// </summary>
        /// <param name="error">The error of the lastly tested value.</param>
        public void ProcessError(double error)
        {
            _testCaseCollection[_currentTestCaseIdx].Error = error;
            if (_testCaseCollection.Length > 1)
            {
                //Move to the next test case
                ++_currentTestCaseIdx;
                if (_currentTestCaseIdx == _testCaseCollection.Length || _testCaseCollection[_currentTestCaseIdx].Done)
                {
                    //We have evaluated all planed values
                    //Find index of param value having the smallest associated error
                    int bestIdx = 0;
                    for (int i = 1; i < _testCaseCollection.Length; i++)
                    {
                        if (_testCaseCollection[i].Error < _testCaseCollection[bestIdx].Error)
                        {
                            bestIdx = i;
                        }
                    }
                    //Narrow the searched interval
                    //Determine first (min value) and last (max value) indexes to be used as the new interval edges
                    int firstIdx = bestIdx > 0 ? bestIdx - 1 : bestIdx;
                    int lastIdx = bestIdx < _testCaseCollection.Length - 1 ? bestIdx + 1 : bestIdx;
                    if (firstIdx == 0 && lastIdx == _testCaseCollection.Length - 1)
                    {
                        //Avoid repeating the same cycle
                        if (_testCaseCollection[firstIdx].Error < _testCaseCollection[lastIdx].Error)
                        {
                            lastIdx = bestIdx;
                        }
                        else
                        {
                            firstIdx = bestIdx;
                        }
                    }
                    Replan(_testCaseCollection[firstIdx].ParamValue,
                           _testCaseCollection[firstIdx].Error,
                           _testCaseCollection[lastIdx].ParamValue,
                           _testCaseCollection[lastIdx].Error
                           );
                }
            }
            return;
        }

        //Inner classes
        [Serializable]
        private class TestCase
        {
            //Attribute properties
            public double ParamValue { get; set; }
            public double Error { get; set; }

            //Properties
            public bool Pending { get { return (double.IsNaN(Error)); } }
            public bool Done { get { return !Pending; } }

        }//TestCase

    }//ParamValFinder

}//Namespace
