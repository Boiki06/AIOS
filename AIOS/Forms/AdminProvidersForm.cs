// Administration screen for searching, reviewing, and managing service providers.
using AIOS.DataAccess;
using AIOS.Services;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AIOS.Forms
{
    // Owns provider search, review, and administration actions.
    public class AdminProvidersForm : Form
    {
        private DataGridView grid = new();
        private TextBox txtSearch = new();

        public AdminProvidersForm()
        {
            BuildUI();
            UiTheme.Apply(this);
            LoadProviders();
        }

        private void BuildUI()
        {
            Text = "AIOS - Provider Management";
            Width = 1250;
            Height = 700;
            StartPosition =
                FormStartPosition.CenterParent;

            Label title = new()
            {
                Text = "Service Provider Management",
                Font = new Font(
                    "Segoe UI",
                    20,
                    FontStyle.Bold),
                Location = new Point(25, 20),
                AutoSize = true
            };

            txtSearch.SetBounds(
                25, 70, 300, 30);

            Button btnSearch = new()
            {
                Text = "Search",
                Location = new Point(335, 70),
                Width = 100
            };

            btnSearch.Click +=
                (s, e) => LoadProviders();

            Button btnAvailable = new()
            {
                Text = "Set Available",
                Location = new Point(455, 70),
                Width = 120
            };

            btnAvailable.Click +=
                (s, e) =>
                    UpdateAvailability("Available");

            Button btnUnavailable = new()
            {
                Text = "Set Unavailable",
                Location = new Point(585, 70),
                Width = 130
            };

            btnUnavailable.Click +=
                (s, e) =>
                    UpdateAvailability("Unavailable");

            Button btnBlacklist = new()
            {
                Text = "Blacklist",
                Location = new Point(725, 70),
                Width = 110
            };

            btnBlacklist.Click +=
                BlacklistProvider;

            Button btnRemoveBlacklist = new()
            {
                Text = "Remove Blacklist",
                Location = new Point(845, 70),
                Width = 140
            };

            btnRemoveBlacklist.Click +=
                RemoveBlacklist;

            grid.SetBounds(
                20, 120, 1180, 500);

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
                    txtSearch,
                    btnSearch,
                    btnAvailable,
                    btnUnavailable,
                    btnBlacklist,
                    btnRemoveBlacklist,
                    grid
                });
        }

        private void LoadProviders()
        {
            try
            {
                using SqlConnection conn =
                    DatabaseConnection.GetConnection();

                conn.Open();

                string sql = @"
                    SELECT
                        sp.ProviderID,
                        u.FullName,
                        u.Email,
                        sp.BusinessName,
                        sp.RegistrationNo,
                        sp.ContactNo,
                        sp.Address,
                        sp.Region,
                        sp.Ward,
                        sp.Block,
                        sp.AvailabilityStatus,
                        sp.IsBlacklisted

                    FROM ServiceProviders sp

                    INNER JOIN Users u
                        ON sp.UserID =
                           u.UserID

                    WHERE
                        sp.BusinessName LIKE @Search
                        OR u.FullName LIKE @Search
                        OR sp.Region LIKE @Search
                        OR sp.Ward LIKE @Search
                        OR sp.Block LIKE @Search

                    ORDER BY
                        sp.ProviderID DESC";

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
                    "Provider Error");
            }
        }

        private int GetSelectedProviderID()
        {
            if (grid.SelectedRows.Count == 0)
                return 0;

            return Convert.ToInt32(
                grid.SelectedRows[0]
                    .Cells["ProviderID"]
                    .Value);
        }

        private void UpdateAvailability(
            string status)
        {
            int providerID =
                GetSelectedProviderID();

            if (providerID == 0)
            {
                MessageBox.Show(
                    "Select a provider first.");

                return;
            }

            try
            {
                using SqlConnection conn =
                    DatabaseConnection.GetConnection();

                conn.Open();

                string sql = @"
                    UPDATE ServiceProviders
                    SET AvailabilityStatus =
                        @Status
                    WHERE ProviderID =
                        @ProviderID";

                using SqlCommand cmd =
                    new SqlCommand(sql, conn);

                cmd.Parameters.AddWithValue(
                    "@Status",
                    status);

                cmd.Parameters.AddWithValue(
                    "@ProviderID",
                    providerID);

                cmd.ExecuteNonQuery();

                AuditService.Log(
                    Session.UserID,
                    "UPDATE PROVIDER AVAILABILITY",
                    $"Provider {providerID} set to {status}.");

                MessageBox.Show(
                    $"Provider set to {status}.");

                LoadProviders();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Availability Error");
            }
        }

        private void BlacklistProvider(
            object? sender,
            EventArgs e)
        {
            int providerID =
                GetSelectedProviderID();

            if (providerID == 0)
            {
                MessageBox.Show(
                    "Select a provider first.");

                return;
            }

            DialogResult result =
                MessageBox.Show(
                    "Blacklist this provider?",
                    "Confirm Blacklist",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
                return;

            SetBlacklist(providerID, true);
        }

        private void RemoveBlacklist(
            object? sender,
            EventArgs e)
        {
            int providerID =
                GetSelectedProviderID();

            if (providerID == 0)
            {
                MessageBox.Show(
                    "Select a provider first.");

                return;
            }

            SetBlacklist(providerID, false);
        }

        private void SetBlacklist(
            int providerID,
            bool blacklisted)
        {
            try
            {
                using SqlConnection conn =
                    DatabaseConnection.GetConnection();

                conn.Open();

                string sql = @"
                    UPDATE ServiceProviders
                    SET
                        IsBlacklisted =
                            @Blacklisted,

                        AvailabilityStatus =
                            CASE
                                WHEN @Blacklisted = 1
                                THEN 'Unavailable'
                                ELSE AvailabilityStatus
                            END

                    WHERE ProviderID =
                        @ProviderID";

                using SqlCommand cmd =
                    new SqlCommand(sql, conn);

                cmd.Parameters.AddWithValue(
                    "@Blacklisted",
                    blacklisted);

                cmd.Parameters.AddWithValue(
                    "@ProviderID",
                    providerID);

                cmd.ExecuteNonQuery();

                AuditService.Log(
                    Session.UserID,
                    blacklisted
                        ? "BLACKLIST PROVIDER"
                        : "REMOVE PROVIDER BLACKLIST",
                    $"Provider {providerID} blacklist status " +
                    $"changed to {blacklisted}.");

                MessageBox.Show(
                    blacklisted
                        ? "Provider blacklisted."
                        : "Provider removed from blacklist.");

                LoadProviders();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Blacklist Error");
            }
        }
    }
}