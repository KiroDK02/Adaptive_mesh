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

         var splitVertexEdges = new Dictionary<(int i, int j), (Vector2D vert, int num)[]>();
         int countVertex = Vertex.Length;

         foreach (var element in Elements)
         {
            for (int i = 0; i < element.NumberOfEdges; i++)
            {
               var edge = element.Edge(i);
               edge = (element.VertexNumber[edge.i], element.VertexNumber[edge.j]);
               if (edge.i > edge.j) edge = (edge.j, edge.i);

               if (splitVertexEdges.ContainsKey(edge)) continue;

               var v0 = Vertex[edge.i];
               var v1 = Vertex[edge.j];
               var split = smoothSplitsOfEdges[edge];
               split = (int)Math.Pow(2, split);

               var hx = (v1.X - v0.X) / split;
               var hy = (v1.Y - v0.Y) / split;

               Vector2D h = new(hx, hy);

               var vertices = new (Vector2D vert, int num)[split + 1];

               vertices[0] = (v0, edge.i);

               for (int k = 1; k < split; k++)
               {
                  Vector2D newVertex = v0 + k * h;
                  vertices[k] = (newVertex, countVertex++);
               }

               vertices[split] = (v1, edge.j);

               splitVertexEdges[edge] = vertices;
            }
         }

         var listVertices = new List<(Vector2D vert, int num)>();

         (int i, int j) edgeMain = (0, 0);
         (int i, int j) edgeFirst = (0, 0);
         (int i, int j) edgeSecond = (0, 0);

         foreach (var element in Elements)
         {
            (var edge1, var edge2, var edge3) = DefineOrderEdges(element);



         }
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

      ((int i, int j), (int i, int j), (int i, int j)) DefineOrderEdges(IFiniteElement element)
      {
         (int i, int j) edgeMain = (0, 0);
         (int i, int j) edgeFirst = (0, 0);
         (int i, int j) edgeSecond = (0, 0);

         var edge1 = element.Edge(0);
         var edge2 = element.Edge(1);
         var edge3 = element.Edge(2);
         edge1 = (element.VertexNumber[edge1.i], element.VertexNumber[edge1.j]);
         edge2 = (element.VertexNumber[edge2.i], element.VertexNumber[edge2.j]);
         edge3 = (element.VertexNumber[edge3.i], element.VertexNumber[edge3.j]);
         if (edge1.i > edge1.j) edge1 = (edge1.j, edge1.i);
         if (edge2.i > edge2.j) edge2 = (edge2.j, edge2.i);
         if (edge3.i > edge3.j) edge3 = (edge3.j, edge3.i);

         var sumNum1 = edge1.i + edge1.j;
         var sumNum2 = edge2.i + edge2.j;
         var sumNum3 = edge3.i + edge3.j;

         var min = -MaxValue(-sumNum1, -sumNum2, -sumNum3);

         if (min == sumNum1)
         {
            edgeMain = edge1;
            edgeFirst = edge2;
            edgeSecond = edge3;
         }

         if (min == sumNum2)
         {
            edgeMain = edge2;
            edgeFirst = edge1;
            edgeSecond = edge3;
         }

         if (min == sumNum3)
         {
            edgeMain = edge3;
            edgeFirst = edge1;
            edgeSecond = edge2;
         }

         return (edgeMain, edgeFirst, edgeSecond);
      }
   }
}
