/**
 * StaffPanel — admin CRUD for staff accounts.
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using OSPTracker.Models;
using OSPTracker.Services;

namespace OSPTracker.Admin
{
    public partial class StaffPanel : AdminPanelBase
    {
        public StaffPanel()
        {
            InitializeComponent();
            var btnReset = AddToolbarButton("Reset PW", System.Drawing.Color.FromArgb(140, 90, 0), 100);
            btnReset.Click += async (s, e) =>
            {
                if (SelectedRow?.Tag is StaffDto staff)
                    await SafeRunAsync(() => ResetPasswordAsync(staff));
            };
        }

        protected override string DeleteButtonText => "Deactivate";

        protected override void DefineColumns()
        {
            AddColText("ID",       "id",       0.4f);
            AddColText("Username", "username", 1f);
            AddColText("Name",     "name",     1.3f);
            AddColText("Email",    "email",    1.3f);
            AddColText("Role",     "role",     0.7f);
            AddColText("Status",   "status",   0.7f);
            AddColText("Last Login", "last_login", 1f);
            HideIdColumn();
        }

        protected override async Task LoadDataAsync()
        {
            _grid.Rows.Clear();
            var resp = await ApiService.Instance.GetAsync<ListResponse<StaffDto>>("/staff/index.php");
            foreach (var s in resp?.Data ?? new List<StaffDto>())
            {
                var row = AddRow(s.Id, s.Username, s.FullName, s.Email ?? "—", s.Role,
                    s.IsActive == 1 ? "Active" : "Inactive", s.LastLogin ?? "—");
                row.Tag = s;
            }
        }

        protected override async Task AddItemAsync()
        {
            using var dlg = new StaffEditDialog(null);
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
            var result = await ApiService.Instance.PostAsync<SingleResponse<MessageResult>>("/staff/create.php", dlg.ToCreatePayload());
            await ReloadAsync();
            if (!string.IsNullOrEmpty(result?.Data?.TempPassword))
            {
                Clipboard.SetText(result.Data.TempPassword);
                MessageBox.Show(FindForm(), $"Temporary password: {result.Data.TempPassword}\n\nCopied to clipboard — paste it into an email to the staff member.",
                    "Staff Created", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        protected override async Task EditItemAsync(DataGridViewRow row)
        {
            if (!(row.Tag is StaffDto s)) return;
            using var dlg = new StaffEditDialog(s);
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
            await ApiService.Instance.PutAsync<SingleResponse<MessageResult>>("/staff/update.php", dlg.ToUpdatePayload(s.Id));
            await ReloadAsync();
        }

        protected override async Task DeleteItemAsync(DataGridViewRow row)
        {
            if (!(row.Tag is StaffDto s)) return;
            await ApiService.Instance.DeleteAsync<SingleResponse<MessageResult>>("/staff/delete.php", new { id = s.Id });
            await ReloadAsync();
        }

        private async Task ResetPasswordAsync(StaffDto staff)
        {
            var result = await ApiService.Instance.PostAsync<SingleResponse<MessageResult>>("/staff/reset-password.php", new { id = staff.Id });
            await ReloadAsync();
            if (!string.IsNullOrEmpty(result?.Data?.TempPassword))
                Clipboard.SetText(result.Data.TempPassword);
            MessageBox.Show(FindForm(), $"Temporary password: {result?.Data?.TempPassword}\n\nCopied to clipboard — paste it into an email to the staff member.",
                "Password Reset", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
