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
                                                                                                  ref int countVertex,
                                                                                                  IEnumerable<IFiniteElement> elements,
                                                                                                  Vector2D[] vertex)
        {
            var verticesOfSplitEdges = new Dictionary<(int i, int j), (Vector2D vert, int num)[]>();

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

        public static Dictionary<(int i, int j), int> DistributeSplitsToEdges(ISolution solution, IDictionary<string, IMaterial> materials)
        {
            var distributedSplits = new Dictionary<(int i, int j), int>();

            var edgeSplits = CalcEdgeSplits(solution, materials);

            foreach (var element in solution.Mesh.Elements)
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

        public static Dictionary<(int i, int j), int> CalcEdgeSplits(ISolution solution, IDictionary<string, IMaterial> materials)
        {
            var edgeSplits = new Dictionary<(int i, int j), int>();
            var occurencesOfEdges = CalcNumberOccurrencesOfEdgesInElems(solution.Mesh.Elements);
            var differenceFlow = solution.CalcDifferenceOfFlow(materials, occurencesOfEdges);

            var scaleDifference = new double[5];
            var scaleSplits = new int[4];

            var maxDifference = differenceFlow.Values.Max();
            var minDifference = differenceFlow.Where(x => occurencesOfEdges[x.Key] != 1).MinBy(x => x.Value).Value;

            var step = (maxDifference - minDifference) / 4;

            /*         scaleSplits[0] = 0;
                     scaleSplits[1] = 0;
                     scaleSplits[2] = 1;
                     scaleSplits[3] = 2;*/

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
    }
}
