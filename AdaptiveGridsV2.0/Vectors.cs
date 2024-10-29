
namespace TelmaCore
{
   public enum AngleMeasureUnits { amuRadians = 0, amuDegrees = 1 };

   public readonly struct Vector2D : IEquatable<Vector2D>
   {
      public static readonly Vector2D Zero = new Vector2D(0, 0);
      public static readonly Vector2D XAxis = new Vector2D(1, 0);
      public static readonly Vector2D YAxis = new Vector2D(0, 1);

      public double X { get; }
      public double Y { get; }
      public double[] AsArray() => new[] { X, Y };

      public Vector2D(double x, double y)
      {
         X = x;
         Y = y;
      }

      public void Deconstruct(out double x, out double y)
          => (x, y) = (X, Y);
      /// <summary>
      ///  из полярных координат в декартовы
      /// </summary>
      public Vector2D(double a, double b, AngleMeasureUnits measure) : this()
      {
         if (measure == AngleMeasureUnits.amuRadians)
         {
            X = a * Math.Cos(b);
            Y = a * Math.Sin(b);
         }
         else
         {
            double c = b * Math.PI / 180;
            X = a * Math.Cos(c);
            Y = a * Math.Sin(c);
         }
      }
      public Vector2D(ReadOnlySpan<double> arr)
      {
#if DEBUG
         if (arr.Length != 2) throw new ArgumentException("Array size error");
#endif
         X = arr[0];
         Y = arr[1];
      }
      public double this[int k]
      {
         get
         {
            return k switch
            {
               0 => X,
               1 => Y,
               _ => throw new Exception("get: Vector2D out of range"),
            };
         }
      }
      public static double Distance(Vector2D a, Vector2D b) => (a - b).Norm;

      public static double SqrDistance(Vector2D a, Vector2D b)
      {
         Vector2D diff = a - b;
         return diff * diff;
      }
      public double Distance(Vector2D b) => (this - b).Norm;

      public double SqrDistance(Vector2D b) => SqrDistance(this, b);

      public double Norm => Math.Sqrt(X * X + Y * Y);

      public Vector2D Normalize() => this / Norm;

      public override string ToString() => $"Vec({X}, {Y})";

      public override bool Equals(object? obj) => obj is Vector2D v && Equals(v);

      public override int GetHashCode() => HashCode.Combine(X, Y);

      public bool Equals(Vector2D a) => a.X == X && a.Y == Y;
      public static bool TryParse(string line, out Vector2D res)
      {
         double x, y;
         var words = line.Split(new[] { ' ', '\t', ',', '>', '<', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
         if (words[0] == "Vec")
         {
            if (words.Length != 3 || !double.TryParse(words[1], out x) || !double.TryParse(words[2], out y))
            {
               res = Zero;
               return false;
            }
            else { res = new Vector2D(x, y); return true; }
         }
         if (words.Length != 2 || !double.TryParse(words[0], out x) || !double.TryParse(words[1], out y))
         {
            res = Zero;
            return false;
         }
         else { res = new Vector2D(x, y); return true; }
      }

      public static Vector2D Parse(string line)
      {
         if (!TryParse(line, out Vector2D res))
            throw new FormatException("Can't parse Vector2D!");
         return res;
      }
      public Vector2D Round(int digits) => new Vector2D(Math.Round(X, digits), Math.Round(Y, digits));

      public static Vector2D Vec(double x, double y) => new Vector2D(x, y);
      #region Static operators

      public static Vector2D operator -(Vector2D a) => new Vector2D(-a.X, -a.Y);

      public static Vector2D operator +(Vector2D a, Vector2D b) => new Vector2D(a.X + b.X, a.Y + b.Y);

      public static Vector2D operator -(Vector2D a, Vector2D b) => new Vector2D(a.X - b.X, a.Y - b.Y);

      public static Vector2D operator /(Vector2D a, double v) => new Vector2D(a.X / v, a.Y / v);

      public static Vector2D operator *(Vector2D a, double v) => new Vector2D(a.X * v, a.Y * v);

      public static Vector2D operator *(double v, Vector2D a) => new Vector2D(v * a.X, v * a.Y);

      public static double operator *(Vector2D a, Vector2D b) => a.X * b.X + a.Y * b.Y;

      public static bool operator ==(Vector2D a, Vector2D b) => a.X == b.X && a.Y == b.Y;

      public static bool operator !=(Vector2D a, Vector2D b) => a.X != b.X || a.Y != b.Y;

      public static Vector2D Cross(Vector2D v1) => new Vector2D(v1.Y, -v1.X);

      public static double Mixed(Vector2D v1, Vector2D v2) => v1.Y * v2.X - v1.X * v2.Y;

      public static Vector2D Sum(Vector2D a, Vector2D b) => new Vector2D(a.X + b.X, a.Y + b.Y);

      #endregion
      #region EqualityComparer

      private class EqualityComparer : IEqualityComparer<Vector2D>
      {
         public int Digits { get; set; }

         public bool Equals(Vector2D v1, Vector2D v2)
         {
            return v1.Round(Digits) == v2.Round(Digits);
         }

         public int GetHashCode(Vector2D obj)
         {
            return obj.Round(Digits).GetHashCode();
         }
      }

      public static IEqualityComparer<Vector2D> CreateComparer(int digits = 7)
      {
         return new EqualityComparer { Digits = digits };
      }


      #endregion
   }



}
