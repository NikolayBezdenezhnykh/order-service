using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Infrastructure.KafkaConsumer
{
    public class KafkaConsumerHandler : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public KafkaConsumerHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Factory.StartNew(() => Start(stoppingToken), TaskCreationOptions.LongRunning);
        }

        private async Task Start(CancellationToken cancellationToken)
        {
            var kafkaConsumerConfig = _serviceProvider.GetRequiredService<IOptions<KafkaConsumerConfig>>().Value;

            var conf = new ConsumerConfig
            {
                GroupId = kafkaConsumerConfig.GroupId,
                BootstrapServers = kafkaConsumerConfig.BootstrapServers,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoOffsetStore = false,
                SaslMechanism = SaslMechanism.ScramSha256,
                SecurityProtocol = SecurityProtocol.SaslPlaintext,
                SaslUsername = kafkaConsumerConfig.UserName,
                SaslPassword = kafkaConsumerConfig.Password
            };

            using (var consumer = new ConsumerBuilder<Ignore, string>(conf).Build())
            {
                consumer.Subscribe(kafkaConsumerConfig.TopicNames);

                try
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            var consumeResult = consumer.Consume(cancellationToken);

                            using (var scope = _serviceProvider.CreateScope())
                            {
                                var handler = scope.ServiceProvider.GetRequiredService<IKafkaMessageHandlerFactory>();
                                await handler.GetKafkaMessageHandler(consumeResult.Topic).HandleMessageAsync(consumeResult.Message.Value);                                
                            }
                            try
                            {
                                consumer.StoreOffset(consumeResult);
                            }
                            catch (KafkaException e)
                            {
                                Console.WriteLine($"Store Offset error: {e.Error.Reason}");
                            }
                        }
                        catch (ConsumeException e)
                        {
                            Console.WriteLine($"Consume error: {e.Error.Reason}");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Handle message error: {e.Message}");
                        }

                        await Task.Delay(100, cancellationToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Closing consumer.");
                    consumer.Close();
                }
            }

            await Task.CompletedTask;
        }
    }
}
