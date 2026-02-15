// These are the essential packages we need to run our appointment management system
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.OpenApi;
using AppointmentSystem.Web.Data;
using AppointmentSystem.Web.Filters;

// This creates a new web application builder - think of it as the foundation of our app
var builder = WebApplication.CreateBuilder(args);

// Here we're telling ASP.NET Core to use MVC (Model-View-Controller) pattern
// This lets us organize our code into models (data), views (UI), and controllers (logic)
// We also add a global filter that loads user-specific menus for the navigation bar
builder.Services.AddControllersWithViews(options =>
{
    // Add the MenuLoaderFilter globally so it runs before every action
    // This ensures the navigation bar always has the correct menus for the logged-in user
    options.Filters.AddService<MenuLoaderFilter>();
});

// Register the API explorer so Swagger can discover all API endpoints
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger/OpenAPI documentation for our REST API
// This automatically generates interactive API docs at /swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Appointra API",
        Version = "v1",
        Description = "REST API for the Appointra Appointment Management System. " +
                      "Provides endpoints for managing appointments, staff, users, roles, and menus."
    });
});

// Register the MenuLoaderFilter with dependency injection
// It needs the database context, so we register it as scoped (one instance per request)
builder.Services.AddScoped<MenuLoaderFilter>();

// Setting up our database connection here
// We're using SQLite because it's lightweight and doesn't require a separate database server
// The connection string lives in appsettings.json so we can easily change it without touching code
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("AppointmentSystem")));

// Setting up cookie-based authentication
// When a user logs in, a secure cookie is created in their browser
// This cookie is sent with every request so the server knows who the user is
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // If a user tries to access a protected page without being logged in,
        // they get redirected to the login page
        options.LoginPath = "/Account/Login";

        // If a logged-in user tries to access something they don't have permission for,
        // they get redirected to this access denied page
        options.AccessDeniedPath = "/Account/AccessDenied";

        // The cookie expires after 30 minutes of inactivity
        // After that, the user needs to log in again
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);

        // For API requests (paths starting with /api/), return 401/403 JSON responses
        // instead of redirecting to the login page — this is essential for API consumers
        options.Events.OnRedirectToLogin = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }
            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            }
            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
    })
    // Add Google OAuth as an external authentication provider
    // Users can click "Sign in with Google" and authenticate via their Google account
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
        // Request email and profile scopes so we get the user's name and email
        options.Scope.Add("email");
        options.Scope.Add("profile");
    });

// Now we build the app with all the services we configured above
var app = builder.Build();

// Apply pending migrations and seed the superadmin account + default roles/menus
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.MigrateAsync();
    await AppointmentSystem.Web.Data.DbSeeder.SeedAsync(context);
}

// Enable Swagger UI for interactive API documentation
// Available at /swagger in any environment for easy API exploration and testing
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Appointra API v1");
    options.DocumentTitle = "Appointra API Documentation";
});

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

// Enable authentication - this reads the cookie and creates a user identity
// Must come before authorization so we know WHO the user is before checking WHAT they can do
app.UseAuthentication();

// Enable authorization checks - this checks if the authenticated user has permission
// to access the requested resource (based on roles, policies, etc.)
app.UseAuthorization();

// This is our default routing pattern
// URL format: /Controller/Action/Id
// Example: /Appointments/Details/123 will call AppointmentsController.Details with id=123
// If no controller is specified, it defaults to Home controller and Index action
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Finally, start the web server and listen for incoming requests!
await app.RunAsync();
