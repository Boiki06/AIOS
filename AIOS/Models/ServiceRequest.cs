// Represents a customer's requested service, schedule, location, description, and lifecycle state.
namespace AIOS.Models
{
    // Data-transfer model shared by request creation, assignment, and history screens.
    public class ServiceRequest
    {
        // Database identifier for the request.
        public int RequestID { get; set; }

        // Customer who submitted the request.
        public int CustomerID { get; set; }

        // Provider service selected for the request.
        public int ProviderServiceID { get; set; }

        // Address where the service should be performed.
        public string ServiceAddress { get; set; } = "";

        // Region used for provider matching.
        public string Region { get; set; } = "";

        // Ward used for provider matching.
        public string Ward { get; set; } = "";

        // Block or neighborhood used for provider matching.
        public string Block { get; set; } = "";

        // Calendar date requested by the customer.
        public DateTime RequestedDate { get; set; }

        // Preferred beginning of the service window.
        public TimeSpan StartTime { get; set; }

        // Preferred end of the service window.
        public TimeSpan EndTime { get; set; }

        // Customer's explanation of the required work.
        public string Description { get; set; } = "";

        // Current request lifecycle state.
        public string Status { get; set; } = "Requested";

        // Actual start timestamp, when the provider begins the work.
        public DateTime? WorkStartedAt { get; set; }
    }
}