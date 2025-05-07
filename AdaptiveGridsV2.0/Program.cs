using FEM;
using TelmaCore;
using AdaptiveGrids;
using AdaptiveGrids.FiniteElements2D;
using AdaptiveGrids.FiniteElements1D;
using static FEM.IAdaptiveFiniteElementMesh;
using Quadratures;
using System.Xml.Linq;
using System.Xml;
System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

double[] t = { 0, 0.1 };

double[] array = new double[10];
Array.Fill(array, double.MaxValue);

TimeMesh timeMesh = new TimeMesh(t);

//var generator = new GeneratorOfTriangleMesh(0.0, Math.PI, 0.0, Math.PI, 16, 16);

var areas = new List<SubArea>();

/*areas.Add(new(0, 1, 0, 1, "air"));
areas.Add(new(1, 2, 0, 1, "iron"));*/

areas.Add(new(0, 1, 0, 1, "volume"));

/*var generator = new GeneratorOfRectangleMesh([0.0, Math.PI / 2.0, Math.PI],
                                             [0.0, Math.PI],
                                             [8, 8],
                                             [16],
                                             [1.0, 1.0],
                                             [1.0],
                                             [.. areas]);*/

/*var generator = new GeneratorOfRectangleMesh([0.0, Math.PI],
                                             [0.0, Math.PI],
                                             [16],
                                             [16],
                                             [1.0],
                                             [1.0],
                                             [.. areas]);*/

/*var generator = new GeneratorOfRectangleMesh([0.0, 1.0],
                                             [0.0, 3.0],
                                             [16],
                                             [16],
                                             [1.0],
                                             [1.0],
                                             [.. areas]);

(IFiniteElement[] elements, Vector2D[] vertex) = generator.GenerateToMesh();*/

Console.WriteLine(
    """
    Select type difference of Flow:
    <1> - Relative difference of flow.
    <2> - Absolute difference of flow.
    """);
var type = (TypeRelativeDifference)(int.Parse(Console.ReadLine()!) - 1);

Console.WriteLine(type switch
{
    TypeRelativeDifference.Relative => "Selected <1> - Relative difference of flow.",
    TypeRelativeDifference.Absolute => "Selected <2> - Absolute difference of flow.",
    _ => throw new Exception("Invalid.")
});

//IAdaptiveFiniteElementMesh mesh = new FiniteElementMesh(elements, vertex, type);
FileManager manager = new FileManager();
//IAdaptiveFiniteElementMesh mesh = manager.ReadMeshFromTelma("Input\\квадруполь_грубая.txt", type);
//IAdaptiveFiniteElementMesh mesh = manager.ReadMyMeshFormat("Input\\квадруполь_грубая.txt", type);
IAdaptiveFiniteElementMesh startMesh = manager.ReadMeshFromTelma("Input\\квадруполь_грубая.txt", type);
//IAdaptiveFiniteElementMesh startMesh = manager.ReadMyMeshFormat("Input\\adaptedMesh.txt", type);
IAdaptiveFiniteElementMesh quadMesh = manager.ReadMyMeshFormat("Input\\квадруполь_грубая_учетверенная.txt", type);
//IAdaptiveFiniteElementMesh quadMesh = manager.ReadMyMeshFormat("Input\\квадруполь_грубая_удвоенная.txt", type);

IDictionary<string, IMaterial> materials = new Dictionary<string, IMaterial>();

materials.Add("air", new Material(true, false, false, x => 1, x => 0, (x, t) => 0, (x, t) => 0, (x, t) => 0));
materials.Add("JMinus", new Material(true, false, false, x => 1, x => 0, (x, t) => 0, (x, t) => 0, (x, t) => -1));
materials.Add("JPlus", new Material(true, false, false, x => 1, x => 0, (x, t) => 0, (x, t) => 0, (x, t) => 1));
materials.Add("steel", new Material(true, false, false, x => 0.01, x => 0, (x, t) => 0, (x, t) => 0, (x, t) => 0));

materials.Add("Zero tangent", new Material(false, true, false, x => 1, x => 0, (x, t) => 0, (x, t) => 0.0, (x, t) => 0));

/*materials.Add("volume", new Material(true, false, false, x => 1, x => 0, (x, t) => 0, (x, t) => 0, (x, t) => -2.0 / 3.0 * x.X));
materials.Add("1", new Material(false, true, false, x => 1, x => 0, (x, t) => 0, (x, t) => x.X * x.X * x.X / 9.0, (x, t) => 0));
materials.Add("2", new Material(false, true, false, x => 1, x => 0, (x, t) => 0, (x, t) => 1.0 / 9.0, (x, t) => 0));
materials.Add("3", new Material(false, true, false, x => 1, x => 0, (x, t) => 0, (x, t) => x.X * x.X * x.X / 9.0, (x, t) => 0));
materials.Add("4", new Material(false, true, false, x => 1, x => 0, (x, t) => 0, (x, t) => 0.0, (x, t) => 0));*/

