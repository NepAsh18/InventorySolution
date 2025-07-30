// Notification.cs
public class Notification
{
    public int Id { get; set; }
    public string Message { get; set; }
    public string Type { get; set; } // "OrderPlaced", "OrderCancelled", "NewCustomer"
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public bool IsRead { get; set; } = false;
    public int? RelatedEntityId { get; set; } // OrderId or UserId
    public string UserId { get; set; } // For admin notifications
}