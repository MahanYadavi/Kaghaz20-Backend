namespace PaperSite.Application.DTOs.Admin;

public class DashboardStatisticsDto
{
    public int TotalUsers { get; set; }
    public int TotalOrders { get; set; }
    public int TotalProducts { get; set; }
    public decimal TotalRevenue { get; set; }
}
