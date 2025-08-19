// ViewModels/OrderRequestViewModels.cs
using System.ComponentModel.DataAnnotations;

namespace RestaurantOrderingProject.ViewModels
{
    public class OrderRequestModel
    {
        [Required]
        public int TableId { get; set; }

        [Required]
        public List<OrderItemRequest> Items { get; set; } = new List<OrderItemRequest>();

        public string? Notes { get; set; }

        public bool IsTakeaway { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }
        public string? Token { get; set; }
    }

    public class OrderItemRequest
    {
        [Required]
        public int MenuItemId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public string ItemName { get; set; } = string.Empty;
    }

    public class OrderResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? OrderId { get; set; }
        public string? RedirectUrl { get; set; }
    }
}