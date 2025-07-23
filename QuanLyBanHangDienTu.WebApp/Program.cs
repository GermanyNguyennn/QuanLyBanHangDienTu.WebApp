using Microsoft.AspNetCore.Identity;
using QuanLyBanHangDienTu.WebApp.Models.MoMo;
using QuanLyBanHangDienTu.WebApp.Models;
using QuanLyBanHangDienTu.WebApp.Services.EmailTemplates;
using QuanLyBanHangDienTu.WebApp.Services.Location;
using QuanLyBanHangDienTu.WebApp.Services.MoMo;
using QuanLyBanHangDienTu.WebApp.Services.VNPay;
using Microsoft.EntityFrameworkCore;
using QuanLyBanHangDienTu.WebApp.Repository;


var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MoMoOptionModel>(builder.Configuration.GetSection("MoMoAPI"));
builder.Services.AddScoped<IMoMoService, MoMoService>();

builder.Services.AddScoped<IVNPayService, VNPayService>();

builder.Services.AddTransient<IEmailSender, EmailSender>();

builder.Services.AddHttpClient<ILocationService, LocationService>();

builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlServer(builder.Configuration["ConnectionStrings:DbConnection"]);
});

builder.Services.AddTransient<IEmailSender, EmailSender>();

builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.IsEssential = true;
});

builder.Services.AddIdentity<UserModel, IdentityRole>()
    .AddEntityFrameworkStores<DataContext>().AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = false;
    options.Cookie.IsEssential = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

builder.Services.Configure<IdentityOptions>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
    options.User.RequireUniqueEmail = true;

    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 0;

    options.User.RequireUniqueEmail = true;
});

builder.Services.AddSingleton<EmailTemplateRenderer>();

var app = builder.Build();

app.UseStatusCodePagesWithRedirects("/Home/Error?statuscode={0}");

app.UseSession();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "Areas",
    pattern: "{area:exists}/{controller=Product}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "Category",
    pattern: "category/{slug?}",
    defaults: new { controller = "Category", action = "Index" });

app.MapControllerRoute(
    name: "Brand",
    pattern: "brand/{slug?}",
    defaults: new { controller = "Brand", action = "Index" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedData.SeedingDataAsync(services);
}

app.Run();