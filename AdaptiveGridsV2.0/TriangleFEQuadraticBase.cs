using FEM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TelmaCore;

namespace AdaptiveGrids
{
    namespace FiniteElements2D
    {
        public class TriangleFEQuadraticBase : IFiniteElement
        {
            public TriangleFEQuadraticBase(string material, int[] vertexNumber)
            {
                Material = material;
                VertexNumber = vertexNumber;
            }

            public string Material { get; }

            public int[] VertexNumber { get; } = new int[3];

            public int NumberOfEdges => 3;

            public int[] Dofs { get; } = new int[6];

            public virtual double[,] BuildLocalMatrix(Vector2D[] VertexCoords, IFiniteElement.MatrixType type, Func<Vector2D, double> Coeff)
               => throw new NotSupportedException();

            public virtual double[] BuildLocalRightPart(Vector2D[] VertexCoords, Func<Vector2D, double> F)
               => throw new NotSupportedException();

            public double[] BuildLocalRightPartWithFirstBoundaryConditions(Vector2D[] VertexCoords, Func<Vector2D, double> Ug)
               => throw new NotSupportedException();

            public double[] BuildLocalRightPartWithSecondBoundaryConditions(Vector2D[] VertexCoords, Func<Vector2D, double> Thetta)
               => throw new NotSupportedException();

            public int DOFOnEdge(int edge) => 1;

            public int DOFOnElement() => 0;

            public (int i, int j) Edge(int edge)
               => edge switch { 0 => (0, 1), 1 => (1, 2), 2 => (2, 0), _ => throw new Exception("Invalid number of edge.") };

            public Vector2D GetGradientAtPoint(Vector2D[] VertexCoords, ReadOnlySpan<double> coeffs, Vector2D point)
            {
                var point1 = VertexCoords[VertexNumber[0]];
                var point2 = VertexCoords[VertexNumber[1]];
                var point3 = VertexCoords[VertexNumber[2]];

                double detD = (point2.X - point1.X) * (point3.Y - point1.Y) -
                              (point3.X - point1.X) * (point2.Y - point1.Y);

                double[,] J = { { (point3.Y - point1.Y) / detD, (point1.Y - point2.Y) / detD },
                            { (point1.X - point3.X) / detD, (point2.X - point1.X) / detD } };

                double localX = ((point3.X * point1.Y - point1.X * point3.Y) + (point3.Y - point1.Y) * point.X + (point1.X - point3.X) * point.Y) / detD;
                double localY = ((point1.X * point2.Y - point2.X * point1.Y) + (point1.Y - point2.Y) * point.X + (point2.X - point1.X) * point.Y) / detD;

                Vector2D localPoint = new(localX, localY);

                double valueGradAtPointX = 0;
                double valueGradAtPointY = 0;

                var gradBasicFuncs = BaseFuncs.TriangleGradientBarycentricQuadraticBase;

                for (int i = 0; i < Dofs.Length; i++)
                {
                    valueGradAtPointX += coeffs[Dofs[i]] * (gradBasicFuncs[i, 0](localPoint) * J[0, 0] +
                                                            gradBasicFuncs[i, 1](localPoint) * J[0, 1]);
                    valueGradAtPointY += coeffs[Dofs[i]] * (gradBasicFuncs[i, 0](localPoint) * J[1, 0] +
                                                            gradBasicFuncs[i, 1](localPoint) * J[1, 1]);
                }

                return new(valueGradAtPointX, valueGradAtPointY);
            }

            public double GetValueAtPoint(Vector2D[] VertexCoords, ReadOnlySpan<double> coeffs, Vector2D point)
            {
                var point1 = VertexCoords[VertexNumber[0]];
                var point2 = VertexCoords[VertexNumber[1]];
                var point3 = VertexCoords[VertexNumber[2]];

                double detD = (point2.X - point1.X) * (point3.Y - point1.Y) -
                              (point3.X - point1.X) * (point2.Y - point1.Y);

                double localX = ((point3.X * point1.Y - point1.X * point3.Y) + (point3.Y - point1.Y) * point.X + (point1.X - point3.X) * point.Y) / detD;
                double localY = ((point1.X * point2.Y - point2.X * point1.Y) + (point1.Y - point2.Y) * point.X + (point2.X - point1.X) * point.Y) / detD;

                Vector2D localPoint = new Vector2D(localX, localY);

                double valueFuncAtPoint = 0;

                var basicFuncs = BaseFuncs.TriangleBarycentricQuadraticBase;

                for (int i = 0; i < Dofs.Length; i++)
                {
                    valueFuncAtPoint += coeffs[Dofs[i]] * basicFuncs[i](localPoint);
                }

                return valueFuncAtPoint;
            }

            public bool IsPointOnElement(Vector2D[] VertexCoords, Vector2D point)
            {
                double x1 = VertexCoords[VertexNumber[0]].X, x2 = VertexCoords[VertexNumber[1]].X, x3 = VertexCoords[VertexNumber[2]].X;
                double y1 = VertexCoords[VertexNumber[0]].Y, y2 = VertexCoords[VertexNumber[1]].Y, y3 = VertexCoords[VertexNumber[2]].Y;
                double x0 = point.X, y0 = point.Y;

                double product1 = (x1 - x0) * (y2 - y1) - (x2 - x1) * (y1 - y0);
                double product2 = (x2 - x0) * (y3 - y2) - (x3 - x2) * (y2 - y0);
                double product3 = (x3 - x0) * (y1 - y3) - (x1 - x3) * (y3 - y0);

                if (product1 <= 0 && product2 <= 0 && product3 <= 0)
                    return true;
                else if (product1 >= 0 && product2 >= 0 && product3 >= 0)
                    return true;
                else
                    return false;
            }

            public void SetEdgeDOF(int edge, int n, int dof)
            {
                switch (edge)
                {
                    case 0:
                        {
                            Dofs[3] = dof;
                            break;
                        }

                    case 1:
                        {
                            Dofs[4] = dof;
                            break;
                        }

                    case 2:
                        {
                            Dofs[5] = dof;
                            break;
                        }

                    default: throw new Exception("Invalid number of edge.");
                }
            }

            public void SetElementDOF(int n, int dof)
               => throw new NotSupportedException();

            public void SetVertexDOF(int vertex, int dof)
            {
                switch (vertex)
                {
                    case 0:
                        {
                            Dofs[0] = dof;
                            break;
                        }

                    case 1:
                        {
                            Dofs[1] = dof;
                            break;
                        }

                    case 2:
                        {
                            Dofs[2] = dof;
                            break;
                        }

                    default: throw new Exception("Invalid number of vertex");
                }
            }
        }
    }
}
