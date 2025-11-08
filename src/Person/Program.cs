using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<PersonDb>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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

// builder.Services.AddControllers();
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

// app.MapGet("/", () => Results.Redirect("/swagger"));

// var persons = app.MapGroup("/api/v1/persons");

//     persons.MapGet("/", async (PersonDb db) =>
//     {
//         var list = await db.Persons.Select(p => new PersonResponse(p)).ToListAsync();
//         return Results.Json(list);
//     })
//     .Produces<List<PersonResponse>>(StatusCodes.Status200OK, "application/json");

// persons.MapGet("/{id:int}", async (int id, PersonDb db) =>
// {
//     var person = await db.Persons.FindAsync(id);
//     if (person is null)
//     {
//         return Results.Json(new ErrorResponse($"Person with id={id} not found"),
//             statusCode: StatusCodes.Status404NotFound);
//     }

//     return Results.Json(new PersonResponse(person));
// })
// .Produces<PersonResponse>(StatusCodes.Status200OK)
// .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

// persons.MapPost("/", async (PersonRequest dto, PersonDb db) =>
// {
//     if (string.IsNullOrWhiteSpace(dto.Name))
//     {
//         return Results.Json(new ValidationErrorResponse(
//             "Invalid data",
//             new Dictionary<string, string> { { "name", "Name is required" } }),
//             statusCode: StatusCodes.Status400BadRequest);
//     }

//     var person = new Person
//     {
//         Name = dto.Name,
//         Age = dto.Age,
//         Address = dto.Address,
//         Work = dto.Work
//     };

//     db.Persons.Add(person);
//     await db.SaveChangesAsync();

//     var location = $"/api/v1/persons/{person.Id}";
//     return Results.Created(location, null);
// })
// .Produces(StatusCodes.Status201Created)
// .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);

// persons.MapPatch("/{id:int}", async (int id, PersonRequest dto, PersonDb db) =>
// {
//     var person = await db.Persons.FindAsync(id);
//     if (person is null)
//     {
//         return Results.Json(new ErrorResponse($"Person with id={id} not found"),
//             statusCode: StatusCodes.Status404NotFound);
//     }

//     if (dto.Name != null && string.IsNullOrWhiteSpace(dto.Name))
//     {
//         return Results.Json(new ValidationErrorResponse(
//             "Invalid data",
//             new Dictionary<string, string> { { "name", "Name cannot be empty" } }),
//             statusCode: StatusCodes.Status400BadRequest);
//     }

//     if (dto.Name != null) person.Name = dto.Name;
//     if (dto.Age.HasValue) person.Age = dto.Age;
//     if (dto.Address != null) person.Address = dto.Address;
//     if (dto.Work != null) person.Work = dto.Work;

//     await db.SaveChangesAsync();

//     return Results.Json(new PersonResponse(person));
// })
// .Produces<PersonResponse>(StatusCodes.Status200OK)
// .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest)
// .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

// persons.MapDelete("/{id:int}", async (int id, PersonDb db) =>
// {
//     var person = await db.Persons.FindAsync(id);
//     if (person is null)
//     {
//         return Results.Json(new ErrorResponse($"Person with id={id} not found"),
//             statusCode: StatusCodes.Status404NotFound);
//     }

//     db.Persons.Remove(person);
//     await db.SaveChangesAsync();
//     return Results.NoContent();
// })
// .Produces(StatusCodes.Status204NoContent)
// .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

app.Run();
