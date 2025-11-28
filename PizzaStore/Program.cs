using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PizzaStore.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Services (Swagger and EF Core In-Memory)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo {
        Title = "PizzaStore API",
        Description = "Making the Pizzas you love",
        Version = "v1" });
});

// Configure In-Memory Database
builder.Services.AddDbContext<PizzaDb>(options => 
    options.UseInMemoryDatabase("pizzas"));

var app = builder.Build();

// 2. Configure Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PizzaStore API V1");
    });
}

app.MapGet("/", () => "Hello World!");

// 3. CRUD Operations

// GET All Pizzas
app.MapGet("/pizzas", async (PizzaDb db) => 
    await db.Pizzas.ToListAsync());

// GET Single Pizza
app.MapGet("/pizza/{id}", async (PizzaDb db, int id) => 
    await db.Pizzas.FindAsync(id));

// POST (Create) Pizza
app.MapPost("/pizza", async (PizzaDb db, Pizza pizza) =>
{
    await db.Pizzas.AddAsync(pizza);
    await db.SaveChangesAsync();
    return Results.Created($"/pizza/{pizza.Id}", pizza);
});

// PUT (Update) Pizza
app.MapPut("/pizza/{id}", async (PizzaDb db, Pizza updatepizza, int id) =>
{
    var pizza = await db.Pizzas.FindAsync(id);
    if (pizza is null) return Results.NotFound();
    
    pizza.Name = updatepizza.Name;
    pizza.Description = updatepizza.Description;
    
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// DELETE Pizza
app.MapDelete("/pizza/{id}", async (PizzaDb db, int id) =>
{
    var pizza = await db.Pizzas.FindAsync(id);
    if (pizza is null) return Results.NotFound();
    
    db.Pizzas.Remove(pizza);
    await db.SaveChangesAsync();
    return Results.Ok();
});

app.Run();

// 4. Database Context Class
class PizzaDb : DbContext
{
    public PizzaDb(DbContextOptions options) : base(options) { }
    public DbSet<Pizza> Pizzas { get; set; } = null!;
}