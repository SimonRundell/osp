/**
 * AdminTabPanel — tabbed admin interface: Staff, Students, Projects.
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System.Windows.Forms;
using OSPTracker.Utils;

namespace OSPTracker.Admin
{
    public partial class AdminTabPanel : UserControl
    {
        public AdminTabPanel()
        {
            InitializeComponent();
            BuildUi();
        }

        private void BuildUi()
        {
            Dock = DockStyle.Fill;

            var tabs = new TabControl { Dock = DockStyle.Fill, Font = new System.Drawing.Font(Theme.FontFamily, 9f) };

            var pgStaff = new TabPage("Staff");
            pgStaff.Controls.Add(new StaffPanel { Dock = DockStyle.Fill });

            var pgStudents = new TabPage("Students");
            pgStudents.Controls.Add(new StudentsPanel { Dock = DockStyle.Fill });

            var pgProjects = new TabPage("Projects");
            pgProjects.Controls.Add(new ProjectAdminPanel { Dock = DockStyle.Fill });

            tabs.TabPages.AddRange(new[] { pgStaff, pgStudents, pgProjects });
            Controls.Add(tabs);
        }
    }
}
