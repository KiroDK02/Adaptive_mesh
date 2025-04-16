using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FEM;
using TelmaCore;
using org.mariuszgromada.math.mxparser;
using AdaptiveGrids;
using Quasar.Native;
using Core;

namespace AdaptiveGrids
{
    public class ParabolicProblem : IProblem
    {

        public ParabolicProblem(IFiniteElementMesh mesh, ITimeMesh timeMesh, Func<Vector2D, double> initCondition, IDictionary<string, IMaterial> materials)
        {
            Mesh = mesh;
            TimeMesh = timeMesh;
            InitialCondition = initCondition;
            Materials = materials;
        }
        public IDictionary<string, IMaterial> Materials { get; }
        public IFiniteElementMesh Mesh { get; }
        public ITimeMesh TimeMesh { get; }

        public Func<Vector2D, double> InitialCondition { get; }

        PardisoSLAE? SLAE { get; set; }

        public void Prepare()
        {
            FemAlgorithms.EnumerateMeshDofs(Mesh);
            SLAE = new PardisoSLAE(new PardisoMatrix(FemAlgorithms.BuildPortraitFirstStep(Mesh), Quasar.Native.PardisoMatrixType.SymmetricIndefinite));
            TimeMesh.ChangeCoefs(GetWeightsForInitialCondition());
        }

        double[] GetWeightsForInitialCondition()
        {
            //Parallel.ForEach(Mesh.Elements, element =>
            foreach (var element in Mesh.Elements)
            {
                var material = Materials[element.Material];

                if (material.IsVolume)
                {

                    var LM = element.BuildLocalMatrix(Mesh.Vertex, IFiniteElement.MatrixType.Mass, _ => 1);
                    SLAE?.Matrix.AddLocal(element.Dofs, LM);

                    var LRP = element.BuildLocalRightPart(Mesh.Vertex, InitialCondition);
                    SLAE?.AddLocalRightPart(element.Dofs, LRP);

                }

            }
            //);

            using (PardisoSLAESolver SLAESolver = new PardisoSLAESolver(SLAE!))
            {
                SLAESolver.Prepare();

                var solutionVector = SLAESolver.Solve();

                return solutionVector;
            }
        }

        public void Solve(ISolution result)
        {
            result.AddSolutionVector(TimeMesh[0], TimeMesh.Coefs(1));

            int tN = TimeMesh.Size();
            double[] timeCoefs = new double[3];

            PardisoSLAESolver? SLAESolver = null;

            for (int iT = 1; iT < tN; ++iT)
            {
                var isChanged = TimeMesh.IsChangedStep(iT);

                SLAE?.Clear();

                var t = TimeMesh[iT];
                double deltaT;
                double deltaT1;
                double deltaT0 = TimeMesh[iT] - TimeMesh[iT - 1];
                int scheme;

                if (iT != 1)
                {
                    scheme = 3;

                    deltaT = TimeMesh[iT] - TimeMesh[iT - 2];
                    deltaT1 = TimeMesh[iT - 1] - TimeMesh[iT - 2];

                    timeCoefs[0] = (deltaT + deltaT0) / (deltaT * deltaT0);
                    timeCoefs[1] = deltaT / (deltaT1 * deltaT0);
                    timeCoefs[2] = -deltaT0 / (deltaT * deltaT1);
                }
                else
                {
                    scheme = 2;
                    timeCoefs[0] = 1 / deltaT0;
                    timeCoefs[1] = 1 / deltaT0;
                }

                //Parallel.ForEach(Mesh.Elements, element =>
                foreach (var element in Mesh.Elements)
                {
                    var material = Materials[element.Material];

                    if (material.IsVolume)
                    {

                        var LM = element.BuildLocalMatrix(Mesh.Vertex, IFiniteElement.MatrixType.Stiffness, material.Lambda);
                        SLAE?.Matrix.AddLocal(element.Dofs, LM);

                        LM = element.BuildLocalMatrix(Mesh.Vertex, IFiniteElement.MatrixType.Mass, material.Sigma);

                        var LRP = element.BuildLocalRightPart(Mesh.Vertex, Coeff => material.F(Coeff, t));
                        SLAE?.AddLocalRightPart(element.Dofs, LRP);


                        double[] LC = new double[element.Dofs.Length];
                        SLAE?.Matrix.AddLocal(element.Dofs, LM, timeCoefs[0]);
                        for (int i = 1; i < scheme; ++i)
                        {
                            LRP = LinearAlgebraAlgorithms.MultiplyMatrixVector(LM, GetLocalCoeffs(LC, element.Dofs, TimeMesh.Coefs(i)), timeCoefs[i]);
                            SLAE?.AddLocalRightPart(element.Dofs, LRP);
                        }


                        //if (iT != 1)
                        //{
                        //    SLAE.Matrix.AddLocal(element.Dofs, LM, timeCoefs[1]);

                        //    LRP = LinearAlgebraAlgorithms.MultiplyMatrixVector(LM, TimeMesh.Coefs(2), timeCoefs[2]);
                        //    SLAE.AddLocalRightPart(element.Dofs, LRP);

                        //    LRP = LinearAlgebraAlgorithms.MultiplyMatrixVector(LM, TimeMesh.Coefs(1), timeCoefs[3]);
                        //    SLAE.AddLocalRightPart(element.Dofs, LRP);
                        //}
                        //else
                        //{
                        //    SLAE.Matrix.AddLocal(element.Dofs, LM, timeCoefs[0]);

                        //    LRP = LinearAlgebraAlgorithms.MultiplyMatrixVector(LM, TimeMesh.Coefs(1), timeCoefs[0]);
                        //    SLAE.AddLocalRightPart(element.Dofs, LRP);
                        //}

                    }
                    else if (material.Is2)
                    {
                        var LRP = element.BuildLocalRightPartWithSecondBoundaryConditions(Mesh.Vertex, Coeff => material.Theta(Coeff, t));
                        SLAE?.AddLocalRightPart(element.Dofs, LRP);
                    }
                }
                //);

                foreach (var element in Mesh.Elements)
                {
                    var material = Materials[element.Material];

                    if (material.Is1)
                    {
                        var LRP = element.BuildLocalRightPartWithFirstBoundaryConditions(Mesh.Vertex, Coeff => material.Ug(Coeff, t));
                        SLAE?.AddFirstBoundaryConditions(element.Dofs, LRP);
                    }
                }

                if (isChanged)
                {
                    SLAESolver?.Dispose();
                    SLAESolver = new PardisoSLAESolver(SLAE!);
                    SLAESolver.Prepare();
                }

                var solutionVector = SLAESolver?.Solve();

                result.AddSolutionVector(TimeMesh[iT], solutionVector!);

                TimeMesh.ChangeCoefs(solutionVector!);
            }

            SLAESolver?.Dispose();
        }

        double[] GetLocalCoeffs(double[] lc, int[] dofs, double[] coeffs)
        {
            for (int i = 0; i < dofs.Length; ++i)
                lc[i] = coeffs[dofs[i]];

            return lc;
        }

    }
}