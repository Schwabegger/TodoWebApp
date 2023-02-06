using Core.Interfaces;
using Data.Dtos;
using Data.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Endpoints
{
    public static class TodoEndpoints
    {
        public static void ConfigureTodoEndpoints(this WebApplication app)
        {
            var todosGroup = app.MapGroup("/api/todos");
            todosGroup.WithOpenApi();
            //todosGroup.RequireAuthorization();

            todosGroup.MapGet("/", GetAllAsync)
                .WithName("GetTodos");

            todosGroup.MapGet("/owned", GetAllOwnedAsync)
                .WithName("GetOwnedTodos");

            todosGroup.MapGet("/{id:int}", GetTodo)
                .WithName("GetTodo");

            todosGroup.MapPost("/", CreateTodo)
                .WithName("CreateTodo");

            todosGroup.MapPut("/", UpdateTodo)
                .WithName("UpdateTodo");

            todosGroup.MapDelete("/{id:int}", DeleteTodo)
                .WithName("DeleteTodo");
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        private static async Task<IResult> GetAllOwnedAsync(ITodoService todoService, IHttpContextAccessor httpContextAccessor)
        {
            var userId = GetUserId(httpContextAccessor);
            if (userId == null)
                return Results.BadRequest("User id not found");

            var response = new ApiResponseDto();
            response.Content = await todoService.GetTodosByUserIdAsync(userId);
            response.Success = true;

            return Results.Ok(response);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        private static async Task<IResult> CreateTodo(ITodoService todoService, IHttpContextAccessor httpContextAccessor, [FromBody] TodoDto todoDto)
        {
            var userId = GetUserId(httpContextAccessor);
            if (userId == null)
                return Results.BadRequest("User id not found");
            
            await todoService.CreateTodoAsync(todoDto, userId);
            return Results.Ok();
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        private static async Task<IResult> GetAllAsync(ITodoService todoService, IHttpContextAccessor httpContextAccessor)
        {
            var todos = await todoService.GetAllTodosAsync();
            return Results.Ok(todos);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        private static async Task<IResult> GetTodo(ITodoService todoService, IHttpContextAccessor httpContextAccessor, int id)
        {
            var userId = GetUserId(httpContextAccessor);
            if (userId == null)
                return Results.BadRequest("User id not found");
            
            throw new NotImplementedException();
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        private static async Task<IResult> UpdateTodo(ITodoService todoService, IHttpContextAccessor httpContextAccessor, [FromBody] TodoDto todoDto)
        {
            var userId = GetUserId(httpContextAccessor);
            if (userId == null)
                return Results.BadRequest("User id not found");
            
            throw new NotImplementedException();
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        private static async Task<IResult> DeleteTodo(ITodoService todoService, IHttpContextAccessor httpContextAccessor, int id)
        {
            var userId = GetUserId(httpContextAccessor);
            if (userId == null)
                return Results.BadRequest("User id not found");
            
            throw new NotImplementedException();
        }

        private static string GetUserId(IHttpContextAccessor httpContextAccessor)
        {
            var user = httpContextAccessor.HttpContext.User;
            return user.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}
