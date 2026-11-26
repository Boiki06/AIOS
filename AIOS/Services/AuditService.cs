// Writes user and system actions to the database audit log.
using Microsoft.Data.SqlClient;
using AIOS.DataAccess;

namespace AIOS.Services
{
    // Provides best-effort audit logging so auditing failures do not interrupt user workflows.
    public static class AuditService
    {
        // Persists an action and its description, optionally associated with a user account.
        public static void Log(
            int? userID,
            string action,
            string description)
        {
            try
            {
                using SqlConnection conn =
                    DatabaseConnection.GetConnection();

                conn.Open();

                string sql = @"
                    INSERT INTO AuditLogs
                    (UserID, Action, Description)
                    VALUES
                    (@UserID, @Action, @Description)";

                using SqlCommand cmd =
                    new SqlCommand(sql, conn);

                if (userID.HasValue)
                    cmd.Parameters.AddWithValue(
                        "@UserID",
                        userID.Value);
                else
                    cmd.Parameters.AddWithValue(
                        "@UserID",
                        DBNull.Value);

                cmd.Parameters.AddWithValue(
                    "@Action",
                    action);

                cmd.Parameters.AddWithValue(
                    "@Description",
                    description);

                cmd.ExecuteNonQuery();
            }
            catch
            {
                // Audit failure should not crash the application.
            }
        }
    }
}