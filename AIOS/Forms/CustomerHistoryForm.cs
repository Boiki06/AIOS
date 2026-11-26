// Customer screen for viewing previously submitted service requests and their statuses.
using AIOS.DataAccess;
using AIOS.Services;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AIOS.Forms
{
    // Owns customer request-history queries and display.
    public class CustomerHistoryForm : Form
    {
        private DataGridView grid = new();

        private Label lblRequests = new();
        private Label lblCompleted = new();
        private Label lblInvoiced = new();
        private Label lblPaid = new();

        public CustomerHistoryForm()
        {
            BuildUI();
            UiTheme.Apply(this);
            LoadHistory();
        }

        private void BuildUI()
        {
            Text = "AIOS - Customer Transaction History";

            Width = 1250;
            Height = 700;

            StartPosition =
                FormStartPosition.CenterParent;

            Panel summary = new()
            {
                Dock = DockStyle.Top,
                Height = 80
            };

            lblRequests.SetBounds(
                20, 20, 220, 30);

            lblCompleted.SetBounds(
                260, 20, 220, 30);

            lblInvoiced.SetBounds(
                500, 20, 220, 30);

            lblPaid.SetBounds(
                740, 20, 220, 30);

            summary.Controls.AddRange(
                new Control[]
                {
                    lblRequests,
                    lblCompleted,
                    lblInvoiced,
                    lblPaid
                });

            grid.Dock =
                DockStyle.Fill;

            grid.ReadOnly = true;

            grid.AllowUserToAddRows =
                false;

            grid.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;

            grid.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            Controls.Add(grid);
            Controls.Add(summary);
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
                        sp.BusinessName,
                        sc.CategoryName,
                        sr.Region,
                        sr.Ward,
                        sr.Block,
                        sr.RequestedDate,
                        sr.Status,

                        sr.WorkStartedAt,

                        ISNULL(
                            rec.DurationMinutes,
                            0
                        ) AS DurationMinutes,

                        i.InvoiceNumber,
                        ISNULL(
                            i.Amount,
                            0
                        ) AS InvoiceAmount,

                        i.Status AS InvoiceStatus,

                        ISNULL(
                            p.TotalPaid,
                            0
                        ) AS TotalPaid,

                        p.PaymentDate,
                        p.PaymentMethod,
                        p.PaymentStatus

                    FROM ServiceRequests sr

                    INNER JOIN Customers c
                        ON sr.CustomerID =
                        c.CustomerID

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

                    OUTER APPLY
                    (
                        SELECT
                            SUM(pay.Amount)
                            AS TotalPaid,

                            MAX(pay.PaymentDate)
                            AS PaymentDate,

                            MAX(pay.PaymentMethod)
                            AS PaymentMethod,

                            MAX(pay.Status)
                            AS PaymentStatus

                        FROM Payments pay

                        WHERE pay.InvoiceID =
                            i.InvoiceID

                        AND pay.Status =
                            'Successful'

                        AND pay.Direction =
                            'CustomerToAIOS'
                    ) p

                    WHERE c.UserID =
                        @UserID

                    ORDER BY
                        sr.RequestID DESC";

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

                CalculateSummary(table);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "History Error");
            }
        }

        private void CalculateSummary(
            DataTable table)
        {
            int requests =
                table.Rows.Count;

            int completed =
                table.AsEnumerable()
                .Count(r =>
                    r["Status"]
                    .ToString()
                    == "Completed");

            decimal invoiced =
                table.AsEnumerable()
                .Sum(r =>
                {
                    decimal value;

                    decimal.TryParse(
                        r["InvoiceAmount"]
                        .ToString(),
                        out value);

                    return value;
                });

            decimal paid =
                table.AsEnumerable()
                .Sum(r =>
                {
                    decimal value;

                    decimal.TryParse(
                        r["TotalPaid"]
                        .ToString(),
                        out value);

                    return value;
                });

            lblRequests.Text =
                $"Requests: {requests}";

            lblCompleted.Text =
                $"Completed: {completed}";

            lblInvoiced.Text =
                $"Invoiced: BWP {invoiced:N2}";

            lblPaid.Text =
                $"Paid: BWP {paid:N2}";
        }
    }
}