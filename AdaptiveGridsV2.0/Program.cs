using FEM;
using TelmaCore;
using AdaptiveGrids;
using AdaptiveGrids.FiniteElements2D;
using AdaptiveGrids.FiniteElements1D;
using static FEM.IAdaptiveFiniteElementMesh;
using Quadratures;
using System.Xml.Linq;
System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

double[] t = { 0, 0.1 };

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

var generator = new GeneratorOfRectangleMesh([0.0, 3.0],
                                             [0.0, 3.0],
                                             [16],
                                             [16],
                                             [1.0],
                                             [1.0],
                                             [.. areas]);

(IFiniteElement[] elements, Vector2D[] vertex) = generator.GenerateToMesh();

Console.WriteLine(
    """
    Select type difference of Flow:
    <0> - Relative difference of flow.
    <1> - Absolute difference of flow.
    """);
var type = (TypeRelativeDifference)int.Parse(Console.ReadLine()!);

Console.WriteLine(type switch
{
    TypeRelativeDifference.Relative => "Selected <0> - Relative difference of flow.",
    TypeRelativeDifference.Absolute => "Selected <1> - Absolute difference of flow.",
    _ => throw new Exception("Invalid.")
});

IAdaptiveFiniteElementMesh mesh = new FiniteElementMesh(elements, vertex, type);
/*FileManager manager = new FileManager();
IAdaptiveFiniteElementMesh mesh = manager.ReadMeshFromTelma("Input\\mesh_telma.txt", type);*/

IDictionary<string, IMaterial> materials = new Dictionary<string, IMaterial>();

materials.Add("volume", new Material(true, false, false, x => 1, x => 0, (x, t) => 0, (x, t) => 0, (x, t) => -2.0 / 3.0 * x.X));
materials.Add("1", new Material(false, true, false, x => 1, x => 0, (x, t) => 0, (x, t) => x.X * x.X * x.X / 9.0, (x, t) => 0));
materials.Add("2", new Material(false, true, false, x => 1, x => 0, (x, t) => 0, (x, t) => 3.0, (x, t) => 0));
materials.Add("3", new Material(false, true, false, x => 1, x => 0, (x, t) => 0, (x, t) => x.X * x.X * x.X / 9.0, (x, t) => 0));
materials.Add("4", new Material(false, true, false, x => 1, x => 0, (x, t) => 0, (x, t) => 0.0, (x, t) => 0));

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

//Func<Vector2D, double> initCondition = (x) => x.X + x.Y;
//Func<Vector2D, double> initCondition = (x) => x.X * x.X + x.Y * x.Y;
Func<Vector2D, double> initCondition = (x) => x.X * x.X * x.X / 9.0;
//Func<Vector2D, double> initCondition = (x) => Math.Sin(x.X + x.Y);
//Func<Vector2D, double> initCondition = (x) => Math.Sin(x.X) * Math.Sin(x.Y);
//Func<Vector2D, double> initCondition = (x) => Math.Sin(x.X) + Math.Sin(x.Y);
//Func<Vector2D, double> initCondition = (x) => Math.Cos(x.X) + Math.Cos(x.Y);
//Func<Vector2D, double> initCondition = (x) => Math.Sin(x.X) + Math.Exp(-x.Y);
//Func<Vector2D, double> initCondition = (x) => Math.Exp(x.X + x.Y);

ParabolicProblem problem = new ParabolicProblem(mesh, timeMesh, initCondition, materials);

problem.Prepare();
Solution solution = new Solution(mesh, timeMesh);
problem.Solve(solution);

