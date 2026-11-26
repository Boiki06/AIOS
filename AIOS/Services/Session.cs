// Stores the identity and role of the currently authenticated user for the desktop session.
namespace AIOS.Services
{
    // This state is process-wide because the application has one active user at a time.
    public static class Session
    {
        // Database identifier of the signed-in user; zero means no user is authenticated.
        public static int UserID { get; set; }

        // Display name shown in dashboards and user-facing messages.
        public static string FullName { get; set; } = "";

        // Email address associated with the authenticated account.
        public static string Email { get; set; } = "";

        // Authorization role used to select available dashboard features.
        public static string Role { get; set; } = "";

        // Indicates whether the session currently represents an authenticated user.
        public static bool IsLoggedIn =>
            UserID > 0;

        // Populates the session after credentials have been validated successfully.
        public static void Start(
            int userID,
            string fullName,
            string email,
            string role)
        {
            UserID = userID;
            FullName = fullName;
            Email = email;
            Role = role;
        }

        // Clears all identity data when the user signs out or the dashboard closes.
        public static void Logout()
        {
            UserID = 0;
            FullName = "";
            Email = "";
            Role = "";
        }
    }
}

