using System;

namespace RCNet.MathTools.PS
{
    /// <summary>
    /// Implements result driven iterative search for the optimal value of a given parameter
    /// </summary>
    [Serializable]
    public class ParamSeeker
    {
        //Constants
        /// <summary>
        /// Default number of inner points dividing focused interval (relevant for nonzero span interval)
        /// </summary>
        public const int DefaultNumOfIntervalInnerPoints = 1;

        //Attributes
        private readonly int _numOfIntervalInnerPoints;
        private readonly TestCase[] _testCaseCollection;
        private int _currentTestCaseIdx;

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="settings">Configuration parameters</param>
        public ParamSeeker(ParamSeekerSettings settings)
        {
            if (settings.NumOfSubIntervals == ParamSeekerSettings.AutoSubIntervalsNum)
            {
                if (settings.Min == settings.Max)
                {
                    _numOfIntervalInnerPoints = 0;
                    _testCaseCollection = new TestCase[1];
                    _testCaseCollection[0] = new TestCase() { ParamValue = settings.Min, Error = double.NaN };
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
                _numOfIntervalInnerPoints = settings.NumOfSubIntervals;
                _testCaseCollection = new TestCase[2 + _numOfIntervalInnerPoints];
            }
            for (int i = 0; i < _testCaseCollection.Length; i++)
            {
                _testCaseCollection[i] = new TestCase() { ParamValue = double.NaN, Error = double.NaN };
            }
            Replan(settings.Min, double.NaN, settings.Max, double.NaN);
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
                return _testCaseCollection[_currentTestCaseIdx].Pending ? _testCaseCollection[_currentTestCaseIdx].ParamValue : double.NaN;
            }
        }

        //Methods
        /// <summary>
        /// Initializes plan of values to be tested.
        /// </summary>
        /// <param name="min">Min value</param>
        /// <param name="minErr">Error corresponding to min value</param>
        /// <param name="max">Min value</param>
        /// <param name="maxErr">Error corresponding to max value</param>
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
        /// Evaluates given error and prepares next value to be tested
        /// </summary>
        /// <param name="error">Result error of the last tested value</param>
        public void ProcessError(double error)
        {
            _testCaseCollection[_currentTestCaseIdx].Error = error;
            if (_testCaseCollection.Length > 1)
            {
                //Move to next test case
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

    }//ParamSeeker
}//Namespace
