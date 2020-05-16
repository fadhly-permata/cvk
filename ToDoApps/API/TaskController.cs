using BsTableQueryable;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Repository;
using Repository.Data.Task;
using Repository.Models.DTO;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ToDoApps.Models;

namespace ToDoApps.API
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly AppSettingsModel _appSettings;
        private readonly EntityManagerInitiateModel _emi;

        public TaskController(IOptions<AppSettingsModel> appSettings)
        {
            _appSettings = appSettings.Value;
            _emi = new EntityManagerInitiateModel
            {
                Constring = _appSettings.Constring.Main
            };
        }

        [HttpGet]
        public async Task<List<TaskView>> Get(CancellationToken cancellationToken)
        {
            using var mgr = new Manager(_emi);
            return await mgr.Reader.IntoList(null, cancellationToken);
        }

        [HttpGet("{id}")]
        public async Task<TaskView> Get(CancellationToken cancellationToken, Guid id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            using var mgr = new Manager(_emi);
            return await mgr.Reader.IntoSingle(x => x.TaskId.Equals(id), cancellationToken);
        }

        [HttpPost]
        public async Task<BsTableModel.Response<TaskView>> Paged(CancellationToken cancellationToken, [FromBody] BsTableModel.Request request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            using var mgr = new Manager(_emi);
            return await mgr.Reader.Paged(request, cancellationToken);
        }

        [HttpPost]
        public async Task<TaskView> Create(CancellationToken cancellationToken, [FromBody] TaskCreate data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            using var mgr = new Manager(_emi);
            return await mgr.Creator.Create(data, cancellationToken);
        }

        [HttpPut]
        public async Task<TaskView> Update(CancellationToken cancellationToken, [FromBody] TaskUpdate data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            using var mgr = new Manager(_emi);
            return await mgr.Updater.Update(data, cancellationToken);
        }

        [HttpDelete]
        public async Task<TaskView> Delete(CancellationToken cancellationToken, Guid id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            using var mgr = new Manager(_emi);
            return await mgr.Deleter.Delete(id, cancellationToken);
        }
    }
}