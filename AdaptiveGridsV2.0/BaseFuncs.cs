using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelmaCore;

namespace AdaptiveGrids
{
   public static class BaseFuncs
   {
      public static Func<Vector2D, double>[] TriangleBarycentricLinearBase =
      {
         (Vector2D vert) => 1 - vert.X - vert.Y,
         (Vector2D vert) => vert.X,
         (Vector2D vert) => vert.Y
      };

      public static Func<Vector2D, double>[,] TriangleGradientBarycentricLinearBase =
      {
         {
            (Vector2D vert) => -1,
            (Vector2D vert) => -1
         },

         {
            (Vector2D vert) => 1,
            (Vector2D vert) => 0
         },

         {
            (Vector2D vert) => 0,
            (Vector2D vert) => 1
         },
      };

      public static Func<Vector2D, double>[] TriangleBarycentricQuadraticBase =
      {
         (Vector2D vert) => (1 - vert.X - vert.Y) * (2 * (1 - vert.X - vert.Y) - 1),
         (Vector2D vert) => vert.X * (2 * vert.X - 1),
         (Vector2D vert) => vert.Y * (2 * vert.Y - 1),
         (Vector2D vert) => 4 * (1 - vert.X - vert.Y) * vert.X,
         (Vector2D vert) => 4 * vert.X * vert.Y,
         (Vector2D vert) => 4 * (1 - vert.X - vert.Y) * vert.Y
      };

      public static Func<Vector2D, double>[,] TriangleGradientBarycentricQuadraticBase =
      {
         {
            (Vector2D vert) => -(2 * (1 - vert.X - vert.Y) - 1) - 2 * (1 - vert.X - vert.Y),
            (Vector2D vert) => -(2 * (1 - vert.X - vert.Y) - 1) - 2 * (1 - vert.X - vert.Y)
         },

         {
            (Vector2D vert) => (2 * vert.X - 1) + 2 * vert.X,
            (Vector2D vert) => 0
         },

         {
            (Vector2D vert) => 0,
            (Vector2D vert) => (2 * vert.Y - 1) + 2 * vert.Y
         },

         {
            (Vector2D vert) => 4 * (1 - 2 * vert.X - vert.Y),
            (Vector2D vert) => -4 * vert.X
         },

         {
            (Vector2D vert) => 4 * vert.Y,
            (Vector2D vert) => 4 * vert.X
         },

         {
            (Vector2D vert) => -4 * vert.Y,
            (Vector2D vert) => 4 * (1 - vert.X - 2 * vert.Y)
         }
      };

      public static Func<double, double>[] QuadraticBase =
      {
            x => 2 * (x - 1 / 2d) * (x - 1),
            x => 2 * x * (x - 1 / 2d),
            x => -4 * x * (x - 1)
      };
   }
}
