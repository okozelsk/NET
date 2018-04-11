using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCNet.Extensions;

namespace RCNet.MathTools
{
    /// <summary>
    /// Class computes coefficients of the straight line best fitting specified sample points.
    /// </summary>
    [Serializable]
    public class LinearFit
    {
        //Attributes
        private List<Point2D> _pointCollection;
        private double _sumOfX;
        private double _sumOfSquaredX;
        private double _sumOfY;
        private double _sumOfXYDotProduct;
        private bool _invalidated;
        private double _a;
        private double _b;

        //Constructor
        /// <summary>
        /// Constructs an uninitialized instance
        /// </summary>
        public LinearFit()
        {
            _pointCollection = new List<Point2D>();
            Reset();
            return;
        }

        //Properties
        /// <summary>
        /// Number of sample points
        /// </summary>
        public int NumOfPoints { get { return _pointCollection.Count; } }
        /// <summary>
        /// The A coeficient of the linear equation y = A*x + B
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
        /// The B coeficient of the linear equation y = A*x + B
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
        /// Resets instance to its initial state
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
        /// Adds sample point
        /// </summary>
        /// <param name="point">Sample point</param>
        public void AddSamplePoint(Point2D point)
        {
            _pointCollection.Add(point);
            _sumOfX += point._x;
            _sumOfSquaredX += point._x.Power(2);
            _sumOfY += point._y;
            _sumOfXYDotProduct += point._x * point._y;
            _invalidated = true;
            return;
        }

        /// <summary>
        /// Adds a sample point
        /// </summary>
        /// <param name="x">X coordinate of a sample point</param>
        /// <param name="y">Y coordinate of a sample point</param>
        public void AddSamplePoint(double x, double y)
        {
            AddSamplePoint(new Point2D(x, y));
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
        /// Computes error statistics of Y values (computedY - sampleY)
        /// </summary>
        public BasicStat ComputeRegressionErrStat()
        {
            ComputeRegression();
            BasicStat errStat = new BasicStat();
            if (_pointCollection.Count > 0)
            {
                foreach (Point2D point in _pointCollection)
                {
                    double regrY = _a * point._x + _b;
                    errStat.AddSampleValue(Math.Abs(regrY - point._y));
                }
            }
            return errStat;
        }

        /// <summary>
        /// Computes the Y value for given X value
        /// </summary>
        /// <param name="x">X value</param>
        public double ComputeY (double x)
        {
            ComputeRegression();
            return _a * x + _b;
        }

    }//LinearFit
}//Namespace
