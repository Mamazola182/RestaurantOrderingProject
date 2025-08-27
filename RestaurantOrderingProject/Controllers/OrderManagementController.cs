using Microsoft.AspNetCore.Mvc;
using RestaurantOrderingProject.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using ClosedXML.Excel;
using Microsoft.AspNetCore.SignalR;
using SignalR.Hubs;

namespace RestaurantOrderingProject.Controllers
{
    public class OrderManagementController(RestaurantQrorderingContext _context, IHubContext<ChatHub> hubContext) : Controller
    {




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
        public async Task<IActionResult> LoadOrderListPartial()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(i => i.MenuItem)
                .ToListAsync();

            return PartialView("_OrderList", orders);
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
                await hubContext.Clients.All.SendAsync("DataChange");
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
                await hubContext.Clients.All.SendAsync("DataChange");
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
                await hubContext.Clients.All.SendAsync("DataChange");
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
                await hubContext.Clients.All.SendAsync("DataChange");
            }
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> ExportToExcel()
        {
            if (!IsAuthenticated()) return RedirectToAction("Index", "Login");

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Orders");

            // Header
            worksheet.Cell(1, 1).Value = "Order ID";
            worksheet.Cell(1, 2).Value = "Table";
            worksheet.Cell(1, 3).Value = "Order Time";
            worksheet.Cell(1, 4).Value = "Status";
            worksheet.Cell(1, 5).Value = "Total Money";
            worksheet.Cell(1, 6).Value = "Note";
            worksheet.Cell(1, 7).Value = "Is Take Away";
            worksheet.Cell(1, 8).Value = "Order Items";

            int row = 2;
            foreach (var order in orders)
            {
                var itemDescriptions = string.Join("; ", order.OrderItems.Select(oi =>
                    $"{oi.Quantity} x {oi.MenuItem?.Name} ({oi.UnitPrice:N0}) - {oi.Status}"
                ));

                worksheet.Cell(row, 1).Value = order.Id;
                worksheet.Cell(row, 2).Value = order.TableId;
                worksheet.Cell(row, 3).Value = order.OrderTime?.ToString("dd/MM/yyyy HH:mm");
                worksheet.Cell(row, 4).Value = order.Status;
                worksheet.Cell(row, 5).Value = order.TotalMoney ?? 0;
                worksheet.Cell(row, 6).Value = order.Note;
                worksheet.Cell(row, 7).Value = order.IsTakeAway == true ? "Yes" : "No";
                worksheet.Cell(row, 8).Value = itemDescriptions;
                row++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            string fileName = $"Orders_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}