using AdaptiveGrids.FiniteElements1D;
using AdaptiveGrids.FiniteElements2D;
using FEM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelmaCore;
using static FEM.IAdaptiveFiniteElementMesh;

namespace AdaptiveGrids
{
    public class FileManager
    {
        public FileManager(string pathNodes = "", string pathTriangles = "", string pathValues = "")
        {
            PathNodes = pathNodes;
            PathTriangles = pathTriangles;
            PathValues = pathValues;
        }

        private string PathNodes { get; }
        private string PathTriangles { get; }
        private string PathValues { get; }

        public IAdaptiveFiniteElementMesh ReadMeshFromTelma(string path, TypeRelativeDifference type)
        {
            var reader = new StreamReader(path);

            reader.ReadLine();

            int countVertices = int.Parse(reader.ReadLine()!);

            var vertices = new Vector2D[countVertices];

            for (int i = 0; i < countVertices; i++)
            {
                var inputStr = reader.ReadLine()!.Split('\t', StringSplitOptions.RemoveEmptyEntries);

                vertices[i] = new Vector2D(double.Parse(inputStr[0]), double.Parse(inputStr[1]));
            }

            int countElements = int.Parse(reader.ReadLine()!);

            var listElems = new List<(int material, int[] vertices)>();

            for (int i = 0; i < countElements; i++)
            {
                var inputStr = reader.ReadLine()!.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                int[] verts = inputStr[0] == "Triangle" ?
                              [int.Parse(inputStr[5]), int.Parse(inputStr[6]), int.Parse(inputStr[7])] :
                              [int.Parse(inputStr[5]), int.Parse(inputStr[6])];

                int material = int.Parse(inputStr[3]);

                listElems.Add((material, verts));
            }

            int countMaterial = int.Parse(reader.ReadLine()!);

            var materials = new Dictionary<int, string>();

            for (int i = 0; i < countMaterial; i++)
            {
                var inputStr = reader.ReadLine()!.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                materials.TryAdd(int.Parse(inputStr[0]), string.Join(' ', inputStr[1..]));
            }

            int countBoundMaterials = int.Parse(reader.ReadLine()!);

            var boundMaterials = new Dictionary<int, string>();

            for (int i = 0; i < countBoundMaterials; i++)
            {
                var inputStr = reader.ReadLine()!.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                boundMaterials.TryAdd(int.Parse(inputStr[0]), string.Join(' ', inputStr[1..]));
            }

            reader.Close();

            IFiniteElement[] elements = new IFiniteElement[listElems.Count];

            for (int i = 0; i < listElems.Count; i++)
            {
                elements[i] = listElems[i].vertices.Length == 3 ?
                              new TriangleFEQuadraticBaseWithNI(materials[listElems[i].material], listElems[i].vertices) :
                              new TriangleFEStraightQuadraticBaseWithNI(boundMaterials[listElems[i].material], listElems[i].vertices);
            }

            return new FiniteElementMesh(elements, vertices, type);
        }

        public void LoadToFile(Vector2D[] nodes, IEnumerable<IFiniteElement> elements, double[] coeffs)
        {
            var values = coeffs[..nodes.Length];

            LoadNodesToFile(nodes);
            LoadTrianglesToFile(elements);
            LoadValuesToFile(values);
        }

        public void LoadNodesToFile(Vector2D[] nodes)
        {
            using (StreamWriter writer = new(PathNodes))
            {
                foreach ((double x, double y) in nodes)
                    writer.WriteLine($"{x} {y}");
            }
        }
        public void LoadTrianglesToFile(IEnumerable<IFiniteElement> elements)
        {
            using (StreamWriter writer = new(PathTriangles))
            {
                foreach (var element in elements)
                {
                    if (element.VertexNumber.Length == 2)
                        continue;

                    int num1 = element.VertexNumber[0];
                    int num2 = element.VertexNumber[1];
                    int num3 = element.VertexNumber[2];

                    writer.WriteLine($"{num1} {num2} {num3}");
                }
            }
        }
        public void LoadValuesToFile(double[] values, string path = "")
        {
            using (StreamWriter writer = path == "" ? new StreamWriter(PathValues) : new StreamWriter(path))
            {
                foreach (double value in values)
                    writer.WriteLine(value);
            }
        }

        public void CopyDirectory(string pathSource, string pathTarget)
        {
            Directory.CreateDirectory(pathTarget);

            foreach (var file in Directory.GetFiles(pathSource))
            {
                string targetFile = Path.Combine(pathTarget, Path.GetFileName(file));
                File.Copy(file, targetFile, overwrite: true);
            }

            foreach (var subDir in Directory.GetDirectories(pathSource))
            {
                string targetSubDir = Path.Combine(pathTarget, Path.GetFileName(subDir));
                Directory.CreateDirectory(targetSubDir);
                CopyDirectory(subDir, targetSubDir);
            }
        }
    }
}
