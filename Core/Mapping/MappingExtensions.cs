using Data.Dtos;
using Data.Models;
using System.Runtime.CompilerServices;

namespace Core.Mapping
{
    internal class MappingExtensions
    {
    }

    public static class TodoMappingExtensions
    {
        public static TodoDto AsDto(this Todo todo)
        {
            return new()
            {
                Id = todo.Id,
                Title = todo.Title,
                Tasks = todo.Tasks.Select(x => x.AsDto()).ToList(),
            };
        }
        public static TodoTaskDto AsDto(this TodoTask todoTask)
        {
            return new()
            {
                TaskName = todoTask.TaskName,
                IsCompleted = todoTask.IsCompleted
            };
        }

        public static Todo AsEntity(this TodoDto todoDto, string userId)
        {
            return new()
            {
                Title = todoDto.Title,
                OwnerId = userId
            };
        }

        public static TodoTask AsEntity(this TodoTaskDto todoTaskDto)
        {
            return new()
            {
                TaskName = todoTaskDto.TaskName,
                IsCompleted = todoTaskDto.IsCompleted
            };
        }
    }
}
