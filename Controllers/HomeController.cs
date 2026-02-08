using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AppointmentSystem.Web.Models;

namespace AppointmentSystem.Web.Controllers
{
    /// The HomeController handles the main pages of our website
    /// This includes the homepage, privacy policy, and error pages
    /// Think of this as the "front door" of our application
    public class HomeController : Controller
    {
        // Logger lets us write debug info and track what's happening in our app
        // ASP.NET Core automatically injects this when the controller is created
        private readonly ILogger<HomeController> _logger;

        // Constructor: Gets called once when the controller is first created
        // The logger parameter is automatically provided by dependency injection
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // This is the homepage action - what users see when they first visit our site
        // URL: / or /Home or /Home/Index
        // Returns the Index.cshtml view
        public IActionResult Index()
        {
            return View();
        }

        // The privacy policy page
        // URL: /Home/Privacy
        // Returns the Privacy.cshtml view with our privacy policy content
        public IActionResult Privacy()
        {
            return View();
        }

        // This is the error page that users see when something goes wrong
        // It's marked with [ResponseCache] to ensure error pages are never cached
        // We don't want browsers showing old error messages!
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // Create an ErrorViewModel with a request ID
            // This ID helps us trace the error in logs if the user reports a problem
            // Activity.Current?.Id is the trace ID from distributed tracing
            // If that's null, we fall back to the HttpContext.TraceIdentifier
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
