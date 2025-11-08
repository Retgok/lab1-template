using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();
var connectionString = Environment.GetEnvironmentVariable("DB_CONN_STR")
                       ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<PersonDb>(options =>
    options.UseNpgsql(connectionString));

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});
builder.Services.AddScoped<IPersonRepo, PersonRepo>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "PersonAPI";
    config.Title = "PersonAPI v1";
    config.Version = "v1";
});

builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (System.Text.Json.JsonException ex)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/json";

        var error = new ValidationErrorResponse(
            "Invalid data",
            new Dictionary<string, string> { { "body", ex.Message } }
        );

        await context.Response.WriteAsJsonAsync(error);
    }
});

app.MapControllers();

app.Run();
