using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdaptiveGrids
{
    public class SubArea
    {
        public SubArea(int x0, int x1, int y0, int y1, string material)
        {
            X0 = x0;
            X1 = x1;
            Y0 = y0;
            Y1 = y1;
            Material = material;
        }

        public int X0 { get; }
        public int X1 { get; }
        public int Y0 { get; }
        public int Y1 { get; }
        public string Material { get; }
    }
}
