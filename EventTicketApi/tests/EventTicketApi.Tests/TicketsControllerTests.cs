using EventTicketApi.Controllers;
using EventTicketApi.DTOs;
using EventTicketApi.Models;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace EventTicketApi.Tests;

public class TicketsControllerTests
{
    [Fact]
    public async Task UpdateTicket_StatusSoldWithoutCustomerId_ReturnsBadRequest()
    {
        await using var context = TestDbContextFactory.Create();
        context.Tickets.Add(new Ticket
        {
            TicketId = 1,
            EventId = 1,
            TicketTypeId = 1,
            Status = TicketStatuses.Available,
            Price = 500
        });
        await context.SaveChangesAsync();

        var controller = new TicketsController(context);
        var dto = new TicketUpdateDto
        {
            Status = TicketStatuses.Sold,
            Price = 500,
            CustomerId = null,       
            PurchaseDate = null      
        };

        var result = await controller.UpdateTicket(1, dto);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdateTicket_InvalidStatus_ReturnsBadRequest()
    {
        await using var context = TestDbContextFactory.Create();
        context.Tickets.Add(new Ticket
        {
            TicketId = 1,
            EventId = 1,
            TicketTypeId = 1,
            Status = TicketStatuses.Available,
            Price = 500
        });
        await context.SaveChangesAsync();

        var controller = new TicketsController(context);
        var dto = new TicketUpdateDto { Status = "НесуществующийСтатус", Price = 500 };

        var result = await controller.UpdateTicket(1, dto);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreateTicket_NonExistentEvent_ReturnsBadRequest()
    {
        await using var context = TestDbContextFactory.Create();
        context.TicketTypes.Add(new TicketType
        {
            TicketTypeId = 1,
            EventId = 1,
            Name = "Standard",
            Price = 100,
            TotalQuantity = 10
        });
        await context.SaveChangesAsync();

        var controller = new TicketsController(context);
        var dto = new TicketCreateDto { TicketTypeId = 1, EventId = 999, Price = 100 };

        var result = await controller.CreateTicket(dto);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }
}