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
      public class TriangleFEStraghtLinearBase : IFiniteElement
      {
         public TriangleFEStraghtLinearBase(string material, int[] vertexNumber)
         {
            Material = material;
            VertexNumber = vertexNumber;
         }
         public string Material {  get; }

         public int[] VertexNumber { get; } = new int[2];

         public int NumberOfEdges => 0;

         public int[] Dofs { get; } = new int[2];

         public double[,] BuildLocalMatrix(Vector2D[] VertexCoords, IFiniteElement.MatrixType type, Func<Vector2D, double> Coeff)
            => throw new NotSupportedException();

         public double[] BuildLocalRightPart(Vector2D[] VertexCoords, Func<Vector2D, double> F)
            => throw new NotSupportedException();

         public double[] BuildLocalRightPartWithFirstBoundaryConditions(Vector2D[] VertexCoords, Func<Vector2D, double> Ug)
            => CalcLocalF(VertexCoords, Ug);

         public double[] BuildLocalRightPartWithSecondBoundaryConditions(Vector2D[] VertexCoords, Func<Vector2D, double> Thetta)
         {
            var localRightPart = new double[2];

            var Point1 = VertexCoords[VertexNumber[0]];
            var Point2 = VertexCoords[VertexNumber[1]];

            var hm = Math.Sqrt((Point2.X - Point1.X) * (Point2.X - Point1.X) + (Point2.Y - Point1.Y) * (Point2.Y - Point1.Y));

            var M = Matrices.OneDimensionalLinearBaseMatrix.M;
            var LocalThetta = CalcLocalF(VertexCoords, Thetta);

            for (int i = 0; i < 2; i++)
            {
               double sum = 0;

               for (int j = 0; j < 2; j++)
               {
                  sum += M[i, j] * LocalThetta[j];
               }

               localRightPart[i] = hm * sum;
            }

            return localRightPart;
         }

         public int DOFOnEdge(int edge) => 0;

         public int DOFOnElement() => 0;

         public (int i, int j) Edge(int edge)
            => edge switch { 0 => (0, 1), _ => throw new Exception("Invalid number of edge.") };

         public Vector2D GetGradientAtPoint(Vector2D[] VertexCoords, ReadOnlySpan<double> coeffs, Vector2D point)
            => throw new NotSupportedException();

         public double GetValueAtPoint(Vector2D[] VertexCoords, ReadOnlySpan<double> coeffs, Vector2D point)
            => throw new NotSupportedException();

         public bool IsPointOnElement(Vector2D[] VertexCoords, Vector2D point)
            => throw new NotSupportedException();

         public void SetEdgeDOF(int edge, int n, int dof)
            => throw new NotSupportedException();

         public void SetElementDOF(int n, int dof)
            => throw new NotSupportedException();

         public void SetVertexDOF(int vertex, int dof) => Dofs[vertex] = dof;

         double[] CalcLocalF(Vector2D[] VertexCoords, Func<Vector2D, double> F)
         {
            var LocalF = new double[VertexNumber.Length];

            for (int i = 0; i < VertexNumber.Length; i++)
               LocalF[i] = F(VertexCoords[VertexNumber[i]]);

            return LocalF;
         }
      }

   }
}
