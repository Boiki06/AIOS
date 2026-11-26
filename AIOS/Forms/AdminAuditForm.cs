// Administration screen for reviewing recorded user and system activity.
using Microsoft.Data.SqlClient;
using AIOS.DataAccess;
using AIOS.Services;
using System;
using System.Data;
using System.Windows.Forms;

namespace AIOS.Forms
{
    // Owns audit-log loading, filtering, and presentation for administrators.
    public class AdminAuditForm : Form
    {
        private DataGridView dgvAudit;
        private TextBox txtSearch;
        private Button btnSearch;
        private Button btnRefresh;
        private Label lblTitle;

        public AdminAuditForm()
        {
            InitializeComponent();
            UiTheme.Apply(this);
            LoadAuditLogs();
        }

        private void InitializeComponent()
        {
            Text = "AIOS - Audit Logs";
            Width = 1100;
            Height = 650;
            StartPosition = FormStartPosition.CenterScreen;

            lblTitle = new Label
            {
                Text = "Audit Logs",
                Left = 20,
                Top = 20,
                Width = 400,
                Height = 40,
                Font = new System.Drawing.Font(
                    "Segoe UI",
                    20,
                    System.Drawing.FontStyle.Bold)
            };

            txtSearch = new TextBox
            {
                Left = 20,
                Top = 80,
                Width = 300,
                Height = 35
            };

            btnSearch = new Button
            {
                Text = "Search",
                Left = 330,
                Top = 78,
                Width = 100,
                Height = 35
            };

            btnRefresh = new Button
            {
                Text = "Refresh",
                Left = 440,
                Top = 78,
                Width = 100,
                Height = 35
            };

            dgvAudit = new DataGridView
            {
                Left = 20,
                Top = 130,
                Width = 1040,
                Height = 440,
                AutoSizeColumnsMode =
                    DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                SelectionMode =
                    DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false
            };

            btnSearch.Click += BtnSearch_Click;
            btnRefresh.Click += BtnRefresh_Click;

            Controls.Add(lblTitle);
            Controls.Add(txtSearch);
            Controls.Add(btnSearch);
            Controls.Add(btnRefresh);
            Controls.Add(dgvAudit);
        }

        private void LoadAuditLogs()
        {
            try
            {
                using SqlConnection conn =
                    DatabaseConnection.GetConnection();

                conn.Open();

                string sql = @"
                    SELECT
                        u.FullName AS UserName,
                        a.Action,
                        a.Description,
                        a.CreatedAt
                    FROM AuditLogs a
                    LEFT JOIN Users u
                        ON a.UserID = u.UserID
                    ORDER BY a.CreatedAt DESC";

                using SqlCommand cmd =
                    new SqlCommand(sql, conn);

                using SqlDataAdapter adapter =
                    new SqlDataAdapter(cmd);

                DataTable table = new DataTable();

                adapter.Fill(table);

                dgvAudit.DataSource = table;

                if (dgvAudit.Columns.Contains("UserName"))
                    dgvAudit.Columns["UserName"]
                        .HeaderText = "User";

                if (dgvAudit.Columns.Contains("Action"))
                    dgvAudit.Columns["Action"]
                        .HeaderText = "Action";

                if (dgvAudit.Columns.Contains("Description"))
                    dgvAudit.Columns["Description"]
                        .HeaderText = "Description";

                if (dgvAudit.Columns.Contains("CreatedAt"))
                    dgvAudit.Columns["CreatedAt"]
                        .HeaderText = "Date & Time";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unable to load audit logs.\n\n" +
                    ex.Message,
                    "Audit Log Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void SearchAuditLogs()
        {
            try
            {
                using SqlConnection conn =
                    DatabaseConnection.GetConnection();

                conn.Open();

                string sql = @"
                    SELECT
                        u.FullName AS UserName,
                        a.Action,
                        a.Description,
                        a.CreatedAt
                    FROM AuditLogs a
                    LEFT JOIN Users u
                        ON a.UserID = u.UserID
                    WHERE
                        u.FullName LIKE @Search
                        OR a.Action LIKE @Search
                        OR a.Description LIKE @Search
                    ORDER BY a.CreatedAt DESC";

                using SqlCommand cmd =
                    new SqlCommand(sql, conn);

                cmd.Parameters.AddWithValue(
                    "@Search",
                    "%" + txtSearch.Text.Trim() + "%");

                using SqlDataAdapter adapter =
                    new SqlDataAdapter(cmd);

                DataTable table = new DataTable();

                adapter.Fill(table);

                dgvAudit.DataSource = table;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unable to search audit logs.\n\n" +
                    ex.Message,
                    "Audit Log Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void BtnSearch_Click(
            object? sender,
            EventArgs e)
        {
            SearchAuditLogs();
        }

        private void BtnRefresh_Click(
            object? sender,
            EventArgs e)
        {
            txtSearch.Clear();
            LoadAuditLogs();
        }
    }
}