using Microsoft.EntityFrameworkCore;

public static class TodoEndpoints{
    public static void Map(WebApplication app){

        var todoItems = app.MapGroup("/todoitems");

        todoItems.MapGet("/", GetAllTodos);
        todoItems.MapGet("/{id:int}", GetTodo).RequireAuthorization("admin_greetings");
        // todoItems.MapGet("/complete", GetCompleteTodos);
        todoItems.MapPost("/", MyHandler.create);
        todoItems.MapPut("/{id:int}", UpdateTodo);
        todoItems.MapDelete("/{id:int}", DeleteTodo);
        todoItems.MapGet("/exception", () => 
        {
            throw new InvalidOperationException("Sample Exception");
        });
    }

    static async Task<IResult> GetAllTodos(TodoDb db)
    {
        return TypedResults.Ok(await db.todo.Select(x => new TodoItemDTO(x)).ToArrayAsync());
    }

    // static async Task<IResult> GetCompleteTodos(TodoDb db) {
    // return TypedResults.Ok(await db.todo.Where(t => t.IsComplete).Select(x => new TodoItemDTO(x)).ToListAsync());
    // }


    static async Task<IResult> GetTodo(TodoDb db, int id)
    {
        return await db.todo.FindAsync(id) is Todo todo? TypedResults.Ok(new TodoItemDTO(todo)) : TypedResults.NotFound();
    }

    static async Task<IResult> CreateTodo(TodoItemDTO? todoItemDTO, TodoDb db)
{
    var todoItem = new Todo
    {
        // IsComplete = todoItemDTO.IsComplete,
        Name = todoItemDTO.Name
    };

    db.todo.Add(todoItem);
    await db.SaveChangesAsync();

    todoItemDTO = new TodoItemDTO(todoItem);

    return TypedResults.Created($"/todoitems/{todoItem.Id}", todoItemDTO);
}

static async Task<IResult> UpdateTodo(int? id, TodoItemDTO todoItemDTO, TodoDb db)
{
    var todo = await db.todo.FindAsync(id);

    if (todo is null) return TypedResults.NotFound();

    todo.Name = todoItemDTO.Name;
    // todo.IsComplete = todoItemDTO.IsComplete;

    await db.SaveChangesAsync();

    return TypedResults.NoContent();
}


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

    // Static Method
class MyHandler
{
    public static async Task<IResult> create(TodoItemDTO? todoItemDTO, TodoDb db)
    {
        var todoItem = new Todo
    {
        // IsComplete = todoItemDTO.IsComplete,
        Id = todoItemDTO.Id,
        Name = todoItemDTO.Name
    };

    db.todo.AddAsync(todoItem);
    await db.SaveChangesAsync();

    todoItemDTO = new TodoItemDTO(todoItem);

    return TypedResults.Created($"/todoitems/{todoItem.Id}", todoItemDTO);
    }
}


}