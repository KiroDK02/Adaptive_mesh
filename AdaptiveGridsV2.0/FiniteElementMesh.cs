using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using AdaptiveGrids.FiniteElements1D;
using AdaptiveGrids.FiniteElements2D;
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
         Adapted = adapt;
      }
      public bool Adapted { get; set; }

      public IEnumerable<IFiniteElement> Elements { get; }

      public Vector2D[] Vertex { get; }

      public int NumberOfDofs { get; set; }

      // Пока только для элементов с одинаковым числом разбиений
      public IAdaptiveFiniteElementMesh DoAdaptation(ISolution solution, IDictionary<string, IMaterial> materials)
      {
         var splitsOfEdges = DistributeSplitsToEdges(solution, materials);
         var smoothSplitsOfEdges = SmoothToSplits(splitsOfEdges);

         int countVertex = Vertex.Length;
         var splitVertexEdges = CalcVerticesOfEdges(smoothSplitsOfEdges, ref countVertex);

         var listElems = new List<IFiniteElement>();
         var listVertices = new List<(Vector2D vert, int num)>();

         foreach (var element in Elements)
         {
            if (element.VertexNumber.Length == 2) continue;

            (var edge1, var edge2, var edge3) = DefineOrderEdges(element);

            var split1 = (int)Math.Pow(2, smoothSplitsOfEdges[edge1]);
            var split2 = (int)Math.Pow(2, smoothSplitsOfEdges[edge2]);
            var split3 = (int)Math.Pow(2, smoothSplitsOfEdges[edge3]);

            var verticesEdge1 = splitVertexEdges[edge1];
            var verticesEdge2 = splitVertexEdges[edge2];
            var verticesEdge3 = splitVertexEdges[edge3];

            var minSplit = -MaxValue(-split1, -split2, -split3);

            var countLayer = minSplit;

            var globalVertices = CalcAllVertices(split1,
                                                 split2,
                                                 split3,
                                                 verticesEdge1,
                                                 verticesEdge2,
                                                 verticesEdge3,
                                                 ref countVertex);

            // двигаться по слоям и забирать локальные номера вершин для элементов
            for (int layer = 0; layer < countLayer; layer++)
            {
               var numElemsOnLayer = 2 * (minSplit - layer) - 1;

               for (int elemi = 0; elemi < numElemsOnLayer; elemi++)
               {
                  var countPassedVertices = (2 * (minSplit + 1) - (layer - 1)) * layer / 2;
                  var countPassedVerticesNextLayer = (2 * (minSplit + 1) - layer) * (layer + 1) / 2;

                  (int localV1, int localV2, int localV3) =
                     elemi % 2 == 0
                  ? (elemi / 2 + countPassedVertices, elemi / 2 + countPassedVertices + 1, elemi / 2 + countPassedVerticesNextLayer)
                  : ((elemi + 1) / 2 + countPassedVertices, (elemi + 1) / 2 + countPassedVerticesNextLayer, (elemi + 1) / 2 + countPassedVerticesNextLayer - 1);

                  int[] globalNums = { globalVertices[localV1].num, globalVertices[localV2].num, globalVertices[localV3].num };
                  var elem = new TriangleFEQuadraticBaseWithNI(element.Material, globalNums);

                  listElems.Add(elem);
               }
            }

            for (int i = 0; i < globalVertices.Length; i++)
            {
               listVertices.Add(globalVertices[i]);
            }
         }

         foreach (var element in Elements)
         {
            if (element.VertexNumber.Length != 2) continue;

            var edge = element.Edge(0);
            edge = (element.VertexNumber[edge.i], element.VertexNumber[edge.j]);
            if (edge.i > edge.j) edge = (edge.j, edge.i);

            var verticesEdge = splitVertexEdges[edge];

            for (int i = 0; i < verticesEdge.Length - 1; i++)
            {
               int[] globalNums = { verticesEdge[i].num, verticesEdge[i + 1].num };

               var elem = new TriangleFEStraightQuadraticBaseWithNI(element.Material, globalNums);
               listElems.Add(elem);
            }
         }

         var vertex = new Vector2D[countVertex];

         foreach (var (vert, num) in listVertices)
         {
            vertex[num] = vert;
         }

         return new FiniteElementMesh(listElems, vertex);
      }

      IDictionary<(int i, int j), int> SmoothToSplits(IDictionary<(int i, int j), int> splits)
      {
         var stop = false;

         while (!stop)
         {
            stop = true;

            foreach (var element in Elements)
            {
               if (element.VertexNumber.Length == 2) continue;

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

                     splits[edges[i].edge] = maxSplit; // Временно не maxSplit - 1
                  }
               }
            }
         }

         return splits;
      }

      Dictionary<(int i, int j), int> DistributeSplitsToEdges(ISolution solution, IDictionary<string, IMaterial> materials)
      {
         var distributedSplits = new Dictionary<(int i, int j), int>();

         var edgeSplits = CalcEdgeSplits(solution, materials);

         foreach (var element in Elements)
         {
            if (element.VertexNumber.Length == 2) continue;

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

      Dictionary<(int i, int j), int> CalcEdgeSplits(ISolution solution, IDictionary<string, IMaterial> materials)
      {
         var edgeSplits = new Dictionary<(int i, int j), int>();
         var differenceFlow = solution.CalcDifferenceOfFlow(materials, CalcNumberOccurrencesOfEdgesInElems());

         var scaleDifference = new double[5];
         var scaleSplits = new int[4];

         var maxDifference = differenceFlow.Values.Max();
         var minDifference = differenceFlow.Values.Min();

         var step = (maxDifference - minDifference) / 4;

//         scaleSplits[0] = 0;
//         scaleSplits[1] = 0;
//         scaleSplits[2] = 1;
//         scaleSplits[3] = 2;

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
            (edgeFirst, edgeSecond) = edge2.i < edge3.i ?
                                      (edge2, edge3) :
                                      (edge3, edge2);
         }

         if (min == sumNum2)
         {
            edgeMain = edge2;
            (edgeFirst, edgeSecond) = edge1.i < edge3.i ?
                                      (edge1, edge3) :
                                      (edge3, edge1);
         }

         if (min == sumNum3)
         {
            edgeMain = edge3;
            (edgeFirst, edgeSecond) = edge2.i < edge1.i ?
                                      (edge2, edge1) :
                                      (edge1, edge2);
         }

         return (edgeMain, edgeFirst, edgeSecond);
      }

      (Vector2D vert, int num)[] CalcAllVertices(int split1,
                                                 int split2,
                                                 int split3,
                                                 (Vector2D vert, int num)[] verticesEdge1,
                                                 (Vector2D vert, int num)[] verticesEdge2,
                                                 (Vector2D vert, int num)[] verticesEdge3,
                                                 ref int countVertex)
      {
         var minSplit = -MaxValue(-split1, -split2, -split3);
         var countLayer = minSplit;

         var countVertices = (minSplit + 2) * (countLayer + 1) / 2;
         var globalVertices = new (Vector2D vert, int num)[countVertices];

         var step1 = split1 / minSplit;
         var step2 = split2 / minSplit;
         var step3 = split3 / minSplit;

         for (int i = 0, step = 0; i < minSplit + 1; i++, step += step1)
         {
            globalVertices[i] = verticesEdge1[step];
         }

         var k2 = step2;
         var k3 = step3;

         var h = (verticesEdge1[minSplit].vert - verticesEdge1[0].vert) / minSplit;

         for (int layer = 1, numVert = minSplit; layer < countLayer; layer++, numVert--)
         {
            var countPassedVertices = (2 * (minSplit + 1) - (layer - 1)) * layer / 2;
            globalVertices[countPassedVertices] = verticesEdge2[k2];

            for (int vi = 1; vi < numVert - 1; vi++)
            {
               var localNum = countPassedVertices + vi;
               var newVertex = verticesEdge2[k2].vert + h * vi;
               globalVertices[localNum] = (newVertex, countVertex++);
            }

            globalVertices[(2 * (minSplit + 1) - layer) * (layer + 1) / 2 - 1] = verticesEdge3[k3];

            k2 += step2;
            k3 += step3;
         }

         globalVertices[countVertices - 1] = verticesEdge2[^1];

         return globalVertices;
      }

      Dictionary<(int i, int j), (Vector2D vert, int num)[]> CalcVerticesOfEdges(IDictionary<(int i, int j), int> smoothSplitsOfEdges,
                                                                                  ref int countVertex)
      {
         var splitVertexEdges = new Dictionary<(int i, int j), (Vector2D vert, int num)[]>();

         foreach (var element in Elements)
         {
            if (element.VertexNumber.Length == 2) continue;

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

               Vector2D h = (v1 - v0) / split;

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

         return splitVertexEdges;
      }
   }
}
