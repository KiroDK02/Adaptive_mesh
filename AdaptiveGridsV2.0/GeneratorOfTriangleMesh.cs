using AdaptiveGrids.FiniteElements1D;
using AdaptiveGrids.FiniteElements2D;
using FEM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelmaCore;

namespace AdaptiveGrids
{
    public class GeneratorOfTriangleMesh
    {
        public GeneratorOfTriangleMesh(double x0, double x1, double y0, double y1, int sizeX, int sizeY, double coefX = 1.0, double coefY = 1.0)
        {
            X0 = x0;
            X1 = x1;
            Y0 = y0;
            Y1 = y1;
            SizeX = sizeX;
            SizeY = sizeY;
            CoefX = coefX;
            CoefY = coefY;
        }

        public double X0 { get; }
        public double X1 { get; }
        public double Y0 { get; }
        public double Y1 { get; }
        public int SizeX { get; }
        public int SizeY { get; }
        public double CoefX { get; }
        public double CoefY { get; }

        public (IFiniteElement[] elems, Vector2D[] vert) Generate()
        {
            var elements = new List<IFiniteElement>();

            double hx = 0.0;
            double hy = 0.0;

            var x = new double[SizeX + 1];
            var y = new double[SizeY + 1];
            var vert = new Vector2D[(SizeX + 1) * (SizeY + 1)];

            x[0] = X0;
            if (CoefX != 1)
            {
                double sumProg = (Math.Pow(CoefX, SizeX) - 1.0) / (CoefX - 1.0);
                hx = (X1 - X0) / sumProg;

                for (int i = 1; i < SizeX; i++)
                    x[i] = X0 + hx * (Math.Pow(CoefX, i) - 1.0) / (CoefX - 1.0);
            }
            else
            {
                hx = (X1 - X0) / SizeX;

                for (int i = 1; i < SizeX; i++)
                    x[i] = X0 + i * hx;
            }
            x[^1] = X1;

            y[0] = Y0;
            if (CoefY != 1)
            {
                double sumProg = (Math.Pow(CoefY, SizeY) - 1.0) / (CoefY - 1.0);
                hy = (Y1 - Y0) / sumProg;

                for (int i = 1; i < SizeY; i++)
                    y[i] = Y0 + hy * (Math.Pow(CoefY, i) - 1.0) / (CoefY - 1.0);
            }
            else
            {
                hy = (Y1 - Y0) / SizeY;

                for (int i = 1; i < SizeY; i++)
                    y[i] = Y0 + i * hy;
            }
            y[^1] = Y1;

            for (int i = 0; i < SizeY; i++)
            {
                for (int j = 0; j < SizeX; j++)
                {
                    int nv1 = i * (SizeX + 1) + j;
                    int nv2 = i * (SizeX + 1) + j + 1;
                    int nv3 = (i + 1) * (SizeX + 1) + j;
                    int nv4 = (i + 1) * (SizeX + 1) + j + 1;

                    vert[nv1] = new Vector2D(x[j], y[i]);
                    vert[nv2] = new Vector2D(x[j + 1], y[i]);
                    vert[nv3] = new Vector2D(x[j], y[i + 1]);
                    vert[nv4] = new Vector2D(x[j + 1], y[i + 1]);

                    elements.Add(new TriangleFEQuadraticBaseWithNI("volume", [nv1, nv2, nv4]));
                    elements.Add(new TriangleFEQuadraticBaseWithNI("volume", [nv1, nv4, nv3]));
                }
            }

            for (int i = 0; i < SizeX; i++)
            {
                int nv1 = i;
                int nv2 = SizeY * (SizeX + 1) + i;

                elements.Add(new TriangleFEStraightQuadraticBaseWithNI("1", [nv1, nv1 + 1]));
                elements.Add(new TriangleFEStraightQuadraticBaseWithNI("3", [nv2, nv2 + 1]));
            }

            for (int i = 0; i < SizeY; i++)
            {
                int nv01 = i * (SizeX + 1);
                int nv11 = (i + 1) * (SizeX + 1);

                int nv02 = i * (SizeX + 1) + SizeX;
                int nv12 = (i + 1) * (SizeX + 1) + SizeX;

                elements.Add(new TriangleFEStraightQuadraticBaseWithNI("4", [nv01, nv11]));
                elements.Add(new TriangleFEStraightQuadraticBaseWithNI("2", [nv02, nv12]));
            }

            return (elements.ToArray(), vert);
        }
    }
}
