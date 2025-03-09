using FEM;
using TelmaCore;
using AdaptiveGrids;
using AdaptiveGrids.FiniteElements2D;
using AdaptiveGrids.FiniteElements1D;
System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

double[] t = { 0, 1 };

TimeMesh timeMesh = new TimeMesh(t);

//var generator = new GeneratorOfTriangleMesh(1.0, 9.0, 1.0, 9.0, 26, 26);

//(IFiniteElement[] elements, Vector2D[] vertex) = generator.Generate();

/*Vector2D[] vertex = { new(0, 0), new(6, 0), new(3, 6) };*/
//Vector2D[] vertex = { new(0, 0), new(6, 0), new(3, 3), new(0, 6), new(6, 6) };
//                        0          1            2          3            4           5           6           7
Vector2D[] vertex = { new(1, 1), new(5, 1), new(10, 5), new(10, 10), new(8, 12), new(5, 10), new(1, 10), new(3, 5.5) };
//Vector2D[] vertex = { new Vector2D(0, 6), new Vector2D(3, 6), new Vector2D(6, 6), 
//                      new Vector2D(0, 3), new Vector2D(3, 3), new Vector2D(6, 3), 
//                      new Vector2D(0, 0), new Vector2D(3, 0), new Vector2D(6, 0)};
/*IFiniteElement[] elements = { new TriangleFEQuadraticBaseWithNI("volume", new int[] { 0, 1, 2 }), new TriangleFEQuadraticBaseWithNI("volume", new int[] { 1, 4, 2 }),
                              new TriangleFEQuadraticBaseWithNI("volume", new int[] { 2, 4, 3 }), new TriangleFEQuadraticBaseWithNI("volume", new int[] { 0, 2, 3 }),
                              new TriangleFEStraightQuadraticBaseWithNI("1", new int[] { 0, 1 }), new TriangleFEStraightQuadraticBaseWithNI("2", new int[] { 1, 4 }),
                              new TriangleFEStraightQuadraticBaseWithNI("3", new int[] { 4, 3 }), new TriangleFEStraightQuadraticBaseWithNI("4", new int[] { 3, 0 })};*/

IFiniteElement[] elements = { new TriangleFEQuadraticBaseWithNI("volume", new int[] { 0, 1, 7 }), new TriangleFEQuadraticBaseWithNI("volume", new int[] { 0, 7, 6 }),
                              new TriangleFEQuadraticBaseWithNI("volume", new int[] { 1, 5, 7 }), new TriangleFEQuadraticBaseWithNI("volume", new int[] { 7, 5, 6 }),
                              new TriangleFEQuadraticBaseWithNI("volume", new int[] { 1, 2, 5 }), new TriangleFEQuadraticBaseWithNI("volume", new int[] { 2, 3, 5 }),
                              new TriangleFEQuadraticBaseWithNI("volume", new int[] { 5, 3, 4 }), new TriangleFEQuadraticBaseWithNI("volume", new int[] { 5, 4, 6 }),
                              new TriangleFEStraightQuadraticBaseWithNI("2", new int[] { 0, 1 }), new TriangleFEStraightQuadraticBaseWithNI("4", new int[] { 1, 2 }),
                              new TriangleFEStraightQuadraticBaseWithNI("3", new int[] { 2, 3 }), new TriangleFEStraightQuadraticBaseWithNI("4", new int[] { 3, 4 }),
                              new TriangleFEStraightQuadraticBaseWithNI("4", new int[] { 4, 6 }), new TriangleFEStraightQuadraticBaseWithNI("1", new int[] { 6, 0 })};

/*IFiniteElement[] elements = { new TriangleFELinearBase("volume", new int[] { 0, 1, 3 }), new TriangleFELinearBase("volume", new int[] { 1, 2, 3 }),
                              new TriangleFEStraghtLinearBase("1", new int[] { 0, 1 }), new TriangleFEStraghtLinearBase("2", new int[] { 1, 3 }),
                              new TriangleFEStraghtLinearBase("3", new int[] { 2, 3 }), new TriangleFEStraghtLinearBase("4", new int[] { 0, 2 })};*/

/*IFiniteElement[] elements = { new TriangleFELinearBase("volum5e", new int[] { 0, 1, 2 }),
                              new TriangleFEStraghtLinearBase("1", new int[] { 0, 1 }), new TriangleFEStraghtLinearBase("2", new int[] { 1, 2 }),
                              new TriangleFEStraghtLinearBase("3", new int[] { 0, 2 })};*/

