// Defines the shared identity fields and behavior inherited by application users.
namespace AIOS.Models
{
    // Base domain model for customers and service providers.
    public abstract class Person
    {
        public int UserID { get; set; }

        public string FullName { get; set; } = "";

        public string Email { get; set; } = "";

        public string Phone { get; set; } = "";

        public string Address { get; set; } = "";

        // Requires each derived person type to identify its authorization role.
        public abstract string GetRole();

        // Provides a concise display string suitable for lists and search results.
        public virtual string GetSummary()
        {
            return $"{FullName} - {Email}";
        }
    }
}
