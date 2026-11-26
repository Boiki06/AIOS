// Customer-facing search screen for finding providers by service and location.
using AIOS.DataAccess;
using AIOS.Services;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AIOS.Forms
{
    // Owns provider filtering and selection for customers creating requests.
    public class ProviderSearchForm : Form
    {
        private ComboBox cmbCategory = new();
        private ComboBox cmbRegion = new();
        private TextBox txtWard = new();
        private TextBox txtBlock = new();

        private DataGridView grid = new();

        public ProviderSearchForm()
        {
            BuildUI();
            UiTheme.Apply(this);
            LoadCategories();
            LoadRegions();
            LoadProviders();
        }

        private void BuildUI()
        {
            Text = "AIOS - Find Service Providers";

            Width = 1100;
            Height = 650;

            StartPosition =
                FormStartPosition.CenterParent;

            Label categoryLabel = new()
            {
                Text = "Category",
                Location = new Point(20, 20),
                AutoSize = true
            };

            cmbCategory.SetBounds(
                20,
                45,
                200,
                30);

            Label regionLabel = new()
            {
                Text = "Region",
                Location = new Point(240, 20),
                AutoSize = true
            };

            cmbRegion.SetBounds(
                240,
                45,
                200,
                30);

            Label wardLabel = new()
            {
                Text = "Ward",
                Location = new Point(460, 20),
                AutoSize = true
            };

            txtWard.SetBounds(
                460,
                45,
                180,
                30);

            Label blockLabel = new()
            {
                Text = "Block / Area",
                Location = new Point(660, 20),
                AutoSize = true
            };

            txtBlock.SetBounds(
                660,
                45,
                180,
                30);

            Button btnSearch = new()
            {
                Text = "SEARCH",
                Location = new Point(860, 45),
                Width = 150,
                Height = 30
            };

            btnSearch.Click +=
                (s, e) => LoadProviders();

            grid.Location =
                new Point(20, 100);

            grid.Size =
                new Size(1010, 470);

            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            grid.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;

            grid.CellDoubleClick +=
                ProviderSelected;

            Controls.AddRange(
                new Control[]
                {
                    categoryLabel,
                    cmbCategory,
                    regionLabel,
                    cmbRegion,
                    wardLabel,
                    txtWard,
                    blockLabel,
                    txtBlock,
                    btnSearch,
                    grid
                });
        }

        private void LoadCategories()
        {
            cmbCategory.Items.Clear();

            cmbCategory.Items.Add(
                new CategoryItem
                {
                    ID = 0,
                    Name = "All Categories"
                });

            using SqlConnection conn =
                DatabaseConnection.GetConnection();

            conn.Open();

            string sql = @"
                SELECT CategoryID,
                       CategoryName
                FROM ServiceCategories
                ORDER BY CategoryName";

            using SqlCommand cmd =
                new SqlCommand(sql, conn);

            using SqlDataReader reader =
                cmd.ExecuteReader();

            while (reader.Read())
            {
                cmbCategory.Items.Add(
                    new CategoryItem
                    {
                        ID =
                            Convert.ToInt32(
                                reader["CategoryID"]),

                        Name =
                            reader["CategoryName"]
                            .ToString()!
                    });
            }

            cmbCategory.SelectedIndex = 0;
        }

        private void LoadRegions()
        {
            cmbRegion.Items.Clear();

            cmbRegion.Items.Add("All Regions");

            using SqlConnection conn =
                DatabaseConnection.GetConnection();

            conn.Open();

            string sql = @"
                SELECT DISTINCT Region
                FROM ServiceProviders
                WHERE Region IS NOT NULL
                ORDER BY Region";

            using SqlCommand cmd =
                new SqlCommand(sql, conn);

            using SqlDataReader reader =
                cmd.ExecuteReader();

            while (reader.Read())
            {
                cmbRegion.Items.Add(
                    reader["Region"].ToString());
            }

            cmbRegion.SelectedIndex = 0;
        }

        private void LoadProviders()
        {
            try
            {
                CategoryItem category =
                    (CategoryItem)cmbCategory
                    .SelectedItem!;

                string region =
                    cmbRegion.Text;

                string sql = @"
                    SELECT
                        ps.ProviderServiceID,
                        sp.ProviderID,
                        sp.BusinessName,
                        sc.CategoryName,
                        ps.ServiceRate,
                        sp.Region,
                        sp.Ward,
                        sp.Block,
                        sp.Address,
                        sp.ContactNo,
                        sp.AvailabilityStatus
                    FROM ServiceProviders sp

                    INNER JOIN Users u
                        ON sp.UserID = u.UserID

                    INNER JOIN ProviderServices ps
                        ON sp.ProviderID = ps.ProviderID

                    INNER JOIN ServiceCategories sc
                        ON ps.CategoryID = sc.CategoryID

                    WHERE u.IsActive = 1
                    AND u.Role = 'Provider'
                    AND sp.IsBlacklisted = 0
                    AND sp.AvailabilityStatus = 'Available'

                    AND
                    (
                        @CategoryID = 0
                        OR sc.CategoryID = @CategoryID
                    )

                    AND
                    (
                        @Region = 'All Regions'
                        OR sp.Region = @Region
                    )

                    AND
                    (
                        @Ward = ''
                        OR sp.Ward LIKE '%' + @Ward + '%'
                    )

                    AND
                    (
                        @Block = ''
                        OR sp.Block LIKE '%' + @Block + '%'
                    )

                    ORDER BY sp.BusinessName";

                using SqlConnection conn =
                    DatabaseConnection.GetConnection();

                conn.Open();

                using SqlCommand cmd =
                    new SqlCommand(sql, conn);

                cmd.Parameters.AddWithValue(
                    "@CategoryID",
                    category.ID);

                cmd.Parameters.AddWithValue(
                    "@Region",
                    region);

                cmd.Parameters.AddWithValue(
                    "@Ward",
                    txtWard.Text.Trim());

                cmd.Parameters.AddWithValue(
                    "@Block",
                    txtBlock.Text.Trim());

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
                    "Search Error");
            }
        }

        private void ProviderSelected(
            object? sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            int providerServiceID =
                Convert.ToInt32(
                    grid.Rows[e.RowIndex]
                    .Cells["ProviderServiceID"]
                    .Value);

            int providerID =
                Convert.ToInt32(
                    grid.Rows[e.RowIndex]
                    .Cells["ProviderID"]
                    .Value);

            string businessName =
                grid.Rows[e.RowIndex]
                .Cells["BusinessName"]
                .Value
                .ToString()!;

            string categoryName =
                grid.Rows[e.RowIndex]
                .Cells["CategoryName"]
                .Value
                .ToString()!;

            ServiceRequestForm form =
                new ServiceRequestForm(
                    providerServiceID,
                    providerID,
                    businessName,
                    categoryName);

            form.ShowDialog();

            LoadProviders();
        }

        private class CategoryItem
        {
            public int ID { get; set; }

            public string Name { get; set; } = "";

            public override string ToString()
            {
                return Name;
            }
        }
    }
}