IAdaptiveFiniteElementMesh mesh = new FiniteElementMesh(elements, vertex);

IDictionary<string, IMaterial> materials = new Dictionary<string, IMaterial>();
/*materials.Add("volume", new Material(true, false, false, x => 1, x => 0, (x, t) => 0, (x, t) => 0, (x, t) => -4));
materials.Add("1", new Material(false, true, false, x => 1, x => 0, (x, t) => 0, (x, t) => 1 + x.Y * x.Y, (x, t) => 4));
materials.Add("2", new Material(false, false, true, x => 1, x => 0, (x, t) => -2, (x, t) => 0, (x, t) => 4));
materials.Add("3", new Material(false, true, false, x => 1, x => 0, (x, t) => 0, (x, t) => 100 + x.Y * x.Y, (x, t) => 4));
materials.Add("4", new Material(false, true, false, x => 1, x => 0, (x, t) => 0, (x, t) => x.X * x.X + x.Y * x.Y, (x, t) => 4));*/

/*materials.Add("volume", new Material(true, false, false, x => 1, x => 0, (x, t) => 0, (x, t) => 0, (x, t) => 0));
materials.Add("1", new Material(false, true, false, x => 1, x => 0, (x, t) => 0, (x, t) => 1 + x.Y, (x, t) => -4));
materials.Add("2", new Material(false, false, true, x => 1, x => 0, (x, t) => -1, (x, t) => 216 + x.Y * x.Y * x.Y, (x, t) => -4));
materials.Add("3", new Material(false, true, false, x => 1, x => 0, (x, t) => 216, (x, t) => 10 + x.Y, (x, t) => -4));
materials.Add("4", new Material(false, true, false, x => 1, x => 0, (x, t) => 0, (x, t) => x.X + x.Y, (x, t) => -4));*/

/*materials.Add("volume", new Material(true, false, false, x => 1, x => 1, (x, t) => 0, (x, t) => 0, (x, t) => 2 * Math.Sin(x.X + x.Y)));
materials.Add("1", new Material(false, true, false, x => 1, x => 1, (x, t) => 0, (x, t) => Math.Sin(1 + x.Y), (x, t) => -6 * (x.X + x.Y)));
materials.Add("2", new Material(false, true, false, x => 1, x => 1, (x, t) => -Math.Cos(x.X + 1), (x, t) => Math.Sin(x.X + 1), (x, t) => -6 * (x.X + x.Y)));
materials.Add("3", new Material(false, true, false, x => 1, x => 1, (x, t) => 216, (x, t) => Math.Sin(10 + x.Y), (x, t) => -6 * (x.X + x.Y)));
materials.Add("4", new Material(false, true, false, x => 1, x => 1, (x, t) => 0, (x, t) => Math.Sin(x.X + x.Y), (x, t) => -6 * (x.X + x.Y)));*/

/*materials.Add("volume", new Material(true, false, false, x => 1, x => 1, (x, t) => 0, (x, t) => 0, (x, t) => 2 * Math.Sin(x.X + x.Y)));
materials.Add("1", new Material(false, true, false, x => 1, x => 1, (x, t) => 0, (x, t) => Math.Sin(x.X), (x, t) => -6 * (x.X + x.Y)));
materials.Add("2", new Material(false, true, false, x => 1, x => 1, (x, t) => -Math.Cos(x.X + 1), (x, t) => Math.Sin(Math.PI + x.Y), (x, t) => -6 * (x.X + x.Y)));
materials.Add("3", new Material(false, true, false, x => 1, x => 1, (x, t) => 216, (x, t) => Math.Sin(x.X + Math.PI), (x, t) => -6 * (x.X + x.Y)));
materials.Add("4", new Material(false, true, false, x => 1, x => 1, (x, t) => 0, (x, t) => Math.Sin(x.Y), (x, t) => -6 * (x.X + x.Y)));*/

