using RCNet.Extensions;
using System;

namespace RCNet.MathTools.Differential
{
    /// <summary>
    /// Moving deep differentiator
    /// </summary>
    [Serializable]
    public class MovingDifferentiator
    {
        //Constants
        /// <summary>
        /// Minimum allowed depth
        /// </summary>
        public const int MinDepth = 2;
        /// <summary>
        /// Maximum allowed depth
        /// </summary>
        public const int MaxDepth = 1024 + 1;

        //Attribute properties
        /// <summary>
        /// Input data range
        /// </summary>
        public Interval InputDataRange { get; }
        /// <summary>
        /// Differentiator depth
        /// </summary>
        public int Depth { get; }

        //Attributes
        private readonly double[][] _diffDataCollection;

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="inputDataRange">Range of input data</param>
        /// <param name="depth">Differentiator depth</param>
        public MovingDifferentiator(Interval inputDataRange, int depth = MaxDepth)
        {
            InputDataRange = inputDataRange.DeepClone();
            Depth = Math.Max(Math.Min(MaxDepth, depth), MinDepth);
            _diffDataCollection = new double[Depth][];
            for (int i = 0; i < Depth; i++)
            {
                _diffDataCollection[i] = new double[2];
            }
            Reset();
            return;
        }

        //Properties
        /// <summary>
        /// Range of the differential data
        /// </summary>
        public static Interval DiffDataRange { get { return Interval.IntN1P1; } }

        //Methods
        /// <summary>
        /// Resets differentiator to its initial state
        /// </summary>
        public void Reset()
        {
            for (int i = 0; i < Depth; i++)
            {
                _diffDataCollection[i].Populate(double.NaN);
            }
            return;
        }

        /// <summary>
        /// Updates differentiator by the next data value
        /// </summary>
        /// <param name="nextDataValue">Next data value</param>
        public void Update(double nextDataValue)
        {
            //Base
            _diffDataCollection[0].ShiftLeft(double.NaN);
            _diffDataCollection[0][1] = nextDataValue;
            //Differences
            for (int i = 1; i < Depth; i++)
            {
                if (!double.IsNaN(_diffDataCollection[i - 1][0]))
                {
                    _diffDataCollection[i].ShiftLeft(double.NaN);
                    _diffDataCollection[i][1] = (_diffDataCollection[i - 1][1] - _diffDataCollection[i - 1][0]) / InputDataRange.Span;
                }
                else
                {
                    break;
                }
            }
            return;
        }

        /// <summary>
        /// Returns difference at specified depth or NaN if not enaugh data
        /// </summary>
        /// <param name="depth">Depth</param>
        public double GetDifferenceAt(int depth)
        {
            if (!double.IsNaN(_diffDataCollection[depth][1]))
            {
                return _diffDataCollection[depth][1];
            }
            else
            {
                return _diffDataCollection[depth][0];
            }
        }

        /// <summary>
        /// Checks if is available difference at the specified depth
        /// </summary>
        /// <param name="depth">Depth</param>
        public bool IsDifferenceAvailable(int depth)
        {
            return !double.IsNaN(GetDifferenceAt(depth));
        }

    }//MovingDifferentiator

}//Namespace
