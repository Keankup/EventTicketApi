namespace EventTicketApi.DTOs;

// Результат fn_orders_sum_on_birthday()
public class OrdersSumOnBirthdayDto
{
    public int CustomerId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly BirthDate { get; set; }
    public long OrdersCount { get; set; }
    public decimal OrdersSum { get; set; }
}

// Результат fn_avg_check_by_hour()
public class AvgCheckByHourDto
{
    public int HourOfDay { get; set; }
    public long OrdersCount { get; set; }
    public decimal AvgCheck { get; set; }
}