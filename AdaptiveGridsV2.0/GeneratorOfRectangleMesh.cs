using AdaptiveGrids.FiniteElements1D;
using AdaptiveGrids.FiniteElements2D;
using FEM;
using TelmaCore;

namespace AdaptiveGrids
{
    public class GeneratorOfRectangleMesh
    {
        public GeneratorOfRectangleMesh(double[] x, double[] y, int[] splitsX, int[] splitsY, double[] coefsX, double[] coefsY, SubArea[] areas)
        {
            X = x;
            Y = y;
            SplitsX = splitsX;
            SplitsY = splitsY;
            CoefsX = coefsX;
            CoefsY = coefsY;
            Areas = areas;
        }

        public double[] X { get; }
        public double[] Y { get; }
        public int[] SplitsX { get; }
        public int[] SplitsY { get; }
        public double[] CoefsX { get; }
        public double[] CoefsY { get; }
        public SubArea[] Areas { get; }

        public (IFiniteElement[] elems, Vector2D[] vert) GenerateToMesh()
        {
            var elements = new List<IFiniteElement>();

            (double[] x, int[] shiftsX) = AlgorithmsOfGenerator.SplitToAxis(X, SplitsX, CoefsX);
            (double[] y, int[] shiftsY) = AlgorithmsOfGenerator.SplitToAxis(Y, SplitsY, CoefsY);

            var vertices = new Vector2D[x.Length * y.Length];

            int sizeX = x.Length - 1;
            int sizeY = y.Length - 1;

            for (int i = 0; i < sizeY; i++)
            {
                for (int j = 0; j < sizeX; j++)
                {
                    int nv1 = i * (sizeX + 1) + j;
                    int nv2 = i * (sizeX + 1) + j + 1;
                    int nv3 = (i + 1) * (sizeX + 1) + j;
                    int nv4 = (i + 1) * (sizeX + 1) + j + 1;

                    vertices[nv1] = new Vector2D(x[j], y[i]);
                    vertices[nv2] = new Vector2D(x[j + 1], y[i]);
                    vertices[nv3] = new Vector2D(x[j], y[i + 1]);
                    vertices[nv4] = new Vector2D(x[j + 1], y[i + 1]);

                    string material = GetMaterial(i, j, shiftsX, shiftsY);

                    elements.Add(new TriangleFEQuadraticBaseWithNI(material, [nv1, nv2, nv4]));
                    elements.Add(new TriangleFEQuadraticBaseWithNI(material, [nv1, nv4, nv3]));
                }
            }

            for (int i = 0; i < sizeX; i++)
            {
                int nv1 = i;
                int nv2 = sizeY * (sizeX + 1) + i;

                elements.Add(new TriangleFEStraightQuadraticBaseWithNI("1", [nv1, nv1 + 1]));
                elements.Add(new TriangleFEStraightQuadraticBaseWithNI("3", [nv2, nv2 + 1]));
            }

            for (int i = 0; i < sizeY; i++)
            {
                int nv01 = i * (sizeX + 1);
                int nv11 = (i + 1) * (sizeX + 1);

                int nv02 = i * (sizeX + 1) + sizeX;
                int nv12 = (i + 1) * (sizeX + 1) + sizeX;

                elements.Add(new TriangleFEStraightQuadraticBaseWithNI("4", [nv01, nv11]));
                elements.Add(new TriangleFEStraightQuadraticBaseWithNI("2", [nv02, nv12]));
            }

            return ([.. elements], vertices);
        }

        private string GetMaterial(int i, int j, int[] shiftsX, int[] shiftsY)
        {
            foreach (var area in Areas)
            {
                int j0 = shiftsX[area.X0];
                int j1 = shiftsX[area.X1];
                int i0 = shiftsY[area.Y0];
                int i1 = shiftsY[area.Y1];

                if (i0 <= i && (i + 1) <= i1 &&
                    j0 <= j && (j + 1) <= j1)
                    return area.Material;
            }

            return "";
        }
    }

    public static class AlgorithmsOfGenerator
    {
        public static (double[], int[]) SplitToAxis(double[] coords, int[] splits, double[] coefs)
        {
            int size = coords.Length;

            var shifts = new int[size];
            var newCoords = new double[splits.Sum() + 1];

            int nk = 0;

            newCoords[0] = coords[0];

            for (int i = 0, j = 1; i < size - 1; i++, j++)
            {
                int countIntervals = splits[i];
                double coef = coefs[i];

                nk += countIntervals;

                if (coef != 1)
                {
                    double sumProgression = (Math.Pow(coef, countIntervals) - 1.0) / (coef - 1.0);
                    double step = (coords[i + 1] - coords[i]) / sumProgression;

                    int jk = 1;
                    for (; j < nk; j++, jk++)
                        newCoords[j] = coords[i] + step * (Math.Pow(coef, jk) - 1.0) / (coef - 1.0);
                }
                else
                {
                    double step = (coords[i + 1] - coords[i]) / countIntervals;

                    int jk = 1;
                    for (; j < nk; j++, jk++)
                        newCoords[j] = coords[i] + step * jk;
                }

                newCoords[j] = coords[i + 1];
                shifts[i + 1] = j;
            }

            return (newCoords, shifts);
        }
    }
}
