/**
 * LoginForm — initial login dialog for OSPTracker.
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System;
using System.Windows.Forms;
using OSPTracker.Services;

namespace OSPTracker.Forms
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
            if (Program.AppIcon != null) Icon = Program.AppIcon;
            _btnLogin.FlatAppearance.BorderSize = 0;
            _txtUsername.Focus();
        }

        private async void OnLoginClick(object sender, EventArgs e)
        {
            _lblError.Text    = "";
            _btnLogin.Enabled = false;
            _btnLogin.Text    = "Signing in...";

            try
            {
                await ApiService.Instance.LoginAsync(
                    _txtUsername.Text.Trim(),
                    _txtPassword.Text);

                var staff = ApiService.Instance.CurrentStaff;
                if (staff.MustChangePassword == 1)
                {
                    Hide();
                    using (var dlg = new ChangePasswordForm(forced: true))
                        dlg.ShowDialog(this);
                    // Change password always logs the user out afterwards
                    // (forced or voluntary) so the JWT is re-issued cleanly.
                    Show();
                    _txtPassword.Text = "";
                    _btnLogin.Enabled = true;
                    _btnLogin.Text    = "Sign In";
                }
                else
                {
                    Hide();
                    using (var main = new MainForm())
                        main.ShowDialog(this);

                    if (ApiService.Instance.IsAuthenticated)
                    {
                        // MainForm closed via Exit/window-close, not Logout — exit the app.
                        Close();
                        return;
                    }

                    // MainForm closed via Logout — show the login screen again.
                    Show();
                    _txtUsername.Text = "";
                    _txtPassword.Text = "";
                    _btnLogin.Enabled = true;
                    _btnLogin.Text    = "Sign In";
                    _txtUsername.Focus();
                }
            }
            catch (Exception ex)
            {
                _lblError.Text = ex.Message.Contains("401")
                    ? "Username or password incorrect — please try again."
                    : ex.Message;
                _btnLogin.Enabled = true;
                _btnLogin.Text    = "Sign In";
            }
        }
    }
}
