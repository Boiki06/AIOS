// Provider screen for managing current jobs, status transitions, and completion details.
using Microsoft.Data.SqlClient;
using AIOS.DataAccess;
using AIOS.Services;
using System;
using System.Data;
using System.Windows.Forms;

namespace AIOS.Forms
{
    // Owns provider job queries, status changes, and completion workflows.
    public class ProviderJobsForm : Form
    {
        private DataGridView dgvJobs;
        private Button btnStartWork;
        private Button btnCompleteJob;
        private Button btnRefresh;
        private Label lblTitle;

        public ProviderJobsForm()
        {
            InitializeComponent();
            UiTheme.Apply(this);
            LoadJobs();
        }

        private void InitializeComponent()
        {
            Text = "AIOS - Manage My Jobs";
            Width = 1250;
            Height = 700;
            StartPosition = FormStartPosition.CenterScreen;

            lblTitle = new Label
            {
                Text = "Manage My Jobs",
                Left = 20,
                Top = 20,
                Width = 400,
                Height = 40,
                Font = new System.Drawing.Font("Segoe UI", 20, System.Drawing.FontStyle.Bold)
            };

            dgvJobs = new DataGridView
            {
                Left = 20,
                Top = 80,
                Width = 1190,
                Height = 480,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false
            };

            btnStartWork = new Button
            {
                Text = "Start Work",
                Left = 20,
                Top = 590,
                Width = 150,
                Height = 40
            };

            btnCompleteJob = new Button
            {
                Text = "Complete Job & Invoice",
                Left = 190,
                Top = 590,
                Width = 200,
                Height = 40
            };

            btnRefresh = new Button
            {
                Text = "Refresh",
                Left = 410,
                Top = 590,
                Width = 120,
                Height = 40
            };

            btnStartWork.Click += BtnStartWork_Click;
            btnCompleteJob.Click += BtnCompleteJob_Click;
            btnRefresh.Click += BtnRefresh_Click;

            Controls.Add(lblTitle);
            Controls.Add(dgvJobs);
            Controls.Add(btnStartWork);
            Controls.Add(btnCompleteJob);
            Controls.Add(btnRefresh);
        }

