using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FEM;
using TelmaCore;
using Quadratures;
using static FEM.IAdaptiveFiniteElementMesh;

namespace AdaptiveGrids
{
    public class Solution : ISolution
    {
        public Solution(IAdaptiveFiniteElementMesh mesh, ITimeMesh timeMesh, string _path = "")
        {
            Mesh = mesh;
            TimeMesh = timeMesh;
            solutionVector = new double[mesh.NumberOfDofs];

            if (_path.Length == 0)
                path = "ParabolicProblemWeights";

            if (Directory.Exists(path))
                Directory.Delete(path, true);

            Directory.CreateDirectory(path!);
        }

        double time = -1;
        public double Time
        {
            get { return time; }

            set
            {
                if (TimeMesh[0] <= value && value <= TimeMesh[TimeMesh.Size() - 1])
                {
                    if (value != time)
                    {
                        int ind = BinarySearch(TimeMesh, value, 0, TimeMesh.Size() - 1);
                        time = TimeMesh[ind];

                        using (StreamReader reader = new StreamReader(Path.Combine(path, time.ToString() + ".txt")))
                        {
                            string? coeff = null;

                            for (int i = 0; (coeff = reader.ReadLine()) != null; ++i)
                            {
                                solutionVector[i] = double.Parse(coeff);
                            }
                        }
                    }
                }
            }
        }

        string path = "";
        public IAdaptiveFiniteElementMesh Mesh { get; }
        public ITimeMesh TimeMesh { get; }

        double[] solutionVector { get; }
        public ReadOnlySpan<double> SolutionVector => solutionVector;

        public IDictionary<(int i, int j), double> CalcDifferenceOfFlow(IDictionary<string, IMaterial> materials, IDictionary<(int i, int j), int> numberOccurrencesOfEdges)
        {
            var differenceFlow = new Dictionary<(int i, int j), double>();
            var flowAtCenterElements = new Dictionary<(int i, int j), double>();
            var quadratures = new QuadratureNodes<double>([.. NumericalIntegration.GaussQuadrature1DOrder3()], 3);

            foreach (var element in Mesh.Elements)
            {
                if (element.VertexNumber.Length == 2)
                    continue;

                var lambda = materials[element.Material].Lambda;

                var point1 = Mesh.Vertex[element.VertexNumber[0]];
                var point2 = Mesh.Vertex[element.VertexNumber[1]];
                var point3 = Mesh.Vertex[element.VertexNumber[2]];

                var center = (point1 + point2 + point3) / 3.0;

                double flowAtCenter = (lambda(center) * element.GetGradientAtPoint(Mesh.Vertex, SolutionVector, center)).Norm;

                for (int i = 0; i < element.NumberOfEdges; ++i)
                {
                    var edge = element.Edge(i);
                    edge = (element.VertexNumber[edge.i], element.VertexNumber[edge.j]);

                    var x0 = Mesh.Vertex[edge.i].X;
                    var x1 = Mesh.Vertex[edge.j].X;
                    var y0 = Mesh.Vertex[edge.i].Y;
                    var y1 = Mesh.Vertex[edge.j].Y;

                    var lengthEdge = Math.Sqrt((x1 - x0) * (x1 - x0) + (y1 - y0) * (y1 - y0));

                    var vectorOuterNormal = new Vector2D(y1 - y0, -(x1 - x0));
                    vectorOuterNormal /= lengthEdge;

                    var flowAcrossEdge = NumericalIntegration.NumericalValueIntegralOnEdge(quadratures,
                        t =>
                        {
                            var x = x0 * (1 - t) + x1 * t;
                            var y = y0 * (1 - t) + y1 * t;

                            return lambda(new(x, y)) * vectorOuterNormal * element.GetGradientAtPoint(Mesh.Vertex, SolutionVector, new(x, y));
                        });

                    if (edge.i > edge.j)
                        edge = (edge.j, edge.i);

                    if (numberOccurrencesOfEdges[edge] == 1)
                        differenceFlow.Add(edge, 0.0);
                    else
                    {
                        if (differenceFlow.TryGetValue(edge, out var curFlow))
                        {
                            double flow = flowAtCenterElements[edge];

                            double weight = Mesh.TypeDifference switch
                            {
                                TypeRelativeDifference.Relative => double.Max(flow, flowAtCenter),
                                //double.Max(Math.Abs(curFlow), Math.Abs(flowAcrossEdge)),

                                TypeRelativeDifference.Absolute => 1.0,

                                _ => throw new Exception("Invalid type relative difference")
                            };

                            // TODO:
                            // попробовать относительно максимума модулей градиентов в центрах смежных
                            // как разницу нормально считать, там разные знаки, что логично
                            // пришло в голову в тупую поставить плюс, можно брать модуль разности модулей,
                            // можно брать по одинаковой нормали, как лучше? Будто одинаково и проще всего 1 или 2 вариант.
                            differenceFlow[edge] = Math.Abs(curFlow + flowAcrossEdge) / weight;
                            //                            differenceFlow[edge] = Math.Abs(curFlow - flowAcrossEdge) / weight;
                            //differenceFlow[edge] = Math.Log(1.0 + Math.Abs(curFlow - flowAcrossEdge) / Math.Abs((curFlow + flowAcrossEdge) / 2.0));
                        }
                        else
                        {
                            differenceFlow.TryAdd(edge, flowAcrossEdge);
                            flowAtCenterElements.TryAdd(edge, flowAtCenter);
                        }
                    }
                }
            }

            return differenceFlow;
        }

