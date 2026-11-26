// Customer screen that lists the signed-in user's active and historical requests.
using AIOS.DataAccess;
using AIOS.Services;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AIOS.Forms
{
    // Owns retrieval and presentation of the current customer's requests.
    public class MyServiceRequestsForm : Form
    {
        private DataGridView grid = new();

        public MyServiceRequestsForm()
        {
            BuildUI();
            UiTheme.Apply(this);
            LoadRequests();
        }

        private void BuildUI()
        {
            Text = "AIOS - My Service Requests";

            Width = 1250;
            Height = 650;

            StartPosition =
                FormStartPosition.CenterParent;

            Label title = new()
            {
                Text = "My Service Requests",
                Font = new Font(
                    "Segoe UI",
                    20,
                    FontStyle.Bold),
                Location =
                    new Point(25, 20),
                AutoSize = true
            };

            Button refresh = new()
            {
                Text = "Refresh",
                Location =
                    new Point(1050, 20),
                Width = 100,
                Height = 35
            };

            refresh.Click +=
                (s, e) => LoadRequests();

            grid.SetBounds(
                20,
                75,
                1190,
                500);

            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;

            grid.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;

            grid.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            Controls.AddRange(
                new Control[]
                {
                    title,
                    refresh,
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

                        sp.BusinessName
                            AS Provider,

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

                        sr.Status,
                        sr.Description

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
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Request Error");
            }
        }
    }
}