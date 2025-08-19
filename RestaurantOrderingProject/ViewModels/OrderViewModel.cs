using RestaurantOrderingProject.Models;

namespace RestaurantOrderingProject.ViewModels
{
    public class OrderViewModel
    {
        public int TableId { get; set; }
        public List<MenuItem>? MenuItems { get; set; } // Thêm ? để nullable
        public List<Category>? Categories { get; set; } // Thêm ? để nullable
    }
}