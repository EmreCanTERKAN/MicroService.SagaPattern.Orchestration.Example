using MassTransit;
using Payment.API.Consumer;
using Shared.Settings;
//Consumer �zerinden i�lem yap�laca�� i�in swaggerlar� falan Payment.API'dan sildik
var builder = WebApplication.CreateBuilder(args);

// MassTransit Konfig�rasyonu ayarlar�
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
