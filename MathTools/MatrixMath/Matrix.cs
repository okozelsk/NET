using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OKOSW.Extensions;

namespace OKOSW.MathTools.MatrixMath
{
    [Serializable]
    public class Matrix
    {
        //Constants
        //Attributes
        protected double[][] _data;
        
        //Constructors
        public Matrix(int rowsCount, int colsCount, double[] flatData = null)
        {
            _data = new double[rowsCount][];
            for (int row = 0; row < rowsCount; row++)
            {
                _data[row] = new double[colsCount];
            }
            if (flatData != null)
            {
                Set(flatData);
            }
            return;
        }

        public Matrix(Matrix sourceMatrix)
            :this(sourceMatrix._data)
        {
            return;
        }

        public Matrix(double[][] data)
        {
            _data = data.Clone2D();
            return;
        }

        //Properties
        public double[][] Data { get { return _data; } }
        public int RowsCount { get { return _data.Length; } }
        public int ColsCount { get { return _data[0].Length; } }
        public int Size { get { return RowsCount * ColsCount; } }
        public bool IsVector { get { return (RowsCount == 1 || ColsCount == 1); } }
        public bool Singular
        {
            get
            {
                for(int i = 0; i < RowsCount; i++)
                {
                    for(int j = 0; j < ColsCount; j++)
                    {
                        if(_data[i][j] != 0)
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
        }

        //Methods
        //Static methods
        /// <summary>
        /// Creates a matrix that has colData.Length rows and one column.
        /// </summary>
        /// <param name="colData">Column values</param>
        public static Matrix CreateSingleColumnMatrix(double[] colData)
        {
            Matrix result = new Matrix(colData.Length, 1);
            result.SetCol(0, colData);
            return result;
        }

        /// <summary>
        /// Creates a matrix that has one row and rowData.Length columns.
        /// </summary>
        /// <param name="rowData">Row values.</param>
        public static Matrix CreateRowMatrix(double[] rowData)
        {
            Matrix result = new Matrix(1, rowData.Length);
            result.SetRow(0, rowData);
            return result;
        }

        //Instance methods
        public void Set(double value = 0)
        {
            _data.Populate(value);
            return;
        }

        public void Set(double[] flatData)
        {
            int dataIndex = 0;
            for (int i = 0; i < RowsCount; i++)
            {
                for (int j = 0; j < ColsCount; j++)
                {
                    _data[i][j] = flatData[dataIndex++];
                }
            }
            return;
        }

        public void Set(Matrix source)
        {
            for (int i = 0; i < RowsCount; i++)
            {
                for (int j = 0; j < ColsCount; j++)
                {
                    _data[i][j] = source._data[i][j];
                }
            }
            return;
        }

        public void SetRow(int row, double value = 0)
        {
            _data[row].Populate(value);
            return;
        }

        public void SetRow(int row, double[] rowData)
        {
            rowData.CopyTo(_data[row], 0);
            return;
        }

        public void SetCol(int col, double value = 0)
        {
            for(int i = 0; i < _data.Length; i++)
            {
                _data[i][col] = value;
            }
            return;
        }

        public void SetCol(int col, double[] colData)
        {
            for (int i = 0; i < colData.Length; i++)
            {
                _data[i][col] = colData[i];
            }
            return;
        }

        /// <summary>
        /// Set a submatrix.
        /// </summary>
        /// <param name="fromRow">Initial row index</param>
        /// <param name="fromCol">Initial column index</param>
        /// <param name="source">Source matrix</param>
        public void SetSubMatrix(int fromRow, int fromCol, Matrix source)
        {
            for (int i = 0; i <= source.RowsCount; i++)
            {
                for (int j = 0; j <= source.ColsCount; j++)
                {
                    _data[fromRow + i][fromCol + j] = source._data[i][j];
                }
            }
            return;
        }

        public void GetFlatData(double[] flatData)
        {
            int dataIndex = 0;
            for (int i = 0; i < RowsCount; i++)
            {
                for (int j = 0; j < ColsCount; j++)
                {
                    flatData[dataIndex++] = _data[i][j];
                }
            }
            return;
        }

        public double[] GetFlatData()
        {
            double[] flatData = new double[Size];
            GetFlatData(flatData);
            return flatData;
        }

        public Matrix GetColSubMatrix(int col)
        {
            double[][] colMatrixData = new double[RowsCount][];
            for (int row = 0; row < RowsCount; row++)
            {
                colMatrixData[row] = new double[1];
                colMatrixData[row][0] = _data[row][col];
            }
            return new Matrix(colMatrixData);
        }

        public Matrix GetRowSubMatrix(int row)
        {
            double[][] rowMatrixData = new double[1][];
            rowMatrixData[0] = new double[ColsCount];
            for (int col = 0; col < ColsCount; col++)
            {
                rowMatrixData[0][col] = _data[row][col];
            }
            return new Matrix(rowMatrixData);
        }

        /// <summary>
        /// Gets a submatrix.
        /// </summary>
        /// <param name="fromRow">Initial row index.</param>
        /// <param name="toRow">Final row index.</param>
        /// <param name="fromCol">Initial column index.</param>
        /// <param name="toCol">Final column index.</param>
        public Matrix GetSubMatrix(int fromRow, int toRow, int fromCol, int toCol)
        {
            Matrix resultMatrix = new Matrix(toRow - fromRow + 1, toCol - fromCol + 1);
            for (int i = fromRow; i <= toRow; i++)
            {
                for (int j = fromCol; j <= toCol; j++)
                {
                    resultMatrix._data[i - fromRow][j - fromCol] = _data[i][j];
                }
            }
            return resultMatrix;
        }

        public double[][] GetDataClone()
        {
            return _data.Clone2D();
        }

        public Matrix Clone()
        {
            return new Matrix(this);
        }

        /// <summary>
        /// Add the specified value to the specified row and column of the matrix.
        /// </summary>
        /// <param name="row">The row to add to.</param>
        /// <param name="col">The column to add to.</param>
        /// <param name="value">The value to add.</param>
        public void Add(int row, int col, double value)
        {
            _data[row][col] += value;
            return;
        }

        /// <summary>
        /// Add the values of specified matrix into the this matrix.
        /// </summary>
        public void Add(Matrix values)
        {
            for(int i = 0; i < RowsCount; i++)
            {
                for(int j = 0; j < ColsCount; j++)
                {
                    _data[i][j] = values._data[i][j];
                }
            }
            return;
        }

        /// <summary>
        /// Sum all of the values in the matrix.
        /// </summary>
        public double Sum(Matrix values)
        {
            double sum = 0;
            for (int i = 0; i < RowsCount; i++)
            {
                for (int j = 0; j < ColsCount; j++)
                {
                    sum += _data[i][j];
                }
            }
            return sum;
        }

    }//Matrix
} //Namespace
