// Provider screen for displaying and updating business and contact information.
using Microsoft.Data.SqlClient;
using AIOS.DataAccess;
using AIOS.Services;

namespace AIOS.Forms
{
    // Owns provider business-profile loading, editing, and saving.
    public class ProviderProfileForm : Form
    {
        private TextBox txtFullName = new();
        private TextBox txtEmail = new();
        private TextBox txtBusinessName = new();
        private TextBox txtRegistrationNo = new();
        private TextBox txtContact = new();
        private TextBox txtAddress = new();
        private ComboBox cmbRegion = new();
        private TextBox txtWard = new();
        private TextBox txtBlock = new();
        private ComboBox cmbAvailability = new();

        public ProviderProfileForm()
        {
            BuildUI();
            UiTheme.Apply(this);
            LoadProfile();
        }

        private void BuildUI()
        {
            Text = "AIOS - Provider Profile";

            Width = 750;
            Height = 700;

            StartPosition =
                FormStartPosition.CenterParent;

            int left = 40;
            int right = 380;
            int y = 30;

            AddLabel(
                "Full Name",
                left,
                y);

            txtFullName.SetBounds(
                left,
                y + 25,
                280,
                30);

            AddLabel(
                "Email",
                right,
                y);

            txtEmail.SetBounds(
                right,
                y + 25,
                280,
                30);

            txtEmail.ReadOnly = true;

            y += 70;

            AddLabel(
                "Business Name",
                left,
                y);

            txtBusinessName.SetBounds(
                left,
                y + 25,
                280,
                30);

            AddLabel(
                "Registration Number",
                right,
                y);

            txtRegistrationNo.SetBounds(
                right,
                y + 25,
                280,
                30);

            y += 70;

            AddLabel(
                "Contact Number",
                left,
                y);

            txtContact.SetBounds(
                left,
                y + 25,
                280,
                30);

            AddLabel(
                "Address",
                right,
                y);

            txtAddress.SetBounds(
                right,
                y + 25,
                280,
                30);

            y += 70;

            AddLabel(
                "Region",
                left,
                y);

            cmbRegion.SetBounds(
                left,
                y + 25,
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
                right,
                y);

            txtWard.SetBounds(
                right,
                y + 25,
                280,
                30);

            y += 70;

            AddLabel(
                "Block / Area",
                left,
                y);

            txtBlock.SetBounds(
                left,
                y + 25,
                280,
                30);

            AddLabel(
                "Availability",
                right,
                y);

            cmbAvailability.SetBounds(
                right,
                y + 25,
                280,
                30);

            cmbAvailability.Items.AddRange(
                new object[]
                {
                    "Available",
                    "Unavailable"
                });

            y += 70;

            Button btnSave = new()
            {
                Text = "SAVE PROFILE",
                Location =
                    new Point(40, y),
                Width = 200,
                Height = 40
            };

            btnSave.Click += SaveProfile;

            Controls.AddRange(
                new Control[]
                {
                    txtFullName,
                    txtEmail,
                    txtBusinessName,
                    txtRegistrationNo,
                    txtContact,
                    txtAddress,
                    cmbRegion,
                    txtWard,
                    txtBlock,
                    cmbAvailability,
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
                        sp.BusinessName,
                        sp.RegistrationNo,
                        sp.ContactNo,
                        sp.Address,
                        sp.Region,
                        sp.Ward,
                        sp.Block,
                        sp.AvailabilityStatus
                    FROM Users u

                    INNER JOIN ServiceProviders sp
                        ON u.UserID =
                        sp.UserID

                    WHERE u.UserID =
                        @UserID";

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
                        reader["FullName"].ToString();

                    txtEmail.Text =
                        reader["Email"].ToString();

                    txtBusinessName.Text =
                        reader["BusinessName"].ToString();

                    txtRegistrationNo.Text =
                        reader["RegistrationNo"].ToString();

                    txtContact.Text =
                        reader["ContactNo"].ToString();

                    txtAddress.Text =
                        reader["Address"].ToString();

                    cmbRegion.Text =
                        reader["Region"].ToString();

                    txtWard.Text =
                        reader["Ward"].ToString();

                    txtBlock.Text =
                        reader["Block"].ToString();

                    cmbAvailability.Text =
                        reader["AvailabilityStatus"]
                        .ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Provider Profile Error");
            }
        }

        private void SaveProfile(
            object? sender,
            EventArgs e)
        {
            try
            {
                using SqlConnection conn =
                    DatabaseConnection.GetConnection();

                conn.Open();

                string sql = @"
                    UPDATE Users
                    SET FullName =
                        @FullName

                    WHERE UserID =
                        @UserID;

                    UPDATE ServiceProviders
                    SET
                        BusinessName =
                            @BusinessName,

                        RegistrationNo =
                            @RegistrationNo,

                        ContactNo =
                            @ContactNo,

                        Address =
                            @Address,

                        Region =
                            @Region,

                        Ward =
                            @Ward,

                        Block =
                            @Block,

                        AvailabilityStatus =
                            @Availability

                    WHERE UserID =
                        @UserID";

                using SqlCommand cmd =
                    new SqlCommand(sql, conn);

                cmd.Parameters.AddWithValue(
                    "@FullName",
                    txtFullName.Text.Trim());

                cmd.Parameters.AddWithValue(
                    "@BusinessName",
                    txtBusinessName.Text.Trim());

                cmd.Parameters.AddWithValue(
                    "@RegistrationNo",
                    txtRegistrationNo.Text.Trim());

                cmd.Parameters.AddWithValue(
                    "@ContactNo",
                    txtContact.Text.Trim());

                cmd.Parameters.AddWithValue(
                    "@Address",
                    txtAddress.Text.Trim());

                cmd.Parameters.AddWithValue(
                    "@Region",
                    cmbRegion.Text);

                cmd.Parameters.AddWithValue(
                    "@Ward",
                    txtWard.Text.Trim());

                cmd.Parameters.AddWithValue(
                    "@Block",
                    txtBlock.Text.Trim());

                cmd.Parameters.AddWithValue(
                    "@Availability",
                    cmbAvailability.Text);

                cmd.Parameters.AddWithValue(
                    "@UserID",
                    Session.UserID);

                cmd.ExecuteNonQuery();

                AuditService.Log(
                    Session.UserID,
                    "UPDATE PROVIDER PROFILE",
                    "Provider updated profile.");

                MessageBox.Show(
                    "Provider profile updated successfully.");
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