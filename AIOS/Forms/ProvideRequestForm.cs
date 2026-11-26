// Provider screen for reviewing assigned requests and updating their work status.
using AIOS.DataAccess;
using AIOS.Services;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AIOS.Forms
{
    // Owns provider request assignment review and request-status updates.
    public class ProviderRequestsForm : Form
    {
        private DataGridView grid = new();

        public ProviderRequestsForm()
        {
            BuildUI();
            UiTheme.Apply(this);
            LoadRequests();
        }

        private void BuildUI()
        {
            Text = "AIOS - Service Requests";

            Width = 1250;
            Height = 650;

            StartPosition =
                FormStartPosition.CenterParent;

            Label title = new()
            {
                Text = "Incoming Service Requests",
                Font = new Font(
                    "Segoe UI",
                    20,
                    FontStyle.Bold),
                Location =
                    new Point(25, 20),
                AutoSize = true
            };

            Button btnAccept = new()
            {
                Text = "Accept",
                Location =
                    new Point(850, 20),
                Width = 100,
                Height = 35
            };

            btnAccept.Click +=
                (s, e) =>
                    UpdateRequest("Accepted");

            Button btnReject = new()
            {
                Text = "Reject",
                Location =
                    new Point(960, 20),
                Width = 100,
                Height = 35
            };

            btnReject.Click +=
                (s, e) =>
                    UpdateRequest("Rejected");

            Button btnRefresh = new()
            {
                Text = "Refresh",
                Location =
                    new Point(1070, 20),
                Width = 100,
                Height = 35
            };

            btnRefresh.Click +=
                (s, e) => LoadRequests();

            grid.SetBounds(
                20,
                75,
                1190,
                500);

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
                    btnAccept,
                    btnReject,
                    btnRefresh,
                    grid
                });
        }

        private void LoadRequests()
        {
            try
            {
                using SqlConnection conn =
                    DatabaseConnection.GetConnection();

                conn.Open();

                string sql = @"
                    SELECT
                        sr.RequestID,

                        u.FullName
                            AS Customer,

                        sc.CategoryName
                            AS Service,

                        sr.ServiceAddress,

                        sr.Region,
                        sr.Ward,
                        sr.Block,

                        CONVERT(
                            varchar(10),
                            sr.RequestedDate,
                            103
                        ) AS RequestedDate,

                        CONVERT(
                            varchar(5),
                            sr.StartTime,
                            108
                        ) AS StartTime,

                        CONVERT(
                            varchar(5),
                            sr.EndTime,
                            108
                        ) AS EndTime,

                        sr.Description,
                        sr.Status

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

                    WHERE sp.UserID =
                        @UserID

                    AND sr.Status =
                        'Requested'

                    ORDER BY
                        sr.RequestedDate,
                        sr.StartTime";

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
                    "Request Error");
            }
        }

        private void UpdateRequest(
            string newStatus)
        {
            if (grid.SelectedRows.Count == 0)
            {
                MessageBox.Show(
                    "Select a request first.");

                return;
            }

            int requestID =
                Convert.ToInt32(
                    grid.SelectedRows[0]
                        .Cells["RequestID"]
                        .Value);

            try
            {
                using SqlConnection conn =
                    DatabaseConnection.GetConnection();

                conn.Open();

                /*
                 * Make sure the request belongs
                 * to this provider.
                 */
                string sql = @"
                    UPDATE sr

                    SET sr.Status =
                        @Status

                    FROM ServiceRequests sr

                    INNER JOIN ProviderServices ps
                        ON sr.ProviderServiceID =
                           ps.ProviderServiceID

                    INNER JOIN ServiceProviders sp
                        ON ps.ProviderID =
                           sp.ProviderID

                    WHERE
                        sr.RequestID =
                            @RequestID

                    AND sp.UserID =
                        @UserID

                    AND sr.Status =
                        'Requested'";

                using SqlCommand cmd =
                    new SqlCommand(sql, conn);

                cmd.Parameters.AddWithValue(
                    "@Status",
                    newStatus);

                cmd.Parameters.AddWithValue(
                    "@RequestID",
                    requestID);

                cmd.Parameters.AddWithValue(
                    "@UserID",
                    Session.UserID);

                int affected =
                    cmd.ExecuteNonQuery();

                if (affected == 0)
                {
                    MessageBox.Show(
                        "The request could not be updated. It may already have been processed.");

                    return;
                }

                AuditService.Log(
                    Session.UserID,
                    newStatus == "Accepted"
                        ? "ACCEPT SERVICE REQUEST"
                        : "REJECT SERVICE REQUEST",
                    $"Provider changed request " +
                    $"{requestID} to {newStatus}.");

                MessageBox.Show(
                    $"Request {newStatus.ToLower()}.");

                LoadRequests();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Update Error");
            }
        }
    }
}