/*materials.Add("volume", new Material(true, false, false, x => 1, x => 1, (x, t) => 0, (x, t) => 0, (x, t) => Math.Sin(x.X) + Math.Sin(x.Y)));
materials.Add("1", new Material(false, true, false, x => 1, x => 1, (x, t) => 0, (x, t) => Math.Sin(x.X), (x, t) => -6 * (x.X + x.Y)));
materials.Add("2", new Material(false, true, false, x => 1, x => 1, (x, t) => -Math.Cos(x.X + 1), (x, t) => Math.Sin(Math.PI) + Math.Sin(x.Y), (x, t) => -6 * (x.X + x.Y)));
materials.Add("3", new Material(false, true, false, x => 1, x => 1, (x, t) => 216, (x, t) => Math.Sin(x.X) + Math.Sin(Math.PI), (x, t) => -6 * (x.X + x.Y)));
materials.Add("4", new Material(false, true, false, x => 1, x => 1, (x, t) => 0, (x, t) => Math.Sin(x.Y), (x, t) => -6 * (x.X + x.Y)));*/

/*materials.Add("volume", new Material(true, false, false, x => 1, x => 1, (x, t) => 0, (x, t) => 0, (x, t) => Math.Cos(x.X) + Math.Cos(x.Y)));
materials.Add("1", new Material(false, true, false, x => 1, x => 1, (x, t) => 0, (x, t) => Math.Cos(x.X) + 1, (x, t) => 0));
materials.Add("2", new Material(false, true, false, x => 1, x => 1, (x, t) => 0, (x, t) => -1 + Math.Cos(x.Y), (x, t) => 0));
materials.Add("3", new Material(false, true, false, x => 1, x => 1, (x, t) => 0, (x, t) => Math.Cos(x.X) - 1, (x, t) => 0));
materials.Add("4", new Material(false, true, false, x => 1, x => 1, (x, t) => 0, (x, t) => 1 + Math.Cos(x.Y), (x, t) => 0));*/

materials.Add("volume", new Material(true, false, false, x => 1, x => 0, (x, t) => 0, (x, t) => 0, (x, t) => -6 * (x.X + x.Y)));
materials.Add("1", new Material(false, true, false, x => 1, x => 0, (x, t) => 0, (x, t) => 1 + x.Y * x.Y * x.Y, (x, t) => -6 * (x.X + x.Y)));
materials.Add("2", new Material(false, false, true, x => 1, x => 0, (x, t) => -3, (x, t) => 0, (x, t) => -6 * (x.X + x.Y)));
materials.Add("3", new Material(false, true, false, x => 1, x => 0, (x, t) => 216, (x, t) => 1000 + x.Y * x.Y * x.Y, (x, t) => -6 * (x.X + x.Y)));
materials.Add("4", new Material(false, true, false, x => 1, x => 0, (x, t) => 0, (x, t) => x.X * x.X * x.X + x.Y * x.Y * x.Y, (x, t) => -6 * (x.X + x.Y)));

/*materials.Add("volume", new Material(true, false, false, x => 2, x => 4, (x, t) => 0, (x, t) => 0, (x, t) => -12 * (x.X * x.Y)));
materials.Add("1", new Material(false, false, true, x => 2, x => 4, (x, t) => 0, (x, t) => x.X * x.X * x.X, (x, t) => -12 * (x.X * x.Y)));
materials.Add("2", new Material(false, true, false, x => 2, x => 4, (x, t) => 216, (x, t) => 216 + x.Y * x.Y * x.Y, (x, t) => -12 * (x.X * x.Y)));
materials.Add("3", new Material(false, true, false, x => 2, x => 4, (x, t) => 216, (x, t) => x.X * x.X * x.X + 216, (x, t) => -12 * (x.X * x.Y)));
materials.Add("4", new Material(false, true, false, x => 2, x => 4, (x, t) => 0, (x, t) => x.Y * x.Y * x.Y, (x, t) => -12 * (x.X * x.Y)));*/

/*materials.Add("volume", new Material(true, false, false, x => 2, x => 4, (x, t) => 0, (x, t) => 0, (x, t) => 4));
materials.Add("1", new Material(false, false, true, x => 2, x => 4, (x, t) => -2, (x, t) => x.X + t, (x, t) => 4));
materials.Add("2", new Material(false, true, false, x => 2, x => 4, (x, t) => 0, (x, t) => 6 + x.Y + t, (x, t) => 4));
materials.Add("3", new Material(false, true, false, x => 2, x => 4, (x, t) => 0, (x, t) => x.X + 6 + t, (x, t) => 4));
materials.Add("4", new Material(false, true, false, x => 2, x => 4, (x, t) => 0, (x, t) => x.Y + t, (x, t) => 4));*/

