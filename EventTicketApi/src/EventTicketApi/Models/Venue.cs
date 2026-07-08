using Microsoft.Extensions.Logging;

namespace EventTicketApi.Models;

public class Venue
{
    public int VenueId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string City { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<Event> Events { get; set; } = new List<Event>();
}