using Microsoft.EntityFrameworkCore;
using Repository.Models.DTO;
using Repository.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Repository.Data.Task
{
    public class Deleter
    {
        private readonly Manager _manager;

        public Deleter(Manager manager)
        {
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        }

        public async Task<TaskView> Delete(Guid id, CancellationToken cancellationToken = default)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            var oData = await _manager.Reader.BaseQuery().FirstOrDefaultAsync(x => x.TaskId.Equals(id), cancellationToken);
            if (oData == null)
                throw new Exception("Can not remove unregistered data.");

            // map to entity model and set deleted flag
            var tData = oData.CopyPropertiesToNewObject<Models.Task>();
            tData.IsDeleted = true;

            _manager.DbContext.Update(tData);
            await _manager.DbContext.SaveChangesAsync(cancellationToken);
            return tData.CastToView();
        }
    }
}