using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FEM;
using TelmaCore;

namespace AdaptiveGrids
{
   public class FiniteElementMesh : IFiniteElementMesh
   {

      public FiniteElementMesh(IEnumerable<IFiniteElement> elements, Vector2D[] vertex)
      {
         Elements = elements;
         Vertex = vertex;
      }

      public IEnumerable<IFiniteElement> Elements { get; }

      public Vector2D[] Vertex { get; }

      public int NumberOfDofs { get; set; }
   }
}
