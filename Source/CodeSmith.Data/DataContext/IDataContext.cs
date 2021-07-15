using System;

namespace CodeSmith.Data
{
    public interface IDataContext : IDisposable
    {
        string ConnectionString { get; }

        bool HasOpenTransaction { get; }

        bool ObjectTrackingEnabled { get; set; }

        IDisposable BeginTransaction();

        void CommitTransaction();

        void Detach(params object[] enities);

        void RollbackTransaction();

        void SubmitChanges();
    }
}
