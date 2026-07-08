using System.ComponentModel.DataAnnotations;

namespace EventTicketApi.DTOs;

public class TicketDto
{
    public int TicketId { get; set; }
    public int TicketTypeId { get; set; }
    public int EventId { get; set; }
    public string? SeatNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? CustomerId { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public decimal Price { get; set; }
}

public class TicketCreateDto
{
    [Required]
    public int TicketTypeId { get; set; }

    [Required]
    public int EventId { get; set; }

    [MaxLength(20)]
    public string? SeatNumber { get; set; }

    [Required]
    public decimal Price { get; set; }
}

public class TicketUpdateDto
{
    [Required]
    public string Status { get; set; } = string.Empty;

    public int? CustomerId { get; set; }

    public DateTime? PurchaseDate { get; set; }

    [Required]
    public decimal Price { get; set; }

    [MaxLength(20)]
    public string? SeatNumber { get; set; }
}

public class TicketQueryParameters
{
    public int? TicketId { get; set; }
    public int? EventId { get; set; }
    public int? TicketTypeId { get; set; }
    public int? CustomerId { get; set; }
    public string? Status { get; set; }
    public string? SeatNumber { get; set; }
    public decimal? PriceFrom { get; set; }
    public decimal? PriceTo { get; set; }
    public DateTime? PurchaseDateFrom { get; set; }
    public DateTime? PurchaseDateTo { get; set; }

    private int _pageNumber = 1;
    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }

    private int _pageSize = 20;
    private const int MaxPageSize = 100;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value < 1 ? 1 : (value > MaxPageSize ? MaxPageSize : value);
    }
}