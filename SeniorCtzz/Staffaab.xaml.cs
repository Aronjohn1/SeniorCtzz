using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using static SeniorCtzz.Models;

namespace SeniorCtzz
{
    public partial class Staffaab : Window
    {
        private string _activeTab = "AICS";
        private string _quarterlyFilter = "Pending";
        private List<SeniorSearchItem> _seniorList = new();
        private string _selectedSeniorIdAAB = "";
        private string _selectedSeniorIdBereaved = "";
        private ObservableCollection<QuarterlyItem> _allQuarterlyData = new();

        public Staffaab()
        {
            InitializeComponent();
            SetUserInfo();
        }

        private void SetUserInfo()
        {
            TxtUserName.Text = AppSession.FullName;
            TxtAvatar.Text = AppSession.GetInitials();
        }

   
        private void BtnTabAICS_Click(object sender, RoutedEventArgs e) => SetActiveTab("AICS");
        private void BtnTabAPR_Click(object sender, RoutedEventArgs e) => SetActiveTab("APR");
        private void BtnTabBereaved_Click(object sender, RoutedEventArgs e) => SetActiveTab("BEREAVED");
        private void BtnTabQuarterly_Click(object sender, RoutedEventArgs e) => SetActiveTab("QUARTERLY");

        private void SetActiveTab(string tab)
        {
            _activeTab = tab;
            BtnTabAICS.Style = (Style)FindResource("TabButtonStyle");
            BtnTabAPR.Style = (Style)FindResource("TabButtonStyle");
            BtnTabBereaved.Style = (Style)FindResource("TabButtonStyle");
            BtnTabQuarterly.Style = (Style)FindResource("TabButtonStyle");

            PanelAICS_APR.Visibility = Visibility.Collapsed;
            PanelBereaved.Visibility = Visibility.Collapsed;
            PanelQuarterly.Visibility = Visibility.Collapsed;

            switch (tab)
            {
                case "AICS":
                    BtnTabAICS.Style = (Style)FindResource("TabButtonActiveStyle");
                    PanelAICS_APR.Visibility = Visibility.Visible;
                    GridAICSFields.Visibility = Visibility.Visible;
                    GridAPRField.Visibility = Visibility.Collapsed;
                
                    TxtPanelLabel.Text = "AICS — Assistance to Indigent Senior Citizens";
                    ClearAABForm();
                    break;
                case "APR":
                    BtnTabAPR.Style = (Style)FindResource("TabButtonActiveStyle");
                    PanelAICS_APR.Visibility = Visibility.Visible;
                    GridAICSFields.Visibility = Visibility.Collapsed;
                    GridAPRField.Visibility = Visibility.Visible;
            
                    TxtPanelLabel.Text = "APR — Assistance to Persons with Disability";
                    ClearAABForm();
                    break;
                case "BEREAVED":
                    BtnTabBereaved.Style = (Style)FindResource("TabButtonActiveStyle");
                    PanelBereaved.Visibility = Visibility.Visible;
                    ClearBereavdForm();
                    break;
                case "QUARTERLY":
                    BtnTabQuarterly.Style = (Style)FindResource("TabButtonActiveStyle");
                    PanelQuarterly.Visibility = Visibility.Visible;
                    LoadQuarterlyData();
                    LoadAllBarangaysForQuarterly();
                    break;
            }
        }

    
        private void TxtHospitalBill_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (decimal.TryParse(TxtHospitalBill.Text.Trim(), out decimal bill))
            {
                decimal thirtyPercent = bill * 0.3m;
                Txt30Percent.Text = thirtyPercent.ToString("N2");
                TxtAmountAAB.Text = thirtyPercent.ToString("N2");
            }
            else
            {
                Txt30Percent.Text = "";
                TxtAmountAAB.Text = "";
            }
        }

