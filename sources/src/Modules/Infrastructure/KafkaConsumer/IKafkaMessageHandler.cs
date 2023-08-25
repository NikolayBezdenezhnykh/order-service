using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.KafkaConsumer
{
    public interface IKafkaMessageHandler
    {
        Task HandleMessageAsync(string message);
    }
}