//Func<Vector2D, double, double> RealFunc = (x, t) => x.X + x.Y;
//Func<Vector2D, double, double> RealFunc = (x, t) => x.X * x.X + x.Y * x.Y;
Func<Vector2D, double, double> RealFunc = (x, t) => x.X * x.X * x.X / 9.0;
//Func<Vector2D, double, double> RealFunc = (x, t) => Math.Exp(x.X);
//Func<Vector2D, double, double> RealFunc = (x, t) => Math.Sin(x.X + x.Y);
//Func<Vector2D, double, double> RealFunc = (x, t) => Math.Sin(x.X) * Math.Sin(x.Y);
//Func<Vector2D, double, double> RealFunc = (x, t) => Math.Sin(x.X) + Math.Sin(x.Y);
//Func<Vector2D, double, double> RealFunc = (x, t) => Math.Cos(x.X) + Math.Cos(x.Y);
//Func<Vector2D, double, double> RealFunc = (x, t) => Math.Cos(x.X) + Math.Sin(x.Y);
//Func<Vector2D, double, double> RealFunc = (x, t) => Math.Sin(x.X) + Math.Exp(-x.Y);
//Func<Vector2D, double, double> RealFunc = (x, t) => Math.Exp(x.X + x.Y);

//Func<Vector2D, double, Vector2D> RealGradientFunc = (x, t) => new Vector2D(1, 1);
//Func<Vector2D, double, Vector2D> RealGradientFunc = (x, t) => new Vector2D(2 * x.X, 2 * x.Y);
Func<Vector2D, double, Vector2D> RealGradientFunc = (x, t) => new Vector2D(x.X * x.X / 3.0, 0);
//Func<Vector2D, double, Vector2D> RealGradientFunc = (x, t) => new Vector2D(Math.Cos(x.X + x.Y), Math.Cos(x.X + x.Y));
//Func<Vector2D, double, Vector2D> RealGradientFunc = (x, t) => new Vector2D(Math.Cos(x.X) * Math.Sin(x.Y), Math.Sin(x.X) * Math.Cos(x.Y));
//Func<Vector2D, double, Vector2D> RealGradientFunc = (x, t) => new Vector2D(-Math.Sin(x.X), -Math.Sin(x.Y));
//Func<Vector2D, double, Vector2D> RealGradientFunc = (x, t) => new Vector2D(-Math.Sin(x.X), Math.Cos(x.Y));
//Func<Vector2D, double, Vector2D> RealGradientFunc = (x, t) => new Vector2D(Math.Cos(x.X), -Math.Exp(-x.Y));
//Func<Vector2D, double, Vector2D> RealGradientFunc = (x, t) => new Vector2D(Math.Exp(x.X + x.Y), Math.Exp(x.X + x.Y));

solution.Time = 0.1;

string output = "Output";

Directory.CreateDirectory(output);

var fileManager = new FileManager(Path.Combine(output, "verticesBeforeAddaptation.txt"),
                                  Path.Combine(output, "trianglesBeforeAddaptation.txt"),
                                  Path.Combine(output, "valuesBeforeAddaptation.txt"));

fileManager.LoadToFile(vertex, elements, solution.SolutionVector.ToArray());

var dxValues = new double[mesh.Vertex.Length];
var dyValues = new double[mesh.Vertex.Length];

for (int i = 0; i < mesh.Vertex.Length; i++)
{
    var grad = solution.Gradient(mesh.Vertex[i]);

    dxValues[i] = grad.X;
    dyValues[i] = grad.Y;
}

fileManager.LoadValuesToFile(dxValues, Path.Combine(output, "dxValuesBeforeAdaptation.txt"));
fileManager.LoadValuesToFile(dyValues, Path.Combine(output, "dyValuesBeforeAdaptation.txt"));

Console.WriteLine($"dofs - {mesh.NumberOfDofs}");
Console.WriteLine($"elements - {mesh.Elements.Where(x => x.VertexNumber.Length != 2).Count()}");

var addaptedMesh = mesh.DoAdaptation(solution, materials);

var addaptedProblem = new ParabolicProblem(addaptedMesh, timeMesh, initCondition, materials);

addaptedProblem.Prepare();
var addaptedSolution = new Solution(addaptedMesh, timeMesh);
addaptedProblem.Solve(addaptedSolution);

addaptedSolution.Time = 0.1;

fileManager = new FileManager(Path.Combine(output, "verticesAfterAddaptation.txt"),
                              Path.Combine(output, "trianglesAfterAddaptation.txt"),
                              Path.Combine(output, "valuesAfterAddaptation.txt"));

fileManager.LoadToFile(addaptedMesh.Vertex, addaptedMesh.Elements, addaptedSolution.SolutionVector.ToArray());

