using MassTransit;
using MongoDB.Driver;
using Shared.OrderEvents;
using Shared.Settings;
using Shared.StockEvents;
using Stock.API.Models;
using Stock.API.Services;

namespace Stock.API.Consumers
{
    public class OrderCreatedEventConsumer(MongoDbService mongoDb, ISendEndpointProvider sendEndpointProvider) : IConsumer<OrderCreatedEvent>
    {
        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            //Her üründe stockların resultlarını tutacağımız listeyi tanımladık.
            List<bool> stockResults = new();
            //Veritabanındaki verileri bir değişkene atadık.
            var stockCollection = mongoDb.GetCollection<Models.Stock>();

            //Her bir ürün için foreachle dönüyoruz ve var mı yok mu kontrol ediyoruz
            foreach (var orderItem in context.Message.OrderItems)
            {
                //FindAsync kullanırken MongoDriver frameworkunde olan metodu kullanmamız gerekiyor.
               stockResults.Add( await (await stockCollection.FindAsync(s => s.ProductId == orderItem.ProductId && s.Count >= orderItem.Count)).AnyAsync());
            }
            // Eğer ki true olmaz ise geriye bir stocknotreserved eventi publish etmemiz gerekmektedir.
            //Burada stockResult içerisindeki bütün değerlerin true olup olmadığını değerlendiriyoruz.

            var sendEndPoint = await sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettings.StateMachineQueue}"));

            if (stockResults.TrueForAll(s=> s.Equals(true)))
            {
                foreach (var orderItem in context.Message.OrderItems)
                {
                    //Stoğa direk olarak erişiyoruz.
                  var stock =  await (await stockCollection.FindAsync(s => s.ProductId == orderItem.ProductId)).FirstOrDefaultAsync();
                    //Stock üzerinde güncelleme işlemi yapıyoruz.
                    stock.Count -= orderItem.Count;

                    //MongoDB'de veritabanı güncelleyelim.
                    await stockCollection.FindOneAndReplaceAsync(x => x.ProductId == orderItem.ProductId, stock);
                    //Bu işlemleri yaptıktan sonra stockmachine ' e başarılı olduğunu bildirmek zorundayız. Bunun için stockresulteventi stock machine göndercez. ISendProvider ile bu işlemi yapıcaz 
                }

                StockReservedEvent stockReservedEvent = new(context.Message.CorrelationId)
                {
                    OrderItems = context.Message.OrderItems,
                };
                await sendEndPoint.Send(stockReservedEvent);
            }
            else
            {
                StockNotReservedEvent stockNotReservedEvent = new(context.Message.CorrelationId)
                {
                    Message = "Stok Yetersiz."
                };
                await sendEndPoint.Send(stockNotReservedEvent);
            }
        }
    }
}
