// Administration screen for reviewing invoice and payment transactions.
using AIOS.DataAccess;
using AIOS.Services;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AIOS.Forms
{
    // Owns transaction retrieval and display for administrators.
    public class AdminTransactionsForm : Form
    {
        private DataGridView grid = new();

        public AdminTransactionsForm()
        {
            BuildUI();
            UiTheme.Apply(this);
            LoadTransactions();
        }

        private void BuildUI()
        {
            Text = "AIOS - Transaction Management";

            Width = 1400;
            Height = 750;

            StartPosition =
                FormStartPosition.CenterParent;

            Label title = new()
            {
                Text = "Transaction Management",
                Font = new Font(
                    "Segoe UI",
                    20,
                    FontStyle.Bold),
                Location = new Point(25, 20),
                AutoSize = true
            };

            Button btnRefresh = new()
            {
                Text = "Refresh",
                Location = new Point(1150, 20),
                Width = 100,
                Height = 35
            };

            btnRefresh.Click +=
                (s, e) => LoadTransactions();

            Button btnPayProvider = new()
            {
                Text = "Pay Provider",
                Location = new Point(1260, 20),
                Width = 110,
                Height = 35
            };

            btnPayProvider.Click +=
                PayProvider;

            grid.SetBounds(
                20, 75, 1340, 570);

            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;

            grid.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            grid.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;

            Controls.AddRange(
                new Control[]
                {
                    title,
                    btnRefresh,
                    btnPayProvider,
                    grid
                });
        }

        private void LoadTransactions()
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
                            AS CustomerName,

                        sp.BusinessName
                            AS ProviderName,

                        sc.CategoryName,

                        sr.RequestedDate,

                        sr.Status
                            AS ServiceStatus,

                        i.InvoiceID,
                        i.InvoiceNumber,
                        i.Amount,
                        i.Status
                            AS InvoiceStatus,

                        ISNULL(
                            (
                                SELECT SUM(p.Amount)
                                FROM Payments p
                                WHERE
                                    p.InvoiceID =
                                        i.InvoiceID

                                AND p.Direction =
                                    'CustomerToAIOS'

                                AND p.Status =
                                    'Successful'
                            ),
                            0
                        ) AS CustomerPaid,

                        ISNULL(
                            (
                                SELECT SUM(p.Amount)
                                FROM Payments p
                                WHERE
                                    p.InvoiceID =
                                        i.InvoiceID

                                AND p.Direction =
                                    'AIOSToProvider'

                                AND p.Status =
                                    'Successful'
                            ),
                            0
                        ) AS ProviderPaid

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

                    LEFT JOIN Invoices i
                        ON sr.RequestID =
                           i.RequestID

                    ORDER BY
                        sr.RequestID DESC";

                using SqlCommand cmd =
                    new SqlCommand(sql, conn);

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
                    "Transaction Error");
            }
        }

        private void PayProvider(
            object? sender,
            EventArgs e)
        {
            if (grid.SelectedRows.Count == 0)
            {
                MessageBox.Show(
                    "Select a transaction first.");

                return;
            }

            DataGridViewRow row =
                grid.SelectedRows[0];

            if (row.Cells["InvoiceID"].Value
                == DBNull.Value)
            {
                MessageBox.Show(
                    "This request has no invoice.");

                return;
            }

            int invoiceID =
                Convert.ToInt32(
                    row.Cells["InvoiceID"].Value);

            decimal invoiceAmount =
                Convert.ToDecimal(
                    row.Cells["Amount"].Value);

            decimal customerPaid =
                Convert.ToDecimal(
                    row.Cells["CustomerPaid"].Value);

            decimal providerPaid =
                Convert.ToDecimal(
                    row.Cells["ProviderPaid"].Value);

            if (customerPaid < invoiceAmount)
            {
                MessageBox.Show(
                    "The customer has not fully paid the invoice yet.");

                return;
            }

            if (providerPaid >= invoiceAmount)
            {
                MessageBox.Show(
                    "The provider has already been paid.");

                return;
            }

            decimal amountToPay =
                invoiceAmount - providerPaid;

            DialogResult result =
                MessageBox.Show(
                    $"Pay BWP {amountToPay:N2} to the provider?",
                    "Provider Payment",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            try
            {
                using SqlConnection conn =
                    DatabaseConnection.GetConnection();

                conn.Open();

                using SqlTransaction transaction =
                    conn.BeginTransaction();

                string sql = @"
                    INSERT INTO Payments
                    (
                        InvoiceID,
                        Amount,
                        PaymentDate,
                        PaymentMethod,
                        Direction,
                        Status
                    )
                    VALUES
                    (
                        @InvoiceID,
                        @Amount,
                        SYSDATETIME(),
                        'AIOS Provider Payout',
                        'AIOSToProvider',
                        'Successful'
                    )";

                using SqlCommand cmd =
                    new SqlCommand(
                        sql,
                        conn,
                        transaction);

                cmd.Parameters.AddWithValue(
                    "@InvoiceID",
                    invoiceID);

                cmd.Parameters.AddWithValue(
                    "@Amount",
                    amountToPay);

                cmd.ExecuteNonQuery();

                transaction.Commit();

                AuditService.Log(
                    Session.UserID,
                    "PROVIDER PAYMENT",
                    $"AIOS paid provider BWP " +
                    $"{amountToPay:N2} for InvoiceID " +
                    $"{invoiceID}.");

                MessageBox.Show(
                    "Provider payment recorded successfully.");

                LoadTransactions();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Provider Payment Error");
            }
        }
    }
}