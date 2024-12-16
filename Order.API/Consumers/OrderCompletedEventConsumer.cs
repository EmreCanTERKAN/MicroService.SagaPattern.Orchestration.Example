using MassTransit;
using Order.API.Context;
using Shared.OrderEvents;

namespace Order.API.Consumers
{
    public class OrderCompletedEventConsumer(OrderDbContext orderDbContext) : IConsumer<OrderCompletedEvent>
    {
        public async Task Consume(ConsumeContext<OrderCompletedEvent> context)
        {
           Models.Order order= await orderDbContext.Orders.FindAsync(context.Message.OrderId);
            if (order is not null)
            {
                order.OrderStatu = Enums.OrderStatus.Completed;
                await orderDbContext.SaveChangesAsync();
            }
            
        }
    }
}
