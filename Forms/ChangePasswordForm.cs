/**
 * ChangePasswordForm — change the current user's password.
 *
 * Used both voluntarily (from the MainForm menu) and as a forced step
 * straight after login when the staff account has must_change_password
 * set. When forced, Cancel and the window close button are disabled —
 * the form can only be dismissed by a successful save. Any successful
 * change logs the user out (matching the API, which does not re-issue a
 * token) so the next login picks up a fresh JWT without the flag set.
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System;
using System.Windows.Forms;
using OSPTracker.Services;

namespace OSPTracker.Forms
{
    public partial class ChangePasswordForm : Form
    {
        private readonly bool _forced;

        public ChangePasswordForm() : this(forced: false) { }

        public ChangePasswordForm(bool forced)
        {
            InitializeComponent();
            _forced = forced;
            _btnSave.FlatAppearance.BorderSize   = 0;
            _btnCancel.FlatAppearance.BorderSize = 0;

            if (_forced)
            {
                _btnCancel.Visible = false;
                ControlBox         = false;
                _lblForced.Visible = true;
            }
        }

        private async void OnSave(object sender, EventArgs e)
        {
            _lblError.Text = "";

            if (_txtNew.Text.Length < 8)
            {
                _lblError.Text = "New password must be at least 8 characters.";
                return;
            }
            if (!System.Text.RegularExpressions.Regex.IsMatch(_txtNew.Text, "[A-Z]"))
            {
                _lblError.Text = "New password must contain at least one uppercase letter.";
                return;
            }
            if (!System.Text.RegularExpressions.Regex.IsMatch(_txtNew.Text, "[0-9]"))
            {
                _lblError.Text = "New password must contain at least one digit.";
                return;
            }
            if (_txtNew.Text != _txtConfirm.Text)
            {
                _lblError.Text = "Passwords do not match.";
                return;
            }

            _btnSave.Enabled = false;
            try
            {
                await ApiService.Instance.ChangePasswordAsync(_txtCurrent.Text, _txtNew.Text);
                ApiService.Instance.Logout();
                MessageBox.Show(this, "Password changed successfully. Please log in again.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
            }
            catch (Exception ex)
            {
                _lblError.Text   = ex.Message;
                _btnSave.Enabled = true;
            }
        }

        private void OnCancel(object sender, EventArgs e) => Close();
    }
}
