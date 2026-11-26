// Administration screen for creating the initial administrator account.
using System;
using System.Drawing;
using System.Windows.Forms;
using AIOS.Services;

namespace AIOS.Forms
{
    // Owns the administrator setup workflow and its validation messages.
    public class AdminSetupForm : Form
    {
        private TextBox txtEmail = new TextBox();
        private TextBox txtPassword = new TextBox();
        private Button btnCreate = new Button();

        public AdminSetupForm()
        {
            Text = "AIOS - Administrator Setup";
            Size = new Size(420, 260);
            StartPosition = FormStartPosition.CenterScreen;

            Label lblEmail = new Label
            {
                Text = "Administrator email",
                Location = new Point(30, 25),
                AutoSize = true
            };

            txtEmail.Location = new Point(30, 50);
            txtEmail.Width = 330;

            Label lblPassword = new Label
            {
                Text = "Temporary password",
                Location = new Point(30, 90),
                AutoSize = true
            };

            txtPassword.Location = new Point(30, 115);
            txtPassword.Width = 330;
            txtPassword.UseSystemPasswordChar = true;

            btnCreate.Text = "Create Administrator";
            btnCreate.Location = new Point(30, 160);
            btnCreate.Size = new Size(180, 35);
            btnCreate.Click += BtnCreate_Click;

            Controls.AddRange(new Control[]
            {
                lblEmail, txtEmail,
                lblPassword, txtPassword,
                btnCreate
            });

            UiTheme.Apply(this);
        }

        private void BtnCreate_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Enter an email and password.");
                return;
            }

            if (password.Length < 12)
            {
                MessageBox.Show(
                    "Use a password with at least 12 characters.");
                return;
            }

            try
            {
                AdminSetup.CreateAdminIfMissing(email, password);

                MessageBox.Show(
                    "Setup completed. If this email was new, " +
                    "the administrator account was created.\n\n" +
                    "If the email already existed, its password " +
                    "was not changed.");

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Setup failed: " + ex.Message);
            }
        }
    }
}