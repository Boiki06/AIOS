// Provider screen for viewing completed work and historical service assignments.
using AIOS.DataAccess;
using AIOS.Services;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AIOS.Forms
{
    // Owns retrieval and presentation of a provider's completed work history.
    public class ProviderHistoryForm : Form
    {
        private DataGridView grid = new();

        public ProviderHistoryForm()
        {
            BuildUI();
            UiTheme.Apply(this);
            LoadHistory();
        }

        private void BuildUI()
        {
            Text = "AIOS - Provider Service History";
            Width = 1200;
            Height = 650;
            StartPosition = FormStartPosition.CenterParent;

            Label title = new()
            {
                Text = "My Completed Service History",
                Font = new Font(
                    "Segoe UI",
                    20,
                    FontStyle.Bold),
                Location = new Point(25, 20),
                AutoSize = true
            };

            grid.Location = new Point(20, 70);
            grid.Size = new Size(1140, 500);

            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;
            grid.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;

            Controls.Add(title);
            Controls.Add(grid);
        }

        private void LoadHistory()
        {
            try
            {
                using SqlConnection conn =
                    DatabaseConnection.GetConnection();

                conn.Open();

                string sql = @"
                    SELECT
                        sr.RequestID,
                        c.CustomerID,
                        u.FullName AS CustomerName,
                        sc.CategoryName,
                        sr.ServiceAddress,
                        sr.Region,
                        sr.Ward,
                        sr.Block,
                        sr.RequestedDate,
                        sr.Status,
                        sr.WorkStartedAt,
                        rec.CompletionDate,
                        rec.DurationMinutes,
                        i.InvoiceNumber,
                        i.Amount AS InvoiceAmount,
                        i.Status AS InvoiceStatus,

                        ISNULL(
                            (
                                SELECT AVG(
                                    CAST(f.Rating AS DECIMAL(10,2))
                                )
                                FROM Feedback f
                                WHERE f.RequestID =
                                    sr.RequestID
                            ),
                            0
                        ) AS Rating

                    FROM ServiceRequests sr

                    INNER JOIN Customers c
                        ON sr.CustomerID =
                           c.CustomerID

                    INNER JOIN Users u
                        ON c.UserID =
                           u.UserID

                    INNER JOIN ProviderServices ps
                        ON sr.ProviderServiceID =
                           ps.ProviderServiceID

                    INNER JOIN ServiceProviders sp
                        ON ps.ProviderID =
                           sp.ProviderID

                    INNER JOIN ServiceCategories sc
                        ON ps.CategoryID =
                           sc.CategoryID

                    LEFT JOIN ServiceRecords rec
                        ON sr.RequestID =
                           rec.RequestID

                    LEFT JOIN Invoices i
                        ON sr.RequestID =
                           i.RequestID

                    WHERE sp.UserID =
                        @UserID

                    AND sr.Status =
                        'Completed'

                    ORDER BY
                        rec.CompletionDate DESC";

                using SqlCommand cmd =
                    new SqlCommand(sql, conn);

                cmd.Parameters.AddWithValue(
                    "@UserID",
                    Session.UserID);

                using SqlDataAdapter adapter =
                    new SqlDataAdapter(cmd);

                DataTable table = new();

                adapter.Fill(table);

                grid.DataSource = table;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "History Error");
            }
        }
    }
}