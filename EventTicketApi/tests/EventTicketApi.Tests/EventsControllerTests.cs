using EventTicketApi.Controllers;
using EventTicketApi.DTOs;
using EventTicketApi.Models;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace EventTicketApi.Tests;

public class EventsControllerTests
{
    [Fact]
    public async Task CreateEvent_EndDateBeforeStartDate_ReturnsBadRequest()
    {
        await using var context = TestDbContextFactory.Create();
        context.Venues.Add(new Venue { VenueId = 1, Name = "Тест", City = "Брянск", Capacity = 100 });
        await context.SaveChangesAsync();

        var controller = new EventsController(context);
        var dto = new EventCreateDto
        {
            Name = "Некорректное мероприятие",
            VenueId = 1,
            StartDateTime = new DateTime(2026, 8, 1, 20, 0, 0),
            EndDateTime = new DateTime(2026, 8, 1, 18, 0, 0) 
        };

        var result = await controller.CreateEvent(dto);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateEvent_ValidData_DefaultsToPlannedStatus()
    {
        await using var context = TestDbContextFactory.Create();
        context.Venues.Add(new Venue { VenueId = 1, Name = "Тест", City = "Брянск", Capacity = 100 });
        await context.SaveChangesAsync();

        var controller = new EventsController(context);
        var dto = new EventCreateDto
        {
            Name = "Концерт",
            VenueId = 1,
            StartDateTime = new DateTime(2026, 8, 1, 18, 0, 0),
            EndDateTime = new DateTime(2026, 8, 1, 20, 0, 0)
        };

        var result = await controller.CreateEvent(dto);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var createdEvent = Assert.IsType<EventDto>(createdResult.Value);
        Assert.Equal(EventStatuses.Planned, createdEvent.Status);
    }
}