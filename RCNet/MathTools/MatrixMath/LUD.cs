﻿using RCNet.Extensions;
using RCNet.MathTools.VectorMath;
using System;

namespace RCNet.MathTools.MatrixMath
{

    /// <summary>
    /// Implements the LU (Lowed-Upper) decomposition of a square matrix.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class is based on a class from the public domain JAMA package.
    /// </para>
    /// <para>
    /// http://math.nist.gov/javanumerics/jama/
    /// </para>
    /// </remarks>
    public class LUD
    {
        //Attributes
        private readonly int _size;
        private readonly double[][] _lu;

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="matrix">The square matrix.</param>
        public LUD(Matrix matrix)
        {
            //Check the matrix is the square matrix
            if (!matrix.IsSquareMatrix)
            {
                throw new ArgumentException("Matrix is not square.", "matrix");
            }
            //Initialization
            _size = matrix.NumOfRows;
            //LU data
            _lu = new double[_size][];
            for (int i = 0; i < _size; i++)
            {
                _lu[i] = new double[_size];
                _lu[i].Populate(0);
            }
            //Decomposition
            for (int i = 0; i < _size; i++)
            {
                for (int j = i; j < _size; j++)
                {
                    double sum = 0;
                    for (int k = 0; k < i; k++)
                    {
                        sum += _lu[i][k] * _lu[k][j];
                    }
                    _lu[i][j] = matrix.Data[i][j] - sum;
                }
                for (int j = i + 1; j < _size; j++)
                {
                    double sum = 0;
                    for (int k = 0; k < i; k++)
                    {
                        sum += _lu[j][k] * _lu[k][i];
                    }
                    _lu[j][i] = (1d / _lu[i][i]) * (matrix.Data[j][i] - sum);
                }
            }
            return;
        }

        //Properties
        /// <summary>
        /// LU data
        /// </summary>
        public double[][] LU { get { return _lu; } }

        //Methods
        /// <summary>
        /// Solves system of linear equations A*x = b.
        /// </summary>
        /// <param name="b">Vector of desired results (b).</param>
        /// <returns>Vector of computed linear coefficients (x).</returns>
        public Vector Solve(Vector b)
        {
            if (_size != b.Length)
            {
                throw new ArgumentException($"Invalid length ({b.Length}) of specified vector. Expected length {_size}.", "b");
            }
            double[] bData = b.Data;
            //Find solution
            double[] y = new double[_size];
            for (int i = 0; i < _size; i++)
            {
                double sum = 0;
                for (int k = 0; k < i; k++)
                {
                    sum += _lu[i][k] * y[k];
                }
                y[i] = bData[i] - sum;
            }
            double[] x = new double[_size];
            for (int i = _size - 1; i >= 0; i--)
            {
                double sum = 0;
                for (int k = i + 1; k < _size; k++)
                {
                    sum += _lu[i][k] * x[k];
                }
                x[i] = (1d / _lu[i][i]) * (y[i] - sum);
            }
            return new Vector(x, false);
        }

    }//LUD

}//Namespace
