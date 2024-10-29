using System.Collections;
using System.Reflection;
using System.Xml.Linq;
using FEM;
using TelmaCore;
using ElementConstructorFunction = System.Func<int[], string, FEM.IFiniteElement[]>;


namespace Meshes
{
   public enum ElementShapeEnum
   {
      TRIANGLE,
      RECTANGLE,
   }

   public enum CoordinateSystemEnum
   {
      CYLINDRICAL,
      CARTESIAN,
   };
   public enum BasisEnum
   {
      HIERARCHICHAL_BIQUADRATIC,
      HIERARCHICHAL_QUADRATIC,
      LAGRANGE_BILINEAR,
      LAGRANGE_BIQUADRARIC,
      LAGRANGE_BICUBIC,
      LINEAR
   };

   public class ElementConstructor
   {
      public string DisplayName { get; set; } = "";

      ElementShapeEnum elementShape;
      CoordinateSystemEnum coordinateSystem;
      BasisEnum basis;

      public ElementConstructorFunction constructorFunction;


      public ElementConstructor(string _display,
                                 ElementShapeEnum elementShape,
                                 CoordinateSystemEnum coordinateSystem,
                                 BasisEnum basis,
                                 ElementConstructorFunction _constructor)
      {
         DisplayName = _display;
         this.elementShape = elementShape;
         this.coordinateSystem = coordinateSystem;
         this.basis = basis;
         constructorFunction = _constructor;
      }
   }

   public class RefineParams
   {
      public List<int> splitCount = [];
      public List<double> stretchRatio = [];
   };


   public class RectMesh : IFiniteElementMesh
   {
      /* Координаты после разбития сетки */
      List<double> X;
      List<double> Y;

      /* Координаты до разбития / Координаты опорных точек */
      List<double> Xw;
      List<double> Yw;

      /* Массивы связи координат до разбития с координатами после */
      List<int> IXw;
      List<int> IYw;

      /* Границы */
      List<BoundaryCondition> _boundaries;
      Vector2D[] _vertices = [];
      List<Subdomain> _subdomains = [];
      List<IFiniteElement> _elements = [];
      ElementConstructorFunction _constructor;


      // координаты - координаты опорных точек, по которым определяются подобласти
      public struct Subdomain
      {
         public int x1;
         public int y1;
         public int x2;
         public int y2;
         public string material;
      }

      public struct BoundaryCondition
      {
         public int x1;
         public int y1;
         public int x2;
         public int y2;
         public string material;
      }

      /* Принимает на вход массивы с координатными линиями, список
          подобластей и список краевых условий. По умолчанию
          сетка не разбивается. При необходимости для этого вызывается
          метод Refine.
       */
      public RectMesh(List<double> _Xw, List<double> _Yw, List<Subdomain> _subdomains_in, List<BoundaryCondition> _boundaries_in, ElementConstructorFunction _constructor)
      {
         Xw = _Xw;
         Yw = _Yw;

         X = new(Xw);
         Y = new(Yw);

         this._constructor = _constructor;

         IXw = [];
         IYw = [];
         // так сетка ещё не разбита опорные координаты соотносятся 
         // 1к1 с координатами после разбития
         for (int i = 0; i < Xw.Count; i++)
         {
            IXw.Add(i);
         }
         for (int i = 0; i < Yw.Count; i++)
         {
            IYw.Add(i);
         }

         _boundaries = _boundaries_in;
         _subdomains = _subdomains_in;

         UpdateVertices();
         UpdateElements();

         FemAlgorithms.EnumerateMeshDofs(this);
      }

      /* Вычисление размера первого шага с учётом коэф. растяжения */
      double FirstStepSize(double stretch, int seg_count, double gap)
      {
         double sum;
         if (stretch != 1.0)
         {
            sum = (1.0 - Math.Pow(stretch, seg_count)) / (1.0 - stretch);
         }
         else
         {
            sum = seg_count;
         }

         return gap / sum;
      }

