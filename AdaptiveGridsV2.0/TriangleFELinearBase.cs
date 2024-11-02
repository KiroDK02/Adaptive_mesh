using FEM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using TelmaCore;

namespace AdaptiveGrids
{
   namespace FiniteElements2D
   {
      public class TriangleFELinearBase : IFiniteElement
      {
         public TriangleFELinearBase(string material, int[] vertexNumber)
         {
            Material = material;
            VertexNumber = vertexNumber;
         }
         public string Material { get; }

         public int[] VertexNumber { get; } = new int[3];

         public int NumberOfEdges => 3;

         public int[] Dofs { get; } = new int[3];

         public double[,] BuildLocalMatrix(Vector2D[] VertexCoords, IFiniteElement.MatrixType type, Func<Vector2D, double> Coeff)
         {
            double[,] localMatrix = new double[3, 3];

            var Point1 = VertexCoords[VertexNumber[0]];
            var Point2 = VertexCoords[VertexNumber[1]];
            var Point3 = VertexCoords[VertexNumber[2]];

            var averageCoeff = CalcAverageCoeff(VertexCoords, Coeff);

            double detD = (Point2.X - Point1.X) * (Point3.Y - Point1.Y) - (Point3.X - Point1.X) * (Point2.Y - Point1.Y);

            switch (type)
            {
               case IFiniteElement.MatrixType.Stiffness:
               {
                  var InverseD = CalcInverseD(VertexCoords, detD);

                  for (int i = 0; i < 3; i++)
                     for (int j = 0; j < 3; j++)
                     {
                        localMatrix[i, j] = averageCoeff * Math.Abs(detD) / 2d * (InverseD[i, 1] * InverseD[j, 1] + InverseD[i, 2] * InverseD[j, 2]);
                     }

                  return localMatrix;
               }

               case IFiniteElement.MatrixType.Mass:
               {
                  var M = Matrices.TriangleLinearBaseMatrix.M;

                  for (int i = 0; i < 3; i++)
                     for (int j = 0; j < 3; j++)
                     {
                        localMatrix[i, j] = averageCoeff * Math.Abs(detD) * M[i, j];
                     }

                  return localMatrix;
               }
               default: throw new Exception("Invalid type of matrix.");
            }
         }

         public double[] BuildLocalRightPart(Vector2D[] VertexCoords, Func<Vector2D, double> F)
         {
            var localRightPart = new double[3];

            var Point1 = VertexCoords[VertexNumber[0]];
            var Point2 = VertexCoords[VertexNumber[1]];
            var Point3 = VertexCoords[VertexNumber[2]];

            var M = Matrices.TriangleLinearBaseMatrix.M;
            var detD = (Point2.X - Point1.X) * (Point3.Y - Point1.Y) - (Point3.X - Point1.X) * (Point2.Y - Point1.Y);
            var LocalF = CalcLocalF(VertexCoords, F);

            for (int i = 0; i < 3; i++)
            {
               double sum = 0;

               for (int j = 0; j < 3; j++)
               {
                  sum += M[i, j] * LocalF[j];
               }

               localRightPart[i] = Math.Abs(detD) * sum;
            }

            return localRightPart;
         }

         public double[] BuildLocalRightPartWithFirstBoundaryConditions(Vector2D[] VertexCoords, Func<Vector2D, double> Ug)
            => throw new NotSupportedException();

         public double[] BuildLocalRightPartWithSecondBoundaryConditions(Vector2D[] VertexCoords, Func<Vector2D, double> Thetta)
            => throw new NotSupportedException();

         public int DOFOnEdge(int edge) => 0;

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

            Vector2D localPoint = new Vector2D(localX, localY);
            
            double valueGradAtPointX = 0;
            double valueGradAtPointY = 0;

            var gradBasicFuncs = BaseFuncs.TriangleGradientBarycentricLinearBase;

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

            var basicFuncs = BaseFuncs.TriangleBarycentricLinearBase;

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
            => throw new NotSupportedException();

         public void SetElementDOF(int n, int dof)
            => throw new NotSupportedException();

         public void SetVertexDOF(int vertex, int dof) => Dofs[vertex] = dof;

         double CalcAverageCoeff(Vector2D[] VertexCoords, Func<Vector2D, double> Coeff)
         {
            double average = 0;

            for (int i = 0; i < VertexNumber.Length; ++i)
            {
               average += Coeff(VertexCoords[VertexNumber[i]]);
            }

            return average / VertexNumber.Length;
         }

         double[] CalcLocalF(Vector2D[] VertexCoords, Func<Vector2D, double> F)
         {
            var LocalF = new double[VertexNumber.Length];

            for (int i = 0; i < VertexNumber.Length; i++)
               LocalF[i] = F(VertexCoords[VertexNumber[i]]);

            return LocalF;
         }
         double[,] CalcInverseD(Vector2D[] VertexCoords, double detD)
         {
            var InverseD = new double[3, 3];

            var Point1 = VertexCoords[VertexNumber[0]];
            var Point2 = VertexCoords[VertexNumber[1]];
            var Point3 = VertexCoords[VertexNumber[2]];

            InverseD[0, 0] = (Point2.X * Point3.Y - Point3.X * Point2.Y) / detD;
            InverseD[1, 0] = (Point3.X * Point1.Y - Point1.X * Point3.Y) / detD;
            InverseD[2, 0] = (Point1.X * Point2.Y - Point2.X * Point1.Y) / detD;
            InverseD[0, 1] = (Point2.Y - Point3.Y) / detD;
            InverseD[1, 1] = (Point3.Y - Point1.Y) / detD;
            InverseD[2, 1] = (Point1.Y - Point2.Y) / detD;
            InverseD[0, 2] = (Point3.X - Point2.X) / detD;
            InverseD[1, 2] = (Point1.X - Point3.X) / detD;
            InverseD[2, 2] = (Point2.X - Point1.X) / detD;

            return InverseD;
         }
      }
   }
}
