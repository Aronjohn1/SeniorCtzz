using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using static SeniorCtzz.Models;

namespace SeniorCtzz
{
    public partial class Admindashboard : Window
    {
        private ObservableCollection<SeniorSearchItem> _seniorSearchCache
            = new ObservableCollection<SeniorSearchItem>();

        public Admindashboard()
        {
            InitializeComponent();
            SetUserInfo();
            SetCurrentDate();
            LoadStats();
            LoadAllSeniorsSummary();
        }

        // ═══════════════════════════════════════════
        // INIT
        // ═══════════════════════════════════════════

        private void SetUserInfo()
        {
            TxtUserName.Text = AppSession.FullName;
            TxtAvatar.Text = AppSession.GetInitials();
            TxtRole.Text = AppSession.Role;
        }

        private void SetCurrentDate()
        {
            // Shows e.g. "May 18, 2026" in the header date badge
            TxtCurrentDate.Text = DateTime.Now.ToString("MMMM dd, yyyy");
        }

        // ═══════════════════════════════════════════
        // STATS
        // ═══════════════════════════════════════════

        private void LoadStats()
        {
            // ── Row 1: Key metrics ──────────────────────────────────
            int total = SeniorDB.GetTotalCount();
            int active = SeniorDB.GetActiveCount();
            int inactive = total - active;

            txtTotalSC.Text = total.ToString();
            txtTotalActive.Text = active.ToString();
            txtTotalInactive.Text = inactive.ToString();

            // ── Row 2: Pension breakdown ────────────────────────────
            txtTotalAICS.Text = PensionDB.GetCount("AICS").ToString();
            txtTotalAPR.Text = PensionDB.GetCount("APR").ToString();
            txtTotalQuarterly.Text = PensionDB.GetCount("QUARTERLY").ToString();
            txtTotalBereaved.Text = PensionDB.GetCount("BEREAVED").ToString();

            // ── Row 3: Admin-only metrics ───────────────────────────
            txtTotalStaff.Text = StaffDB.GetActiveStaffCount().ToString();
            txtEntriesMonth.Text = SeniorDB.GetEntriesThisMonth().ToString();
            txtReleasesMonth.Text = PensionDB.GetReleasesThisMonth().ToString();
        }

        // ═══════════════════════════════════════════
        // TABLE
        // ═══════════════════════════════════════════

        private void LoadAllSeniorsSummary()
        {
            var data = PensionDB.GetDashboardRecent();
            dgRecent.ItemsSource = data;
            UpdateRecordCount(data?.Count ?? 0);
        }

        /// <summary>Updates the record-count pill beside the table title.</summary>
        private void UpdateRecordCount(int count)
        {
            TxtRecordCount.Text = count == 1 ? "1 record" : $"{count} records";
        }

        // ═══════════════════════════════════════════
        // SEARCH
        // ═══════════════════════════════════════════

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string query = TxtSearch.Text.Trim();

            if (string.IsNullOrEmpty(query))
            {
                SearchDropdown.Visibility = Visibility.Collapsed;
                LoadAllSeniorsSummary(); // reset table when cleared
                return;
            }

            _seniorSearchCache = SeniorDB.Search(query);

            if (_seniorSearchCache.Any())
            {
                SearchResultList.ItemsSource =
                    _seniorSearchCache.Select(s => $"{s.FullName} ({s.SeniorId})").ToList();
                SearchDropdown.Visibility = Visibility.Visible;
            }
            else
            {
                // ── No match: hide dropdown AND clear the table ──
                SearchDropdown.Visibility = Visibility.Collapsed;
                dgRecent.ItemsSource = null;
                UpdateRecordCount(0);
            }
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            string query = TxtSearch.Text.Trim();

            if (string.IsNullOrEmpty(query))
            {
                LoadAllSeniorsSummary();
                return;
            }

            _seniorSearchCache = SeniorDB.Search(query);

            if (_seniorSearchCache.Any())
            {
                // If only one result, load directly
                if (_seniorSearchCache.Count == 1)
                {
                    var senior = _seniorSearchCache.First();
                    var filtered = PensionDB.GetDashboardRecent(senior.SeniorId);
                    dgRecent.ItemsSource = filtered;
                    UpdateRecordCount(filtered?.Count ?? 0);
                    SearchDropdown.Visibility = Visibility.Collapsed;
                }
                else
                {
                    SearchResultList.ItemsSource =
                        _seniorSearchCache.Select(s => $"{s.FullName} ({s.SeniorId})").ToList();
                    SearchDropdown.Visibility = Visibility.Visible;
                }
            }
            else
            {
                // No match: clear table
                SearchDropdown.Visibility = Visibility.Collapsed;
                dgRecent.ItemsSource = null;
                UpdateRecordCount(0);
            }
        }

        private void SearchResultList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SearchResultList.SelectedItem == null) return;

            string selectedEntry = SearchResultList.SelectedItem.ToString();
            SearchDropdown.Visibility = Visibility.Collapsed;

            // Match by FullName since display is "FullName (OscaId)"
            var senior = _seniorSearchCache.FirstOrDefault(s =>
                selectedEntry.StartsWith(s.FullName, StringComparison.OrdinalIgnoreCase));

            if (senior != null)
            {
                TxtSearch.Text = senior.FullName;
                var filtered = PensionDB.GetDashboardRecent(senior.SeniorId);
                dgRecent.ItemsSource = filtered;
                UpdateRecordCount(filtered?.Count ?? 0);
            }
        }

        // ═══════════════════════════════════════════
        // EXPORT
        // ═══════════════════════════════════════════

        private void BtnExportReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get current table data (whatever is bound — filtered or all)
                var data = dgRecent.ItemsSource as System.Collections.IList;
                if (data == null || data.Count == 0)
                {
                    MessageBox.Show("No records to export.", "Export Report",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Save dialog
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "Export Pension Summary Report",
                    Filter = "CSV File (*.csv)|*.csv",
                    FileName = $"PensionSummary_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (dialog.ShowDialog() != true) return;

                // Build CSV
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("DATE (REG.),LAST NAME,FIRST NAME,BARANGAY,AICS,APR,QUARTERLY,BEREAVED,TOTAL");

                foreach (var item in data)
                {
                    // Uses the same property names bound in the DataGrid
                    dynamic row = item;
                    sb.AppendLine(
                        $"{row.Date},{row.LastName},{row.FirstName},{row.Barangay}," +
                        $"{row.AICSCount},{row.APRCount},{row.QuarterlyCount}," +
                        $"{row.BereavedCount},{row.TotalPensions}");
                }

                System.IO.File.WriteAllText(dialog.FileName, sb.ToString(),
                    System.Text.Encoding.UTF8);

                MessageBox.Show($"Report exported successfully.\n\n{dialog.FileName}",
                    "Export Report", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ═══════════════════════════════════════════
        // NAVIGATION
        // ═══════════════════════════════════════════

        private void BtnDashboard_Click(object sender, RoutedEventArgs e)
        {
            SetCurrentDate();
            LoadStats();
            LoadAllSeniorsSummary();
        }

        private void BtnManageStaff_Click(object sender, RoutedEventArgs e)
        { new Adminmanagestaff().Show(); this.Close(); }

        private void BtnPensionList_Click(object sender, RoutedEventArgs e)
        { new Adminpensionlist().Show(); this.Close(); }

        private void BtnReports_Click(object sender, RoutedEventArgs e)
        { new Adminreports().Show(); this.Close(); }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to logout?", "Logout",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                AppSession.UserId = "";
                AppSession.FullName = "";
                AppSession.Role = "";
                new MainWindow().Show();
                this.Close();
            }
        }
    }
}