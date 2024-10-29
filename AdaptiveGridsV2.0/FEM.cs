using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEM
{
   public static class FemAlgorithms
   {
      public static Dictionary<(int, int), int> BuildEdgePortrait(IFiniteElementMesh mesh)
      {
         var dict = new Dictionary<(int, int), int>();
         foreach (var element in mesh.Elements)
         {
            for (int i = 0; i < element.NumberOfEdges; i++)
            {
               var edge = element.Edge(i);
               edge = (element.VertexNumber[edge.i], element.VertexNumber[edge.j]);
               if (edge.i < edge.j) edge = (edge.j, edge.i);
               var n = element.DOFOnEdge(i);
               if (!dict.TryGetValue(edge, out int c) || c > n) dict[edge] = n;
            }
         }
         return dict;
      }
      public static void EnumerateMeshDofs(IFiniteElementMesh mesh)
      {
         int dof = 0;
         int[] VertexDof = new int[mesh.Vertex.Length];
         for (int i = 0; i < VertexDof.Length; i++)
         {
            VertexDof[i] = dof++;
         }
         foreach (var element in mesh.Elements)
         {
            for (int i = 0; i < element.VertexNumber.Length; i++)
               element.SetVertexDOF(i, VertexDof[element.VertexNumber[i]]);
         }

         var edges = BuildEdgePortrait(mesh);
         edges = edges.ToDictionary(edgeinfo => edgeinfo.Key, edgeinfo => dof += edgeinfo.Value);

         foreach (var element in mesh.Elements)
         {
            for (int i = 0; i < element.NumberOfEdges; i++)
            {
               var edge = element.Edge(i);
               edge = (element.VertexNumber[edge.i], element.VertexNumber[edge.j]);
               if (edge.i < edge.j) edge = (edge.j, edge.i);
               var n = element.DOFOnEdge(i);
               var start = edges[edge] - n;
               for (int j = 0; j < n; j++)
                  element.SetEdgeDOF(i, j, start + j);
            }
         }

         foreach (var element in mesh.Elements)
            for (int i = 0; i < element.DOFOnElement(); i++)
               element.SetElementDOF(i, dof++);

         mesh.NumberOfDofs = dof;
      }
      public static SortedSet<int>[] BuildPortraitFirstStep(IFiniteElementMesh mesh)
      {
         var a = new SortedSet<int>[mesh.NumberOfDofs];
         for (int i = 0; i < mesh.NumberOfDofs; i++) a[i] = new();
         foreach (var element in mesh.Elements)
         {
            for (int i = 0; i < element.Dofs.Length; i++)
               for (int j = 0; j < element.Dofs.Length; j++)
                  a[element.Dofs[i]].Add(element.Dofs[j]);
         }
         return a;
      }
   }
}