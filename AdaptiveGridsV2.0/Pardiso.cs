using Quasar.Native;

namespace Core
{
    public class Pardiso<T> : IDisposable where T : unmanaged
    {
        private int _maxfct = 1;
        private int _mnum = 1;
        private int _nrhs = 1;
        private int _msglvl = 0;
        private int _type;
        private int _n;
        private IPardisoMatrix<T> _matrix;
        private IntPtr[] _pt = new IntPtr[64];
        private int[] _iparm = new int[64];
        private bool _disposed;

        public long MemorySize => Math.Max(this._iparm[14], this._iparm[15] + this._iparm[16]) * 1024L;

        private void ThrowIfError(int error)
        {
            switch (error)
            {
                case -12:
                    throw new Exception("Pardiso error: pardiso_64 called from 32-bit library");
                case -11:
                    throw new Exception("Pardiso error: read/write error with OOC files");
                case -10:
                    throw new Exception("Pardiso error: error opening OOC files");
                case -9:
                    throw new Exception("Pardiso error: not enough memory for OOC");
                case -8:
                    throw new Exception("Pardiso error: 32-bit integer overflow problem");
                case -7:
                    throw new Exception("Pardiso error: diagonal matrix is singular");
                case -6:
                    throw new Exception("Pardiso error: reordering failed");
                case -5:
                    throw new Exception("Pardiso error: unclassified (internal) error");
                case -4:
                    throw new Exception("Pardiso error: zero pivot, numerical factorization or iterative refinement problem");
                case -3:
                    throw new Exception("Pardiso error: reordering problem");
                case -2:
                    throw new Exception(string.Format("Pardiso error: not enough memory. Need {0} MB", this.MemorySize / 1024L / 1024L));
                case -1:
                    throw new Exception("Pardiso error: input inconsistent");
            }
        }

        public Pardiso(IPardisoMatrix<T> matrix)
        {
            this._matrix = matrix;
            this._n = matrix.n;
            this._type = (int)matrix.MatrixType;
            Pardiso.pardisoinit((Span<IntPtr>)this._pt, this._type, (Span<int>)this._iparm);
            this._iparm[34] = 1;
            this._iparm[8] = 10;
        }

        public void Analysis()
        {
            int error;
            Pardiso.pardiso((Span<IntPtr>)this._pt, this._maxfct, this._mnum, this._type, 11, this._n, this._matrix.a, this._matrix.ia, this._matrix.ja, (Span<int>)(int[])null!, this._nrhs, (Span<int>)this._iparm, this._msglvl, (ReadOnlySpan<T>)(T[])null!, (Span<T>)(T[])null!, out error);
            this.ThrowIfError(error);
        }

        public void Factorization()
        {
            int error;
            Pardiso.pardiso((Span<IntPtr>)this._pt, this._maxfct, this._mnum, this._type, 22, this._n, this._matrix.a, this._matrix.ia, this._matrix.ja, (Span<int>)(int[])null!, this._nrhs, (Span<int>)this._iparm, this._msglvl, (ReadOnlySpan<T>)(T[])null!, (Span<T>)(T[])null!, out error);
            this.ThrowIfError(error);
        }

        public void Solve(ReadOnlySpan<T> b, Span<T> x)
        {
            int error;
            Pardiso.pardiso((Span<IntPtr>)this._pt, this._maxfct, this._mnum, this._type, 33, this._n, this._matrix.a, this._matrix.ia, this._matrix.ja, (Span<int>)(int[])null!, this._nrhs, (Span<int>)this._iparm, this._msglvl, b, x, out error);
            this.ThrowIfError(error);
        }

        public void SolveT(ReadOnlySpan<T> b, Span<T> x)
        {
            int phase = 33;
            this._iparm[11] = 2;
            int error;
            Pardiso.pardiso((Span<IntPtr>)this._pt, this._maxfct, this._mnum, this._type, phase, this._n, this._matrix.a, this._matrix.ia, this._matrix.ja, (Span<int>)(int[])null!, this._nrhs, (Span<int>)this._iparm, this._msglvl, b, x, out error);
            this._iparm[11] = 0;
            this.ThrowIfError(error);
        }

        ~Pardiso() => this.Dispose(false);

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this._disposed)
                return;
            Pardiso.pardiso((Span<IntPtr>)this._pt, this._maxfct, this._mnum, this._type, -1, this._n, this._matrix.a, this._matrix.ia, this._matrix.ja, (Span<int>)(int[])null!, this._nrhs, (Span<int>)this._iparm, this._msglvl, (ReadOnlySpan<T>)(T[])null!, (Span<T>)(T[])null!, out int _);
            this._disposed = true;
        }
    }
}

