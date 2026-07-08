using EventTicketApi.Data;
using EventTicketApi.DTOs;
using EventTicketApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventTicketApi.Controllers;

[ApiController]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    private readonly AppDbContext _context;

    public CustomersController(AppDbContext context)
    {
        _context = context;
    }

    // GET /api/customers?lastName=Петрова&pageNumber=1&pageSize=10
    [HttpGet]
    public async Task<ActionResult<PagedResult<CustomerDto>>> GetCustomers(
        [FromQuery] CustomerQueryParameters query)
    {
        var q = _context.Customers.AsQueryable();

        if (query.CustomerId.HasValue)
            q = q.Where(c => c.CustomerId == query.CustomerId.Value);

        if (!string.IsNullOrWhiteSpace(query.FirstName))
            q = q.Where(c => EF.Functions.ILike(c.FirstName, $"%{query.FirstName}%"));

        if (!string.IsNullOrWhiteSpace(query.LastName))
            q = q.Where(c => EF.Functions.ILike(c.LastName, $"%{query.LastName}%"));

        if (query.BirthDateFrom.HasValue)
            q = q.Where(c => c.BirthDate >= query.BirthDateFrom.Value);

        if (query.BirthDateTo.HasValue)
            q = q.Where(c => c.BirthDate <= query.BirthDateTo.Value);

        if (!string.IsNullOrWhiteSpace(query.Email))
            q = q.Where(c => EF.Functions.ILike(c.Email, $"%{query.Email}%"));

        if (!string.IsNullOrWhiteSpace(query.Phone))
            q = q.Where(c => c.Phone != null && c.Phone.Contains(query.Phone));

        var totalCount = await q.CountAsync();

        var items = await q
            .OrderBy(c => c.CustomerId)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(c => ToDto(c))
            .ToListAsync();

        return Ok(new PagedResult<CustomerDto>
        {
            Items = items,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalCount = totalCount
        });
    }

    // GET /api/customers/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CustomerDto>> GetCustomer(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer is null)
            return NotFound();

        return Ok(ToDto(customer));
    }

    // POST /api/customers
    [HttpPost]
    public async Task<ActionResult<CustomerDto>> CreateCustomer(CustomerCreateDto dto)
    {
        var emailExists = await _context.Customers.AnyAsync(c => c.Email == dto.Email);
        if (emailExists)
            return Conflict($"Клиент с email '{dto.Email}' уже существует.");

        var customer = new Customer
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            BirthDate = dto.BirthDate,
            Email = dto.Email,
            Phone = dto.Phone,
            CreatedAt = DateTime.UtcNow
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCustomer), new { id = customer.CustomerId }, ToDto(customer));
    }

    // PUT /api/customers/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCustomer(int id, CustomerUpdateDto dto)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer is null)
            return NotFound();

        var emailTakenByAnother = await _context.Customers
            .AnyAsync(c => c.Email == dto.Email && c.CustomerId != id);
        if (emailTakenByAnother)
            return Conflict($"Email '{dto.Email}' уже используется другим клиентом.");

        customer.FirstName = dto.FirstName;
        customer.LastName = dto.LastName;
        customer.BirthDate = dto.BirthDate;
        customer.Email = dto.Email;
        customer.Phone = dto.Phone;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE /api/customers/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer is null)
            return NotFound();

        var hasTickets = await _context.Tickets.AnyAsync(t => t.CustomerId == id);
        if (hasTickets)
            return Conflict("Нельзя удалить клиента, у которого есть билеты.");

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static CustomerDto ToDto(Customer c) => new()
    {
        CustomerId = c.CustomerId,
        FirstName = c.FirstName,
        LastName = c.LastName,
        BirthDate = c.BirthDate,
        Email = c.Email,
        Phone = c.Phone
    };
}