/*materials.Add("volume", new Material(true, false, false, x => 1, x => 0, (x, t) => 0, (x, t) => 0, (x, t) => 2 * Math.Sin(x.X + x.Y)));
materials.Add("1", new Material(false, true, false, x => 1, x => 0, (x, t) => 0, (x, t) => Math.Sin(x.X), (x, t) => -6 * (x.X + x.Y)));
materials.Add("2", new Material(false, true, false, x => 1, x => 0, (x, t) => -Math.Cos(x.X + 1), (x, t) => Math.Sin(Math.PI + x.Y), (x, t) => -6 * (x.X + x.Y)));
materials.Add("3", new Material(false, true, false, x => 1, x => 0, (x, t) => 216, (x, t) => Math.Sin(x.X + Math.PI), (x, t) => -6 * (x.X + x.Y)));
materials.Add("4", new Material(false, true, false, x => 1, x => 0, (x, t) => 0, (x, t) => Math.Sin(x.Y), (x, t) => -6 * (x.X + x.Y)));*/

/*materials.Add("air", new Material(true, false, false, x => 1, x => 0, (x, t) => 0, (x, t) => 0, (x, t) => 2 * Math.Sin(x.X) * Math.Sin(x.Y)));
materials.Add("iron", new Material(true, false, false, x => 10000, x => 0, (x, t) => 0, (x, t) => 0, (x, t) => 20000 * Math.Sin(x.X) * Math.Sin(x.Y)));
materials.Add("1", new Material(false, true, false, x => 1, x => 0, (x, t) => 0, (x, t) => 0, (x, t) => -6 * (x.X + x.Y)));
materials.Add("2", new Material(false, true, false, x => 1, x => 0, (x, t) => 0, (x, t) => 0, (x, t) => -6 * (x.X + x.Y)));
materials.Add("3", new Material(false, true, false, x => 1, x => 0, (x, t) => 0, (x, t) => 0, (x, t) => -6 * (x.X + x.Y)));
materials.Add("4", new Material(false, true, false, x => 1, x => 0, (x, t) => 0, (x, t) => 0, (x, t) => -6 * (x.X + x.Y)));*/

/*materials.Add("volume", new Material(true, false, false, x => 1, x => 1, (x, t) => 0, (x, t) => 0, (x, t) => Math.Sin(x.X) + Math.Sin(x.Y)));
materials.Add("1", new Material(false, true, false, x => 1, x => 1, (x, t) => 0, (x, t) => Math.Sin(x.X), (x, t) => -6 * (x.X + x.Y)));
materials.Add("2", new Material(false, true, false, x => 1, x => 1, (x, t) => -Math.Cos(x.X + 1), (x, t) => Math.Sin(Math.PI) + Math.Sin(x.Y), (x, t) => -6 * (x.X + x.Y)));
materials.Add("3", new Material(false, true, false, x => 1, x => 1, (x, t) => 216, (x, t) => Math.Sin(x.X) + Math.Sin(Math.PI), (x, t) => -6 * (x.X + x.Y)));
materials.Add("4", new Material(false, true, false, x => 1, x => 1, (x, t) => 0, (x, t) => Math.Sin(x.Y), (x, t) => -6 * (x.X + x.Y)));*/

/*materials.Add("volume", new Material(true, false, false, x => 1, x => 0, (x, t) => 0, (x, t) => 0, (x, t) => Math.Cos(x.X) + Math.Cos(x.Y)));
materials.Add("1", new Material(false, true, false, x => 1, x => 0, (x, t) => 0, (x, t) => Math.Cos(x.X) + 1, (x, t) => 0));
materials.Add("2", new Material(false, true, false, x => 1, x => 0, (x, t) => 0, (x, t) => -1 + Math.Cos(x.Y), (x, t) => 0));
materials.Add("3", new Material(false, true, false, x => 1, x => 0, (x, t) => 0, (x, t) => Math.Cos(x.X) - 1, (x, t) => 0));
materials.Add("4", new Material(false, true, false, x => 1, x => 0, (x, t) => 0, (x, t) => 1 + Math.Cos(x.Y), (x, t) => 0));*/

/*materials.Add("volume", new Material(true, false, false, x => 1, x => 0, (x, t) => 0, (x, t) => 0, (x, t) => Math.Cos(x.X) + Math.Sin(x.Y)));
materials.Add("1", new Material(false, true, false, x => 1, x => 0, (x, t) => 0, (x, t) => Math.Cos(x.X), (x, t) => 0));
materials.Add("2", new Material(false, true, false, x => 1, x => 0, (x, t) => 0, (x, t) => -1 + Math.Sin(x.Y), (x, t) => 0));
materials.Add("3", new Material(false, true, false, x => 1, x => 0, (x, t) => 0, (x, t) => Math.Cos(x.X), (x, t) => 0));
materials.Add("4", new Material(false, true, false, x => 1, x => 0, (x, t) => 0, (x, t) => 1 + Math.Cos(x.Y), (x, t) => 0));*/

