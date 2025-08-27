using Microsoft.AspNetCore.Mvc;
using RestaurantOrderingProject.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using SignalR.Hubs;

namespace RestaurantOrderingProject.Controllers
{
    public class MenuItemController(RestaurantQrorderingContext _context, IHubContext<ChatHub> hubContext) : Controller
    {
        

        private bool IsAuthenticated()
        {
            return HttpContext.Session.GetString("AdminUsername") != null;
        }

        // List món ăn
        public async Task<IActionResult> Index()
        {
            if (!IsAuthenticated()) return RedirectToAction("Index", "Login");

            var menuItems = await _context.MenuItems
                .Include(m => m.Category)
                .ToListAsync();
            return View(menuItems);
        }

        // Create món ăn (GET)
        public async Task<IActionResult> Create()
        {
            if (!IsAuthenticated()) return RedirectToAction("Index", "Login");

            ViewBag.CategoryId = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");

            return View(new MenuItem()); // 👈 TRUYỀN MODEL VÀO ĐÂY
        }


        // Create món ăn (POST) - Thêm xử lý lỗi và log
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MenuItem menuItem)
        {
            if (!IsAuthenticated()) return RedirectToAction("Index", "Login");

            if (ModelState.IsValid)
            {
                // Kiểm tra CategoryId tồn tại
                if (!await _context.Categories.AnyAsync(c => c.Id == menuItem.CategoryId))
                {
                    ModelState.AddModelError("CategoryId", "Danh mục không tồn tại.");
                }
                else
                {
                    try
                    {
                        _context.Add(menuItem);
                        await _context.SaveChangesAsync();
                        TempData["Success"] = "Thêm món ăn thành công!"; // Thông báo thành công
                        await hubContext.Clients.All.SendAsync("DataChanged");
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Lỗi khi thêm món ăn: " + ex.Message);
                    }
                }
            }

            // Nếu lỗi, load lại dropdown và trả view
            ViewBag.CategoryId = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", menuItem.CategoryId);
            foreach (var key in ModelState.Keys)
            {
                var errors = ModelState[key].Errors;
                foreach (var error in errors)
                {
                    Console.WriteLine($"Field: {key}, Error: {error.ErrorMessage}");
                }
            }
            return View(menuItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAvailable(int id)
        {
            if (!IsAuthenticated()) return RedirectToAction("Index", "Login");

            var item = await _context.MenuItems.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            // Đảo trạng thái
            item.IsAvailable = !item.IsAvailable;

            _context.MenuItems.Update(item);
            await _context.SaveChangesAsync();
            await hubContext.Clients.All.SendAsync("DataChanged");
            TempData["Success"] = "Trạng thái món ăn đã được cập nhật.";
            return RedirectToAction(nameof(Index));
        }
        // Edit món ăn (GET)
        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsAuthenticated()) return RedirectToAction("Index", "Login");

            if (id == null) return NotFound();

            var menuItem = await _context.MenuItems.FindAsync(id);
            if (menuItem == null) return NotFound();

            ViewBag.CategoryId = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", menuItem.CategoryId);
            return View(menuItem);
        }

        // Edit món ăn (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MenuItem menuItem)
        {
            if (!IsAuthenticated()) return RedirectToAction("Index", "Login");

            if (id != menuItem.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra CategoryId tồn tại
                    if (!await _context.Categories.AnyAsync(c => c.Id == menuItem.CategoryId))
                    {
                        ModelState.AddModelError("CategoryId", "Danh mục không tồn tại.");
                    }
                    else
                    {
                        _context.Update(menuItem);
                        await _context.SaveChangesAsync();
                        TempData["Success"] = "Sửa món ăn thành công!";
                        await hubContext.Clients.All.SendAsync("DataChanged");
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await MenuItemExists(menuItem.Id)) return NotFound();
                    throw;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi khi sửa món ăn: " + ex.Message);
                }
            }

            ViewBag.CategoryId = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", menuItem.CategoryId);
            return View(menuItem);
        }

        // Helper method để kiểm tra tồn tại
        private async Task<bool> MenuItemExists(int id)
        {
            return await _context.MenuItems.AnyAsync(e => e.Id == id);
        }

        // Các action khác giữ nguyên...

        public async Task<IActionResult> LoadMenuListPartial()
        {
            var items = await _context.MenuItems.Include(m => m.Category).ToListAsync();
            return PartialView("_MenuList", items);
        }

    }
}