using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Neo4j.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

// Configure the IDriver interface
var driver = GraphDatabase.Driver("neo4j://localhost:7687", AuthTokens.Basic("neo4j", "svejedno6969"));

// Register the IDriver instance for dependency injection
builder.Services.AddSingleton<IDriver>(driver);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors(options => options
    .WithOrigins(new[] { "http://localhost:5173" })
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials());

app.UseEndpoints(endpoints =>
{
    _ = endpoints.MapControllers();
});

app.Run();