/*materials.Add("volume", new Material(true, false, false, x => 1, x => 0, (x, t) => 0, (x, t) => 0, (x, t) => Math.Sin(x.X) - Math.Exp(-x.Y)));
materials.Add("1", new Material(false, true, false, x => 1, x => 0, (x, t) => 0, (x, t) => Math.Sin(x.X) + 1, (x, t) => 0));
materials.Add("2", new Material(false, true, false, x => 1, x => 0, (x, t) => 0, (x, t) => Math.Exp(-x.Y), (x, t) => 0));
materials.Add("3", new Material(false, true, false, x => 1, x => 0, (x, t) => 0, (x, t) => Math.Sin(x.X) + Math.Exp(-Math.PI), (x, t) => 0));
materials.Add("4", new Material(false, true, false, x => 1, x => 0, (x, t) => 0, (x, t) => Math.Exp(-x.Y), (x, t) => 0));*/

//ParabolicProblem problem = new ParabolicProblem(mesh, timeMesh, x => x.X + x.Y, materials);
//ParabolicProblem problem = new ParabolicProblem(mesh, timeMesh, x => x.X * x.X + x.Y * x.Y, materials);
//ParabolicProblem problem = new ParabolicProblem(mesh, timeMesh, x => x.X * x.X * x.X + x.Y * x.Y * x.Y, materials);

//Func<Vector2D, double> initCondition = (x) => x.X * x.X * x.X / 9.0;
Func<Vector2D, double> initCondition = (x) => 0.0;
//Func<Vector2D, double> initCondition = (x) => Math.Sin(x.X + x.Y);

// *********************************************************************
// RESEARCH
/*EllipticalProblem quadProblem = new(materials, quadMesh);

quadProblem.Prepare();
Solution goalSolution = new(quadMesh, new TimeMesh([0.0]));
var discrepancy = quadProblem.Solve(goalSolution);

string outputDirectory = "ResearchShiftWeightsWithDoubleMesh";
Directory.CreateDirectory(Path.Combine("Output", outputDirectory));

Research research = new(0.0, 0.4, 20,
                        0.0, 0.4, 20,
                        0.0, 0.05, 10,
                        0.0, Math.PI / 2.0, 10,
                        goalSolution, outputDirectory);

Console.WriteLine($"""
    Select type saved differences:
    <1> - 10 maximus
    <2> - 10 minimums

    """);

Research.SaveType saveType = (Research.SaveType)(int.Parse(Console.ReadLine()!) - 1);

string saveTypeStr = saveType switch
{
    Research.SaveType.Max => "Selected type save differences - 10 max",
    Research.SaveType.Min => "Selected type save differences - 10 min",
    _ => throw new Exception("Invalid.")
};

Console.WriteLine(saveTypeStr);

Console.WriteLine($"""
    Select type of difference:
    <1> - Shift from start solution
    <2> - Difference with goal solution

    """);

Research.Difference diffType = (Research.Difference)(int.Parse(Console.ReadLine()!) - 1);

string diffTypeStr = diffType switch
{
    Research.Difference.ShiftFromStartSolution => "Selected type differences - Shift from start solution",
    Research.Difference.DiffWithGoalSolution => "Selected type differences - Difference with goal solution",
    _ => throw new Exception("Invalid.")
};

Console.WriteLine(diffTypeStr);

StreamWriter log = new(Path.Combine("Output", outputDirectory, "log.txt"));

log.WriteLine($"""
    {saveTypeStr}
    {diffTypeStr}

    Discrepancy solution of SLAE for quadMesh: {discrepancy:e4}

    """);

log.Close();

research.DoResearch(startMesh, materials, saveType, diffType);

manager.CopyDirectory("Output", "..\\..\\..\\Output");*/

// END RESEARCH
// *********************************************************************

