/**
 * EnrolStudentDialog — enrol a student onto a project, or edit an existing
 * enrolment's access arrangements (extra time / rest breaks / notes).
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
    public partial class EnrolStudentDialog : Form
    {
        private readonly int _projectId;
        private readonly ProjectStudentDto _editing;
        private ComboBox _cboStudent;
        private ComboBox _cboExtension;
        private CheckBox _chkRestBreaks;
        private TextBox  _txtNotes;
        private Label    _lblError;
        private Label    _lblStudentStatic;

        /// <summary>Enrol a new student (editing == null) or edit an existing enrolment's arrangements.</summary>
        public EnrolStudentDialog(int projectId, ProjectStudentDto editing = null)
        {
            InitializeComponent();
            _projectId = projectId;
            _editing   = editing;
            BuildUi();

            if (_editing != null)
            {
                Text = $"Edit Access Arrangements — {_editing.Surname}, {_editing.FirstName}";
                _cboStudent.Visible = false;
                _lblStudentStatic.Visible = true;
                _lblStudentStatic.Text = $"{_editing.Surname}, {_editing.FirstName} ({_editing.CandidateNumber})";
                SelectExtension(_editing.TimeExtensionPercent);
                _chkRestBreaks.Checked = _editing.RestBreaks == 1;
                _txtNotes.Text = _editing.Notes ?? "";
            }
            else
            {
                Text = "Enrol Student";
                _lblStudentStatic.Visible = false;
                Load += async (s, e) => await LoadAvailableStudentsAsync();
            }
        }

        private void BuildUi()
        {
            Font = new Font(Theme.FontFamily, 9f);
            ClientSize = new Size(380, 280);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;

            var lblStudent = new Label { Text = "Student", Location = new Point(20, 16), AutoSize = true };
            _cboStudent = new ComboBox
            {
                Location = new Point(20, 34), Size = new Size(340, 24),
                DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font(Theme.FontFamily, 9f),
            };
            _lblStudentStatic = new Label { Location = new Point(20, 36), Size = new Size(340, 20), Font = new Font(Theme.FontFamily, 9.5f, FontStyle.Bold) };

            var lblExt = new Label { Text = "Time Extension", Location = new Point(20, 68), AutoSize = true };
            _cboExtension = new ComboBox
            {
                Location = new Point(20, 86), Size = new Size(200, 24),
                DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font(Theme.FontFamily, 9f),
            };
            _cboExtension.Items.AddRange(new object[] { "None (0%)", "10% extra time", "20% extra time", "25% extra time" });
            _cboExtension.SelectedIndex = 0;

            _chkRestBreaks = new CheckBox { Text = "Rest breaks", Location = new Point(20, 120), AutoSize = true };

            var lblNotes = new Label { Text = "Notes (optional)", Location = new Point(20, 152), AutoSize = true };
            _txtNotes = new TextBox
            {
                Location = new Point(20, 170), Size = new Size(340, 48),
                Multiline = true, Font = new Font(Theme.FontFamily, 9f),
            };

            _lblError = new Label { Location = new Point(20, 224), Size = new Size(340, 20), ForeColor = Theme.Danger };

            const int buttonTop = 246;
            var btnSave = new Button
            {
                Text = _editing != null ? "Save" : "Enrol Student",
                Location = new Point(160, buttonTop), Size = new Size(120, 32),
                BackColor = Theme.Primary, ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
            };
            btnSave.Click += OnSave;
            var btnCancel = new Button
            {
                Text = "Cancel", DialogResult = DialogResult.Cancel,
                Location = new Point(286, buttonTop), Size = new Size(94, 32),
                BackColor = Color.FromArgb(140, 140, 150), ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
            };
            ClientSize = new Size(400, buttonTop + 52);
            AcceptButton = btnSave;
            CancelButton = btnCancel;

            Controls.AddRange(new Control[] {
                lblStudent, _cboStudent, _lblStudentStatic, lblExt, _cboExtension,
                _chkRestBreaks, lblNotes, _txtNotes, _lblError, btnSave, btnCancel,
            });
        }

        private void SelectExtension(int pct)
        {
            _cboExtension.SelectedIndex = pct switch { 10 => 1, 20 => 2, 25 => 3, _ => 0 };
        }

        private int SelectedExtensionPercent() =>
            _cboExtension.SelectedIndex switch { 1 => 10, 2 => 20, 3 => 25, _ => 0 };

        private async Task LoadAvailableStudentsAsync()
        {
            try
            {
                var allResp      = await ApiService.Instance.GetAsync<ListResponse<StudentDto>>("/students/index.php");
                var enrolledResp = await ApiService.Instance.GetAsync<ListResponse<ProjectStudentDto>>("/students/for-project.php", $"project_id={_projectId}");
                var enrolledIds  = (enrolledResp?.Data ?? new List<ProjectStudentDto>()).Select(e => e.StudentId).ToHashSet();

                _cboStudent.Items.Clear();
                foreach (var s in (allResp?.Data ?? new List<StudentDto>()).Where(s => !enrolledIds.Contains(s.Id)))
                    _cboStudent.Items.Add(s);

                if (_cboStudent.Items.Count == 0)
                    _lblError.Text = "All active students are already enrolled.";
            }
            catch (Exception ex)
            {
                _lblError.Text = ex.Message;
            }
        }

        private void OnSave(object sender, EventArgs e)
        {
            if (_editing == null && !(_cboStudent.SelectedItem is StudentDto))
            {
                _lblError.Text = "Please select a student.";
                return;
            }
            DialogResult = DialogResult.OK;
            Close();
        }

        public object ToEnrolPayload() => new
        {
            project_id              = _projectId,
            student_id              = ((StudentDto)_cboStudent.SelectedItem).Id,
            time_extension_percent  = SelectedExtensionPercent(),
            rest_breaks             = _chkRestBreaks.Checked ? 1 : 0,
            notes                   = _txtNotes.Text.Trim(),
        };

        public object ToUpdatePayload() => new
        {
            project_student_id     = _editing.ProjectStudentId,
            time_extension_percent = SelectedExtensionPercent(),
            rest_breaks            = _chkRestBreaks.Checked ? 1 : 0,
            notes                  = _txtNotes.Text.Trim(),
        };
    }
}
