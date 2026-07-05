/**
 * MainForm — application shell with tab pages for Dashboard, Projects and Admin.
 *
 * The Admin tab is only added for staff members with the 'admin' role,
 * mirroring the API's own role checks and the original React app's
 * route guard on /admin.
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System;
using System.Windows.Forms;
using OSPTracker.Services;

namespace OSPTracker.Forms
{
    public partial class MainForm : Form
    {
        private bool _loggingOut = false;

        public MainForm()
        {
            InitializeComponent();
            if (Program.AppIcon != null) Icon = Program.AppIcon;

            var staff = ApiService.Instance.CurrentStaff;
            _lblUser.Text = $"Logged in as: {staff?.FullName}" + (staff != null && staff.IsAdmin ? " (Admin)" : "");

            if (staff == null || !staff.IsAdmin)
            {
                _tabs.TabPages.Remove(_pageAdmin);
                _miAdmin.Visible = false;
            }

            _tabs.SelectedIndexChanged += async (s, e) =>
            {
                if (_tabs.SelectedTab == _pageDash)
                    await _dashboardPanel.RefreshAsync();
                else if (_tabs.SelectedTab == _pageProjects)
                    await _projectsPanel.RefreshAsync();
            };

            FormClosing += OnMainFormClosing;
        }

        private void OnDashboard(object sender, EventArgs e) => _tabs.SelectedTab = _pageDash;
        private void OnProjects(object sender, EventArgs e)  => _tabs.SelectedTab = _pageProjects;
        private void OnAdmin(object sender, EventArgs e)     => _tabs.SelectedTab = _pageAdmin;
        private void OnExit(object sender, EventArgs e)      => Close();

        private void OnChangePassword(object sender, EventArgs e)
        {
            using (var dlg = new ChangePasswordForm(forced: false))
                dlg.ShowDialog(this);

            // Any successful password change logs the API session out.
            if (!ApiService.Instance.IsAuthenticated)
                OnLogout(sender, e);
        }

        private void OnLogout(object sender, EventArgs e)
        {
            _loggingOut = true;
            ApiService.Instance.Logout();
            Close();
        }

        private void OnAbout(object sender, EventArgs e)
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            MessageBox.Show(this,
                $"OSP Hours Tracker\nVersion {version}\n\n" +
                "© 2026 Simon Rundell, Exeter College / CodeMonkey Design Ltd.\n" +
                "Released under the Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International (CC BY-NC-SA 4.0) license.",
                "About OSP Hours Tracker",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void OnMainFormClosing(object sender, FormClosingEventArgs e)
        {
            if (_loggingOut) return;
            if (e.CloseReason != CloseReason.UserClosing) return;

            var result = MessageBox.Show(this,
                "Do you really want to exit the OSP Hours Tracker?",
                "Confirm Exit",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            if (result != DialogResult.Yes)
                e.Cancel = true;
        }
    }
}
