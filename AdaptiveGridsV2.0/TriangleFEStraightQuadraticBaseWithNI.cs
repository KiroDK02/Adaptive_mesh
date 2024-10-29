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
      public class TriangleFEStraightQuadraticBaseWithNI : TriangleFEStraightQuadraticBase, IFiniteElementWithNumericIntegration<double>
      {
         public TriangleFEStraightQuadraticBaseWithNI(string material, int[] vertexNumber)
            : base(material, vertexNumber)
         {
            MasterElement = MasterElementBarycentricQuadraticBaseStraight.GetInstance();
         }
         public IMasterElement<double> MasterElement { get; }

         public override double[] BuildLocalRightPartWithSecondBoundaryConditions(Vector2D[] VertexCoords, Func<Vector2D, double> Thetta)
         {
            Vector2D point1 = VertexCoords[VertexNumber[0]];
            Vector2D point2 = VertexCoords[VertexNumber[1]];

            double lengthBound = Math.Sqrt((point1.X - point2.X) * (point1.X - point2.X) + (point1.Y - point2.Y) * (point1.Y - point2.Y));

            var nodes = MasterElement.QuadratureNodes;
            var values = MasterElement.ValuesBasicFuncs;
            var localRightPart = new double[Dofs.Length];

            Func<double, double> thetaInLocalCoords = 
               (double t) => FuncInLocalCoords(VertexCoords, Thetta, t);

            for (int i = 0; i < Dofs.Length; i++)
            {
               double valueIntegral = 0;

               for (int k = 0; k < nodes.Nodes.Length; k++)
                  valueIntegral += nodes.Nodes[k].Weight * thetaInLocalCoords(nodes.Nodes[k].Node) * values[i, k];

               localRightPart[i] = lengthBound * valueIntegral;
            }

            return localRightPart;
         }

         double FuncInLocalCoords(Vector2D[] VertexCoords, Func<Vector2D, double> func, double t)
         {
            double x0 = VertexCoords[VertexNumber[0]].X;
            double x1 = VertexCoords[VertexNumber[1]].X;
            double y0 = VertexCoords[VertexNumber[0]].Y;
            double y1 = VertexCoords[VertexNumber[1]].Y;

            return func(new(x0 * (1 - t) + x1 * t, y0 * (1 - t) + y1 * t));
         }
      }
   }
}
