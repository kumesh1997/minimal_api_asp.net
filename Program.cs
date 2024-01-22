using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
     c.SwaggerDoc("v1", new OpenApiInfo {
         Title = "TodoApp API",
         Description = "Making you Todos",
         Version = "v1" });
});
//Connection string
var connectionString = builder.Configuration.GetConnectionString("WebApiDatabase");
//Add DB context
builder.Services.AddDbContext<TodoDb>(opt => opt.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)).LogTo(Console.WriteLine, LogLevel.Information)
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors());

builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddCors();
builder.Services.AddAuthentication().AddJwtBearer();
// Configure Authorization Policies
builder.Services.AddAuthorizationBuilder()
  .AddPolicy("admin_greetings", policy =>
        policy
            .RequireRole("admin")
            .RequireClaim("scope", "greetings_api"));
            
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
   c.SwaggerEndpoint("/swagger/v1/swagger.json", "TodoApp API V1");
});

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

