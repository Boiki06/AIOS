// Customer screen for displaying and updating the signed-in customer's profile details.
using Microsoft.Data.SqlClient;
using AIOS.DataAccess;
using AIOS.Services;

namespace AIOS.Forms
{
    // Owns customer profile loading, editing, validation, and saving.
    public class CustomerProfileForm : Form
    {
        private TextBox txtFullName = new();
        private TextBox txtEmail = new();
        private TextBox txtPhone = new();
        private TextBox txtAddress = new();
        private ComboBox cmbRegion = new();

        public CustomerProfileForm()
        {
            BuildUI();
            UiTheme.Apply(this);
            LoadProfile();
        }

        private void BuildUI()
        {
            Text = "AIOS - My Customer Profile";

            Width = 650;
            Height = 550;

            StartPosition =
                FormStartPosition.CenterParent;

            Label title = new()
            {
                Text = "My Profile",
                Font = new Font(
                    "Segoe UI",
                    22,
                    FontStyle.Bold),
                Location = new Point(40, 30),
                AutoSize = true
            };

            AddLabel("Full Name", 40, 90);

            txtFullName.SetBounds(
                40, 115, 520, 30);

            AddLabel("Email", 40, 165);

            txtEmail.SetBounds(
                40, 190, 520, 30);

            txtEmail.ReadOnly = true;

            AddLabel("Phone", 40, 240);

            txtPhone.SetBounds(
                40, 265, 520, 30);

            AddLabel("Address", 40, 315);

            txtAddress.SetBounds(
                40, 340, 520, 30);

            AddLabel("Region", 40, 390);

            cmbRegion.SetBounds(
                40, 415, 520, 30);

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

            Button btnSave = new()
            {
                Text = "SAVE CHANGES",
                Location =
                    new Point(40, 465),
                Width = 200,
                Height = 35
            };

            btnSave.Click += SaveProfile;

            Controls.AddRange(
                new Control[]
                {
                    title,
                    txtFullName,
                    txtEmail,
                    txtPhone,
                    txtAddress,
                    cmbRegion,
                    btnSave
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
                    AutoSize = true,
                    Location =
                        new Point(x, y)
                });
        }

        private void LoadProfile()
        {
            try
            {
                using SqlConnection conn =
                    DatabaseConnection.GetConnection();

                conn.Open();

                string sql = @"
                    SELECT
                        u.FullName,
                        u.Email,
                        c.Phone,
                        c.Address,
                        c.Region
                    FROM Users u

                    INNER JOIN Customers c
                        ON u.UserID = c.UserID

                    WHERE u.UserID = @UserID";

                using SqlCommand cmd =
                    new SqlCommand(sql, conn);

                cmd.Parameters.AddWithValue(
                    "@UserID",
                    Session.UserID);

                using SqlDataReader reader =
                    cmd.ExecuteReader();

                if (reader.Read())
                {
                    txtFullName.Text =
                        reader["FullName"]
                        .ToString();

                    txtEmail.Text =
                        reader["Email"]
                        .ToString();

                    txtPhone.Text =
                        reader["Phone"]
                        .ToString();

                    txtAddress.Text =
                        reader["Address"]
                        .ToString();

                    cmbRegion.Text =
                        reader["Region"]
                        .ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Profile Error");
            }
        }

        private void SaveProfile(
            object? sender,
            EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(
                    txtFullName.Text) ||
                string.IsNullOrWhiteSpace(
                    txtPhone.Text) ||
                string.IsNullOrWhiteSpace(
                    txtAddress.Text))
            {
                MessageBox.Show(
                    "Please complete all profile fields.");

                return;
            }

            try
            {
                using SqlConnection conn =
                    DatabaseConnection.GetConnection();

                conn.Open();

                string sql = @"
                    UPDATE Users
                    SET FullName = @FullName
                    WHERE UserID = @UserID;

                    UPDATE Customers
                    SET
                        Phone = @Phone,
                        Address = @Address,
                        Region = @Region
                    WHERE UserID = @UserID;";

                using SqlCommand cmd =
                    new SqlCommand(sql, conn);

                cmd.Parameters.AddWithValue(
                    "@FullName",
                    txtFullName.Text.Trim());

                cmd.Parameters.AddWithValue(
                    "@Phone",
                    txtPhone.Text.Trim());

                cmd.Parameters.AddWithValue(
                    "@Address",
                    txtAddress.Text.Trim());

                cmd.Parameters.AddWithValue(
                    "@Region",
                    cmbRegion.Text);

                cmd.Parameters.AddWithValue(
                    "@UserID",
                    Session.UserID);

                cmd.ExecuteNonQuery();

                AuditService.Log(
                    Session.UserID,
                    "UPDATE PROFILE",
                    "Customer updated profile.");

                Session.Start(
                    Session.UserID,
                    txtFullName.Text.Trim(),
                    Session.Email,
                    Session.Role);

                MessageBox.Show(
                    "Profile updated successfully.");
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