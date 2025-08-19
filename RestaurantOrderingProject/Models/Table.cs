using System;
using System.Collections.Generic;

namespace RestaurantOrderingProject.Models;

public partial class Table
{
    public int Id { get; set; }

    public int TableNumber { get; set; }

    public string QrcodeToken { get; set; } = null!;

    public bool? IsActive { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
