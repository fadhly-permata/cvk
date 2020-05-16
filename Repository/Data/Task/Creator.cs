using Repository.Models.DTO;
using Repository.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Repository.Data.Task
{
    public class Creator
    {
        private readonly Manager _manager;

        public Creator(Manager manager)
        {
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        }

        public async Task<TaskView> Create(TaskCreate data, CancellationToken cancellationToken = default)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var oData = data.CopyPropertiesToNewObject<Models.Task>();
            oData.CreatedAt = DateTime.Now;
            await _manager.DbContext.AddAsync(oData, cancellationToken);
            await _manager.DbContext.SaveChangesAsync(cancellationToken);

            return oData.CastToView();
        }
    }
}