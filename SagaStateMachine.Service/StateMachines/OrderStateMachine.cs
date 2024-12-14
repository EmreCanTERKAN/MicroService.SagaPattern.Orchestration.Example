using MassTransit;
using SagaStateMachine.Service.StateInstances;
using Shared.OrderEvents;
using Shared.PaymentEvents;
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
            //Çünkü State Instace oluşturmak sade ve sadece tetikleyici event’inm sorumluluğundadır. Diğer eventler artık bu oluşturulmuş state instance üzerinden durum bilgisinin değişmesini sağlamaktadırlar.
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


        }
    }
}
