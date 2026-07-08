using EventTicketApi.Data;
using EventTicketApi.DTOs;
using EventTicketApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventTicketApi.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly AppDbContext _context;

    public EventsController(AppDbContext context)
    {
        _context = context;
    }

    // GET /api/events?category=Концерт&status=Planned&pageNumber=1&pageSize=10
    [HttpGet]
    public async Task<ActionResult<PagedResult<EventDto>>> GetEvents(
        [FromQuery] EventQueryParameters query)
    {
        var q = _context.Events.AsQueryable();

        if (query.EventId.HasValue)
            q = q.Where(e => e.EventId == query.EventId.Value);

        if (!string.IsNullOrWhiteSpace(query.Name))
            q = q.Where(e => EF.Functions.ILike(e.Name, $"%{query.Name}%"));

        if (query.VenueId.HasValue)
            q = q.Where(e => e.VenueId == query.VenueId.Value);

        if (!string.IsNullOrWhiteSpace(query.Category))
            q = q.Where(e => e.Category != null && EF.Functions.ILike(e.Category, $"%{query.Category}%"));

        if (!string.IsNullOrWhiteSpace(query.Status))
            q = q.Where(e => e.Status == query.Status);

        if (query.StartDateFrom.HasValue)
            q = q.Where(e => e.StartDateTime >= query.StartDateFrom.Value);

        if (query.StartDateTo.HasValue)
            q = q.Where(e => e.StartDateTime <= query.StartDateTo.Value);

        var totalCount = await q.CountAsync();

        var items = await q
            .OrderBy(e => e.EventId)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(e => ToDto(e))
            .ToListAsync();

        return Ok(new PagedResult<EventDto>
        {
            Items = items,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalCount = totalCount
        });
    }

    // GET /api/events/3
    [HttpGet("{id:int}")]
    public async Task<ActionResult<EventDto>> GetEvent(int id)
    {
        var ev = await _context.Events.FindAsync(id);
        if (ev is null)
            return NotFound();

        return Ok(ToDto(ev));
    }

    // POST /api/events
    [HttpPost]
    public async Task<ActionResult<EventDto>> CreateEvent(EventCreateDto dto)
    {
        if (dto.EndDateTime <= dto.StartDateTime)
            return BadRequest("Дата окончания должна быть позже даты начала.");

        var venueExists = await _context.Venues.AnyAsync(v => v.VenueId == dto.VenueId);
        if (!venueExists)
            return BadRequest($"Площадка с id {dto.VenueId} не найдена.");

        var ev = new Event
        {
            Name = dto.Name,
            Description = dto.Description,
            VenueId = dto.VenueId,
            Category = dto.Category,
            StartDateTime = dto.StartDateTime,
            EndDateTime = dto.EndDateTime,
            Status = EventStatuses.Planned,
            CreatedAt = DateTime.UtcNow
        };

        _context.Events.Add(ev);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetEvent), new { id = ev.EventId }, ToDto(ev));
    }

    // PUT /api/events/3
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateEvent(int id, EventUpdateDto dto)
    {
        var ev = await _context.Events.FindAsync(id);
        if (ev is null)
            return NotFound();

        if (dto.EndDateTime <= dto.StartDateTime)
            return BadRequest("Дата окончания должна быть позже даты начала.");

        if (!EventStatuses.All.Contains(dto.Status))
            return BadRequest($"Недопустимый статус '{dto.Status}'. Допустимые значения: {string.Join(", ", EventStatuses.All)}");

        var venueExists = await _context.Venues.AnyAsync(v => v.VenueId == dto.VenueId);
        if (!venueExists)
            return BadRequest($"Площадка с id {dto.VenueId} не найдена.");

        ev.Name = dto.Name;
        ev.Description = dto.Description;
        ev.VenueId = dto.VenueId;
        ev.Category = dto.Category;
        ev.StartDateTime = dto.StartDateTime;
        ev.EndDateTime = dto.EndDateTime;
        ev.Status = dto.Status;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE /api/events/3
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var ev = await _context.Events.FindAsync(id);
        if (ev is null)
            return NotFound();

        var hasNonAvailableTickets = await _context.Tickets
            .AnyAsync(t => t.EventId == id && t.Status != TicketStatuses.Available);
        if (hasNonAvailableTickets)
            return Conflict("Нельзя удалить мероприятие, у которого есть проданные или забронированные билеты.");

        _context.Events.Remove(ev);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static EventDto ToDto(Event e) => new()
    {
        EventId = e.EventId,
        Name = e.Name,
        Description = e.Description,
        VenueId = e.VenueId,
        Category = e.Category,
        StartDateTime = e.StartDateTime,
        EndDateTime = e.EndDateTime,
        Status = e.Status
    };
}