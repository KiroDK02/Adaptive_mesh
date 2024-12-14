using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using FEM;
using TelmaCore;

namespace AdaptiveGrids
{
   public class FiniteElementMesh : IAdaptiveFiniteElementMesh
   {
      public FiniteElementMesh(IEnumerable<IFiniteElement> elements, Vector2D[] vertex, bool adapt = false)
      {
         Elements = elements;
         Vertex = vertex;
         Adapt = adapt;
      }
      public bool Adapt { get; set; }

      public IEnumerable<IFiniteElement> Elements { get; }

      public Vector2D[] Vertex { get; }

      public int NumberOfDofs { get; set; }

      public void DoAdaptation(ISolution solution, IDictionary<string, IMaterial> materials)
      {
         var splitsOfEdges = DistributeSplitsToEdges(solution, materials);
         var smoothSplitsOfEdges = SmoothToSplits(splitsOfEdges);


      }

      IDictionary<(int i, int j), int> SmoothToSplits(IDictionary<(int i, int j), int> splits)
      {
         var stop = false;

         while (!stop)
         {
            stop = true;

            foreach (var element in Elements)
            {
               var edges = new ((int, int) edge, int split)[3];

               for (int i = 0; i < element.NumberOfEdges; i++)
               {
                  var edge = element.Edge(i);
                  edge = (element.VertexNumber[edge.i], element.VertexNumber[edge.j]);
                  if (edge.i > edge.j) edge = (edge.j, edge.i);

                  edges[i] = (edge, splits[edge]);
               }

               var maxSplit = MaxValue(edges[0].split, edges[1].split, edges[2].split);

               for (int i = 0; i < 3; i++)
               {
                  var split = edges[i].split;
                  var differenceSplit = maxSplit - split;

                  if (differenceSplit > 1)
                  {
                     stop = false;

                     splits[edges[i].edge] = maxSplit - 1;
                  }
               }
            }
         }

         return splits;
      }

      IDictionary<(int i, int j), int> DistributeSplitsToEdges(ISolution solution, IDictionary<string, IMaterial> materials)
      {
         var distributedSplits = new Dictionary<(int i, int j), int>();

         var edgeSplits = CalcEdgeSplits(solution, materials);

         foreach (var element in Elements)
         {
            for (int edgei = 0; edgei < element.NumberOfEdges; edgei++)
            {
               var curEdge = element.Edge(edgei);
               curEdge = (element.VertexNumber[curEdge.i], element.VertexNumber[curEdge.j]);
               if (curEdge.i > curEdge.j) curEdge = (curEdge.j, curEdge.i);

               var splitsFromCurEdge = edgeSplits[curEdge];

               for (int edgej = 0; edgej < element.NumberOfEdges; edgej++)
               {
                  if (edgej == edgei)
                     continue;

                  var edge = element.Edge(edgej);
                  edge = (element.VertexNumber[edge.i], element.VertexNumber[edge.j]);
                  if (edge.i > edge.j) edge = (edge.j, edge.i);

                  if (!distributedSplits.TryGetValue(edge, out int split) || split < splitsFromCurEdge)
                     distributedSplits[edge] = splitsFromCurEdge;
               }
            }
         }

         return distributedSplits;
      }

      IDictionary<(int i, int j), int> CalcEdgeSplits(ISolution solution, IDictionary<string, IMaterial> materials)
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

                  if (edge.i > edge.j) edge = (edge.j, edge.i);

                  if (numberOccurrencesOfEdges.TryGetValue(edge, out var count))
                     numberOccurrencesOfEdges[edge] = ++count;
                  else numberOccurrencesOfEdges.TryAdd(edge, 1);
               }
            }
         }

         return numberOccurrencesOfEdges;
      }

      static int MaxValue(int a, int b, int c)
      {
         var max = a;

         if (b > max) max = b;
         if (c > max) max = c;

         return max;
      }
   }
}
