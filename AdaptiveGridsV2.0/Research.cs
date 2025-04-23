using FEM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelmaCore;

namespace AdaptiveGrids
{
    public class Research
    {
        public Research(double x0, double x1, int sizeX,
                        double y0, double y1, int sizeY,
                        double r0, double r1, int sizeR,
                        double phi0, double phi1, int sizePhi,
                        ISolution goalSolution, string savePath = "")
        {
            SavePath = savePath == "" ? "Research" : savePath;
            GoalSolution = goalSolution;

            X = SplitSegment(x0, x1, sizeX);
            Y = SplitSegment(y0, y1, sizeY);
            double[] r = SplitSegment(r0, r1, sizeR);
            double[] phi = SplitSegment(phi0, phi1, sizePhi);

            Points = new Vector2D[r.Length * phi.Length];
            int iter = 0;

            for (int s = 0; s < phi.Length; s++)
            {
                double cosPhi = Math.Cos(phi[s]);
                double sinPhi = Math.Sin(phi[s]);

                for (int p = 0; p < r.Length; p++)
                {
                    double x = r[p] * cosPhi;
                    double y = r[p] * sinPhi;

                    Points[iter] = new(x, y);
                    iter++;
                }
            }

            GoalSolution = goalSolution;
        }
        public enum SaveType { Max, Min }
        public enum Difference { ShiftFromStartSolution, DiffWithGoalSolution }
        public string SavePath { get; }
        public double[] X { get; set; }
        public double[] Y { get; set; }
        public Vector2D[] Points { get; set; }
        ISolution GoalSolution { get; }

        public void DoResearch(IAdaptiveFiniteElementMesh startMesh, IDictionary<string, IMaterial> materials,
                               SaveType saveType, Difference diffType)
        {
            string generalDirectory = Path.Combine("Output", SavePath);
            Directory.CreateDirectory(generalDirectory);

            StreamWriter log = new(Path.Combine(generalDirectory, "log.txt"), append:true);

            EllipticalProblem startProblem = new(materials, startMesh);

            startProblem.Prepare();
            Solution startSolution = new(startMesh, new TimeMesh([0.0]));
            var discrepancy = startProblem.Solve(startSolution);

            log.WriteLine($"Discrepancy solution of SLAE for start mesh: {discrepancy:e4}\n");

            double startDifference = CalcShiftNewSolution(GoalSolution, startSolution, startSolution.Mesh.Vertex.Length);

            double[] valuesGoalSolution = CalcValuesGoalSolution(GoalSolution);
            double[] valuesStartSolution = CalcValuesGoalSolution(startSolution);

            double[] differences = new double[10];
            Rectangle[] rectangles = new Rectangle[10];
            int count = 0;

            for (int s = 0; s < Y.Length - 1; s++)
            {
                for (int p = 0; p < X.Length - 1; p++)
                {
                    var newMesh = startMesh.DoubleInsideRectangle(X[p], X[p + 1], Y[s], Y[s + 1]);

                    EllipticalProblem newProblem = new(materials, newMesh);

                    newProblem.Prepare();
                    Solution newSolution = new(newMesh, new TimeMesh([0.0]));
                    var newDiscrepancy = newProblem.Solve(newSolution);

                    double difference = CalcShiftNewSolution(GoalSolution, newSolution, startSolution.Mesh.Vertex.Length);

                    /*double difference = diffType switch
                    {
                        Difference.ShiftFromStartSolution => CalcShiftNewSolution(valuesStartSolution, newSolution),
                        Difference.DiffWithGoalSolution => CalcDifferenceOfSolutions(valuesGoalSolution, newSolution),
                        _ => throw new Exception("Invalid.")
                    };*/

                    //double difference = CalcDifferenceOfSolutions(valuesGoalSolution, newSolution);

                    switch (saveType)
                    {
                        case SaveType.Max:
                        {
                            SaveDifferenceMax(differences, rectangles, ref count, difference, new(s * (X.Length - 1) + p,
                                                                                                  X[p], X[p + 1],
                                                                                                  Y[s], Y[s + 1],
                                                                                                  startSolution,
                                                                                                  newSolution));
                            break;
                        }

                        case SaveType.Min:
                        {
                            SaveDifferenceMin(differences, rectangles, ref count, difference, new(s * (X.Length - 1) + p,
                                                                                                  X[p], X[p + 1],
                                                                                                  Y[s], Y[s + 1],
                                                                                                  startSolution,
                                                                                                  newSolution));
                            break;
                        }
                        default:
                            throw new Exception("Invalid.");
                    }

/*                    SaveDifferenceInAscendingOrder(differences, rectangles, ref count, difference, new(s * (X.Length - 1) + p,
                                                                                       X[p], X[p + 1],
                                                                                       Y[s], Y[s + 1],
                                                                                       startSolution,
                                                                                       newSolution));*/

                    string str = $"""
                        Number of rectangle - {s * (X.Length - 1) + p}
                            Rectangle:
                            x0 = {X[p]}
                            x1 = {X[p + 1]}
                            y0 = {Y[s]}
                            y1 = {Y[s + 1]}

                            Start mesh:
                            Number of dofs - {startSolution.Mesh.NumberOfDofs}
                            Number of triangles - {startSolution.Mesh.Elements.Where(x => x.VertexNumber.Length != 2).Count()}
                            Discrepancy solution of SLAE - {discrepancy:e4}

                            New mesh:
                            Number of dofs - {newSolution.Mesh.NumberOfDofs}
                            Number of triangles - {newSolution.Mesh.Elements.Where(x => x.VertexNumber.Length != 2).Count()}
                            Discrepancy solution of SLAE - {newDiscrepancy:e4}

                            Relative difference = {difference}
                            Start difference = {startDifference}
                            Start difference - relative difference = {startDifference - difference}

                        """;

                    log.WriteLine(str);
                    Console.WriteLine(str);
                }
            }

            log.Close();
            SaveResult(differences, rectangles, materials);
        }

