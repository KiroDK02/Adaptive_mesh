using FEM;
using TelmaCore;
using AdaptiveGrids;
using Quasar.Native;
using System.Runtime.CompilerServices;
using System.Xml.XPath;
using System.Reflection.PortableExecutable;
using Meshes;
using AdaptiveGrids.FiniteElements2D;
using AdaptiveGrids.FiniteElements1D;
using System.Reflection.Emit;

double[] t = { 0, 2, 4, 6, 8, 10 };

int minSplit = 8;
int layer = 3;

var countPassedVertices = (2 * (minSplit + 1) - (layer - 1)) * layer / 2;
var countPassedVerticesNextLayer = (2 * (minSplit + 1) - layer) * (layer + 1) / 2;
var elemi = 1;

(int localV1, int localV2, int localV3) =
   elemi % 2 == 0
? (elemi / 2 + countPassedVertices, elemi / 2 + countPassedVertices + 1, elemi / 2 + countPassedVerticesNextLayer)
: ((elemi + 1) / 2 + countPassedVertices, (elemi + 1) / 2 + countPassedVerticesNextLayer, (elemi + 1) / 2 + countPassedVerticesNextLayer - 1);

TimeMesh timeMesh = new TimeMesh(t);
/*Vector2D[] vertex = { new(0, 0), new(6, 0), new(3, 6) };*/
Vector2D[] vertex = { new(0, 0), new(6, 0), new(0, 6), new(6, 6) };
//Vector2D[] vertex = { new Vector2D(0, 6), new Vector2D(3, 6), new Vector2D(6, 6), 
//                      new Vector2D(0, 3), new Vector2D(3, 3), new Vector2D(6, 3), 
//                      new Vector2D(0, 0), new Vector2D(3, 0), new Vector2D(6, 0)};
IFiniteElement[] elements = { new TriangleFEQuadraticBaseWithNI("volume", new int[] { 0, 1, 3 }), new TriangleFEQuadraticBaseWithNI("volume", new int[] { 0, 3, 2 }),
                              new TriangleFEStraightQuadraticBaseWithNI("1", new int[] { 0, 1 }), new TriangleFEStraightQuadraticBaseWithNI("2", new int[] { 1, 3 }),
                              new TriangleFEStraightQuadraticBaseWithNI("3", new int[] { 3, 2 }), new TriangleFEStraightQuadraticBaseWithNI("4", new int[] { 2, 0 })};

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

materials.Add("volume", new Material(true, false, false, x => 2, x => 4, (x, t) => 0, (x, t) => 0, (x, t) => -12 * (x.X * x.Y)));
materials.Add("1", new Material(false, false, true, x => 2, x => 4, (x, t) => 0, (x, t) => x.X * x.X * x.X, (x, t) => -12 * (x.X * x.Y)));
materials.Add("2", new Material(false, true, false, x => 2, x => 4, (x, t) => 216, (x, t) => 216 + x.Y * x.Y * x.Y, (x, t) => -12 * (x.X * x.Y)));
materials.Add("3", new Material(false, true, false, x => 2, x => 4, (x, t) => 216, (x, t) => x.X * x.X * x.X + 216, (x, t) => -12 * (x.X * x.Y)));
materials.Add("4", new Material(false, true, false, x => 2, x => 4, (x, t) => 0, (x, t) => x.Y * x.Y * x.Y, (x, t) => -12 * (x.X * x.Y)));

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
ParabolicProblem problem = new ParabolicProblem(mesh, timeMesh, x => x.X * x.X * x.X + x.Y * x.Y * x.Y, materials);

problem.Prepare();
Solution solution = new Solution(mesh, timeMesh);
problem.Solve(solution);

//Func<Vector2D, double, double> RealFunc = (x, t) => x.X + x.Y + t;
//Func<Vector2D, double, double> RealFunc = (x, t) => x.X * x.X + x.Y * x.Y;
Func<Vector2D, double, double> RealFunc = (x, t) => x.X * x.X * x.X + x.Y * x.Y * x.Y;

//Func<Vector2D, Vector2D> RealGradientFunc = x => new Vector2D(1, 1);
//Func<Vector2D, Vector2D> RealGradientFunc = x => new Vector2D(2 * x.X, 2 * x.Y);
Func<Vector2D, double, Vector2D> RealGradientFunc = (x, t) => new Vector2D(3 * x.X * x.X, 3 * x.Y * x.Y);

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

solution.Time = 2.0;

var addaptedMesh = mesh.DoAdaptation(solution, materials);

var addaptedProblem = new ParabolicProblem(addaptedMesh, timeMesh, x => x.X * x.X * x.X + x.Y * x.Y * x.Y, materials);

addaptedProblem.Prepare();
var addaptedSolution = new Solution(addaptedMesh, timeMesh);
addaptedProblem.Solve(addaptedSolution);

addaptedSolution.Time = 2.0;

string flag = "yes";

while (flag != "no")
{
//   Console.WriteLine("Введите время: ");
//   double time = double.Parse(Console.ReadLine()!);
//
//   solution.Time = time;

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