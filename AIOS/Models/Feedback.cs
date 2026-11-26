// Represents a customer's rating, comments, and moderation state for a service request.
namespace AIOS.Models
{
    // Simple data-transfer model for feedback records.
    public class Feedback
    {
        // Database identifier for the feedback record.
        public int FeedbackID { get; set; }

        // Request being evaluated by the customer.
        public int RequestID { get; set; }

        // Provider whose work received the feedback.
        public int ProviderID { get; set; }

        // Customer who submitted the feedback.
        public int CustomerID { get; set; }

        // Numeric rating assigned to the completed service.
        public int Rating { get; set; }

        // Optional written explanation supplied by the customer.
        public string Comments { get; set; } = "";

        // Moderation state used when feedback requires administrator attention.
        public string FlagStatus { get; set; } = "Normal";
    }
}