Console.WriteLine($"dofs - {addaptedMesh.NumberOfDofs}");
Console.WriteLine($"elements - {addaptedMesh.Elements.Where(x => x.VertexNumber.Length != 2).Count()}");

dxValues = new double[addaptedMesh.Vertex.Length];
dyValues = new double[addaptedMesh.Vertex.Length];

for (int i = 0; i < addaptedMesh.Vertex.Length; i++)
{
    var grad = addaptedSolution.Gradient(addaptedMesh.Vertex[i]);

    dxValues[i] = grad.X;
    dyValues[i] = grad.Y;
}

fileManager.LoadValuesToFile(dxValues, Path.Combine(output, "dxValuesAfterAdaptation.txt"));
fileManager.LoadValuesToFile(dyValues, Path.Combine(output, "dyValuesAfterAdaptation.txt"));

string flag = "yes";

double x0 = 0.1;
double x1 = 2.9;
double y0 = 0.1;
double y1 = 2.9;

int sizeX = 1000;
int sizeY = 1000;

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

/*for (int i = 0; i < points.Length; i++)
{
    var realValue = RealFunc(points[i], solution.Time);

    var valueBeforeAddaptation = solution.Value(points[i]);
  //  var valueAfterAddaptation = addaptedSolution.Value(points[i]);

    normRealSolution += realValue * realValue;

    errBeforeAddaptation += (valueBeforeAddaptation - realValue) * (valueBeforeAddaptation - realValue);
  //  errAfterAddaptation += (valueAfterAddaptation - realValue) * (valueAfterAddaptation - realValue);
}*/

Console.WriteLine($"Относительная погрешность решения ||uReal - uNumeric|| / ||uReal|| до адаптации: {Math.Sqrt(errBeforeAddaptation / normRealSolution):e3}");
Console.WriteLine($"Относительная погрешность решения ||uReal - uNumeric|| / ||uReal|| после адаптации: {Math.Sqrt(errAfterAddaptation / normRealSolution):e3}");

fileManager.CopyDirectory(output, "..\\..\\..\\Output");

/*while (flag != "no")
{
    //    Console.WriteLine("Введите время: ");
    //    double time = double.Parse(Console.ReadLine()!);
    // 
    //    solution.Time = time;

    Console.WriteLine("Введите x: ");
    double x = double.Parse(Console.ReadLine()!);

    Console.WriteLine("Введите y: ");
    double y = double.Parse(Console.ReadLine()!);

    Vector2D point = new Vector2D(x, y);

    Console.WriteLine($"time = {solution.Time}");
    Console.WriteLine($"Значение в точке ({x};{y}) реального решения = " + RealFunc(point, solution.Time));
    Console.WriteLine($"Значение в точке ({x};{y}) численного решения до адаптации = " + solution.Value(point));
    Console.WriteLine($"Значение в точке ({x};{y}) численного решения после адаптации = " + addaptedSolution.Value(point));

    Console.WriteLine($"Значение градиента в точке ({x};{y}) реального решения = " + RealGradientFunc(point, solution.Time));
    Console.WriteLine($"Значение градиента в точке ({x};{y}) численного решения до адаптации = " + solution.Gradient(point));
    Console.WriteLine($"Значение градиента в точке ({x};{y}) численного решения после адаптации = " + addaptedSolution.Gradient(point));

    Console.WriteLine("Хотите продолжить?");

    flag = Console.ReadLine()!;
}*/

// cosx + cosy
// 289 dofs - begin mesh
// 3.760e-004

// 9885 dofs - addaptive
// 6.230e-006

// 10609 dofs
// 1.750e-006   


// x^3 + y^3
// 289 dofs - begin mesh
// 9.96976424920583E-05

// 2703 dofs - addaptive
// 3.737150248060685E-05

// 2809 dofs - uniform
// 2.872799012531717E-06

// TODO:
// 1. взвесить на максимум по модулю из двух потоков
// 2. взвесить на модуль разности значений в центрах
// элементов деленное на расстояние (по сути численная производная)
// 3. поэкспериментировать с параметрами адаптации (число интервалов шкалы, количество дроблений на интервале)
// 4. для курсовика можно рисовать решение в виде градации цветов в ху-координатах - completed.

return 0;