        private void LoadJobs()
        {
            try
            {
                using SqlConnection conn = DatabaseConnection.GetConnection();
                conn.Open();

                string sql = @"
                    SELECT
                        sr.RequestID,
                        u.FullName AS Customer,
                        sc.CategoryName AS Service,
                        sr.ServiceAddress,
                        sr.Region,
                        sr.Ward,
                        sr.Block,
                        CONVERT(varchar(10), sr.RequestedDate, 103) AS RequestedDate,
                        CONVERT(varchar(5), sr.StartTime, 108) AS StartTime,
                        CONVERT(varchar(5), sr.EndTime, 108) AS EndTime,
                        sr.Status,
                        sr.WorkStartedAt,
                        sr.Description
                    FROM ServiceRequests sr
                    INNER JOIN Customers c
                        ON sr.CustomerID = c.CustomerID
                    INNER JOIN Users u
                        ON c.UserID = u.UserID
                    INNER JOIN ProviderServices ps
                        ON sr.ProviderServiceID = ps.ProviderServiceID
                    INNER JOIN ServiceCategories sc
                        ON ps.CategoryID = sc.CategoryID
                    WHERE ps.ProviderID =
                        (
                            SELECT ProviderID
                            FROM ServiceProviders
                            WHERE UserID = @UserID
                        )
                    AND sr.Status IN ('Accepted', 'In Progress')
                    ORDER BY sr.RequestedDate, sr.StartTime";

                using SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserID", Session.UserID);

                using SqlDataAdapter adapter = new SqlDataAdapter(cmd);

                DataTable table = new DataTable();
                adapter.Fill(table);

                dgvJobs.DataSource = table;

                if (dgvJobs.Columns.Contains("RequestID"))
                    dgvJobs.Columns["RequestID"].HeaderText = "Request ID";

                if (dgvJobs.Columns.Contains("Customer"))
                    dgvJobs.Columns["Customer"].HeaderText = "Customer";

                if (dgvJobs.Columns.Contains("Service"))
                    dgvJobs.Columns["Service"].HeaderText = "Service";

                if (dgvJobs.Columns.Contains("ServiceAddress"))
                    dgvJobs.Columns["ServiceAddress"].HeaderText = "Address";

                if (dgvJobs.Columns.Contains("RequestedDate"))
                    dgvJobs.Columns["RequestedDate"].HeaderText = "Date";

                if (dgvJobs.Columns.Contains("StartTime"))
                    dgvJobs.Columns["StartTime"].HeaderText = "Start";

                if (dgvJobs.Columns.Contains("EndTime"))
                    dgvJobs.Columns["EndTime"].HeaderText = "End";

                if (dgvJobs.Columns.Contains("WorkStartedAt"))
                    dgvJobs.Columns["WorkStartedAt"].HeaderText = "Actual Start";

                if (dgvJobs.Columns.Contains("Description"))
                    dgvJobs.Columns["Description"].HeaderText = "Description";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unable to load your jobs.\n\n" + ex.Message,
                    "AIOS",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private int GetSelectedRequestID()
        {
            if (dgvJobs.SelectedRows.Count == 0)
            {
                MessageBox.Show(
                    "Please select a job first.",
                    "AIOS",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return 0;
            }

            return Convert.ToInt32(
                dgvJobs.SelectedRows[0].Cells["RequestID"].Value);
        }

        private void BtnStartWork_Click(object? sender, EventArgs e)
        {
            int requestID = GetSelectedRequestID();

            if (requestID == 0)
                return;

            try
            {
                using SqlConnection conn = DatabaseConnection.GetConnection();
                conn.Open();

                string sql = @"
                    UPDATE sr
                    SET
                        sr.Status = 'In Progress',
                        sr.WorkStartedAt = SYSDATETIME()
                    FROM ServiceRequests sr
                    INNER JOIN ProviderServices ps
                        ON sr.ProviderServiceID = ps.ProviderServiceID
                    INNER JOIN ServiceProviders sp
                        ON ps.ProviderID = sp.ProviderID
                    WHERE sr.RequestID = @RequestID
                      AND sp.UserID = @UserID
                      AND sr.Status = 'Accepted'
                      AND sp.IsBlacklisted = 0";

                using SqlCommand cmd = new SqlCommand(sql, conn);

                cmd.Parameters.AddWithValue("@RequestID", requestID);
                cmd.Parameters.AddWithValue("@UserID", Session.UserID);

                int rows = cmd.ExecuteNonQuery();

                if (rows == 0)
                {
                    MessageBox.Show(
                        "The job cannot be started. It may no longer be accepted, may not belong to you, or the provider may be blacklisted.",
                        "AIOS",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                AuditService.Log(
                    Session.UserID,
                    "START_SERVICE",
                    $"Service request {requestID} was started.");

                MessageBox.Show(
                    "Work has been started successfully.",
                    "AIOS",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                LoadJobs();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unable to start the job.\n\n" + ex.Message,
                    "AIOS",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void BtnCompleteJob_Click(object? sender, EventArgs e)
        {
            int requestID = GetSelectedRequestID();

            if (requestID == 0)
                return;

            try
            {
                using SqlConnection conn = DatabaseConnection.GetConnection();
                conn.Open();

                using SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    string requestSql = @"
                        SELECT
                            sr.Status,
                            sr.WorkStartedAt,
                            ps.ServiceRate,
                            ps.ProviderID
                        FROM ServiceRequests sr
                        INNER JOIN ProviderServices ps
                            ON sr.ProviderServiceID = ps.ProviderServiceID
                        INNER JOIN ServiceProviders sp
                            ON ps.ProviderID = sp.ProviderID
                        WHERE sr.RequestID = @RequestID
                          AND sp.UserID = @UserID
                          AND sp.IsBlacklisted = 0";

                    using SqlCommand requestCmd =
                        new SqlCommand(requestSql, conn, transaction);

                    requestCmd.Parameters.AddWithValue("@RequestID", requestID);
                    requestCmd.Parameters.AddWithValue("@UserID", Session.UserID);

                    string status = "";
                    DateTime? workStartedAt = null;
                    decimal serviceRate = 0;
                    int providerID = 0;

                    using (SqlDataReader reader = requestCmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            MessageBox.Show(
                                "This job was not found or does not belong to you.",
                                "AIOS",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);

                            transaction.Rollback();
                            return;
                        }

                        status = reader["Status"].ToString() ?? "";

                        if (reader["WorkStartedAt"] != DBNull.Value)
                            workStartedAt =
                                Convert.ToDateTime(reader["WorkStartedAt"]);

                        serviceRate =
                            Convert.ToDecimal(reader["ServiceRate"]);

                        providerID =
                            Convert.ToInt32(reader["ProviderID"]);
                    }

                    if (status != "In Progress")
                    {
                        MessageBox.Show(
                            "Only jobs currently marked 'In Progress' can be completed.",
                            "AIOS",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);

                        transaction.Rollback();
                        return;
                    }

                    if (!workStartedAt.HasValue)
                    {
                        MessageBox.Show(
                            "The actual work start time is missing. The job cannot be completed.",
                            "AIOS",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);

                        transaction.Rollback();
                        return;
                    }

                    DateTime completionTime = DateTime.Now;

                    double duration =
                        (completionTime - workStartedAt.Value).TotalMinutes;

                    int durationMinutes =
                        Math.Max(1, (int)Math.Round(duration));

                    string recordSql = @"
                        INSERT INTO ServiceRecords
                        (
                            RequestID,
                            Description,
                            CompletionDate,
                            DurationMinutes
                        )
                        SELECT
                            RequestID,
                            Description,
                            SYSDATETIME(),
                            @DurationMinutes
                        FROM ServiceRequests
                        WHERE RequestID = @RequestID";

                    using SqlCommand recordCmd =
                        new SqlCommand(recordSql, conn, transaction);

                    recordCmd.Parameters.AddWithValue(
                        "@RequestID",
                        requestID);

                    recordCmd.Parameters.AddWithValue(
                        "@DurationMinutes",
                        durationMinutes);

                    recordCmd.ExecuteNonQuery();

                    string invoiceNumber =
                        "INV-" +
                        DateTime.Now.ToString("yyyyMMddHHmmss") +
                        "-" +
                        requestID;

                    string invoiceSql = @"
                        INSERT INTO Invoices
                        (
                            RequestID,
                            InvoiceNumber,
                            Amount,
                            InvoiceDate,
                            Status
                        )
                        VALUES
                        (
                            @RequestID,
                            @InvoiceNumber,
                            @Amount,
                            SYSDATETIME(),
                            'Outstanding'
                        )";

                    using SqlCommand invoiceCmd =
                        new SqlCommand(invoiceSql, conn, transaction);

                    invoiceCmd.Parameters.AddWithValue(
                        "@RequestID",
                        requestID);

                    invoiceCmd.Parameters.AddWithValue(
                        "@InvoiceNumber",
                        invoiceNumber);

                    invoiceCmd.Parameters.AddWithValue(
                        "@Amount",
                        serviceRate);

                    invoiceCmd.ExecuteNonQuery();

                    string completeSql = @"
                        UPDATE ServiceRequests
                        SET Status = 'Completed'
                        WHERE RequestID = @RequestID
                          AND Status = 'In Progress'";

                    using SqlCommand completeCmd =
                        new SqlCommand(completeSql, conn, transaction);

                    completeCmd.Parameters.AddWithValue(
                        "@RequestID",
                        requestID);

                    int updated = completeCmd.ExecuteNonQuery();

                    if (updated == 0)
                    {
                        throw new Exception(
                            "The service request could not be marked as completed.");
                    }

                    /*
                     * Provider availability is recalculated here.
                     * If the provider has another active job, remain unavailable.
                     * Otherwise, make the provider available again.
                     */
                    string availabilitySql = @"
                        UPDATE sp
                        SET AvailabilityStatus =
                            CASE
                                WHEN EXISTS
                                (
                                    SELECT 1
                                    FROM ServiceRequests sr2
                                    INNER JOIN ProviderServices ps2
                                        ON sr2.ProviderServiceID =
                                           ps2.ProviderServiceID
                                    WHERE ps2.ProviderID = sp.ProviderID
                                      AND sr2.Status IN
                                          ('Accepted', 'In Progress')
                                )
                                THEN 'Unavailable'
                                ELSE 'Available'
                            END
                        FROM ServiceProviders sp
                        WHERE sp.ProviderID = @ProviderID";

                    using SqlCommand availabilityCmd =
                        new SqlCommand(
                            availabilitySql,
                            conn,
                            transaction);

                    availabilityCmd.Parameters.AddWithValue(
                        "@ProviderID",
                        providerID);

                    availabilityCmd.ExecuteNonQuery();

                    transaction.Commit();

                    AuditService.Log(
                        Session.UserID,
                        "COMPLETE_SERVICE",
                        $"Service request {requestID} completed. " +
                        $"Duration: {durationMinutes} minutes. " +
                        $"Invoice: {invoiceNumber}. " +
                        $"Amount: {serviceRate:C}");

                    MessageBox.Show(
                        "Job completed successfully.\n\n" +
                        $"Duration: {durationMinutes} minutes\n" +
                        $"Invoice: {invoiceNumber}\n" +
                        $"Amount: {serviceRate:C}\n\n" +
                        "The customer can now pay the invoice.",
                        "AIOS",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    LoadJobs();
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
                    "Unable to complete the job.\n\n" + ex.Message,
                    "AIOS",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void BtnRefresh_Click(object? sender, EventArgs e)
        {
            LoadJobs();
        }
    }
}
