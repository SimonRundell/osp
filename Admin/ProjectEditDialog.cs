/**
 * ProjectEditDialog — create/edit an OSP project.
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using OSPTracker.Models;
using OSPTracker.Utils;

namespace OSPTracker.Admin
{
    public partial class ProjectEditDialog : Form
    {
        private readonly bool _isEdit;
        private TextBox  _txtName, _txtCentre;
        private TextBox  _txtDescription;
        private NumericUpDown _numYear;
        private NumericUpDown _numHours;
        private DateTimePicker _dtpStart, _dtpEnd;
        private CheckBox _chkStart, _chkEnd;
        private CheckBox _chkActive;
        private Label _lblError;

        public ProjectEditDialog(ProjectDto p)
        {
            InitializeComponent();
            _isEdit = p != null;
            BuildUi();
            if (_isEdit)
            {
                Text = "Edit Project";
                _txtName.Text        = p.Name;
                _txtDescription.Text = p.Description ?? "";
                _numYear.Value       = p.Year;
                _txtCentre.Text      = p.CentreNumber;
                _numHours.Value      = p.BaseHours;
                if (!string.IsNullOrEmpty(p.StartDate)) { _chkStart.Checked = true; _dtpStart.Value = DateTime.Parse(p.StartDate); }
                if (!string.IsNullOrEmpty(p.EndDate))   { _chkEnd.Checked   = true; _dtpEnd.Value   = DateTime.Parse(p.EndDate); }
                _chkActive.Visible = true;
                _chkActive.Checked = p.IsActive == 1;
            }
            else
            {
                Text = "New Project";
                _numYear.Value  = DateTime.Now.Year;
                _txtCentre.Text = "54221";
                _numHours.Value = 30;
                _chkActive.Checked = true;
                _chkActive.Visible = false;
            }
        }

        private void BuildUi()
        {
            Font = new Font(Theme.FontFamily, 9f);
            ClientSize = new Size(400, 420);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;

            var lblName = Lbl("Project Name *", 20, 16);
            _txtName = Txt(20, 34, 360);

            var lblDesc = Lbl("Description", 20, 66);
            _txtDescription = new TextBox
            {
                Location = new Point(20, 84), Size = new Size(360, 48),
                Multiline = true, Font = new Font(Theme.FontFamily, 9f),
            };

            var lblYear = Lbl("Year", 20, 142);
            _numYear = new NumericUpDown { Location = new Point(20, 160), Size = new Size(100, 24), Minimum = 2020, Maximum = 2099 };

            var lblCentre = Lbl("Centre Number", 140, 142);
            _txtCentre = Txt(140, 160, 120);

            var lblHours = Lbl("Base Hours", 20, 194);
            _numHours = new NumericUpDown { Location = new Point(20, 212), Size = new Size(100, 24), Minimum = 1, Maximum = 999, DecimalPlaces = 1, Increment = 0.5m };

            var lblStart = Lbl("Start Date", 20, 246);
            _chkStart = new CheckBox { Location = new Point(20, 264), Size = new Size(18, 20) };
            _dtpStart = new DateTimePicker { Location = new Point(44, 262), Size = new Size(160, 24), Format = DateTimePickerFormat.Short };
            _chkStart.CheckedChanged += (s, e) => _dtpStart.Enabled = _chkStart.Checked;
            _dtpStart.Enabled = false;

            var lblEnd = Lbl("End Date", 210, 246);
            _chkEnd = new CheckBox { Location = new Point(210, 264), Size = new Size(18, 20) };
            _dtpEnd = new DateTimePicker { Location = new Point(234, 262), Size = new Size(150, 24), Format = DateTimePickerFormat.Short };
            _chkEnd.CheckedChanged += (s, e) => _dtpEnd.Enabled = _chkEnd.Checked;
            _dtpEnd.Enabled = false;

            _chkActive = new CheckBox { Text = "Active", Location = new Point(20, 298), AutoSize = true };

            _lblError = new Label { Location = new Point(20, 330), Size = new Size(360, 20), ForeColor = Theme.Danger };

            var btnSave = new Button
            {
                Text = _isEdit ? "Save Changes" : "Create Project",
                Location = new Point(160, 366), Size = new Size(120, 32),
                BackColor = Theme.Primary, ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
            };
            btnSave.Click += OnSave;
            var btnCancel = new Button
            {
                Text = "Cancel", DialogResult = DialogResult.Cancel,
                Location = new Point(286, 366), Size = new Size(94, 32),
                BackColor = Color.FromArgb(140, 140, 150), ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
            };
            AcceptButton = btnSave;
            CancelButton = btnCancel;

            Controls.AddRange(new Control[] {
                lblName, _txtName, lblDesc, _txtDescription,
                lblYear, _numYear, lblCentre, _txtCentre,
                lblHours, _numHours,
                lblStart, _chkStart, _dtpStart, lblEnd, _chkEnd, _dtpEnd,
                _chkActive, _lblError, btnSave, btnCancel,
            });
        }

        private void OnSave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtName.Text))
            {
                _lblError.Text = "Project name is required.";
                return;
            }
            DialogResult = DialogResult.OK;
            Close();
        }

        public object ToPayload(int id = 0) => new
        {
            id,
            name          = _txtName.Text.Trim(),
            description   = _txtDescription.Text.Trim(),
            year          = (int)_numYear.Value,
            centre_number = _txtCentre.Text.Trim(),
            base_hours    = _numHours.Value,
            start_date    = _chkStart.Checked ? _dtpStart.Value.ToString("yyyy-MM-dd") : null,
            end_date      = _chkEnd.Checked   ? _dtpEnd.Value.ToString("yyyy-MM-dd")   : null,
            is_active     = _chkActive.Checked ? 1 : 0,
        };

        private static Label Lbl(string text, int x, int y) =>
            new Label { Text = text, Location = new Point(x, y), AutoSize = true };

        private static TextBox Txt(int x, int y, int w) =>
            new TextBox { Location = new Point(x, y), Size = new Size(w, 24), Font = new Font(Theme.FontFamily, 9f) };
    }
}
