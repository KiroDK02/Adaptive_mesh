using AdaptiveGrids.FiniteElements1D;
using FEM;
using Quadratures;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelmaCore;
using static FEM.MasterElementsAlgorithms;

namespace AdaptiveGrids
{
    namespace FiniteElements2D
    {
        public class TriangleFEQuadraticBaseWithNI : TriangleFEQuadraticBase, IFiniteElementWithNumericIntegration<Vector2D>
        {
            public TriangleFEQuadraticBaseWithNI(string material, int[] vertexNumber)
               : base(material, vertexNumber)
            {
                MasterElement = MasterElementTriangleBarycentrycQuadraticBase.GetInstance();
            }

            public IMasterElement<Vector2D> MasterElement { get; }

            public override double[,] BuildLocalMatrix(Vector2D[] VertexCoords, IFiniteElement.MatrixType type, Func<Vector2D, double> Coeff)
            {
                Vector2D point1 = VertexCoords[VertexNumber[0]];
                Vector2D point2 = VertexCoords[VertexNumber[1]];
                Vector2D point3 = VertexCoords[VertexNumber[2]];

                double detD = (point2.X - point1.X) * (point3.Y - point1.Y) -
                              (point3.X - point1.X) * (point2.Y - point1.Y);

                double coefX1 = point2.X - point1.X, coefX2 = point3.X - point1.X;
                double coefY1 = point2.Y - point1.Y, coefY2 = point3.Y - point1.Y;

                double coefInLocalCoords(Vector2D vert)
                   => Coeff(new(coefX1 * vert.X + coefX2 * vert.Y + point1.X, coefY1 * vert.X + coefY2 * vert.Y + point1.Y));

                double[,] J = { { (point3.Y - point1.Y) / detD, (point1.Y - point2.Y) / detD },
                            { (point1.X - point3.X) / detD, (point2.X - point1.X) / detD } };

                var nodes = MasterElement.QuadratureNodes;

                double[,] localMatrix = new double[Dofs.Length, Dofs.Length];

                switch (type)
                {
                    case IFiniteElement.MatrixType.Stiffness:
                        {
                            for (int i = 0; i < Dofs.Length; i++)
                            {
                                for (int j = 0; j < Dofs.Length; j++)
                                {
                                    var values = CalcGradMultGrad(nodes, MasterElement.ValuesBasicFuncsGradient, i, j, J);
                                    double valueIntegral = 0;

                                    for (int k = 0; k < nodes.Nodes.Length; k++)
                                        valueIntegral += coefInLocalCoords(nodes.Nodes[k].Node) * values[k];

                                    localMatrix[i, j] = Math.Abs(detD) * valueIntegral;
                                }
                            }

                            return localMatrix;
                        }

                    case IFiniteElement.MatrixType.Mass:
                        {
                            for (int i = 0; i < Dofs.Length; i++)
                            {
                                for (int j = 0; j < Dofs.Length; j++)
                                {
                                    var values = MasterElement.PsiMultPsi[(i, j)];
                                    double valueIntegral = 0;

                                    for (int k = 0; k < nodes.Nodes.Length; k++)
                                        valueIntegral += coefInLocalCoords(nodes.Nodes[k].Node) * values[k];

                                    localMatrix[i, j] = Math.Abs(detD) * valueIntegral;
                                }
                            }

                            return localMatrix;
                        }

                    default: throw new Exception("Invalid type of matrix.");
                }
            }

            public override double[] BuildLocalRightPart(Vector2D[] VertexCoords, Func<Vector2D, double> F)
            {
                Vector2D point1 = VertexCoords[VertexNumber[0]];
                Vector2D point2 = VertexCoords[VertexNumber[1]];
                Vector2D point3 = VertexCoords[VertexNumber[2]];

                double detD = (point2.X - point1.X) * (point3.Y - point1.Y) -
                              (point3.X - point1.X) * (point2.Y - point1.Y);

                double coefX1 = point2.X - point1.X, coefX2 = point3.X - point1.X;
                double coefY1 = point2.Y - point1.Y, coefY2 = point3.Y - point1.Y;

                var nodes = MasterElement.QuadratureNodes;
                var values = MasterElement.ValuesBasicFuncs;
                var localRightPart = new double[Dofs.Length];

                double FInLocalCoords(Vector2D vert)
                   => F(new(coefX1 * vert.X + coefX2 * vert.Y + point1.X, coefY1 * vert.X + coefY2 * vert.Y + point1.Y));

                for (int i = 0; i < Dofs.Length; i++)
                {
                    double valueIntegral = 0;

                    for (int k = 0; k < nodes.Nodes.Length; k++)
                        valueIntegral += nodes.Nodes[k].Weight * FInLocalCoords(nodes.Nodes[k].Node) * values[i, k];

                    localRightPart[i] = Math.Abs(detD) * valueIntegral;
                }

                return localRightPart;
            }
        }
    }
}
