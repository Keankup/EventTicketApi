using System.Data;
using EventTicketApi.Data;
using EventTicketApi.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventTicketApi.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ReportsController(AppDbContext context)
    {
        _context = context;
    }

    // GET /api/reports/orders-sum-on-birthday
    [HttpGet("orders-sum-on-birthday")]
    public async Task<ActionResult<List<OrdersSumOnBirthdayDto>>> GetOrdersSumOnBirthday()
    {
        var result = new List<OrdersSumOnBirthdayDto>();

        var connection = _context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM fn_orders_sum_on_birthday()";

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new OrdersSumOnBirthdayDto
            {
                CustomerId = reader.GetInt32(reader.GetOrdinal("customer_id")),
                FirstName = reader.GetString(reader.GetOrdinal("first_name")),
                LastName = reader.GetString(reader.GetOrdinal("last_name")),
                BirthDate = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("birth_date"))),
                OrdersCount = reader.GetInt64(reader.GetOrdinal("orders_count")),
                OrdersSum = reader.GetDecimal(reader.GetOrdinal("orders_sum"))
            });
        }

        return Ok(result);
    }

    // GET /api/reports/avg-check-by-hour
    [HttpGet("avg-check-by-hour")]
    public async Task<ActionResult<List<AvgCheckByHourDto>>> GetAvgCheckByHour()
    {
        var result = new List<AvgCheckByHourDto>();

        var connection = _context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM fn_avg_check_by_hour()";

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new AvgCheckByHourDto
            {
                HourOfDay = reader.GetInt32(reader.GetOrdinal("hour_of_day")),
                OrdersCount = reader.GetInt64(reader.GetOrdinal("orders_count")),
                AvgCheck = reader.GetDecimal(reader.GetOrdinal("avg_check"))
            });
        }

        return Ok(result);
    }
}