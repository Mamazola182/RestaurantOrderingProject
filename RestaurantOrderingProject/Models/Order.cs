using System;
using System.Collections.Generic;

namespace RestaurantOrderingProject.Models;

public partial class Order
{
    public int Id { get; set; }

    public int TableId { get; set; }

    public DateTime? OrderTime { get; set; }

    public string? Status { get; set; }

    public string? Note { get; set; }

    public decimal? TotalMoney { get; set; }

    public bool? IsTakeAway { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual Table Table { get; set; } = null!;
}