// *********************************************************************
// ADAPTATION
/*
EllipticalProblem problem = new(materials, startMesh);

problem.Prepare();
Solution solution = new Solution(startMesh, new TimeMesh([0.0]));
problem.Solve(solution);

EllipticalProblem quadProblem = new(materials, quadMesh);

quadProblem.Prepare();
Solution goalSolution = new(quadMesh, new TimeMesh([0.0]));
quadProblem.Solve(goalSolution);
//Func<Vector2D, double, double> RealFunc = (x, t) => x.X * x.X * x.X / 9.0;
//Func<Vector2D, double, double> RealFunc = (x, t) => Math.Sin(x.X + x.Y);
//Func<Vector2D, double, double>? RealFunc = null;

string output = "Output";

Directory.CreateDirectory(output);

var fileManager = new FileManager(Path.Combine(output, "verticesBeforeAddaptation.txt"),
                                  Path.Combine(output, "trianglesBeforeAddaptation.txt"),
                                  Path.Combine(output, "valuesBeforeAddaptation.txt"));

fileManager.LoadToFile(startMesh.Vertex, startMesh.Elements, solution.SolutionVector.ToArray());

var xFlowValues = new double[startMesh.Vertex.Length];
var yFlowValues = new double[startMesh.Vertex.Length];

for (int i = 0; i < startMesh.Vertex.Length; i++)
{
    var flow = solution.Flow(materials, startMesh.Vertex[i]);

    xFlowValues[i] = flow.X;
    yFlowValues[i] = flow.Y;
}

fileManager.LoadValuesToFile(xFlowValues, Path.Combine(output, "dxValuesBeforeAdaptation.txt"));
fileManager.LoadValuesToFile(yFlowValues, Path.Combine(output, "dyValuesBeforeAdaptation.txt"));

Console.WriteLine($"""

    Base mesh:
    dofs - {startMesh.NumberOfDofs}
    elements - {startMesh.Elements.Where(x => x.VertexNumber.Length != 2).Count()}

    """);

var addaptedMesh = startMesh.DoAdaptation(solution, materials);

EllipticalProblem addaptedProblem = new(materials, addaptedMesh);

addaptedProblem.Prepare();
var addaptedSolution = new Solution(addaptedMesh, timeMesh);
addaptedProblem.Solve(addaptedSolution);

fileManager = new FileManager(Path.Combine(output, "verticesAfterAddaptation.txt"),
                              Path.Combine(output, "trianglesAfterAddaptation.txt"),
                              Path.Combine(output, "valuesAfterAddaptation.txt"));

fileManager.LoadToFile(addaptedMesh.Vertex, addaptedMesh.Elements, addaptedSolution.SolutionVector.ToArray());

Console.WriteLine($"""
    Adapted mesh:
    dofs - {addaptedMesh.NumberOfDofs}
    elements - {addaptedMesh.Elements.Where(x => x.VertexNumber.Length != 2).Count()}

    """);

xFlowValues = new double[addaptedMesh.Vertex.Length];
yFlowValues = new double[addaptedMesh.Vertex.Length];

for (int i = 0; i < addaptedMesh.Vertex.Length; i++)
{
    var flow = addaptedSolution.Flow(materials, addaptedMesh.Vertex[i]);

    xFlowValues[i] = flow.X;
    yFlowValues[i] = flow.Y;
}

fileManager.LoadValuesToFile(xFlowValues, Path.Combine(output, "dxValuesAfterAdaptation.txt"));
fileManager.LoadValuesToFile(yFlowValues, Path.Combine(output, "dyValuesAfterAdaptation.txt"));

Research research = new(0.0, 0.4, 20,
                        0.0, 0.4, 20,
                        0.0, 0.05, 10,
                        0.0, Math.PI / 2.0, 10,
                        goalSolution);

double[] r = research.SplitSegment(0.0, 0.05, 10);
double[] phi = research.SplitSegment(0.0, Math.PI / 2.0, 10);
int N = r.Length * phi.Length;

double shiftAdaptedFromGoalSolution = 0.0;
double shiftAdaptedFromStartSolution = 0.0;
double shiftStartFromGoalSolution = 0.0;

for (int s = 0; s < phi.Length; s++)
{
    for (int p = 0; p < r.Length; p++)
    {
        double valueStartMesh = solution.Value(new(r[p], phi[s]));
        double valueAdaptedMesh = addaptedSolution.Value(new(r[p], phi[s]));
        double valueGoalSolution = goalSolution.Value(new(r[p], phi[s]));

        shiftAdaptedFromGoalSolution += (valueAdaptedMesh - valueGoalSolution) * (valueAdaptedMesh - valueGoalSolution);
        shiftAdaptedFromStartSolution += (valueAdaptedMesh - valueStartMesh) * (valueAdaptedMesh - valueStartMesh);
        shiftStartFromGoalSolution += (valueGoalSolution - valueStartMesh) * (valueGoalSolution - valueStartMesh);
    }
}

string nameDiffFlow = "flowAverageOnEdgeRelativeMaxAbsFlowOnEdge";
Directory.CreateDirectory(Path.Combine(output, nameDiffFlow));

using (StreamWriter log = new(Path.Combine(output, nameDiffFlow, "log.txt")))
{
    log.WriteLine($"""
    Base mesh:
    dofs - {startMesh.NumberOfDofs}
    elements - {startMesh.Elements.Where(x => x.VertexNumber.Length != 2).Count()}

    """);

    log.WriteLine($"""
    Adapted mesh:
    dofs - {addaptedMesh.NumberOfDofs}
    elements - {addaptedMesh.Elements.Where(x => x.VertexNumber.Length != 2).Count()}

    """);

    log.WriteLine($"Shift start solution from goal solution - {(shiftStartFromGoalSolution / N):e5}");
    log.WriteLine($"Shift adapted solution from goal solution - {(shiftAdaptedFromGoalSolution / N):e5}");
    log.WriteLine($"Shift adapted solution from start solution - {(shiftAdaptedFromStartSolution / N):e5}");
}

fileManager.CopyDirectory(output, "..\\..\\..\\Output");*/

// END ADAPTATION
// *********************************************************************

// *********************************************************************
// CYCLE ADAPTATION

