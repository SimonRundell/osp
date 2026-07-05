/**
 * StaffEditDialog — add/edit a staff account.
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System.Drawing;
using System.Windows.Forms;
using OSPTracker.Models;
using OSPTracker.Utils;

namespace OSPTracker.Admin
{
    public partial class StaffEditDialog : Form
    {
        private readonly bool _isEdit;
        private TextBox   _txtUsername, _txtFirst, _txtLast, _txtEmail;
        private ComboBox  _cboRole, _cboStatus;
        private Label     _lblStatus;

        public StaffEditDialog(StaffDto s)
        {
            InitializeComponent();
            _isEdit = s != null;
            BuildUi();
            if (_isEdit)
            {
                Text = "Edit Staff Member";
                _txtUsername.Text  = s.Username;
                _txtUsername.Enabled = false;
                _txtFirst.Text  = s.FirstName;
                _txtLast.Text   = s.LastName;
                _txtEmail.Text  = s.Email ?? "";
                _cboRole.SelectedItem = s.Role == "admin" ? "admin" : "staff";
                _cboStatus.SelectedIndex = s.IsActive == 1 ? 0 : 1;
            }
            else
            {
                Text = "Add Staff Member";
                _lblStatus.Visible = _cboStatus.Visible = false;
            }
        }

        private void BuildUi()
        {
            Font = new Font(Theme.FontFamily, 9f);
            ClientSize = new Size(360, 310);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;

            var lblUser = Lbl("Username", 20, 16);
            _txtUsername = Txt(20, 34, 320);

            var lblFirst = Lbl("First Name", 20, 66);
            _txtFirst = Txt(20, 84, 320);

            var lblLast = Lbl("Last Name", 20, 116);
            _txtLast = Txt(20, 134, 320);

            var lblEmail = Lbl("Email (optional)", 20, 166);
            _txtEmail = Txt(20, 184, 320);

            var lblRole = Lbl("Role", 20, 216);
            _cboRole = new ComboBox
            {
                Location = new Point(20, 234), Size = new Size(150, 24),
                DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font(Theme.FontFamily, 9f),
            };
            _cboRole.Items.AddRange(new object[] { "staff", "admin" });
            _cboRole.SelectedIndex = 0;

            _lblStatus = Lbl("Status", 190, 216);
            _cboStatus = new ComboBox
            {
                Location = new Point(190, 234), Size = new Size(150, 24),
                DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font(Theme.FontFamily, 9f),
            };
            _cboStatus.Items.AddRange(new object[] { "Active", "Inactive" });
            _cboStatus.SelectedIndex = 0;

            var btnSave = new Button
            {
                Text = _isEdit ? "Save Changes" : "Create Staff", DialogResult = DialogResult.OK,
                Location = new Point(130, 264), Size = new Size(110, 32),
                BackColor = Theme.Primary, ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
            };
            var btnCancel = new Button
            {
                Text = "Cancel", DialogResult = DialogResult.Cancel,
                Location = new Point(246, 264), Size = new Size(94, 32),
                BackColor = Color.FromArgb(140, 140, 150), ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
            };
            AcceptButton = btnSave;
            CancelButton = btnCancel;

            Controls.AddRange(new Control[] {
                lblUser, _txtUsername, lblFirst, _txtFirst, lblLast, _txtLast,
                lblEmail, _txtEmail, lblRole, _cboRole, _lblStatus, _cboStatus,
                btnSave, btnCancel,
            });
        }

        public object ToCreatePayload() => new
        {
            username   = _txtUsername.Text.Trim(),
            first_name = _txtFirst.Text.Trim(),
            last_name  = _txtLast.Text.Trim(),
            email      = _txtEmail.Text.Trim(),
            role       = _cboRole.SelectedItem?.ToString() ?? "staff",
        };

        public object ToUpdatePayload(int id) => new
        {
            id,
            first_name = _txtFirst.Text.Trim(),
            last_name  = _txtLast.Text.Trim(),
            email      = _txtEmail.Text.Trim(),
            role       = _cboRole.SelectedItem?.ToString() ?? "staff",
            is_active  = _cboStatus.SelectedIndex == 0 ? 1 : 0,
        };

        private static Label Lbl(string text, int x, int y) =>
            new Label { Text = text, Location = new Point(x, y), AutoSize = true };

        private static TextBox Txt(int x, int y, int w) =>
            new TextBox { Location = new Point(x, y), Size = new Size(w, 24), Font = new Font(Theme.FontFamily, 9f) };
    }
}
