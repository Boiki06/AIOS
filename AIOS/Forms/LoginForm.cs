// Authentication screen that validates credentials and starts the user's application session.
using Microsoft.Data.SqlClient;
using AIOS.DataAccess;
using AIOS.Services;

namespace AIOS.Forms
{
    // Owns credential validation, session creation, and navigation to registration or dashboard.
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
            UiTheme.Apply(this);
            lblTitle.ForeColor = UiTheme.AccentDark;
            lblSubtitle.ForeColor = UiTheme.MutedInk;
        }

        private void btnLogin_Click(object? sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                lblMessage.Text = "Please enter your email and password.";
                return;
            }

            try
            {
                using SqlConnection conn = DatabaseConnection.GetConnection();
                conn.Open();

                const string sql = @"
                    SELECT UserID, FullName, Email, PasswordHash, Role
                    FROM Users
                    WHERE Email = @Email AND IsActive = 1";

                using SqlCommand cmd = new(sql, conn);
                cmd.Parameters.AddWithValue("@Email", email);

                using SqlDataReader reader = cmd.ExecuteReader();

                if (!reader.Read() ||
                    !PasswordService.VerifyPassword(
                        password,
                        reader["PasswordHash"].ToString() ?? string.Empty))
                {
                    lblMessage.Text = "Invalid email or password.";
                    return;
                }

                Session.Start(
                    Convert.ToInt32(reader["UserID"]),
                    reader["FullName"].ToString() ?? string.Empty,
                    reader["Email"].ToString() ?? string.Empty,
                    reader["Role"].ToString() ?? string.Empty);

                AuditService.Log(
                    Session.UserID,
                    "LOGIN",
                    "User logged into AIOS.");

                Hide();
                DashboardForm dashboard = new();
                dashboard.FormClosed += DashboardClosed;
                dashboard.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Login Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnRegister_Click(object? sender, EventArgs e)
        {
            RegisterForm registerForm = new();
            Hide();
            registerForm.FormClosed += (s, args) => Show();
            registerForm.Show();
        }

        private void DashboardClosed(object? sender, FormClosedEventArgs e)
        {
            Session.Logout();
            txtPassword.Clear();
            Show();
        }
    }
}
