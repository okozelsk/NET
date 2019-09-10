using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using RCNet.Extensions;
using RCNet.MathTools.VectorMath;

namespace RCNet.MathTools.MatrixMath
{
    /// <summary>
    /// Class represents the mathematical matrix of double values.
    /// Class does not support the sparse matrix format.
    /// </summary>
    [Serializable]
    public class Matrix
    {
        //Constants
        //Attributes
        /// <summary>
        /// Matrix data stored in array of arrays of double.
        /// </summary>
        protected double[][] _data;

        //Constructors
        /// <summary>
        /// Instantiates the matrix.
        /// </summary>
        /// <param name="numOfRows">Number of rows</param>
        /// <param name="numOfCols">Number of columns</param>
        /// <param name="flatData">Optional. The data to be copied into the matrix (in the flat form)</param>
        public Matrix(int numOfRows, int numOfCols, double[] flatData = null)
        {
            _data = new double[numOfRows][];
            Parallel.For(0, numOfRows, row =>
            {
                _data[row] = new double[numOfCols];
                _data[row].Populate(0);
            });
            if (flatData != null)
            {
                Set(flatData);
            }
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="sourceMatrix"></param>
        public Matrix(Matrix sourceMatrix)
            :this(sourceMatrix._data)
        {
            return;
        }

        /// <summary>
        /// Instantiates matrix based on dimensions of given array of arrays and copies the data into the new matrix.
        /// </summary>
        /// <param name="data">Matrix data</param>
        /// <param name="copy">Specifies if to create copy of the data or adopt given instance</param>
        public Matrix(double[][] data, bool copy = true)
        {
            _data = copy ? data.Clone2D() : data;
            return;
        }

        //Properties
        /// <summary>
        /// Matrix data
        /// </summary>
        public double[][] Data { get { return _data; } }
        /// <summary>
        /// Number of matrix rows
        /// </summary>
        public int NumOfRows { get { return _data.Length; } }
        /// <summary>
        /// Number of matrix columns
        /// </summary>
        public int NumOfCols { get { return _data[0].Length; } }
        /// <summary>
        /// Size of the matrix = (NumOfRows * NumOfCols)
        /// </summary>
        public int Size { get { return NumOfRows * NumOfCols; } }
        /// <summary>
        /// Indicates whether the matrix is a vector.
        /// </summary>
        public bool IsVector { get { return (NumOfRows == 1 || NumOfCols == 1); } }
        /// <summary>
        /// Indicates whether the matrix is the singular matrix.
        /// </summary>
        public bool IsSingular
        {
            get
            {
                for(int i = 0; i < NumOfRows; i++)
                {
                    for(int j = 0; j < NumOfCols; j++)
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

        /// <summary>
        /// Checkes if the matrix is squared (equal number of rows and columns)
        /// </summary>
        public bool IsSquared
        {
            get
            {
                return (NumOfCols == NumOfRows);
            }
        }

        //Methods
        //Static methods
        /// <summary>
        /// Calculates hypotenuse.
        /// https://en.wikipedia.org/wiki/Hypot
        /// </summary>
        /// <param name="x">The x value.</param>
        /// <param name="y">The y value.</param>
        public static double Hypotenuse(double x, double y)
        {
            double hypot = 0d;
            if (Math.Abs(x) > Math.Abs(y))
            {
                hypot = Math.Abs(x) * Math.Sqrt(1d + (y / x).Power(2));
            }
            else if (y != 0)
            {
                hypot = Math.Abs(y) * Math.Sqrt(1d + (x / y).Power(2));
            }
            return hypot;
        }

        /// <summary>
        /// Creates a matrix that has data.Length rows and one column.
        /// </summary>
        /// <param name="data">Column values</param>
        public static Matrix CreateSingleColumnMatrix(double[] data)
        {
            Matrix result = new Matrix(data.Length, 1);
            result.SetCol(0, data);
            return result;
        }

        /// <summary>
        /// Creates a matrix that has one row and data.Length columns.
        /// </summary>
        /// <param name="data">Row values</param>
        public static Matrix CreateSingleRowMatrix(double[] data)
        {
            Matrix result = new Matrix(1, data.Length);
            result.SetRow(0, data);
            return result;
        }

        //Instance methods
        /// <summary>
        /// Fills the whole matrix with given value
        /// </summary>
        /// <param name="value">Value</param>
        public void Set(double value = 0)
        {
            _data.Populate(value);
            return;
        }

        /// <summary>
        /// Fills the whole matrix with the values from given 1D array (flat format)
        /// </summary>
        /// <param name="flatData">Data in the flat format</param>
        public void Set(double[] flatData)
        {
            int dataIndex = 0;
            for (int i = 0; i < NumOfRows; i++)
            {
                for (int j = 0; j < NumOfCols; j++)
                {
                    _data[i][j] = flatData[dataIndex++];
                }
            }
            return;
        }

        /// <summary>
        /// Copies all values from the source matrix into the this matrix.
        /// </summary>
        /// <param name="source">Source matrix</param>
        public void Set(Matrix source)
        {
            Parallel.For(0, NumOfRows, i =>
            {
                for (int j = 0; j < NumOfCols; j++)
                {
                    _data[i][j] = source._data[i][j];
                }
            });
            return;
        }

        /// <summary>
        /// Fills specified row with given value
        /// </summary>
        /// <param name="row">Matrix row</param>
        /// <param name="value">Value</param>
        public void SetRow(int row, double value = 0)
        {
            _data[row].Populate(value);
            return;
        }

        /// <summary>
        /// Copies values from the given array into the specified matrix row.
        /// </summary>
        /// <param name="row">Matrix row</param>
        /// <param name="data">Data</param>
        public void SetRow(int row, double[] data)
        {
            data.CopyTo(_data[row], 0);
            return;
        }

        /// <summary>
        /// Fills specified matrix column with the specified value.
        /// </summary>
        /// <param name="col">Matrix column</param>
        /// <param name="value">Value</param>
        public void SetCol(int col, double value = 0)
        {
            for(int i = 0; i < _data.Length; i++)
            {
                _data[i][col] = value;
            }
            return;
        }

        /// <summary>
        /// Copies values from the given array into the specified matrix column.
        /// </summary>
        /// <param name="col">Matrix column</param>
        /// <param name="data">Data</param>
        public void SetCol(int col, double[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                _data[i][col] = data[i];
            }
            return;
        }

        /// <summary>
        /// Copies values from the source matrix into the this matrix starting from specified row and col position.
        /// </summary>
        /// <param name="fromRow">Initial row index</param>
        /// <param name="fromCol">Initial column index</param>
        /// <param name="source">Source matrix</param>
        public void SetSubMatrix(int fromRow, int fromCol, Matrix source)
        {
            for (int i = 0; i <= source.NumOfRows; i++)
            {
                for (int j = 0; j <= source.NumOfCols; j++)
                {
                    _data[fromRow + i][fromCol + j] = source._data[i][j];
                }
            }
            return;
        }

        /// <summary>
        /// Copies data of this matrix into the 1D array of double values (flat format)
        /// </summary>
        /// <param name="flatData">Array to be filled</param>
        public void GetFlatData(double[] flatData)
        {
            int dataIndex = 0;
            for (int i = 0; i < NumOfRows; i++)
            {
                for (int j = 0; j < NumOfCols; j++)
                {
                    flatData[dataIndex++] = _data[i][j];
                }
            }
            return;
        }

        /// <summary>
        /// Converts data of this matrix into the 1D array of double values (flat format)
        /// </summary>
        public double[] GetFlatData()
        {
            double[] flatData = new double[Size];
            GetFlatData(flatData);
            return flatData;
        }

        /// <summary>
        /// Creates a matrix having the same number of rows as this matrix and one column.
        /// Column values are copied from the specified column of this matrix.
        /// </summary>
        /// <param name="col">This matrix column index</param>
        public Matrix GetColSubMatrix(int col)
        {
            double[][] colMatrixData = new double[NumOfRows][];
            for (int row = 0; row < NumOfRows; row++)
            {
                colMatrixData[row] = new double[1];
                colMatrixData[row][0] = _data[row][col];
            }
            return new Matrix(colMatrixData);
        }

        /// <summary>
        /// Creates a matrix having the same number of columns as this matrix and one row.
        /// Row values are copied from the specified row of this matrix.
        /// </summary>
        /// <param name="row">This matrix row index</param>
        public Matrix GetRowSubMatrix(int row)
        {
            double[][] rowMatrixData = new double[1][];
            rowMatrixData[0] = new double[NumOfCols];
            for (int col = 0; col < NumOfCols; col++)
            {
                rowMatrixData[0][col] = _data[row][col];
            }
            return new Matrix(rowMatrixData);
        }

        /// <summary>
        /// Creates a submatrix of this matrix.
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

        /// <summary>
        /// Returns clone of the internal array of arrays.
        /// </summary>
        public double[][] GetDataClone()
        {
            return _data.Clone2D();
        }

        /// <summary>
        /// Creates the deep copy
        /// </summary>
        public Matrix DeepClone()
        {
            return new Matrix(this);
        }

        /// <summary>
        /// See the base.
        /// </summary>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// See the base.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            Matrix cmpMatrix = obj as Matrix;
            if (NumOfCols != cmpMatrix.NumOfCols || NumOfRows != cmpMatrix.NumOfRows)
            {
                return false;
            }
            for(int i = 0; i < NumOfRows; i++)
            {
                for(int j = 0; j < NumOfCols; j++)
                {
                    if(_data[i][j] != cmpMatrix._data[i][j])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Computes A + B
        /// </summary>
        /// <param name="A">Matrix A</param>
        /// <param name="B">Matrix B</param>
        /// <returns>Resulting matrix</returns>
        public static Matrix Add(Matrix A, Matrix B)
        {
            int rowsA = A.NumOfRows;
            int colsA = A.NumOfCols;
            int rowsB = B.NumOfRows;
            int colsB = B.NumOfCols;
            if (colsA != colsB || rowsA != rowsB)
            {
                throw new Exception("Dimensions of A must equal to dimensions of B");
            }
            double[][] resultData = new double[rowsA][];
            var rangePartitioner = Partitioner.Create(0, rowsA);
            Parallel.ForEach(rangePartitioner, range =>
            {
                double[] rowDataResult, rowDataA, rowDataB;
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    rowDataResult = new double[colsA];
                    rowDataA = A._data[i];
                    rowDataB = B._data[i];
                    for (int j = 0; j < colsA; j++)
                    {
                        rowDataResult[j] = rowDataA[j] + rowDataB[j];
                    }
                    resultData[i] = rowDataResult;
                }
            });
            return new Matrix(resultData, false);
        }

        /// <summary>
        /// Computes A + B
        /// </summary>
        /// <param name="A">Matrix A</param>
        /// <param name="B">Matrix B</param>
        /// <returns>Resulting matrix</returns>
        public static Matrix operator +(Matrix A, Matrix B)
        {
            return Add(A, B);
        }

        /// <summary>
        /// Adds matrix B
        /// </summary>
        /// <param name="B">Matrix B</param>
        public void Add(Matrix B)
        {
            int rowsB = B.NumOfRows;
            int colsB = B.NumOfCols;
            if (NumOfCols != colsB || NumOfRows != rowsB)
            {
                throw new Exception("Dimensions of B must equal to dimensions of this matrix");
            }
            var rangePartitioner = Partitioner.Create(0, rowsB);
            Parallel.ForEach(rangePartitioner, range =>
            {
                double[] rowData, rowDataB;
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    rowData = _data[i];
                    rowDataB = B._data[i];
                    for (int j = 0; j < colsB; j++)
                    {
                        rowData[j] += rowDataB[j];
                    }
                }
            });
            return;
        }

        /// <summary>
        /// Adds scalar value s to main diagonal of square matrix A.
        /// </summary>
        /// <param name="A">Matrix A</param>
        /// <param name="s">Scalar value</param>
        /// <returns>Resulting matrix</returns>
        public static Matrix AddScalarToDiagonal(Matrix A, double s)
        {
            int rowsA = A.NumOfRows;
            int colsA = A.NumOfCols;
            if(rowsA != colsA)
            {
                throw new Exception("Matrix A must be a square matrix (rows dimension = columns dimension)");
            }
            double[][] resultData = new double[rowsA][];
            var rangePartitioner = Partitioner.Create(0, rowsA);
            Parallel.ForEach(rangePartitioner, range =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    resultData[i] = (double[])A._data[i].Clone();
                    resultData[i][i] += s;
                }
            });
            return new Matrix(resultData, false);
        }

        /// <summary>
        /// Adds scalar value to main diagonal of this square matrix.
        /// </summary>
        /// <param name="s">Scalar value</param>
        public void AddScalarToDiagonal(double s)
        {
            if (!IsSquared)
            {
                throw new Exception("Matrix must be a square matrix (rows dimension = columns dimension)");
            }
            var rangePartitioner = Partitioner.Create(0, NumOfRows);
            Parallel.ForEach(rangePartitioner, range =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    _data[i][i] += s;
                }
            });
            return;
        }

