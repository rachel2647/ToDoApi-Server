using Microsoft.EntityFrameworkCore;
using ToDoApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ToDoDbContext>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(options =>
{
    options.AddPolicy("MyCors", policy =>
        policy.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
});
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseCors("MyCors");

app.UseSwagger(options =>
{
    options.SerializeAsV2 = true;
});
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});

app.MapGet("/", () => "Hello World!");
app.MapGet("/items", async (ToDoDbContext db) =>
              await db.Items.ToListAsync());
app.MapGet("/items/{id}", async (int id, ToDoDbContext db) =>
              await db.Items.FindAsync(id)
                  is Item item
                  ? Results.Ok(item)
                  : Results.NotFound());
app.MapPost("/items/", async (Item item, ToDoDbContext db) =>
{
    db.Items.Add(item);
    await db.SaveChangesAsync();
    return Results.Created($"/items/{item.Id}", item);
});
app.MapPut("/items/{id}", async (int id, Boolean isComplete, ToDoDbContext db) =>
{
    Console.WriteLine("put func");
    var item = await db.Items.FindAsync(id);
    if (item is null)
        return Results.NotFound();
    item.IsComplete = isComplete;
    await db.SaveChangesAsync();
    return Results.NoContent();
});
app.MapDelete("/items/{id}", async (int id, ToDoDbContext db) =>
{
    if (await db.Items.FindAsync(id) is Item item)
    {
        db.Items.Remove(item);
        await db.SaveChangesAsync();
        return Results.Ok(item);
    }
    return Results.NotFound();
});
app.Run();
