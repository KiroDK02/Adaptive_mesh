using AdaptiveGrids.FiniteElements1D;
using AdaptiveGrids.FiniteElements2D;
using FEM;
using TelmaCore;

using static AdaptiveGrids.AddaptiveMeshesAlgorithms;
using static FEM.IAdaptiveFiniteElementMesh;

namespace AdaptiveGrids
{
    public class FiniteElementMesh : IAdaptiveFiniteElementMesh
    {
        public FiniteElementMesh(IEnumerable<IFiniteElement> elements, Vector2D[] vertex, TypeRelativeDifference typeDifference)
        {
            Elements = elements;
            Vertex = vertex;
            TypeDifference = typeDifference;
        }

        public TypeRelativeDifference TypeDifference { get; }

        public IEnumerable<IFiniteElement> Elements { get; }

        public Vector2D[] Vertex { get; }

        public int NumberOfDofs { get; set; }

        // Пока только для элементов с одинаковым числом разбиений
        public IAdaptiveFiniteElementMesh DoAdaptation(ISolution solution, IDictionary<string, IMaterial> materials)
        {
            var splitsOfEdges = DistributeSplitsToEdges(solution, materials);
            var smoothSplitsOfEdges = SmoothToSplits(splitsOfEdges, Elements);

            int countVertex = Vertex.Length;
            var verticesOfSplitedEdges = CalcVerticesOfEdges(smoothSplitsOfEdges, ref countVertex, Elements, Vertex);

            var listElems = new List<IFiniteElement>();
            var listVertices = new List<(Vector2D vert, int num)>();

            foreach (var element in Elements)
            {
                var listElemsFromCurElem = new List<IFiniteElement>();

                if (element.VertexNumber.Length == 2)
                    continue;

                (var edge1, var edge2, var edge3) = DefineOrderEdges(element);

                var split1 = (int)Math.Pow(2, smoothSplitsOfEdges[edge1]);
                var split2 = (int)Math.Pow(2, smoothSplitsOfEdges[edge2]);
                var split3 = (int)Math.Pow(2, smoothSplitsOfEdges[edge3]);

                var verticesEdge1 = verticesOfSplitedEdges[edge1];
                var verticesEdge2 = verticesOfSplitedEdges[edge2];
                var verticesEdge3 = verticesOfSplitedEdges[edge3];

                var minSplit = int.Min(split1, int.Min(split2, split3));

                var globalVertices = CalcAllVertices(split1,
                                                     split2,
                                                     split3,
                                                     verticesEdge1,
                                                     verticesEdge2,
                                                     verticesEdge3,
                                                     ref countVertex);

                SplitToTriangles(minSplit, globalVertices, element, listElemsFromCurElem);

                var listVerticesCurElem = new List<(Vector2D vert, int num)>(globalVertices);

                if (split1 / minSplit != 1)
                    DoubleElemsOnEdge(split1, minSplit, verticesEdge1, listElemsFromCurElem, listVerticesCurElem);
                if (split2 / minSplit != 1)
                    DoubleElemsOnEdge(split2, minSplit, verticesEdge2, listElemsFromCurElem, listVerticesCurElem);
                if (split3 / minSplit != 1)
                    DoubleElemsOnEdge(split3, minSplit, verticesEdge3, listElemsFromCurElem, listVerticesCurElem);

                listElems.AddRange(listElemsFromCurElem);
                listVertices.AddRange(listVerticesCurElem);
            }

            foreach (var element in Elements)
            {
                if (element.VertexNumber.Length != 2)
                    continue;

                SplitToEdges(element, verticesOfSplitedEdges, listElems);
            }

            var vertex = new Vector2D[countVertex];

            foreach (var (vert, num) in listVertices)
                vertex[num] = vert;

            return new FiniteElementMesh(listElems, vertex, TypeDifference);
        }
    }
}
