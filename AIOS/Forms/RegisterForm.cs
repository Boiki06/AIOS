// Registration screen that validates new account details and stores a protected password hash.
using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using AIOS.DataAccess;
using AIOS.Services;

namespace AIOS.Forms
{
    // Owns new-account validation, password hashing, and user creation.
    public class RegisterForm : Form
    {
        private TextBox txtName = new TextBox();
        private TextBox txtEmail = new TextBox();
        private TextBox txtPhone = new TextBox();
        private TextBox txtPassword = new TextBox();
        private TextBox txtAddress = new TextBox();
        private TextBox txtRegion = new TextBox();
        private TextBox txtBusinessName = new TextBox();
        private TextBox txtRegistrationNo = new TextBox();
        private TextBox txtRate = new TextBox();

        private Label lblAvailability = new Label();
        private ComboBox cmbAvailability = new ComboBox();

        private ComboBox cmbRole = new ComboBox();
        private ComboBox cmbCategory = new ComboBox();
        private Button btnRegister = new Button();

        public RegisterForm()
        {
            Text = "AIOS - User Registration";
            Size = new Size(500, 850);
            StartPosition = FormStartPosition.CenterScreen;
            AutoScroll = true;
            MinimumSize = new Size(500, 650);

            int y = 20;

            AddLabel("Full name", ref y);
            AddTextBox(txtName, ref y);

            AddLabel("Email address", ref y);
            AddTextBox(txtEmail, ref y);

            AddLabel("Phone number", ref y);
            AddTextBox(txtPhone, ref y);

            AddLabel("Password (minimum 12 characters)", ref y);
            txtPassword.UseSystemPasswordChar = true;
            AddTextBox(txtPassword, ref y);

            AddLabel("Account type", ref y);

            cmbRole.Location = new Point(30, y);
            cmbRole.Size = new Size(400, 28);
            cmbRole.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRole.Items.AddRange(new object[]
            {
                "Customer",
                "Provider"
            });
            cmbRole.SelectedIndex = 0;
            Controls.Add(cmbRole);
            y += 40;

            AddLabel("Address", ref y);
            AddTextBox(txtAddress, ref y);

            AddLabel("Region / Ward / Block (providers only)", ref y);
            AddTextBox(txtRegion, ref y);

            AddLabel("Business name (providers only)", ref y);
            AddTextBox(txtBusinessName, ref y);

            AddLabel("Business registration number (optional)", ref y);
            AddTextBox(txtRegistrationNo, ref y);

            AddLabel("Service category (providers only)", ref y);

            cmbCategory.Location = new Point(30, y);
            cmbCategory.Size = new Size(400, 28);
            cmbCategory.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCategory.Items.AddRange(new object[]
            {
                "Plumbing",
                "Electrical",
                "Gardening",
                "Cleaning",
                "Babysitting"
            });
            cmbCategory.SelectedIndex = 0;
            Controls.Add(cmbCategory);
            y += 40;

            AddLabel("Service rate (providers only)", ref y);
            AddTextBox(txtRate, ref y);

            // Provider availability status
            lblAvailability.Text = "Availability status (providers only)";
            lblAvailability.Location = new Point(30, y);
            lblAvailability.AutoSize = true;
            Controls.Add(lblAvailability);
            y += 22;

            cmbAvailability.Location = new Point(30, y);
            cmbAvailability.Size = new Size(400, 28);
            cmbAvailability.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbAvailability.Items.AddRange(new object[]
            {
                "Available",
                "Unavailable"
            });
            cmbAvailability.SelectedIndex = 0;
            Controls.Add(cmbAvailability);
            y += 45;

            btnRegister.Text = "Register Account";
            btnRegister.Location = new Point(30, y + 10);
            btnRegister.Size = new Size(400, 40);
            btnRegister.Click += BtnRegister_Click;
            Controls.Add(btnRegister);

            // Update provider-only fields when the account type changes.
            cmbRole.SelectedIndexChanged += (s, e) =>
            {
                UpdateRoleFields();
            };

            UpdateRoleFields();
            UiTheme.Apply(this);
        }

        private void AddLabel(string text, ref int y)
        {
            Label label = new Label
            {
                Text = text,
                Location = new Point(30, y),
                AutoSize = true
            };

            Controls.Add(label);
            y += 22;
        }

        private void AddTextBox(TextBox box, ref int y)
        {
            box.Location = new Point(30, y);
            box.Size = new Size(400, 25);
            Controls.Add(box);
            y += 38;
        }

        private void UpdateRoleFields()
        {
            bool isProvider = cmbRole.Text == "Provider";

            txtBusinessName.Enabled = isProvider;
            txtRegistrationNo.Enabled = isProvider;
            cmbCategory.Enabled = isProvider;
            txtRate.Enabled = isProvider;
            txtRegion.Enabled = isProvider;

            lblAvailability.Enabled = isProvider;
            cmbAvailability.Enabled = isProvider;
        }

