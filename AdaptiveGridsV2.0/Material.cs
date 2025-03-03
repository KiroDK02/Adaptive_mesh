using FEM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelmaCore;

namespace AdaptiveGrids
{
    public class Material : IMaterial
    {
        public Material(bool isVolume, bool is1, bool is2, Func<Vector2D, double> lambda, Func<Vector2D, double> sigma, Func<Vector2D, double, double> theta, Func<Vector2D, double, double> ug, Func<Vector2D, double, double> f)
        {
            IsVolume = isVolume;
            Is1 = is1;
            Is2 = is2;
            Lambda = lambda;
            Sigma = sigma;
            Theta = theta;
            Ug = ug;
            F = f;
        }

        public bool IsVolume { get; }

        public bool Is1 { get; }

        public bool Is2 { get; }

        public Func<Vector2D, double> Lambda { get; }

        public Func<Vector2D, double> Sigma { get; }

        public Func<Vector2D, double, double> Theta { get; }

        public Func<Vector2D, double, double> Ug { get; }

        public Func<Vector2D, double, double> F { get; }
    }
}
