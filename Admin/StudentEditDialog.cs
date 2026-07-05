/**
 * StudentEditDialog — add/edit a student record.
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System.Drawing;
using System.Windows.Forms;
using OSPTracker.Models;
using OSPTracker.Utils;

namespace OSPTracker.Admin
{
    public partial class StudentEditDialog : Form
    {
        private readonly bool _isEdit;
        private TextBox _txtCandidate, _txtCis, _txtSurname, _txtFirst;
        private Label   _lblError;

        public StudentEditDialog(StudentDto s)
        {
            InitializeComponent();
            _isEdit = s != null;
            BuildUi();
            if (_isEdit)
            {
                Text = "Edit Student";
                _txtCandidate.Text = s.CandidateNumber;
                _txtCis.Text       = s.CisRef ?? "";
                _txtSurname.Text   = s.Surname;
                _txtFirst.Text     = s.FirstName;
            }
            else
            {
                Text = "Add Student";
            }
        }

        private void BuildUi()
        {
            Font = new Font(Theme.FontFamily, 9f);
            ClientSize = new Size(360, 290);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;

            var lblCandidate = Lbl("Candidate Number *", 20, 16);
            _txtCandidate = Txt(20, 34, 320);
            _txtCandidate.MaxLength = 30;
            var lblHint = new Label
            {
                Text = "Required. Max 30 characters, e.g. LL-000020681",
                Location = new Point(20, 60), AutoSize = true,
                Font = new Font(Theme.FontFamily, 7.5f, FontStyle.Italic), ForeColor = Color.Gray,
            };

            var lblCis = Lbl("CIS Reference (optional)", 20, 84);
            _txtCis = Txt(20, 102, 320);

            var lblSurname = Lbl("Surname *", 20, 134);
            _txtSurname = Txt(20, 152, 320);

            var lblFirst = Lbl("First Name *", 20, 184);
            _txtFirst = Txt(20, 202, 320);

            _lblError = new Label { Location = new Point(20, 228), Size = new Size(320, 20), ForeColor = Theme.Danger };

            var btnSave = new Button
            {
                Text = _isEdit ? "Save Changes" : "Create Student",
                Location = new Point(130, 254), Size = new Size(110, 32),
                BackColor = Theme.Primary, ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
            };
            btnSave.Click += OnSave;
            var btnCancel = new Button
            {
                Text = "Cancel", DialogResult = DialogResult.Cancel,
                Location = new Point(246, 254), Size = new Size(94, 32),
                BackColor = Color.FromArgb(140, 140, 150), ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
            };
            AcceptButton = btnSave;
            CancelButton = btnCancel;

            Controls.AddRange(new Control[] {
                lblCandidate, _txtCandidate, lblHint, lblCis, _txtCis,
                lblSurname, _txtSurname, lblFirst, _txtFirst, _lblError,
                btnSave, btnCancel,
            });
        }

        private void OnSave(object sender, System.EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtCandidate.Text) || _txtCandidate.Text.Trim().Length > 30)
            {
                _lblError.Text = "Candidate number is required and must be 30 characters or fewer.";
                return;
            }
            if (string.IsNullOrWhiteSpace(_txtSurname.Text) || string.IsNullOrWhiteSpace(_txtFirst.Text))
            {
                _lblError.Text = "Surname and first name are required.";
                return;
            }
            DialogResult = DialogResult.OK;
            Close();
        }

        public object ToPayload(int id = 0) => new
        {
            id,
            candidate_number = _txtCandidate.Text.Trim(),
            cis_ref          = _txtCis.Text.Trim(),
            surname          = _txtSurname.Text.Trim(),
            first_name       = _txtFirst.Text.Trim(),
        };

        private static Label Lbl(string text, int x, int y) =>
            new Label { Text = text, Location = new Point(x, y), AutoSize = true };

        private static TextBox Txt(int x, int y, int w) =>
            new TextBox { Location = new Point(x, y), Size = new Size(w, 24), Font = new Font(Theme.FontFamily, 9f) };
    }
}
