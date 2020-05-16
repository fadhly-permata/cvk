using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Repository.Utils
{
    public class DataManagerInterfaces
    {
        public interface ICreator<TEntity, TView>
        {
            Task<TView> Create(TEntity data, CancellationToken cancellationToken = default);
        }

        public interface IUpdater<TEntity, TView>
        {
            Task<TView> Update(TEntity data, CancellationToken cancellationToken = default);
        }

        public interface IDeleter<in IdType, TEntity>
        {
            Task<TEntity> Delete(IdType id, CancellationToken cancellationToken = default);

            Task<TEntity> SoftDelete(IdType id, CancellationToken cancellationToken = default);
        }

        [Obsolete("This interface is obsolete, Use IReaderV2 instead.")]
        public interface IReaderV1<TView, TId, BsTableResponse, BsTableRequest>
        {
            Task<BsTableResponse> Paged(BsTableRequest request, CancellationToken cancellationToken = default);

            Task<List<TView>> IntoList(Expression<Func<TView, bool>> predicate, CancellationToken cancellationToken = default);

            Task<TView> IntoSingle(Expression<Func<TView, bool>> predicate, CancellationToken cancellationToken = default);
        }

        public interface IReaderV2<TBase, TView, BsTableRequest, BsTableResponse>
        {
            Task<List<TView>> ListView(Expression<Func<TBase, bool>> predicate, CancellationToken cancellationToken = default);

            Task<TView> SingleView(Expression<Func<TBase, bool>> predicate, CancellationToken cancellationToken = default);

            Task<List<TBase>> ListBase(Expression<Func<TBase, bool>> predicate, CancellationToken cancellationToken = default);

            Task<TBase> SingleBase(Expression<Func<TBase, bool>> predicate, CancellationToken cancellationToken = default);

            Task<BsTableResponse> Paged(Guid id, BsTableRequest request, CancellationToken cancellationToken = default);

            Task<BsTableResponse> Paged(BsTableRequest request, CancellationToken cancellationToken = default);
        }
    }
}