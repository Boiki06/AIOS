// Customer form for entering location, schedule, and description details for a new request.
using AIOS.DataAccess;
using AIOS.Services;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AIOS.Forms
{
    // Owns collection and validation of the details needed to create a service request.
    public class ServiceRequestForm : Form
    {
        private readonly int providerServiceID;
        private readonly int providerID;

        private Label lblProvider = new();
        private Label lblCategory = new();

        private TextBox txtAddress = new();
        private ComboBox cmbRegion = new();
        private TextBox txtWard = new();
        private TextBox txtBlock = new();

        private DateTimePicker dtpDate = new();
        private DateTimePicker dtpStart = new();
        private DateTimePicker dtpEnd = new();

        private TextBox txtDescription = new();

        public ServiceRequestForm(
            int providerServiceID,
            int providerID,
            string providerName,
            string categoryName)
        {
            this.providerServiceID =
                providerServiceID;

            this.providerID =
                providerID;

            BuildUI(
                providerName,
                categoryName);
            UiTheme.Apply(this);
        }

        private void BuildUI(
            string providerName,
            string categoryName)
        {
            Text = "AIOS - Request Service";

            Width = 700;
            Height = 720;

            StartPosition =
                FormStartPosition.CenterParent;

            Label title = new()
            {
                Text = "Request a Service",
                Font = new Font(
                    "Segoe UI",
                    22,
                    FontStyle.Bold),
                Location =
                    new Point(30, 25),
                AutoSize = true
            };

            lblProvider = new Label
            {
                Text = $"Provider: {providerName}",
                Location =
                    new Point(30, 80),
                AutoSize = true,
                Font = new Font(
                    "Segoe UI",
                    11,
                    FontStyle.Bold)
            };

            lblCategory = new Label
            {
                Text = $"Service: {categoryName}",
                Location =
                    new Point(30, 110),
                AutoSize = true
            };

            AddLabel(
                "Service Address",
                30,
                150);

            txtAddress.SetBounds(
                30,
                175,
                580,
                30);

            AddLabel(
                "Region",
                30,
                220);

            cmbRegion.SetBounds(
                30,
                245,
                280,
                30);

            cmbRegion.Items.AddRange(
                new object[]
                {
                    "Gaborone",
                    "Tlokweng",
                    "Mogoditshane",
                    "Broadhurst",
                    "Phakalane",
                    "Kgale",
                    "Block 3",
                    "Block 5",
                    "Block 6",
                    "Block 8",
                    "Block 9",
                    "Block 10",
                    "Block 15",
                    "Other"
                });

            AddLabel(
                "Ward",
                330,
                220);

            txtWard.SetBounds(
                330,
                245,
                280,
                30);

            AddLabel(
                "Block / Area",
                30,
                290);

            txtBlock.SetBounds(
                30,
                315,
                280,
                30);

            AddLabel(
                "Requested Date",
                330,
                290);

            dtpDate.SetBounds(
                330,
                315,
                280,
                30);

            dtpDate.Format =
                DateTimePickerFormat.Short;

            dtpDate.MinDate =
                DateTime.Today;

            AddLabel(
                "Preferred Start Time",
                30,
                360);

            dtpStart.SetBounds(
                30,
                385,
                280,
                30);

            dtpStart.Format =
                DateTimePickerFormat.Time;

            dtpStart.ShowUpDown = true;

            AddLabel(
                "Preferred End Time",
                330,
                360);

            dtpEnd.SetBounds(
                330,
                385,
                280,
                30);

            dtpEnd.Format =
                DateTimePickerFormat.Time;

            dtpEnd.ShowUpDown = true;

            AddLabel(
                "Description",
                30,
                430);

            txtDescription.SetBounds(
                30,
                455,
                580,
                100);

            txtDescription.Multiline = true;

            Button btnSubmit = new()
            {
                Text = "SUBMIT SERVICE REQUEST",
                Location =
                    new Point(30, 590),
                Width = 260,
                Height = 45
            };

            btnSubmit.Click +=
                SubmitRequest;

            Button btnCancel = new()
            {
                Text = "Cancel",
                Location =
                    new Point(310, 590),
                Width = 120,
                Height = 45
            };

            btnCancel.Click +=
                (s, e) => Close();

            Controls.AddRange(
                new Control[]
                {
                    title,
                    lblProvider,
                    lblCategory,
                    txtAddress,
                    cmbRegion,
                    txtWard,
                    txtBlock,
                    dtpDate,
                    dtpStart,
                    dtpEnd,
                    txtDescription,
                    btnSubmit,
                    btnCancel
                });
        }

        private void AddLabel(
            string text,
            int x,
            int y)
        {
            Controls.Add(
                new Label
                {
                    Text = text,
                    Location =
                        new Point(x, y),
                    AutoSize = true
                });
        }

        private void SubmitRequest(
            object? sender,
            EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(
                    txtAddress.Text))
            {
                MessageBox.Show(
                    "Enter the service address.");

                return;
            }

            if (string.IsNullOrWhiteSpace(
                    cmbRegion.Text))
            {
                MessageBox.Show(
                    "Select a region.");

                return;
            }

            if (string.IsNullOrWhiteSpace(
                    txtDescription.Text))
            {
                MessageBox.Show(
                    "Describe the service you require.");

                return;
            }

            DateTime selectedDate =
                dtpDate.Value.Date;

            DateTime startDateTime =
                selectedDate.Add(
                    dtpStart.Value.TimeOfDay);

            DateTime endDateTime =
                selectedDate.Add(
                    dtpEnd.Value.TimeOfDay);

            if (endDateTime <= startDateTime)
            {
                MessageBox.Show(
                    "The end time must be later than the start time.");

                return;
            }

            if (selectedDate < DateTime.Today)
            {
                MessageBox.Show(
                    "The requested date cannot be in the past.");

                return;
            }

            try
            {
                using SqlConnection conn =
                    DatabaseConnection.GetConnection();

                conn.Open();

                /*
                 * First verify that the provider is still
                 * available and not blacklisted.
                 */
                string providerCheck = @"
                    SELECT
                        AvailabilityStatus,
                        IsBlacklisted

                    FROM ServiceProviders

                    WHERE ProviderID =
                        @ProviderID";

                using SqlCommand providerCmd =
                    new SqlCommand(
                        providerCheck,
                        conn);

                providerCmd.Parameters.AddWithValue(
                    "@ProviderID",
                    providerID);

                using SqlDataReader reader =
                    providerCmd.ExecuteReader();

                if (!reader.Read())
                {
                    MessageBox.Show(
                        "The selected provider could not be found.");

                    return;
                }

                string availability =
                    reader["AvailabilityStatus"]
                    .ToString() ?? "";

                bool blacklisted =
                    Convert.ToBoolean(
                        reader["IsBlacklisted"]);

                reader.Close();

                if (blacklisted)
                {
                    MessageBox.Show(
                        "This provider is currently blacklisted.");

                    return;
                }

                if (!availability.Equals(
                        "Available",
                        StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show(
                        "This provider is currently unavailable.");

                    return;
                }

                /*
                 * Make sure the service itself still belongs
                 * to the selected provider.
                 */
                string serviceCheck = @"
                    SELECT COUNT(*)

                    FROM ProviderServices

                    WHERE ProviderServiceID =
                        @ProviderServiceID

                    AND ProviderID =
                        @ProviderID";

                using SqlCommand serviceCmd =
                    new SqlCommand(
                        serviceCheck,
                        conn);

                serviceCmd.Parameters.AddWithValue(
                    "@ProviderServiceID",
                    providerServiceID);

                serviceCmd.Parameters.AddWithValue(
                    "@ProviderID",
                    providerID);

                int serviceExists =
                    Convert.ToInt32(
                        serviceCmd.ExecuteScalar());

                if (serviceExists == 0)
                {
                    MessageBox.Show(
                        "The selected service is no longer available.");

                    return;
                }

                /*
                 * Prevent overlapping appointments for the
                 * same provider.
                 */
                string overlapCheck = @"
                    SELECT COUNT(*)

                    FROM ServiceRequests sr

                    INNER JOIN ProviderServices ps
                        ON sr.ProviderServiceID =
                           ps.ProviderServiceID

                    WHERE ps.ProviderID =
                        @ProviderID

                    AND sr.RequestedDate =
                        @RequestedDate

                    AND sr.Status IN
                    (
                        'Requested',
                        'Accepted',
                        'In Progress'
                    )

                    AND
                    (
                        @StartTime < sr.EndTime
                        AND
                        @EndTime > sr.StartTime
                    )";

                using SqlCommand overlapCmd =
                    new SqlCommand(
                        overlapCheck,
                        conn);

                overlapCmd.Parameters.AddWithValue(
                    "@ProviderID",
                    providerID);

                overlapCmd.Parameters.AddWithValue(
                    "@RequestedDate",
                    selectedDate);

                overlapCmd.Parameters.Add(
                    "@StartTime",
                    SqlDbType.DateTime2)
                    .Value = startDateTime;

                overlapCmd.Parameters.Add(
                    "@EndTime",
                    SqlDbType.DateTime2)
                    .Value = endDateTime;

                int overlaps =
                    Convert.ToInt32(
                        overlapCmd.ExecuteScalar());

                if (overlaps > 0)
                {
                    MessageBox.Show(
                        "The provider already has a service scheduled during that time.");

                    return;
                }

                /*
                 * Find the logged-in customer's ID.
                 */
                string customerSql = @"
                    SELECT CustomerID

                    FROM Customers

                    WHERE UserID =
                        @UserID";

                using SqlCommand customerCmd =
                    new SqlCommand(
                        customerSql,
                        conn);

                customerCmd.Parameters.AddWithValue(
                    "@UserID",
                    Session.UserID);

                object? customerResult =
                    customerCmd.ExecuteScalar();

                if (customerResult == null)
                {
                    MessageBox.Show(
                        "Customer profile could not be found.");

                    return;
                }

                int customerID =
                    Convert.ToInt32(
                        customerResult);

                /*
                 * Insert the actual request.
                 *
                 * StartTime and EndTime are DATETIME2
                 * in your current database.
                 */
                string insertSql = @"
                    INSERT INTO ServiceRequests
                    (
                        CustomerID,
                        ProviderServiceID,
                        ServiceAddress,
                        Region,
                        Ward,
                        Block,
                        RequestedDate,
                        StartTime,
                        EndTime,
                        Description,
                        Status
                    )

                    VALUES
                    (
                        @CustomerID,
                        @ProviderServiceID,
                        @ServiceAddress,
                        @Region,
                        @Ward,
                        @Block,
                        @RequestedDate,
                        @StartTime,
                        @EndTime,
                        @Description,
                        'Requested'
                    )";

                using SqlCommand insertCmd =
                    new SqlCommand(
                        insertSql,
                        conn);

                insertCmd.Parameters.AddWithValue(
                    "@CustomerID",
                    customerID);

                insertCmd.Parameters.AddWithValue(
                    "@ProviderServiceID",
                    providerServiceID);

                insertCmd.Parameters.AddWithValue(
                    "@ServiceAddress",
                    txtAddress.Text.Trim());

                insertCmd.Parameters.AddWithValue(
                    "@Region",
                    cmbRegion.Text.Trim());

                insertCmd.Parameters.AddWithValue(
                    "@Ward",
                    txtWard.Text.Trim());

                insertCmd.Parameters.AddWithValue(
                    "@Block",
                    txtBlock.Text.Trim());

                insertCmd.Parameters.Add(
                    "@RequestedDate",
                    SqlDbType.DateTime2)
                    .Value = selectedDate;

                insertCmd.Parameters.Add(
                    "@StartTime",
                    SqlDbType.DateTime2)
                    .Value = startDateTime;

                insertCmd.Parameters.Add(
                    "@EndTime",
                    SqlDbType.DateTime2)
                    .Value = endDateTime;

                insertCmd.Parameters.AddWithValue(
                    "@Description",
                    txtDescription.Text.Trim());

                insertCmd.ExecuteNonQuery();

                AuditService.Log(
                    Session.UserID,
                    "SERVICE REQUEST",
                    $"Customer created a service request " +
                    $"for ProviderID {providerID}.");

                MessageBox.Show(
                    "Service request submitted successfully.");

                Close();
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