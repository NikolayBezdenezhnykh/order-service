using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.KafkaConsumer
{
    public class KafkaConsumerConfig
    {
        public string BootstrapServers { get; set; }

        public List<string> TopicNames { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string GroupId { get; set; }
    }
}