        /// <summary>
        /// Computes A - B
        /// </summary>
        /// <param name="A">Matrix A</param>
        /// <param name="B">Matrix B</param>
        /// <returns>Resulting matrix</returns>
        public static Matrix Substract(Matrix A, Matrix B)
        {
            int rowsA = A.NumOfRows;
            int colsA = A.NumOfCols;
            int rowsB = B.NumOfRows;
            int colsB = B.NumOfCols;
            if (colsA != colsB || rowsA != rowsB)
            {
                throw new Exception("Dimensions of A must equal to dimensions of B");
            }
            double[][] resultData = new double[rowsA][];
            var rangePartitioner = Partitioner.Create(0, rowsA);
            Parallel.ForEach(rangePartitioner, range =>
            {
                double[] rowDataResult, rowDataA, rowDataB;
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    rowDataResult = new double[colsA];
                    rowDataA = A._data[i];
                    rowDataB = B._data[i];
                    for (int j = 0; j < colsA; j++)
                    {
                        rowDataResult[j] = rowDataA[j] - rowDataB[j];
                    }
                    resultData[i] = rowDataResult;
                }
            });
            return new Matrix(resultData, false);
        }

        /// <summary>
        /// Computes A - B
        /// </summary>
        /// <param name="A">Matrix A</param>
        /// <param name="B">Matrix B</param>
        /// <returns>Resulting matrix</returns>
        public static Matrix operator -(Matrix A, Matrix B)
        {
            return Substract(A, B);
        }

