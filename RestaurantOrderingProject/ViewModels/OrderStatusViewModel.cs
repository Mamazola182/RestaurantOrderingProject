using RestaurantOrderingProject.Models;

namespace RestaurantOrderingProject.ViewModels
{
    public class OrderStatusViewModel
    {
        public Order Order { get; set; }
        public Dictionary<int, MenuItem> MenuItems { get; set; }

        //public DateTime OrderDate { get; set; }
    }
}