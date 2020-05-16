using BsTableQueryable;
using Microsoft.EntityFrameworkCore;
using Repository.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Repository.Data.Task
{
    public class Reader
    {
        private readonly Manager _manager;

        public Reader(Manager manager)
        {
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        }

        internal IQueryable<TaskView> BaseQuery()
        {
            return
                from
                    task in _manager.DbContext.Task

                where
                    task.IsDeleted == false

                select
                    new TaskView
                    {
                        TaskId = task.TaskId,
                        Title = task.Title,
                        StartDate = task.StartDate,
                        EndDate = task.EndDate,
                        Status = task.Status,
                        Description = task.Description,
                        CreatedAt = task.CreatedAt
                    };
        }

        public async Task<List<TaskView>> IntoList(Expression<Func<TaskView, bool>> predicate = null, CancellationToken cancellationToken = default)
        {
            var query = BaseQuery();
            return await (predicate == null ? query : query.Where(predicate)).ToListAsync(cancellationToken);
        }

        public async Task<TaskView> IntoSingle(Expression<Func<TaskView, bool>> predicate = null, CancellationToken cancellationToken = default)
        {
            var query = BaseQuery();
            return await query.FirstOrDefaultAsync(predicate, cancellationToken);
        }

        public async Task<BsTableModel.Response<TaskView>> Paged(BsTableModel.Request request, CancellationToken cancellationToken = default)
        {
            var result = await BsTableDataProvider.BsTableDataGenerator(
                    BaseQuery(),
                    request,
                    cancellationToken
                );

            return new BsTableModel.Response<TaskView>
            {
                Total = result.Total,
                TotalNotFiltered = result.TotalNotFiltered,
                Rows = result.Rows
            };
        }
    }
}