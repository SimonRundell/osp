/**
 * AttendanceEntryForm — enter/edit minutes_present per student for a session.
 * The "Mins This Session" column is editable; live warnings are shown if an
 * entry exceeds the session duration or the student's total allowed time.
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using OSPTracker.Models;
using OSPTracker.Services;
using OSPTracker.Utils;

namespace OSPTracker.Forms
{
    public partial class AttendanceEntryForm : Form
    {
        private readonly int _projectId;
        private readonly int _sessionId;
        private AttendanceForSessionDto _data;
        private Label _lblInfo, _lblError, _lblSuccess;
        private Button _btnSave;
        private DataGridView _grid;

        private const int ColMinutes = 5;
        private const int ColWarning = 6;

        public AttendanceEntryForm(int projectId, int sessionId)
        {
            InitializeComponent();
            _projectId = projectId;
            _sessionId = sessionId;
            BuildUi();
            Load += async (s, e) => await RefreshAsync();
        }

        private void BuildUi()
        {
            if (Program.AppIcon != null) Icon = Program.AppIcon;
            Text = "Attendance Entry";
            ClientSize = new Size(900, 520);
            Font = new Font(Theme.FontFamily, 9f);
            StartPosition = FormStartPosition.CenterScreen;

            var header = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.FromArgb(240, 244, 248) };
            var lblTitle = new Label { Left = 12, Top = 8, AutoSize = true, Font = new Font(Theme.FontFamily, 13f, FontStyle.Bold), ForeColor = Theme.Primary, Text = "Attendance Entry" };
            _lblInfo = new Label { Left = 12, Top = 34, AutoSize = true };
            header.Controls.AddRange(new Control[] { lblTitle, _lblInfo });

            var footer = new Panel { Dock = DockStyle.Bottom, Height = 46, Padding = new Padding(8) };
            _btnSave = new Button { Text = "Save All", Left = 8, Top = 6, Width = 120, Height = 30, BackColor = Theme.Primary, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            _btnSave.Click += async (s, e) => await OnSaveAsync();
            _lblSuccess = new Label { Left = 140, Top = 12, AutoSize = true, ForeColor = Theme.Success };
            footer.Controls.AddRange(new Control[] { _btnSave, _lblSuccess });

            _lblError = new Label { Dock = DockStyle.Bottom, Height = 22, ForeColor = Theme.Danger, Padding = new Padding(8, 2, 0, 0) };

            _grid = new DataGridView
            {
                Dock = DockStyle.Fill, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.CellSelect, MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Theme.Primary, ForeColor = Color.White,
                    Font = new Font(Theme.FontFamily, 8.5f, FontStyle.Bold),
                },
                DefaultCellStyle = new DataGridViewCellStyle { Font = new Font(Theme.FontFamily, 9f) },
                RowTemplate = { Height = 26 },
            };
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Candidate #", Name = "cand", ReadOnly = true, FillWeight = 1f });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Name", Name = "name", ReadOnly = true, FillWeight = 1.2f });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Allowed (mins)", Name = "allowed", ReadOnly = true, FillWeight = 1f });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Used to date", Name = "used", ReadOnly = true, FillWeight = 1f });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Remaining", Name = "remaining", ReadOnly = true, FillWeight = 1f });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Mins This Session", Name = "minutes", ReadOnly = false, FillWeight = 1.1f });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Warnings", Name = "warning", ReadOnly = true, FillWeight = 1.6f });
            _grid.CellValueChanged += (s, e) => { if (e.RowIndex >= 0 && e.ColumnIndex == ColMinutes) UpdateWarningsForRow(_grid.Rows[e.RowIndex]); };
            _grid.CurrentCellDirtyStateChanged += (s, e) => { if (_grid.IsCurrentCellDirty) _grid.CommitEdit(DataGridViewDataErrorContexts.Commit); };

            Controls.Add(_grid);
            Controls.Add(footer);
            Controls.Add(_lblError);
            Controls.Add(header);
        }

        private async Task RefreshAsync()
        {
            _lblError.Text = "";
            _lblSuccess.Text = "";
            try
            {
                var resp = await ApiService.Instance.GetAsync<SingleResponse<AttendanceForSessionDto>>("/attendance/for-session.php", $"session_id={_sessionId}");
                _data = resp?.Data;
                if (_data == null) { _lblError.Text = "Could not load attendance data."; return; }

                int availMins = (int)Math.Round(
                    (DateTime.Parse("1970-01-01 " + _data.Session.EndTime) - DateTime.Parse("1970-01-01 " + _data.Session.StartTime)).TotalMinutes);
                _lblInfo.Text = $"Available: {availMins} mins";

                _grid.Rows.Clear();
                foreach (var s in _data.Students)
                {
                    int idx = _grid.Rows.Add(
                        s.CandidateNumber, $"{s.Surname}, {s.FirstName}",
                        s.TotalMinutesAllowed, s.TotalMinutesUsed - s.MinutesPresent, s.MinutesRemaining,
                        s.MinutesPresent, "");
                    var row = _grid.Rows[idx];
                    row.Tag = s;
                    UpdateWarningsForRow(row);
                }
            }
            catch (Exception ex)
            {
                _lblError.Text = ex.Message;
            }
        }

        private void UpdateWarningsForRow(DataGridViewRow row)
        {
            if (!(row.Tag is AttendanceStudentDto s)) return;
            int availMins = (int)Math.Round(
                (DateTime.Parse("1970-01-01 " + _data.Session.EndTime) - DateTime.Parse("1970-01-01 " + _data.Session.StartTime)).TotalMinutes);

            int entered = ParseMinutes(row.Cells[ColMinutes].Value?.ToString());
            int usedBefore = s.TotalMinutesUsed - s.MinutesPresent;
            int wouldTotal = usedBefore + entered;
            bool overSession = entered > availMins;
            bool overAllowed = wouldTotal > s.TotalMinutesAllowed;

            var warnings = new List<string>();
            if (overSession) warnings.Add($"Exceeds session time ({availMins} mins)");
            if (overAllowed) warnings.Add("Would exceed total allowed time");
            row.Cells[ColWarning].Value = string.Join("; ", warnings);
            row.Cells[ColWarning].Style.ForeColor = warnings.Count > 0 ? Theme.Danger : Color.Black;
        }

        private static int ParseMinutes(string val)
        {
            val = (val ?? "").Trim();
            if (int.TryParse(val, out int n)) return n;
            var parts = val.Split(':');
            if (parts.Length == 2 && int.TryParse(parts[0], out int h) && int.TryParse(parts[1], out int m))
                return h * 60 + m;
            return 0;
        }

        private async Task OnSaveAsync()
        {
            _lblError.Text = "";
            _lblSuccess.Text = "";
            try
            {
                var records = _grid.Rows.Cast<DataGridViewRow>()
                    .Select(row => new AttendanceRecordUpdate
                    {
                        ProjectStudentId = ((AttendanceStudentDto)row.Tag).ProjectStudentId,
                        MinutesPresent   = ParseMinutes(row.Cells[ColMinutes].Value?.ToString()),
                    })
                    .ToList();

                await ApiService.Instance.PostAsync<SingleResponse<MessageResult>>("/attendance/save.php", new
                {
                    session_id = _sessionId,
                    attendance = records,
                });

                _lblSuccess.Text = "Attendance saved successfully.";
                await RefreshAsync();
            }
            catch (Exception ex)
            {
                _lblError.Text = ex.Message;
            }
        }
    }
}
