// Administration screen for searching users and changing account status or roles.
using AIOS.DataAccess;
using AIOS.Services;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AIOS.Forms
{
    // Owns user lookup and account-management actions.
    public class AdminUsersForm : Form
    {
        private DataGridView grid = new();
        private TextBox txtSearch = new();

        public AdminUsersForm()
        {
            BuildUI();
            UiTheme.Apply(this);
            LoadUsers();
        }

        private void BuildUI()
        {
            Text = "AIOS - User Management";
            Width = 1100;
            Height = 650;
            StartPosition =
                FormStartPosition.CenterParent;

            Label title = new()
            {
                Text = "User Management",
                Font = new Font(
                    "Segoe UI",
                    20,
                    FontStyle.Bold),
                Location = new Point(25, 20),
                AutoSize = true
            };

            Label searchLabel = new()
            {
                Text = "Search:",
                Location = new Point(25, 70),
                AutoSize = true
            };

            txtSearch.SetBounds(
                90, 65, 300, 30);

            Button btnSearch = new()
            {
                Text = "Search",
                Location = new Point(400, 65),
                Width = 100,
                Height = 30
            };

            btnSearch.Click +=
                (s, e) => LoadUsers();

            Button btnActivate = new()
            {
                Text = "Activate",
                Location = new Point(520, 65),
                Width = 110,
                Height = 30
            };

            btnActivate.Click +=
                (s, e) => UpdateUserStatus(true);

            Button btnDeactivate = new()
            {
                Text = "Deactivate",
                Location = new Point(640, 65),
                Width = 110,
                Height = 30
            };

            btnDeactivate.Click +=
                (s, e) => UpdateUserStatus(false);

            grid.SetBounds(
                20, 120, 1040, 450);

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
                    searchLabel,
                    txtSearch,
                    btnSearch,
                    btnActivate,
                    btnDeactivate,
                    grid
                });
        }

        private void LoadUsers()
        {
            try
            {
                using SqlConnection conn =
                    DatabaseConnection.GetConnection();

                conn.Open();

                string sql = @"
                    SELECT
                        UserID,
                        FullName,
                        Email,
                        Role,
                        IsActive
                    FROM Users
                    WHERE
                        FullName LIKE @Search
                        OR Email LIKE @Search
                        OR Role LIKE @Search
                    ORDER BY UserID DESC";

                using SqlCommand cmd =
                    new SqlCommand(sql, conn);

                cmd.Parameters.AddWithValue(
                    "@Search",
                    "%" + txtSearch.Text.Trim() + "%");

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
                    "User Management Error");
            }
        }

        private void UpdateUserStatus(
            bool active)
        {
            if (grid.SelectedRows.Count == 0)
            {
                MessageBox.Show(
                    "Select a user first.");

                return;
            }

            int userID =
                Convert.ToInt32(
                    grid.SelectedRows[0]
                        .Cells["UserID"]
                        .Value);

            if (userID == Session.UserID)
            {
                MessageBox.Show(
                    "You cannot deactivate your own account.");

                return;
            }

            try
            {
                using SqlConnection conn =
                    DatabaseConnection.GetConnection();

                conn.Open();

                string sql = @"
                    UPDATE Users
                    SET IsActive = @IsActive
                    WHERE UserID = @UserID";

                using SqlCommand cmd =
                    new SqlCommand(sql, conn);

                cmd.Parameters.AddWithValue(
                    "@IsActive",
                    active);

                cmd.Parameters.AddWithValue(
                    "@UserID",
                    userID);

                cmd.ExecuteNonQuery();

                AuditService.Log(
                    Session.UserID,
                    active
                        ? "ACTIVATE USER"
                        : "DEACTIVATE USER",
                    $"Administrator changed UserID " +
                    $"{userID} active status to {active}.");

                MessageBox.Show(
                    active
                        ? "User activated."
                        : "User deactivated.");

                LoadUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Status Error");
            }
        }
    }
}