using Microsoft.AspNetCore.Mvc;
using RestaurantOrderingProject.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace RestaurantOrderingProject.Controllers
{
    public class AdminController : Controller
    {
        private readonly RestaurantQrorderingContext _context;

        public AdminController(RestaurantQrorderingContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Kiểm tra session
            if (HttpContext.Session.GetString("AdminUsername") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            // Truy vấn dữ liệu tóm tắt
            ViewBag.CategoryCount = await _context.Categories.CountAsync();
            ViewBag.MenuItemCount = await _context.MenuItems.CountAsync();
            ViewBag.OrderCount = await _context.Orders.CountAsync();
            ViewBag.TableCount = await _context.Tables.CountAsync();

            return View();
        }
    }
}