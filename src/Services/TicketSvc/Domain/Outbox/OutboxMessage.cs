namespace TicketSvc.Domain.Outbox;

public enum OutboxMessageStatus
{
    Pending = 0,
    Processing = 1,
    Published = 2,
    Failed = 3
}

public sealed class OutboxMessage
{
    public Guid Id { get; private set; }
    public Guid TicketId { get; private set; }
    public string TenantId { get; private set; } = default!;
    public string Type { get; private set; } = default!;
    public string Payload { get; private set; } = default!;
    public DateTimeOffset OccurredAt { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }
    public OutboxMessageStatus Status { get; private set; }
    public string? Error { get; private set; }

    private OutboxMessage() { }

    private OutboxMessage(
        Guid id,
        Guid ticketId,
        string tenantId,
        string type,
        string payload,
        DateTimeOffset occurredAt)
    {
        Id = id;
        TicketId = ticketId;
        TenantId = tenantId;
        Type = type;
        Payload = payload;
        OccurredAt = occurredAt;
        Status = OutboxMessageStatus.Pending;
    }

    public static OutboxMessage Create(
        Guid ticketId,
        string tenantId,
        string type,
        string payload,
        DateTimeOffset occurredAt)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("TenantId is required.", nameof(tenantId));

        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Type is required.", nameof(type));

        if (string.IsNullOrWhiteSpace(payload))
            throw new ArgumentException("Payload is required.", nameof(payload));

        return new OutboxMessage(
            Guid.NewGuid(),
            ticketId,
            tenantId,
            type,
            payload,
            occurredAt);
    }

    public void MarkProcessing()
    {
        if (Status != OutboxMessageStatus.Pending)
            return;

        Status = OutboxMessageStatus.Processing;
    }

    public void MarkPublished(DateTimeOffset processedAt)
    {
        Status = OutboxMessageStatus.Published;
        ProcessedAt = processedAt;
        Error = null;
    }

    public void MarkFailed(string error, DateTimeOffset processedAt)
    {
        Status = OutboxMessageStatus.Failed;
        ProcessedAt = processedAt;
        Error = error;
    }
}
