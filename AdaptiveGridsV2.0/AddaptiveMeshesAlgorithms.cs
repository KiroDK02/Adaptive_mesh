using AdaptiveGrids.FiniteElements1D;
using AdaptiveGrids.FiniteElements2D;
using FEM;
using Quadratures;
using System.Xml.Linq;
using TelmaCore;

namespace AdaptiveGrids
{
    public static class AddaptiveMeshesAlgorithms
    {
        public static ((int i, int j), (int i, int j), (int i, int j)) DefineOrderEdges(IFiniteElement element)
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

            if (edge1.i > edge1.j)
                edge1 = (edge1.j, edge1.i);
            if (edge2.i > edge2.j)
                edge2 = (edge2.j, edge2.i);
            if (edge3.i > edge3.j)
                edge3 = (edge3.j, edge3.i);

            var sumNum1 = edge1.i + edge1.j;
            var sumNum2 = edge2.i + edge2.j;
            var sumNum3 = edge3.i + edge3.j;

            var min = int.Min(sumNum1, int.Min(sumNum2, sumNum3));

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

        public static (Vector2D vert, int num)[] CalcAllVertices(int split1,
                                                                 int split2,
                                                                 int split3,
                                                                 (Vector2D vert, int num)[] verticesEdge1,
                                                                 (Vector2D vert, int num)[] verticesEdge2,
                                                                 (Vector2D vert, int num)[] verticesEdge3,
                                                                 ref int countVertex)
        {
            var minSplit = int.Min(split1, int.Min(split2, split3));
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

            var h = (verticesEdge1[split1].vert - verticesEdge1[0].vert) / minSplit;

            for (int layer = 1, numVert = minSplit; layer < countLayer; layer++, numVert--)
            {
                // (minSplit + 1 + (minSplit + 1 - (layer - 1))) * layer / 2
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

            globalVertices[^1] = verticesEdge2[^1];

            return globalVertices;
        }

        public static IDictionary<(int i, int j), (Vector2D vert, int num)[]> CalcVerticesOfEdges(IDictionary<(int i, int j), int> smoothSplitsOfEdges,
                                                                                                  IDictionary<(int i, int j), (int i, int j)> numberOldEdgeForNewEdges,
                                                                                                  ref int countVertex,
                                                                                                  IEnumerable<IFiniteElement> elements,
                                                                                                  Vector2D[] vertex)
        {
            var verticesOfSplitEdges = new Dictionary<(int i, int j), (Vector2D vert, int num)[]>();
            numberOldEdgeForNewEdges.Clear();

            foreach (var element in elements)
            {
                if (element.VertexNumber.Length == 2)
                    continue;

                for (int i = 0; i < element.NumberOfEdges; i++)
                {
                    var edge = element.Edge(i);
                    edge = (element.VertexNumber[edge.i], element.VertexNumber[edge.j]);
                    if (edge.i > edge.j)
                        edge = (edge.j, edge.i);

                    if (verticesOfSplitEdges.ContainsKey(edge))
                        continue;

                    var v0 = vertex[edge.i];
                    var v1 = vertex[edge.j];
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

                    verticesOfSplitEdges[edge] = vertices;

                    if (split == 1)
                        continue;

                    for (int k = 0; k < vertices.Length - 1; k++)
                    {
                        (int i, int j) newEdge = (vertices[k].num, vertices[k + 1].num);
                        if (newEdge.i > newEdge.j)
                            newEdge = (newEdge.j, newEdge.i);

                        numberOldEdgeForNewEdges[newEdge] = edge;
                    }
                }
            }

            return verticesOfSplitEdges;
        }

        public static void SplitToTriangles(int minSplit,
                                            (Vector2D vert, int num)[] globalVertices,
                                            IFiniteElement element,
                                            IList<IFiniteElement> listElems)
        {
            var countLayer = minSplit;

            // двигаемся по слоям и забираем локальные номера вершин для элементов
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
        }

        public static void DoubleElemsOnEdge(int split,
                                             int minSplit,
                                             (Vector2D vert, int num)[] verticesEdge,
                                             IList<IFiniteElement> listElemsFromCurElem,
                                             IList<(Vector2D vert, int num)> listVerticesCurElems)
        {
            var step = split / minSplit;

            for (int k = 1; k < verticesEdge.Length; k += step)
            {
                listVerticesCurElems.Add(verticesEdge[k]);

                for (int elemi = 0; elemi < listElemsFromCurElem.Count; elemi++)
                {
                    for (int edgei = 0; edgei < 3; edgei++)
                    {
                        var edge = listElemsFromCurElem[elemi].Edge(edgei);
                        edge = (listElemsFromCurElem[elemi].VertexNumber[edge.i], listElemsFromCurElem[elemi].VertexNumber[edge.j]);
                        if (edge.i > edge.j)
                            edge = (edge.j, edge.i);

                        if (verticesEdge[k - 1].num == edge.i && verticesEdge[k + 1].num == edge.j ||
                            verticesEdge[k - 1].num == edge.j && verticesEdge[k + 1].num == edge.i)
                        {
                            var thirdVertex = 0;
                            if (edgei == 0)
                                thirdVertex = listElemsFromCurElem[elemi].VertexNumber[2];
                            if (edgei == 1)
                                thirdVertex = listElemsFromCurElem[elemi].VertexNumber[0];
                            if (edgei == 2)
                                thirdVertex = listElemsFromCurElem[elemi].VertexNumber[1];

                            var elem1 = new TriangleFEQuadraticBaseWithNI(listElemsFromCurElem[elemi].Material, [edge.i, verticesEdge[k].num, thirdVertex]);
                            var elem2 = new TriangleFEQuadraticBaseWithNI(listElemsFromCurElem[elemi].Material, [verticesEdge[k].num, edge.j, thirdVertex]);

                            listElemsFromCurElem[elemi] = elem1;
                            listElemsFromCurElem.Add(elem2);
                        }
                    }
                }
            }
        }

        public static void SplitToEdges(IFiniteElement element,
                                        IDictionary<(int i, int j), (Vector2D vert, int num)[]> verticesOfSplitedEdges,
                                        IList<IFiniteElement> listElems)
        {
            (int i, int j) edge = (element.VertexNumber[0], element.VertexNumber[1]);
            if (edge.i > edge.j)
                edge = (edge.j, edge.i);

            var verticesEdge = verticesOfSplitedEdges[edge];

            for (int i = 0; i < verticesEdge.Length - 1; i++)
            {
                int[] globalNums = { verticesEdge[i].num, verticesEdge[i + 1].num };

                var elem = new TriangleFEStraightQuadraticBaseWithNI(element.Material, globalNums);
                listElems.Add(elem);
            }
        }

        public static Dictionary<(int i, int j), int> CalcNumberOccurrencesOfEdgesInElems(IEnumerable<IFiniteElement> elements)
        {
            var numberOccurrencesOfEdges = new Dictionary<(int i, int j), int>();

            foreach (var element in elements)
            {
                if (element.VertexNumber.Length == 2)
                    continue;

                for (int i = 0; i < element.NumberOfEdges; ++i)
                {
                    var edge = element.Edge(i);
                    edge = (element.VertexNumber[edge.i], element.VertexNumber[edge.j]);
                    if (edge.i > edge.j)
                        edge = (edge.j, edge.i);

                    if (numberOccurrencesOfEdges.TryGetValue(edge, out var count))
                        numberOccurrencesOfEdges[edge] = ++count;
                    else
                        numberOccurrencesOfEdges.Add(edge, 1);
                }
            }

            return numberOccurrencesOfEdges;
        }

        public static IDictionary<(int i, int j), int> SmoothToSplits(IDictionary<(int i, int j), int> splits,
                                                                      IEnumerable<IFiniteElement> elements)
        {
            var stop = false;

            while (!stop)
            {
                stop = true;

                foreach (var element in elements)
                {
                    if (element.VertexNumber.Length == 2)
                        continue;

                    var maxSplit = 0;

                    for (int i = 0; i < element.NumberOfEdges; i++)
                    {
                        var edge = element.Edge(i);
                        edge = (element.VertexNumber[edge.i], element.VertexNumber[edge.j]);
                        if (edge.i > edge.j)
                            edge = (edge.j, edge.i);

                        maxSplit = int.Max(maxSplit, splits[edge]);
                    }

                    for (int i = 0; i < 3; i++)
                    {
                        var edge = element.Edge(i);
                        edge = (element.VertexNumber[edge.i], element.VertexNumber[edge.j]);
                        if (edge.i > edge.j)
                            edge = (edge.j, edge.i);

                        var split = splits[edge];
                        var differenceSplit = maxSplit - split;

                        if (differenceSplit > 1)
                        {
                            stop = false;

                            splits[edge] = maxSplit - 1;
                        }
                    }
                }
            }

            return splits;
        }

        public static Dictionary<(int i, int j), int> DistributeSplitsToEdges(IEnumerable<IFiniteElement> elements,
                                                                              IDictionary<(int i, int j), int> edgeSplits)
        {
            var distributedSplits = new Dictionary<(int i, int j), int>();

            foreach (var element in elements)
            {
                if (element.VertexNumber.Length == 2)
                    continue;

                for (int edgei = 0; edgei < element.NumberOfEdges; edgei++)
                {
                    var curEdge = element.Edge(edgei);
                    curEdge = (element.VertexNumber[curEdge.i], element.VertexNumber[curEdge.j]);
                    if (curEdge.i > curEdge.j)
                        curEdge = (curEdge.j, curEdge.i);

                    var splitsFromCurEdge = edgeSplits[curEdge];

                    for (int edgej = 0; edgej < element.NumberOfEdges; edgej++)
                    {
                        if (edgej == edgei)
                            continue;

                        var edge = element.Edge(edgej);
                        edge = (element.VertexNumber[edge.i], element.VertexNumber[edge.j]);
                        if (edge.i > edge.j)
                            edge = (edge.j, edge.i);

                        if (!distributedSplits.TryGetValue(edge, out int split) || split < splitsFromCurEdge)
                            distributedSplits[edge] = splitsFromCurEdge;
                    }
                }
            }

            return distributedSplits;
        }

        public static IDictionary<(int i, int j), int> CalcEdgeSplits(IDictionary<(int i, int j), int> occurencesOfEdges,
                                                                      IDictionary<(int i, int j), double> differenceFlow)
        {
            var edgeSplits = new Dictionary<(int i, int j), int>();

            var scaleDifference = new double[5];
            var scaleSplits = new int[4];

            var maxDifference = differenceFlow.Values.Max();
            var minDifference = differenceFlow.Where(x => occurencesOfEdges[x.Key] != 1).MinBy(x => x.Value).Value;

            var step = (maxDifference - minDifference) / 4;

            scaleSplits[0] = 0;
            scaleSplits[1] = 0;
            scaleSplits[2] = 0;
            scaleSplits[3] = 1;

            /*            scaleDifference[0] = minDifference;
                        scaleDifference[1] = minDifference + step;
                        scaleDifference[2] = 0.5 * maxDifference;
                        scaleDifference[3] = 0.99 * maxDifference;*/

            for (int i = 0; i < 4; ++i)
            {
                scaleDifference[i] = minDifference + step * i;
                //scaleSplits[i] = i;
            }

            scaleDifference[3] = 0.65 * maxDifference;
            scaleDifference[4] = maxDifference;

            foreach ((var edge, double difference) in differenceFlow)
            {
                var split = 0;

                for (int i = 0; i < 4; ++i)
                    if (difference <= scaleDifference[i + 1])
                    {
                        split = scaleSplits[i];
                        break;
                    }

                edgeSplits.TryAdd(edge, split);
            }

            return edgeSplits;
        }

        public static IDictionary<(int i, int j), int> CalcEdgeSplits(IDictionary<(int i, int j), int> occurencesOfEdges,
                                                                      IDictionary<(int i, int j), double> differenceFlow,
                                                                      IDictionary<(int i, int j), (int i, int j)> numberOldEdgesForNewEdges,
                                                                      IDictionary<(int i, int j), int> edgeSplits,
                                                                      IEnumerable<IFiniteElement> elements, Vector2D[] vertices)
        {
            var splits = new Dictionary<(int i, int j), int>();

            double maxDifferences = differenceFlow.Values.Max();
            double minDifferences = differenceFlow.Where(x => occurencesOfEdges[x.Key] != 1).MinBy(x => x.Value).Value;

            double step = (maxDifferences - minDifferences) / 4.0;

            int[] scaleSplits = [0, 0, 0, 1];
            double[] scaleDifferences = [minDifferences, minDifferences + step, minDifferences + 2 * step, 0.65 * maxDifferences, maxDifferences];

            int maxNumber = edgeSplits.Keys.SelectMany(t => new[] { t.i, t.j }).Max();

            foreach ((var edge, double diff) in differenceFlow)
            {
                int split = 0;

                for (int i = 0; i < 4; i++)
                    if (diff <= scaleDifferences[i + 1])
                    {
                        split = scaleSplits[i];
                        break;
                    }

                if (edge.i <= maxNumber && edge.j <= maxNumber)
                    splits[edge] = edgeSplits[edge] + split;
                else if (numberOldEdgesForNewEdges.TryGetValue(edge, out var oldEdge))
                {
                    if (!splits.TryGetValue(oldEdge, out var countSplit) || countSplit < edgeSplits[oldEdge] + split)
                        splits[oldEdge] = edgeSplits[oldEdge] + split;
                }
                else
                {
                    var point = (vertices[edge.i] + vertices[edge.j]) / 2.0;

                    foreach (var element in elements)
                        if (element.VertexNumber.Length != 2 && element.IsPointOnElement(vertices, point))
                        {
                            for (int i = 0; i < element.NumberOfEdges; i++)
                            {
                                var edgei = element.Edge(i);
                                edgei = (element.VertexNumber[edgei.i], element.VertexNumber[edgei.j]);
                                if (edgei.i > edgei.j)
                                    edgei = (edgei.j, edgei.i);

                                if (!splits.TryGetValue(edgei, out var countSplit) || countSplit < edgeSplits[edgei] + split)
                                    splits[edgei] = edgeSplits[edgei] + split;
                            }
                        }
                }
            }

            return splits;
        }

        public static IDictionary<(int i, int j), int> SearchSplits(Vector2D[] vertex,
                                                                    IEnumerable<IFiniteElement> elements,
                                                                    double x0, double x1,
                                                                    double y0, double y1)
        {
            Dictionary<(int i, int j), int> splits = new();

            foreach (var element in elements)
            {
                if (element.VertexNumber.Length == 2)
                    continue;

                for (int edgei = 0; edgei < element.NumberOfEdges; edgei++)
                {
                    var edge = element.Edge(edgei);
                    edge = (element.VertexNumber[edge.i], element.VertexNumber[edge.j]);
                    if (edge.i > edge.j)
                        edge = (edge.j, edge.i);

                    if (splits.ContainsKey(edge))
                        continue;

                    int split = IsPointInsideRectangle(vertex[edge.i].X, vertex[edge.i].Y, x0, x1, y0, y1) ||
                                IsPointInsideRectangle(vertex[edge.j].X, vertex[edge.j].Y, x0, x1, y0, y1) ?
                                1 : 0;

                    splits.Add(edge, split);
                }
            }

            return splits;
        }

        public static bool IsPointInsideRectangle(double x, double y,
                                                  double x0, double x1,
                                                  double y0, double y1)
        {
            return x0 <= x && x <= x1 &&
                   y0 <= y && y <= y1;
        }

        public static int ReturnNumberThirdVertex(int vertex1, int vertex2)
        {
            int vertex3 = vertex1 switch
            {
                0 => vertex2 switch
                {
                    1 => 2,
                    2 => 1,
                    _ => throw new Exception("Invalid.")
                },

                1 => vertex2 switch
                {
                    0 => 2,
                    2 => 0,
                    _ => throw new Exception("Invalid.")
                },

                2 => vertex2 switch
                {
                    0 => 1,
                    1 => 0,
                    _ => throw new Exception("Invalid.")
                },
                _ => throw new Exception("Invalid.")
            };

            return vertex3;
        }

        public static Vector2D CalcOuterNormal(double x0, double x1, double x2,
                                               double y0, double y1, double y2)
        {
            var lengthEdge = Math.Sqrt((x1 - x0) * (x1 - x0) + (y1 - y0) * (y1 - y0));

            var vector = new Vector2D(x2 - x0, y2 - y0);
            var vectorOuterNormal = new Vector2D(y1 - y0, -(x1 - x0));
            vectorOuterNormal /= lengthEdge;

            if (vector * vectorOuterNormal > 0)
                vectorOuterNormal = -vectorOuterNormal;

            return vectorOuterNormal;
        }

        public static double SearchMaxNormFlowAtCenter(ISolution solution, IDictionary<string, IMaterial> materials)
        {
            double max = 0;

            foreach (var element in solution.Mesh.Elements)
            {
                if (element.VertexNumber.Length == 2)
                    continue;

                var lambda = materials[element.Material].Lambda;

                var point1 = solution.Mesh.Vertex[element.VertexNumber[0]];
                var point2 = solution.Mesh.Vertex[element.VertexNumber[1]];
                var point3 = solution.Mesh.Vertex[element.VertexNumber[2]];
                var center = (point1 + point2 + point3) / 3.0;

                var flowAtCenter = (lambda(center) * element.GetGradientAtPoint(solution.Mesh.Vertex, solution.SolutionVector, center)).Norm;

                if (max < flowAtCenter)
                    max = flowAtCenter;
            }

            return max;
        }

        public static double SearchMaxFlowOnEdge(ISolution solution, IDictionary<string, IMaterial> materials)
        {
            double max = 0;

            var quadratures = new QuadratureNodes<double>([.. NumericalIntegration.GaussQuadrature1DOrder3()], 3);

            foreach (var element in solution.Mesh.Elements)
            {
                if (element.VertexNumber.Length == 2)
                    continue;

                var lambda = materials[element.Material].Lambda;

                for (int edgei = 0; edgei < element.NumberOfEdges; edgei++)
                {
                    var edge = element.Edge(edgei);
                    int vertex3 = ReturnNumberThirdVertex(edge.i, edge.j);

                    edge = (element.VertexNumber[edge.i], element.VertexNumber[edge.j]);
                    vertex3 = element.VertexNumber[vertex3];

                    var x0 = solution.Mesh.Vertex[edge.i].X;
                    var x1 = solution.Mesh.Vertex[edge.j].X;
                    var x2 = solution.Mesh.Vertex[vertex3].X;
                    var y0 = solution.Mesh.Vertex[edge.i].Y;
                    var y1 = solution.Mesh.Vertex[edge.j].Y;
                    var y2 = solution.Mesh.Vertex[vertex3].Y;

                    var vectorOuterNormal = CalcOuterNormal(x0, x1, x2, y0, y1, y2);

                    var flowAcrossEdge = Math.Abs(NumericalIntegration.NumericalValueIntegralOnEdge(quadratures,
                        t =>
                        {
                            var x = x0 * (1 - t) + x1 * t;
                            var y = y0 * (1 - t) + y1 * t;

                            return lambda(new(x, y)) * vectorOuterNormal * element.GetGradientAtPoint(solution.Mesh.Vertex, solution.SolutionVector, new(x, y));
                        }));

                    if (max < flowAcrossEdge)
                        max = flowAcrossEdge;
                }
            }

            return max;
        }

        public static double SearchMaxProjectionFlowOnEdge(ISolution solution, IDictionary<string, IMaterial> materials)
        {
            double max = 0;

            var quadratures = new QuadratureNodes<double>([.. NumericalIntegration.GaussQuadrature1DOrder3()], 3);

            foreach (var element in solution.Mesh.Elements)
            {
                if (element.VertexNumber.Length == 2)
                    continue;

                var lambda = materials[element.Material].Lambda;

                var point1 = solution.Mesh.Vertex[element.VertexNumber[0]];
                var point2 = solution.Mesh.Vertex[element.VertexNumber[1]];
                var point3 = solution.Mesh.Vertex[element.VertexNumber[2]];
                var center = (point1 + point2 + point3) / 3.0;

                var flowAtCenter = (lambda(center) * element.GetGradientAtPoint(solution.Mesh.Vertex, solution.SolutionVector, center));

                for (int edgei = 0; edgei < element.NumberOfEdges; edgei++)
                {
                    var edge = element.Edge(edgei);
                    int vertex3 = ReturnNumberThirdVertex(edge.i, edge.j);

                    edge = (element.VertexNumber[edge.i], element.VertexNumber[edge.j]);
                    vertex3 = element.VertexNumber[vertex3];

                    var x0 = solution.Mesh.Vertex[edge.i].X;
                    var x1 = solution.Mesh.Vertex[edge.j].X;
                    var x2 = solution.Mesh.Vertex[vertex3].X;
                    var y0 = solution.Mesh.Vertex[edge.i].Y;
                    var y1 = solution.Mesh.Vertex[edge.j].Y;
                    var y2 = solution.Mesh.Vertex[vertex3].Y;

                    var vectorOuterNormal = CalcOuterNormal(x0, x1, x2, y0, y1, y2);

                    var flowAcrossEdge = Math.Abs(vectorOuterNormal * flowAtCenter);

                    if (max < flowAcrossEdge)
                        max = flowAcrossEdge;
                }
            }

            return max;
        }
    }
}
