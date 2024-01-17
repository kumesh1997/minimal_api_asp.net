using Microsoft.EntityFrameworkCore;

class TodoDb : DbContext
{
    public TodoDb(DbContextOptions<TodoDb> options): base(options) { }

    public DbSet<Todo> todo {get; set;} = null!;
}