/*EllipticalProblem startProblem = new(materials, startMesh);

startProblem.Prepare();
Solution startSolution = new Solution(startMesh, new TimeMesh([0.0]));
startProblem.Solve(startSolution);

EllipticalProblem quadProblem = new(materials, quadMesh);

quadProblem.Prepare();
Solution goalSolution = new(quadMesh, new TimeMesh([0.0]));
quadProblem.Solve(goalSolution);

string output = Path.Combine("Output", "CycleAdaptationThreeIter35PercFromMaxWithConstraintAndSourceThirdCriteria");
Directory.CreateDirectory(output);

int countAdaptation = 4;

var meshBefore = startMesh;
EllipticalProblem problemBefore = new(materials, meshBefore);
problemBefore.Prepare();
Solution solutionBefore = new(meshBefore, new TimeMesh([0.0]));
problemBefore.Solve(solutionBefore);

Research research = new(0.0, 0.4, 20,
                        0.0, 0.4, 20,
                        0.0, 0.05, 10,
                        0.0, Math.PI / 2.0, 10,
                        goalSolution);

double[] r = research.SplitSegment(0.0, 0.05, 10);
double[] phi = research.SplitSegment(0.0, Math.PI / 2.0, 10);
int N = r.Length * phi.Length;

for (int i = 0; i < countAdaptation; i++)
{
    string directory = Path.Combine(output, "Adaptation" + i.ToString());
    Directory.CreateDirectory(directory);

    StreamWriter log = new(Path.Combine(directory, "log.txt"));

    var fileManager = new FileManager(Path.Combine(directory, "verticesBeforeAddaptation.txt"),
                                      Path.Combine(directory, "trianglesBeforeAddaptation.txt"),
                                      Path.Combine(directory, "valuesBeforeAddaptation.txt"));

    fileManager.LoadToFile(meshBefore.Vertex, meshBefore.Elements, solutionBefore.SolutionVector.ToArray());

    var xFlowValues = new double[meshBefore.Vertex.Length];
    var yFlowValues = new double[meshBefore.Vertex.Length];

    for (int k = 0; k < meshBefore.Vertex.Length; k++)
    {
        var flow = solutionBefore.Flow(materials, meshBefore.Vertex[k]);

        xFlowValues[k] = flow.X;
        yFlowValues[k] = flow.Y;
    }

    fileManager.LoadValuesToFile(xFlowValues, Path.Combine(directory, "dxValuesBeforeAdaptation.txt"));
    fileManager.LoadValuesToFile(yFlowValues, Path.Combine(directory, "dyValuesBeforeAdaptation.txt"));

    string strLog = $"""

    Start mesh {i}:
    dofs - {meshBefore.NumberOfDofs}
    elements - {meshBefore.Elements.Where(x => x.VertexNumber.Length != 2).Count()}

    """;

    log.WriteLine(strLog);
    Console.WriteLine(strLog);

    var meshAfter = meshBefore.DoAdaptation(solutionBefore, materials, directory);

    EllipticalProblem problemAfter = new(materials, meshAfter);
    problemAfter.Prepare();
    Solution solutionAfter = new(meshAfter, new TimeMesh([0.0]));
    problemAfter.Solve(solutionAfter);

    fileManager = new FileManager(Path.Combine(directory, "verticesAfterAddaptation.txt"),
                                  Path.Combine(directory, "trianglesAfterAddaptation.txt"),
                                  Path.Combine(directory, "valuesAfterAddaptation.txt"));

    fileManager.LoadToFile(meshAfter.Vertex, meshAfter.Elements, solutionAfter.SolutionVector.ToArray());

    xFlowValues = new double[meshAfter.Vertex.Length];
    yFlowValues = new double[meshAfter.Vertex.Length];

    for (int k = 0; k < meshAfter.Vertex.Length; k++)
    {
        var flow = solutionAfter.Flow(materials, meshAfter.Vertex[k]);

        xFlowValues[k] = flow.X;
        yFlowValues[k] = flow.Y;
    }

    fileManager.LoadValuesToFile(xFlowValues, Path.Combine(directory, "dxValuesAfterAdaptation.txt"));
    fileManager.LoadValuesToFile(yFlowValues, Path.Combine(directory, "dyValuesAfterAdaptation.txt"));

    strLog = $"""
    Adapted mesh {i}:
    dofs - {meshAfter.NumberOfDofs}
    elements - {meshAfter.Elements.Where(x => x.VertexNumber.Length != 2).Count()}

    """;

    log.WriteLine(strLog);
    Console.WriteLine(strLog);

    fileManager.LoadMyMeshFormat(Path.Combine(directory, "adaptedMesh.txt"), meshAfter);

    double shiftAdaptedFromGoalSolution = 0.0;
    double shiftAdaptedFromStartSolution = 0.0;
    double shiftStartFromGoalSolution = 0.0;

    for (int s = 0; s < phi.Length; s++)
    {
        double cosPhi = Math.Cos(phi[s]);
        double sinPhi = Math.Sin(phi[s]);

        for (int p = 0; p < r.Length; p++)
        {
            Vector2D point = new(r[p] * cosPhi, r[p] * sinPhi);

            double valueStartMesh = solutionBefore.Value(point);
            double valueAdaptedMesh = solutionAfter.Value(point);
            double valueGoalSolution = goalSolution.Value(point);

            shiftAdaptedFromGoalSolution += (valueAdaptedMesh - valueGoalSolution) * (valueAdaptedMesh - valueGoalSolution);
            shiftAdaptedFromStartSolution += (valueAdaptedMesh - valueStartMesh) * (valueAdaptedMesh - valueStartMesh);
            shiftStartFromGoalSolution += (valueGoalSolution - valueStartMesh) * (valueGoalSolution - valueStartMesh);
        }
    }

    strLog = $"""
        Shift start solution from goal solution - {shiftStartFromGoalSolution}
        Shift adapted solution from goal solution - {shiftAdaptedFromGoalSolution}
        Shift adapted solution from start solution - {shiftAdaptedFromStartSolution}

        """;

    log.WriteLine(strLog);

    log.Close();

    meshBefore = meshAfter;
    solutionBefore = solutionAfter;
}

manager.CopyDirectory("Output", "..\\..\\..\\Output");*/

