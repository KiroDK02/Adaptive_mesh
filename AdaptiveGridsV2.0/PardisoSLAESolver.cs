using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FEM;
using Quasar.Native;

namespace Core
{
   public class PardisoSLAESolver : ISLAESolver
   {
      public PardisoSLAESolver(PardisoSLAE slae)
      {
         SLAE = slae;
         Pardiso = new Pardiso<double>((IPardisoMatrix<double>)slae.Matrix);
      }

      public ISLAE SLAE { get; }
      Pardiso<double> Pardiso { get; }

      public void Prepare()
      {
         Pardiso.Analysis();
         Pardiso.Factorization();
      }

      public double[] Solve()
      {
         var solution = new double[SLAE.Matrix.N];

         Pardiso.Solve(SLAE.RightPart, solution);

         return solution;
      }

      private bool disposedValue;

      protected virtual void Dispose(bool disposing)
      {
         if (!disposedValue)
         {
            Pardiso.Dispose();

            disposedValue = true;
         }
      }

      // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
      ~PardisoSLAESolver()
      {
         // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
         Dispose(disposing: false);
      }

      public void Dispose()
      {
         // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
         Dispose(disposing: true);
         GC.SuppressFinalize(this);
      }
   }
}