using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using RestaurantOrderingProject.Models;

namespace RestaurantOrderingProject.Controllers
{
    public class OrderController : Controller
    {
        private readonly FoodOrderDbContext _context;

        public OrderController(FoodOrderDbContext context)
        {
            _context = context;
        }

        // GET: Order
        public async Task<IActionResult> Index(string searchString, string sortOrder, string statusFilter, int page = 1, int pageSize = 5)
        {
            var orders = _context.Orders
                .Include(o => o.User) 
                .AsQueryable();

            // Tìm theo tên user
            if (!string.IsNullOrEmpty(searchString))
            {
                orders = orders.Where(o => o.User.FullName.Contains(searchString));
            }

            // Filter theo trạng thái
            if (!string.IsNullOrEmpty(statusFilter))
            {
                orders = orders.Where(o => o.Status == statusFilter);
            }

            // Sort
            ViewData["DateSortParm"] = sortOrder == "date_asc" ? "date_desc" : "date_asc";
            ViewData["AmountSortParm"] = sortOrder == "amount_asc" ? "amount_desc" : "amount_asc";

            switch (sortOrder)
            {
                case "date_desc":
                    orders = orders.OrderByDescending(o => o.OrderDate);
                    break;
                case "date_asc":
                    orders = orders.OrderBy(o => o.OrderDate);
                    break;
                case "amount_desc":
                    orders = orders.OrderByDescending(o => o.TotalAmount);
                    break;
                case "amount_asc":
                    orders = orders.OrderBy(o => o.TotalAmount);
                    break;
                default:
                    orders = orders.OrderByDescending(o => o.OrderDate);
                    break;
            }

            // Phân trang
            int totalOrders = await orders.CountAsync();
            var ordersPaged = await orders.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.TotalPages = (int)Math.Ceiling(totalOrders / (double)pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.SearchString = searchString;
            ViewBag.StatusFilter = statusFilter;
            ViewBag.SortOrder = sortOrder;

            return View(ordersPaged);
        }

        // GET: Order/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Food)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // Xuất ra Excel
        public async Task<IActionResult> ExportExcel()
        {
            // Set license EPPlus (EPPlus 8+ bắt buộc)
           // ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var orders = await _context.Orders.Include(o => o.User).ToListAsync();

            using (var package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("Orders");
                ws.Cells[1, 1].Value = "OrderId";
                ws.Cells[1, 2].Value = "User";
                ws.Cells[1, 3].Value = "OrderDate";
                ws.Cells[1, 4].Value = "TotalAmount";
                ws.Cells[1, 5].Value = "Status";

                int row = 2;
                foreach (var o in orders)
                {
                    ws.Cells[row, 1].Value = o.OrderId;
                    ws.Cells[row, 2].Value = o.User?.FullName;
                    ws.Cells[row, 3].Value = o.OrderDate?.ToString("yyyy-MM-dd");
                    ws.Cells[row, 4].Value = o.TotalAmount;
                    ws.Cells[row, 5].Value = o.Status;
                    row++;
                }

                var stream = new MemoryStream(package.GetAsByteArray());
                return File(stream,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "Orders.xlsx");
            }
        }
        // GET: Order/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
                return NotFound();

            return View(order);
        }

        // POST: Order/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails) // load chi tiết để xóa trước
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order != null)
            {
                // xóa OrderDetails trước
                _context.OrderDetails.RemoveRange(order.OrderDetails);

                // xóa Order
                _context.Orders.Remove(order);

                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

    }
}
