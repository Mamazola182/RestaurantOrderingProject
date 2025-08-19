using Microsoft.AspNetCore.Mvc;
using RestaurantOrderingProject.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

namespace RestaurantOrderingProject.Controllers
{
    public class CategoryController : Controller
    {
        private readonly RestaurantQrorderingContext _context;

        public CategoryController(RestaurantQrorderingContext context)
        {
            _context = context;
        }

        // Check session để bảo vệ (tương tự dashboard)
        private bool IsAuthenticated()
        {
            return HttpContext.Session.GetString("AdminUsername") != null;
        }

        // List danh mục
        public async Task<IActionResult> Index()
        {
            if (!IsAuthenticated()) return RedirectToAction("Index", "Login");

            var categories = await _context.Categories.ToListAsync();
            return View(categories);
        }

        // Create danh mục (GET)
        public IActionResult Create()
        {
            if (!IsAuthenticated()) return RedirectToAction("Index", "Login");
            return View();
        }

        // Create danh mục (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (!IsAuthenticated()) return RedirectToAction("Index", "Login");

            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // Edit danh mục (GET)
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAuthenticated()) return RedirectToAction("Index", "Login");

            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        // Edit danh mục (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (!IsAuthenticated()) return RedirectToAction("Index", "Login");

            if (id != category.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // Delete danh mục (GET - confirm)
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAuthenticated()) return RedirectToAction("Index", "Login");

            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        // Delete danh mục (POST) - Thay đổi trạng thái IsAvailable thay vì xóa
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAuthenticated()) return RedirectToAction("Index", "Login");

            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                category.IsAvailable = !category.IsAvailable; // Toggle trạng thái từ true sang false hoặc ngược lại
                _context.Update(category);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}