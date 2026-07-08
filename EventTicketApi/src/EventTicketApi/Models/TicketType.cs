using System.Net.Sockets;

namespace EventTicketApi.Models;

public class TicketType
{
    public int TicketTypeId { get; set; }
    public int EventId { get; set; }
    public Event? Event { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int TotalQuantity { get; set; }
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}