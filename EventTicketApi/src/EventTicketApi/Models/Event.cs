using System.Net.Sockets;

namespace EventTicketApi.Models;

public static class EventStatuses
{
    public const string Planned = "Planned";
    public const string Ongoing = "Ongoing";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";

    public static readonly string[] All = { Planned, Ongoing, Completed, Cancelled };
}

public class Event
{
    public int EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int VenueId { get; set; }
    public Venue? Venue { get; set; }
    public string? Category { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public string Status { get; set; } = EventStatuses.Planned;
    public DateTime CreatedAt { get; set; }
    public ICollection<TicketType> TicketTypes { get; set; } = new List<TicketType>();
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}