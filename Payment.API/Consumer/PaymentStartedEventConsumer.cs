using MassTransit;
using Shared.PaymentEvents;
using Shared.Settings;

namespace Payment.API.Consumer
{
    public class PaymentStartedEventConsumer(ISendEndpointProvider sendEndpointProvider) : IConsumer<PaymentStartedEvent>
    {
        public async Task Consume(ConsumeContext<PaymentStartedEvent> context)
        {

            var sendEndPoint = await sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettings.StateMachineQueue}"));
            // Burada detaylı bir ödeme işlemi yapmayacağız..
            if (true)
            {
                //Ödeme başarılıysa
                PaymentCompletedEvent paymentCompletedEvent = new(context.Message.CorrelationId)
                {
                     
                };

               await sendEndPoint.Send(paymentCompletedEvent);
            }
            else
            {
                PaymentFailedEvent paymentFailedEvent = new(context.Message.CorrelationId)
                {
                    // Burada orderItemslar önemli çünkü state machine da transaction işlemlerini ödeme yetersiz olduğunda baştan tekrardan yapacağımız için bunu önemsememiz gerekmektedir. 
                    OrderItems = context.Message.OrderItems,
                    Message = "Ödeme işlemi başarısız."
                };

                await sendEndPoint.Send(paymentFailedEvent);
            }
        }
    }
}
