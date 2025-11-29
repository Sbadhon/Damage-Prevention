using System.Text.Json.Serialization;
using SchedulingSvc.Domain.WorkOrders;

public sealed class UpdateWorkOrderStatusRequest
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public WorkOrderStatus Status { get; set; }
}
