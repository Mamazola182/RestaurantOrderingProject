using System;
using System.Collections.Generic;

namespace RestaurantOrderingProject.Models;

public partial class Admin
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;
}
