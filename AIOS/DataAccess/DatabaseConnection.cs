// Centralizes creation of SQL Server connections used by the application's data-access code.
using Microsoft.Data.SqlClient;

namespace AIOS.DataAccess
{
    // Provides a single connection-string policy without maintaining an open shared connection.
    public static class DatabaseConnection
    {
        // Connection details for the local AIOS database used by the application's queries.
        private static readonly string connectionString =
            @"Server=Boiki\MSSQLSERVERST;" +
            @"Database=AIOS_DB;" +
            @"Integrated Security=True;" +
            @"Encrypt=True;" +
            @"TrustServerCertificate=True;";

        // Creates a new unopened connection; callers own and dispose the returned instance.
        public static SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }
    }
}
