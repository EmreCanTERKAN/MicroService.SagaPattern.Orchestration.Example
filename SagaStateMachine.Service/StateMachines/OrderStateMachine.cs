using MassTransit;
using SagaStateMachine.Service.StateInstances;
using Shared.Messages;
using Shared.OrderEvents;
using Shared.PaymentEvents;
using Shared.Settings;
using Shared.StockEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SagaStateMachine.Service.StateMachines
{
    public class OrderStateMachine : MassTransitStateMachine<OrderStateInstance>
    {
        //Her bir evente karşılık property olarak tanımlalamız gerekiyor
        public Event<OrderStartedEvent> OrderStartedEvent { get; set; }
        public Event<StockReservedEvent> StockReservedEvent { get; set; }
        public Event<PaymentCompletedEvent> PaymentCompletedEvent { get; set; }
        public Event<StockNotReservedEvent> StockNotReservedEvent { get; set; }
        public Event<PaymentFailedEvent> PaymentFailedEvent { get; set; }

        //5 tane evente karşılık 5 tane statei tanımladık.
        public State OrderCreated { get; set; }
        public State StockReserved { get; set; }
        public State StockNotReserved { get; set; }
        public State PaymentCompleted { get; set; }
        public State PaymentFailed { get; set; }


        public OrderStateMachine()
        {
            InstanceState(instance => instance.CurrentState);

            // 1. OrderStartedEvent olayını tanımlıyoruz.
            // Bu olay, sipariş başladığında state machine tarafından işlenecek.
            Event(() => OrderStartedEvent, config =>
            {
                // 2. Correlation (ilişkilendirme) kurallarını belirtiyoruz.
                // Gönderilen mesajın OrderId'si ile state instance'daki OrderId'yi eşleştiriyoruz.
                config.CorrelateBy<int>(
                    stateInstance => stateInstance.OrderId,     // State instance'dan alınan OrderId
                    eventMessage => eventMessage.Message.OrderId // Gelen olaydaki OrderId
                );

                // 3. Eğer state instance bulunamazsa, yeni bir ID ile yeni bir instance oluştur.
                config.SelectId(_ => Guid.NewGuid());
            });
            //StockReservedEvent fırlatıldığında veritabanındaki hangi correlationId değerine sahip state instace'in stateni değiştecek bunu belirtiyoruz.
            //Aynı Çalışmayı StockNotReservedEvent, StockNotReservedEvent, PaymentCompletedEvent ve PaymentFailedEvent için de yapıyoruz.
            //Dikkat ederseniz tetikleyici event dışındaki tüm eventler taşıdıkları korelasyon değeri ile eşleşen veritabanındaki State Instace satırı üzerinde işlem gerçekleştirmektedir/gerçekleştirecektir.
            //Çünkü State Instace oluşturmak sade ve sadece tetikleyici event’in sorumluluğundadır. Diğer eventler artık bu oluşturulmuş state instance üzerinden durum bilgisinin değişmesini sağlamaktadırlar.
            Event(() => StockReservedEvent, config =>
            {
                config.CorrelateById(
                    eventmessage => eventmessage.Message.CorrelationId
                    );

            });

            Event(() => PaymentCompletedEvent, config =>
            {
                config.CorrelateById(
                    eventmessage => eventmessage.Message.CorrelationId
                    );

            });

            Event(() => StockNotReservedEvent, config =>
            {
                config.CorrelateById(
                    eventmessage => eventmessage.Message.CorrelationId
                    );

            });

            Event(() => PaymentFailedEvent, config =>
            {
                config.CorrelateById(
                    eventmessage => eventmessage.Message.CorrelationId
                    );

            });


            //Tetikleyici event geldiğinde State Machine’de ilk karşılayıcı State Initially fonksiyonu tarafından tanımlanmış olan Initial olacaktır. Burada When fonksiyonu ile o anki gelen event’in OrderStartedEvent olduğu kontrol ediliyor. Ve eğer OrderStartedEvent ise Then fonksiyonu içerisinde gerekli işlemler gerçekleştiriyor.

            //Then fonksiyonunu içeriğine göz atarsanız eğer oluşturulacak olan State Instance’ın hangi property’sine tetikleyici event’e gelen hangi propertyilerin atanacağı belirtilmektedir. Ayrıca Then fonksiyonunun ihtiyaç doğrultusunda ara işlem olarak ifade edilen işlem parçacıkları olabileceğini de gözlemleyebilirsiniz.


            Initially(When(OrderStartedEvent)
                .Then(context =>
                {
                    context.Saga.OrderId = context.Message.OrderId;
                    context.Saga.BuyerId = context.Message.BuyerId;
                    context.Saga.TotalPrice = context.Message.TotalPrice;
                    context.Saga.CreatedDate = DateTime.UtcNow;
                })
                .TransitionTo(OrderCreated)
                .Send(new Uri($"queue:{RabbitMQSettings.Stock_OrderCreatedEventQueue}"),
                context => new OrderCreatedEvent(context.Saga.CorrelationId)
                {
                    OrderItems = context.Message.OrderItems,
                }));
            //Eğerki veri tabanında ilgili siparişin state'i OrderCreated ise ve gelen event stockereserve se stateini stock reservend demiş olduk. benzer mantık la stocknotreservedevent geldiğinde stateini stocknotreserved çekiyoruz.

            During(OrderCreated,
                When(StockReservedEvent)
                .TransitionTo(StockReserved)
                .Send(new Uri($"queue:{RabbitMQSettings.Payment_PaymentStartedEventQueue}"),
                context => new PaymentStartedEvent(context.Saga.CorrelationId)
                {
                    TotalPrice = context.Saga.TotalPrice,
                    OrderItems = context.Message.OrderItems,
                }),
                When(StockNotReservedEvent)
                .TransitionTo(StockNotReserved)
                .Send(new Uri($"queue:{RabbitMQSettings.Order_OrderFailedEventQueue}"),
                context => new OrderFailedEvent
                {
                    OrderId = context.Saga.OrderId,
                    Message = context.Message.Message
                }));

            During(StockReserved,
                When(PaymentCompletedEvent)
                .TransitionTo(PaymentCompleted)
                .Send(new Uri($"queue:{RabbitMQSettings.Order_OrderCompletedEventQueue}"),
                context => new OrderCompletedEvent
                {
                    OrderId = context.Saga.OrderId,

                })
                .Finalize(),
                When(PaymentFailedEvent)
                .TransitionTo(PaymentFailed)
                .Send(new Uri($"queue:{RabbitMQSettings.Order_OrderFailedEventQueue}"),
                context => new OrderFailedEvent
                {
                    OrderId = context.Saga.OrderId,
                    Message = context.Message.Message
                })
                .Send(new Uri($"queue:{RabbitMQSettings.Stock_RollbackMessageQueue}"),
                context => new StockRollbackMessage
                {
                    OrderItems = context.Message.OrderItems,
                }));
            SetCompletedWhenFinalized();
        }
    }
}
