using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using FEM;
using Meshes;

namespace AdaptiveGrids
{
    public class TimeMesh : ITimeMesh
    {

        public TimeMesh(double[] t)
        {
            int n = t.Length;

            T = t;
            baseT = t;

            for (int i = 0; i < n - 1; ++i)
            {
                refineParams.splitCount.Add(1);
                refineParams.stretchRatio.Add(1);
            }
        }

        public TimeMesh(double[] baseTimeMesh, RefineParams refParams)
        {
            int n = baseTimeMesh.Length;

            if (n - 1 != refParams.splitCount.Count)
                throw new ArgumentException("Количество интервалов в базовой временной сетке не совпадает с количеством интервалов для разбиения.");

            if (n - 1 != refParams.stretchRatio.Count)
                throw new ArgumentException("Количество интервалов в базовой временной сетке не совпадает с количеством коэффициентов для разбиения.");

            baseT = baseTimeMesh;
            refineParams = refParams;
            T = CreateTimeMesh();
        }

        public double this[int i] { get => T[i]; }

        double[] baseT;
        RefineParams refineParams = new();
        double[] T { get; set; }
        double[][] Coeffs = new double[2][];

        public void ChangeCoefs(double[] coefs)
        {
            Coeffs[1] = Coeffs[0];
            Coeffs[0] = coefs;
        }
        public double[] Coefs(int i)
        {
            switch (i)
            {
                case 1:
                    return Coeffs[0];
                case 2:
                    return Coeffs[1];
                default:
                    throw new ArgumentException();
            }
        }

        const double Eps = 1e-14;
        public bool IsChangedStep(int i)
        {
            if (i > 2)
            {
                double deltaT = T[i] - T[i - 1];
                double deltaT1 = T[i - 1] - T[i - 2];
                double deltaT2 = T[i - 2] - T[i - 3];


                if (Math.Abs(deltaT - deltaT1) / deltaT < Eps && Math.Abs(deltaT1 - deltaT2) / deltaT1 < Eps)
                    return false;

                return true;
            }

            return true;
        }
        public int Size() => T.Length;

        public void DoubleMesh()
        {
            ChangeRefineParams();

            T = CreateTimeMesh();
        }

        void ChangeRefineParams()
        {
            for (int i = 0; i < refineParams.splitCount.Count; ++i)
            {
                refineParams.splitCount[i] *= 2;

                refineParams.stretchRatio[i] = Math.Sqrt(refineParams.stretchRatio[i]);
            }
        }

        double[] CreateTimeMesh()
        {
            int n = baseT.Length;

            int newN = 1;
            for (int i = 0; i < n - 1; ++i)
            {
                newN += refineParams.splitCount[i];
            }

            double[] newT = new double[newN];

            newT[0] = baseT[0];
            int sum = 0;
            for (int i = 0; i < n - 1; ++i)
            {
                int numIntervals = refineParams.splitCount[i];
                double coef = refineParams.stretchRatio[i];

                double step = 0;
                if (coef == 1d)
                {
                    step = (baseT[i + 1] - baseT[i]) / numIntervals;

                    for (int j = 1; j < numIntervals; ++j)
                    {
                        newT[j + sum] = baseT[i] + step * j;
                    }
                }
                else
                {
                    step = (baseT[i + 1] - baseT[i]) * (1 - coef) / (1 - Math.Pow(coef, numIntervals));

                    for (int j = 1; j < numIntervals; ++j)
                    {
                        newT[j + sum] = baseT[i] + step * (1 - Math.Pow(coef, j)) / (1 - coef);
                    }
                }

                sum += numIntervals;
                newT[sum] = baseT[i + 1];
            }

            return newT;

        }
    }
}
