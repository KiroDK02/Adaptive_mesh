using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FEM;
using TelmaCore;

namespace AdaptiveGrids
{
   public class Solution : ISolution
   {
      public Solution(IFiniteElementMesh mesh, ITimeMesh timeMesh, string _path = "")
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
      public IFiniteElementMesh Mesh { get; }
      public ITimeMesh TimeMesh { get; }

      double[] solutionVector { get; }
      public ReadOnlySpan<double> SolutionVector => solutionVector;

      public IDictionary<(int i, int j), double> CalcDifferenceOfFlow(IDictionary<string, IMaterial> materials, IDictionary<(int i, int j), int> numberOccurrencesOfEdges)
      {
         var differenceFlow = new Dictionary<(int i, int j), double>();

         foreach (var element in Mesh.Elements)
         {
            if (element.VertexNumber.Length != 2)
            {
               var lambda = materials[element.Material].Lambda;

               for (int i = 0; i < element.NumberOfEdges; ++i)
               {
                  var edge = element.Edge(i);
                  edge = (element.VertexNumber[edge.i], element.VertexNumber[edge.j]);

                  var point1 = Mesh.Vertex[edge.i];
                  var point2 = Mesh.Vertex[edge.j];
                  var middleOfEdge = new Vector2D((point1.X + point2.X) / 2d, (point1.Y + point2.Y) / 2d);

                  var vector = new Vector2D(point2.X - point1.X, point2.Y - point1.Y);
                  var vectorOuterNormal = new Vector2D(vector.Y, -vector.X);
                  vectorOuterNormal.Normalize();

                  var valueGrad = Gradient(middleOfEdge);

                  var flowAcrossEdge = lambda(middleOfEdge) * vectorOuterNormal * valueGrad;

                  if (edge.i > edge.j) edge = (edge.j, edge.i);

                  if (numberOccurrencesOfEdges[edge] == 1)
                     differenceFlow.TryAdd(edge, 0);
                  else
                  {
                     if (differenceFlow.TryGetValue(edge, out var curFlow))
                        differenceFlow[edge] = Math.Abs(curFlow - flowAcrossEdge);
                     else differenceFlow.TryAdd(edge, flowAcrossEdge);
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

            if (midValue == target) return mid;
            else if (midValue < target) low = mid + 1;
            else high = mid - 1;
         }

         return mid;
      }
   }
}