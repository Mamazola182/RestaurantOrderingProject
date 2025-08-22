using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging.Signing;
using RestaurantOrderingProject.Models;

namespace RestaurantOrderingProject.Controllers
{
    public class FoodsController : Controller
    {
        private readonly FoodOrderDbContext _context;

        public FoodsController(FoodOrderDbContext context)
        {
            _context = context;
        }

        // GET: FoodsController
        public async Task<IActionResult> Index()
        {
            try
            {
                var count = await _context.Foods.CountAsync();
                ViewBag.FoodCount = count;

                // Include Category để hiển thị thông tin category
                var foods = await _context.Foods
                    .Include(f => f.Category)
                    .ToListAsync();

                return View(foods);
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View(new List<Food>());
            }
        }

        // GET: FoodsController/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var food = await _context.Foods
                .Include(f => f.Category)
                .FirstOrDefaultAsync(m => m.FoodId == id);

            if (food == null)
            {
                return NotFound();
            }

            return View(food);
        }

        // GET: FoodsController/Create
        public IActionResult Create()
        {
            // Load categories để hiển thị trong dropdown
            ViewBag.Categories = _context.Categories.ToList();
            return View();
        }

        // POST: FoodsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FoodId,Name,Description,Price,ImageUrl,CategoryId,IsAvailable")] Food food)
        {
            if (ModelState.IsValid)
            {
                _context.Add(food);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Reload categories nếu model không hợp lệ
            ViewBag.Categories = _context.Categories.ToList();
            return View(food);
        }

        // GET: FoodsController/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var food = await _context.Foods.FindAsync(id);
            if (food == null)
            {
                return NotFound();
            }

            // Load categories để hiển thị trong dropdown
            ViewBag.Categories = _context.Categories.ToList();
            return View(food);
        }

        // POST: FoodsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("FoodId,Name,Description,Price,ImageUrl,CategoryId,IsAvailable")] Food food)
        {
            if (id != food.FoodId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(food);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FoodExists(food.FoodId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            // Reload categories nếu model không hợp lệ
            ViewBag.Categories = _context.Categories.ToList();
            return View(food);
        }

        // GET: FoodsController/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var food = await _context.Foods
                .Include(f => f.Category)
                .FirstOrDefaultAsync(m => m.FoodId == id);

            if (food == null)
            {
                return NotFound();
            }

            return View(food);
        }

        // POST: FoodsController/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var food = await _context.Foods.FindAsync(id);
            if (food != null)
            {
                _context.Foods.Remove(food);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FoodExists(int id)
        {
            return _context.Foods.Any(e => e.FoodId == id);
        }
    }
}