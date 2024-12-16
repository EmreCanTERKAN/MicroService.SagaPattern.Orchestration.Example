using MassTransit;
using Payment.API.Consumer;
using Shared.Settings;
//Consumer üzerinden iþlem yapýlacaðý için swaggerlarý falan Payment.API'dan sildik
var builder = WebApplication.CreateBuilder(args);

// MassTransit Konfigürasyonu ayarlarý
builder.Services.AddMassTransit(configurator =>
{
    configurator.AddConsumer<PaymentStartedEventConsumer>();
    configurator.UsingRabbitMq((context, _configure) =>
    {
        _configure.Host(builder.Configuration["RabbitMQ"]);
        _configure.ReceiveEndpoint(RabbitMQSettings.Payment_PaymentStartedEventQueue, e => e.ConfigureConsumer<PaymentStartedEventConsumer>(context));
    });
});

var app = builder.Build();


app.Run();
