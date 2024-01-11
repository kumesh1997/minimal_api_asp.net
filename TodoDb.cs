using Microsoft.EntityFrameworkCore;

class TodoDb : DbContext
{
    public TodoDb(DbContextOptions<TodoDb> options)
        : base(options) { }

    public DbSet<Todo> todo => Set<Todo>();

    // public TodoDb(DbContextOptions<TodoDb> options)
    //     : base(options) { }

    // public DbSet<Todo>? todo {get; set;}
}