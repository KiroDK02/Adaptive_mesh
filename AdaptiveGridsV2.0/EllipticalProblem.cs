using Core;
using FEM;
using Quasar.Native;

namespace AdaptiveGrids
{
    public class EllipticalProblem : IProblem
    {
        public EllipticalProblem(IDictionary<string, IMaterial> materials, IFiniteElementMesh mesh)
        {
            Materials = materials;
            Mesh = mesh;
        }
        public IDictionary<string, IMaterial> Materials { get; }
        public IFiniteElementMesh Mesh { get; }
        PardisoSLAE? SLAE { get; set; }
        public void Prepare()
        {
            FemAlgorithms.EnumerateMeshDofs(Mesh);
            SLAE = new PardisoSLAE(new PardisoMatrix(FemAlgorithms.BuildPortraitFirstStep(Mesh), PardisoMatrixType.SymmetricIndefinite));
        }

        public double? Solve(ISolution result)
        {
            foreach (var element in Mesh.Elements)
            {
                var material = Materials[element.Material];

                if (material.IsVolume)
                {
                    var localMatrix = element.BuildLocalMatrix(Mesh.Vertex,
                                                               IFiniteElement.MatrixType.Stiffness,
                                                               material.Lambda);
                    SLAE?.Matrix.AddLocal(element.Dofs, localMatrix);

                    localMatrix = element.BuildLocalMatrix(Mesh.Vertex,
                                                           IFiniteElement.MatrixType.Mass,
                                                           material.Sigma);
                    SLAE?.Matrix.AddLocal(element.Dofs, localMatrix);

                    var localRightPart = element.BuildLocalRightPart(Mesh.Vertex,
                                                                     func => material.F(func, 1));
                    SLAE?.AddLocalRightPart(element.Dofs, localRightPart);
                }
                else if (material.Is2)
                {
                    var localRightPart = element.BuildLocalRightPartWithSecondBoundaryConditions(Mesh.Vertex,
                                                                                                 func => material.Theta(func, 1));
                    SLAE?.AddLocalRightPart(element.Dofs, localRightPart);
                }
            }

            foreach (var element in Mesh.Elements)
            {
                var material = Materials[element.Material];

                if (material.Is1)
                {
                    var localRightPart = element.BuildLocalRightPartWithFirstBoundaryConditions(Mesh.Vertex,
                                                                                                func => material.Ug(func, 1));
                    SLAE?.AddFirstBoundaryConditions(element.Dofs, localRightPart);
                }
            }

            var SLAESolver = new PardisoSLAESolver(SLAE!);
            SLAESolver.Prepare();
            
            result.SolutionVector = SLAESolver.Solve();

            SLAESolver.Dispose();

            var discrepancy = SLAE?.CalcDiscrepancy([..result.SolutionVector]);

            return discrepancy;
        }
    }
}
