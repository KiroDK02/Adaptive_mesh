using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdaptiveGrids;
using FEM;
using Quasar.Native;

namespace Core
{
    public class PardisoSLAE : ISLAE
    {
        public PardisoSLAE(PardisoMatrix matrix)
        {
            Matrix = matrix;

            RightPart = new double[Matrix.N];
        }

        public IMatrix Matrix { get; }

        public void AddLocalRightPart(int[] dofs, double[] lrp)
        {
            for (int i = 0; i < dofs.Length; ++i)
                LinqExtensions.ThreadSafeAdd(RightPart, dofs[i], lrp[i]);
        }

        public void AddFirstBoundaryConditions(int[] dofs, double[] lrp)
        {
            for (int i = 0; i < dofs.Length; ++i)
            {
                double value = lrp[i];
                LinqExtensions.ThreadSafeSet(RightPart, dofs[i], value);
                Matrix.Symmetrize(dofs[i], value, RightPart);
            }
        }
        public double CalcDiscrepancy(double[] solution)
        {
            double[] Ax = new double[Matrix.N];
            Matrix.MultVect(solution, Ax);

            double discrepancy = 0.0;
            double normRightPart = 0.0;

            for (int i = 0; i < Matrix.N; i++)
            {
                discrepancy += (Ax[i] - RightPart[i]) * (Ax[i] - RightPart[i]);
                normRightPart += RightPart[i] * RightPart[i];
            }

            return Math.Sqrt(discrepancy / normRightPart);
        }
        public void Clear()
        {
            Matrix.Clear();
            ClearRightPart();
        }
        public void ClearRightPart()
        {
            Array.Clear(RightPart, 0, RightPart.Length);
        }
        public double[] RightPart { get; }

    }

}