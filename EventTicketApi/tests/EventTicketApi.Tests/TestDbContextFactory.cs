using EventTicketApi.Data;
using Microsoft.EntityFrameworkCore;

namespace EventTicketApi.Tests;

public static class TestDbContextFactory
{

    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}