      public void Refine(RefineParams xAxis, RefineParams yAxis)
      {
         if (xAxis.splitCount.Count != xAxis.stretchRatio.Count)
         {
            throw new ArgumentException("Размеры массивов для разбиения" +
            " по оси X не совпадают");
         }
         if (yAxis.splitCount.Count != yAxis.stretchRatio.Count)
         {
            throw new ArgumentException("Размеры массивов для разбиения" +
            " по оси Y не совпадают");
         }

         if (xAxis.splitCount.Count != Xw.Count - 1 ||
             yAxis.splitCount.Count != Yw.Count - 1)
         {
            throw new ArgumentException("Неверное кол-во интервалов");
         }

         X.Clear();
         Y.Clear();
         IXw.Clear();
         IYw.Clear();

         /* Разбиение оси X */
         IXw.Add(X.Count);
         X.Add(Xw[0]);
         for (int i = 1; i < Xw.Count; i++)
         {
            double gap = Xw[i] - Xw[i - 1];
            int seg_count = xAxis.splitCount[i - 1];
            double stretch = xAxis.stretchRatio[i - 1];

            double step = FirstStepSize(stretch, seg_count, gap);
            double step_n = step;
            double stretch_n = stretch;
            int idx = X.Count - 1;
            for (int j = 0; j < seg_count - 1; j++)
            {
               X.Add(X[idx] + step_n);
               stretch_n *= stretch;
               if (stretch != 1.0)
               {
                  step_n = step * (stretch_n - 1.0) / (stretch - 1.0);
               }
               else
               {
                  step_n = step * (j + 2);
               }
            }
            IXw.Add(X.Count);
            X.Add(Xw[i]);
         }

         /* Разбиение оси Y */
         IYw.Add(Y.Count);
         Y.Add(Yw[0]);
         for (int i = 1; i < Yw.Count; i++)
         {
            double gap = Yw[i] - Yw[i - 1];
            int seg_count = yAxis.splitCount[i - 1];
            double stretch = yAxis.stretchRatio[i - 1];

            double step = FirstStepSize(stretch, seg_count, gap);
            for (int j = 0; j < seg_count - 1; j++)
            {
               Y.Add(Y.Last() + step);
               step *= stretch;
            }
            IYw.Add(Y.Count);
            Y.Add(Yw[i]);
         }

         UpdateVertices();
         UpdateElements();
      }

      // получение подобласти по координате конечного элемента
      // элемент определяется нижним левым узлом
      // принимаются координаты в нумерации после разбития, функция
      // позоботися о переводе их в нужную нумерацию
      // возвращает null если подобласть не была найдена
      Subdomain? GetSubdomainAtElemCoord(int ix, int iy)
      {
         foreach (var a in _subdomains)
         {
            if (ix >= IXw[a.x1] && ix < IXw[a.x2] &&
                iy >= IYw[a.y1] && iy < IYw[a.y2])
            {
               return a;
            }
         }
         return null;
      }

      // обновление списка узлов. вызывается при создании экземляра сетки
      // и после разбития
      void UpdateVertices()
      {
         var vertices = new List<Vector2D>();

         for (int iy = 0; iy < Y.Count; iy++)
         {
            for (int ix = 0; ix < X.Count; ix++)
            {
               var point = new Vector2D(X[ix], Y[iy]);
               vertices.Add(point);
            }
         }

         _vertices = vertices.ToArray();
      }

      void UpdateElements()
      {
         _elements = [];

         var Xn = X.Count;
         var Yn = Y.Count;
         // добавление обычные элементов
         for (int iy = 0; iy < Y.Count - 1; iy++)
         {
            for (int ix = 0; ix < X.Count - 1; ix++)
            {
               // первая и четвёртая вершины в обходе против часовой стрелки
               var v1 = iy * Xn + ix;
               var v4 = (iy + 1) * Xn + ix;
               var subdom = GetSubdomainAtElemCoord(ix, iy);
               if (!subdom.HasValue)
               {
                  continue;
               }
               var material = subdom.Value.material;

               // throw new NotImplementedException();
               // нужен конкретный класс конечного элемента
               // _elements.Add(new([v1, v1 + 1, v4 + 1, v4], material));
               _elements.AddRange(_constructor([v1, v1 + 1, v4 + 1, v4], material));
            }
         }
         // добавление элементов, отвечающих за краевые условия
         foreach (var el in _boundaries)
         {
            var ix1 = IXw[el.x1];
            var ix2 = IXw[el.x2];
            var iy1 = IYw[el.y1];
            var iy2 = IYw[el.y2];

            var v1 = iy1 * Xn + ix1;
            var v2 = iy2 * Xn + ix2;

            // throw new NotImplementedException();
            // нужен конкретный класс конечного элемента
            // _elements.Add(new([v1, v2], el.material));
            // для одномерных передавать снизу вверз или слева направо
            _elements.AddRange(_constructor([v1, v2], el.material));
         }

         FemAlgorithms.EnumerateMeshDofs(this);
      }


      public IEnumerable<IFiniteElement> Elements { get => _elements; }

      public Vector2D[] Vertex { get => _vertices; }

      public int NumberOfDofs { get; set; }

   }
}

