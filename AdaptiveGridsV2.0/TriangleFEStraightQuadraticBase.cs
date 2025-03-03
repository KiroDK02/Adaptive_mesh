using FEM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelmaCore;

namespace AdaptiveGrids
{
    namespace FiniteElements1D
    {
        public class TriangleFEStraightQuadraticBase : IFiniteElement
        {
            public TriangleFEStraightQuadraticBase(string material, int[] vertexNumber)
            {
                Material = material;
                VertexNumber = vertexNumber;
            }
            public string Material { get; }

            public int[] VertexNumber { get; } = new int[2];

            public int NumberOfEdges => 1;

            public int[] Dofs { get; } = new int[3];

            public double[,] BuildLocalMatrix(Vector2D[] VertexCoords, IFiniteElement.MatrixType type, Func<Vector2D, double> Coeff)
               => throw new NotSupportedException();

            public double[] BuildLocalRightPart(Vector2D[] VertexCoords, Func<Vector2D, double> F)
               => throw new NotSupportedException();

            public double[] BuildLocalRightPartWithFirstBoundaryConditions(Vector2D[] VertexCoords, Func<Vector2D, double> Ug)
            {
                return CalcLocalF(VertexCoords, Ug);
            }

            public virtual double[] BuildLocalRightPartWithSecondBoundaryConditions(Vector2D[] VertexCoords, Func<Vector2D, double> Thetta)
               => throw new NotSupportedException();

            public int DOFOnEdge(int edge) => 1;

            public int DOFOnElement() => 0;

            public (int i, int j) Edge(int edge)
               => edge switch { 0 => (0, 1), _ => throw new Exception("Invalid number of edge") };

            public Vector2D GetGradientAtPoint(Vector2D[] VertexCoords, ReadOnlySpan<double> coeffs, Vector2D point)
               => throw new NotSupportedException();

            public double GetValueAtPoint(Vector2D[] VertexCoords, ReadOnlySpan<double> coeffs, Vector2D point)
               => throw new NotSupportedException();

            public bool IsPointOnElement(Vector2D[] VertexCoords, Vector2D point)
               => throw new NotSupportedException();

            public void SetEdgeDOF(int edge, int n, int dof)
            {
                Dofs[2] = edge switch
                {
                    0 => dof,
                    _ => throw new ArgumentException("Incorrect number of edge.")
                };
            }

            public void SetElementDOF(int n, int dof)
               => throw new NotSupportedException();

            public void SetVertexDOF(int vertex, int dof)
            {
                switch (vertex)
                {
                    case 0:
                        Dofs[0] = dof;
                        break;

                    case 1:
                        Dofs[1] = dof;
                        break;

                    default: throw new ArgumentException("Incorrect number of vertex.");
                }
            }

            double[] CalcLocalF(Vector2D[] VertexCoords, Func<Vector2D, double> F)
            {
                var LocalF = new double[Dofs.Length];

                LocalF[0] = F(VertexCoords[VertexNumber[0]]);
                LocalF[1] = F(VertexCoords[VertexNumber[1]]);
                LocalF[2] = F((VertexCoords[VertexNumber[1]] + VertexCoords[VertexNumber[0]]) / 2d);

                return LocalF;
            }
        }
    }
}
