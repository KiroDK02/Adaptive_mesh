using FEM;
using TelmaCore;
using AdaptiveGrids;
using AdaptiveGrids.FiniteElements2D;
using AdaptiveGrids.FiniteElements1D;
System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

double[] t = { 0, 1 };

TimeMesh timeMesh = new TimeMesh(t);
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
/*materials.Add("volume", new Material(true, false, false, x => 2, x => 4, (x, t) => 0, (x, t) => 0, (x, t) => -4));
materials.Add("1", new Material(false, false, true, x => 2, x => 4, (x, t) => 0, (x, t) => x.X + t, (x, t) => 4));
materials.Add("2", new Material(false, true, false, x => 2, x => 4, (x, t) => 0, (x, t) => 36 + x.Y * x.Y + t, (x, t) => 4));
materials.Add("3", new Material(false, true, false, x => 2, x => 4, (x, t) => 0, (x, t) => x.X * x.X + 36 + t, (x, t) => 4));
materials.Add("4", new Material(false, true, false, x => 2, x => 4, (x, t) => 0, (x, t) => x.Y * x.Y + t, (x, t) => 4));*/

/*materials.Add("volume", new Material(true, false, false, x => 2, x => 4, (x, t) => 0, (x, t) => 0, (x, t) => -8));
materials.Add("1", new Material(false, false, true, x => 2, x => 4, (x, t) => 0, (x, t) => x.X * x.X, (x, t) => -8));
materials.Add("2", new Material(false, true, false, x => 2, x => 4, (x, t) => 24, (x, t) => 36 + x.Y * x.Y, (x, t) => -8));
materials.Add("3", new Material(false, true, false, x => 2, x => 4, (x, t) => 24, (x, t) => x.X * x.X + 36, (x, t) => -8));
materials.Add("4", new Material(false, true, false, x => 2, x => 4, (x, t) => 0, (x, t) => x.Y * x.Y, (x, t) => -8));*/

/*materials.Add("volume", new Material(true, false, false, x => 1, x => 1, (x, t) => 0, (x, t) => 0, (x, t) => -4));
materials.Add("1", new Material(false, true, false, x => 1, x => 1, (x, t) => 0, (x, t) => 1 + x.Y * x.Y, (x, t) => -4));
materials.Add("2", new Material(false, false, true, x => 1, x => 1, (x, t) => -2, (x, t) => 216 + x.Y * x.Y * x.Y, (x, t) => -4));
materials.Add("3", new Material(false, true, false, x => 1, x => 1, (x, t) => 216, (x, t) => 100 + x.Y * x.Y, (x, t) => -4));
materials.Add("4", new Material(false, true, false, x => 1, x => 1, (x, t) => 0, (x, t) => x.X * x.X + x.Y * x.Y, (x, t) => -4));*/

materials.Add("volume", new Material(true, false, false, x => 1, x => 1, (x, t) => 0, (x, t) => 0, (x, t) => 2 * Math.Sin(x.X + x.Y)));
materials.Add("1", new Material(false, true, false, x => 1, x => 1, (x, t) => 0, (x, t) => Math.Sin(1 + x.Y), (x, t) => -6 * (x.X + x.Y)));
materials.Add("2", new Material(false, true, false, x => 1, x => 1, (x, t) => -Math.Cos(x.X + 1), (x, t) => Math.Sin(x.X + 1), (x, t) => -6 * (x.X + x.Y)));
materials.Add("3", new Material(false, true, false, x => 1, x => 1, (x, t) => 216, (x, t) => Math.Sin(10 + x.Y), (x, t) => -6 * (x.X + x.Y)));
materials.Add("4", new Material(false, true, false, x => 1, x => 1, (x, t) => 0, (x, t) => Math.Sin(x.X + x.Y), (x, t) => -6 * (x.X + x.Y)));

/*materials.Add("volume", new Material(true, false, false, x => 1, x => 1, (x, t) => 0, (x, t) => 0, (x, t) => 2 * Math.Sin(x.X) + Math.Sin(x.Y)));
materials.Add("1", new Material(false, true, false, x => 1, x => 1, (x, t) => 0, (x, t) => Math.Sin(1) + Math.Sin(x.Y), (x, t) => -6 * (x.X + x.Y)));
materials.Add("2", new Material(false, true, false, x => 1, x => 1, (x, t) => -Math.Cos(x.X + 1), (x, t) => Math.Sin(x.X) + Math.Sin(1), (x, t) => -6 * (x.X + x.Y)));
materials.Add("3", new Material(false, true, false, x => 1, x => 1, (x, t) => 216, (x, t) => Math.Sin(10) + Math.Sin(x.Y), (x, t) => -6 * (x.X + x.Y)));
materials.Add("4", new Material(false, true, false, x => 1, x => 1, (x, t) => 0, (x, t) => Math.Sin(x.X) + Math.Sin(x.Y), (x, t) => -6 * (x.X + x.Y)));*/

