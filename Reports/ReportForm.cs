/**
 * ReportForm — print-ready project report, rendered via the built-in
 * WebBrowser control. The user can print / save to PDF via the browser's
 * own print dialog, or export CSV / Excel.
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using OSPTracker.Models;
using OSPTracker.Services;
using OSPTracker.Utils;

namespace OSPTracker.Reports
{
    public partial class ReportForm : Form
    {
        private readonly int _projectId;
        private ReportOverviewDto _data;
        private Panel _toolbar;
        private Button _btnPrint, _btnCsv, _btnExcel;
        private Label _lblStatus;
        private WebBrowser _browser;

        public ReportForm(int projectId)
        {
            InitializeComponent();
            _projectId = projectId;
            BuildUi();
            Load += async (s, e) => await LoadReportAsync();
        }

        private void BuildUi()
        {
            if (Program.AppIcon != null) Icon = Program.AppIcon;
            Text = "Report";
            ClientSize = new Size(1000, 700);
            Font = new Font(Theme.FontFamily, 9f);
            StartPosition = FormStartPosition.CenterScreen;

            _toolbar = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = Color.FromArgb(240, 244, 248) };
            _btnPrint = Btn("Print / Save PDF", Theme.Primary, 8);
            _btnPrint.Click += (s, e) => _browser.Print();
            _btnCsv = Btn("Export CSV", Color.FromArgb(80, 80, 80), 150);
            _btnCsv.Click += (s, e) => { if (_data != null) ExportHelper.ExportCsv(_data, _data.Project.Name); };
            _btnExcel = Btn("Export Excel", Theme.Success, 260);
            _btnExcel.Click += (s, e) => { if (_data != null) ExportHelper.ExportExcel(_data, _data.Project.Name); };
            _toolbar.Controls.AddRange(new Control[] { _btnPrint, _btnCsv, _btnExcel });

            _lblStatus = new Label { Dock = DockStyle.Top, Height = 22, ForeColor = Color.Gray, Padding = new Padding(8, 2, 0, 0) };

            _browser = new WebBrowser { Dock = DockStyle.Fill };
            _browser.DocumentText = "<html><body style='font-family:Trebuchet MS;padding:20px;'><p style='color:#666'>Loading report…</p></body></html>";

            Controls.Add(_browser);
            Controls.Add(_lblStatus);
            Controls.Add(_toolbar);
        }

        private async System.Threading.Tasks.Task LoadReportAsync()
        {
            _lblStatus.Text = "Loading report...";
            try
            {
                var resp = await ApiService.Instance.GetAsync<SingleResponse<ReportOverviewDto>>("/reports/project-overview.php", $"project_id={_projectId}");
                _data = resp?.Data;
                if (_data == null) { _lblStatus.Text = "No data returned."; return; }

                Text = $"Report — {_data.Project.Name}";
                _browser.DocumentText = ReportBuilder.Build(_data);
                _lblStatus.Text = "";
            }
            catch (Exception ex)
            {
                _lblStatus.Text = "Error: " + ex.Message;
                _lblStatus.ForeColor = Theme.Danger;
            }
        }

        private static Button Btn(string text, Color back, int left) => new Button
        {
            Text = text, Left = left, Top = 7, Width = 130, Height = 30,
            BackColor = back, ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
            Font = new Font(Theme.FontFamily, 8.5f),
        };
    }
}
