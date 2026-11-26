// Represents provider business information, service area, availability, and moderation state.
namespace AIOS.Models
{
    // Domain model for users who perform services for customers.
    public class ServiceProvider : Person
    {
        // Database identifier for the provider profile.
        public int ProviderID { get; set; }

        // Registered or public business name.
        public string BusinessName { get; set; } = "";

        // Provider registration or license reference.
        public string RegistrationNo { get; set; } = "";

        // Primary service region.
        public string Region { get; set; } = "";

        // Primary service ward.
        public string Ward { get; set; } = "";

        // Primary service block or neighborhood.
        public string Block { get; set; } = "";

        // Indicates whether the provider can accept new work.
        public string AvailabilityStatus { get; set; } = "Available";

        // Indicates whether administrators have blocked the provider from new assignments.
        public bool IsBlacklisted { get; set; }

        // Identifies this person as a provider for role-based navigation and permissions.
        public override string GetRole()
        {
            return "Provider";
        }
    }
}
