using System;
using System.Collections.Generic;
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

         return 0; // что возвращать, если не попала ни в один элемент?
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

         return new Vector2D(0, 0);
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