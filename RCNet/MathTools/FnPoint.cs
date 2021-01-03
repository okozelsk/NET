using System;

namespace RCNet.MathTools
{
    /// <summary>
    /// Implements the function point (the pair of X and Y=f(X) values).
    /// </summary>
    [Serializable]
    public class FnPoint
    {
        //Attribute properties
        /// <summary>
        /// The X value.
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// The Y value.
        /// </summary>
        public double Y { get; set; }

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="x">The X value.</param>
        /// <param name="y">The Y value.</param>
        public FnPoint(double x = 0, double y = 0)
        {
            Set(x, y);
            return;
        }

        //Methods
        /// <summary>
        /// Sets the X and Y values.
        /// </summary>
        /// <param name="x">The X value.</param>
        /// <param name="y">The Y value.</param>
        public void Set(double x, double y)
        {
            X = x;
            Y = y;
            return;
        }

    }//FnPoint

}//Namespace
