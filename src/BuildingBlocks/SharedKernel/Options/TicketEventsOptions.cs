namespace SharedKernel.Options;

public sealed class TicketEventsOptions
{
    public bool UseKafka { get; set; }
    public bool UseAzureServiceBus { get; set; }

    public KafkaOptions Kafka { get; set; } = new();
    public AzureServiceBusOptions AzureServiceBus { get; set; } = new();

    public sealed class KafkaOptions
    {
        public string BootstrapServers { get; set; } = string.Empty;
        public string TicketSubmittedTopic { get; set; } = "ticket-submitted";
        public string GroupId { get; set; } = "default-consumer-group";
    }

    public sealed class AzureServiceBusOptions
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string TopicName { get; set; } = "ticket-submitted";
        public string SubscriptionName { get; set; } = "default-worker";
    }
}
