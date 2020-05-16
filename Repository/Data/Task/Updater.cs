using Microsoft.EntityFrameworkCore;
using Repository.Models.DTO;
using Repository.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Repository.Data.Task
{
    public class Updater
    {
        private readonly Manager _manager;

        public Updater(Manager manager)
        {
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        }

        public async Task<TaskView> Update(TaskUpdate data, CancellationToken cancellationToken = default)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var oData = await _manager.Reader.BaseQuery().FirstOrDefaultAsync(x => x.TaskId.Equals(data.TaskId), cancellationToken);
            if (oData == null)
                throw new Exception("Can not modify unregistered data.");

            // map to entity model and set default value
            var tData = oData.CopyPropertiesToNewObject<Models.Task>();
            tData.IsDeleted = false;

            // update change fields
            data.CopyPropertiesTo(tData);

            _manager.DbContext.Update(tData);
            await _manager.DbContext.SaveChangesAsync(cancellationToken);
            return tData.CastToView();
        }
    }
}