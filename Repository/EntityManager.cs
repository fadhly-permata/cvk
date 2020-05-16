using Repository.Contexts;
using System;

namespace Repository
{
    public class EntityManagerInitiateModel
    {
        //public AuthenticationResultModel AuthInfo { get; set; }
        public string Constring { get; set; }
    }

    public class EntityManager : IDisposable
    {
        public EntityManager(EntityManagerInitiateModel data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (string.IsNullOrWhiteSpace(data.Constring))
                throw new Exception("Can not initialize database connection.");

            DbContext = new CvkmpContext(data.Constring);
            InitiatorData = data;
        }

        internal CvkmpContext DbContext { get; }

        public EntityManagerInitiateModel InitiatorData { get; internal set; }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        ~EntityManager()
        {
            Dispose(false);
        }

        private static void ReleaseUnmanagedResources()
        {
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    DbContext?.Dispose();

                    InitiatorData = null;

                    ReleaseUnmanagedResources();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}