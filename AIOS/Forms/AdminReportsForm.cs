// Administration dashboard that calculates and displays operational summary reports.
using AIOS.DataAccess;
using AIOS.Services;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AIOS.Forms
{
    // Owns report queries and summary metric presentation.
    public class AdminReportsForm : Form
    {
        private Label lblCustomers = new();
        private Label lblProviders = new();
        private Label lblRequests = new();
        private Label lblCompleted = new();
        private Label lblInvoices = new();
        private Label lblPayments = new();
        private Label lblRating = new();

        private DataGridView providerGrid = new();
        private DataGridView historyGrid = new();

        public AdminReportsForm()
        {
            BuildUI();
            UiTheme.Apply(this);
            LoadStatistics();
            LoadProviderPerformance();
            LoadCompletedHistory();
        }

        private void BuildUI()
        {
            Text = "AIOS - Reports & Statistics";

            Width = 1400;
            Height = 800;

            StartPosition =
                FormStartPosition.CenterParent;

            Label title = new()
            {
                Text = "AIOS Reports & Statistics",
                Font = new Font(
                    "Segoe UI",
                    22,
                    FontStyle.Bold),
                Location = new Point(25, 20),
                AutoSize = true
            };

            lblCustomers.SetBounds(
                25, 70, 180, 30);

            lblProviders.SetBounds(
                220, 70, 180, 30);

            lblRequests.SetBounds(
                415, 70, 180, 30);

            lblCompleted.SetBounds(
                610, 70, 180, 30);

            lblInvoices.SetBounds(
                805, 70, 180, 30);

            lblPayments.SetBounds(
                1000, 70, 180, 30);

            lblRating.SetBounds(
                1195, 70, 150, 30);

            Label providerTitle = new()
            {
                Text = "Provider Performance",
                Font = new Font(
                    "Segoe UI",
                    15,
                    FontStyle.Bold),
                Location = new Point(25, 115),
                AutoSize = true
            };

            providerGrid.SetBounds(
                20, 150, 1340, 250);

            providerGrid.ReadOnly = true;
            providerGrid.AllowUserToAddRows =
                false;

            providerGrid.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;

            providerGrid.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            Label historyTitle = new()
            {
                Text = "Completed Service History",
                Font = new Font(
                    "Segoe UI",
                    15,
                    FontStyle.Bold),
                Location = new Point(25, 420),
                AutoSize = true
            };

            historyGrid.SetBounds(
                20, 455, 1340, 260);

            historyGrid.ReadOnly = true;
            historyGrid.AllowUserToAddRows =
                false;

            historyGrid.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;

            Controls.AddRange(
                new Control[]
                {
                    title,
                    lblCustomers,
                    lblProviders,
                    lblRequests,
                    lblCompleted,
                    lblInvoices,
                    lblPayments,
                    lblRating,
                    providerTitle,
                    providerGrid,
                    historyTitle,
                    historyGrid
                });
        }

        private void LoadStatistics()
        {
            try
            {
                using SqlConnection conn =
                    DatabaseConnection.GetConnection();

                conn.Open();

                lblCustomers.Text =
                    "Customers: " +
                    ExecuteCount(
                        conn,
                        "SELECT COUNT(*) FROM Customers");

                lblProviders.Text =
                    "Providers: " +
                    ExecuteCount(
                        conn,
                        "SELECT COUNT(*) FROM ServiceProviders");

                lblRequests.Text =
                    "Requests: " +
                    ExecuteCount(
                        conn,
                        "SELECT COUNT(*) FROM ServiceRequests");

                lblCompleted.Text =
                    "Completed: " +
                    ExecuteCount(
                        conn,
                        @"SELECT COUNT(*)
                          FROM ServiceRequests
                          WHERE Status = 'Completed'");

                lblInvoices.Text =
                    "Invoices: " +
                    ExecuteCount(
                        conn,
                        "SELECT COUNT(*) FROM Invoices");

                object paymentTotal =
                    ExecuteScalar(
                        conn,
                        @"SELECT ISNULL(SUM(Amount),0)
                          FROM Payments
                          WHERE Status='Successful'");

                lblPayments.Text =
                    "Payments: BWP " +
                    Convert.ToDecimal(
                        paymentTotal)
                    .ToString("N2");

                object averageRating =
                    ExecuteScalar(
                        conn,
                        @"SELECT ISNULL(
                              AVG(
                                CAST(Rating AS DECIMAL(10,2))
                              ),
                              0
                           )
                           FROM Feedback");

                lblRating.Text =
                    "Avg Rating: " +
                    Convert.ToDecimal(
                        averageRating)
                    .ToString("0.00");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Statistics Error");
            }
        }

        private int ExecuteCount(
            SqlConnection conn,
            string sql)
        {
            using SqlCommand cmd =
                new SqlCommand(sql, conn);

            return Convert.ToInt32(
                cmd.ExecuteScalar());
        }

        private object ExecuteScalar(
            SqlConnection conn,
            string sql)
        {
            using SqlCommand cmd =
                new SqlCommand(sql, conn);

            return cmd.ExecuteScalar();
        }

        private void LoadProviderPerformance()
        {
            try
            {
                using SqlConnection conn =
                    DatabaseConnection.GetConnection();

                conn.Open();

                string sql = @"
                    SELECT
                        sp.ProviderID,
                        sp.BusinessName,

                        COUNT(
                            DISTINCT
                            CASE
                                WHEN sr.Status =
                                    'Completed'
                                THEN sr.RequestID
                            END
                        ) AS CompletedServices,

                        COUNT(f.FeedbackID)
                            AS TotalRatings,

                        ISNULL(
                            AVG(
                                CAST(
                                    f.Rating
                                    AS DECIMAL(10,2)
                                )
                            ),
                            0
                        ) AS AverageRating,

                        SUM(
                            CASE
                                WHEN f.FlagStatus =
                                    'Flagged'
                                THEN 1
                                ELSE 0
                            END
                        ) AS FlaggedFeedback

                    FROM ServiceProviders sp

                    LEFT JOIN ProviderServices ps
                        ON sp.ProviderID =
                           ps.ProviderID

                    LEFT JOIN ServiceRequests sr
                        ON ps.ProviderServiceID =
                           sr.ProviderServiceID

                    LEFT JOIN Feedback f
                        ON sp.ProviderID =
                           f.ProviderID

                    GROUP BY
                        sp.ProviderID,
                        sp.BusinessName

                    ORDER BY
                        AverageRating DESC";

                using SqlCommand cmd =
                    new SqlCommand(sql, conn);

                using SqlDataAdapter adapter =
                    new SqlDataAdapter(cmd);

                DataTable table = new();

                adapter.Fill(table);

                providerGrid.DataSource =
                    table;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Provider Report Error");
            }
        }

        private void LoadCompletedHistory()
        {
            try
            {
                using SqlConnection conn =
                    DatabaseConnection.GetConnection();

                conn.Open();

                string sql = @"
                    SELECT
                        sr.RequestID,
                        cu.FullName
                            AS Customer,

                        sp.BusinessName
                            AS Provider,

                        sc.CategoryName,

                        sr.Region,
                        sr.Ward,
                        sr.Block,

                        rec.CompletionDate,
                        rec.DurationMinutes,

                        i.InvoiceNumber,
                        i.Amount,
                        i.Status
                            AS InvoiceStatus

                    FROM ServiceRequests sr

                    INNER JOIN Customers c
                        ON sr.CustomerID =
                           c.CustomerID

                    INNER JOIN Users cu
                        ON c.UserID =
                           cu.UserID

                    INNER JOIN ProviderServices ps
                        ON sr.ProviderServiceID =
                           ps.ProviderServiceID

                    INNER JOIN ServiceProviders sp
                        ON ps.ProviderID =
                           sp.ProviderID

                    INNER JOIN ServiceCategories sc
                        ON ps.CategoryID =
                           sc.CategoryID

                    INNER JOIN ServiceRecords rec
                        ON sr.RequestID =
                           rec.RequestID

                    LEFT JOIN Invoices i
                        ON sr.RequestID =
                           i.RequestID

                    WHERE sr.Status =
                        'Completed'

                    ORDER BY
                        rec.CompletionDate DESC";

                using SqlCommand cmd =
                    new SqlCommand(sql, conn);

                using SqlDataAdapter adapter =
                    new SqlDataAdapter(cmd);

                DataTable table = new();

                adapter.Fill(table);

                historyGrid.DataSource =
                    table;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "History Report Error");
            }
        }
    }
}