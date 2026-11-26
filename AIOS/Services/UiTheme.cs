// Defines the shared baby-pink visual system and applies it consistently to WinForms controls.
using System.Drawing;
using System.Windows.Forms;

namespace AIOS.Services
{
    // Centralizes colors, typography, and control styling for the application's user interface.
    public static class UiTheme
    {
        // Main form background color.
        public static readonly Color Canvas = Color.FromArgb(255, 247, 251);
        // Background for cards, panels, and input controls.
        public static readonly Color Surface = Color.White;
        // Alternating grid-row background color.
        public static readonly Color SurfaceAlt = Color.FromArgb(255, 237, 245);
        // Primary text color.
        public static readonly Color Ink = Color.FromArgb(54, 35, 54);
        // Secondary text color.
        public static readonly Color MutedInk = Color.FromArgb(112, 88, 108);
        // Primary interactive accent color.
        public static readonly Color Accent = Color.FromArgb(221, 110, 163);
        // Contrast accent used for headings and grid headers.
        public static readonly Color AccentDark = Color.FromArgb(164, 64, 117);
        // Light accent used for selected rows and subtle highlights.
        public static readonly Color AccentLight = Color.FromArgb(255, 218, 234);
        // Soft border color for data grids.
        public static readonly Color GridLine = Color.FromArgb(239, 211, 226);

        // Applies the shared theme to a form and recursively styles its child controls.
        public static void Apply(Form form)
        {
            form.BackColor = Canvas;
            form.ForeColor = Ink;
            form.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            form.StartPosition = FormStartPosition.CenterScreen;

            StyleControls(form.Controls);
        }

        // Walks the control tree and selects styling appropriate for each control type.
        private static void StyleControls(Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                switch (control)
                {
                    case Panel panel:
                        panel.BackColor = Surface;
                        break;
                    case GroupBox groupBox:
                        groupBox.BackColor = Surface;
                        groupBox.ForeColor = AccentDark;
                        break;
                    case Label label:
                        label.ForeColor = label.Font.Size >= 16F &&
                            label.Font.Bold
                            ? AccentDark
                            : MutedInk;
                        break;
                    case TextBox textBox:
                        textBox.BackColor = Surface;
                        textBox.ForeColor = Ink;
                        textBox.BorderStyle = BorderStyle.FixedSingle;
                        break;
                    case ComboBox comboBox:
                        comboBox.BackColor = Surface;
                        comboBox.ForeColor = Ink;
                        comboBox.FlatStyle = FlatStyle.Flat;
                        break;
                    case DateTimePicker dateTimePicker:
                        dateTimePicker.CalendarMonthBackground = Surface;
                        dateTimePicker.CalendarTitleBackColor = Accent;
                        dateTimePicker.CalendarTitleForeColor = Color.White;
                        break;
                    case Button button:
                        StyleButton(button);
                        break;
                    case DataGridView grid:
                        StyleGrid(grid);
                        break;
                }

                if (control.HasChildren)
                    StyleControls(control.Controls);
            }
        }

        // Applies the consistent accent button appearance and hand cursor.
        private static void StyleButton(Button button)
        {
            button.BackColor = Accent;
            button.ForeColor = Color.White;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = AccentDark;
            button.FlatAppearance.BorderSize = 1;
            button.Cursor = Cursors.Hand;
            button.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
        }

        // Configures grid colors, headers, selection behavior, spacing, and column fitting.
        private static void StyleGrid(DataGridView grid)
        {
            grid.BackgroundColor = Surface;
            grid.BorderStyle = BorderStyle.None;
            grid.GridColor = GridLine;
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor = AccentDark;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font(
                "Segoe UI Semibold",
                9.5F,
                FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleLeft;
            grid.ColumnHeadersDefaultCellStyle.WrapMode =
                DataGridViewTriState.True;
            grid.ColumnHeadersHeightSizeMode =
                DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            grid.DefaultCellStyle.BackColor = Surface;
            grid.DefaultCellStyle.ForeColor = Ink;
            grid.DefaultCellStyle.SelectionBackColor = AccentLight;
            grid.DefaultCellStyle.SelectionForeColor = Ink;
            grid.DefaultCellStyle.Padding = new Padding(8, 6, 8, 6);
            grid.AlternatingRowsDefaultCellStyle.BackColor = SurfaceAlt;
            grid.RowHeadersVisible = false;
            grid.AllowUserToResizeColumns = true;
            grid.AllowUserToResizeRows = false;
            grid.ScrollBars = ScrollBars.Both;
            grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            grid.DataBindingComplete += (_, _) => FitGridColumns(grid);
            FitGridColumns(grid);
        }

        // Ensures each grid column is wide enough for its heading and readable cell content.
        private static void FitGridColumns(DataGridView grid)
        {
            if (grid.Columns.Count == 0)
                return;

            grid.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.AllCells;

            foreach (DataGridViewColumn column in grid.Columns)
            {
                int headingWidth = TextRenderer.MeasureText(
                    column.HeaderText,
                    grid.ColumnHeadersDefaultCellStyle.Font).Width;

                int minimumWidth = Math.Max(120, headingWidth + 32);
                column.MinimumWidth = minimumWidth;
                column.AutoSizeMode =
                    DataGridViewAutoSizeColumnMode.AllCells;
            }

            grid.AutoResizeColumnHeadersHeight();
        }
    }
}
