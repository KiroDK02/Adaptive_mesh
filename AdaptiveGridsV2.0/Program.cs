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
IAdaptiveFiniteElementMesh quadMesh = manager.ReadMyMeshFormat("Input\\квадруполь_грубая_учетверенная.txt", type);

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
// Research
EllipticalProblem quadProblem = new(materials, quadMesh);

quadProblem.Prepare();
Solution goalSolution = new(quadMesh, new TimeMesh([0.0]));
quadProblem.Solve(goalSolution);

Research research = new(0.0, 0.4, 20,
                        0.0, 0.4, 20,
                        0.0, 0.05, 10,
                        0.0, Math.PI / 2.0, 10);

research.DoResearch(goalSolution, startMesh, materials);

manager.CopyDirectory("Output", "..\\..\\..\\Output");

// End research
// *********************************************************************

/*EllipticalProblem problem = new(materials, mesh);

problem.Prepare();
Solution solution = new Solution(mesh, new TimeMesh([0.0]));
problem.Solve(solution);

//solution.SolutionVector = [0.0, 1.0 / 9.0, 0.0, 1.0 / 9.0, 1.0 / 72.0, 1.0 / 9.0, 1.0 / 72.0, 1.0 / 72.0, 0];
*//*int k = 0;
foreach (var element in mesh.Elements)
{
    if (element.VertexNumber.Length == 2)
        continue;
    Vector2D edge = k % 2 == 0 ?
                    mesh.Vertex[element.VertexNumber[0]] - mesh.Vertex[element.VertexNumber[2]] :
                    mesh.Vertex[element.VertexNumber[1]] - mesh.Vertex[element.VertexNumber[0]];

    var vectorOuterNorm = new Vector2D(edge.Y, -edge.X) / edge.Norm;

    Vector2D center = k % 2 == 0 ?
                      (mesh.Vertex[element.VertexNumber[0]] + mesh.Vertex[element.VertexNumber[2]]) / 2.0 :
                      (mesh.Vertex[element.VertexNumber[1]] + mesh.Vertex[element.VertexNumber[0]]) / 2.0;
    k++;

    Console.WriteLine($"{vectorOuterNorm * element.GetGradientAtPoint(mesh.Vertex, solution.SolutionVector, center)}");
    //    Console.WriteLine($"{element.GetGradientAtPoint(mesh.Vertex, solution.SolutionVector, new(0.5, 0.5))}");
    //    Console.WriteLine($"{element.GetGradientAtPoint(mesh.Vertex, solution.SolutionVector, new(0.9, 0.9))}\n");
}*//*

//Func<Vector2D, double, double> RealFunc = (x, t) => x.X * x.X * x.X / 9.0;
//Func<Vector2D, double, double> RealFunc = (x, t) => Math.Sin(x.X + x.Y);
Func<Vector2D, double, double>? RealFunc = null;

Func<Vector2D, double, Vector2D> RealGradientFunc = (x, t) => new Vector2D(x.X * x.X / 3.0, 0);
//Func<Vector2D, double, Vector2D> RealGradientFunc = (x, t) => new Vector2D(Math.Cos(x.X + x.Y), Math.Cos(x.X + x.Y));

string output = "Output";

Directory.CreateDirectory(output);

var fileManager = new FileManager(Path.Combine(output, "verticesBeforeAddaptation.txt"),
                                  Path.Combine(output, "trianglesBeforeAddaptation.txt"),
                                  Path.Combine(output, "valuesBeforeAddaptation.txt"));

fileManager.LoadToFile(mesh.Vertex, mesh.Elements, solution.SolutionVector.ToArray());

var xFlowValues = new double[mesh.Vertex.Length];
var yFlowValues = new double[mesh.Vertex.Length];

for (int i = 0; i < mesh.Vertex.Length; i++)
{
    var flow = solution.Flow(materials, mesh.Vertex[i]);

    xFlowValues[i] = flow.X;
    yFlowValues[i] = flow.Y;
}

fileManager.LoadValuesToFile(xFlowValues, Path.Combine(output, "dxValuesBeforeAdaptation.txt"));
fileManager.LoadValuesToFile(yFlowValues, Path.Combine(output, "dyValuesBeforeAdaptation.txt"));

Console.WriteLine($"""

    Base mesh:
    dofs - {mesh.NumberOfDofs}
    elements - {mesh.Elements.Where(x => x.VertexNumber.Length != 2).Count()}

    """);

var addaptedMesh = mesh.DoAdaptation(solution, materials);

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

return 0;
