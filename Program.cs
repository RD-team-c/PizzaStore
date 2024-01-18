using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;


// using PizzaStore.DB; // v1
using PizzaStore.Models; // v1.1


// BUILDER SERVICES ---

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<PizzaDb>(options => options.UseInMemoryDatabase("items")); // v1.1

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
 c.SwaggerDoc("v1.1", new OpenApiInfo { Title = "PizzaStore API", Description = "Making the Pizzas you love", Version = "v1.1" });
});

// APPLICATION CONTEXT ---

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
 c.SwaggerEndpoint("/swagger/v1.1/swagger.json", "PizzaStore API V1.1");
});


// v1 ---
// app.MapGet("/pizzas/{id}", (int id) => PizzaDB.GetPizza(id));
// app.MapGet("/pizzas", () => PizzaDB.GetPizzas());
// app.MapPost("/pizzas", (Pizza pizza) => PizzaDB.CreatePizza(pizza));
// app.MapPut("/pizzas", (Pizza pizza) => PizzaDB.UpdatePizza(pizza));
// app.MapDelete("/pizzas/{id}", (int id) => PizzaDB.RemovePizza(id));

// v1.1 ---

// Create (post) pizza
app.MapPost("/pizza", async (PizzaDb db, Pizza pizza) =>
{
 await db.Pizzas.AddAsync(pizza);
 await db.SaveChangesAsync();
 return Results.Created($"/pizza/{pizza.Id}", pizza);
});

// update (put) pizza 
app.MapPut("/pizza/{id}", async (PizzaDb db, Pizza updatePizza, int id) => {
 var pizza = await db.Pizzas.FindAsync(id);
 if (pizza is null) return Results.NotFound();
 pizza.Name = updatePizza.Name;
 pizza.Description = updatePizza.Description;
 await db.SaveChangesAsync();
 return Results.NoContent();
});

// get all pizzas 
app.MapGet("/pizzas", async (PizzaDb db) => await db.Pizzas.ToListAsync());

// get pizza by id
app.MapGet("/pizza/{id}", async (PizzaDb db, int id) => await db.Pizzas.FindAsync(id));

// get all subclasses sanity test
app.MapGet("/", () => "Hello World!");

// Delete pizza
app.MapDelete("/pizza/{id}", async (PizzaDb db, int id) =>
{
 // await to find pizza by async id
 var pizza = await db.Pizzas.FindAsync(id);
 // check null
 if (pizza is null) return Results.NotFound();
 // remove pizza
 db.Pizzas.Remove(pizza);
 // await and save changes
 await db.SaveChangesAsync();
 // return result status
 return Results.Ok();
});

// ---
app.Run();