//ParabolicProblem problem = new ParabolicProblem(mesh, timeMesh, x => x.X + x.Y, materials);
//ParabolicProblem problem = new ParabolicProblem(mesh, timeMesh, x => x.X * x.X + x.Y * x.Y, materials);
//ParabolicProblem problem = new ParabolicProblem(mesh, timeMesh, x => x.X * x.X * x.X + x.Y * x.Y * x.Y, materials);

//Func<Vector2D, double> initCondition = (x) => x.X + x.Y;
//Func<Vector2D, double> initCondition = (x) => x.X * x.X + x.Y * x.Y;
Func<Vector2D, double> initCondition = (x) => x.X * x.X * x.X + x.Y * x.Y * x.Y;
//Func<Vector2D, double> initCondition = (x) => Math.Sin(x.X + x.Y);
//Func<Vector2D, double> initCondition = (x) => Math.Sin(x.X) + Math.Sin(x.Y);
//Func<Vector2D, double> initCondition = (x) => Math.Cos(x.X) + Math.Cos(x.Y);

ParabolicProblem problem = new ParabolicProblem(mesh, timeMesh, initCondition, materials);

problem.Prepare();
Solution solution = new Solution(mesh, timeMesh);
problem.Solve(solution);

//Func<Vector2D, double, double> RealFunc = (x, t) => x.X + x.Y;
//Func<Vector2D, double, double> RealFunc = (x, t) => x.X * x.X + x.Y * x.Y;
Func<Vector2D, double, double> RealFunc = (x, t) => x.X * x.X * x.X + x.Y * x.Y * x.Y;
//Func<Vector2D, double, double> RealFunc = (x, t) => Math.Exp(x.X);
//Func<Vector2D, double, double> RealFunc = (x, t) => Math.Sin(x.X + x.Y);
//Func<Vector2D, double, double> RealFunc = (x, t) => Math.Sin(x.X) + Math.Sin(x.Y);
//Func<Vector2D, double, double> RealFunc = (x, t) => Math.Cos(x.X) + Math.Cos(x.Y);

//Func<Vector2D, double, Vector2D> RealGradientFunc = (x, t) => new Vector2D(1, 1);
//Func<Vector2D, double, Vector2D> RealGradientFunc = (x, t) => new Vector2D(2 * x.X, 2 * x.Y);
Func<Vector2D, double, Vector2D> RealGradientFunc = (x, t) => new Vector2D(3 * x.X * x.X, 3 * x.Y * x.Y);
//Func<Vector2D, double, Vector2D> RealGradientFunc = (x, t) => new Vector2D(Math.Cos(x.X + x.Y), Math.Cos(x.X + x.Y));
//Func<Vector2D, double, Vector2D> RealGradientFunc = (x, t) => new Vector2D(-Math.Sin(x.X), -Math.Sin(x.Y));

solution.Time = 1.0;

var fileManager = new FileManager("verticesBeforeAddaptation.txt",
                                  "trianglesBeforeAddaptation.txt",
                                  "valuesBeforeAddaptation.txt");

fileManager.LoadToFile(vertex, elements, solution.SolutionVector.ToArray());

Console.WriteLine(mesh.NumberOfDofs);

var addaptedMesh = mesh.DoAdaptation(solution, materials);

var addaptedProblem = new ParabolicProblem(addaptedMesh, timeMesh, initCondition, materials);

addaptedProblem.Prepare();
var addaptedSolution = new Solution(addaptedMesh, timeMesh);
addaptedProblem.Solve(addaptedSolution);

addaptedSolution.Time = 1.0;

/*writerVertices = new StreamWriter("verticesAfterAddaptation.txt");

for (int i = 0; i < addaptedMesh.Vertex.Length; i++)
{
    writerVertices.WriteLine($"{addaptedMesh.Vertex[i].X} {addaptedMesh.Vertex[i].Y}");
}

writerVertices.Close();

writerTriangle = new StreamWriter("trianglesAfterAddaptation.txt");

foreach (var element in addaptedMesh.Elements)
{
    if (element.VertexNumber.Length != 2)
    {
        writerTriangle.WriteLine($"{element.VertexNumber[0]} {element.VertexNumber[1]} {element.VertexNumber[2]}");
    }
}

writerTriangle.Close();*/

