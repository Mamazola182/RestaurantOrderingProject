using Microsoft.AspNetCore.Mvc;
using RestaurantOrderingProject.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

namespace RestaurantOrderingProject.Controllers
{
    public class OrderManagementController : Controller
    {
        private readonly RestaurantQrorderingContext _context;

        public OrderManagementController(RestaurantQrorderingContext context)
        {
            _context = context;
        }

        // Check session để bảo vệ
        private bool IsAuthenticated()
        {
            return HttpContext.Session.GetString("AdminUsername") != null;
        }

        // List đơn hàng
        public async Task<IActionResult> Index()
        {
            if (!IsAuthenticated()) return RedirectToAction("Index", "Login");

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem) // Để hiển thị chi tiết món ăn
                .ToListAsync();

            return View(orders);
        }

        // Chấp nhận đơn hàng (POST) - Chỉnh sửa để đồng bộ Status OrderItem
        [HttpPost]
        public async Task<IActionResult> Accept(int id)
        {
            if (!IsAuthenticated()) return RedirectToAction("Index", "Login");

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order != null && order.Status == "Pending")
            {
                order.Status = "Accepted";
                foreach (var item in order.OrderItems)
                {
                    if (item.Status != "Rejected") // Nếu không phải Rejected thì set thành Accepted (bao gồm null hoặc trạng thái khác)
                    {
                        item.Status = "Accepted";
                    }
                }
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // Hoàn thành đơn hàng (POST) - Giữ nguyên logic như trước
        [HttpPost]
        public async Task<IActionResult> Complete(int id)
        {
            if (!IsAuthenticated()) return RedirectToAction("Index", "Login");

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order != null && order.Status == "Accepted")
            {
                order.Status = "Completed";
                foreach (var item in order.OrderItems)
                {
                    if (item.Status != "Rejected") // Không thay đổi nếu đã Rejected
                    {
                        item.Status = "Accepted";
                    }
                }
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // Từ chối đơn hàng (POST) - Giữ nguyên
        [HttpPost]
        public async Task<IActionResult> Reject(int id)
        {
            if (!IsAuthenticated()) return RedirectToAction("Index", "Login");

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order != null && order.Status == "Pending")
            {
                order.Status = "Rejected";
                foreach (var item in order.OrderItems)
                {
                    item.Status = "Rejected";
                }
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // Từ chối một OrderItem cụ thể (POST) - Giữ nguyên
        [HttpPost]
        public async Task<IActionResult> RejectItem(int itemId, string rejectionReason)
        {
            if (!IsAuthenticated()) return RedirectToAction("Index", "Login");

            var orderItem = await _context.OrderItems
                .Include(oi => oi.Order)
                .FirstOrDefaultAsync(oi => oi.Id == itemId);

            if (orderItem != null && orderItem.Order.Status != "Completed" && orderItem.Order.Status != "Rejected")
            {
                orderItem.Status = "Rejected";
                orderItem.RejectionReason = rejectionReason;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}