// Role-aware landing screen that routes the signed-in user to available application features.
using AIOS.Services;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace AIOS.Forms
{
    // Owns role-specific navigation and the signed-in user's welcome view.
    public class DashboardForm : Form
    {
        private Label lblWelcome;
        private Label lblRole;

        private Panel menuPanel;
        private Panel contentPanel;

        public DashboardForm()
        {
            InitializeComponent();
            UiTheme.Apply(this);
            menuPanel.BackColor = UiTheme.AccentDark;
            lblWelcome.ForeColor = UiTheme.AccentDark;

            foreach (Control control in menuPanel.Controls)
            {
                if (control is Button button)
                {
                    button.BackColor = Color.FromArgb(188, 70, 130);
                    button.FlatAppearance.BorderColor = UiTheme.Accent;
                }
            }
        }

        private void InitializeComponent()
        {
            Text = "AIOS - Dashboard";
            Width = 1200;
            Height = 750;
            StartPosition = FormStartPosition.CenterScreen;

            menuPanel = new Panel
            {
                Left = 0,
                Top = 0,
                Width = 260,
                Height = ClientSize.Height,
                BackColor = Color.FromArgb(35, 35, 35),
                Anchor = AnchorStyles.Top |
                         AnchorStyles.Bottom |
                         AnchorStyles.Left
            };

            contentPanel = new Panel
            {
                Left = 260,
                Top = 0,
                Width = ClientSize.Width - 260,
                Height = ClientSize.Height,
                BackColor = Color.White,
                Anchor = AnchorStyles.Top |
                         AnchorStyles.Bottom |
                         AnchorStyles.Left |
                         AnchorStyles.Right
            };

            lblWelcome = new Label
            {
                Text = "Welcome to AIOS",
                Left = 40,
                Top = 50,
                Width = 700,
                Height = 50,
                Font = new Font(
                    "Segoe UI",
                    24,
                    FontStyle.Bold)
            };

            lblRole = new Label
            {
                Text = "",
                Left = 40,
                Top = 110,
                Width = 700,
                Height = 35,
                Font = new Font(
                    "Segoe UI",
                    13,
                    FontStyle.Regular)
            };

            contentPanel.Controls.Add(lblWelcome);
            contentPanel.Controls.Add(lblRole);

            Controls.Add(contentPanel);
            Controls.Add(menuPanel);

            BuildMenu();
            BuildContentOverview();
        }

        private void BuildContentOverview()
        {
            Panel headerPanel = new Panel
            {
                Left = 32,
                Top = 28,
                Width = contentPanel.ClientSize.Width - 64,
                Height = 150,
                Anchor = AnchorStyles.Top |
                         AnchorStyles.Left |
                         AnchorStyles.Right,
                BackColor = UiTheme.Surface,
                Padding = new Padding(28)
            };

            Label eyebrow = new Label
            {
                Text = "ALL-IN-ONE SERVICE PLATFORM",
                Left = 28,
                Top = 18,
                Width = 400,
                Height = 24,
                ForeColor = UiTheme.Accent,
                Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold)
            };

            lblWelcome.Dock = DockStyle.None;
            lblWelcome.Left = 28;
            lblWelcome.Top = 42;
            lblWelcome.Height = 54;
            lblWelcome.Width = headerPanel.ClientSize.Width;
            lblRole.Dock = DockStyle.None;
            lblRole.Left = 28;
            lblRole.Top = 98;
            lblRole.Height = 30;
            lblRole.Width = headerPanel.ClientSize.Width;

            headerPanel.Controls.Add(eyebrow);
            headerPanel.Controls.Add(lblWelcome);
            headerPanel.Controls.Add(lblRole);
            contentPanel.Controls.Add(headerPanel);

            FlowLayoutPanel cards = new FlowLayoutPanel
            {
                Left = 32,
                Top = 204,
                Width = contentPanel.ClientSize.Width - 64,
                Height = 150,
                Anchor = AnchorStyles.Top |
                         AnchorStyles.Left |
                         AnchorStyles.Right,
                WrapContents = false,
                Padding = new Padding(0),
                BackColor = UiTheme.Canvas
            };

            cards.Controls.Add(CreateOverviewCard(
                "SERVICES",
                "Find trusted providers\nacross Greater Gaborone"));
            cards.Controls.Add(CreateOverviewCard(
                "CONTROL CENTRE",
                "Manage requests, invoices\nand service records"));
            cards.Controls.Add(CreateOverviewCard(
                "SECURE PAYMENTS",
                "Payments and invoices\nmanaged through AIOS"));

            contentPanel.Controls.Add(cards);
        }

        private static Panel CreateOverviewCard(string title, string text)
        {
            Panel card = new Panel
            {
                Width = 250,
                Height = 132,
                Margin = new Padding(0, 0, 18, 0),
                BackColor = UiTheme.Surface,
                Padding = new Padding(20)
            };

            Label titleLabel = new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Height = 28,
                ForeColor = UiTheme.AccentDark,
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold)
            };

            Label textLabel = new Label
            {
                Text = text,
                Dock = DockStyle.Fill,
                ForeColor = UiTheme.MutedInk,
                Font = new Font("Segoe UI", 11F, FontStyle.Regular)
            };

            card.Controls.Add(textLabel);
            card.Controls.Add(titleLabel);
            return card;
        }

        private void BuildMenu()
        {
            menuPanel.Controls.Clear();

            Label logo = new Label
            {
                Text = "AIOS",
                Left = 25,
                Top = 25,
                Width = 200,
                Height = 50,
                ForeColor = Color.White,
                Font = new Font(
                    "Segoe UI",
                    24,
                    FontStyle.Bold)
            };

            menuPanel.Controls.Add(logo);

            int top = 100;

            AddMenuButton(
                "My Profile",
                top,
                BtnMyProfile_Click);

            top += 50;

            if (Session.Role == "Customer")
            {
                AddMenuButton(
                    "Find Service Providers",
                    top,
                    BtnFindProviders_Click);

                top += 50;

                AddMenuButton(
                    "My Service Requests",
                    top,
                    BtnMyRequests_Click);

                top += 50;

                AddMenuButton(
                    "My Invoices & Payments",
                    top,
                    BtnInvoices_Click);

                top += 50;

                AddMenuButton(
                    "Transaction History",
                    top,
                    BtnCustomerHistory_Click);

                top += 50;

                AddMenuButton(
                    "My Feedback",
                    top,
                    BtnFeedback_Click);
            }
            else if (Session.Role == "Provider")
            {
                AddMenuButton(
                    "Service Requests",
                    top,
                    BtnProviderRequests_Click);

                top += 50;

                AddMenuButton(
                    "Manage My Jobs",
                    top,
                    BtnProviderJobs_Click);

                top += 50;

                AddMenuButton(
                    "My Service History",
                    top,
                    BtnProviderHistory_Click);
            }
            else if (Session.Role == "Admin")
            {
                AddMenuButton(
                    "Manage Users",
                    top,
                    BtnAdminUsers_Click);

                top += 50;

                AddMenuButton(
                    "Manage Providers",
                    top,
                    BtnAdminProviders_Click);

                top += 50;

                AddMenuButton(
                    "Transactions",
                    top,
                    BtnAdminTransactions_Click);

                top += 50;

                AddMenuButton(
                    "Reports & Statistics",
                    top,
                    BtnAdminReports_Click);

                top += 50;

                AddMenuButton(
                    "Audit Logs",
                    top,
                    BtnAdminAudit_Click);
            }

            Button logoutButton = new Button
            {
                Text = "Logout",
                Left = 20,
                Top = menuPanel.Height - 70,
                Width = 220,
                Height = 46,
                BackColor = Color.FromArgb(180, 50, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Bottom |
                         AnchorStyles.Left
            };

            logoutButton.Click += BtnLogout_Click;

            menuPanel.Controls.Add(logoutButton);

            lblWelcome.Text =
                $"Welcome, {Session.FullName}";

            lblRole.Text =
                $"Role: {Session.Role}";
        }

        private void AddMenuButton(
            string text,
            int top,
            EventHandler clickHandler)
        {
            Button button = new Button
            {
                Text = text,
                Left = 20,
                Top = top,
                Width = 220,
                Height = 40,
                BackColor = Color.FromArgb(55, 55, 55),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            button.Click += clickHandler;

            menuPanel.Controls.Add(button);
        }

        private void OpenForm(Form form)
        {
            form.ShowDialog();
        }

        private void BtnMyProfile_Click(
            object? sender,
            EventArgs e)
        {
            if (Session.Role == "Customer")
            {
                OpenForm(new CustomerProfileForm());
            }
            else if (Session.Role == "Provider")
            {
                OpenForm(new ProviderProfileForm());
            }
            else
            {
                MessageBox.Show(
                    "Admin profile management is handled through the user management section.",
                    "AIOS",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        private void BtnFindProviders_Click(
            object? sender,
            EventArgs e)
        {
            OpenForm(new ProviderSearchForm());
        }

        private void BtnMyRequests_Click(
            object? sender,
            EventArgs e)
        {
            OpenForm(new MyServiceRequestsForm());
        }

        private void BtnInvoices_Click(
            object? sender,
            EventArgs e)
        {
            OpenForm(new CustomerInvoicesForm());
        }

        private void BtnCustomerHistory_Click(
            object? sender,
            EventArgs e)
        {
            OpenForm(new CustomerHistoryForm());
        }

        private void BtnFeedback_Click(
            object? sender,
            EventArgs e)
        {
            OpenForm(new CustomerFeedbackForm());
        }

        private void BtnProviderRequests_Click(
            object? sender,
            EventArgs e)
        {
            OpenForm(new ProviderRequestsForm());
        }

        private void BtnProviderJobs_Click(
            object? sender,
            EventArgs e)
        {
            OpenForm(new ProviderJobsForm());
        }

        private void BtnProviderHistory_Click(
            object? sender,
            EventArgs e)
        {
            OpenForm(new ProviderHistoryForm());
        }

        private void BtnAdminUsers_Click(
            object? sender,
            EventArgs e)
        {
            OpenForm(new AdminUsersForm());
        }

        private void BtnAdminProviders_Click(
            object? sender,
            EventArgs e)
        {
            OpenForm(new AdminProvidersForm());
        }

        private void BtnAdminTransactions_Click(
            object? sender,
            EventArgs e)
        {
            OpenForm(new AdminTransactionsForm());
        }

        private void BtnAdminReports_Click(
            object? sender,
            EventArgs e)
        {
            OpenForm(new AdminReportsForm());
        }

        private void BtnAdminAudit_Click(
            object? sender,
            EventArgs e)
        {
            OpenForm(new AdminAuditForm());
        }

        private void BtnLogout_Click(
            object? sender,
            EventArgs e)
        {
            DialogResult result =
                MessageBox.Show(
                    "Are you sure you want to logout?",
                    "Logout",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            Session.Logout();

            Hide();

            LoginForm loginForm = new LoginForm();

            loginForm.FormClosed +=
                (s, args) => Close();

            loginForm.Show();
        }
    }
}