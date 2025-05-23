using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToHeBE.Models;
using ToHeBE.Models.DTO;

namespace ToHeBE.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class SanPhamController : ControllerBase
	{
		private readonly ToHeDbContext dbContext;

		public SanPhamController(ToHeDbContext dbContext)
		{
			this.dbContext = dbContext;
		}
		// 1. Lấy danh sách sản phẩm có phân trang (chỉ lấy Status = true)[
		
		[HttpGet]
		[Route("/SanPham/GetList")]
		public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
		{
			if (pageNumber <= 0 || pageSize <= 0)
				return BadRequest("Số trang và kích thước trang phải lớn hơn 0.");

			var query = dbContext.Tsanphams.Where(sp => sp.Status);

			var totalItems = await query.CountAsync();
			var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

			var sps = await query
				.OrderByDescending(sp => sp.NgayThemSp) // Sắp xếp theo ngày thêm mới nhất trước
				.Skip((pageNumber - 1) * pageSize)
				
				.Take(pageSize)
				.ToListAsync();

			var dtoList = sps
				.Select(sp => new SanPhamDto
			{
				MaSanPham = sp.MaSanPham,
				TenSanPham = sp.TenSanPham,
				GiaSanPham = sp.GiaSanPham,
				MaLoai = sp.MaLoai,
				SLtonKho = sp.SLtonKho,
				AnhSp = sp.AnhSp,
				MoTaSp = sp.MoTaSp,
				NgayThemSp = sp.NgayThemSp,
				Status = sp.Status
			}).ToList();

			var response = new
			{
				CurrentPage = pageNumber,
				PageSize = pageSize,
				TotalItems = totalItems,
				TotalPages = totalPages,
				Items = dtoList
			};

			return Ok(response);
		}
		// Get product by ID with its details (ChiTietSp) for editing
        [HttpGet]
        [Route("/SanPham/GetById")]
        public async Task<IActionResult> GetById([FromQuery] int id)
        {
            // Fetch product with related ChiTietSp, including MaMau and MaCl navigation properties
            var sp = await dbContext.Tsanphams
                .Include(x => x.TchitietSps)
                    .ThenInclude(ct => ct.MaMauNavigation) // Include color details
                .Include(x => x.TchitietSps)
                    .ThenInclude(ct => ct.MaClNavigation) // Include material details
                .FirstOrDefaultAsync(x => x.MaSanPham == id && x.Status);

            if (sp == null)
                return NotFound("Không tìm thấy sản phẩm.");

            // Map to DTO
            var dto = new SanPhamDto
            {
                MaSanPham = sp.MaSanPham,
                TenSanPham = sp.TenSanPham,
                GiaSanPham = sp.GiaSanPham,
                MaLoai = sp.MaLoai,
                SLtonKho = sp.SLtonKho,
                AnhSp = sp.AnhSp,
                MoTaSp = sp.MoTaSp,
                NgayThemSp = sp.NgayThemSp,
                Status = sp.Status,
                ChiTietSps = sp.TchitietSps?.Select(ct => new ChiTietSpDto
                {
                    MaChiTietSp = ct.MaChiTietSp,
                    MaMau = ct.MaMau,
                    MaCl = ct.MaCl,
                    GiamGiaSp = ct.GiamGiaSp,
                    AnhChiTietSp = ct.AnhChiTietSp,
                    TenMau = ct.MaMauNavigation?.TenMau, // Optional: Include color name
                    TenCl = ct.MaClNavigation?.TenCl     // Optional: Include material name
                }).ToList() ?? new List<ChiTietSpDto>()
            };

            return Ok(dto);
        }

		// 2. Tìm kiếm sản phẩm
		[HttpPost]
		[Route("/SanPham/Search")]
		public async Task<IActionResult> Search([FromQuery] string s)
		{
			if (string.IsNullOrWhiteSpace(s))
				return BadRequest("Từ khóa tìm kiếm không hợp lệ.");

			bool isInt = int.TryParse(s, out int number);

			var sps = await dbContext.Tsanphams.Where(sp => sp.Status).ToListAsync(); ;

			var filtered = sps.Where(sp =>
				(isInt && sp.MaSanPham == number) ||
				(!string.IsNullOrEmpty(sp.TenSanPham) && sp.TenSanPham.ToLower().Contains(s.ToLower())) ||
				(!string.IsNullOrEmpty(sp.MoTaSp) && sp.MoTaSp.ToLower().Contains(s.ToLower()))
			).ToList();

			var dtoList = filtered.Select(sp => new SanPhamDto
			{
				MaSanPham = sp.MaSanPham,
				TenSanPham = sp.TenSanPham,
				GiaSanPham = sp.GiaSanPham,
				MaLoai = sp.MaLoai,
				SLtonKho = sp.SLtonKho,
				AnhSp = sp.AnhSp,
				MoTaSp = sp.MoTaSp,
				NgayThemSp = sp.NgayThemSp,
				Status = sp.Status
			}).ToList();

			return Ok(dtoList);
		}
		
		/*[HttpPost]
		[Route("/SanPham/Create")]
		public async Task<IActionResult> Create([FromForm] SanPhamDto dto)
		{
			try
			{
				// Kiểm tra dữ liệu đầu vào
				if (!ModelState.IsValid)
					return BadRequest(ModelState);

				string? imagePath = null;
				if (dto.File != null && dto.File.Length > 0)
				{
					// Đường dẫn thư mục lưu ảnh
					var uploadPath = @"D:\tohe_fe\to_he_fe\public\assets\images";
					if (!Directory.Exists(uploadPath))
						Directory.CreateDirectory(uploadPath);

					// Tạo tên file duy nhất
					var fileName = $"{Guid.NewGuid()}_{dto.File.FileName}";
					var filePath = Path.Combine(uploadPath, fileName);

					// Lưu file vào thư mục
					using (var stream = new FileStream(filePath, FileMode.Create))
					{
						await dto.File.CopyToAsync(stream);
					}

					// Lưu đường dẫn tương đối để lưu vào DB
					imagePath = $"{fileName}";
				}

				// Tạo đối tượng sản phẩm
				var sp = new Tsanpham
				{
					TenSanPham = dto.TenSanPham,
					GiaSanPham = dto.GiaSanPham,
					MaLoai = dto.MaLoai,
					SLtonKho = dto.SLtonKho,
					AnhSp = imagePath, // Lưu đường dẫn ảnh
					MoTaSp = dto.MoTaSp,
					NgayThemSp = dto.NgayThemSp ?? DateTime.Now,
					Status = true
				};

				// Thêm vào DB
				await dbContext.Tsanphams.AddAsync(sp);
				await dbContext.SaveChangesAsync();

				// Cập nhật DTO để trả về
				dto.MaSanPham = sp.MaSanPham;
				dto.AnhSp = sp.AnhSp;
				dto.Status = true;

				return Ok(dto);
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Lỗi server: {ex.Message}");
			}
		}
*/
		
		/*[HttpPut]
		[Route("/SanPham/Update")]
		public async Task<IActionResult> Update([FromForm] SanPhamDto dto)
		{
			try
			{
				var sp = await dbContext.Tsanphams.FirstOrDefaultAsync(x => x.MaSanPham == dto.MaSanPham && x.Status);
				if (sp == null)
					return NotFound("Không tìm thấy sản phẩm cần cập nhật.");

				string? imagePath = sp.AnhSp;
				if (dto.File != null && dto.File.Length > 0)
				{
					var uploadPath = @"D:\tohe_fe\to_he_fe\public\assets\images";
					if (!Directory.Exists(uploadPath))
						Directory.CreateDirectory(uploadPath);

					var fileName = $"{Guid.NewGuid()}_{dto.File.FileName}";
					var filePath = Path.Combine(uploadPath, fileName);

					using (var stream = new FileStream(filePath, FileMode.Create))
					{
						await dto.File.CopyToAsync(stream);
					}

					imagePath = fileName;
				}

				sp.TenSanPham = dto.TenSanPham;
				sp.GiaSanPham = dto.GiaSanPham;
				sp.MaLoai = dto.MaLoai;
				sp.SLtonKho = dto.SLtonKho;
				sp.AnhSp = imagePath;
				sp.MoTaSp = dto.MoTaSp;
				sp.NgayThemSp = dto.NgayThemSp ?? sp.NgayThemSp;
				sp.Status = dto.Status;

				await dbContext.SaveChangesAsync();

				dto.AnhSp = sp.AnhSp;
				return Ok(dto);
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Lỗi server: {ex.Message}");
			}
		}
*/
		// 6. "Xóa" sản phẩm (chuyển Status = false)
		[HttpDelete]
		[Route("/SanPham/Delete")]
		public async Task<IActionResult> Delete([FromQuery] int id)
		{
			var sp = await dbContext.Tsanphams.FirstOrDefaultAsync(x => x.MaSanPham == id && x.Status);
			if (sp == null)
				return NotFound("Không tìm thấy sản phẩm cần xóa.");

			sp.Status = false; // Chuyển trạng thái
			await dbContext.SaveChangesAsync();

			return Ok("Đã xóa sản phẩm thành công.");
		}
		[HttpPut]
		[Route("/SanPham/Update")]
		public async Task<IActionResult> Update([FromForm] SanPhamDto dto)
		{
			try
			{
				// Validate input
				if (!ModelState.IsValid)
					return BadRequest(ModelState);

				// Find product
				var sp = await dbContext.Tsanphams.FirstOrDefaultAsync(x => x.MaSanPham == dto.MaSanPham && x.Status);
				if (sp == null)
					return NotFound("Không tìm thấy sản phẩm cần cập nhật.");

				// Validate foreign keys
				if (!await dbContext.Tloais.AnyAsync(l => l.MaLoai == dto.MaLoai))
					return BadRequest("Loại sản phẩm không tồn tại.");

				foreach (var chiTiet in dto.ChiTietSps)
				{
					if (!await dbContext.Tmaus.AnyAsync(m => m.MaMau == chiTiet.MaMau))
						return BadRequest($"Màu sắc với ID {chiTiet.MaMau} không tồn tại.");
					if (!await dbContext.Tchatlieus.AnyAsync(c => c.MaCl == chiTiet.MaCl))
						return BadRequest($"Chất liệu với ID {chiTiet.MaCl} không tồn tại.");
				}

				// Handle main product image
				string? imagePath = sp.AnhSp;
				if (dto.File != null && dto.File.Length > 0)
				{
					var uploadPath = @"D:\tohe_fe\to_he_fe\public\assets\images";
					if (!Directory.Exists(uploadPath))
						Directory.CreateDirectory(uploadPath);

					var fileName = $"{Guid.NewGuid()}_{dto.File.FileName}";
					var filePath = Path.Combine(uploadPath, fileName);

					using (var stream = new FileStream(filePath, FileMode.Create))
					{
						await dto.File.CopyToAsync(stream);
					}
					imagePath = fileName;
				}

				// Update product
				sp.TenSanPham = dto.TenSanPham;
				sp.GiaSanPham = dto.GiaSanPham;
				sp.MaLoai = dto.MaLoai;
				sp.SLtonKho = dto.SLtonKho;
				sp.AnhSp = imagePath;
				sp.MoTaSp = dto.MoTaSp;
				sp.NgayThemSp = dto.NgayThemSp ?? sp.NgayThemSp;
				sp.Status = dto.Status;

				// Handle product details
				// Get existing details
				var existingDetails = await dbContext.TchitietSps
					.Where(c => c.MaSanPham == sp.MaSanPham)
					.ToListAsync();

				// Remove details not in the new DTO
				foreach (var existing in existingDetails)
				{
					if (!dto.ChiTietSps.Any(c => c.MaChiTietSp == existing.MaChiTietSp))
						dbContext.TchitietSps.Remove(existing);
				}

				// Add or update details
				foreach (var chiTietDto in dto.ChiTietSps)
				{
					string? detailImagePath = null;
					if (chiTietDto.AnhChiTietSpFile != null && chiTietDto.AnhChiTietSpFile.Length > 0)
					{
						var uploadPath = @"D:\tohe_fe\to_he_fe\public\assets\images";
						var fileName = $"{Guid.NewGuid()}_{chiTietDto.AnhChiTietSpFile.FileName}";
						var filePath = Path.Combine(uploadPath, fileName);

						using (var stream = new FileStream(filePath, FileMode.Create))
						{
							await chiTietDto.AnhChiTietSpFile.CopyToAsync(stream);
						}
						detailImagePath = fileName;
					}
					else
					{
						var existingDetail = existingDetails.FirstOrDefault(c => c.MaChiTietSp == chiTietDto.MaChiTietSp);
						detailImagePath = existingDetail?.AnhChiTietSp ?? chiTietDto.AnhChiTietSp;
					}

					var chiTiet = existingDetails.FirstOrDefault(c => c.MaChiTietSp == chiTietDto.MaChiTietSp);
					if (chiTiet == null)
					{
						// Add new detail
						chiTiet = new TchitietSp
						{
							MaSanPham = sp.MaSanPham,
							MaMau = chiTietDto.MaMau,
							MaCl = chiTietDto.MaCl,
							GiamGiaSp = chiTietDto.GiamGiaSp,
							AnhChiTietSp = detailImagePath
						};
						await dbContext.TchitietSps.AddAsync(chiTiet);
					}
					else
					{
						// Update existing detail
						chiTiet.MaMau = chiTietDto.MaMau;
						chiTiet.MaCl = chiTietDto.MaCl;
						chiTiet.GiamGiaSp = chiTietDto.GiamGiaSp;
						chiTiet.AnhChiTietSp = detailImagePath;
					}
				}

				await dbContext.SaveChangesAsync();

				// Update DTO to return
				dto.AnhSp = sp.AnhSp;
				foreach (var chiTietDto in dto.ChiTietSps)
				{
					var chiTiet = await dbContext.TchitietSps
						.FirstOrDefaultAsync(c => c.MaSanPham == sp.MaSanPham && c.MaChiTietSp == chiTietDto.MaChiTietSp);
					if (chiTiet != null)
					{
						chiTietDto.MaChiTietSp = chiTiet.MaChiTietSp;
						chiTietDto.AnhChiTietSp = chiTiet.AnhChiTietSp;
					}
				}

				return Ok(dto);
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Lỗi server: {ex.Message}");
			}
		}
		
		[HttpPost]
		[Route("/SanPham/Create")]
		public async Task<IActionResult> Create([FromForm] SanPhamDto dto)
		{
			try
			{
				// Validate input
				if (!ModelState.IsValid)
					return BadRequest(ModelState);

				// Validate foreign keys
				if (!await dbContext.Tloais.AnyAsync(l => l.MaLoai == dto.MaLoai))
					return BadRequest("Loại sản phẩm không tồn tại.");

				foreach (var chiTiet in dto.ChiTietSps)
				{
					if (!await dbContext.Tmaus.AnyAsync(m => m.MaMau == chiTiet.MaMau))
						return BadRequest($"Màu sắc với ID {chiTiet.MaMau} không tồn tại.");
					if (!await dbContext.Tchatlieus.AnyAsync(c => c.MaCl == chiTiet.MaCl))
						return BadRequest($"Chất liệu với ID {chiTiet.MaCl} không tồn tại.");
				}

				// Handle main product image
				string? imagePath = null;
				if (dto.File != null && dto.File.Length > 0)
				{
					var uploadPath = @"D:\tohe_fe\to_he_fe\public\assets\images";
					if (!Directory.Exists(uploadPath))
						Directory.CreateDirectory(uploadPath);

					var fileName = $"{Guid.NewGuid()}_{dto.File.FileName}";
					var filePath = Path.Combine(uploadPath, fileName);

					using (var stream = new FileStream(filePath, FileMode.Create))
					{
						await dto.File.CopyToAsync(stream);
					}
					imagePath = fileName;
				}

				// Create product
				var sp = new Tsanpham
				{
					TenSanPham = dto.TenSanPham,
					GiaSanPham = dto.GiaSanPham,
					MaLoai = dto.MaLoai,
					SLtonKho = dto.SLtonKho,
					AnhSp = imagePath,
					MoTaSp = dto.MoTaSp,
					NgayThemSp = dto.NgayThemSp ?? DateTime.Now,
					Status = true
				};

				await dbContext.Tsanphams.AddAsync(sp);
				await dbContext.SaveChangesAsync();

				// Handle product details
				foreach (var chiTietDto in dto.ChiTietSps)
				{
					string? detailImagePath = null;
					if (chiTietDto.AnhChiTietSpFile != null && chiTietDto.AnhChiTietSpFile.Length > 0)
					{
						var uploadPath = @"D:\tohe_fe\to_he_fe\public\assets\images";
						var fileName = $"{Guid.NewGuid()}_{chiTietDto.AnhChiTietSpFile.FileName}";
						var filePath = Path.Combine(uploadPath, fileName);

						using (var stream = new FileStream(filePath, FileMode.Create))
						{
							await chiTietDto.AnhChiTietSpFile.CopyToAsync(stream);
						}
						detailImagePath = fileName;
					}

					var chiTiet = new TchitietSp
					{
						MaSanPham = sp.MaSanPham,
						MaMau = chiTietDto.MaMau,
						MaCl = chiTietDto.MaCl,
						GiamGiaSp = chiTietDto.GiamGiaSp,
						AnhChiTietSp = detailImagePath
					};
					await dbContext.TchitietSps.AddAsync(chiTiet);
				}

				await dbContext.SaveChangesAsync();

				// Update DTO to return
				dto.MaSanPham = sp.MaSanPham;
				dto.AnhSp = sp.AnhSp;
				dto.Status = sp.Status;
				foreach (var chiTietDto in dto.ChiTietSps)
				{
					var chiTiet = await dbContext.TchitietSps
						.FirstOrDefaultAsync(c => c.MaSanPham == sp.MaSanPham && c.MaMau == chiTietDto.MaMau && c.MaCl == chiTietDto.MaCl);
					if (chiTiet != null)
						chiTietDto.MaChiTietSp = chiTiet.MaChiTietSp;
					chiTietDto.AnhChiTietSp = chiTiet?.AnhChiTietSp;
				}

				return Ok(dto);
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Lỗi server: {ex.Message}");
			}
		}

	}


}
