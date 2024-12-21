using Quadratures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelmaCore;

namespace FEM
{
   public interface IFiniteElement
   {
      string Material { get; }
      enum MatrixType { Stiffness, Mass }
      int[] VertexNumber { get; } // в порядке левого обхода
      void SetVertexDOF(int vertex, int dof);
      int NumberOfEdges { get; }
      (int i, int j) Edge(int edge);
      int DOFOnEdge(int edge);

      void SetEdgeDOF(int edge, int n, int dof);
      int DOFOnElement();
      void SetElementDOF(int n, int dof);
      int[] Dofs { get; }
      double[,] BuildLocalMatrix(Vector2D[] VertexCoords, MatrixType type, Func<Vector2D, double> Coeff);                                                                                          // Как будем понимать, интегрируем или коэффициент раскладывается, если он не постоянный
      double[] BuildLocalRightPart(Vector2D[] VertexCoords, Func<Vector2D, double> F);

      double[] BuildLocalRightPartWithFirstBoundaryConditions(Vector2D[] VertexCoords, Func<Vector2D, double> Ug);
      double[] BuildLocalRightPartWithSecondBoundaryConditions(Vector2D[] VertexCoords, Func<Vector2D, double> Thetta);
      bool IsPointOnElement(Vector2D[] VertexCoords, Vector2D point);
      double GetValueAtPoint(Vector2D[] VertexCoords, ReadOnlySpan<double> coeffs, Vector2D point); // Получить значение в точке на конечном элементе
      Vector2D GetGradientAtPoint(Vector2D[] VertexCoords, ReadOnlySpan<double> coeffs, Vector2D point); // Получить градиент в точке на конечном элементе
   }

   public interface IFiniteElementWithNumericIntegration<T> : IFiniteElement
   {
      IMasterElement<T> MasterElement { get; }
   }

   public interface IMasterElement<T>
   {
      Func<T, double>[] BasesFuncs { get; }
      Func<T, double>[,] GradientsBasesFuncs { get; }
      double[,] ValuesBasicFuncs { get; }
      double[,,] ValuesBasicFuncsGradient { get; }
      QuadratureNodes<T> QuadratureNodes { get; }
      IDictionary<(int, int), double[]> PsiMultPsi { get; }
   }


   public interface IFiniteElementMesh
   {
      IEnumerable<IFiniteElement> Elements { get; }
      Vector2D[] Vertex { get; }
      int NumberOfDofs { get; set; }
   }

   public interface IAdaptiveFiniteElementMesh : IFiniteElementMesh
   {
      public bool Adapted { get; set; }
      public IAdaptiveFiniteElementMesh DoAdaptation(ISolution solution, IDictionary<string, IMaterial> materials);
   }

   public interface ITimeMesh
   {
      double this[int i] { get; } // значение времени по индексу в массиве времени
      int Size();
      double[] Coefs(int i); 
      void ChangeCoefs(double[] coefs); // добавляем только что насчитанные коэф., заменяя (j-2) на (j-1), а (j-1) на (j)
      bool IsChangedStep(int i); // смотрим, поменялся ли шаг по времени
      void DoubleMesh();
   }
   public interface IMaterial
   {
      bool IsVolume { get; }

      bool Is1 { get; }

      bool Is2 { get; }

      Func<Vector2D, double> Lambda { get; }

      Func<Vector2D, double> Sigma { get; }

      Func<Vector2D, double, double> Theta { get; }

      Func<Vector2D, double, double> Ug { get; }

      Func<Vector2D, double, double> F { get; }

   }

   public interface ISolution
   {
      double Time { get; set; }
      IFiniteElementMesh Mesh { get; }
      ITimeMesh TimeMesh { get; }
      ReadOnlySpan<double> SolutionVector { get; }
      IDictionary<(int i, int j), double> CalcDifferenceOfFlow(IDictionary<string, IMaterial> materials, IDictionary<(int i, int j), int> numberOccurrencesOfEdges);
      void AddSolutionVector(double t, double[] solution);
      double Value(Vector2D point);
      Vector2D Gradient(Vector2D point);
   }

   public interface IProblem
   {
      IDictionary<string, IMaterial> Materials { get; }
      void Prepare();
      void Solve(ISolution result);
   }

   public interface IMatrix
   {
      int N { get; }
      void SetProfile(SortedSet<int>[] profile);
      void AddLocal(int[] dofs, double[,] matrix, double coeff = 1d);
      void Symmetrize(int dof, double value, double[] RightPart);
      void Clear();
   }

   public interface ISLAE
   {
      IMatrix Matrix { get; }
      void AddLocalRightPart(int[] dofs, double[] lrp);
      void AddFirstBoundaryConditions(int[] dofs, double[] lrp);
      void Clear();
      void ClearRightPart();
      double[] RightPart { get; }
   }

   public interface ISLAESolver : IDisposable
   {
      ISLAE SLAE { get; }
      double[] Solve();
   }
}
