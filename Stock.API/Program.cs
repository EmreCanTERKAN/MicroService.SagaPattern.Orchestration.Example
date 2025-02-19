using MassTransit;
using MongoDB.Driver;
using Shared.Settings;
using Stock.API.Consumers;
using Stock.API.Models;
using Stock.API.Services;
//Consumer �zerinden i�lem yap�laca�� i�in swaggerlar� falan Stock.API'dan sildik
var builder = WebApplication.CreateBuilder(args);


// MassTransit Konfig�rasyonu ayarlar�
builder.Services.AddMassTransit(configurator =>
{
    configurator.AddConsumer<OrderCreatedEventConsumer>();
    configurator.AddConsumer<StockRollbackMessageConsumer>();
    configurator.UsingRabbitMq((context, _configure) =>
    {
        _configure.Host(builder.Configuration["RabbitMQ"]);

        _configure.ReceiveEndpoint(RabbitMQSettings.Stock_OrderCreatedEventQueue,e=> e.ConfigureConsumer<OrderCreatedEventConsumer>(context));
        _configure.ReceiveEndpoint(RabbitMQSettings.Stock_RollbackMessageQueue,e=> e.ConfigureConsumer<StockRollbackMessageConsumer>(context));
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