/*materials.Add("volume", new Material(true, false, false, x => 1, x => 1, (x, t) => 0, (x, t) => 0, (x, t) => -6 * (x.X + x.Y)));
materials.Add("1", new Material(false, true, false, x => 1, x => 1, (x, t) => 0, (x, t) => 1 + x.Y * x.Y * x.Y, (x, t) => -6 * (x.X + x.Y)));
materials.Add("2", new Material(false, false, true, x => 1, x => 1, (x, t) => -3, (x, t) => 216 + x.Y * x.Y * x.Y, (x, t) => -6 * (x.X + x.Y)));
materials.Add("3", new Material(false, true, false, x => 1, x => 1, (x, t) => 216, (x, t) => 1000 + x.Y * x.Y * x.Y, (x, t) => -6 * (x.X + x.Y)));
materials.Add("4", new Material(false, true, false, x => 1, x => 1, (x, t) => 0, (x, t) => x.X * x.X * x.X + x.Y * x.Y * x.Y, (x, t) => -6 * (x.X + x.Y)));*/

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

////materials.Add("volume", new Material(true, false, false, x => 2, x => 4, (x, t) => 0, (x, t) => 0, (x, t) => 4));
////materials.Add("1", new Material(false, true, false, x => 2, x => 4, (x, t) => 0, (x, t) => x.Y + t, (x, t) => 4));
////materials.Add("2", new Material(false, true, false, x => 2, x => 4, (x, t) => 0, (x, t) => 7 * x.X + 6 + t, (x, t) => 4));
////materials.Add("3", new Material(false, false, true, x => 2, x => 4, (x, t) => (1 + x.Y) * 2, (x, t) => 7 * x.Y + 6 + t, (x, t) => 4));
////materials.Add("4", new Material(false, false, true, x => 2, x => 4, (x, t) => -2 * (1 + x.X), (x, t) => x.X + t, (x, t) => 4));

////materials.Add("volume", new Material(true, false, false, x => 2, x => 4, (x, t) => 0, (x, t) => 0, (x, t) => -2 * (6 * x.X + 6 * x.Y + 2 * x.Y * x.Y + 2 * x.X * x.X) + 4));
////materials.Add("1", new Material(false, true, false, x => 2, x => 4, (x, t) => 0, (x, t) => x.Y * x.Y * x.Y + t, (x, t) => 4));
////materials.Add("2", new Material(false, true, false, x => 2, x => 4, (x, t) => 0, (x, t) => x.X * x.X * x.X + 216 + 36 * x.X * x.X + t, (x, t) => 4));
////materials.Add("3", new Material(false, false, true, x => 2, x => 4, (x, t) => 2 * (108 + 12 * x.Y * x.Y), (x, t) => 7 * x.Y + 6 + t, (x, t) => 4));
////materials.Add("4", new Material(false, false, true, x => 2, x => 4, (x, t) => 0, (x, t) => x.X + t, (x, t) => 4));

//ParabolicProblem problem = new ParabolicProblem(mesh, timeMesh, x => x.X + x.Y, materials);
//ParabolicProblem problem = new ParabolicProblem(mesh, timeMesh, x => x.X * x.X + x.Y * x.Y, materials);
//ParabolicProblem problem = new ParabolicProblem(mesh, timeMesh, x => x.X * x.X * x.X + x.Y * x.Y * x.Y, materials);
ParabolicProblem problem = new ParabolicProblem(mesh, timeMesh, x => Math.Sin(x.X + x.Y), materials);

problem.Prepare();
Solution solution = new Solution(mesh, timeMesh);
problem.Solve(solution);

