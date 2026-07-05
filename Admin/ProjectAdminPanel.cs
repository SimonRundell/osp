/**
 * ProjectAdminPanel — admin CRUD for OSP projects (create, edit, activate/deactivate toggle).
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using OSPTracker.Models;
using OSPTracker.Services;

namespace OSPTracker.Admin
{
    public partial class ProjectAdminPanel : AdminPanelBase
    {
        public ProjectAdminPanel()
        {
            InitializeComponent();
        }

        protected override string DeleteButtonText => "Toggle Active";
        protected override string DeleteConfirmMessage => "Toggle this project's active status?";

        protected override void DefineColumns()
        {
            AddColText("ID",     "id",     0.4f);
            AddColText("Name",   "name",   2f);
            AddColText("Year",   "year",   0.6f);
            AddColText("Centre", "centre", 0.8f);
            AddColText("Base Hours", "hours", 0.8f);
            AddColText("Start",  "start",  1f);
            AddColText("End",    "end",    1f);
            AddColText("Status", "status", 0.8f);
            HideIdColumn();
        }

        protected override async Task LoadDataAsync()
        {
            _grid.Rows.Clear();
            var resp = await ApiService.Instance.GetAsync<ListResponse<ProjectDto>>("/projects/index.php");
            foreach (var p in resp?.Data ?? new List<ProjectDto>())
            {
                var row = AddRow(p.Id, p.Name, p.Year, p.CentreNumber, $"{p.BaseHours}h",
                    p.StartDate ?? "—", p.EndDate ?? "—", p.IsActive == 1 ? "Active" : "Inactive");
                row.Tag = p;
            }
        }

        protected override async Task AddItemAsync()
        {
            using var dlg = new ProjectEditDialog(null);
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
            await ApiService.Instance.PostAsync<SingleResponse<MessageResult>>("/projects/create.php", dlg.ToPayload());
            await ReloadAsync();
        }

        protected override async Task EditItemAsync(DataGridViewRow row)
        {
            if (!(row.Tag is ProjectDto p)) return;
            using var dlg = new ProjectEditDialog(p);
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
            await ApiService.Instance.PutAsync<SingleResponse<MessageResult>>("/projects/update.php", dlg.ToPayload(p.Id));
            await ReloadAsync();
        }

        /// <summary>"Delete" for projects means toggling is_active — no data is ever removed.</summary>
        protected override async Task DeleteItemAsync(DataGridViewRow row)
        {
            if (!(row.Tag is ProjectDto p)) return;
            await ApiService.Instance.PutAsync<SingleResponse<MessageResult>>("/projects/update.php", new
            {
                id = p.Id, p.Name, p.Description, p.Year, p.CentreNumber, p.BaseHours,
                start_date = p.StartDate, end_date = p.EndDate,
                is_active = p.IsActive == 1 ? 0 : 1,
            });
            await ReloadAsync();
        }
    }
}