        /// <summary>
        /// Substracts matrix B from this matrix
        /// </summary>
        /// <param name="B">Matrix B</param>
        public void Substract(Matrix B)
        {
            int rowsB = B.NumOfRows;
            int colsB = B.NumOfCols;
            if (NumOfCols != colsB || NumOfRows != rowsB)
            {
                throw new Exception("Dimensions of B must equal to dimensions of this matrix");
            }
            var rangePartitioner = Partitioner.Create(0, rowsB);
            Parallel.ForEach(rangePartitioner, range =>
            {
                double[] rowData, rowDataB;
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    rowData = _data[i];
                    rowDataB = B._data[i];
                    for (int j = 0; j < colsB; j++)
                    {
                        rowData[j] -= rowDataB[j];
                    }
                }
            });
            return;
        }

        /// <summary>
        /// Multiplies matrix A and matrix B.
        /// Function is single-threaded.
        /// </summary>
        /// <param name="A">Matrix A</param>
        /// <param name="B">Matrix B</param>
        /// <returns>Resulting matrix</returns>
        public static Matrix ST_Multiply(Matrix A, Matrix B)
        {
            int rowsA = A.NumOfRows;
            int colsA = A.NumOfCols;
            int rowsB = B.NumOfRows;
            int colsB = B.NumOfCols;
            if (colsA != rowsB)
            {
                throw new Exception("Number of columns of A must be equal to number of rows of B");
            }
            Matrix R = new Matrix(rowsA, colsB);
            for (int i = 0; i < rowsA; i++)
            {
                for (int j = 0; j < colsB; j++)
                {
                    for (int k = 0; k < rowsB; k++)
                    {
                        R._data[i][j] += A._data[i][k] * B._data[k][j];
                    }
                }
            }
            return R;
        }

        /// <summary>
        /// Multiplies matrix A and matrix B
        /// </summary>
        /// <param name="A">Matrix A</param>
        /// <param name="B">Matrix B</param>
        /// <returns>Resulting matrix</returns>
        public static Matrix Multiply(Matrix A, Matrix B)
        {
            int rowsA = A.NumOfRows;
            int colsA = A.NumOfCols;
            int rowsB = B.NumOfRows;
            int colsB = B.NumOfCols;
            if (colsA != rowsB)
            {
                throw new Exception("Number of columns of A must be equal to number of rows of B");
            }
            var rangePartitioner = Partitioner.Create(0, rowsA);
            double[][] resultData = new double[rowsA][];
            Parallel.ForEach(rangePartitioner, range =>
            {
                double[] rowDataA, rowDataB, rowDataResult;
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    rowDataA = A._data[i];
                    rowDataResult = new double[colsB];
                    rowDataResult.Populate(0);
                    for (int j = 0; j < rowsB; j++)
                    {
                        rowDataB = B._data[j];
                        double valA = rowDataA[j];
                        for (int k = 0; k < colsB; k++)
                        {
                            rowDataResult[k] += valA * rowDataB[k];
                        }
                    }
                    resultData[i] = rowDataResult;
                };
            });
            return new Matrix(resultData, false);
        }

        /// <summary>
        /// Multiplies matrix A and matrix B
        /// </summary>
        /// <param name="A">Matrix A</param>
        /// <param name="B">Matrix B</param>
        /// <returns>Resulting matrix</returns>
        public static Matrix operator *(Matrix A, Matrix B)
        {
            return Multiply(A, B);
        }

        /// <summary>
        /// Multiplies matrix A and vector v.
        /// </summary>
        /// <param name="A">Matrix A</param>
        /// <param name="v">Vector</param>
        /// <returns>Resulting vector</returns>
        public static Vector Multiply(Matrix A, Vector v)
        {
            int rowsA = A.NumOfRows;
            int colsA = A.NumOfCols;
            if (colsA != v.Length)
            {
                throw new Exception("Number of columns of A must be equal to length of vector v");
            }
            double[] resultData = new double[rowsA];
            double[] vData = v.Data;
            int vDataLength = vData.Length;
            var rangePartitioner = Partitioner.Create(0, rowsA);
            Parallel.ForEach(rangePartitioner, range =>
            {
                for(int i = range.Item1; i < range.Item2; i++)
                {
                    double[] dataRowA = A._data[i];
                    double sum = 0;
                    for (int j = 0; j < vDataLength; j++)
                    {
                        sum += dataRowA[j] * vData[j];
                    }
                    resultData[i] = sum;
                }
            });
            return new Vector(resultData, false);
        }

        /// <summary>
        /// Multiplies matrix A and vector v.
        /// Function is single-threaded.
        /// </summary>
        /// <param name="A">Matrix A</param>
        /// <param name="v">Vector</param>
        /// <returns>Resulting vector</returns>
        public static Vector ST_Multiply(Matrix A, Vector v)
        {
            int rowsA = A.NumOfRows;
            int colsA = A.NumOfCols;
            if (colsA != v.Length)
            {
                throw new Exception("Number of columns of A must be equal to length of vector v");
            }
            double[] resultData = new double[rowsA];
            resultData.Populate(0);
            for (int i = 0; i < rowsA; i++)
            {
                for (int j = 0; j < v.Length; j++)
                {
                    resultData[i] += A._data[i][j] * v.Data[j];
                }
            }
            return new Vector(resultData, false);
        }

        /// <summary>
        /// Multiplies matrix A and vector V
        /// </summary>
        /// <param name="A">Matrix A</param>
        /// <param name="v">Vector</param>
        /// <returns>Resulting vector</returns>
        public static Vector operator *(Matrix A, Vector v)
        {
            return Multiply(A, v);
        }

        /// <summary>
        /// Multiplies this matrix by vector v
        /// </summary>
        /// <param name="v">Vector v</param>
        /// <returns>Resulting vector</returns>
        public Vector Multiply(Vector v)
        {
            if (NumOfCols != v.Length)
            {
                throw new Exception("Number of columns of the matrix must be equal to length of vector V");
            }
            int rows = NumOfRows;
            double[] resultData = new double[rows];
            double[] vData = v.Data;
            var rangePartitioner = Partitioner.Create(0, NumOfRows);
            Parallel.ForEach(rangePartitioner, range =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    double[] dataRow = _data[i];
                    double sum = 0;
                    for (int j = 0; j < rows; j++)
                    {
                        sum += dataRow[j] * vData[j];
                    }
                    resultData[i] = sum;
                }
            });
            return new Vector(resultData, false);
        }

        /// <summary>
        /// Multiplies matrix A and scalar s
        /// </summary>
        /// <param name="A">Matrix A</param>
        /// <param name="s">Scalar value</param>
        /// <returns>Resulting matrix</returns>
        public static Matrix Multiply(Matrix A, double s)
        {
            int rowsA = A.NumOfRows;
            int colsA = A.NumOfCols;
            double[][] dataR = new double[rowsA][];
            var rangePartitioner = Partitioner.Create(0, rowsA);
            Parallel.ForEach(rangePartitioner, range =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    double[] rowDataA = A._data[i];
                    double[] rowDataR = new double[colsA];
                    for (int j = 0; j < colsA; j++)
                    {
                        rowDataR[j] = rowDataA[j] * s;
                    }
                    dataR[i] = rowDataR;
                }
            });
            return new Matrix(dataR, false);
        }

        /// <summary>
        /// Multiplies matrix A and scalar s
        /// </summary>
        /// <param name="A">Matrix A</param>
        /// <param name="s">Scalar value</param>
        /// <returns>Resulting matrix</returns>
        public static Matrix operator *(Matrix A, double s)
        {
            return Multiply(A, s);
        }

        /// <summary>
        /// Multiplies elements in this matrix by scalar value
        /// </summary>
        /// <param name="s">Scalar value</param>
        public void Multiply(double s)
        {
            int cols = NumOfCols;
            var rangePartitioner = Partitioner.Create(0, NumOfRows);
            Parallel.ForEach(rangePartitioner, range =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    double[] rowData = _data[i];
                    for (int j = 0; j < cols; j++)
                    {
                        rowData[j] *= s;
                    }
                }
            });
            return;
        }

        /// <summary>
        /// Retuns transposed given matrix.
        /// </summary>
        /// <param name="A">Matrix A</param>
        public static Matrix Transpose(Matrix A)
        {
            int rowsA = A.NumOfRows;
            int colsA = A.NumOfCols;
            int rowsR = colsA;
            int colsR = rowsA;
            double[][] dataR = new double[rowsR][];
            var rangePartitioner = Partitioner.Create(0, rowsR);
            Parallel.ForEach(rangePartitioner, range =>
            {
                for(int i = range.Item1; i < range.Item2; i++)
                {
                    double[] rowData = new double[colsR];
                    for (int j = 0; j < colsR; j++)
                    {
                        rowData[j] = A._data[j][i];
                    }
                    dataR[i] = rowData;
                }
            });
            return new Matrix(dataR, false);
        }

        /// <summary>
        /// Retuns transposed this matrix.
        /// </summary>
        public Matrix Transpose()
        {
            int rowsR = NumOfCols;
            int colsR = NumOfRows;
            double[][] dataR = new double[rowsR][];
            var rangePartitioner = Partitioner.Create(0, rowsR);
            Parallel.ForEach(rangePartitioner, range =>
            {
                for(int i = range.Item1; i < range.Item2; i++)
                {
                    double[] rowData = new double[colsR];
                    for (int j = 0; j < colsR; j++)
                    {
                        rowData[j] = _data[j][i];
                    }
                    dataR[i] = rowData;
                }
            });
            return new Matrix(dataR, false);
        }

        /// <summary>
        /// Retuns transposed this matrix.
        /// Function is single-threaded.
        /// </summary>
        public Matrix ST_Transpose()
        {
            int rowsR = NumOfCols;
            int colsR = NumOfRows;
            double[][] dataR = new double[rowsR][];
            for(int i = 0; i < rowsR; i++)
            {
                dataR[i] = new double[colsR];
                for (int j = 0; j < colsR; j++)
                {
                    dataR[i][j] = _data[j][i];
                }
            }
            return new Matrix(dataR, false);
        }

        /// <summary>
        /// Computes inverse matrix of this matrix.
        /// Function implements enhanced algorithm originally proposed by Ahmad FAROOQ and Khan HAMID in the publication:
        /// <see href="https://www.researchgate.net/publication/220337321_An_Efficient_and_Generic_Algorithm_for_Matrix_Inversion">An Efficient and Generic Algorithm for Matrix Inversion</see>
        /// (parallel processing was introduced to improve performance) 
        /// 
        /// Additionaly was implemented flexible off-diagonal pivot selection using dictionary
        /// approach to build final inverted matrix proposed by Hafsa Athar Jafree, Muhammad Imtiaz, Syed Inayatullah,
        /// Fozia Hanif Khan and Tajuddin Nizami in the publication:
        /// <see href="https://arxiv.org/ftp/arxiv/papers/1304/1304.6893.pdf">A space efficient flexible pivot selection approach to evaluate determinant and inverse of a matrix</see>
        /// </summary>
        /// <param name="preferAccuracy">If true, function favorites accuracy (selects pivots having max abs values) over the execution speed (selects diagonal pivots).</param>
        /// <returns>Inverse matrix of this matrix</returns>
        public Matrix Inverse(bool preferAccuracy = true)
        {
            if (!IsSquared)
            {
                throw new Exception("Matrix must be squared.");
            }
            //Dimension to be used within the function
            int size = NumOfRows;
            //Prepare ranges for parallel processing
            var rangePartitioner = Partitioner.Create(0, size);
            //Dictionary
            //Computational dictionary matrix - a copy of this matrix in the beginning
            Matrix dictMatrix = new Matrix(this);
            //Rows
            int[] dictRows = new int[size];
            dictRows.Indices();
            //Available pivot rows
            List<int> availableDictPivotRows = new List<int>(dictRows);
            //Cols
            int[] dictCols = new int[size];
            dictCols.Indices();
            //Available pivot columns
            List<int> availableDictPivotCols = new List<int>(dictCols);
            double minPivotValue = 1e-20;
            //Indicates that some changes were made in dictionary
            bool dictChanged = false;

            //Main loop
            for (int n = 0; n < size; n++)
            {
                //Pivot
                int pivotRow = -1;
                int pivotCol = -1;
                //Select pivot element
                if (!preferAccuracy)
                {
                    //Simply use diagonal element
                    pivotRow = n;
                    pivotCol = n;
                    //Validate pivot value
                    if (Math.Abs(dictMatrix._data[pivotRow][pivotCol]) < minPivotValue)
                    {
                        //Failed
                        throw new Exception($"Absolute value of the diagonal Pivot at row {pivotRow} is too small. Pivot = {dictMatrix._data[pivotRow][pivotCol]}.");
                    }
                }
                else
                {
                    //Find new Pivot element having highest absolute value
                    double selectedPivotValue = 0;
                    int selectedPivotRowListIdx = -1, selectedPivotColListIdx = -1;
                    for (int pivotRowListIdx = 0; pivotRowListIdx < availableDictPivotRows.Count; pivotRowListIdx++)
                    {
                        int row = availableDictPivotRows[pivotRowListIdx];
                        for (int pivotColListIdx = 0; pivotColListIdx < availableDictPivotCols.Count; pivotColListIdx++)
                        {
                            int col = availableDictPivotCols[pivotColListIdx];
                            double elemAbsValue = Math.Abs(dictMatrix._data[row][col]);
                            if (elemAbsValue >= minPivotValue && (pivotColListIdx == 0 || elemAbsValue > Math.Abs(selectedPivotValue)))
                            {
                                selectedPivotValue = dictMatrix._data[row][col];
                                selectedPivotRowListIdx = pivotRowListIdx;
                                pivotRow = row;
                                selectedPivotColListIdx = pivotColListIdx;
                                pivotCol = col;
                            }
                        }
                    }
                    //Test success of pivot selection
                    if (selectedPivotValue == 0)
                    {
                        //Pivot was not selected
                        throw new Exception($"Can't select Pivot in step {n + 1}. No matrix element has enaugh absolute value >= {minPivotValue}.");
                    }
                    //Remove row from available pivot rows
                    availableDictPivotRows.RemoveAt(selectedPivotRowListIdx);
                    //Remove col from available pivot cols
                    availableDictPivotCols.RemoveAt(selectedPivotColListIdx);
                }
                //Pick up Pivot value
                double pivot = dictMatrix._data[pivotRow][pivotCol];
                //Update dictionary
                if (pivotRow != n || pivotCol != n)
                {
                    dictRows[pivotRow] = pivotCol;
                    dictCols[pivotCol] = pivotRow;
                    dictChanged = true;
                }
                //Pivot processing
                //Pivot column elements
                Parallel.ForEach(rangePartitioner, range =>
                {
                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        if (i != pivotRow)
                        {
                            dictMatrix._data[i][pivotCol] /= -pivot;
                        }
                    }
                });
                //Whole matrix except pivot row and column elements
                Parallel.ForEach(rangePartitioner, range =>
                {
                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        if (i != pivotRow)
                        {
                            for (int j = 0; j < size; j++)
                            {
                                if (j != pivotCol)
                                {
                                    dictMatrix._data[i][j] += dictMatrix._data[pivotRow][j] * dictMatrix._data[i][pivotCol];
                                }
                            }
                        }
                    }
                });
                ;
                //Pivot row elements
                Parallel.ForEach(rangePartitioner, range =>
                {
                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        if (i != pivotCol)
                        {
                            dictMatrix._data[pivotRow][i] /= pivot;
                        }
                    }
                });
                ;
                //Pivot element
                dictMatrix._data[pivotRow][pivotCol] = 1d / pivot;
            }
            //Result finalization
            if(!dictChanged)
            {
                //No transpositionings so use directly IM as the result
                return dictMatrix;
            }
            else
            {
                //Use dictionary and build resulting matrix
                Matrix resultingMatrix = new Matrix(size, size);
                Parallel.ForEach(rangePartitioner, range =>
                {
                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        for (int j = 0; j < size; j++)
                        {
                            resultingMatrix._data[i][j] = dictMatrix._data[dictCols[j]][dictRows[i]];
                        }
                    }
                });
                return resultingMatrix;
            }
        }

        /// <summary>
        /// Method uses power iteration method to estimate largest eigen value (in magnitude) and corresponding eigen vector.
        /// Matrix must be squared.
        /// </summary>
        /// <param name="resultEigenVector">Returned corresponding eigen vector</param>
        /// <param name="maxNumOfIterations">Maximum number of method's iterations</param>
        /// <param name="stopDelta">Stopping corvengence delta between previous iteration and current iteration</param>
        /// <returns>Estimated largest eigen value (in magnitude)</returns>
        public double EstimateLargestEigenValue(out double[] resultEigenVector, int maxNumOfIterations = 1000, double stopDelta = 1e-6)
        {
            //Firstly check squared matrix
            if (!IsSquared)
            {
                throw new Exception("Matrix must be squared.");
            }
            //Local variables
            //Iteration initialization
            int iteration = 0;
            double iterationDelta = 0;
            int n = NumOfRows;
            double[] tmpVector = new double[n];
            double eigenValue = 0;
            double[] eigenVector = new double[n];
            eigenVector.Populate(1);
            //Results
            double minDelta = double.MaxValue;
            double resultEigenValue = 0;
            resultEigenVector = new double[n];
            //Convergence loop
            do
            {
                Parallel.For(0, n, i =>
                {
                    tmpVector[i] = 0;
                    for (int j = 0; j < n; j++)
                    {
                        tmpVector[i] += _data[i][j] * eigenVector[j];
                    }
                });

                //Find element having max magnitude (= new eigen value)
                double prevEigenValue = eigenValue;
                eigenValue = tmpVector[0];
                for (int i = 1; i < n; i++)
                {
                    if (Math.Abs(tmpVector[i]) > Math.Abs(eigenValue))
                    {
                        eigenValue = tmpVector[i];
                    }
                }

                //Prepare new normalized eigenVector
                for (int i = 0; i < n; i++)
                {
                    eigenVector[i] = tmpVector[i] / eigenValue;
                }

                //Iteration results
                ++iteration;
                iterationDelta = Math.Abs(eigenValue - prevEigenValue);
                if (minDelta > iterationDelta)
                {
                    minDelta = iterationDelta;
                    resultEigenValue = eigenValue;
                    eigenVector.CopyTo(resultEigenVector, 0);
                }

            } while (iteration < maxNumOfIterations && iterationDelta > stopDelta);
            return resultEigenValue;
        }



    }//Matrix

} //Namespace