//Func<Vector2D, double, double> RealFunc = (x, t) => x.X + x.Y + t;
//Func<Vector2D, double, double> RealFunc = (x, t) => x.X * x.X + x.Y * x.Y;
//Func<Vector2D, double, double> RealFunc = (x, t) => x.X * x.X * x.X + x.Y * x.Y * x.Y;
//Func<Vector2D, double, double> RealFunc = (x, t) => Math.Exp(x.X);
Func<Vector2D, double, double> RealFunc = (x, t) => Math.Sin(x.X + x.Y);
//Func<Vector2D, double, double> RealFunc = (x, t) => Math.Sin(x.X) + Math.Sin(x.Y);

//Func<Vector2D, Vector2D> RealGradientFunc = x => new Vector2D(1, 1);
//Func<Vector2D, double, Vector2D> RealGradientFunc = (x, t) => new Vector2D(2 * x.X, 2 * x.Y);
//Func<Vector2D, double, Vector2D> RealGradientFunc = (x, t) => new Vector2D(3 * x.X * x.X, 3 * x.Y * x.Y);
Func<Vector2D, double, Vector2D> RealGradientFunc = (x, t) => new Vector2D(Math.Cos(x.X + x.Y), Math.Cos(x.X + x.Y));
//Func<Vector2D, double, Vector2D> RealGradientFunc = (x, t) => new Vector2D(Math.Cos(x.X), Math.Cos(x.Y));

//Vector2D[] vertexes = { new Vector2D(0, 6), new Vector2D(3, 6), new Vector2D(6, 6),
//                      new Vector2D(0, 3), new Vector2D(3, 3), new Vector2D(6, 3),
//                      new Vector2D(0, 0), new Vector2D(3, 0), new Vector2D(6, 0),
//                      new Vector2D(1, 0), new Vector2D(2, 0), new Vector2D(3, 1),
//                      new Vector2D(3, 2), new Vector2D(1, 3), new Vector2D(2, 3),
//                      new Vector2D(0, 1), new Vector2D(0, 2), new Vector2D(4, 0),
//                      new Vector2D(5, 0), new Vector2D(6, 1), new Vector2D(6, 2),
//                      new Vector2D(4, 3), new Vector2D(5, 3), new Vector2D(3, 4),
//                      new Vector2D(3, 5), new Vector2D(1, 6), new Vector2D(2, 6),
//                      new Vector2D(0, 4), new Vector2D(0, 5), new Vector2D(6, 4),
//                      new Vector2D(6, 5), new Vector2D(4, 6), new Vector2D(5, 6),
//                      new Vector2D(1, 1), new Vector2D(2, 1), new Vector2D(1, 2),
//                      new Vector2D(2, 2), new Vector2D(4, 1), new Vector2D(5, 1),
//                      new Vector2D(4, 2), new Vector2D(5, 2), new Vector2D(1, 4),
//                      new Vector2D(2, 4), new Vector2D(1, 5), new Vector2D(2, 5),
//                      new Vector2D(4, 4), new Vector2D(5, 4), new Vector2D(4, 5),
//                      new Vector2D(5, 5) };

////for (int i = 0; i < t.Length; ++i)
////{
////    Console.WriteLine("t = " + t[i].ToString());
////    foreach (var v in vertexes)
////        Console.WriteLine(RealFunc(v, t[i]));
////    Console.WriteLine();
////}

StreamWriter writerVertices = new StreamWriter("verticesBeforeAddaptation.txt");

for (int i = 0; i < mesh.Vertex.Length; i++)
{
    writerVertices.WriteLine($"{mesh.Vertex[i].X} {mesh.Vertex[i].Y}");
}

writerVertices.Close();

StreamWriter writerTriangle = new StreamWriter("trianglesBeforeAddaptation.txt");

foreach (var element in mesh.Elements)
{
    if (element.VertexNumber.Length != 2)
    {
        writerTriangle.WriteLine($"{element.VertexNumber[0]} {element.VertexNumber[1]} {element.VertexNumber[2]}");
    }

}

writerTriangle.Close();

solution.Time = 1.0;

var addaptedMesh = mesh.DoAdaptation(solution, materials);

var addaptedProblem = new ParabolicProblem(addaptedMesh, timeMesh, x => Math.Sin(x.X + x.Y), materials);

addaptedProblem.Prepare();
var addaptedSolution = new Solution(addaptedMesh, timeMesh);
addaptedProblem.Solve(addaptedSolution);

addaptedSolution.Time = 1.0;

writerVertices = new StreamWriter("verticesAfterAddaptation.txt");

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

writerTriangle.Close();

string flag = "yes";

var sizeX = (int)((8.0 - 4.0) / 0.0001);
var points = new Vector2D[3 * (sizeX + 1)];

