
// Creates the default administrator account when an installation does not yet have one.
using System;
using Microsoft.Data.SqlClient;
using AIOS.DataAccess;

namespace AIOS.Services
{
    // Contains one-time administrative database setup operations.
    public static class AdminSetup
    {
        // Hashes the supplied password and inserts an administrator only when the email is unused.
        public static void CreateAdminIfMissing(
            string email,
            string password)
        {
            string passwordHash =
                PasswordService.HashPassword(password);

            using (SqlConnection connection =
                DatabaseConnection.GetConnection())
            {
                connection.Open();

                string sql = @"
IF NOT EXISTS (
    SELECT 1 FROM Users WHERE Email = @Email
)
BEGIN
    INSERT INTO Users
        (FullName, Email, PasswordHash, Role)
    VALUES
        (@Name, @Email, @PasswordHash, 'Admin')
END";

                using (SqlCommand command =
                    new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue(
                        "@Name", "AIOS Administrator");

                    command.Parameters.AddWithValue(
                        "@Email", email);

                    command.Parameters.AddWithValue(
                        "@PasswordHash", passwordHash);

                    command.ExecuteNonQuery();
                }
            }
        }
    }
}