// END CYCLE ADAPTATION
// *********************************************************************

// ******************************************************************************************************************************************************
// NEW CYCLE ADAPTATION

EllipticalProblem startProblem = new(materials, startMesh);

startProblem.Prepare();
Solution startSolution = new Solution(startMesh, new TimeMesh([0.0]));
startProblem.Solve(startSolution);

EllipticalProblem quadProblem = new(materials, quadMesh);

quadProblem.Prepare();
Solution goalSolution = new(quadMesh, new TimeMesh([0.0]));
quadProblem.Solve(goalSolution);

string output = Path.Combine("Output", "Test");
Directory.CreateDirectory(output);

int countAdaptation = 4;

var meshBefore = startMesh;
EllipticalProblem problemBefore = new(materials, meshBefore);
problemBefore.Prepare();
Solution solutionBefore = new(meshBefore, new TimeMesh([0.0]));
problemBefore.Solve(solutionBefore);

Research research = new(0.0, 0.4, 20,
                        0.0, 0.4, 20,
                        0.0, 0.05, 10,
                        0.0, Math.PI / 2.0, 10,
                        goalSolution);

double[] r = research.SplitSegment(0.0, 0.05, 10);
double[] phi = research.SplitSegment(0.0, Math.PI / 2.0, 10);
int N = r.Length * phi.Length;

string directory = Path.Combine(output, "Adaptation0");
Directory.CreateDirectory(directory);

StreamWriter log = new(Path.Combine(directory, "log.txt"));

var fileManager = new FileManager(Path.Combine(directory, "verticesBeforeAddaptation.txt"),
                                  Path.Combine(directory, "trianglesBeforeAddaptation.txt"),
                                  Path.Combine(directory, "valuesBeforeAddaptation.txt"));

fileManager.LoadToFile(meshBefore.Vertex, meshBefore.Elements, solutionBefore.SolutionVector.ToArray());

var xFlowValues = new double[meshBefore.Vertex.Length];
var yFlowValues = new double[meshBefore.Vertex.Length];

for (int k = 0; k < meshBefore.Vertex.Length; k++)
{
    var flow = solutionBefore.Flow(materials, meshBefore.Vertex[k]);

    xFlowValues[k] = flow.X;
    yFlowValues[k] = flow.Y;
}

fileManager.LoadValuesToFile(xFlowValues, Path.Combine(directory, "dxValuesBeforeAdaptation.txt"));
fileManager.LoadValuesToFile(yFlowValues, Path.Combine(directory, "dyValuesBeforeAdaptation.txt"));

string strLog = $"""

    Start mesh 0:
    dofs - {meshBefore.NumberOfDofs}
    elements - {meshBefore.Elements.Where(x => x.VertexNumber.Length != 2).Count()}

    """;

log.WriteLine(strLog);
Console.WriteLine(strLog);

var meshAfter = meshBefore.DoAdaptation(solutionBefore, materials, directory);

EllipticalProblem problemAfter = new(materials, meshAfter);
problemAfter.Prepare();
Solution solutionAfter = new(meshAfter, new TimeMesh([0.0]));
problemAfter.Solve(solutionAfter);

fileManager = new FileManager(Path.Combine(directory, "verticesAfterAddaptation.txt"),
                              Path.Combine(directory, "trianglesAfterAddaptation.txt"),
                              Path.Combine(directory, "valuesAfterAddaptation.txt"));

fileManager.LoadToFile(meshAfter.Vertex, meshAfter.Elements, solutionAfter.SolutionVector.ToArray());

xFlowValues = new double[meshAfter.Vertex.Length];
yFlowValues = new double[meshAfter.Vertex.Length];

for (int k = 0; k < meshAfter.Vertex.Length; k++)
{
    var flow = solutionAfter.Flow(materials, meshAfter.Vertex[k]);

    xFlowValues[k] = flow.X;
    yFlowValues[k] = flow.Y;
}

fileManager.LoadValuesToFile(xFlowValues, Path.Combine(directory, "dxValuesAfterAdaptation.txt"));
fileManager.LoadValuesToFile(yFlowValues, Path.Combine(directory, "dyValuesAfterAdaptation.txt"));

strLog = $"""
    Adapted mesh 0:
    dofs - {meshAfter.NumberOfDofs}
    elements - {meshAfter.Elements.Where(x => x.VertexNumber.Length != 2).Count()}

    """;

log.WriteLine(strLog);
Console.WriteLine(strLog);

fileManager.LoadMyMeshFormat(Path.Combine(directory, "adaptedMesh.txt"), meshAfter);

double shiftAdaptedFromGoalSolution = 0.0;
double shiftAdaptedFromStartSolution = 0.0;
double shiftStartFromGoalSolution = 0.0;

