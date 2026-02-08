namespace AppointmentSystem.Web.Models
{
    /// This model is used when something goes wrong and we need to show an error page
    /// It's part of ASP.NET Core's built-in error handling system
    public class ErrorViewModel
    {
        // This is a unique ID that helps us trace what went wrong
        // Every HTTP request gets its own ID, so we can look it up in logs
        public string? RequestId { get; set; }

        // This is a helper property that tells us whether to display the RequestId on the error page
        // If RequestId is null or empty, there's no point showing it to the user
        // We only show it when it actually contains something useful for debugging
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
