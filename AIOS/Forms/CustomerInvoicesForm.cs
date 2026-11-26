// Customer screen for viewing invoices generated for the customer's service requests.
using Microsoft.Data.SqlClient;
using AIOS.DataAccess;
using AIOS.Services;
using System;
using System.Data;
using System.Windows.Forms;

namespace AIOS.Forms
{
    // Owns invoice retrieval and invoice-state presentation for customers.
    public class CustomerInvoicesForm : Form
    {
        private DataGridView dgvInvoices;
        private Button btnPay;
        private Button btnRefresh;
        private Label lblTitle;

        public CustomerInvoicesForm()
        {
            InitializeComponent();
            UiTheme.Apply(this);
            LoadInvoices();
        }

        private void InitializeComponent()
        {
            Text = "AIOS - My Invoices & Payments";
            Width = 1100;
            Height = 650;
            StartPosition = FormStartPosition.CenterScreen;

            lblTitle = new Label
            {
                Text = "My Invoices & Payments",
                Left = 20,
                Top = 20,
                Width = 500,
                Height = 40,
                Font = new System.Drawing.Font(
                    "Segoe UI",
                    20,
                    System.Drawing.FontStyle.Bold)
            };

            dgvInvoices = new DataGridView
            {
                Left = 20,
                Top = 80,
                Width = 1040,
                Height = 430,
                AutoSizeColumnsMode =
                    DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                SelectionMode =
                    DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false
            };

            btnPay = new Button
            {
                Text = "Pay Selected Invoice",
                Left = 20,
                Top = 540,
                Width = 190,
                Height = 40
            };

            btnRefresh = new Button
            {
                Text = "Refresh",
                Left = 230,
                Top = 540,
                Width = 120,
                Height = 40
            };

            btnPay.Click += BtnPay_Click;
            btnRefresh.Click += BtnRefresh_Click;

            Controls.Add(lblTitle);
            Controls.Add(dgvInvoices);
            Controls.Add(btnPay);
            Controls.Add(btnRefresh);
        }

