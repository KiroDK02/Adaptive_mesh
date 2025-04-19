using FEM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdaptiveGrids
{
    public class Rectangle
    {
        public Rectangle(int number, double x0, double x1, double y0, double y1, ISolution startSolution, ISolution newSolution)
        {
            Number = number;
            X0 = x0;
            X1 = x1;
            Y0 = y0;
            Y1 = y1;
            StartSolution = startSolution;
            NewSolution = newSolution;
        }
        public ISolution StartSolution { get; }
        public ISolution NewSolution { get; }
        public int Number { get; }
        public double X0 { get; }
        public double X1 { get; }
        public double Y0 { get; }
        public double Y1 { get; }
    }
}
