/**
 * AdminPanelBase — shared boilerplate for all admin CRUD panels.
 *
 * Provides: a DataGridView, toolbar buttons (Add / Edit / Deactivate / Refresh),
 * an error label and the Load event trigger. Subclasses implement:
 *   - DefineColumns()      — add columns to _grid
 *   - LoadDataAsync()      — populate _grid.Rows
 *   - AddItemAsync()       — open add dialog and POST
 *   - EditItemAsync(row)   — open edit dialog and PUT
 *   - DeleteItemAsync(row) — DELETE (soft-deactivate)
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using OSPTracker.Utils;

namespace OSPTracker.Admin
{
    public abstract partial class AdminPanelBase : UserControl
    {
        protected DataGridView _grid;
        protected Label        _lblError;
        protected Button       _btnAdd, _btnEdit, _btnDelete, _btnRefresh;
        private   Panel        _toolbar;
        private   int          _nextToolbarLeft = 428;

        /// <summary>Text shown on the delete/deactivate toolbar button. Override for different wording.</summary>
        protected virtual string DeleteButtonText => "Deactivate";

        protected AdminPanelBase()
        {
            Dock      = DockStyle.Fill;
            BackColor = Color.White;

            _toolbar = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 40,
                Padding   = new Padding(4, 4, 4, 0),
                BackColor = Color.FromArgb(240, 244, 248),
            };

            _btnAdd     = MakeBtn("+ Add",   Theme.Success);
            _btnEdit    = MakeBtn("Edit",    Theme.Secondary);
            _btnDelete  = MakeBtn(DeleteButtonText, Theme.Danger);
            _btnRefresh = MakeBtn("Refresh", Color.FromArgb(80, 80, 80));

            _btnAdd.Left     = 4;
            _btnEdit.Left    = 110;
            _btnDelete.Left  = 216;
            _btnRefresh.Left = 322;

            _btnAdd.Click     += async (s, e) => await SafeRunAsync(AddItemAsync);
            _btnEdit.Click    += async (s, e) => { if (SelectedRow != null) await SafeRunAsync(() => EditItemAsync(SelectedRow)); };
            _btnDelete.Click  += async (s, e) => { if (SelectedRow != null && ConfirmDelete()) await SafeRunAsync(() => DeleteItemAsync(SelectedRow)); };
            _btnRefresh.Click += async (s, e) => await SafeRunAsync(ReloadAsync);

            _toolbar.Controls.AddRange(new Control[] { _btnAdd, _btnEdit, _btnDelete, _btnRefresh });

            _lblError = new Label
            {
                Dock      = DockStyle.Bottom,
                Height    = 22,
                ForeColor = Theme.Danger,
                Font      = new Font(Theme.FontFamily, 8.5f),
                Padding   = new Padding(4, 2, 0, 0),
            };

            _grid = new DataGridView
            {
                Dock                  = DockStyle.Fill,
                AllowUserToAddRows    = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible     = false,
                ReadOnly              = true,
                SelectionMode         = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect           = false,
                AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor       = Color.White,
                BorderStyle           = BorderStyle.None,
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Theme.Primary,
                    ForeColor = Color.White,
                    Font      = new Font(Theme.FontFamily, 8.5f, FontStyle.Bold),
                },
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Font = new Font(Theme.FontFamily, 9f),
                    SelectionBackColor = Color.FromArgb(210, 230, 255),
                    SelectionForeColor = Color.Black,
                },
                RowTemplate = { Height = 24 },
            };
            _grid.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && SelectedRow != null)
                    _ = SafeRunAsync(() => EditItemAsync(SelectedRow));
            };

            DefineColumns();

            var filterControl = BuildFilterBar();
            Controls.Add(_grid);
            Controls.Add(_lblError);
            if (filterControl != null) Controls.Add(filterControl);
            Controls.Add(_toolbar);

            Load += async (s, e) => await SafeRunAsync(ReloadAsync);
        }

        // ----------------------------------------------------------------
        // Template methods — override in each admin panel
        // ----------------------------------------------------------------

        protected abstract void DefineColumns();
        protected abstract Task LoadDataAsync();
        protected abstract Task AddItemAsync();
        protected abstract Task EditItemAsync(DataGridViewRow row);
        protected abstract Task DeleteItemAsync(DataGridViewRow row);

        // ----------------------------------------------------------------
        // Helpers
        // ----------------------------------------------------------------

        protected DataGridViewRow SelectedRow =>
            _grid.SelectedRows.Count > 0 ? _grid.SelectedRows[0] : null;

        protected void SetError(string msg) => _lblError.Text = msg ?? "";

        protected async Task SafeRunAsync(Func<Task> action)
        {
            SetError("");
            try    { await action(); }
            catch (Exception ex) { SetError(ex.Message); }
        }

        /// <summary>Confirmation prompt shown before DeleteItemAsync runs. Override for different wording.</summary>
        protected virtual string DeleteConfirmMessage => $"{DeleteButtonText} this item?";

        protected bool ConfirmDelete() =>
            MessageBox.Show(DeleteConfirmMessage, "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;

        protected void HideIdColumn()
        {
            if (_grid.Columns.Count > 0) _grid.Columns[0].Visible = false;
        }

        protected void AddColText(string header, string name, float weight = 1)
        {
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = header, Name = name, FillWeight = weight,
                SortMode   = DataGridViewColumnSortMode.NotSortable,
            });
        }

        protected DataGridViewRow AddRow(params object[] values)
        {
            int idx = _grid.Rows.Add(values);
            return _grid.Rows[idx];
        }

        // ----------------------------------------------------------------
        // Filter support — override in subclasses that need a filter bar
        // ----------------------------------------------------------------

        protected virtual Control BuildFilterBar() => null;

        protected virtual bool RowMatchesFilter(DataGridViewRow row) => true;

        protected void ApplyFilter()
        {
            foreach (DataGridViewRow row in _grid.Rows)
                row.Visible = RowMatchesFilter(row);
        }

        protected async Task ReloadAsync()
        {
            await LoadDataAsync();
            ApplyFilter();
        }

        protected static bool CellContains(DataGridViewRow row, int col, string filter)
            => string.IsNullOrEmpty(filter)
            || (row.Cells[col].Value?.ToString() ?? "").IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;

        protected Panel MakeFilterPanel()
        {
            var p = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 54,
                BackColor = Color.FromArgb(232, 238, 248),
            };
            p.Controls.Add(new Label
            {
                Text      = "Filter",
                Location  = new Point(6, 19),
                AutoSize  = false,
                Size      = new Size(42, 16),
                Font      = new Font(Theme.FontFamily, 8f, FontStyle.Italic),
                ForeColor = Color.FromArgb(80, 80, 110),
            });
            return p;
        }

        protected TextBox AddFilterBox(Panel bar, string caption, int x, int width = 140)
        {
            bar.Controls.Add(new Label
            {
                Text      = caption,
                Location  = new Point(x, 5),
                AutoSize  = false,
                Size      = new Size(width, 16),
                Font      = new Font(Theme.FontFamily, 7.5f),
                ForeColor = Color.FromArgb(60, 60, 80),
            });
            var txt = new TextBox
            {
                Location = new Point(x, 23),
                Size     = new Size(width, 24),
                Font     = new Font(Theme.FontFamily, 9f),
            };
            txt.TextChanged += (s, e) => ApplyFilter();
            bar.Controls.Add(txt);
            return txt;
        }

        protected void AddClearButton(Panel bar, int x, params Control[] fields)
        {
            var btn = new Button
            {
                Text      = "Clear",
                Location  = new Point(x, 19),
                Size      = new Size(60, 26),
                BackColor = Color.FromArgb(140, 140, 150),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font(Theme.FontFamily, 8f),
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) =>
            {
                foreach (var c in fields)
                    if (c is TextBox t) t.Text = "";
            };
            bar.Controls.Add(btn);
        }

        /// <summary>Adds an extra toolbar button after the built-in Add/Edit/Delete/Refresh set. Call from a subclass constructor after InitializeComponent().</summary>
        protected Button AddToolbarButton(string text, Color back, int width = 100)
        {
            var btn = MakeBtn(text, back);
            btn.Width = width;
            btn.Left  = _nextToolbarLeft;
            _nextToolbarLeft += width + 6;
            _toolbar.Controls.Add(btn);
            return btn;
        }

        private Button MakeBtn(string text, Color back)
        {
            var b = new Button
            {
                Text      = text,
                Width     = 100, Height = 30, Top = 4,
                BackColor = back, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font(Theme.FontFamily, 8.5f),
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }
    }
}
