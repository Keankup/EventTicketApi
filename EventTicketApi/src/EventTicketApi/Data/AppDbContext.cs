using EventTicketApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace EventTicketApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Venue> Venues => Set<Venue>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<TicketType> TicketTypes => Set<TicketType>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Ticket> Tickets => Set<Ticket>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Venue>(e =>
        {
            e.ToTable("venues");
            e.HasKey(x => x.VenueId);
            e.Property(x => x.VenueId).HasColumnName("venue_id");
            e.Property(x => x.Name).HasColumnName("name");
            e.Property(x => x.Address).HasColumnName("address");
            e.Property(x => x.City).HasColumnName("city");
            e.Property(x => x.Capacity).HasColumnName("capacity");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<Event>(e =>
        {
            e.ToTable("events");
            e.HasKey(x => x.EventId);
            e.Property(x => x.EventId).HasColumnName("event_id");
            e.Property(x => x.Name).HasColumnName("name");
            e.Property(x => x.Description).HasColumnName("description");
            e.Property(x => x.VenueId).HasColumnName("venue_id");
            e.Property(x => x.Category).HasColumnName("category");
            e.Property(x => x.StartDateTime).HasColumnName("start_datetime");
            e.Property(x => x.EndDateTime).HasColumnName("end_datetime");
            e.Property(x => x.Status).HasColumnName("status");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");

            e.HasOne(x => x.Venue)
             .WithMany(v => v.Events)
             .HasForeignKey(x => x.VenueId);
        });

        modelBuilder.Entity<TicketType>(e =>
        {
            e.ToTable("ticket_types");
            e.HasKey(x => x.TicketTypeId);
            e.Property(x => x.TicketTypeId).HasColumnName("ticket_type_id");
            e.Property(x => x.EventId).HasColumnName("event_id");
            e.Property(x => x.Name).HasColumnName("name");
            e.Property(x => x.Price).HasColumnName("price").HasColumnType("numeric(10,2)");
            e.Property(x => x.TotalQuantity).HasColumnName("total_quantity");

            e.HasOne(x => x.Event)
             .WithMany(ev => ev.TicketTypes)
             .HasForeignKey(x => x.EventId);
        });

        modelBuilder.Entity<Customer>(e =>
        {
            e.ToTable("customers");
            e.HasKey(x => x.CustomerId);
            e.Property(x => x.CustomerId).HasColumnName("customer_id");
            e.Property(x => x.FirstName).HasColumnName("first_name");
            e.Property(x => x.LastName).HasColumnName("last_name");
            e.Property(x => x.BirthDate).HasColumnName("birth_date");
            e.Property(x => x.Email).HasColumnName("email");
            e.Property(x => x.Phone).HasColumnName("phone");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");

            e.HasIndex(x => x.Email).IsUnique();
        });

        modelBuilder.Entity<Ticket>(e =>
        {
            e.ToTable("tickets");
            e.HasKey(x => x.TicketId);
            e.Property(x => x.TicketId).HasColumnName("ticket_id");
            e.Property(x => x.TicketTypeId).HasColumnName("ticket_type_id");
            e.Property(x => x.EventId).HasColumnName("event_id");
            e.Property(x => x.SeatNumber).HasColumnName("seat_number");
            e.Property(x => x.Status).HasColumnName("status");
            e.Property(x => x.CustomerId).HasColumnName("customer_id");
            e.Property(x => x.PurchaseDate).HasColumnName("purchase_date");
            e.Property(x => x.Price).HasColumnName("price").HasColumnType("numeric(10,2)");

            e.HasOne(x => x.TicketType)
             .WithMany(tt => tt.Tickets)
             .HasForeignKey(x => x.TicketTypeId);

            e.HasOne(x => x.Event)
             .WithMany(ev => ev.Tickets)
             .HasForeignKey(x => x.EventId);

            e.HasOne(x => x.Customer)
             .WithMany(c => c.Tickets)
             .HasForeignKey(x => x.CustomerId)
             .IsRequired(false);
        });
    }
}