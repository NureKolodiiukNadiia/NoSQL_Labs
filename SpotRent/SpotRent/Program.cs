using dotenv.net;
using SpotRent.Endpoints;
using SpotRent.Data;
using SpotRent.Extensions;
using SpotRent.Implementations;
using SpotRent.Interfaces;
using SpotRent.Middleware;

DotEnv.Load();
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.Configure<MongoOptions>(builder.Configuration.GetSection("Mongo"));
builder.Services.AddScoped<MongoDbContext>();
builder.Services.AddTransient<DbSchemaCreateOrValidateService>();
builder.Services.RegisterServices();
builder.Services.AddCors();

var app = builder.Build();
var dbInitService = app.Services.GetRequiredService<DbSchemaCreateOrValidateService>();
await dbInitService.InitializeAsync();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseCors(b => b.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
app.MapUserEndpoints();
app.MapWorkspaceEndpoints();
app.MapBookingEndpoints();

app.Run();
