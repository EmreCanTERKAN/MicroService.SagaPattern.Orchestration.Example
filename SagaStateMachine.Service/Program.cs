using MassTransit;

var builder = Host.CreateApplicationBuilder(args);
//Workerdada Payment.API ve Stock.API'da olduðu gibi 
builder.Services.AddMassTransit(configurator =>
{
    configurator.UsingRabbitMq((context, _configure) =>
    {
        _configure.Host(builder.Configuration["RabbitMQ"]);
    });
});

var host = builder.Build();
host.Run();
