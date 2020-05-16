using Repository.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Repository.Models.DTO
{
    public class TaskView : TaskUpdate
    {
        [Column(TypeName = "datetime")]
        public DateTime CreatedAt { get; set; }
    }

    public class TaskCreate
    {
        [Required]
        [StringLength(50)]
        public string Title { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? StartDate { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? EndDate { get; set; }

        public short Status { get; set; }

        [StringLength(255)]
        public string Description { get; set; }
    }

    public class TaskUpdate : TaskCreate
    {
        public Guid TaskId { get; set; }
    }

    internal static class TaskExtension
    {
        internal static Task CastToEntity(this TaskView data)
        {
            return data.CopyPropertiesToNewObject<Task>();
        }

        internal static TaskView CastToView(this Task data)
        {
            return data.CopyPropertiesToNewObject<TaskView>();
        }

        internal static List<Task> CastToEntity(this List<TaskView> data)
        {
            return data.Select(x => x.CastToEntity()).ToList();
        }

        internal static List<TaskView> CastToView(this List<Task> data)
        {
            return data.Select(x => x.CastToView()).ToList();
        }
    }
}