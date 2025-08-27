using Microsoft.EntityFrameworkCore;
using RestaurantOrderingProject.Models;
using SignalR.Hubs;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache(); // Backing store cho session (in-memory cho dev)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Th?i gian timeout session (tùy ch?nh)
    options.Cookie.HttpOnly = true; // B?o m?t cookie
    options.Cookie.IsEssential = true; // Cookie essential, không c?n consent
});
builder.Services.AddDbContext<RestaurantQrorderingContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DB")));

// N?u b?n dùng authentication, thêm ? ?ây. Ví d?:
// builder.Services.AddAuthentication(...);
builder.Services.AddDbContext<RestaurantQrorderingContext>(option => option.UseSqlServer(builder.Configuration.GetConnectionString("MyCnn")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization(); // B? n?u không dùng authentication

app.UseSession(); // Ph?i sau UseRouting
app.MapHub<ChatHub>("/orderHub");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Qr}/{action=GenerateList}/{id?}"); // ??i t? GenerateList thành Index

app.Run();