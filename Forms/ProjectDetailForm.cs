/**
 * ProjectDetailForm — project info plus enrolled students and enrolment
 * management. Mirrors the React app's ProjectDetailPage.
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
    public partial class ProjectDetailForm : Form
    {
        private readonly int _projectId;
        private Label _lblTitle, _lblInfo, _lblError;
        private Button _btnSessions, _btnReport, _btnEnrol, _btnRefresh;
        private DataGridView _grid;
        private bool IsAdmin => ApiService.Instance.CurrentStaff?.IsAdmin == true;

        public ProjectDetailForm(int projectId)
        {
            InitializeComponent();
            _projectId = projectId;
            BuildUi();
            Load += async (s, e) => await RefreshAsync();
        }

        private void BuildUi()
        {
            if (Program.AppIcon != null) Icon = Program.AppIcon;
            Text = "Project Detail";
            ClientSize = new Size(920, 560);
            Font = new Font(Theme.FontFamily, 9f);
            StartPosition = FormStartPosition.CenterScreen;

            var header = new Panel { Dock = DockStyle.Top, Height = 88, BackColor = Color.FromArgb(240, 244, 248) };
            _lblTitle = new Label { Left = 12, Top = 8, AutoSize = true, Font = new Font(Theme.FontFamily, 13f, FontStyle.Bold), ForeColor = Theme.Primary };
            _lblInfo  = new Label { Left = 12, Top = 34, AutoSize = true, Font = new Font(Theme.FontFamily, 9f) };

            _btnSessions = MakeBtn("Sessions", Theme.Secondary);
            _btnSessions.Click += (s, e) => new SessionListForm(_projectId).Show();
            _btnReport = MakeBtn("Report", Color.FromArgb(140, 140, 150));
            _btnReport.Click += (s, e) => new Reports.ReportForm(_projectId).Show();
            _btnEnrol = MakeBtn("+ Enrol Student", Theme.Success);
            _btnEnrol.Click += async (s, e) => await OnEnrolAsync();
            _btnRefresh = MakeBtn("Refresh", Color.FromArgb(80, 80, 80));
            _btnRefresh.Click += async (s, e) => await RefreshAsync();

            _btnSessions.Left = 12; _btnSessions.Top = 56;
            _btnReport.Left   = 118; _btnReport.Top = 56;
            _btnEnrol.Left    = 224; _btnEnrol.Top = 56; _btnEnrol.Width = 130;
            _btnRefresh.Left  = 360; _btnRefresh.Top = 56;

            header.Controls.AddRange(new Control[] { _lblTitle, _lblInfo, _btnSessions, _btnReport, _btnEnrol, _btnRefresh });

            _lblError = new Label { Dock = DockStyle.Bottom, Height = 24, ForeColor = Theme.Danger, Padding = new Padding(8, 4, 0, 0) };

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
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Candidate #", Name = "cand", FillWeight = 1.2f });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Surname", Name = "surname", FillWeight = 1f });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "First Name", Name = "first", FillWeight = 1f });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Extension", Name = "ext", FillWeight = 0.8f });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Rest Breaks", Name = "rest", FillWeight = 0.8f });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Allowed", Name = "allowed", FillWeight = 0.8f });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Used", Name = "used", FillWeight = 0.8f });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Remaining", Name = "remaining", FillWeight = 0.8f });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "% Used", Name = "pct", FillWeight = 0.7f });
            if (IsAdmin)
            {
                _grid.Columns.Add(new DataGridViewButtonColumn { HeaderText = "", Name = "edit", Text = "Edit", UseColumnTextForButtonValue = true, FillWeight = 0.6f });
                _grid.Columns.Add(new DataGridViewButtonColumn { HeaderText = "", Name = "remove", Text = "Remove", UseColumnTextForButtonValue = true, FillWeight = 0.7f });
            }
            _grid.CellClick += Grid_CellClick;
            _grid.CellPainting += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.Value != null && _grid.Columns[e.ColumnIndex].Name == "pct")
                    ProgressCellRenderer.Paint(e, Convert.ToInt32(e.Value));
            };

            Controls.Add(_grid);
            Controls.Add(_lblError);
            Controls.Add(header);
        }

        private static Button MakeBtn(string text, Color back) => new Button
        {
            Text = text, Height = 28, Width = 100,
            BackColor = back, ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
            Font = new Font(Theme.FontFamily, 8.5f),
        };

        private async void Grid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (!(_grid.Rows[e.RowIndex].Tag is ProjectStudentDto ps)) return;
            string colName = _grid.Columns[e.ColumnIndex].Name;

            if (colName == "edit" && IsAdmin)
            {
                using var dlg = new EnrolStudentDialog(_projectId, ps);
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        await ApiService.Instance.PutAsync<SingleResponse<MessageResult>>("/projects/update-enrolment.php", dlg.ToUpdatePayload());
                        await RefreshAsync();
                    }
                    catch (Exception ex) { _lblError.Text = ex.Message; }
                }
            }
            else if (colName == "remove" && IsAdmin)
            {
                await RemoveStudentAsync(ps, false);
            }
        }

        private async Task RemoveStudentAsync(ProjectStudentDto ps, bool confirm)
        {
            try
            {
                if (!confirm)
                {
                    var result = MessageBox.Show(this, $"Remove {ps.Surname}, {ps.FirstName} from this project?",
                        "Confirm Remove", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result != DialogResult.Yes) return;
                }
                await ApiService.Instance.DeleteAsync<SingleResponse<MessageResult>>("/projects/unenrol.php", new
                {
                    project_student_id = ps.ProjectStudentId, confirm,
                });
                await RefreshAsync();
            }
            catch (Exception ex) when (ex.Message.Contains("attendance records"))
            {
                var result = MessageBox.Show(this, ex.Message + "\n\nRemove anyway?", "Attendance Data Exists",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                    await RemoveStudentAsync(ps, true);
            }
            catch (Exception ex)
            {
                _lblError.Text = ex.Message;
            }
        }

        private async Task OnEnrolAsync()
        {
            using var dlg = new EnrolStudentDialog(_projectId);
            if (dlg.ShowDialog(this) != DialogResult.OK) return;
            try
            {
                await ApiService.Instance.PostAsync<SingleResponse<MessageResult>>("/projects/enrol.php", dlg.ToEnrolPayload());
                await RefreshAsync();
            }
            catch (Exception ex) { _lblError.Text = ex.Message; }
        }

        private async Task RefreshAsync()
        {
            _lblError.Text = "";
            try
            {
                var projResp = await ApiService.Instance.GetAsync<SingleResponse<ProjectDto>>("/projects/show.php", $"id={_projectId}");
                var studResp = await ApiService.Instance.GetAsync<ListResponse<ProjectStudentDto>>("/students/for-project.php", $"project_id={_projectId}");

                var project = projResp?.Data;
                if (project != null)
                {
                    Text = $"Project Detail — {project.Name}";
                    _lblTitle.Text = project.Name;
                    _lblInfo.Text = $"Year: {project.Year}   Centre: {project.CentreNumber}   " +
                        $"Base Hours: {project.BaseHours}h   Students: {project.StudentCount}   Sessions: {project.SessionCount}";
                }

                _btnEnrol.Visible = IsAdmin;

                _grid.Rows.Clear();
                foreach (var ps in studResp?.Data ?? new List<ProjectStudentDto>())
                {
                    var values = new List<object> {
                        ps.CandidateNumber, ps.Surname, ps.FirstName,
                        ps.TimeExtensionPercent > 0 ? $"+{ps.TimeExtensionPercent}%" : "—",
                        ps.RestBreaks == 1 ? "Yes" : "—",
                        ps.TotalMinutesAllowed, ps.TotalMinutesUsed, ps.MinutesRemaining,
                        ps.PercentUsed,
                    };
                    if (IsAdmin) values.AddRange(new object[] { "Edit", "Remove" });

                    int idx = _grid.Rows.Add(values.ToArray());
                    var row = _grid.Rows[idx];
                    row.Tag = ps;
                    if (ps.MinutesRemaining < 0) row.Cells["remaining"].Style.ForeColor = Theme.Danger;
                }
            }
            catch (Exception ex)
            {
                _lblError.Text = ex.Message;
            }
        }
    }
}
