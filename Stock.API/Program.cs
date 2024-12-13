using MassTransit;
//Consumer �zerinden i�lem yap�laca�� i�in swaggerlar� falan Stock.API'dan sildik
var builder = WebApplication.CreateBuilder(args);

// MassTransit Konfig�rasyonu ayarlar�
builder.Services.AddMassTransit(configurator =>
{
    configurator.UsingRabbitMq((context, _configure) =>
    {
        _configure.Host(builder.Configuration["RabbitMQ"]);
    });
});

var app = builder.Build();


app.Run();
