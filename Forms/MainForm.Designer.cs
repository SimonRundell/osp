using OSPTracker.Admin;
using OSPTracker.Forms.Dashboard;

namespace OSPTracker.Forms
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this._menuStrip        = new System.Windows.Forms.MenuStrip();
            this._fileMenu         = new System.Windows.Forms.ToolStripMenuItem();
            this._miDashboard      = new System.Windows.Forms.ToolStripMenuItem();
            this._miProjects       = new System.Windows.Forms.ToolStripMenuItem();
            this._miAdmin          = new System.Windows.Forms.ToolStripMenuItem();
            this._sep1             = new System.Windows.Forms.ToolStripSeparator();
            this._miChangePassword = new System.Windows.Forms.ToolStripMenuItem();
            this._sep2             = new System.Windows.Forms.ToolStripSeparator();
            this._miLogout         = new System.Windows.Forms.ToolStripMenuItem();
            this._sep3             = new System.Windows.Forms.ToolStripSeparator();
            this._miExit           = new System.Windows.Forms.ToolStripMenuItem();
            this._helpMenu         = new System.Windows.Forms.ToolStripMenuItem();
            this._miAbout          = new System.Windows.Forms.ToolStripMenuItem();
            this._status           = new System.Windows.Forms.StatusStrip();
            this._lblUser          = new System.Windows.Forms.ToolStripStatusLabel();
            this._lblVersion       = new System.Windows.Forms.ToolStripStatusLabel();
            this._tabs             = new System.Windows.Forms.TabControl();
            this._pageDash         = new System.Windows.Forms.TabPage();
            this._pageProjects     = new System.Windows.Forms.TabPage();
            this._pageAdmin        = new System.Windows.Forms.TabPage();
            this._dashboardPanel   = new DashboardPanel();
            this._projectsPanel    = new ProjectsPanel();
            this._adminPanel       = new AdminTabPanel();
            this._menuStrip.SuspendLayout();
            this._status.SuspendLayout();
            this._tabs.SuspendLayout();
            this._pageDash.SuspendLayout();
            this._pageProjects.SuspendLayout();
            this._pageAdmin.SuspendLayout();
            this.SuspendLayout();
            //
            // _menuStrip
            //
            this._menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { this._fileMenu, this._helpMenu });
            this._menuStrip.Location = new System.Drawing.Point(0, 0);
            this._menuStrip.Name     = "_menuStrip";
            this._menuStrip.Size     = new System.Drawing.Size(1264, 24);
            //
            // _fileMenu
            //
            this._fileMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this._miDashboard,
                this._miProjects,
                this._miAdmin,
                this._sep1,
                this._miChangePassword,
                this._sep2,
                this._miLogout,
                this._sep3,
                this._miExit
            });
            this._fileMenu.Name = "_fileMenu";
            this._fileMenu.Text = "File";
            //
            // _miDashboard
            //
            this._miDashboard.Name         = "_miDashboard";
            this._miDashboard.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D;
            this._miDashboard.Text         = "Dashboard";
            this._miDashboard.Click       += new System.EventHandler(this.OnDashboard);
            //
            // _miProjects
            //
            this._miProjects.Name         = "_miProjects";
            this._miProjects.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.P;
            this._miProjects.Text         = "Projects";
            this._miProjects.Click       += new System.EventHandler(this.OnProjects);
            //
            // _miAdmin
            //
            this._miAdmin.Name         = "_miAdmin";
            this._miAdmin.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.A;
            this._miAdmin.Text         = "Admin Panel...";
            this._miAdmin.Click       += new System.EventHandler(this.OnAdmin);
            //
            // _sep1, _sep2, _sep3
            //
            this._sep1.Name = "_sep1";
            this._sep2.Name = "_sep2";
            this._sep3.Name = "_sep3";
            //
            // _miChangePassword
            //
            this._miChangePassword.Name   = "_miChangePassword";
            this._miChangePassword.Text   = "Change Password...";
            this._miChangePassword.Click += new System.EventHandler(this.OnChangePassword);
            //
            // _miLogout
            //
            this._miLogout.Name   = "_miLogout";
            this._miLogout.Text   = "Logout";
            this._miLogout.Click += new System.EventHandler(this.OnLogout);
            //
            // _miExit
            //
            this._miExit.Name   = "_miExit";
            this._miExit.Text   = "Exit";
            this._miExit.Click += new System.EventHandler(this.OnExit);
            //
            // _helpMenu
            //
            this._helpMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { this._miAbout });
            this._helpMenu.Name = "_helpMenu";
            this._helpMenu.Text = "Help";
            //
            // _miAbout
            //
            this._miAbout.Name   = "_miAbout";
            this._miAbout.Text   = "About...";
            this._miAbout.Click += new System.EventHandler(this.OnAbout);
            //
            // _status
            //
            this._status.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { this._lblUser, this._lblVersion });
            this._status.Name = "_status";
            //
            // _lblUser
            //
            this._lblUser.Name = "_lblUser";
            this._lblUser.Text = "Logged in as:";
            //
            // _lblVersion
            //
            this._lblVersion.Name      = "_lblVersion";
            this._lblVersion.Spring    = true;
            this._lblVersion.Text      = "© 2026 Simon Rundell. Exeter College — CC NC-BY-SA 4.0";
            this._lblVersion.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // _tabs
            //
            this._tabs.Controls.Add(this._pageDash);
            this._tabs.Controls.Add(this._pageProjects);
            this._tabs.Controls.Add(this._pageAdmin);
            this._tabs.Dock     = System.Windows.Forms.DockStyle.Fill;
            this._tabs.Font     = new System.Drawing.Font("Trebuchet MS", 9F, System.Drawing.FontStyle.Bold);
            this._tabs.Location = new System.Drawing.Point(0, 24);
            this._tabs.Name     = "_tabs";
            //
            // _pageDash
            //
            this._pageDash.Controls.Add(this._dashboardPanel);
            this._pageDash.Name = "_pageDash";
            this._pageDash.Text = "Dashboard";
            //
            // _dashboardPanel
            //
            this._dashboardPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._dashboardPanel.Name = "_dashboardPanel";
            //
            // _pageProjects
            //
            this._pageProjects.Controls.Add(this._projectsPanel);
            this._pageProjects.Name = "_pageProjects";
            this._pageProjects.Text = "Projects";
            //
            // _projectsPanel
            //
            this._projectsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._projectsPanel.Name = "_projectsPanel";
            //
            // _pageAdmin
            //
            this._pageAdmin.Controls.Add(this._adminPanel);
            this._pageAdmin.Name = "_pageAdmin";
            this._pageAdmin.Text = "Admin";
            //
            // _adminPanel
            //
            this._adminPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._adminPanel.Name = "_adminPanel";
            //
            // MainForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize          = new System.Drawing.Size(1264, 761);
            this.Controls.Add(this._tabs);
            this.Controls.Add(this._status);
            this.Controls.Add(this._menuStrip);
            this.Font           = new System.Drawing.Font("Trebuchet MS", 9F);
            this.MainMenuStrip  = this._menuStrip;
            this.MinimumSize    = new System.Drawing.Size(900, 600);
            this.Name           = "MainForm";
            this.StartPosition  = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text           = "OSP Hours Tracker";
            this._menuStrip.ResumeLayout(false);
            this._menuStrip.PerformLayout();
            this._status.ResumeLayout(false);
            this._status.PerformLayout();
            this._pageDash.ResumeLayout(false);
            this._pageProjects.ResumeLayout(false);
            this._pageAdmin.ResumeLayout(false);
            this._tabs.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.MenuStrip            _menuStrip;
        private System.Windows.Forms.ToolStripMenuItem     _fileMenu;
        private System.Windows.Forms.ToolStripMenuItem     _miDashboard;
        private System.Windows.Forms.ToolStripMenuItem     _miProjects;
        private System.Windows.Forms.ToolStripMenuItem     _miAdmin;
        private System.Windows.Forms.ToolStripSeparator     _sep1;
        private System.Windows.Forms.ToolStripMenuItem     _miChangePassword;
        private System.Windows.Forms.ToolStripSeparator     _sep2;
        private System.Windows.Forms.ToolStripMenuItem     _miLogout;
        private System.Windows.Forms.ToolStripSeparator     _sep3;
        private System.Windows.Forms.ToolStripMenuItem     _miExit;
        private System.Windows.Forms.ToolStripMenuItem     _helpMenu;
        private System.Windows.Forms.ToolStripMenuItem     _miAbout;
        private System.Windows.Forms.StatusStrip           _status;
        private System.Windows.Forms.ToolStripStatusLabel  _lblUser;
        private System.Windows.Forms.ToolStripStatusLabel  _lblVersion;
        private System.Windows.Forms.TabControl            _tabs;
        private System.Windows.Forms.TabPage               _pageDash;
        private System.Windows.Forms.TabPage               _pageProjects;
        private System.Windows.Forms.TabPage               _pageAdmin;
        private DashboardPanel                             _dashboardPanel;
        private ProjectsPanel                               _projectsPanel;
        private AdminTabPanel                                _adminPanel;
    }
}
