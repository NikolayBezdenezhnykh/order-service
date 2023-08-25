using Infrastructure.KafkaConsumer;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.KafkaMessageHandlers
{
    public class KafkaMessageHandlerFactory : IKafkaMessageHandlerFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public KafkaMessageHandlerFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IKafkaMessageHandler GetKafkaMessageHandler(string topic)
        {
            return topic switch
            {
                "invoice_paid" => _serviceProvider.GetRequiredService<KafkaMessageInvoicePaidHandler>(),
                _ => throw new NotImplementedException(),
            };
        }
    }
}
