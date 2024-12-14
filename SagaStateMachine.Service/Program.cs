using MassTransit;
using Microsoft.EntityFrameworkCore;
using SagaStateMachine.Service.StateDbContexts;
using SagaStateMachine.Service.StateInstances;
using SagaStateMachine.Service.StateMachines;

var builder = Host.CreateApplicationBuilder(args);
//Workerdada Payment.API ve Stock.API'da olduðu gibi 
builder.Services.AddMassTransit(configurator =>
{
    //Masstransitte bu yapýlandýrmayý yapmamýz için AddSagaStateMachine generic olarak bir statemachine ardýndan da T parametresi olarak bir stateInstance istiyor.
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
