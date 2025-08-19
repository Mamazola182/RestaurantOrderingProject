using Microsoft.AspNetCore.Mvc;

using Microsoft.EntityFrameworkCore;
using RestaurantOrderingProject.Utils;
using RestaurantOrderingProject.Models;
namespace Project_PRN222.Controllers
{
    public class AuthenController(FoodOrderDbContext context) : Controller
    {
        public IActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user = context.Users.FirstOrDefault(u => u.Email == email);

            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                HttpContext.Session.SetObject("user", user);
                Console.WriteLine(user.Email);

                if (user.Role == "Admin")
                {
                    return RedirectToAction("Dashboard", "Admin"); // Admin
                }
                else // Customer
                {
                    return RedirectToAction("Index", "Home");
                }

            }

            ViewBag.Error = "Email hoặc mật khẩu không đúng.";
            return View("Index");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("user");

            return RedirectToAction("Index");
        }

        public IActionResult Signup()
        {

            return View();
        }

		[HttpPost]
		public IActionResult Signup(string email, string pass, string re_pass)
		{
			
			if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass) || string.IsNullOrEmpty(re_pass))
			{
				ViewBag.Error = "Vui lòng nhập đầy đủ thông tin.";
				return View();
			}

			
			if (pass != re_pass)
			{
				ViewBag.Error = "Mật khẩu không khớp.";
				return View();
			}

		
			if (context.Users.Any(u => u.Email == email))
			{
				ViewBag.Error = "Email đã được đăng ký.";
				return View();
			}

			
			var user = new User
			{
				Email = email,
				PasswordHash = BCrypt.Net.BCrypt.HashPassword(pass),
				Role = "Customer" 
			};

			context.Users.Add(user);
			context.SaveChanges();

		

			return RedirectToAction("Index"); 
		}

		
	}
}