for (int s = 0; s < phi.Length; s++)
{
    double cosPhi = Math.Cos(phi[s]);
    double sinPhi = Math.Sin(phi[s]);

    for (int p = 0; p < r.Length; p++)
    {
        Vector2D point = new(r[p] * cosPhi, r[p] * sinPhi);

        double valueStartMesh = solutionBefore.Value(point);
        double valueAdaptedMesh = solutionAfter.Value(point);
        double valueGoalSolution = goalSolution.Value(point);

        shiftAdaptedFromGoalSolution += (valueAdaptedMesh - valueGoalSolution) * (valueAdaptedMesh - valueGoalSolution);
        shiftAdaptedFromStartSolution += (valueAdaptedMesh - valueStartMesh) * (valueAdaptedMesh - valueStartMesh);
        shiftStartFromGoalSolution += (valueGoalSolution - valueStartMesh) * (valueGoalSolution - valueStartMesh);
    }
}

strLog = $"""
        Shift start solution from goal solution - {shiftStartFromGoalSolution}
        Shift adapted solution from goal solution - {shiftAdaptedFromGoalSolution}
        Shift adapted solution from start solution - {shiftAdaptedFromStartSolution}

        """;

log.WriteLine(strLog);

log.Close();

for (int i = 1; i < countAdaptation; i++)
{
    directory = Path.Combine(output, "Adaptation" + i.ToString());
    Directory.CreateDirectory(directory);

    log = new(Path.Combine(directory, "log.txt"));

    fileManager = new FileManager(Path.Combine(directory, "verticesBeforeAddaptation.txt"),
                                  Path.Combine(directory, "trianglesBeforeAddaptation.txt"),
                                  Path.Combine(directory, "valuesBeforeAddaptation.txt"));

    fileManager.LoadToFile(meshAfter.Vertex, meshAfter.Elements, solutionAfter.SolutionVector.ToArray());

    xFlowValues = new double[meshAfter.Vertex.Length];
    yFlowValues = new double[meshAfter.Vertex.Length];

    for (int k = 0; k < meshAfter.Vertex.Length; k++)
    {
        var flow = solutionAfter.Flow(materials, meshAfter.Vertex[k]);

        xFlowValues[k] = flow.X;
        yFlowValues[k] = flow.Y;
    }

    fileManager.LoadValuesToFile(xFlowValues, Path.Combine(directory, "dxValuesBeforeAdaptation.txt"));
    fileManager.LoadValuesToFile(yFlowValues, Path.Combine(directory, "dyValuesBeforeAdaptation.txt"));

    strLog = $"""

    Start mesh {i}:
    dofs - {meshAfter.NumberOfDofs}
    elements - {meshAfter.Elements.Where(x => x.VertexNumber.Length != 2).Count()}

    """;

    log.WriteLine(strLog);
    Console.WriteLine(strLog);

    meshAfter = meshBefore.DoAdaptation(solutionAfter, materials, directory);

    problemAfter = new(materials, meshAfter);
    problemAfter.Prepare();
    solutionAfter = new(meshAfter, new TimeMesh([0.0]));
    problemAfter.Solve(solutionAfter);

    fileManager = new FileManager(Path.Combine(directory, "verticesAfterAddaptation.txt"),
                                  Path.Combine(directory, "trianglesAfterAddaptation.txt"),
                                  Path.Combine(directory, "valuesAfterAddaptation.txt"));

    fileManager.LoadToFile(meshAfter.Vertex, meshAfter.Elements, solutionAfter.SolutionVector.ToArray());

    xFlowValues = new double[meshAfter.Vertex.Length];
    yFlowValues = new double[meshAfter.Vertex.Length];

    for (int k = 0; k < meshAfter.Vertex.Length; k++)
    {
        var flow = solutionAfter.Flow(materials, meshAfter.Vertex[k]);

        xFlowValues[k] = flow.X;
        yFlowValues[k] = flow.Y;
    }

    fileManager.LoadValuesToFile(xFlowValues, Path.Combine(directory, "dxValuesAfterAdaptation.txt"));
    fileManager.LoadValuesToFile(yFlowValues, Path.Combine(directory, "dyValuesAfterAdaptation.txt"));

    strLog = $"""
    Adapted mesh {i}:
    dofs - {meshAfter.NumberOfDofs}
    elements - {meshAfter.Elements.Where(x => x.VertexNumber.Length != 2).Count()}

    """;

    log.WriteLine(strLog);
    Console.WriteLine(strLog);

    fileManager.LoadMyMeshFormat(Path.Combine(directory, "adaptedMesh.txt"), meshAfter);

    shiftAdaptedFromGoalSolution = 0.0;
    shiftAdaptedFromStartSolution = 0.0;
    shiftStartFromGoalSolution = 0.0;

    for (int s = 0; s < phi.Length; s++)
    {
        double cosPhi = Math.Cos(phi[s]);
        double sinPhi = Math.Sin(phi[s]);

        for (int p = 0; p < r.Length; p++)
        {
            Vector2D point = new(r[p] * cosPhi, r[p] * sinPhi);

            double valueStartMesh = solutionBefore.Value(point);
            double valueAdaptedMesh = solutionAfter.Value(point);
            double valueGoalSolution = goalSolution.Value(point);

            shiftAdaptedFromGoalSolution += (valueAdaptedMesh - valueGoalSolution) * (valueAdaptedMesh - valueGoalSolution);
            shiftAdaptedFromStartSolution += (valueAdaptedMesh - valueStartMesh) * (valueAdaptedMesh - valueStartMesh);
            shiftStartFromGoalSolution += (valueGoalSolution - valueStartMesh) * (valueGoalSolution - valueStartMesh);
        }
    }

    strLog = $"""
        Shift start solution from goal solution - {shiftStartFromGoalSolution}
        Shift adapted solution from goal solution - {shiftAdaptedFromGoalSolution}
        Shift adapted solution from start solution - {shiftAdaptedFromStartSolution}

        """;

    log.WriteLine(strLog);

    log.Close();
}

