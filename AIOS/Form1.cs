// Legacy database test form retained for manually checking connectivity during development.
using System;
using System.Windows.Forms;
using AIOS.DataAccess;

namespace AIOS
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
        }

        private void TestDatabaseConnection()
        {
            try
            {
                using (var connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();

                    MessageBox.Show(
                        "Connected to AIOS_DB successfully!",
                        "AIOS Database Test",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Database connection failed.\n" + ex.Message,
                    "Connection Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            TestDatabaseConnection();
        }
    }
}
