// Represents customer-specific identity and membership data stored with a person record.
namespace AIOS.Models
{
    // Domain model used when customer data is loaded or passed between application layers.
    public class Customer : Person
    {
        // Database identifier for the customer profile.
        public int CustomerID { get; set; }

        // Business-facing membership identifier assigned to the customer.
        public string MembershipID { get; set; } = "";

        // Geographic region used when matching customers with providers.
        public string Region { get; set; } = "";

        // Returns the role value used by authentication and dashboard routing.
        public override string GetRole()
        {
            return "Customer";
        }
    }
}
