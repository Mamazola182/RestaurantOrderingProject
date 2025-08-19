using System.ComponentModel.DataAnnotations; // Import namespace này

namespace RestaurantOrderingProject.Models;

public partial class MenuItem
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Tên món ăn là bắt buộc")]
    [StringLength(100, ErrorMessage = "Tên không vượt quá 100 ký tự")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Giá là bắt buộc")]
    [Range(0.01, 1000000, ErrorMessage = "Giá phải lớn hơn 0")]
    public decimal Price { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsAvailable { get; set; }

    [Required(ErrorMessage = "Danh mục là bắt buộc")]
    public int CategoryId { get; set; }

    public string? Description { get; set; }

    public virtual Category? Category { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}