var x0 = 4.0;
var y0 = 4.0;

var hx = 0.0001;
var hy = 2.0;

for (int i = 0; i < 3; i++)
{
    for (int j = 0; j < sizeX + 1; j++)
    {
        var num = i * (sizeX + 1) + j;

        points[num] = new(x0 + j * hx, y0 + i * hy);
    }
}
//err uniform split: 10.885540733491958
//err after addaptation: 10.102403825784894
var errBeforeAddaptation = 0.0;
var errAfterAddaptation = 0.0;

for (int i = 0; i < points.Length; i++)
{
    var realValue = RealFunc(points[i], solution.Time);

    var valueBeforeAddaptation = solution.Value(points[i]);
    var valueAfterAddaptation = addaptedSolution.Value(points[i]);

    errBeforeAddaptation += (valueBeforeAddaptation - realValue) * (valueBeforeAddaptation - realValue);
    errAfterAddaptation += (valueAfterAddaptation - realValue) * (valueAfterAddaptation - realValue);
}

Console.WriteLine($"err before addaptation: {Math.Sqrt(errBeforeAddaptation)}");
Console.WriteLine($"err after addaptation: {Math.Sqrt(errAfterAddaptation)}");

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
    Console.WriteLine($"Значение в точке ({x};{y}) численного решения после адаптации = " + addaptedSolution.Value(point));

    Console.WriteLine($"Значение градиента в точке ({x};{y}) реального решения = " + RealGradientFunc(point, solution.Time));
    Console.WriteLine($"Значение градиента в точке ({x};{y}) численного решения до адаптации = " + solution.Gradient(point));
    Console.WriteLine($"Значение градиента в точке ({x};{y}) численного решения после адаптации = " + addaptedSolution.Gradient(point));

    /*foreach (var element in mesh.Elements)
    {
       if (element.VertexNumber.Length != 2)
       {
          if (element.IsPointOnElement(mesh.Vertex, point))
          {
             Vector2D gradNumerical = element.GetGradientAtPoint(mesh.Vertex, solution.SolutionVector, point);
             Vector2D gradReal = RealGradientFunc(point, solution.Time);
             Console.WriteLine($"Градиент численного решения в точке ({x};{y}) = " + gradNumerical);
             Console.WriteLine($"Градиент реального решения в точке ({x};{y}) = " + gradReal);
          }
       }
    }*/

    Console.WriteLine("Хотите продолжить?");

    flag = Console.ReadLine()!;
}


//Console.WriteLine(Solution.BinarySearch(timeMesh, 0.5, 0, timeMesh.Size() - 1));
//Console.WriteLine(Solution.BinarySearch(timeMesh, 1.5, 0, timeMesh.Size() - 1));
//Console.WriteLine(Solution.BinarySearch(timeMesh, 9, 0, timeMesh.Size() - 1));
//Console.WriteLine(Solution.BinarySearch(timeMesh, 0, 0, timeMesh.Size() - 1));
//Console.WriteLine(Solution.BinarySearch(timeMesh, 5, 0, timeMesh.Size() - 1));
//Console.WriteLine(Solution.BinarySearch(timeMesh, 10, 0, timeMesh.Size() - 1));
//Console.WriteLine(Solution.BinarySearch(timeMesh, 8, 0, timeMesh.Size() - 1));

////Console.WriteLine("Hello, World!");

////var a = new SortedSet<int>[4];
////a[0] = new([0, 1, 2, 3]);
////a[1] = new([0, 1, 2, 3]);
////a[2] = new([0, 1, 2, 3]);
////a[3] = new([0, 1, 2, 3]);

////PardisoMatrix matrix = new PardisoMatrix(a, PardisoMatrixType.SymmetricIndefinite);

////double[,] LM = { { 10d, 1d, 1d, 1d }, { 1d, 10d, 2d, 2d }, { 1d, 2d, 10d, 3d }, { 1d, 2d, 3d, 10d } };
////int[] dofs = { 0, 1, 2, 3 };

////double[] x = new double[4];

////double[] b = { 19d, 35d, 47d, 54d };

////matrix.AddLocal(dofs, LM);

////Core.Pardiso<double> pardiso = new Core.Pardiso<double>(matrix);

////pardiso.Analysis();
////pardiso.Factorization();
////pardiso.Solve(b, x);

////for (int i = 0; i < 4; ++i)
////    Console.WriteLine(x[i]);

return 0;