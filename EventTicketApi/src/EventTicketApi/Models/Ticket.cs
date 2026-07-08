namespace EventTicketApi.Models;

public static class TicketStatuses
{
    public const string Available = "Available";
    public const string Reserved = "Reserved";
    public const string Sold = "Sold";
    public const string Cancelled = "Cancelled";

    public static readonly string[] All = { Available, Reserved, Sold, Cancelled };
}

public class Ticket
{
    public int TicketId { get; set; }
    public int TicketTypeId { get; set; }
    public TicketType? TicketType { get; set; }
    public int EventId { get; set; }
    public Event? Event { get; set; }
    public string? SeatNumber { get; set; }
    public string Status { get; set; } = TicketStatuses.Available;
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public decimal Price { get; set; }
}