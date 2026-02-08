// These are the essential packages we need to run our appointment management system
using Microsoft.EntityFrameworkCore;
using AppointmentSystem.Web.Data;

// This creates a new web application builder - think of it as the foundation of our app
var builder = WebApplication.CreateBuilder(args);

// Here we're telling ASP.NET Core to use MVC (Model-View-Controller) pattern
// This lets us organize our code into models (data), views (UI), and controllers (logic)
builder.Services.AddControllersWithViews();

// Setting up our database connection here
// We're using SQLite because it's lightweight and doesn't require a separate database server
// The connection string lives in appsettings.json so we can easily change it without touching code
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("AppointmentSystem")));

// Now we build the app with all the services we configured above
var app = builder.Build();

// This section configures how our app behaves when handling web requests
// We have different settings for development vs production environments
if (!app.Environment.IsDevelopment())
{
    // In production, if something goes wrong, show a friendly error page
    // instead of exposing technical details to users
    app.UseExceptionHandler("/Home/Error");
    
    // HSTS tells browsers to only use HTTPS for security
    // This protects our users' data from being intercepted
    app.UseHsts();
}

// Force all HTTP traffic to upgrade to HTTPS for security
app.UseHttpsRedirection();

// This lets us serve CSS, JavaScript, images, and other static files from wwwroot folder
app.UseStaticFiles();

// Enable routing so the app knows which controller action to call based on the URL
app.UseRouting();

// Enable authorization checks (even though we're not using authentication yet)
// It's good to have this in place for future expansion
app.UseAuthorization();

// This is our default routing pattern
// URL format: /Controller/Action/Id
// Example: /Appointments/Details/123 will call AppointmentsController.Details with id=123
// If no controller is specified, it defaults to Home controller and Index action
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Finally, start the web server and listen for incoming requests!
app.Run();