fileManager = new FileManager("verticesAfterAddaptation.txt",
                              "trianglesAfterAddaptation.txt",
                              "valuesAfterAddaptation.txt");

fileManager.LoadToFile(addaptedMesh.Vertex, addaptedMesh.Elements, addaptedSolution.SolutionVector.ToArray());

Console.WriteLine(addaptedMesh.NumberOfDofs);

string flag = "yes";

double x0 = 4.1;
double x1 = 8.9;
double y0 = 5.1;
double y1 = 8.9;

int sizeX = 3000;
int sizeY = 3000;

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

for (int i = 0; i < points.Length; i++)
{
    var realValue = RealFunc(points[i], solution.Time);

    var valueBeforeAddaptation = solution.Value(points[i]);
    var valueAfterAddaptation = addaptedSolution.Value(points[i]);

    normRealSolution += realValue * realValue;

    errBeforeAddaptation += (valueBeforeAddaptation - realValue) * (valueBeforeAddaptation - realValue);
    errAfterAddaptation += (valueAfterAddaptation - realValue) * (valueAfterAddaptation - realValue);
}

Console.WriteLine($"Относительная погрешность решения ||uReal - uNumeric|| / ||uReal|| до адаптации: {Math.Sqrt(errBeforeAddaptation / normRealSolution)}");
Console.WriteLine($"Относительная погрешность решения ||uReal - uNumeric|| / ||uReal|| после адаптации: {Math.Sqrt(errAfterAddaptation / normRealSolution)}");

while (flag != "no")
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
//    Console.WriteLine($"Значение в точке ({x};{y}) численного решения после адаптации = " + addaptedSolution.Value(point));

    Console.WriteLine($"Значение градиента в точке ({x};{y}) реального решения = " + RealGradientFunc(point, solution.Time));
    Console.WriteLine($"Значение градиента в точке ({x};{y}) численного решения до адаптации = " + solution.Gradient(point));
//    Console.WriteLine($"Значение градиента в точке ({x};{y}) численного решения после адаптации = " + addaptedSolution.Gradient(point));

    Console.WriteLine("Хотите продолжить?");

    flag = Console.ReadLine()!;
}

// 1773 dofs - addaptive
// 0.005389952437832932

// 1681 dofs - uniform
// 0.003041484607276872
// 0.002863590074864738


// 2517 dofs - addaptive
// 0.0027808808181993628

// 2401 - uniform
// 0.0017131286988966666 

// 1961 dofs - addaptive 
// 0.12585888106981472

// 2025 dofs
// 0.061320402275378424

// 121 dofs - begin mesh, split to 4 by x and y
// 0.0217688908547131

// 2377 dofs - addaptive
// 0.00036421364694810474

// 2401 dofs - uniform
// 0.00022429445197742951

// 1089 dofs - begin mesh, split to 16 by x and y
// 0.0007560106325773837

// 43027 dofs - addaptive
// 1.206845527986386E-05

// 42849 dofs - uniform split
// 2.870676334549267E-06

// 289 dofs - begin mesh, split to 8 by x and y
// 0.0015009761732569776

// 10593 dofs - addaptive
// 1.81806315926021E-05

// 9409 dofs - uniform split
// 7.080744703655906E-06

// 289 dofs - begin mesh
// 0.00023045741973200148

// 2709 dofs - addaptive
// 2.8713111160430566E-05

// 2809 dofs
// 6.839302581450459E-06


// cosx + cosy
// 289 dofs - begin mesh
// 0.00037566887783794893

// 10637 dofs - addaptive
// 6.154514250740975E-06

// 10609 dofs
// 1.4588086325895405E-06

// x^3 + y^3
// 289 dofs - begin mesh
// 9.96976424920583E-05

// 2703 dofs - addaptive
// 3.737150248060685E-05

// 2809 dofs - uniform
// 2.872799012531717E-06

// TODO:
// 1. взвесить на максимум из двух потоков
// 2. взвесить на модуль разности значений в центрах
// элементов деленное на расстояние (по сути численная производная)
// 3. поэкспериментировать с параметрами адаптации (число интервалов шкалы, количество дроблений на интервале)
// 4. для курсовика можно рисовать решение в виде градации цветов в ху-координатах

return 0;
