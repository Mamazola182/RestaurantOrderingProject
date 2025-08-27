using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RestaurantOrderingProject.Models;
using RestaurantOrderingProject.ViewModels;
using SignalR.Hubs;

namespace RestaurantOrderingProject.Controllers
{
    public class OrderController(RestaurantQrorderingContext _context, IHubContext<ChatHub> hubContext) : Controller
    {
        
        public IActionResult Index(string token)
        {
            if (string.IsNullOrEmpty(token))
                return BadRequest(new { success = false, message = "Thiếu mã bàn." });

            try
            {
                string decoded = Encoding.UTF8.GetString(Convert.FromBase64String(token));
                if (!decoded.StartsWith("table-"))
                    return BadRequest(new { success = false, message = "Token không hợp lệ." });

                int tableId = int.Parse(decoded.Replace("table-", ""));

                var menuItems = _context.MenuItems
                    .Where(m => m.IsAvailable)
                    .Include(m => m.Category)
                    .ToList() ?? new List<MenuItem>();
                var categories = _context.Categories.ToList() ?? new List<Category>();

                var model = new OrderViewModel
                {
                    TableId = tableId,
                    MenuItems = menuItems,
                    Categories = categories
                };

                ViewBag.tokenQr = token;
                return View(model);
            }
            catch (FormatException)
            {
                return BadRequest(new { success = false, message = "Token không đúng định dạng." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = $"Token lỗi: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SubmitOrder([FromBody] OrderRequestModel orderRequest)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Kiểm tra cơ bản
                if (orderRequest == null)
                {
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
                }

                if (orderRequest.TableId <= 0)
                {
                    return Json(new { success = false, message = "Bàn không hợp lệ" });
                }

                if (orderRequest.Items == null || orderRequest.Items.Count == 0)
                {
                    return Json(new { success = false, message = "Giỏ hàng trống" });
                }

                // 2. Kiểm tra bàn có tồn tại không (nếu có bảng Tables)
                // var table = await _context.Tables.FindAsync(orderRequest.TableId);
                // if (table == null)
                // {
                //     return Json(new { success = false, message = "Bàn không tồn tại" });
                // }

                // 3. Kiểm tra tính hợp lệ của các món
                var menuItemIds = orderRequest.Items.Select(i => i.MenuItemId).ToList();
                var validMenuItems = await _context.MenuItems
                    .Where(m => menuItemIds.Contains(m.Id) && m.IsAvailable)
                    .ToDictionaryAsync(m => m.Id, m => m);

                if (validMenuItems.Count != menuItemIds.Distinct().Count())
                {
                    return Json(new { success = false, message = "Một số món không có sẵn hoặc không tồn tại" });
                }

                // 4. Tính toán lại tổng tiền để đảm bảo chính xác
                decimal calculatedTotal = 0;
                foreach (var item in orderRequest.Items)
                {
                    if (validMenuItems.TryGetValue(item.MenuItemId, out var menuItem))
                    {
                        calculatedTotal += menuItem.Price * item.Quantity;
                    }
                }

                // Kiểm tra tổng tiền gửi lên có khớp không (cho phép sai số nhỏ)
                if (Math.Abs(calculatedTotal - orderRequest.TotalAmount) > 0.01m)
                {
                    return Json(new { success = false, message = "Tổng tiền không chính xác" });
                }

                // 5. Tạo Order mới
                var newOrder = new Order
                {
                    TableId = orderRequest.TableId,
                    OrderTime = DateTime.Now,
                    Status = "Pending", // Trạng thái mặc định khi tạo đơn
                    Note = orderRequest.Notes?.Trim(),
                    TotalMoney = calculatedTotal,
                    IsTakeAway = orderRequest.IsTakeaway
                };

                _context.Orders.Add(newOrder);
                await _context.SaveChangesAsync();

                // 6. Tạo các OrderItems
                var orderItems = new List<OrderItem>();
                foreach (var item in orderRequest.Items)
                {
                    if (validMenuItems.TryGetValue(item.MenuItemId, out var menuItem))
                    {
                        var orderItem = new OrderItem
                        {
                            OrderId = newOrder.Id,
                            MenuItemId = item.MenuItemId,
                            Quantity = item.Quantity,
                            UnitPrice = menuItem.Price, // Dùng giá từ database để đảm bảo chính xác
                            RejectionReason = null, // Mặc định null khi tạo mới
                            Status = "Pending" // Thêm trạng thái mặc định cho OrderItem nếu cần
                        };
                        orderItems.Add(orderItem);
                    }
                }

                _context.OrderItems.AddRange(orderItems);
                await _context.SaveChangesAsync();
                await hubContext.Clients.All.SendAsync("DataChange");
                // 7. Commit transaction
                await transaction.CommitAsync();

                // 8. Log thông tin đặt hàng thành công
                Console.WriteLine("=== ĐẶT HÀNG THÀNH CÔNG ===");
                Console.WriteLine($"Order ID: {newOrder.Id}");
                Console.WriteLine($"Bàn số: {newOrder.TableId}");
                Console.WriteLine($"Tổng tiền: {newOrder.TotalMoney:N0} VNĐ");
                Console.WriteLine($"Mang về: {(newOrder.IsTakeAway.GetValueOrDefault() ? "Có" : "Không")}");
                Console.WriteLine($"Ghi chú: {newOrder.Note ?? "Không có"}");
                Console.WriteLine($"Mang về: {newOrder.IsTakeAway}");
                Console.WriteLine($"Số món: {orderItems.Count}");
                Console.WriteLine($"Thời gian: {newOrder.OrderTime:dd/MM/yyyy HH:mm:ss}");

                foreach (var item in orderItems)
                {
                    var menuItem = validMenuItems[item.MenuItemId];
                    Console.WriteLine($"- {menuItem.Name}: {item.Quantity} x {item.UnitPrice:N0} = {(item.Quantity * item.UnitPrice):N0} VNĐ");
                }

                // 9. Trả về thành công
                return Json(new
                {
                    success = true,
                    message = "Đặt hàng thành công!",
                    orderId = newOrder.Id,
                    status = newOrder.Status, // Trả về trạng thái thực tế
                    orderTime = newOrder.OrderTime?.ToString("dd/MM/yyyy HH:mm:ss"),
                    redirectUrl = $"/Order/Index?token={orderRequest.Token}" // Tùy chọn: redirect đến trang xem trạng thái
                });
            }
            catch (Exception ex)
            {
                // Rollback transaction nếu có lỗi
                await transaction.RollbackAsync();

                Console.WriteLine($"LỖI KHI ĐẶT HÀNG: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");

                return Json(new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi xử lý đơn hàng. Vui lòng thử lại."
                });
            }
        }

        // Action để xem trạng thái đơn hàng (tùy chọn)
        public async Task<IActionResult> Status(int tableId)
        {
            Console.WriteLine($"[DEBUG] Starting Status action with tableId: {tableId} at {DateTime.Now}");
            try
            {
                // Debug: Kiểm tra tất cả orders
                var allOrders = await _context.Orders.ToListAsync();
                Console.WriteLine($"[DEBUG] Total orders in database: {allOrders.Count}");
                foreach (var order in allOrders.Where(o => o.TableId == tableId).Take(3))
                {
                    Console.WriteLine($"[DEBUG] Order - ID: {order.Id}, TableId: {order.TableId}, OrderTime: {order.OrderTime}, Status: {order.Status}");
                }

                if (tableId <= 0)
                {
                    Console.WriteLine("[DEBUG] Invalid tableId received: <= 0");
                    return BadRequest("Bàn không hợp lệ");
                }

                var recentOrders = await _context.Orders
                    .Include(o => o.OrderItems)
                    .Where(o => o.TableId == tableId)
                    .OrderByDescending(o => o.OrderTime)
                    .Take(10)
                    .ToListAsync();

                Console.WriteLine($"[DEBUG] Found {recentOrders.Count} orders for tableId {tableId}");

                if (!recentOrders.Any())
                {
                    Console.WriteLine("[DEBUG] No orders found for tableId {tableId}");
                    return NotFound("Không tìm thấy đơn hàng nào cho bàn này");
                }

                ViewBag.TableId = tableId;
                return View(recentOrders);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Error in Status: {ex.Message}");
                Console.WriteLine($"[DEBUG] StackTrace: {ex.StackTrace}");
                return BadRequest("Có lỗi xảy ra");
            }
        }

        // Action để xem chi tiết một đơn hàng
        public async Task<IActionResult> OrderDetail(int id)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.MenuItem) // Include MenuItem để lấy thông tin món
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                {
                    return NotFound("Không tìm thấy đơn hàng");
                }

                // Tạo ViewModel cho chi tiết đơn hàng
                var statusViewModel = new OrderStatusViewModel
                {
                    Order = order,
                    MenuItems = order.OrderItems.ToDictionary(oi => oi.MenuItemId, oi => oi.MenuItem),
                    //OrderDate = DateTime.Now // hoặc lấy từ nơi nào bạn có lưu thông tin ngày đặt
                };
                return View(statusViewModel); // Truyền ViewModel đến OrderDetail.cshtml
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi xem chi tiết đơn hàng: {ex.Message}");
                return BadRequest("Có lỗi xảy ra");
            }
        }
    }
}
//OrderStatusViewModel