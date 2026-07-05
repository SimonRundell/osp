/**
 * StudentImportDialog — bulk-import students from a UTF-8 CSV file
 * (as exported by Excel's "CSV UTF-8" format).
 *
 * Expected columns, matched flexibly by header text: Candidate # (required),
 * Surname (required), Firstname (required), CIS Ref (optional). New
 * students are created active by default; a candidate number that already
 * exists is treated as an update rather than a duplicate error.
 *
 * Every row is parsed and previewed first — rows missing a required field
 * are flagged and excluded — before anything is sent to the server.
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using OSPTracker.Models;
using OSPTracker.Services;
using OSPTracker.Utils;

namespace OSPTracker.Admin
{
    public partial class StudentImportDialog : Form
    {
        private TextBox _txtPath;
        private Button  _btnBrowse, _btnImport, _btnCancel;
        private Label   _lblSummary, _lblError;
        private DataGridView _grid;

        private readonly List<StudentImportRow> _validRows = new List<StudentImportRow>();

        public StudentImportDialog()
        {
            InitializeComponent();
            BuildUi();
        }

        private void BuildUi()
        {
            Font = new Font(Theme.FontFamily, 9f);
            Text = "Import Students from CSV";
            ClientSize = new Size(720, 480);
            FormBorderStyle = FormBorderStyle.Sizable;
            MinimumSize = new Size(560, 360);
            StartPosition = FormStartPosition.CenterParent;

            var top = new Panel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(8) };
            _txtPath = new TextBox { Left = 8, Top = 9, Width = 500, ReadOnly = true, Font = new Font(Theme.FontFamily, 9f) };
            _btnBrowse = new Button
            {
                Text = "Browse...", Left = 516, Top = 7, Width = 100, Height = 26,
                BackColor = Theme.Secondary, ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
            };
            _btnBrowse.Click += OnBrowse;
            top.Controls.AddRange(new Control[] { _txtPath, _btnBrowse });

            var hint = new Label
            {
                Dock = DockStyle.Top, Height = 20, Padding = new Padding(8, 0, 0, 0),
                ForeColor = Color.Gray, Font = new Font(Theme.FontFamily, 7.5f, FontStyle.Italic),
                Text = "Expects columns: Candidate # (required), Surname (required), Firstname (required), CIS Ref (optional). New students are created Active.",
            };

            _lblSummary = new Label { Dock = DockStyle.Top, Height = 26, Padding = new Padding(8, 4, 0, 0), Font = new Font(Theme.FontFamily, 9f, FontStyle.Bold) };
            _lblError   = new Label { Dock = DockStyle.Top, Height = 22, Padding = new Padding(8, 2, 0, 0), ForeColor = Theme.Danger };

            var bottom = new Panel { Dock = DockStyle.Bottom, Height = 48, Padding = new Padding(8) };
            _btnImport = new Button
            {
                Text = "Import 0 Students", Left = 440, Top = 8, Width = 160, Height = 30,
                BackColor = Theme.Success, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Enabled = false,
            };
            _btnImport.Click += OnImport;
            _btnCancel = new Button
            {
                Text = "Cancel", DialogResult = DialogResult.Cancel, Left = 608, Top = 8, Width = 90, Height = 30,
                BackColor = Color.FromArgb(140, 140, 150), ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
            };
            bottom.Controls.AddRange(new Control[] { _btnImport, _btnCancel });
            CancelButton = _btnCancel;

            _grid = new DataGridView
            {
                Dock = DockStyle.Fill, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                RowHeadersVisible = false, ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Theme.Primary, ForeColor = Color.White,
                    Font = new Font(Theme.FontFamily, 8.5f, FontStyle.Bold),
                },
                DefaultCellStyle = new DataGridViewCellStyle { Font = new Font(Theme.FontFamily, 9f) },
                RowTemplate = { Height = 22 },
            };
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Row",         Name = "row",       FillWeight = 0.5f });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Candidate #", Name = "candidate", FillWeight = 1.2f });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Surname",     Name = "surname",   FillWeight = 1f });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "First Name",  Name = "first",     FillWeight = 1f });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "CIS Ref",     Name = "cis",       FillWeight = 1f });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status",      Name = "status",    FillWeight = 1.6f });

            Controls.Add(_grid);
            Controls.Add(_lblError);
            Controls.Add(_lblSummary);
            Controls.Add(hint);
            Controls.Add(top);
            Controls.Add(bottom);
        }

        private void OnBrowse(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog { Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*" };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            _txtPath.Text = dlg.FileName;
            _lblError.Text = "";
            try
            {
                LoadAndPreview(dlg.FileName);
            }
            catch (Exception ex)
            {
                _lblError.Text = ex.Message;
            }
        }

        private void LoadAndPreview(string path)
        {
            string text = File.ReadAllText(path, System.Text.Encoding.UTF8);
            var rows = CsvParser.Parse(text).Where(r => r.Length > 0 && r.Any(c => !string.IsNullOrWhiteSpace(c))).ToList();

            _grid.Rows.Clear();
            _validRows.Clear();

            if (rows.Count < 2)
            {
                _lblSummary.Text = "No data rows found in this file.";
                _btnImport.Text = "Import 0 Students";
                _btnImport.Enabled = false;
                return;
            }

            string[] header = rows[0];
            int candIdx  = FindColumn(header, h => h.Contains("candidate"));
            int surIdx   = FindColumn(header, h => h.Contains("surname") || h == "lastname" || h == "familyname");
            int firstIdx = FindColumn(header, h => h.Contains("firstname") || h.Contains("forename"));
            int cisIdx   = FindColumn(header, h => h.Contains("cis"));

            if (candIdx < 0 || surIdx < 0 || firstIdx < 0)
            {
                _lblError.Text = "Could not find Candidate #, Surname and Firstname columns in the header row.";
                _lblSummary.Text = "";
                _btnImport.Text = "Import 0 Students";
                _btnImport.Enabled = false;
                return;
            }

            int excluded = 0;
            for (int r = 1; r < rows.Count; r++)
            {
                var cells = rows[r];
                string Get(int idx) => idx >= 0 && idx < cells.Length ? cells[idx].Trim() : "";

                string candidate = Get(candIdx);
                string surname    = Get(surIdx);
                string first      = Get(firstIdx);
                string cis        = cisIdx >= 0 ? Get(cisIdx) : "";

                string status;
                if (string.IsNullOrEmpty(candidate) || string.IsNullOrEmpty(surname) || string.IsNullOrEmpty(first))
                {
                    status = "Excluded — missing candidate #, surname or first name";
                    excluded++;
                }
                else if (candidate.Length > 30)
                {
                    status = "Excluded — candidate # exceeds 30 characters";
                    excluded++;
                }
                else
                {
                    status = "OK";
                    _validRows.Add(new StudentImportRow
                    {
                        CandidateNumber = candidate,
                        CisRef          = string.IsNullOrEmpty(cis) ? null : cis,
                        Surname         = surname,
                        FirstName       = first,
                    });
                }

                int idx = _grid.Rows.Add(r, candidate, surname, first, cis, status);
                if (status != "OK") _grid.Rows[idx].DefaultCellStyle.ForeColor = Theme.Danger;
            }

            _lblSummary.Text = $"{rows.Count - 1} rows found — {_validRows.Count} valid, {excluded} excluded.";
            _btnImport.Text = $"Import {_validRows.Count} Students";
            _btnImport.Enabled = _validRows.Count > 0;
        }

        private static int FindColumn(string[] header, Func<string, bool> match)
        {
            for (int i = 0; i < header.Length; i++)
                if (match(Normalize(header[i]))) return i;
            return -1;
        }

        private static string Normalize(string s) =>
            new string((s ?? "").ToLowerInvariant().Where(char.IsLetter).ToArray());

        private async void OnImport(object sender, EventArgs e)
        {
            _lblError.Text = "";
            _btnImport.Enabled = false;
            try
            {
                var result = await ApiService.Instance.PostAsync<SingleResponse<StudentImportResultDto>>(
                    "/students/import.php", new { students = _validRows });

                var msg = $"Imported {result?.Data?.Imported ?? 0} new students, updated {result?.Data?.Updated ?? 0} existing.";
                if (result?.Data?.Errors?.Count > 0)
                    msg += $"\n\n{result.Data.Errors.Count} row(s) rejected by the server:\n" +
                           string.Join("\n", result.Data.Errors.Select(err => $"Row {err.Row}: {err.Message}"));

                MessageBox.Show(this, msg, "Import Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                _lblError.Text = ex.Message;
                _btnImport.Enabled = true;
            }
        }
    }
}