        private double CalcDifferenceOfSolutions(double[] solution1, ISolution solution2)
        {
            double difference = 0.0;
            double weight = 0.0;

            for (int i = 0; i < Points.Length; i++)
            {
                double valueSolution1 = solution1[i];
                double valueSolution2 = solution2.Value(Points[i]);

                weight += valueSolution1 * valueSolution1;

                difference += (valueSolution1 - valueSolution2) * (valueSolution1 - valueSolution2);
            }

            return Math.Sqrt(difference / weight);
        }

        private double CalcShiftNewSolution(double[] startSolution, ISolution newSolution)
        {
            double difference = 0.0;
            double weight = 0.0;

            for (int i = 0; i < Points.Length; i++)
            {
                double valueStartSolution = startSolution[i];
                double valueNewSolution = newSolution.Value(Points[i]);

                weight += valueStartSolution * valueStartSolution;

                difference += (valueStartSolution - valueNewSolution) * (valueStartSolution - valueNewSolution);
            }

            return Math.Sqrt(difference / weight);
        }

        private double CalcShiftNewSolution(ISolution startSolution, ISolution newSolution, int length)
        {
            double difference = 0.0;

            for (int i = 0; i < length; i++)
            {
                double weight1 = startSolution.SolutionVector[i];
                double weight2 = newSolution.SolutionVector[i];

                difference += (weight1 - weight2) * (weight1 - weight2);
            }

            return Math.Sqrt(difference);
        }

        private double[] CalcValuesGoalSolution(ISolution goalSolution)
        {
            double[] values = new double[Points.Length];

            for (int i = 0; i < Points.Length; i++)
                values[i] = goalSolution.Value(Points[i]);

            return values;
        }

        private void SaveDifferenceMax(double[] differences, Rectangle[] rectangles, ref int count, double difference, Rectangle rectangle)
        {
            if (count == 10 && difference < differences[9])
                return;

            int i = count == 10 ? 8 : count - 1;

            while (i >= 0 && differences[i] < difference)
            {
                differences[i + 1] = differences[i];
                rectangles[i + 1] = rectangles[i];
                i--;
            }

            differences[i + 1] = difference;
            rectangles[i + 1] = rectangle;

            if (count < 10)
                count++;
        }

        private void SaveDifferenceMin(double[] differences, Rectangle[] rectangles, ref int count, double difference, Rectangle rectangle)
        {
            if (count == 10 && difference > differences[9])
                return;

            int i = count == 10 ? 8 : count - 1;

            while (i >= 0 && differences[i] > difference)
            {
                differences[i + 1] = differences[i];
                rectangles[i + 1] = rectangles[i];
                i--;
            }

            differences[i + 1] = difference;
            rectangles[i + 1] = rectangle;

            if (count < 10)
                count++;
        }

