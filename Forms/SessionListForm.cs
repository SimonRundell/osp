/**
 * SessionListForm — lists all sessions for a project. Admins can add,
 * edit and delete sessions; everyone can drill into Attendance entry.
 * Footer shows total scheduled time and remaining unscheduled time.
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using OSPTracker.Dialogs;
using OSPTracker.Models;
using OSPTracker.Services;
using OSPTracker.Utils;

namespace OSPTracker.Forms
{
    public partial class SessionListForm : Form
    {
        private readonly int _projectId;
        private ProjectDto _project;
        private Label _lblTitle, _lblFooter, _lblError;
        private Button _btnAdd, _btnRefresh;
        private DataGridView _grid;
        private bool IsAdmin => ApiService.Instance.CurrentStaff?.IsAdmin == true;

        public SessionListForm(int projectId)
        {
            InitializeComponent();
            _projectId = projectId;
            BuildUi();
            Load += async (s, e) => await RefreshAsync();
        }

        private void BuildUi()
        {
            if (Program.AppIcon != null) Icon = Program.AppIcon;
            Text = "Sessions";
            ClientSize = new Size(900, 520);
            Font = new Font(Theme.FontFamily, 9f);
            StartPosition = FormStartPosition.CenterScreen;

            var header = new Panel { Dock = DockStyle.Top, Height = 68, BackColor = Color.FromArgb(240, 244, 248) };
            _lblTitle = new Label { Left = 12, Top = 8, AutoSize = true, Font = new Font(Theme.FontFamily, 13f, FontStyle.Bold), ForeColor = Theme.Primary };
            _btnAdd = new Button { Text = "+ Add Session", Left = 12, Top = 36, Width = 120, Height = 28, BackColor = Theme.Success, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            _btnAdd.Click += async (s, e) => await OnAddAsync();
            _btnRefresh = new Button { Text = "Refresh", Left = 140, Top = 36, Width = 90, Height = 28, BackColor = Color.FromArgb(80, 80, 80), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            _btnRefresh.Click += async (s, e) => await RefreshAsync();
            header.Controls.AddRange(new Control[] { _lblTitle, _btnAdd, _btnRefresh });

            _lblFooter = new Label { Dock = DockStyle.Bottom, Height = 46, Font = new Font(Theme.FontFamily, 9f, FontStyle.Bold), Padding = new Padding(8, 4, 0, 0) };
            _lblError  = new Label { Dock = DockStyle.Bottom, Height = 22, ForeColor = Theme.Danger, Padding = new Padding(8, 2, 0, 0) };

            _grid = new DataGridView
            {
                Dock = DockStyle.Fill, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                RowHeadersVisible = false, ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Theme.Primary, ForeColor = Color.White,
                    Font = new Font(Theme.FontFamily, 8.5f, FontStyle.Bold),
                },
                DefaultCellStyle = new DataGridViewCellStyle { Font = new Font(Theme.FontFamily, 9f) },
                RowTemplate = { Height = 24 },
            };
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "#", Name = "num", FillWeight = 0.4f });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Date", Name = "date", FillWeight = 1f });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Start", Name = "start", FillWeight = 0.7f });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "End", Name = "end", FillWeight = 0.7f });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Avail. Mins", Name = "mins", FillWeight = 0.8f });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Type", Name = "type", FillWeight = 0.8f });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Supervisor", Name = "supervisor", FillWeight = 1.2f });
            _grid.Columns.Add(new DataGridViewButtonColumn { HeaderText = "", Name = "attendance", Text = "Attendance", UseColumnTextForButtonValue = true, FillWeight = 1f });
            if (IsAdmin)
            {
                _grid.Columns.Add(new DataGridViewButtonColumn { HeaderText = "", Name = "edit", Text = "Edit", UseColumnTextForButtonValue = true, FillWeight = 0.6f });
                _grid.Columns.Add(new DataGridViewButtonColumn { HeaderText = "", Name = "delete", Text = "Delete", UseColumnTextForButtonValue = true, FillWeight = 0.7f });
            }
            _grid.CellClick += Grid_CellClick;

            Controls.Add(_grid);
            Controls.Add(_lblFooter);
            Controls.Add(_lblError);
            Controls.Add(header);
        }

        private async void Grid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (!(_grid.Rows[e.RowIndex].Tag is SessionDto session)) return;
            string col = _grid.Columns[e.ColumnIndex].Name;

            if (col == "attendance")
            {
                new AttendanceEntryForm(_projectId, session.SessionId).Show();
            }
            else if (col == "edit" && IsAdmin)
            {
                await OnEditAsync(session);
            }
            else if (col == "delete" && IsAdmin)
            {
                await DeleteSessionAsync(session, false);
            }
        }

        private async Task OnAddAsync()
        {
            using var dlg = new SessionFormDialog(_projectId);
            if (dlg.ShowDialog(this) != DialogResult.OK) return;
            try
            {
                await ApiService.Instance.PostAsync<SingleResponse<MessageResult>>("/sessions/create.php", dlg.ToCreatePayload());
                await RefreshAsync();
            }
            catch (Exception ex) { _lblError.Text = ex.Message; }
        }

        private async Task OnEditAsync(SessionDto session)
        {
            try
            {
                var detail = await ApiService.Instance.GetAsync<SingleResponse<SessionDetailDto>>("/sessions/show.php", $"id={session.SessionId}");
                using var dlg = new SessionFormDialog(_projectId, detail.Data);
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                await ApiService.Instance.PutAsync<SingleResponse<MessageResult>>("/sessions/update.php", dlg.ToUpdatePayload());
                await RefreshAsync();
            }
            catch (Exception ex) { _lblError.Text = ex.Message; }
        }

        private async Task DeleteSessionAsync(SessionDto session, bool confirm)
        {
            try
            {
                if (!confirm)
                {
                    var result = MessageBox.Show(this, $"Delete session #{session.SessionNumber}?",
                        "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result != DialogResult.Yes) return;
                }
                await ApiService.Instance.DeleteAsync<SingleResponse<MessageResult>>("/sessions/delete.php", new { id = session.SessionId, confirm });
                await RefreshAsync();
            }
            catch (Exception ex) when (ex.Message.Contains("attendance records"))
            {
                var result = MessageBox.Show(this, ex.Message + "\n\nDelete anyway?", "Attendance Data Exists",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                    await DeleteSessionAsync(session, true);
            }
            catch (Exception ex)
            {
                _lblError.Text = ex.Message;
            }
        }

        private async Task RefreshAsync()
        {
            _lblError.Text = "";
            try
            {
                var projResp = await ApiService.Instance.GetAsync<SingleResponse<ProjectDto>>("/projects/show.php", $"id={_projectId}");
                var sessResp = await ApiService.Instance.GetAsync<ListResponse<SessionDto>>("/sessions/index.php", $"project_id={_projectId}");

                _project = projResp?.Data;
                Text = $"Sessions — {_project?.Name}";
                _lblTitle.Text = $"Sessions — {_project?.Name}";

                var sessions = sessResp?.Data ?? new List<SessionDto>();
                _grid.Rows.Clear();
                foreach (var s in sessions)
                {
                    var values = new List<object> {
                        s.SessionNumber, s.SessionDate, s.StartTimeShort, s.EndTimeShort,
                        (int)Math.Round(s.AvailableMinutes), s.SessionType, s.SupervisorName, "Attendance",
                    };
                    if (IsAdmin) values.AddRange(new object[] { "Edit", "Delete" });
                    int idx = _grid.Rows.Add(values.ToArray());
                    _grid.Rows[idx].Tag = s;
                }

                double totalScheduled = sessions.Sum(s => s.AvailableMinutes);
                int totalProjectMins  = _project?.TotalProjectMinutes ?? 0;
                int remaining         = totalProjectMins - (int)Math.Round(totalScheduled);
                _lblFooter.Text = $"Scheduled time: {Math.Round(totalScheduled)} mins        Remaining unscheduled time: {remaining} mins";
                _lblFooter.ForeColor = remaining < 0 ? Theme.Danger : Color.Black;
            }
            catch (Exception ex)
            {
                _lblError.Text = ex.Message;
            }
        }
    }
}
