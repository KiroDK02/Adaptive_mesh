using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Matrices
{
    public static class TriangleLinearBaseMatrix
    {
        public static readonly double[,] M = { { 1d / 12d, 1d / 24d, 1d / 24d },
                                             { 1d / 24d, 1d / 12d, 1d / 24d },
                                             { 1d / 24d, 1d / 24d, 1d / 12d } };
    }

    public static class OneDimensionalLinearBaseMatrix
    {
        public static readonly double[,] G = { { 1, -1 },
                                             { -1, 1 } };

        public static readonly double[,] M = { { 1d / 3d, 1d / 6d },
                                             { 1d / 6d, 1d / 3d } };
    }
}
