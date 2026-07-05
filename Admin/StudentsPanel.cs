/**
 * StudentsPanel — admin CRUD for students.
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
    public partial class StudentsPanel : AdminPanelBase
    {
        private TextBox _filterCandidate, _filterSurname, _filterFirst;

        public StudentsPanel()
        {
            InitializeComponent();
            var btnImport = AddToolbarButton("Import CSV...", System.Drawing.Color.FromArgb(140, 90, 0), 130);
            btnImport.Click += async (s, e) =>
            {
                using var dlg = new StudentImportDialog();
                if (dlg.ShowDialog(FindForm()) == DialogResult.OK)
                    await SafeRunAsync(ReloadAsync);
            };
        }

        protected override Control BuildFilterBar()
        {
            var bar = MakeFilterPanel();
            _filterCandidate = AddFilterBox(bar, "Candidate #", 52, 120);
            _filterSurname   = AddFilterBox(bar, "Surname",     180, 130);
            _filterFirst     = AddFilterBox(bar, "First Name",  318, 130);
            AddClearButton(bar, 456, _filterCandidate, _filterSurname, _filterFirst);
            return bar;
        }

        protected override bool RowMatchesFilter(DataGridViewRow row)
            => CellContains(row, 1, _filterCandidate?.Text)
            && CellContains(row, 2, _filterSurname?.Text)
            && CellContains(row, 3, _filterFirst?.Text);

        protected override void DefineColumns()
        {
            AddColText("ID",         "id",         0.4f);
            AddColText("Candidate #","candidate",  1.2f);
            AddColText("Surname",    "surname",    1f);
            AddColText("First Name", "first_name", 1f);
            AddColText("CIS Ref",    "cis_ref",    1f);
            AddColText("Status",     "status",     0.7f);
            HideIdColumn();
        }

        protected override async Task LoadDataAsync()
        {
            _grid.Rows.Clear();
            var resp = await ApiService.Instance.GetAsync<ListResponse<StudentDto>>("/students/index.php");
            foreach (var s in resp?.Data ?? new List<StudentDto>())
            {
                var row = AddRow(s.Id, s.CandidateNumber, s.Surname, s.FirstName, s.CisRef ?? "—",
                    s.IsActive == 1 ? "Active" : "Inactive");
                row.Tag = s;
            }
        }

        protected override async Task AddItemAsync()
        {
            using var dlg = new StudentEditDialog(null);
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
            await ApiService.Instance.PostAsync<SingleResponse<MessageResult>>("/students/create.php", dlg.ToPayload());
            await ReloadAsync();
        }

        protected override async Task EditItemAsync(DataGridViewRow row)
        {
            if (!(row.Tag is StudentDto s)) return;
            using var dlg = new StudentEditDialog(s);
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
            await ApiService.Instance.PutAsync<SingleResponse<MessageResult>>("/students/update.php", dlg.ToPayload(s.Id));
            await ReloadAsync();
        }

        protected override async Task DeleteItemAsync(DataGridViewRow row)
        {
            if (!(row.Tag is StudentDto s)) return;
            await ApiService.Instance.DeleteAsync<SingleResponse<MessageResult>>("/students/delete.php", new { id = s.Id });
            await ReloadAsync();
        }
    }
}
