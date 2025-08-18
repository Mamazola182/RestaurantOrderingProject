using System.Text;
using Microsoft.AspNetCore.Mvc;
using RestaurantOrderingProject.Models;

namespace RestaurantOrderingProject.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View(); // Trả về trang login
        }

        [HttpPost]
        public IActionResult Authenticate(string username, string password)
        {
            if (username == "admin" && password == "123456") // Demo (sau này thay bằng DB)
            {
                HttpContext.Session.SetString("AdminUsername", username); // Lưu session
                return RedirectToAction("Index", "Admin");
            }

            ViewBag.Error = "Tài khoản hoặc mật khẩu không đúng!";
            return View("Index");
        }

        // Thêm action Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Xóa session
            return RedirectToAction("Index");
        }
    }
}
//using System.Linq;
//using System.Security.Cryptography;
//using System.Text;
//using Microsoft.AspNetCore.Components.Forms;
//using Microsoft.AspNetCore.Mvc;
//using RestaurantOrderingProject.Models;

//namespace RestaurantOrderingProject.Controllers
//{
//    public class LoginController : Controller
//    {
//        private readonly RestaurantQrorderingContext _context;

//        public LoginController(RestaurantQrorderingContext context)
//        {
//            _context = context;
//        }

//        public IActionResult Index()
//        {
//            return View(); // Trả về trang login
//        }

//        [HttpPost]
//        public IActionResult Authenticate(string username, string password)
//        {
//            // Truy vấn database để lấy admin theo username
//            var admin = _context.Admins.FirstOrDefault(a => a.Username == username);
//            Console.WriteLine($"Username: {username}");
//            Console.WriteLine($"Input Password Hash: {password}");
//            Console.WriteLine($"DB PasswordHash: {admin.PasswordHash}");

//            if (admin != null && admin.PasswordHash == HashPassword(password))
//            {
//                // Login thành công, có thể thêm session hoặc authentication ở đây (ví dụ: HttpContext.Session.SetString("AdminUsername", username);)
//                return RedirectToAction("Dashboard", "Admin");
//            }

//            ViewBag.Error = "Tài khoản hoặc mật khẩu không đúng!";
//            return View("Index");
//        }

//        // Hàm hash mật khẩu sử dụng SHA256 (giả sử mật khẩu trong DB được hash theo cách này)
//        private string HashPassword(string password)
//        {
//            using (var sha256 = SHA256.Create())
//            {
//                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
//                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
//            }
//        }
//    }
//}
