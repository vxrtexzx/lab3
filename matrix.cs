using System;
using System.Text;

public class Matrix
{
    private double[,] values;

    public int Rows { get; }
    public int Columns { get; }

    public Matrix(double[,] values)
    {
        Rows = values.GetLength(0);
        Columns = values.GetLength(1);
        this.values = values;
    }

    public double this[int i, int j]
    {
        get => values[i, j];
        set => values[i, j] = value;
    }

    public static Matrix Zero(int rows, int columns)
    {
        double[,] values = new double[rows, columns];
        return new Matrix(values);
    }

    public static Matrix Identity(int size)
    {
        double[,] values = new double[size, size];
        for (int i = 0; i < size; i++)
        {
            values[i, i] = 1.0;
        }
        return new Matrix(values);
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                sb.Append(values[i, j]).Append(' ');
            }
            sb.AppendLine();
        }
        return sb.ToString();
    }

    public override bool Equals(object? obj)
    {
        if (obj is Matrix other && Rows == other.Rows && Columns == other.Columns)
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    if (values[i, j] != other[i, j])
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        return false;
    }

    public override int GetHashCode()
    {
        int hash = 17;
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                hash = hash * 23 + values[i, j].GetHashCode();
            }
        }
        return hash;
    }

    public static Matrix operator +(Matrix left, Matrix right)
    {
        return MatrixOperations.Add(left, right);
    }

    public static Matrix operator -(Matrix left, Matrix right)
    {
        return MatrixOperations.Subtract(left, right);
    }

    public static Matrix operator *(Matrix left, Matrix right)
    {
        return MatrixOperations.Multiply(left, right);
    }

    public static Matrix operator *(Matrix matrix, double scalar)
    {
        return MatrixOperations.Multiply(matrix, scalar);
    }

    public static Matrix operator *(double scalar, Matrix matrix)
    {
        return MatrixOperations.Multiply(matrix, scalar);
    }
}
