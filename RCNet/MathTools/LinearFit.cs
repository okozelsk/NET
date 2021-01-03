using RCNet.Extensions;
using System;
using System.Collections.Generic;

namespace RCNet.MathTools
{
    /// <summary>
    /// Implements the calculation of the coefficients of the line that best corresponds to the sample points.
    /// </summary>
    [Serializable]
    public class LinearFit
    {
        //Attributes
        private readonly List<FnPoint> _pointCollection;
        private double _sumOfX;
        private double _sumOfSquaredX;
        private double _sumOfY;
        private double _sumOfXYDotProduct;
        private bool _invalidated;
        private double _a;
        private double _b;

        //Constructor
        /// <summary>
        /// Creates an uninitialized instance.
        /// </summary>
        public LinearFit()
        {
            _pointCollection = new List<FnPoint>();
            Reset();
            return;
        }

        //Properties
        /// <summary>
        /// Gets the number of sample points.
        /// </summary>
        public int NumOfPoints { get { return _pointCollection.Count; } }

        /// <summary>
        /// Gets the A coefficient of the linear equation y = A*x + B.
        /// </summary>
        public double A
        {
            get
            {
                ComputeRegression();
                return _a;
            }
        }

        /// <summary>
        /// Gets the B coefficient of the linear equation y = A*x + B.
        /// </summary>
        public double B
        {
            get
            {
                ComputeRegression();
                return _b;
            }
        }

        //Methods
        /// <summary>
        /// Resets the instance to its initial state.
        /// </summary>
        public void Reset()
        {
            _pointCollection.Clear();
            _sumOfX = 0;
            _sumOfSquaredX = 0;
            _sumOfY = 0;
            _sumOfXYDotProduct = 0;
            _invalidated = false;
            _a = 0;
            _b = 0;
            return;
        }

        /// <summary>
        /// Adds a sample point.
        /// </summary>
        /// <param name="point">The sample point.</param>
        public void AddSamplePoint(FnPoint point)
        {
            _pointCollection.Add(point);
            _sumOfX += point.X;
            _sumOfSquaredX += point.X.Power(2);
            _sumOfY += point.Y;
            _sumOfXYDotProduct += point.X * point.Y;
            _invalidated = true;
            return;
        }

        /// <summary>
        /// Adds a sample point.
        /// </summary>
        /// <param name="x">The X coordinate of a sample point.</param>
        /// <param name="y">The Y coordinate of a sample point.</param>
        public void AddSamplePoint(double x, double y)
        {
            AddSamplePoint(new FnPoint(x, y));
            return;
        }

        private void ComputeRegression()
        {
            if (_invalidated)
            {
                if (_pointCollection.Count > 0)
                {
                    _a = ((_pointCollection.Count * _sumOfXYDotProduct) - (_sumOfX * _sumOfY)) /
                         ((_pointCollection.Count * _sumOfSquaredX) - _sumOfX.Power(2));
                    _b = ((_sumOfSquaredX * _sumOfY) - (_sumOfX * _sumOfXYDotProduct)) /
                         ((_pointCollection.Count * _sumOfSquaredX) - _sumOfX.Power(2));
                }
                else
                {
                    _a = 0;
                    _b = 0;
                }
                _invalidated = false;
            }
            return;
        }

        /// <summary>
        /// Computes the error statistics of Y values (computedY - sampleY).
        /// </summary>
        public BasicStat ComputeRegressionErrStat()
        {
            ComputeRegression();
            BasicStat errStat = new BasicStat();
            if (_pointCollection.Count > 0)
            {
                foreach (FnPoint point in _pointCollection)
                {
                    double regrY = _a * point.X + _b;
                    errStat.AddSample(Math.Abs(regrY - point.Y));
                }
            }
            return errStat;
        }

        /// <summary>
        /// Computes the Y value for the specified X value.
        /// </summary>
        /// <param name="x">The X value.</param>
        public double ComputeY(double x)
        {
            ComputeRegression();
            return _a * x + _b;
        }

    }//LinearFit

}//Namespace
