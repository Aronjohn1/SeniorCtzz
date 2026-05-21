using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SeniorCtzz
{
    public partial class Staffdashboard : Window
    {
        private ObservableCollection<Models.SeniorSearchItem> _seniorSearchCache
            = new ObservableCollection<Models.SeniorSearchItem>();

        public Staffdashboard()
        {
            InitializeComponent();
            SetUserInfo();
            LoadDashboardData();
            LoadRecentTransactions();
        }

        private void SetUserInfo()
        {
            TxtUserName.Text = AppSession.FullName;
            TxtAvatar.Text = AppSession.GetInitials();
        }

        private void LoadDashboardData()
        {
            TxtTotalSeniors.Text = SeniorDB.GetActiveCount().ToString();
            TxtTotalInactive.Text = SeniorDB.GetInactiveCount().ToString();
            TxtTotalAICS.Text = PensionDB.GetCount("AICS").ToString();
            TxtTotalAPR.Text = PensionDB.GetCount("APR").ToString();
            TxtTotalBereaved.Text = PensionDB.GetCount("BEREAVED").ToString();
            TxtTotalQuarterly.Text = PensionDB.GetCount("QUARTERLY").ToString();
        }

        // LOAD RECENT WITH PENSION COUNTS PER SENIOR
        private void LoadRecentTransactions()
        {
            var data = PensionDB.GetDashboardRecent();
            DgRecent.ItemsSource = data;
            DgRecent.Items.Refresh();
        }

        // SEARCH — Name only, no barangay in dropdown
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string query = TxtSearch.Text.Trim();

            // FIXED: When search box is cleared, show all data again
            if (string.IsNullOrEmpty(query))
            {
                SearchDropdown.Visibility = Visibility.Collapsed;
                LoadRecentTransactions(); // Reload all data
                return;
            }

            _seniorSearchCache = SeniorDB.Search(query);

            if (_seniorSearchCache.Any())
            {
                SearchResultList.ItemsSource =
                    _seniorSearchCache.Select(s => s.FullName).ToList();
                SearchDropdown.Visibility = Visibility.Visible;
            }
            else
            {
                SearchDropdown.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            TxtSearch_TextChanged(null!, null!);
        }

        private void SearchResultList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SearchResultList.SelectedItem == null) return;

            string selectedName = SearchResultList.SelectedItem.ToString() ?? "";
            TxtSearch.Text = selectedName;
            SearchDropdown.Visibility = Visibility.Collapsed;

            // Filter recent table by selected senior
            var senior = _seniorSearchCache.FirstOrDefault(s => s.FullName == selectedName);
            if (senior != null)
            {
                var filtered = PensionDB.GetDashboardRecent(senior.SeniorId);
                DgRecent.ItemsSource = filtered;
                DgRecent.Items.Refresh();
            }
        }

        private void BtnDashboard_Click(object sender, RoutedEventArgs e)
        {
            LoadDashboardData();
            LoadRecentTransactions();
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        { new Staffregister().Show(); this.Close(); }

        private void BtnAAB_Click(object sender, RoutedEventArgs e)
        { new Staffaab().Show(); this.Close(); }

        private void BtnPensionType_Click(object sender, RoutedEventArgs e)
        { new Staffpensionlist().Show(); this.Close(); }

        private void BtnReports_Click(object sender, RoutedEventArgs e)
        { new Staffreports().Show(); this.Close(); }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to logout?", "Logout",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
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