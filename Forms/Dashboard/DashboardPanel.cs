/**
 * DashboardPanel — shows all active projects as cards with a quick summary
 * and shortcuts to Sessions, Report and Detail. Mirrors the React app's
 * DashboardPage card grid.
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using OSPTracker.Models;
using OSPTracker.Services;
using OSPTracker.Utils;

namespace OSPTracker.Forms.Dashboard
{
    public partial class DashboardPanel : UserControl
    {
        private Panel        _header;
        private Label        _lblTitle;
        private Label        _lblError;
        private FlowLayoutPanel _flow;

        public DashboardPanel()
        {
            InitializeComponent();
            BuildUi();
            Load += async (s, e) => await RefreshAsync();
        }

        private void BuildUi()
        {
            Dock      = DockStyle.Fill;
            BackColor = Theme.Bg;

            _header = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = Theme.Bg };
            _lblTitle = new Label
            {
                Text = "Dashboard", Left = 12, Top = 10, AutoSize = true,
                Font = new Font(Theme.FontFamily, 14f, FontStyle.Bold),
                ForeColor = Theme.Primary,
            };
            _header.Controls.Add(_lblTitle);

            _lblError = new Label
            {
                Dock = DockStyle.Top, Height = 24, ForeColor = Theme.Danger,
                Padding = new Padding(12, 4, 0, 0),
            };

            _flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(12),
                FlowDirection = FlowDirection.LeftToRight, WrapContents = true,
            };

            Controls.Add(_flow);
            Controls.Add(_lblError);
            Controls.Add(_header);
        }

        public async Task RefreshAsync()
        {
            _lblError.Text = "";
            _flow.Controls.Clear();
            try
            {
                var resp = await ApiService.Instance.GetAsync<ListResponse<ProjectDto>>("/projects/index.php");
                var projects = (resp?.Data ?? new List<ProjectDto>()).Where(p => p.IsActive == 1).ToList();

                if (projects.Count == 0)
                {
                    _flow.Controls.Add(new Label
                    {
                        Text = "No active projects found.", AutoSize = true, ForeColor = Color.Gray,
                        Margin = new Padding(8),
                    });
                    return;
                }

                foreach (var p in projects) _flow.Controls.Add(BuildCard(p));
            }
            catch (Exception ex)
            {
                _lblError.Text = ex.Message;
            }
        }

        private Control BuildCard(ProjectDto p)
        {
            var card = new Panel
            {
                Width = 260, Height = 190, Margin = new Padding(8),
                BackColor = Theme.Surface, BorderStyle = BorderStyle.FixedSingle,
            };

            var lblName = new Label
            {
                Text = p.Name, Left = 10, Top = 8, Width = 200, Height = 20,
                Font = new Font(Theme.FontFamily, 10.5f, FontStyle.Bold),
            };
            var lblYear = new Label
            {
                Text = p.Year.ToString(), Left = 214, Top = 8, Width = 40, Height = 20,
                TextAlign = ContentAlignment.MiddleCenter, BackColor = Theme.Secondary,
                ForeColor = Color.White, Font = new Font(Theme.FontFamily, 8f, FontStyle.Bold),
            };
            var lblCentre = new Label
            {
                Text = p.CentreNumber, Left = 10, Top = 30, Width = 240, Height = 16,
                ForeColor = Color.Gray, Font = new Font(Theme.FontFamily, 7.5f),
            };
            var lblHours = new Label
            {
                Text = $"{p.BaseHours}h base allowance", Left = 10, Top = 50, Width = 240, Height = 18,
            };
            var lblStudents = new Label
            {
                Text = $"{p.StudentCount} students enrolled", Left = 10, Top = 70, Width = 240, Height = 18,
            };
            int remaining = p.RemainingUnscheduledMinutes;
            var lblRemaining = new Label
            {
                Text = $"{remaining} mins unscheduled", Left = 10, Top = 90, Width = 240, Height = 18,
                ForeColor = remaining < 0 ? Theme.Danger : Color.Black,
            };

            var btnSessions = Btn("Sessions", Theme.Secondary);
            btnSessions.Left = 10; btnSessions.Top = 120; btnSessions.Width = 75;
            btnSessions.Click += (s, e) => new SessionListForm(p.Id).Show();

            var btnReport = Btn("Report", Color.FromArgb(140, 140, 150));
            btnReport.Left = 90; btnReport.Top = 120; btnReport.Width = 75;
            btnReport.Click += (s, e) => new Reports.ReportForm(p.Id).Show();

            var btnDetail = Btn("Detail", Color.FromArgb(140, 140, 150));
            btnDetail.Left = 170; btnDetail.Top = 120; btnDetail.Width = 75;
            btnDetail.Click += (s, e) => new ProjectDetailForm(p.Id).Show();

            card.Controls.AddRange(new Control[] {
                lblName, lblYear, lblCentre, lblHours, lblStudents, lblRemaining,
                btnSessions, btnReport, btnDetail,
            });
            return card;
        }

        private static Button Btn(string text, Color back) => new Button
        {
            Text = text, Height = 28, BackColor = back, ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat, Font = new Font(Theme.FontFamily, 8f),
        };
    }
}