        private void SaveResult(double[] differences, Rectangle[] rectangles, IDictionary<string, IMaterial> materials)
        {
            string generalDirectory = Path.Combine("Output", SavePath);
            Directory.CreateDirectory(generalDirectory);

            for (int i = 0; i < differences.Length; i++)
            {
                string directory = Path.Combine(generalDirectory, "Results" + (i + 1).ToString());
                Directory.CreateDirectory(directory);

                StreamWriter writerData = new(Path.Combine(directory, "data.txt"));

                writerData.WriteLine($"""
                    Relative differences - {differences[i]}
                    Border of rectangle:
                    x0 = {rectangles[i].X0}
                    x1 = {rectangles[i].X1}
                    y0 = {rectangles[i].Y0}
                    y1 = {rectangles[i].Y1}
                    Number of rectangle - {rectangles[i].Number}

                    Number of dofs of start mesh - {rectangles[i].StartSolution.Mesh.NumberOfDofs}
                    Number of triangles of start mesh - {rectangles[i].StartSolution.Mesh.Elements.Where(x => x.VertexNumber.Length != 2).Count()}

                    Number of dofs of start mesh - {rectangles[i].NewSolution.Mesh.NumberOfDofs}
                    Number of triangles of start mesh - {rectangles[i].NewSolution.Mesh.Elements.Where(x => x.VertexNumber.Length != 2).Count()}
                    """);

                writerData.Close();

                var fileManager = new FileManager(Path.Combine(directory, "verticesBeforeAddaptation.txt"),
                                                  Path.Combine(directory, "trianglesBeforeAddaptation.txt"),
                                                  Path.Combine(directory, "valuesBeforeAddaptation.txt"));

                fileManager.LoadToFile(rectangles[i].StartSolution.Mesh.Vertex, rectangles[i].StartSolution.Mesh.Elements, [.. rectangles[i].StartSolution.SolutionVector]);

                var xFlowValues = new double[rectangles[i].StartSolution.Mesh.Vertex.Length];
                var yFlowValues = new double[rectangles[i].StartSolution.Mesh.Vertex.Length];

                for (int k = 0; k < rectangles[i].StartSolution.Mesh.Vertex.Length; k++)
                {
                    var flow = rectangles[i].StartSolution.Flow(materials, rectangles[i].StartSolution.Mesh.Vertex[k]);

                    xFlowValues[k] = flow.X;
                    yFlowValues[k] = flow.Y;
                }

                fileManager.LoadValuesToFile(xFlowValues, Path.Combine(directory, "dxValuesBeforeAdaptation.txt"));
                fileManager.LoadValuesToFile(yFlowValues, Path.Combine(directory, "dyValuesBeforeAdaptation.txt"));

                fileManager = new FileManager(Path.Combine(directory, "verticesAfterAddaptation.txt"),
                                              Path.Combine(directory, "trianglesAfterAddaptation.txt"),
                                              Path.Combine(directory, "valuesAfterAddaptation.txt"));

                fileManager.LoadToFile(rectangles[i].NewSolution.Mesh.Vertex, rectangles[i].NewSolution.Mesh.Elements, [.. rectangles[i].NewSolution.SolutionVector]);

                xFlowValues = new double[rectangles[i].NewSolution.Mesh.Vertex.Length];
                yFlowValues = new double[rectangles[i].NewSolution.Mesh.Vertex.Length];

                for (int k = 0; k < rectangles[i].NewSolution.Mesh.Vertex.Length; k++)
                {
                    var flow = rectangles[i].NewSolution.Flow(materials, rectangles[i].NewSolution.Mesh.Vertex[k]);

                    xFlowValues[k] = flow.X;
                    yFlowValues[k] = flow.Y;
                }

                fileManager.LoadValuesToFile(xFlowValues, Path.Combine(directory, "dxValuesAfterAdaptation.txt"));
                fileManager.LoadValuesToFile(yFlowValues, Path.Combine(directory, "dyValuesAfterAdaptation.txt"));

                double[] weightsDifference = new double[rectangles[i].StartSolution.Mesh.Vertex.Length];

                for (int k = 0; k < rectangles[i].StartSolution.Mesh.Vertex.Length; k++)
                    weightsDifference[k] = rectangles[i].StartSolution.SolutionVector[k] - GoalSolution.SolutionVector[k];

                fileManager.LoadValuesToFile(weightsDifference, Path.Combine(directory, "weightsDifferencesBefore.txt"));

                for (int k = 0; k < rectangles[i].StartSolution.Mesh.Vertex.Length; k++)
                    weightsDifference[k] = rectangles[i].NewSolution.SolutionVector[k] - GoalSolution.SolutionVector[k];

                fileManager.LoadValuesToFile(weightsDifference, Path.Combine(directory, "weightsDifferencesAfter.txt"));
            }
        }

        public double[] SplitSegment(double beg, double end, int size)
        {
            double[] coords = new double[size + 1];
            double h = (end - beg) / size;

            coords[0] = beg;
            for (int i = 1; i < size; i++)
                coords[i] = beg + i * h;
            coords[^1] = end;

            return coords;
        }
    }
}
