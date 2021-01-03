using RCNet.Extensions;
using System;

namespace RCNet.MathTools.VectorMath
{
    /// <summary>
    /// Implements the vector.
    /// </summary>
    [Serializable]
    public class Vector
    {
        //Attributes
        private readonly double[] _data;

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source vector.</param>
        public Vector(Vector source)
        {
            _data = (double[])source._data.Clone();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="data">The source data.</param>
        /// <param name="copy">Specifies whether to create a copy of the source data or whether to use the source data directly.</param>
        public Vector(double[] data, bool copy = true)
        {
            _data = copy ? (double[])data.Clone() : data;
            return;
        }

        /// <summary>
        /// Creates an initialized zeroized instance.
        /// </summary>
        /// <param name="length">The vector length.</param>
        public Vector(int length)
        {
            _data = new double[length];
            Set(0);
            return;
        }

        //Properties
        /// <summary>
        /// Gets the vector length.
        /// </summary>
        public int Length { get { return _data.Length; } }

        /// <summary>
        /// Gets the vector data.
        /// </summary>
        public double[] Data { get { return _data; } }

        //Methods
        /// <summary>
        /// Sets the vector to specified value.
        /// </summary>
        /// <param name="value">The value to be populated.</param>
        public void Set(double value = 0)
        {
            _data.Populate(value);
        }

        /// <summary>
        /// Clones the vector.
        /// </summary>
        public Vector Clone()
        {
            return new Vector(this);
        }

        /// <summary>
        /// Gets or sets an element.
        /// </summary>
        /// <param name="idx">The element index.</param>
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

        //Operators
        /// <summary>
        /// Computes v1 + v2.
        /// </summary>
        /// <param name="v1">The vector v1.</param>
        /// <param name="v2">The vector v2.</param>
        /// <returns>The resulting vector.</returns>
        public static Vector operator +(Vector v1, Vector v2)
        {
            Vector result = v1.Clone();
            for (int i = 0; i < v2._data.Length; i++)
            {
                result._data[i] += v2._data[i];
            }
            return result;
        }

        /// <summary>
        /// Adds a scalar to the vector.
        /// </summary>
        /// <param name="v">The vector.</param>
        /// <param name="s">The scalar.</param>
        public static Vector operator +(Vector v, double s)
        {
            Vector result = v.Clone();
            for (int i = 0; i < v._data.Length; i++)
            {
                result._data[i] += s;
            }
            return result;
        }

        /// <inheritdoc cref="operator +(Vector, double)"/>
        public static Vector operator +(double s, Vector v)
        {
            return v + s;
        }

        /// <summary>
        /// Computes v1 - v2.
        /// </summary>
        /// <param name="v1">The vector v1.</param>
        /// <param name="v2">The vector v2.</param>
        /// <returns>The resulting vector.</returns>
        public static Vector operator -(Vector v1, Vector v2)
        {
            Vector result = v1.Clone();
            for (int i = 0; i < v2._data.Length; i++)
            {
                result._data[i] -= v2._data[i];
            }
            return result;
        }

        /// <summary>
        /// Substracts a scalar from the vector.
        /// </summary>
        /// <param name="v">The vector.</param>
        /// <param name="s">The scalar.</param>
        public static Vector operator -(Vector v, double s)
        {
            Vector result = v.Clone();
            for (int i = 0; i < v._data.Length; i++)
            {
                result._data[i] -= s;
            }
            return result;
        }

        /// <summary>
        /// Computes v1 * v2.
        /// </summary>
        /// <param name="v1">The vector v1.</param>
        /// <param name="v2">The vector v2.</param>
        /// <returns>The resulting vector.</returns>
        public static Vector operator *(Vector v1, Vector v2)
        {
            Vector result = v1.Clone();
            for (int i = 0; i < v2._data.Length; i++)
            {
                result._data[i] *= v2._data[i];
            }
            return result;
        }

        /// <summary>
        /// Multiplies a vector by the scalar.
        /// </summary>
        /// <param name="v">The vector.</param>
        /// <param name="s">The scalar.</param>
        public static Vector operator *(Vector v, double s)
        {
            Vector result = v.Clone();
            for (int i = 0; i < v._data.Length; i++)
            {
                result._data[i] *= s;
            }
            return result;
        }

        /// <inheritdoc cref="operator *(Vector, double)"/>
        public static Vector operator *(double s, Vector v)
        {
            return v * s;
        }

        /// <summary>
        /// Computes v1 / v2.
        /// </summary>
        /// <param name="v1">The vector v1.</param>
        /// <param name="v2">The vector v2.</param>
        /// <returns>The resulting vector.</returns>
        public static Vector operator /(Vector v1, Vector v2)
        {
            Vector result = v1.Clone();
            for (int i = 0; i < v2._data.Length; i++)
            {
                result._data[i] /= v2._data[i];
            }
            return result;
        }

        /// <summary>
        /// Divides a vector by the scalar.
        /// </summary>
        /// <param name="v">The vector.</param>
        /// <param name="s">The scalar.</param>
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
