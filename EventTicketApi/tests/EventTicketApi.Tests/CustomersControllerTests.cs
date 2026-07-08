using EventTicketApi.Controllers;
using EventTicketApi.DTOs;
using EventTicketApi.Models;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace EventTicketApi.Tests;

public class CustomersControllerTests
{
    [Fact]
    public async Task CreateCustomer_ValidData_ReturnsCreatedWithCustomer()
    {
        await using var context = TestDbContextFactory.Create();
        var controller = new CustomersController(context);
        var dto = new CustomerCreateDto
        {
            FirstName = "Иван",
            LastName = "Иванов",
            BirthDate = new DateOnly(1990, 3, 14),
            Email = "ivan@test.com",
            Phone = "+79001112233"
        };

        var result = await controller.CreateCustomer(dto);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var createdCustomer = Assert.IsType<CustomerDto>(createdResult.Value);
        Assert.Equal("Иван", createdCustomer.FirstName);
        Assert.Equal("ivan@test.com", createdCustomer.Email);
    }

    [Fact]
    public async Task CreateCustomer_DuplicateEmail_ReturnsConflict()
    {
        await using var context = TestDbContextFactory.Create();
        context.Customers.Add(new Customer
        {
            FirstName = "Существующий",
            LastName = "Клиент",
            BirthDate = new DateOnly(1990, 1, 1),
            Email = "duplicate@test.com"
        });
        await context.SaveChangesAsync();

        var controller = new CustomersController(context);
        var dto = new CustomerCreateDto
        {
            FirstName = "Новый",
            LastName = "Клиент",
            BirthDate = new DateOnly(1995, 5, 5),
            Email = "duplicate@test.com" 
        };

        var result = await controller.CreateCustomer(dto);

        Assert.IsType<ConflictObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetCustomer_NonExistentId_ReturnsNotFound()
    {
        await using var context = TestDbContextFactory.Create();
        var controller = new CustomersController(context);

        var result = await controller.GetCustomer(999);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task DeleteCustomer_WithExistingTickets_ReturnsConflict()
    {
        await using var context = TestDbContextFactory.Create();

        var customer = new Customer
        {
            FirstName = "Тест",
            LastName = "Клиент",
            BirthDate = new DateOnly(1990, 1, 1),
            Email = "test@test.com"
        };
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        context.Tickets.Add(new Ticket
        {
            CustomerId = customer.CustomerId,
            Status = TicketStatuses.Sold,
            Price = 100,
            EventId = 1,
            TicketTypeId = 1
        });
        await context.SaveChangesAsync();

        var controller = new CustomersController(context);
        var result = await controller.DeleteCustomer(customer.CustomerId);

        Assert.IsType<ConflictObjectResult>(result);
    }
}