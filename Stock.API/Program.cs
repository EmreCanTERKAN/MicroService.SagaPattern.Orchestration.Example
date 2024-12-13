using MassTransit;
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

var app = builder.Build();


app.Run();
