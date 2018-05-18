using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Extensions;

namespace RCNet.MathTools.VectorMath
{
    /// <summary>
    /// Represents a vector and implements vector simple operations
    /// </summary>
    [Serializable]
    public class Vector
    {
        private double[] _data;

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="source">Source vector</param>
        public Vector(Vector source)
        {
            _data = (double[])source._data.Clone();
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="data">Source data</param>
        public Vector(double[] data)
        {
            _data = (double[])data.Clone();
            return;
        }

        /// <summary>
        /// Creates an initialized instance (all elements sets to zero)
        /// </summary>
        /// <param name="length">Vector length</param>
        public Vector(int length)
        {
            _data = new double[length];
            Set(0);
            return;
        }

        //Properties
        /// <summary>
        /// Vector length
        /// </summary>
        public int Length { get { return _data.Length; } }
        
        /// <summary>
        /// Vector data
        /// </summary>
        public double[] Data { get { return _data; } }

        //Methods
        /// <summary>
        /// Sets all values to specified value
        /// </summary>
        /// <param name="value">Value to be populated</param>
        public void Set(double value = 0)
        {
            _data.Populate(value);
        }

        /// <summary>
        /// Creates a copy
        /// </summary>
        public Vector Clone()
        {
            return new Vector(this);
        }

        /// <summary>
        /// Access to vector's element
        /// </summary>
        /// <param name="idx">Element index</param>
        public double this[int idx]
        {
            get
            {
                return _data[idx];
            }
            set
            {
                _data[idx] = value;
            }
        }

        //Operations
        /// <summary>Sums vectors.</summary>
        /// <param name="v1">First vector</param>
        /// <param name="v2">Second vector</param>
        public static Vector operator +(Vector v1, Vector v2)
        {
            Vector result = v1.Clone();
            for(int i = 0; i < v2._data.Length; i++)
            {
                result._data[i] += v2._data[i];
            }
            return result;
        }

        /// <summary>Adds a scalar to a vector.</summary>
        /// <param name="v">Vector</param>
        /// <param name="s">Scalar</param>
        public static Vector operator +(Vector v, double s)
        {
            Vector result = v.Clone();
            for (int i = 0; i < v._data.Length; i++)
            {
                result._data[i] += s;
            }
            return result;
        }

        /// <summary>Adds a scalar to a vector.</summary>
        /// <param name="s">Scalar</param>
        /// <param name="v">Vector</param>
        public static Vector operator +(double s, Vector v)
        {
            return v + s;
        }

        //Operations
        /// <summary>Substracts second vector from first vector.</summary>
        /// <param name="v1">First vector</param>
        /// <param name="v2">Second vector</param>
        public static Vector operator -(Vector v1, Vector v2)
        {
            Vector result = v1.Clone();
            for (int i = 0; i < v2._data.Length; i++)
            {
                result._data[i] -= v2._data[i];
            }
            return result;
        }

        /// <summary>Substracts a scalar from a vector.</summary>
        /// <param name="v">Vector</param>
        /// <param name="s">Scalar</param>
        public static Vector operator -(Vector v, double s)
        {
            Vector result = v.Clone();
            for (int i = 0; i < v._data.Length; i++)
            {
                result._data[i] -= s;
            }
            return result;
        }

        /// <summary>Multiplies vectors elements</summary>
        /// <param name="v1">First vector</param>
        /// <param name="v2">Second vector</param>
        public static Vector operator *(Vector v1, Vector v2)
        {
            Vector result = v1.Clone();
            for (int i = 0; i < v2._data.Length; i++)
            {
                result._data[i] *= v2._data[i];
            }
            return result;
        }

        /// <summary>Multiplies a vector by a scalar</summary>
        /// <param name="v">Vector</param>
        /// <param name="s">Scalar</param>
        public static Vector operator *(Vector v, double s)
        {
            Vector result = v.Clone();
            for (int i = 0; i < v._data.Length; i++)
            {
                result._data[i] *= s;
            }
            return result;
        }

        /// <summary>Multiplies a vector by a scalar</summary>
        /// <param name="s">Scalar</param>
        /// <param name="v">Vector</param>
        public static Vector operator *(double s, Vector v)
        {
            return v * s;
        }

        /// <summary>Divides vectors elements</summary>
        /// <param name="v1">First vector (numerator)</param>
        /// <param name="v2">Second vector (denominator)</param>
        public static Vector operator /(Vector v1, Vector v2)
        {
            Vector result = v1.Clone();
            for (int i = 0; i < v2._data.Length; i++)
            {
                result._data[i] /= v2._data[i];
            }
            return result;
        }

        /// <summary>Divides vector by a scalar</summary>
        /// <param name="v">Vector</param>
        /// <param name="s">Scalar</param>
        public static Vector operator /(Vector v, double s)
        {
            Vector result = v.Clone();
            for (int i = 0; i < v._data.Length; i++)
            {
                result._data[i] /= s;
            }
            return result;
        }

    }//Vector
}//Namespace
