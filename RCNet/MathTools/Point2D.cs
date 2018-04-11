using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RCNet.MathTools
{
    /// <summary>
    /// Class represents a function point (pair of X and Y value)
    /// </summary>
    [Serializable]
    public class Point2D
    {
        //Attributes
        /// <summary>
        /// X value
        /// </summary>
        public double _x;
        /// <summary>
        /// Y value
        /// </summary>
        public double _y;

        //Constructor
        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="x">X value</param>
        /// <param name="y">Y value</param>
        public Point2D(double x = 0, double y = 0)
        {
            Set(x, y);
            return;
        }

        //Methods
        /// <summary>
        /// Sets X and Y value
        /// </summary>
        /// <param name="x">X value</param>
        /// <param name="y">Y value</param>
        public void Set(double x, double y)
        {
            _x = x;
            _y = y;
            return;
        }

    }//Point2D
}//Namespace
