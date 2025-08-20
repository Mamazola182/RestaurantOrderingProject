using Microsoft.AspNetCore.Mvc;

namespace RestaurantOrderingProject.Controllers
{
    public class AdminController : Controller
    {
        // GET: /Admin/Dashboard
        public IActionResult Dashboard()
        {
            return View();
        }

        // GET: /Admin/Subjects
        public IActionResult Subjects()
        {
            return View();
        }
    }
}
