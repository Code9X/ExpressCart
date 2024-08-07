using ExpressCart.DataAccess.Data;
using ExpressCart.DataAccess.DbInitializer;
using ExpressCart.DataAccess.Repository;
using ExpressCart.DataAccess.Repository.IRepository;
using ExpressCart.Utility;
using ExpressCartWeb.Areas.Customer.Controllers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//API Config
builder.Services.Configure<API>(builder.Configuration.GetSection("API"));
builder.Services.AddSingleton<IAPIRepository, APIRepository>();

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

builder.Services.AddRazorPages();
builder.Services.AddScoped<IDbInitializer, DbInitializer>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.ConfigureApplicationCookie(options =>
{
	options.LoginPath = $"/Identity/Account/Login";
	options.LogoutPath = $"/Identity/Account/Logout";
	options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
});

//Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(100);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

//Loging
var logFilePath = Path.Combine(app.Environment.ContentRootPath, "Logs", "log-.txt");

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day)
    .CreateLogger();

//MIDDLEWARE PIPELINE
if (!app.Environment.IsDevelopment()) // This middleware handles exceptions and enforces HTTP Strict Transport Security (HSTS) in non-development environments.
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection(); // This middleware redirects HTTP requests to HTTPS.
app.UseStaticFiles(); // This middleware serves static files such as HTML, CSS, and JavaScript from the wwwroot folder.
app.UseRouting(); // This middleware adds route matching to the middleware pipeline.
app.UseAuthentication(); // This middleware adds authentication capabilities to the application.
app.UseAuthorization(); // This middleware ensures that the user is authorized to access certain resources.
app.MapRazorPages(); // This middleware maps Razor Pages to routes. 
app.UseSession(); // This middleware enables session state in the application.

StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();
SeedDataBase();

app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

app.Run();

void SeedDataBase()
{
    using (var scope = app.Services.CreateScope())
    {
        var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
        dbInitializer.Initialize();
    }
}

