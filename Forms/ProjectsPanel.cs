/**
 * ProjectsPanel — read-only browse list of all projects (active and
 * inactive), with buttons to open Detail, Sessions or Report for the
 * selected row. Mirrors the React app's ProjectListPage.
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using OSPTracker.Models;
using OSPTracker.Services;
using OSPTracker.Utils;

namespace OSPTracker.Forms
{
    public partial class ProjectsPanel : UserControl
    {
        private DataGridView _grid;
        private Panel        _toolbar;
        private Label        _lblError;
        private Button       _btnDetail, _btnSessions, _btnReport, _btnRefresh;

        public ProjectsPanel()
        {
            InitializeComponent();
            BuildUi();
            Load += async (s, e) => await RefreshAsync();
        }

        private void BuildUi()
        {
            Dock      = DockStyle.Fill;
            BackColor = Color.White;

            _toolbar = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = Color.FromArgb(240, 244, 248) };
            _btnDetail   = MakeBtn("Detail",   Theme.Secondary, 4);
            _btnSessions = MakeBtn("Sessions", Theme.Secondary, 110);
            _btnReport   = MakeBtn("Report",   Color.FromArgb(140, 140, 150), 216);
            _btnRefresh  = MakeBtn("Refresh",  Color.FromArgb(80, 80, 80), 322);

            _btnDetail.Click   += (s, e) => { if (Selected != null) new ProjectDetailForm(Selected.Id).Show(); };
            _btnSessions.Click += (s, e) => { if (Selected != null) new SessionListForm(Selected.Id).Show(); };
            _btnReport.Click   += (s, e) => { if (Selected != null) new Reports.ReportForm(Selected.Id).Show(); };
            _btnRefresh.Click  += async (s, e) => await RefreshAsync();

            _toolbar.Controls.AddRange(new Control[] { _btnDetail, _btnSessions, _btnReport, _btnRefresh });

            _lblError = new Label { Dock = DockStyle.Bottom, Height = 22, ForeColor = Theme.Danger, Padding = new Padding(4, 2, 0, 0) };

            _grid = new DataGridView
            {
                Dock = DockStyle.Fill, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                RowHeadersVisible = false, ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White, BorderStyle = BorderStyle.None,
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Theme.Primary, ForeColor = Color.White,
                    Font = new Font(Theme.FontFamily, 8.5f, FontStyle.Bold),
                },
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Font = new Font(Theme.FontFamily, 9f),
                    SelectionBackColor = Color.FromArgb(210, 230, 255), SelectionForeColor = Color.Black,
                },
                RowTemplate = { Height = 24 },
            };
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Name", Name = "name", FillWeight = 2.5f });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Year", Name = "year", FillWeight = 0.6f });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Centre", Name = "centre", FillWeight = 0.8f });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Base Hours", Name = "hours", FillWeight = 0.8f });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Start", Name = "start", FillWeight = 1f });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "End", Name = "end", FillWeight = 1f });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status", Name = "status", FillWeight = 0.8f });
            _grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0 && Selected != null) new ProjectDetailForm(Selected.Id).Show(); };

            Controls.Add(_grid);
            Controls.Add(_lblError);
            Controls.Add(_toolbar);
        }

        private ProjectDto Selected => _grid.SelectedRows.Count > 0 ? _grid.SelectedRows[0].Tag as ProjectDto : null;

        public async Task RefreshAsync()
        {
            _lblError.Text = "";
            _grid.Rows.Clear();
            try
            {
                var resp = await ApiService.Instance.GetAsync<ListResponse<ProjectDto>>("/projects/index.php");
                foreach (var p in resp?.Data ?? new List<ProjectDto>())
                {
                    int idx = _grid.Rows.Add(p.Name, p.Year, p.CentreNumber, $"{p.BaseHours}h",
                        p.StartDate ?? "—", p.EndDate ?? "—", p.IsActive == 1 ? "Active" : "Inactive");
                    _grid.Rows[idx].Tag = p;
                }
            }
            catch (Exception ex)
            {
                _lblError.Text = ex.Message;
            }
        }

        private static Button MakeBtn(string text, Color back, int left) => new Button
        {
            Text = text, Left = left, Top = 4, Width = 100, Height = 30,
            BackColor = back, ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
            Font = new Font(Theme.FontFamily, 8.5f),
        };
    }
}
