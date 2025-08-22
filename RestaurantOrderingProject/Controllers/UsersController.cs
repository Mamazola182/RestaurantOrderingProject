using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantOrderingProject.Models;
using System.Security.Cryptography;
using System.Text;

namespace RestaurantOrderingProject.Controllers
{
    public class UsersController : Controller
    {
        private readonly FoodOrderDbContext _context;

        public UsersController(FoodOrderDbContext context)
        {
            _context = context;
        }

        // LIST + Search + Filter + Pagination
        public async Task<IActionResult> Index(string searchString, string roleFilter, int page = 1, int pageSize = 5)
        {
            var users = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                users = users.Where(u => u.FullName.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(roleFilter))
            {
                users = users.Where(u => u.Role == roleFilter);
            }

            // Tổng số record
            var totalUsers = await users.CountAsync();

            // Phân trang
            var data = await users
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentFilter = searchString;
            ViewBag.RoleFilter = roleFilter;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);

            return View(data);
        }

        // DETAILS
        public async Task<IActionResult> Details(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
            if (user == null) return NotFound();

            return View(user);
        }

        // CREATE GET
        public IActionResult Create()
        {
            return View();
        }

        // CREATE POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FullName,Email,PasswordHash,Phone,Role")] User user)
        {
            // Validate
            if (_context.Users.Any(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email", "Email đã tồn tại!");
            }

            if (string.IsNullOrEmpty(user.PasswordHash) || user.PasswordHash.Length < 8)
            {
                ModelState.AddModelError("PasswordHash", "Mật khẩu phải từ 8 ký tự trở lên!");
            }

            if (ModelState.IsValid)
            {
                user.CreatedAt = DateTime.Now;
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // EDIT GET
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            return View(user);
        }

        // EDIT POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,FullName,Email,PasswordHash,Phone,Role,CreatedAt")] User user)
        {
            if (id != user.UserId) return NotFound();

            if (_context.Users.Any(u => u.Email == user.Email && u.UserId != id))
            {
                ModelState.AddModelError("Email", "Email đã tồn tại!");
            }

            if (string.IsNullOrEmpty(user.PasswordHash) || user.PasswordHash.Length < 8)
            {
                ModelState.AddModelError("PasswordHash", "Mật khẩu phải từ 8 ký tự trở lên!");
            }

            if (ModelState.IsValid)
            {
                _context.Update(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // DELETE GET
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
            if (user == null) return NotFound();

            return View(user);
        }

        // DELETE POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users
                .Include(u => u.Orders) // load luôn orders
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
                return NotFound();

            if (user.Orders != null && user.Orders.Any())
            {
                // Có đơn hàng => không cho xóa
                TempData["DeleteError"] = $"Không thể xóa user {user.FullName} vì đã có đơn hàng.";
                return RedirectToAction(nameof(Delete), new { id = id });
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}
