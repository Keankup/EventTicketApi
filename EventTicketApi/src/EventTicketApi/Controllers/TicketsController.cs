using EventTicketApi.Data;
using EventTicketApi.DTOs;
using EventTicketApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventTicketApi.Controllers;

[ApiController]
[Route("api/tickets")]
public class TicketsController : ControllerBase
{
    private readonly AppDbContext _context;

    public TicketsController(AppDbContext context)
    {
        _context = context;
    }

    // GET /api/tickets?status=Sold&eventId=1&pageNumber=1&pageSize=20
    [HttpGet]
    public async Task<ActionResult<PagedResult<TicketDto>>> GetTickets(
        [FromQuery] TicketQueryParameters query)
    {
        var q = _context.Tickets.AsQueryable();

        if (query.TicketId.HasValue)
            q = q.Where(t => t.TicketId == query.TicketId.Value);

        if (query.EventId.HasValue)
            q = q.Where(t => t.EventId == query.EventId.Value);

        if (query.TicketTypeId.HasValue)
            q = q.Where(t => t.TicketTypeId == query.TicketTypeId.Value);

        if (query.CustomerId.HasValue)
            q = q.Where(t => t.CustomerId == query.CustomerId.Value);

        if (!string.IsNullOrWhiteSpace(query.Status))
            q = q.Where(t => t.Status == query.Status);

        if (!string.IsNullOrWhiteSpace(query.SeatNumber))
            q = q.Where(t => t.SeatNumber != null && t.SeatNumber.Contains(query.SeatNumber));

        if (query.PriceFrom.HasValue)
            q = q.Where(t => t.Price >= query.PriceFrom.Value);

        if (query.PriceTo.HasValue)
            q = q.Where(t => t.Price <= query.PriceTo.Value);

        if (query.PurchaseDateFrom.HasValue)
            q = q.Where(t => t.PurchaseDate >= query.PurchaseDateFrom.Value);

        if (query.PurchaseDateTo.HasValue)
            q = q.Where(t => t.PurchaseDate <= query.PurchaseDateTo.Value);

        var totalCount = await q.CountAsync();

        var items = await q
            .OrderBy(t => t.TicketId)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(t => ToDto(t))
            .ToListAsync();

        return Ok(new PagedResult<TicketDto>
        {
            Items = items,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalCount = totalCount
        });
    }

    // GET /api/tickets/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TicketDto>> GetTicket(int id)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket is null)
            return NotFound();

        return Ok(ToDto(ticket));
    }

    // POST /api/tickets
    [HttpPost]
    public async Task<ActionResult<TicketDto>> CreateTicket(TicketCreateDto dto)
    {
        var ticketTypeExists = await _context.TicketTypes.AnyAsync(tt => tt.TicketTypeId == dto.TicketTypeId);
        if (!ticketTypeExists)
            return BadRequest($"Тип билета с id {dto.TicketTypeId} не найден.");

        var eventExists = await _context.Events.AnyAsync(e => e.EventId == dto.EventId);
        if (!eventExists)
            return BadRequest($"Мероприятие с id {dto.EventId} не найдено.");

        var ticket = new Ticket
        {
            TicketTypeId = dto.TicketTypeId,
            EventId = dto.EventId,
            SeatNumber = dto.SeatNumber,
            Price = dto.Price,
            Status = TicketStatuses.Available
        };

        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTicket), new { id = ticket.TicketId }, ToDto(ticket));
    }

    // PUT /api/tickets/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateTicket(int id, TicketUpdateDto dto)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket is null)
            return NotFound();

        if (!TicketStatuses.All.Contains(dto.Status))
            return BadRequest($"Недопустимый статус '{dto.Status}'. Допустимые значения: {string.Join(", ", TicketStatuses.All)}");

        if (dto.CustomerId.HasValue)
        {
            var customerExists = await _context.Customers.AnyAsync(c => c.CustomerId == dto.CustomerId.Value);
            if (!customerExists)
                return BadRequest($"Клиент с id {dto.CustomerId} не найден.");
        }

        if (dto.Status == TicketStatuses.Sold && (dto.CustomerId is null || dto.PurchaseDate is null))
            return BadRequest("Для статуса Sold обязательны CustomerId и PurchaseDate.");

        ticket.Status = dto.Status;
        ticket.CustomerId = dto.CustomerId;
        ticket.PurchaseDate = dto.PurchaseDate;
        ticket.Price = dto.Price;
        ticket.SeatNumber = dto.SeatNumber;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE /api/tickets/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteTicket(int id)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket is null)
            return NotFound();

        _context.Tickets.Remove(ticket);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static TicketDto ToDto(Ticket t) => new()
    {
        TicketId = t.TicketId,
        TicketTypeId = t.TicketTypeId,
        EventId = t.EventId,
        SeatNumber = t.SeatNumber,
        Status = t.Status,
        CustomerId = t.CustomerId,
        PurchaseDate = t.PurchaseDate,
        Price = t.Price
    };
}