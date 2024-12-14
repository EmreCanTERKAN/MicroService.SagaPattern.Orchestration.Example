using MassTransit;
using Microsoft.EntityFrameworkCore;
using SagaStateMachine.Service.StateDbContexts;
using SagaStateMachine.Service.StateInstances;
using SagaStateMachine.Service.StateMachines;

var builder = Host.CreateApplicationBuilder(args);
//Workerdada Payment.API ve Stock.API'da oldu�u gibi 
builder.Services.AddMassTransit(configurator =>
{
    //Masstransitte bu yap�land�rmay� yapmam�z i�in AddSagaStateMachine generic olarak bir statemachine ard�ndan da T parametresi olarak bir stateInstance istiyor.
    configurator.AddSagaStateMachine<OrderStateMachine, OrderStateInstance>().EntityFrameworkRepository(options =>
    {
        options.AddDbContext<DbContext, OrderStateDbContext>((provider, _builder) =>
        {
            _builder.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
        });
    });
    configurator.UsingRabbitMq((context, _configure) =>
    {
        _configure.Host(builder.Configuration["RabbitMQ"]);
    });
});

var host = builder.Build();
host.Run();
