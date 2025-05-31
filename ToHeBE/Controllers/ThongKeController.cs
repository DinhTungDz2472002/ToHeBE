using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToHeBE.Models;


namespace ToHeBE.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ThongKeController : ControllerBase
	{
		private readonly ToHeDbContext dbContext; // Replace with your actual DbContext name

		public ThongKeController(ToHeDbContext dbContext)
		{
			this.dbContext = dbContext;
		}


		[HttpGet("monthly-sales")]
		public async Task<IActionResult> GetMonthlySales([FromQuery] int year, [FromQuery] int month)
		{
			try
			{
				// Validate input
				if (year < 1900 || year > DateTime.Now.Year || month < 1 || month > 12)
				{
					return BadRequest(new { message = "Năm hoặc tháng không hợp lệ" });
				}

				// Get number of days in the selected month
				int daysInMonth = DateTime.DaysInMonth(year, month);
				var startDate = new DateTime(year, month, 1);
				var endDate = startDate.AddMonths(1);

				// Define status types
				var statusTypes = new[]
				{
			"Đã Giao",
			"Chờ Giao Hàng",
			"Chờ Xác Nhận",
			"Khách Muốn Hủy",
			"Đã Hủy"
		};

				// Initialize result arrays
				var labels = new string[daysInMonth];
				var datasets = new List<object>();
				var totalMonthlySales = new Dictionary<string, double>();
				var totalOrderCounts = new Dictionary<string, int>();

				// Query sales data for each status
				foreach (var status in statusTypes)
				{
					var dailySales = await dbContext.Thdbs
						.Where(hdb => hdb.NgayLapHdb != null
							&& hdb.TongTienHdb != null
							&& hdb.Status == status
							&& hdb.NgayLapHdb >= startDate
							&& hdb.NgayLapHdb < endDate)
						.GroupBy(hdb => hdb.NgayLapHdb!.Value.Date)
						.Select(g => new
						{
							Date = g.Key,
							TotalSales = g.Sum(hdb => hdb.TongTienHdb!.Value),
							OrderCount = g.Count()
						})
						.OrderBy(x => x.Date)
						.ToListAsync();

					// Calculate total sales and order count for this status
					totalMonthlySales[status] = dailySales.Sum(s => s.TotalSales);
					totalOrderCounts[status] = dailySales.Sum(s => s.OrderCount);

					// Create sales and order count data arrays for this status
					var dailyData = new object[daysInMonth];
					for (int i = 0; i < daysInMonth; i++)
					{
						var date = startDate.AddDays(i);
						if (status == statusTypes[0]) // Only set labels once
						{
							labels[i] = date.ToString("yyyy-MM-dd");
						}
						var sale = dailySales.FirstOrDefault(s => s.Date == date.Date);
						dailyData[i] = new
						{
							sales = sale?.TotalSales ?? 0,
							orders = sale?.OrderCount ?? 0
						};
					}

					// Define colors for each status
					var (backgroundColor, borderColor) = status switch
					{
						"Đã Giao" => ("rgba(54, 162, 235, 0.2)", "rgba(54, 162, 235, 1)"),
						"Chờ Giao Hàng" => ("rgba(255, 206, 86, 0.2)", "rgba(255, 206, 86, 1)"),
						"Chờ Xác Nhận" => ("rgba(75, 192, 192, 0.2)", "rgba(75, 192, 192, 1)"),
						"Khách Muốn Hủy" => ("rgba(255, 99, 132, 0.2)", "rgba(255, 99, 132, 1)"),
						"Đã Hủy" => ("rgba(153, 102, 255, 0.2)", "rgba(153, 102, 255, 1)"),
						_ => ("rgba(128, 128, 128, 0.2)", "rgba(128, 128, 128, 1)")
					};

					// Add dataset for this status
					datasets.Add(new
					{
						label = $"Thống kê {status} tháng {month}/{year}",
						data = dailyData,
						backgroundColor = backgroundColor,
						borderColor = borderColor,
						borderWidth = 1,
						fill = true
					});
				}

				var result = new
				{
					labels = labels,
					datasets = datasets.ToArray(),
					totalMonthlySales = totalMonthlySales,
					totalOrderCounts = totalOrderCounts
				};

				return Ok(result);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy dữ liệu thống kê", error = ex.Message });
			}
		}
	}
}
