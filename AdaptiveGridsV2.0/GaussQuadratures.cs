using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelmaCore;

namespace Quadratures
{
   public class QuadratureNode<T>
   {
      public QuadratureNode(T node, double weight)
      {
         Node = node;
         Weight = weight;
      }
      public T Node { get; }
      public double Weight { get; }

   }

   public class QuadratureNodes<T>
   {
      public QuadratureNodes(QuadratureNode<T>[] nodes, int order)
      {
         Nodes = nodes;
         Order = order;
      }
      public QuadratureNode<T>[] Nodes { get; }
      public int Order { get; }
   }

   public static class NumericalIntegration
   {
      public static IEnumerable<QuadratureNode<Vector2D>> GaussQuadratureTriangleOrder3()
      {
         Vector2D v1 = new(1.0 / 3.0, 1.0 / 3.0);
         Vector2D v2 = new(3.0 / 5.0, 1.0 / 5.0);
         Vector2D v3 = new(1.0 / 5.0, 3.0 / 5.0);
         Vector2D v4 = new(1.0 / 5.0, 1.0 / 5.0);
         double w1 = -9.0 / 32.0;
         double w2 = 25.0 / 96.0;
         Vector2D[] points = { v1, v2, v3, v4 };
         double[] weights = { w1, w2, w2, w2 };
         for (int i = 0; i < 4; i++)
            yield return new QuadratureNode<Vector2D>(points[i], weights[i]);
      }

      public static IEnumerable<QuadratureNode<Vector2D>> GaussQuadratureTriangleOrder5()
      {
         Vector2D v1 = new(1.0 / 3.0, 1.0 / 3.0);
         Vector2D v2 = new(0.059715871789770, 0.470142064105115);
         Vector2D v3 = new(0.470142064105115, 0.059715871789770);
         Vector2D v4 = new(0.470142064105115, 0.470142064105115);
         Vector2D v5 = new(0.797426985353087, 0.101286507323456);
         Vector2D v6 = new(0.101286507323456, 0.797426985353087);
         Vector2D v7 = new(0.101286507323456, 0.101286507323456);
         double w1 = 0.1125;
         double w2 = 0.066197076394253;
         double w3 = 0.0629695902724135;
         Vector2D[] points = { v1, v2, v3, v4, v5, v6, v7 };
         double[] weights = { w1, w2, w2, w2, w3, w3, w3 };
         for (int i = 0; i < 7; i++)
            yield return new QuadratureNode<Vector2D>(points[i], weights[i]);
      }

      public static IEnumerable<QuadratureNode<Vector2D>> GaussQuadratureTriangleOrder6()
      {
         Vector2D v1 = new(0.873821971016996, 0.063089014491502);
         Vector2D v2 = new(0.063089014491502, 0.873821971016996);
         Vector2D v3 = new(0.063089014491502, 0.063089014491502);
         Vector2D v4 = new(0.501426509658179, 0.249286745170910);
         Vector2D v5 = new(0.249286745170910, 0.501426509658179);
         Vector2D v6 = new(0.249286745170910, 0.249286745170910);
         Vector2D v7 = new(0.636502499121399, 0.310352451033785);
         Vector2D v8 = new(0.310352451033785, 0.636502499121399);
         Vector2D v9 = new(0.636502499121399, 0.053145049844816);
         Vector2D v10 = new(0.053145049844816, 0.636502499121399);
         Vector2D v11 = new(0.310352451033785, 0.053145049844816);
         Vector2D v12 = new(0.053145049844816, 0.310352451033785);
         double w1 = 0.0254224531851035;
         double w2 = 0.0583931378631895;
         double w3 = 0.041425537809187;
         Vector2D[] points = { v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12 };
         double[] weights = { w1, w1, w1, w2, w2, w2, w3, w3, w3, w3, w3, w3 };
         for (int i = 0; i < 12; i++)
            yield return new QuadratureNode<Vector2D>(points[i], weights[i]);
      }

      public static IEnumerable<QuadratureNode<double>> GaussQuadrature1DOrder3()
      {
         double[] points = { -1.0 / Math.Sqrt(3), 1.0 / Math.Sqrt(3) };
         double[] weights = { 1.0, 1.0 };


         for (int i = 0; i < 2; i++)
            yield return new QuadratureNode<double>((points[i] + 1) * 0.5, 0.5 * weights[i]);
      }

      public static IEnumerable<QuadratureNode<double>> GaussQuadrature1DOrder5()
      {
         double[] points = { -Math.Sqrt(0.6), 0.0, Math.Sqrt(0.6) };
         double[] weights = { 5.0 / 9.0, 8.0 / 9.0, 5.0 / 9.0 };

         for (int i = 0; i < 3; i++)
            yield return new QuadratureNode<double>((points[i] + 1) * 0.5, 0.5 * weights[i]);
      }

      public static IEnumerable<QuadratureNode<double>> GaussQuadrature1DOrder7()
      {
         double p1 = 0.339981043584856;
         double p2 = 0.861136311594052;
         double w1 = 0.652145154862546;
         double w2 = 0.347854845137453;
         double[] points = { -p1, p1, -p2, p2 };
         double[] weights = { w1, w1, w2, w2 };

         for (int i = 0; i < 4; i++)
            yield return new QuadratureNode<double>((points[i] + 1) * 0.5, 0.5 * weights[i]);
      }

      public static IEnumerable<QuadratureNode<double>> GaussQuadrature1DOrder9()
      {
         double p1 = 0.9061798459386640;
         double p2 = 0.5384693101056831;
         double w1 = 0.2369268850561891;
         double w2 = 0.4786286704993665;
         double w3 = 0.5688888888888889;
         double[] points = { -p1, -p2, 0.0, p2, p1 };
         double[] weights = { w1, w2, w3, w2, w1 };

         for (int i = 0; i < 5; i++)
            yield return new QuadratureNode<double>((points[i] + 1) * 0.5, 0.5 * weights[i]);
      }
   }
}