        private void LoadInvoices()
        {
            try
            {
                using SqlConnection conn =
                    DatabaseConnection.GetConnection();

                conn.Open();

                string sql = @"
                    SELECT
                        i.InvoiceID,
                        i.InvoiceNumber,
                        i.RequestID,
                        sc.CategoryName AS Service,
                        sp.BusinessName AS Provider,
                        CONVERT(varchar(10),
                            i.InvoiceDate, 103) AS InvoiceDate,
                        i.Amount,
                        i.Status,
                        ISNULL(
                            (
                                SELECT SUM(p.Amount)
                                FROM Payments p
                                WHERE p.InvoiceID = i.InvoiceID
                                  AND p.Direction = 'CustomerToAIOS'
                                  AND p.Status = 'Successful'
                            ),
                            0
                        ) AS AmountPaid
                    FROM Invoices i
                    INNER JOIN ServiceRequests sr
                        ON i.RequestID = sr.RequestID
                    INNER JOIN ProviderServices ps
                        ON sr.ProviderServiceID =
                           ps.ProviderServiceID
                    INNER JOIN ServiceCategories sc
                        ON ps.CategoryID = sc.CategoryID
                    INNER JOIN ServiceProviders sp
                        ON ps.ProviderID = sp.ProviderID
                    INNER JOIN Customers c
                        ON sr.CustomerID = c.CustomerID
                    WHERE c.UserID = @UserID
                    ORDER BY i.InvoiceDate DESC";

                using SqlCommand cmd =
                    new SqlCommand(sql, conn);

                cmd.Parameters.AddWithValue(
                    "@UserID",
                    Session.UserID);

                using SqlDataAdapter adapter =
                    new SqlDataAdapter(cmd);

                DataTable table = new DataTable();

                adapter.Fill(table);

                dgvInvoices.DataSource = table;

                if (dgvInvoices.Columns.Contains("InvoiceID"))
                    dgvInvoices.Columns["InvoiceID"].Visible = false;

                if (dgvInvoices.Columns.Contains("InvoiceNumber"))
                    dgvInvoices.Columns["InvoiceNumber"]
                        .HeaderText = "Invoice Number";

                if (dgvInvoices.Columns.Contains("RequestID"))
                    dgvInvoices.Columns["RequestID"]
                        .HeaderText = "Request ID";

                if (dgvInvoices.Columns.Contains("InvoiceDate"))
                    dgvInvoices.Columns["InvoiceDate"]
                        .HeaderText = "Invoice Date";

                if (dgvInvoices.Columns.Contains("Amount"))
                    dgvInvoices.Columns["Amount"]
                        .HeaderText = "Invoice Amount";

                if (dgvInvoices.Columns.Contains("AmountPaid"))
                    dgvInvoices.Columns["AmountPaid"]
                        .HeaderText = "Amount Paid";

                if (dgvInvoices.Columns.Contains("Status"))
                    dgvInvoices.Columns["Status"]
                        .HeaderText = "Status";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unable to load invoices.\n\n" +
                    ex.Message,
                    "AIOS",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void BtnPay_Click(object? sender, EventArgs e)
        {
            if (dgvInvoices.SelectedRows.Count == 0)
            {
                MessageBox.Show(
                    "Please select an invoice first.",
                    "AIOS",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            int invoiceID = Convert.ToInt32(
                dgvInvoices.SelectedRows[0]
                    .Cells["InvoiceID"].Value);

            string invoiceNumber =
                dgvInvoices.SelectedRows[0]
                    .Cells["InvoiceNumber"].Value?.ToString() ?? "";

            decimal invoiceAmount =
                Convert.ToDecimal(
                    dgvInvoices.SelectedRows[0]
                        .Cells["Amount"].Value);

            decimal amountPaid =
                Convert.ToDecimal(
                    dgvInvoices.SelectedRows[0]
                        .Cells["AmountPaid"].Value);

            string status =
                dgvInvoices.SelectedRows[0]
                    .Cells["Status"].Value?.ToString() ?? "";

            if (status.Equals(
                    "Paid",
                    StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(
                    "This invoice has already been paid.",
                    "AIOS",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            decimal balance =
                invoiceAmount - amountPaid;

            if (balance <= 0)
            {
                MessageBox.Show(
                    "There is no outstanding balance on this invoice.",
                    "AIOS",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            string paymentMethod =
                Microsoft.VisualBasic.Interaction.InputBox(
                    "Enter payment method:\n\n" +
                    "Examples: Cash, Card, Mobile Money, Bank Transfer",
                    "AIOS Payment",
                    "Card");

            if (string.IsNullOrWhiteSpace(paymentMethod))
            {
                MessageBox.Show(
                    "Payment cancelled.",
                    "AIOS",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            DialogResult confirmation =
                MessageBox.Show(
                    $"Invoice: {invoiceNumber}\n" +
                    $"Total: {invoiceAmount:C}\n" +
                    $"Already Paid: {amountPaid:C}\n" +
                    $"Outstanding: {balance:C}\n\n" +
                    $"Payment Method: {paymentMethod}\n\n" +
                    "Confirm payment?",
                    "Confirm Payment",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

            if (confirmation != DialogResult.Yes)
                return;

            ProcessPayment(
                invoiceID,
                invoiceNumber,
                balance,
                paymentMethod);
        }

        private void ProcessPayment(
            int invoiceID,
            string invoiceNumber,
            decimal amount,
            string paymentMethod)
        {
            try
            {
                using SqlConnection conn =
                    DatabaseConnection.GetConnection();

                conn.Open();

                using SqlTransaction transaction =
                    conn.BeginTransaction();

                try
                {
                    /*
                     * Re-check the invoice inside the transaction.
                     * This prevents paying an invoice twice if its
                     * status changed after the grid was loaded.
                     */
                    string invoiceSql = @"
                        SELECT
                            i.Amount,
                            i.Status,
                            ISNULL(
                                (
                                    SELECT SUM(p.Amount)
                                    FROM Payments p
                                    WHERE p.InvoiceID = i.InvoiceID
                                      AND p.Direction =
                                          'CustomerToAIOS'
                                      AND p.Status =
                                          'Successful'
                                ),
                                0
                            ) AS AmountPaid
                        FROM Invoices i
                        INNER JOIN ServiceRequests sr
                            ON i.RequestID = sr.RequestID
                        INNER JOIN Customers c
                            ON sr.CustomerID = c.CustomerID
                        WHERE i.InvoiceID = @InvoiceID
                          AND c.UserID = @UserID";

                    using SqlCommand invoiceCmd =
                        new SqlCommand(
                            invoiceSql,
                            conn,
                            transaction);

                    invoiceCmd.Parameters.AddWithValue(
                        "@InvoiceID",
                        invoiceID);

                    invoiceCmd.Parameters.AddWithValue(
                        "@UserID",
                        Session.UserID);

                    decimal invoiceAmount;
                    decimal amountPaid;
                    string currentStatus;

                    using (SqlDataReader reader =
                        invoiceCmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            MessageBox.Show(
                                "Invoice not found or does not belong to your account.",
                                "AIOS",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);

                            transaction.Rollback();
                            return;
                        }

                        invoiceAmount =
                            Convert.ToDecimal(
                                reader["Amount"]);

                        amountPaid =
                            Convert.ToDecimal(
                                reader["AmountPaid"]);

                        currentStatus =
                            reader["Status"]?.ToString() ?? "";
                    }

                    decimal outstanding =
                        invoiceAmount - amountPaid;

                    if (currentStatus.Equals(
                            "Paid",
                            StringComparison.OrdinalIgnoreCase)
                        || outstanding <= 0)
                    {
                        MessageBox.Show(
                            "This invoice has already been paid.",
                            "AIOS",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

                        transaction.Rollback();
                        return;
                    }

                    if (amount > outstanding)
                    {
                        MessageBox.Show(
                            $"The payment amount cannot exceed the outstanding balance of {outstanding:C}.",
                            "AIOS",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);

                        transaction.Rollback();
                        return;
                    }

                    string paymentSql = @"
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
                            @PaymentMethod,
                            'CustomerToAIOS',
                            'Successful'
                        )";

                    using SqlCommand paymentCmd =
                        new SqlCommand(
                            paymentSql,
                            conn,
                            transaction);

                    paymentCmd.Parameters.AddWithValue(
                        "@InvoiceID",
                        invoiceID);

                    paymentCmd.Parameters.AddWithValue(
                        "@Amount",
                        amount);

                    paymentCmd.Parameters.AddWithValue(
                        "@PaymentMethod",
                        paymentMethod.Trim());

                    paymentCmd.ExecuteNonQuery();

                    decimal newTotalPaid =
                        amountPaid + amount;

                    string newStatus =
                        newTotalPaid >= invoiceAmount
                            ? "Paid"
                            : "Partially Paid";

                    string updateInvoiceSql = @"
                        UPDATE Invoices
                        SET Status = @Status
                        WHERE InvoiceID = @InvoiceID";

                    using SqlCommand updateInvoiceCmd =
                        new SqlCommand(
                            updateInvoiceSql,
                            conn,
                            transaction);

                    updateInvoiceCmd.Parameters.AddWithValue(
                        "@Status",
                        newStatus);

                    updateInvoiceCmd.Parameters.AddWithValue(
                        "@InvoiceID",
                        invoiceID);

                    updateInvoiceCmd.ExecuteNonQuery();

                    transaction.Commit();

                    AuditService.Log(
                        Session.UserID,
                        "CUSTOMER_PAYMENT",
                        $"Customer paid {amount:C} toward invoice " +
                        $"{invoiceNumber} using {paymentMethod}. " +
                        $"Invoice status: {newStatus}.");

                    MessageBox.Show(
                        "Payment successful!\n\n" +
                        $"Invoice: {invoiceNumber}\n" +
                        $"Amount Paid: {amount:C}\n" +
                        $"Payment Method: {paymentMethod}\n" +
                        $"Payment Date: {DateTime.Now:dd/MM/yyyy HH:mm}\n" +
                        $"Invoice Status: {newStatus}",
                        "AIOS Payment",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    LoadInvoices();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Payment could not be completed.\n\n" +
                    ex.Message,
                    "AIOS",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void BtnRefresh_Click(object? sender, EventArgs e)
        {
            LoadInvoices();
        }
    }
}
