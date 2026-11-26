// Customer screen for rating a completed request and submitting written feedback.
using Microsoft.Data.SqlClient;
using AIOS.DataAccess;
using AIOS.Services;

namespace AIOS.Forms
{
    // Owns feedback validation and submission for completed customer requests.
    public class CustomerFeedbackForm : Form
    {
        private ComboBox cmbRequest = new();
        private NumericUpDown numRating = new();
        private TextBox txtComments = new();

        public CustomerFeedbackForm()
        {
            BuildUI();
            UiTheme.Apply(this);
            LoadCompletedRequests();
        }

        private void BuildUI()
        {
            Text = "AIOS - Customer Feedback";

            Width = 650;
            Height = 500;

            StartPosition =
                FormStartPosition.CenterParent;

            AddLabel(
                "Completed Service",
                40,
                40);

            cmbRequest.SetBounds(
                40,
                70,
                520,
                30);

            AddLabel(
                "Rating (1 - 5)",
                40,
                120);

            numRating.SetBounds(
                40,
                150,
                150,
                30);

            numRating.Minimum = 1;
            numRating.Maximum = 5;
            numRating.Value = 5;

            AddLabel(
                "Comments",
                40,
                200);

            txtComments.SetBounds(
                40,
                230,
                520,
                100);

            txtComments.Multiline = true;

            Button btnSubmit = new()
            {
                Text = "SUBMIT FEEDBACK",
                Location =
                    new Point(40, 360),
                Width = 220,
                Height = 40
            };

            btnSubmit.Click +=
                SubmitFeedback;

            Controls.AddRange(
                new Control[]
                {
                    cmbRequest,
                    numRating,
                    txtComments,
                    btnSubmit
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

        private void LoadCompletedRequests()
        {
            try
            {
                using SqlConnection conn =
                    DatabaseConnection.GetConnection();

                conn.Open();

                string sql = @"
                    SELECT
                        sr.RequestID,
                        sp.ProviderID,
                        sp.BusinessName,
                        sc.CategoryName

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

                    AND sr.Status =
                        'Completed'

                    AND NOT EXISTS
                    (
                        SELECT 1
                        FROM Feedback f
                        WHERE f.RequestID =
                            sr.RequestID
                    )

                    ORDER BY
                        sr.RequestID DESC";

                using SqlCommand cmd =
                    new SqlCommand(sql, conn);

                cmd.Parameters.AddWithValue(
                    "@UserID",
                    Session.UserID);

                using SqlDataReader reader =
                    cmd.ExecuteReader();

                while (reader.Read())
                {
                    cmbRequest.Items.Add(
                        new FeedbackRequest
                        {
                            RequestID =
                                Convert.ToInt32(
                                    reader["RequestID"]),

                            ProviderID =
                                Convert.ToInt32(
                                    reader["ProviderID"]),

                            Description =
                                $"{reader["BusinessName"]} - " +
                                $"{reader["CategoryName"]}"
                        });
                }

                if (cmbRequest.Items.Count > 0)
                    cmbRequest.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Feedback Error");
            }
        }

        private void SubmitFeedback(
            object? sender,
            EventArgs e)
        {
            if (cmbRequest.SelectedItem
                == null)
            {
                MessageBox.Show(
                    "Select a completed service.");

                return;
            }

            FeedbackRequest request =
                (FeedbackRequest)
                cmbRequest.SelectedItem;

            string flag =
                numRating.Value <= 2
                    ? "Flagged"
                    : "Normal";

            try
            {
                using SqlConnection conn =
                    DatabaseConnection.GetConnection();

                conn.Open();

                string sql = @"
                    INSERT INTO Feedback
                    (
                        RequestID,
                        ProviderID,
                        CustomerID,
                        Rating,
                        Comments,
                        FlagStatus
                    )
                    VALUES
                    (
                        @RequestID,
                        @ProviderID,
                        @CustomerID,
                        @Rating,
                        @Comments,
                        @FlagStatus
                    )";

                using SqlCommand cmd =
                    new SqlCommand(sql, conn);

                cmd.Parameters.AddWithValue(
                    "@RequestID",
                    request.RequestID);

                cmd.Parameters.AddWithValue(
                    "@ProviderID",
                    request.ProviderID);

                cmd.Parameters.AddWithValue(
                    "@CustomerID",
                    GetCustomerID());

                cmd.Parameters.AddWithValue(
                    "@Rating",
                    (int)numRating.Value);

                cmd.Parameters.AddWithValue(
                    "@Comments",
                    txtComments.Text.Trim());

                cmd.Parameters.AddWithValue(
                    "@FlagStatus",
                    flag);

                cmd.ExecuteNonQuery();

                AuditService.Log(
                    Session.UserID,
                    "FEEDBACK",
                    $"Customer submitted rating " +
                    $"{numRating.Value} for request " +
                    $"{request.RequestID}.");

                MessageBox.Show(
                    flag == "Flagged"
                        ? "Feedback submitted and flagged for administrator review."
                        : "Feedback submitted successfully.");

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Feedback Error");
            }
        }

        private int GetCustomerID()
        {
            using SqlConnection conn =
                DatabaseConnection.GetConnection();

            conn.Open();

            string sql = @"
                SELECT CustomerID
                FROM Customers
                WHERE UserID = @UserID";

            using SqlCommand cmd =
                new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue(
                "@UserID",
                Session.UserID);

            return Convert.ToInt32(
                cmd.ExecuteScalar());
        }

        private class FeedbackRequest
        {
            public int RequestID { get; set; }

            public int ProviderID { get; set; }

            public string Description { get; set; } =
                "";

            public override string ToString()
            {
                return Description;
            }
        }
    }
}