/**
 * SessionFormDialog — dual-mode modal to create or edit a session.
 *
 * Create mode (session == null): all fields editable, including session
 * type and, for individual sessions, the enrolled student.
 * Edit mode (session supplied): session_type and student are immutable
 * after creation, matching the API's business rules.
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

namespace OSPTracker.Dialogs
{
    public partial class SessionFormDialog : Form
    {
        private readonly int _projectId;
        private readonly SessionDetailDto _session;
        private bool IsEdit => _session != null;

        private DateTimePicker _dtpDate, _dtpStart, _dtpEnd;
        private ComboBox _cboType, _cboSupervisor, _cboStudent;
        private TextBox  _txtNotes;
        private Label    _lblStudent, _lblError, _lblType;

        public SessionFormDialog(int projectId, SessionDetailDto session = null)
        {
            InitializeComponent();
            _projectId = projectId;
            _session   = session;
            BuildUi();
            Load += async (s, e) => await LoadFormDataAsync();

            if (IsEdit)
            {
                Text = $"Edit Session #{session.SessionNumber}";
                _dtpDate.Value  = DateTime.Parse(session.SessionDate);
                _dtpStart.Value = DateTime.Parse("1970-01-01 " + session.StartTimeShort);
                _dtpEnd.Value   = DateTime.Parse("1970-01-01 " + session.EndTimeShort);
                _txtNotes.Text  = session.Notes ?? "";
                _cboType.Visible = false;
                _lblType.Visible = true;
                _lblType.Text = session.SessionType;
                _lblStudent.Visible = false;
                _cboStudent.Visible = false;
            }
            else
            {
                Text = "Add Session";
                _dtpDate.Value = DateTime.Today;
            }
        }

        private void BuildUi()
        {
            Font = new Font(Theme.FontFamily, 9f);
            ClientSize = new Size(400, 400);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;

            var lblDate = Lbl("Date", 20, 16);
            _dtpDate = new DateTimePicker { Location = new Point(20, 34), Size = new Size(170, 24), Format = DateTimePickerFormat.Short };

            _lblType = new Label { Text = "class", Location = new Point(210, 36), AutoSize = true, Font = new Font(Theme.FontFamily, 9.5f, FontStyle.Bold), Visible = false };
            var lblType2 = Lbl("Type", 210, 16);
            _cboType = new ComboBox { Location = new Point(210, 34), Size = new Size(150, 24), DropDownStyle = ComboBoxStyle.DropDownList };
            _cboType.Items.AddRange(new object[] { "class", "individual" });
            _cboType.SelectedIndex = 0;
            _cboType.SelectedIndexChanged += (s, e) => UpdateStudentVisibility();

            var lblStart = Lbl("Start Time", 20, 68);
            _dtpStart = new DateTimePicker { Location = new Point(20, 86), Size = new Size(170, 24), Format = DateTimePickerFormat.Time, ShowUpDown = true };

            var lblEnd = Lbl("End Time", 210, 68);
            _dtpEnd = new DateTimePicker { Location = new Point(210, 86), Size = new Size(170, 24), Format = DateTimePickerFormat.Time, ShowUpDown = true };

            var lblSupervisor = Lbl("Supervisor", 20, 120);
            _cboSupervisor = new ComboBox { Location = new Point(20, 138), Size = new Size(360, 24), DropDownStyle = ComboBoxStyle.DropDownList };

            _lblStudent = Lbl("Student", 20, 172);
            _cboStudent = new ComboBox { Location = new Point(20, 190), Size = new Size(360, 24), DropDownStyle = ComboBoxStyle.DropDownList };
            _lblStudent.Visible = _cboStudent.Visible = false;

            var lblNotes = Lbl("Notes (optional)", 20, 224);
            _txtNotes = new TextBox { Location = new Point(20, 242), Size = new Size(360, 48), Multiline = true };

            _lblError = new Label { Location = new Point(20, 298), Size = new Size(360, 34), ForeColor = Theme.Danger };

            var btnSave = new Button
            {
                Text = IsEdit ? "Save Changes" : "Create Session",
                Location = new Point(150, 336), Size = new Size(130, 32),
                BackColor = Theme.Primary, ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
            };
            btnSave.Click += OnSave;
            var btnCancel = new Button
            {
                Text = "Cancel", DialogResult = DialogResult.Cancel,
                Location = new Point(286, 336), Size = new Size(94, 32),
                BackColor = Color.FromArgb(140, 140, 150), ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
            };
            AcceptButton = btnSave;
            CancelButton = btnCancel;

            Controls.AddRange(new Control[] {
                lblDate, _dtpDate, lblType2, _cboType, _lblType,
                lblStart, _dtpStart, lblEnd, _dtpEnd,
                lblSupervisor, _cboSupervisor, _lblStudent, _cboStudent,
                lblNotes, _txtNotes, _lblError, btnSave, btnCancel,
            });
        }

        private void UpdateStudentVisibility()
        {
            bool individual = !IsEdit && _cboType.SelectedItem?.ToString() == "individual";
            _lblStudent.Visible = individual;
            _cboStudent.Visible = individual;
        }

        private async Task LoadFormDataAsync()
        {
            try
            {
                var staffResp = await ApiService.Instance.GetAsync<ListResponse<StaffDto>>("/staff/index.php");
                _cboSupervisor.Items.Clear();
                foreach (var s in (staffResp?.Data ?? new List<StaffDto>()).Where(s => s.IsActive == 1))
                    _cboSupervisor.Items.Add(s);

                if (IsEdit)
                {
                    var current = _cboSupervisor.Items.Cast<StaffDto>().FirstOrDefault(s => s.Id == _session.SupervisorId);
                    _cboSupervisor.SelectedItem = current ?? (_cboSupervisor.Items.Count > 0 ? _cboSupervisor.Items[0] : null);
                }
                else if (_cboSupervisor.Items.Count > 0)
                {
                    var me = _cboSupervisor.Items.Cast<StaffDto>().FirstOrDefault(s => s.Id == ApiService.Instance.CurrentStaff?.Id);
                    _cboSupervisor.SelectedItem = me ?? _cboSupervisor.Items[0];
                }

                var studResp = await ApiService.Instance.GetAsync<ListResponse<ProjectStudentDto>>("/students/for-project.php", $"project_id={_projectId}");
                _cboStudent.Items.Clear();
                foreach (var s in studResp?.Data ?? new List<ProjectStudentDto>())
                    _cboStudent.Items.Add(s);
            }
            catch (Exception ex)
            {
                _lblError.Text = ex.Message;
            }
        }

        private void OnSave(object sender, EventArgs e)
        {
            if (_cboSupervisor.SelectedItem == null)
            {
                _lblError.Text = "Please select a supervisor.";
                return;
            }
            if (_dtpStart.Value.TimeOfDay >= _dtpEnd.Value.TimeOfDay)
            {
                _lblError.Text = "Start time must be before end time.";
                return;
            }
            if (!IsEdit && _cboType.SelectedItem?.ToString() == "individual" && !(_cboStudent.SelectedItem is ProjectStudentDto))
            {
                _lblError.Text = "Please select a student for individual sessions.";
                return;
            }
            DialogResult = DialogResult.OK;
            Close();
        }

        public object ToCreatePayload() => new
        {
            project_id          = _projectId,
            session_date        = _dtpDate.Value.ToString("yyyy-MM-dd"),
            start_time          = _dtpStart.Value.ToString("HH:mm"),
            end_time             = _dtpEnd.Value.ToString("HH:mm"),
            supervisor_id       = ((StaffDto)_cboSupervisor.SelectedItem).Id,
            session_type        = _cboType.SelectedItem?.ToString() ?? "class",
            student_project_id  = _cboStudent.SelectedItem is ProjectStudentDto ps ? ps.ProjectStudentId : (int?)null,
            notes               = _txtNotes.Text.Trim(),
        };

        public object ToUpdatePayload() => new
        {
            id             = _session.SessionId,
            session_date   = _dtpDate.Value.ToString("yyyy-MM-dd"),
            start_time     = _dtpStart.Value.ToString("HH:mm"),
            end_time       = _dtpEnd.Value.ToString("HH:mm"),
            supervisor_id  = ((StaffDto)_cboSupervisor.SelectedItem).Id,
            notes          = _txtNotes.Text.Trim(),
        };

        private static Label Lbl(string text, int x, int y) =>
            new Label { Text = text, Location = new Point(x, y), AutoSize = true };
    }
}
