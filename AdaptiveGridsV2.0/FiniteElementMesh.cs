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
         var differenceFlow = solution.CalcDifferenceOfFlow(materials, CalcNumberOccurrencesOfEdgesInElems());
         var scaleDifference = new double[5];
         var scaleSplits = new int[4];

         var maxDifference = differenceFlow.Values.Max();
         var minDifference = differenceFlow.Values.Min();

         var step = (maxDifference - minDifference) / 4;

         for (int i = 0; i < 4; ++i)
         {
            scaleDifference[i] = minDifference + step * i;
            scaleSplits[i] = i;
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

            edgeSplits.TryAdd(edge.Key, split);
         }

         return edgeSplits;
      }

      Dictionary<(int i, int j), int> CalcNumberOccurrencesOfEdgesInElems()
      {
         var numberOccurrencesOfEdges = new Dictionary<(int i, int j), int>();

         foreach (var element in Elements)
         {
            if (element.VertexNumber.Length != 2)
            {
               for (int i = 0; i < element.NumberOfEdges; ++i)
               {
                  var edge = element.Edge(i);
                  edge = (element.VertexNumber[edge.i], element.VertexNumber[edge.j]);

                  if (numberOccurrencesOfEdges.TryGetValue(edge, out var count))
                     numberOccurrencesOfEdges[edge] = ++count;
                  else numberOccurrencesOfEdges.TryAdd(edge, 1);
               }
            }
         }

         return numberOccurrencesOfEdges;
      }
   }
}
