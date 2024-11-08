using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FEM;
using TelmaCore;

namespace AdaptiveGrids
{
   public class FiniteElementMesh : IFiniteElementMesh
   {

      public FiniteElementMesh(IEnumerable<IFiniteElement> elements, Vector2D[] vertex)
      {
         Elements = elements;
         Vertex = vertex;
      }

      public IEnumerable<IFiniteElement> Elements { get; }

      public Vector2D[] Vertex { get; }

      public int NumberOfDofs { get; set; }

      public IDictionary<(int i, int j), int> EdgeSplits(ISolution solution, IDictionary<string, IMaterial> materials)
      {
         var edgeSplits = new Dictionary<(int i, int j), int>();
         var differenceFlow = solution.CalcDifferenceOfFlow(materials);
         var scaleDifference = new double[5];
         var scaleSplits = new int[4];

         var maxDifference = differenceFlow.Values.Max();
         var minDifference = differenceFlow.Values.Min();

         var step = (maxDifference - minDifference) / 4;

         for (int i = 0; i < 4; ++i)
         {
            scaleDifference[i] = minDifference + step * i;
            scaleSplits[i] = i + 1;
         }

         scaleDifference[4] = maxDifference;

         foreach (var edge in differenceFlow)
         {
            var difference = edge.Value;
            var split = 0;

            for (int i = 0; i < 4; ++i)
               if (scaleDifference[i] <= difference && difference <= scaleDifference[i + 1])
               {
                  split = scaleSplits[i];
                  break;
               }

            edgeSplits[edge.Key] = split;
         }

         return edgeSplits;
      }
   }
}