        private void TxtSearchAAB_TextChanged(object sender, TextChangedEventArgs e)
        {
            string keyword = TxtSearchAAB.Text.Trim();
            if (string.IsNullOrWhiteSpace(keyword))
            {
                SearchDropdownAAB.Visibility = Visibility.Collapsed;
                return;
            }

            _seniorList = SeniorDB.Search(keyword).ToList();
            if (_seniorList.Any())
            {
                SearchResultAAB.ItemsSource = _seniorList.Select(s => s.FullName).ToList();
                SearchDropdownAAB.Visibility = Visibility.Visible;
            }
            else
            {
                SearchDropdownAAB.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnSearchAAB_Click(object sender, RoutedEventArgs e)
        {
            TxtSearchAAB_TextChanged(null!, null!);
        }

        private void SearchResultAAB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SearchResultAAB.SelectedItem == null) return;

            string selectedFullName = SearchResultAAB.SelectedItem.ToString() ?? "";
            TxtSearchAAB.Text = selectedFullName;
            SearchDropdownAAB.Visibility = Visibility.Collapsed;

            var senior = _seniorList.FirstOrDefault(s => s.FullName == selectedFullName);
            if (senior == null)
            {
                senior = _seniorList.FirstOrDefault(s =>
                    s.FullName.Equals(selectedFullName, StringComparison.OrdinalIgnoreCase));
            }

            if (senior == null) return;

            _selectedSeniorIdAAB = senior.SeniorId;
            TxtFullNameAAB.Text = senior.FullName ?? "";
            TxtBirthDateAAB.Text = DateTime.Now.ToString("MM-dd-yy");
            TxtBarangayAAB.Text = senior.Barangay ?? "";
        }

    
        private void TxtSearchBereaved_TextChanged(object sender, TextChangedEventArgs e)
        {
            string keyword = TxtSearchBereaved.Text.Trim();
            if (string.IsNullOrWhiteSpace(keyword))
            {
                SearchDropdownBereaved.Visibility = Visibility.Collapsed;
                return;
            }

            _seniorList = SeniorDB.Search(keyword).ToList();
            if (_seniorList.Any())
            {
                SearchResultBereaved.ItemsSource = _seniorList.Select(s => s.FullName).ToList();
                SearchDropdownBereaved.Visibility = Visibility.Visible;
            }
            else
            {
                SearchDropdownBereaved.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnSearchBereaved_Click(object sender, RoutedEventArgs e)
        {
            TxtSearchBereaved_TextChanged(null!, null!);
        }

        private void SearchResultBereaved_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SearchResultBereaved.SelectedItem == null) return;

            string selected = SearchResultBereaved.SelectedItem.ToString() ?? "";
            string fullName = selected.Contains('[')
                ? selected.Substring(0, selected.IndexOf('[')).Trim()
                : selected;

            TxtSearchBereaved.Text = fullName;
            SearchDropdownBereaved.Visibility = Visibility.Collapsed;

            var senior = _seniorList.FirstOrDefault(s => s.FullName == fullName);
            if (senior == null) return;

            _selectedSeniorIdBereaved = senior.SeniorId;
            TxtFullNameBereaved.Text = senior.FullName;
            TxtBirthDateBereaved.Text = senior.BirthDate;
            TxtBarangayBereaved.Text = senior.Barangay;
        }

  
        private void BtnSaveAAB_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_selectedSeniorIdAAB))
            { ShowErrorAAB("Please search and select a senior citizen first."); return; }

            decimal amount = _activeTab == "AICS"
                ? (decimal.TryParse(TxtAmountAAB.Text, out decimal a) ? a : 0)
                : 5000m;

            if (amount <= 0) { ShowErrorAAB("Invalid amount."); return; }

            string restrictionMsg = _activeTab == "AICS"
                ? PensionDB.CheckAICSRestriction(_selectedSeniorIdAAB)
                : PensionDB.CheckAPRRestriction(_selectedSeniorIdAAB);

            if (!string.IsNullOrEmpty(restrictionMsg))
            {
                MessageBox.Show(restrictionMsg, "Claim Restricted", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string encoderId = AppSession.UserId;
            bool saved = _activeTab == "AICS"
                ? PensionDB.SaveAICS(_selectedSeniorIdAAB, amount, encoderId)
                : PensionDB.SaveAPR(_selectedSeniorIdAAB, amount, "", encoderId);

            if (saved)
            {
                MessageBox.Show($"{_activeTab} record saved for {TxtFullNameAAB.Text}.",
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearAABForm();
                HideErrorAAB();
            }
        }

    
        private void BtnSaveBereaved_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_selectedSeniorIdBereaved))
            { ShowErrorBereaved("Please search and select a senior citizen."); return; }
            if (string.IsNullOrWhiteSpace(TxtRecipientName.Text))
            { ShowErrorBereaved("Recipient Name is required."); return; }
            if (string.IsNullOrWhiteSpace(TxtRelationship.Text))
            { ShowErrorBereaved("Relationship is required."); return; }

            decimal amount = 2000m;
            string encoderId = AppSession.UserId;

            bool saved = PensionDB.SaveBereaved(
                _selectedSeniorIdBereaved, amount,
                TxtRecipientName.Text.Trim(),
                TxtRelationship.Text.Trim(),
                TxtRemarksBereaved.Text.Trim(), encoderId,
                DateTime.Now);

            if (saved)
            {
                MessageBox.Show(
                    $"Bereaved record saved. {TxtFullNameBereaved.Text} is now marked as deceased/inactive.\n" +
                    "They will no longer be included in quarterly pension releases.",
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearBereavdForm();
                HideErrorBereaved();
            }
        }

     
        private string[] _allBarangays = new string[]
        {
            "Agay-ayan", "Alagatan", "Anakan", "Bagubad", "Bakidbakid",
            "Bal-ason", "Bantaawan", "Binakalan", "Capitulangan", "Daan-Lungsod",
            "Dinawehan", "Eureka", "Hindangon", "Kalagonoy", "Kalipay",
            "Kamanikan", "Kianlagan", "Kibuging", "Kipuntos", "Lawaan",
            "Lawit", "Libertad", "Libon", "Lunao", "Lunotan",
            "Malibud", "Malinao", "Maribucao", "Mimbuntong", "Mimbalagon",
            "Mimbunga", "Minsapinit", "Murallon", "Odiongan", "Pangasihan",
            "Pigsaluhan", "Punong", "Ricoro", "Samay", "Sangalan",
            "San Jose", "San Juan", "San Luis", "San Miguel", "Santiago",
            "Tagpako", "Talisay", "Talon", "Tinabalan", "Tinulongan",
            "Barangay 1", "Barangay 2", "Barangay 3", "Barangay 4", "Barangay 5",
            "Barangay 6", "Barangay 7", "Barangay 8", "Barangay 9", "Barangay 10",
            "Barangay 11", "Barangay 12", "Barangay 13", "Barangay 14", "Barangay 15",
            "Barangay 16", "Barangay 17", "Barangay 18", "Barangay 18-A", "Barangay 19",
            "Barangay 20", "Barangay 21", "Barangay 22", "Barangay 22-A", "Barangay 23",
            "Barangay 24", "Barangay 24-A", "Barangay 25", "Barangay 26"
        };

        private void BtnPending_Click(object sender, RoutedEventArgs e) => FilterQuarterly("Pending");
        private void BtnUpcoming_Click(object sender, RoutedEventArgs e) => FilterQuarterly("Upcoming");
        private void BtnRelease_Click(object sender, RoutedEventArgs e) => FilterQuarterly("Released");

        private void FilterQuarterly(string status)
        {
            _quarterlyFilter = status;
            BtnPending.Style = (Style)FindResource("TabButtonStyle");
            BtnUpcoming.Style = (Style)FindResource("TabButtonStyle");
            BtnRelease.Style = (Style)FindResource("TabButtonStyle");
            switch (status)
            {
                case "Pending":
                    BtnPending.Style = (Style)FindResource("TabButtonActiveStyle");
                    break;
                case "Upcoming":
                    BtnUpcoming.Style = (Style)FindResource("TabButtonActiveStyle");
                    break;
                case "Released":
                    BtnRelease.Style = (Style)FindResource("TabButtonActiveStyle");
                    break;
            }
            LoadQuarterlyData();
            LoadAllBarangaysForQuarterly();
        }

 
        private void LoadAllBarangaysForQuarterly()
        {
            string selected = (CmbQuarterlyBarangay.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "All Barangays";

            CmbQuarterlyBarangay.Items.Clear();
            CmbQuarterlyBarangay.Items.Add(new ComboBoxItem { Content = "All Barangays" });

            foreach (var brgy in _allBarangays)
                CmbQuarterlyBarangay.Items.Add(new ComboBoxItem { Content = brgy });

          
            foreach (ComboBoxItem item in CmbQuarterlyBarangay.Items)
            {
                if (item.Content?.ToString() == selected)
                {
                    CmbQuarterlyBarangay.SelectedItem = item;
                    break;
                }
            }
        }

        private void LoadQuarterlyData()
        {
            _allQuarterlyData = PensionDB.GetAllActiveSeniorsForQuarterly(_quarterlyFilter);

            DateTime today = DateTime.Today;

            foreach (var item in _allQuarterlyData)
            {
         
                DateTime registrationDate;
                if (!DateTime.TryParse(item.Date, out registrationDate))
                    registrationDate = today;

                DateTime releaseDate = registrationDate.AddMonths(3);

        
                item.ReleaseDate = releaseDate.ToString("MM/dd/yyyy");

          
                bool isReleased = today >= releaseDate;

        
                DateTime firstOfReleaseMonth = new DateTime(releaseDate.Year, releaseDate.Month, 1);
                DateTime firstOfUpcomingMonth = firstOfReleaseMonth.AddMonths(-1);
                DateTime lastOfUpcomingMonth = firstOfReleaseMonth.AddDays(-1); 

                bool isUpcoming = !isReleased
                                  && today >= firstOfUpcomingMonth
                                  && today <= lastOfUpcomingMonth;

                if (isReleased)
                {
                    item.Status = "Released";
                    item.StatusBg = "#DCFCE7";
                    item.StatusBorder = "#22C55E";
                    item.StatusFg = "#166534";
                }
                else if (isUpcoming)
                {
                    item.Status = $"Upcoming ({releaseDate:MMM dd})";
                    item.StatusBg = "#DBEAFE";
                    item.StatusBorder = "#3B82F6";
                    item.StatusFg = "#1E3A8A";
                }
                else
                {
                    item.Status = "Pending";
                    item.StatusBg = "#FEF9C3";
                    item.StatusBorder = "#EAB308";
                    item.StatusFg = "#854D0E";
                }
            }

            ApplyQuarterlyFilter();
        }

        private void ApplyQuarterlyFilter()
        {
            var filtered = _allQuarterlyData;

            string selectedBarangay = (CmbQuarterlyBarangay.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "All Barangays";
            if (selectedBarangay != "All Barangays")
            {
                filtered = new ObservableCollection<QuarterlyItem>(
                    filtered.Where(q => q.Barangay == selectedBarangay));
            }

            string kw = TxtSearchQuarterly.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(kw))
            {
                filtered = new ObservableCollection<QuarterlyItem>(
                    filtered.Where(q => (q.FullName?.ToLower().Contains(kw) ?? false) ||
                                        (q.OscaId?.ToLower().Contains(kw) ?? false)));
            }

 
            filtered = new ObservableCollection<QuarterlyItem>(
                filtered.Where(q =>
                {
                    if (_quarterlyFilter == "Released")
                        return q.Status == "Released";
                    if (_quarterlyFilter == "Upcoming")
                        return q.Status.StartsWith("Upcoming");
        
                    return q.Status == "Pending";
                }));

            DgQuarterly.ItemsSource = filtered;
        }

        private void CmbQuarterlyBarangay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_allQuarterlyData != null && _allQuarterlyData.Count > 0)
                ApplyQuarterlyFilter();
        }

        private void TxtSearchQuarterly_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_allQuarterlyData != null && _allQuarterlyData.Count > 0)
                ApplyQuarterlyFilter();
        }

        private void BtnSearchQuarterly_Click(object sender, RoutedEventArgs e)
        {
      
        }

   
        private void ClearAABForm()
        {
            TxtSearchAAB.Text = "";
            TxtFullNameAAB.Text = "";
            TxtBirthDateAAB.Text = "";
            TxtBarangayAAB.Text = "";
            TxtHospitalBill.Text = "";
            Txt30Percent.Text = "";
            TxtAmountAAB.Text = "";
         
            _selectedSeniorIdAAB = "";
            SearchDropdownAAB.Visibility = Visibility.Collapsed;
            HideErrorAAB();
        }

        private void ClearBereavdForm()
        {
            TxtSearchBereaved.Text = "";
            TxtFullNameBereaved.Text = "";
            TxtBirthDateBereaved.Text = "";
            TxtBarangayBereaved.Text = "";
            TxtRecipientName.Text = "";
            TxtRelationship.Text = "";
            TxtRemarksBereaved.Text = "";
            _selectedSeniorIdBereaved = "";
            SearchDropdownBereaved.Visibility = Visibility.Collapsed;
            HideErrorBereaved();
        }

        private void ShowErrorAAB(string msg)
        {
            TxtErrorAAB.Text = msg;
            TxtErrorAAB.Visibility = Visibility.Visible;
        }

        private void HideErrorAAB()
        {
            TxtErrorAAB.Text = "";
            TxtErrorAAB.Visibility = Visibility.Collapsed;
        }

        private void ShowErrorBereaved(string msg)
        {
            TxtErrorBereaved.Text = msg;
            TxtErrorBereaved.Visibility = Visibility.Visible;
        }

        private void HideErrorBereaved()
        {
            TxtErrorBereaved.Text = "";
            TxtErrorBereaved.Visibility = Visibility.Collapsed;
        }

     
        private void BtnDashboard_Click(object sender, RoutedEventArgs e)
        { new Staffdashboard().Show(); this.Close(); }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        { new Staffregister().Show(); this.Close(); }

        private void BtnAAB_Click(object sender, RoutedEventArgs e) { }

        private void BtnPensionList_Click(object sender, RoutedEventArgs e)
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
