using System;
using System.Threading.Tasks;

public static class MatrixOperations
{
    public static Matrix Transpose(Matrix matrix)
    {
        double[,] resultValues = new double[matrix.Columns, matrix.Rows];
        for (int i = 0; i < matrix.Rows; i++)
        {
            for (int j = 0; j < matrix.Columns; j++)
            {
                resultValues[j, i] = matrix[i, j];
            }
        }
        return new Matrix(resultValues);
    }

    public static Matrix Add(Matrix left, Matrix right)
    {
        if (left.Rows != right.Rows || left.Columns != right.Columns)
        {
            throw new InvalidOperationException("Matrix dimensions must match for addition.");
        }

        double[,] resultValues = new double[left.Rows, left.Columns];
        for (int i = 0; i < left.Rows; i++)
        {
            for (int j = 0; j < left.Columns; j++)
            {
                resultValues[i, j] = left[i, j] + right[i, j];
            }
        }
        return new Matrix(resultValues);
    }

    public static Matrix Subtract(Matrix left, Matrix right)
    {
        if (left.Rows != right.Rows || left.Columns != right.Columns)
        {
            throw new InvalidOperationException("Matrix dimensions must match for subtraction.");
        }

        double[,] resultValues = new double[left.Rows, left.Columns];
        for (int i = 0; i < left.Rows; i++)
        {
            for (int j = 0; j < left.Columns; j++)
            {
                resultValues[i, j] = left[i, j] - right[i, j];
            }
        }
        return new Matrix(resultValues);
    }

    public static Matrix Multiply(Matrix left, Matrix right)
    {
        if (left.Columns != right.Rows)
        {
            throw new InvalidOperationException("Matrix dimensions are not valid for multiplication.");
        }

        double[,] resultValues = new double[left.Rows, right.Columns];
        Parallel.For(0, left.Rows, i =>
        {
            for (int j = 0; j < right.Columns; j++)
            {
                double sum = 0;
                for (int k = 0; k < left.Columns; k++)
                {
                    sum += left[i, k] * right[k, j];
                }
                resultValues[i, j] = sum;
            }
        });

        return new Matrix(resultValues);
    }

    public static Matrix Multiply(Matrix matrix, double scalar)
    {
        double[,] resultValues = new double[matrix.Rows, matrix.Columns];
        for (int i = 0; i < matrix.Rows; i++)
        {
            for (int j = 0; j < matrix.Columns; j++)
            {
                resultValues[i, j] = matrix[i, j] * scalar;
            }
        }
        return new Matrix(resultValues);
    }
}