        private void BtnRegister_Click(object sender, EventArgs e)
        {
            string name = txtName.Text.Trim();
            string email = txtEmail.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string password = txtPassword.Text;
            string address = txtAddress.Text.Trim();
            string role = cmbRole.Text;

            string region = txtRegion.Text.Trim();
            string businessName = txtBusinessName.Text.Trim();
            string registrationNo = txtRegistrationNo.Text.Trim();
            string category = cmbCategory.Text;
            string availabilityStatus = cmbAvailability.Text;

            // Validate the common required fields.
            if (string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(phone) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(address))
            {
                MessageBox.Show(
                    "Complete all required fields.",
                    "Validation",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(role))
            {
                MessageBox.Show("Select an account type.");
                return;
            }

            if (password.Length < 12)
            {
                MessageBox.Show(
                    "Password must contain at least 12 characters.",
                    "Validation",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // Validate provider-specific fields.
            decimal rate = 0;

            if (role == "Provider")
            {
                if (string.IsNullOrWhiteSpace(businessName))
                {
                    MessageBox.Show("Enter the business name.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(region))
                {
                    MessageBox.Show(
                        "Enter the provider's region, ward or block.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(category))
                {
                    MessageBox.Show("Select a service category.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(availabilityStatus))
                {
                    MessageBox.Show("Select an availability status.");
                    return;
                }

                if (!decimal.TryParse(txtRate.Text.Trim(), out rate) ||
                    rate < 0)
                {
                    MessageBox.Show(
                        "Enter a valid, non-negative service rate.");
                    return;
                }
            }

            string membershipID =
                "AIOS-" + Guid.NewGuid().ToString("N").Substring(0, 10);

            string passwordHash =
                PasswordService.HashPassword(password);

            using (SqlConnection connection =
                DatabaseConnection.GetConnection())
            {
                connection.Open();

                using (SqlTransaction transaction =
                    connection.BeginTransaction())
                {
                    try
                    {
                        // Create the user account.
                        string userSql = @"
INSERT INTO Users
    (FullName, Email, PasswordHash, Role)
OUTPUT INSERTED.UserID
VALUES
    (@Name, @Email, @Hash, @Role);";

                        int userID;

                        using (SqlCommand cmd =
                            new SqlCommand(
                                userSql,
                                connection,
                                transaction))
                        {
                            cmd.Parameters.AddWithValue("@Name", name);
                            cmd.Parameters.AddWithValue("@Email", email);
                            cmd.Parameters.AddWithValue("@Hash", passwordHash);
                            cmd.Parameters.AddWithValue("@Role", role);

                            userID = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        if (role == "Customer")
                        {
                            // Create the customer record.
                            string customerSql = @"
INSERT INTO Customers
    (UserID, MembershipID, Phone, Address)
VALUES
    (@UserID, @MembershipID, @Phone, @Address);";

                            using (SqlCommand cmd =
                                new SqlCommand(
                                    customerSql,
                                    connection,
                                    transaction))
                            {
                                cmd.Parameters.AddWithValue("@UserID", userID);
                                cmd.Parameters.AddWithValue(
                                    "@MembershipID", membershipID);
                                cmd.Parameters.AddWithValue("@Phone", phone);
                                cmd.Parameters.AddWithValue("@Address", address);

                                cmd.ExecuteNonQuery();
                            }
                        }
                        else if (role == "Provider")
                        {
                            // Create the service provider record.
                            // The selected availability is saved to SQL Server.
                            string providerSql = @"
INSERT INTO ServiceProviders
    (UserID, BusinessName, RegistrationNo, ContactNo,
     Address, Region, AvailabilityStatus)
OUTPUT INSERTED.ProviderID
VALUES
    (@UserID, @BusinessName, @RegistrationNo, @Phone,
     @Address, @Region, @AvailabilityStatus);";

                            int providerID;

                            using (SqlCommand cmd =
                                new SqlCommand(
                                    providerSql,
                                    connection,
                                    transaction))
                            {
                                cmd.Parameters.AddWithValue("@UserID", userID);

                                cmd.Parameters.AddWithValue(
                                    "@BusinessName", businessName);

                                cmd.Parameters.AddWithValue(
                                    "@RegistrationNo",
                                    string.IsNullOrWhiteSpace(registrationNo)
                                        ? (object)DBNull.Value
                                        : registrationNo);

                                cmd.Parameters.AddWithValue("@Phone", phone);
                                cmd.Parameters.AddWithValue("@Address", address);
                                cmd.Parameters.AddWithValue("@Region", region);

                                cmd.Parameters.AddWithValue(
                                    "@AvailabilityStatus",
                                    availabilityStatus);

                                providerID = Convert.ToInt32(
                                    cmd.ExecuteScalar());
                            }

                            // Find the selected service category.
                            string categorySql = @"
SELECT CategoryID
FROM ServiceCategories
WHERE CategoryName = @Category;";

                            int categoryID;

                            using (SqlCommand cmd =
                                new SqlCommand(
                                    categorySql,
                                    connection,
                                    transaction))
                            {
                                cmd.Parameters.AddWithValue(
                                    "@Category", category);

                                object result = cmd.ExecuteScalar();

                                if (result == null || result == DBNull.Value)
                                {
                                    throw new Exception(
                                        "The selected service category " +
                                        "does not exist in ServiceCategories.");
                                }

                                categoryID = Convert.ToInt32(result);
                            }

                            // Link the provider to their service category.
                            string serviceSql = @"
INSERT INTO ProviderServices
    (ProviderID, CategoryID, ServiceRate)
VALUES
    (@ProviderID, @CategoryID, @Rate);";

                            using (SqlCommand cmd =
                                new SqlCommand(
                                    serviceSql,
                                    connection,
                                    transaction))
                            {
                                cmd.Parameters.AddWithValue(
                                    "@ProviderID", providerID);

                                cmd.Parameters.AddWithValue(
                                    "@CategoryID", categoryID);

                                cmd.Parameters.AddWithValue("@Rate", rate);

                                cmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();

                        MessageBox.Show(
                            "Registration successful!\n\n" +
                            (role == "Customer"
                                ? "Membership ID: " + membershipID
                                : "Provider availability: " +
                                  availabilityStatus) +
                            "\n\nYou can now log in.",
                            "Registration Successful",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

                        Close();
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            transaction.Rollback();
                        }
                        catch
                        {
                            // Ignore rollback errors if the transaction
                            // has already been rolled back or disconnected.
                        }

                        MessageBox.Show(
                            "Registration failed. Check the details and " +
                            "try again.\n\n" + ex.Message,
                            "Registration Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}


