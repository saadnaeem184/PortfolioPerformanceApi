using Application.Interfaces;
using Application.Services;
using Infrastructure.Data;
using Infrastructure.ExternalServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register custom services for Dependency Injection
// Infrastructure Layer registrations
builder.Services.AddSingleton<IPortfolioRepository, InMemoryStore>();
builder.Services.AddSingleton<IMarketDataService, MarketDataService>();

// Application Layer registrations
builder.Services.AddScoped<IPortfolioService, PortfolioService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }