using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

public static class TodoEndpoints{
    public static void Map(WebApplication app)
    {

        var todoItems = app.MapGroup("/todoitems");

        todoItems.MapGet("/", GetAllTodos);
        todoItems.MapGet("/{id:int}", GetTodo).RequireAuthorization("admin_greetings");
        todoItems.MapGet("/complete", GetCompleteTodos);
        // OpenAPI annotation for endpoint
        todoItems.MapPost("/", MyHandler.create)
        .WithName("Create Todo")
        .WithOpenApi();
        todoItems.MapPut("/{id:int}", UpdateTodo);
        // Add two Filters
        todoItems.MapDelete("/{id:int}", DeleteTodo)
        .AddEndpointFilter(async (invocationContext, next) =>
            {
                var id = invocationContext.GetArgument<int>(0);

                if (id == 0)
                {
                    return Results.Problem("No such Todo!");
                }
                return await next(invocationContext);
            })
            .AddEndpointFilter(async (invocationContext, next) =>
            {
                var id = invocationContext.GetArgument<int>(0);

                if (id == 1)
                {
                    return Results.Problem("Cannot Delete Toto 1!");
                }
                return await next(invocationContext);
            });
        todoItems.MapGet("/exception", () =>
        {
            throw new InvalidOperationException("Sample Exception");
        });

        // Multiple Responses
        todoItems.MapGet("/hello/{id:int}", Results<BadRequest, Ok> (int id)
            => id > 999 ? TypedResults.BadRequest() : TypedResults.Ok());
    }

// Get All Todos
    static async Task<IResult> GetAllTodos(TodoDb db)
{
    try{
        return TypedResults.Ok(await db.todo.Select(x => new TodoItemDTO(x)).ToArrayAsync()); 
    }catch(Exception e){
        Console.WriteLine(e.Message);
    }
    return TypedResults.NoContent();
}

// Get Completed Todos
static async Task<IResult> GetCompleteTodos(TodoDb db) {
    return TypedResults.Ok(await db.todo.Where(t => t.IsComplete).Select(x => new TodoItemDTO(x)).ToListAsync());
}

// Get Todo
static async Task<IResult> GetTodo(TodoDb db, int id)
{
    return await db.todo.FindAsync(id) is Todo todo? TypedResults.Ok(new TodoItemDTO(todo)) : TypedResults.NotFound();
}

// Create Todo
    static async Task<IResult> CreateTodo(TodoItemDTO? todoItemDTO, TodoDb db)
{
    var todoItem = new Todo
    {
        IsComplete = todoItemDTO.IsComplete,
        Name = todoItemDTO.Name
    };

    db.todo.Add(todoItem);
    await db.SaveChangesAsync();

    todoItemDTO = new TodoItemDTO(todoItem);

    return TypedResults.Created($"/todoitems/{todoItem.Id}", todoItemDTO);
}

// Update Todo
static async Task<IResult> UpdateTodo(int? id, TodoItemDTO todoItemDTO, TodoDb db)
{
    var todo = await db.todo.FindAsync(id);

    if (todo is null) return TypedResults.NotFound();

    todo.Name = todoItemDTO.Name;
    todo.IsComplete = todoItemDTO.IsComplete;

    await db.SaveChangesAsync();

    return TypedResults.NoContent();
}


// Delete Todo
static async Task<IResult> DeleteTodo(int id, TodoDb db)
{
    if (await db.todo.FindAsync(id) is Todo todo)
    {
        db.todo.Remove(todo);
        await db.SaveChangesAsync();
        return TypedResults.NoContent();
    }

    return TypedResults.NotFound();
}

    // Static Method to Create Todo
class MyHandler
{
    public static async Task<IResult> create(TodoItemDTO? todoItemDTO, TodoDb db)
    {
        var todoItem = new Todo
    {
        IsComplete = todoItemDTO.IsComplete,
        Name = todoItemDTO.Name
    };

    db.todo.Add(todoItem);
    await db.SaveChangesAsync();

    todoItemDTO = new TodoItemDTO(todoItem);

    return TypedResults.Created($"/todoitems/{todoItem.Id}", todoItemDTO);
    }
}

}