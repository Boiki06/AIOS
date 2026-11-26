// Encapsulates password hashing and verification so authentication uses one security policy.
namespace AIOS.Services
{
    // Delegates password cryptography to BCrypt.Net rather than storing plain-text passwords.
    public static class PasswordService
    {
        // Produces a salted BCrypt hash suitable for storing in the Users table.
        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // Compares a plain-text login value with a previously stored BCrypt hash.
        public static bool VerifyPassword(
            string password,
            string passwordHash)
        {
            return BCrypt.Net.BCrypt.Verify(
                password,
                passwordHash);
        }
    }
}