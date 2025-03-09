using FEM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelmaCore;

namespace AdaptiveGrids
{
    public class FileManager
    {
        public FileManager(string pathNodes, string pathTriangles, string pathValues)
        {
            PathNodes = pathNodes;
            PathTriangles = pathTriangles;
            PathValues = pathValues;
        }

        private string PathNodes { get; }
        private string PathTriangles { get; }
        private string PathValues { get; }

        public void LoadToFile(Vector2D[] nodes, IEnumerable<IFiniteElement> elements, double[] coeffs)
        {
            var values = coeffs[..nodes.Length];

            LoadNodesToFile(nodes);
            LoadTrianglesToFile(elements);
            LoadValuesToFile(values);
        }

        public void LoadNodesToFile(Vector2D[] nodes)
        {
            var streamWriter = new StreamWriter(PathNodes);

            foreach ((double x, double y) in nodes)
                streamWriter.WriteLine($"{x} {y}");

            streamWriter.Close();
        }
        public void LoadTrianglesToFile(IEnumerable<IFiniteElement> elements)
        {
            var streamWriter = new StreamWriter(PathTriangles);

            foreach (var element in elements)
            {
                if (element.VertexNumber.Length == 2)
                    continue;

                int num1 = element.VertexNumber[0];
                int num2 = element.VertexNumber[1];
                int num3 = element.VertexNumber[2];

                streamWriter.WriteLine($"{num1} {num2} {num3}");
            }

            streamWriter.Close();
        }
        public void LoadValuesToFile(double[] values)
        {
            var streamWriter = new StreamWriter(PathValues);

            foreach (double value in values)
                streamWriter.WriteLine(value);

            streamWriter.Close();
        }

    }
}
