using MassTransit;
using MongoDB.Driver;
using Stock.API.Models;
using Stock.API.Services;
//Consumer üzerinden iþlem yapýlacaðý için swaggerlarý falan Stock.API'dan sildik
var builder = WebApplication.CreateBuilder(args);


// MassTransit Konfigürasyonu ayarlarý
builder.Services.AddMassTransit(configurator =>
{
    configurator.UsingRabbitMq((context, _configure) =>
    {
        _configure.Host(builder.Configuration["RabbitMQ"]);
    });
});

builder.Services.AddSingleton<MongoDbService>();

var app = builder.Build();

using var scope = builder.Services.BuildServiceProvider().CreateScope();
var mongoDbService = scope.ServiceProvider.GetRequiredService<MongoDbService>();

if(!await (await mongoDbService.GetCollection<Stock.API.Models.Stock>().FindAsync(x=> true)).AnyAsync())
{
    mongoDbService.GetCollection<Stock.API.Models.Stock>().InsertOne(new Stock.API.Models.Stock()
    {
        ProductId = 1,
        Count = 200
    });
    mongoDbService.GetCollection<Stock.API.Models.Stock>().InsertOne(new Stock.API.Models.Stock()
    {
        ProductId = 2,
        Count = 300
    });
    mongoDbService.GetCollection<Stock.API.Models.Stock>().InsertOne(new Stock.API.Models.Stock()
    {
        ProductId = 3,
        Count = 69
    });
}

app.Run();
