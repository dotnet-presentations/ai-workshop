using Microsoft.EntityFrameworkCore;
using Products.Data;
using Products.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ProductDataContext>(options =>
    options.UseSqlite("Data Source=catalog.db"));

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapProductEndpoints();

// Create and seed the catalog database on first run.
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ProductDataContext>();
    context.Database.EnsureCreated();
    SeedData.Initialize(context);
}

app.Run();