manager.CopyDirectory("Output", "..\\..\\..\\Output");

// END NEW CYCLE ADAPTATION
// ******************************************************************************************************************************************************

/*
string flag = "yes";

double x0 = 0.1;
double x1 = 2.9;
double y0 = 0.1;
double y1 = 2.9;

int sizeX = 100;
int sizeY = 100;

double hx = (x1 - x0) / sizeX;
double hy = (y1 - y0) / sizeY;


var xM = new double[sizeX + 1];
var yM = new double[sizeY + 1];
var points = new Vector2D[(sizeY + 1) * (sizeX + 1)];

xM[0] = x0;
for (int i = 0; i < sizeX; i++)
    xM[i] = x0 + i * hx;
xM[^1] = x1;

yM[0] = y0;
for (int i = 0; i < sizeY; i++)
    yM[i] = y0 + i * hy;
yM[^1] = y1;

for (int i = 0; i < sizeY + 1; i++)
{
    for (int j = 0; j < sizeX + 1; j++)
    {
        var num = i * (sizeX + 1) + j;

        points[num] = new(xM[j], yM[i]);
    }
}

var errBeforeAddaptation = 0.0;
var errAfterAddaptation = 0.0;
var normRealSolution = 0.0;

var difference = 0.0;
var normBeforeAdaptation = 0.0;

*//*for (int i = 0; i < points.Length; i++)
{
    var valueBeforeAddaptation = solution.Value(points[i]);
    var valueAfterAddaptation = addaptedSolution.Value(points[i]);

    difference = (valueAfterAddaptation - valueBeforeAddaptation) * (valueAfterAddaptation - valueBeforeAddaptation);
    normBeforeAdaptation += valueBeforeAddaptation * valueBeforeAddaptation;

    if (RealFunc != null)
    {
        var realValue = RealFunc(points[i], solution.Time);
        normRealSolution += realValue * realValue;

        errBeforeAddaptation += (valueBeforeAddaptation - realValue) * (valueBeforeAddaptation - realValue);
        errAfterAddaptation += (valueAfterAddaptation - realValue) * (valueAfterAddaptation - realValue);
    }
}*//*

if (RealFunc != null)
{
    Console.WriteLine($"Относительная погрешность решения ||uReal - uNumeric|| / ||uReal|| до адаптации: {Math.Sqrt(errBeforeAddaptation / normRealSolution):e3}");
    Console.WriteLine($"Относительная погрешность решения ||uReal - uNumeric|| / ||uReal|| после адаптации: {Math.Sqrt(errAfterAddaptation / normRealSolution):e3}");
}
Console.WriteLine($"Относительная разница решений ||uBefore - uAfter|| / ||uBefore|| до и после адаптации: {Math.Sqrt(difference / normBeforeAdaptation):e3}\n");


Console.WriteLine($"""
    Overwrite or append last strings to file?
    <1> - Overwrite to file.
    <2> - Append to file.
    """);

var append = (int.Parse(Console.ReadLine()!) - 1) == 1;

Console.WriteLine(append switch
{
    false => "Select <1> - Overwrite to file.",
    true => "Select <2> - Append to file."
});

using (StreamWriter writer = new(Path.Combine(output, "differenceSolution.txt"), append))
{
    if (RealFunc != null)
    {
        writer.WriteLine($"Относительная погрешность решения ||uReal - uNumeric|| / ||uReal|| до адаптации: {Math.Sqrt(errBeforeAddaptation / normRealSolution):e3}");
        writer.WriteLine($"Относительная погрешность решения ||uReal - uNumeric|| / ||uReal|| после адаптации: {Math.Sqrt(errAfterAddaptation / normRealSolution):e3}");
    }

    writer.WriteLine($"Относительная разница решений ||uBefore - uAfter|| / ||uBefore|| до и после адаптации: {Math.Sqrt(difference / normBeforeAdaptation):e3}\n");
}

fileManager.CopyDirectory(output, "..\\..\\..\\Output");
Console.WriteLine("Copy finished.");*/

// TODO:
// 1. делить на максимум нормы с ограничением, если меньше 10% от максимума, то не делить.
// 2. Возможно поможет добавка к весу значения правой части в середине ребра. Сначала попробовать без ограничения, потом после.

// TODO:
// Идейно: дробить начальную, а считать скачки по новым сеткам. По оптимизации пока не париться.

// TODO:
// Рефакторинг: идея создать класс адаптатора (мб и дискретизатора), экземпляр которого будет у сетки, по аналогии с решателями слау.
// Тогда по сути этот большой статический класс можно будет или вообще убрать, или большую часть вынести в вышеупомяннутые классы.

return 0;