        public double Value(Vector2D point)
        {
            foreach (var element in Mesh.Elements)
            {
                if (element.VertexNumber.Length != 2)
                {
                    if (element.IsPointOnElement(Mesh.Vertex, point))
                    {
                        return element.GetValueAtPoint(Mesh.Vertex, solutionVector, point);
                    }
                }
            }

            return -100000; // значит точка вне области
        }

        public Vector2D Gradient(Vector2D point)
        {
            foreach (var element in Mesh.Elements)
            {
                if (element.VertexNumber.Length != 2)
                {
                    if (element.IsPointOnElement(Mesh.Vertex, point))
                    {
                        return element.GetGradientAtPoint(Mesh.Vertex, solutionVector, point);
                    }
                }
            }

            return new Vector2D(-100000, -100000); // значит точка вне области
        }

        public Vector2D Flow(IDictionary<string, IMaterial> materials, Vector2D point)
        {
            foreach (var element in Mesh.Elements)
            {
                if (element.VertexNumber.Length != 2 &&
                    element.IsPointOnElement(Mesh.Vertex, point))
                {
                    var lambda = materials[element.Material].Lambda;

                    return lambda(point) * element.GetGradientAtPoint(Mesh.Vertex, SolutionVector, point);
                }
            }

            return new(-10000, -10000);
        }

        public void AddSolutionVector(double t, double[] solution)
        {
            using (StreamWriter writer = new StreamWriter(Path.Combine(path, t.ToString() + ".txt"), false))
            {
                foreach (var coeff in solution)
                    writer.WriteLine(coeff);
            }
        }

        public static int BinarySearch(ITimeMesh timeMesh, double target, int low, int high)
        {
            int mid = 0;
            double midValue = 0;

            while (low <= high)
            {
                mid = (low + high) / 2;
                midValue = timeMesh[mid];

                if (midValue == target)
                    return mid;
                else if (midValue < target)
                    low = mid + 1;
                else
                    high = mid - 1;
            }

            return mid;
        }
    }

    public class ValueAtCenter
    {
        public double Val { get; }
        public Vector2D Center { get; }
        public ValueAtCenter(double val, Vector2D center)
        {
            Center = center;
            Val = val;
        }
    }
}