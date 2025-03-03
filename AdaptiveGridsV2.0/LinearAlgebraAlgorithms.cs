using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quasar.Native;

namespace AdaptiveGrids
{
    public static class LinearAlgebraAlgorithms
    {
        public static double[] MultiplyMatrixVector(double[,] matrix, double[] vector, double coeff = 1d)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            if (cols != vector.Length)
            {
                throw new ArgumentException("Количество столбцов матрицы должно совпадать с длиной вектора.");
            }

            double[] result = new double[rows];

            for (int i = 0; i < rows; i++)
            {
                double sum = 0;
                for (int j = 0; j < cols; j++)
                {
                    sum += matrix[i, j] * vector[j];
                }
                result[i] = coeff * sum;
            }


            return result;
        }

        public static double[] MultiplyMatrixVector(double[,] matrix, ReadOnlySpan<double> vector, double coeff = 1d)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            if (cols != vector.Length)
            {
                throw new ArgumentException("Количество столбцов матрицы должно совпадать с длиной вектора.");
            }

            double[] result = new double[rows];

            for (int i = 0; i < rows; i++)
            {
                double sum = 0;
                for (int j = 0; j < cols; j++)
                {
                    sum += matrix[i, j] * vector[j];
                }
                result[i] = coeff * sum;
            }


            return result;
        }
    }
}
