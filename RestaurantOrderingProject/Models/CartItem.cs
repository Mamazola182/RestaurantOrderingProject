using System;
using System.Collections.Generic;

namespace RestaurantOrderingProject.Models;

public partial class CartItem
{
    public int CartItemId { get; set; }

    public int? UserId { get; set; }

    public int? FoodId { get; set; }

    public int? Quantity { get; set; }

    public string? Notes { get; set; }

    public virtual Food? Food { get; set; }

    public virtual User? User { get; set; }
}
