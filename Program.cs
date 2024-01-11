using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
// builder.Services.AddDbContext<TodoDb>(opt => opt.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddCors();
builder.Services.AddAuthentication().AddJwtBearer();

// OpenAPI Builder
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Authorization Policies
builder.Services.AddAuthorizationBuilder()
  .AddPolicy("admin_greetings", policy =>
        policy
            .RequireRole("admin")
            .RequireClaim("scope", "greetings_api"));
            

var app = builder.Build();

// Use OpenAPI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Use Exception Handler Midleware
app.UseExceptionHandler(exceptionHandlerApp 
    => exceptionHandlerApp.Run(async context 
        => await Results.Problem()
                     .ExecuteAsync(context)));


// Outside endpoints
TodoEndpoints.Map(app);

app.Run();
// app.Run("http://localhost:3000");

