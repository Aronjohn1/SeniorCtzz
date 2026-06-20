using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SeniorCtzz
{
    public class QuarterlyPensionListItem
    {
        public string OscaId { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Date { get; set; } = "";
        public string Barangay { get; set; } = "";
        public string Amount { get; set; } = "2,250";
        public string ReleaseDate { get; set; } = "";
        public string Status { get; set; } = "";

        public string StatusBg { get; set; } = "#FEF9C3";
        public string StatusBorder { get; set; } = "#EAB308";
        public string StatusFg { get; set; } = "#854D0E";
    }

    public partial class Staffpensionlist : Window
    {
        private string _activeTab = "Barangay";


        private string _barangayFilterBarangay = "ALL BARANGAYS";
        private string _barangayFilterQuarterly = "ALL BARANGAYS";
        private string _barangayFilterAICS = "ALL BARANGAYS";
        private string _barangayFilterAPR = "ALL BARANGAYS";
        private string _barangayFilterBereaved = "ALL BARANGAYS";

  
        private string _asOfFilterBarangay = DateTime.Now.ToString("MMM yyyy");
        private string _asOfFilterQuarterly = DateTime.Now.ToString("MMM yyyy");
        private string _asOfFilterAICS = DateTime.Now.ToString("MMM yyyy");
        private string _asOfFilterAPR = DateTime.Now.ToString("MMM yyyy");
        private string _asOfFilterBereaved = DateTime.Now.ToString("MMM yyyy");

     
        private const int START_YEAR = 2020;

        public Staffpensionlist()
        {
            InitializeComponent();
            Loaded += Staffpensionlist_Loaded;
        }

        private void Staffpensionlist_Loaded(object sender, RoutedEventArgs e)
        {
            SetUserInfo();
            LoadAllBarangays();
            LoadAsOfMonths();
            SetActiveTab("Barangay");
        }

        private void SetUserInfo()
        {
            TxtUserName.Text = AppSession.FullName ?? "Jonil P. Sanchez";
            TxtAvatar.Text = AppSession.GetInitials() ?? "JS";
        }

    
        private void LoadAllBarangays()
        {
            string[] barangays = {
                "ALL BARANGAYS",
                "Agay-ayan","Alagatan","Anakan","Bagubad","Bakidbakid","Bal-ason","Bantaawan",
                "Binakalan","Capitulangan","Daan-Lungsod","Dinawehan","Eureka","Hindangon",
                "Kalagonoy","Kalipay","Kamanikan","Kianlagan","Kibuging","Kipuntos","Lawaan",
                "Lawit","Libertad","Libon","Lunao","Lunotan","Malibud","Malinao","Maribucao",
                "Mimbuntong","Mimbalagon","Mimbunga","Minsapinit","Murallon","Odiongan",
                "Pangasihan","Pigsaluhan","Punong","Ricoro","Samay","Sangalan","San Jose",
                "San Juan","San Luis","San Miguel","Santiago","Tagpako","Talisay","Talon",
                "Tinabalan","Tinulongan",
                "Barangay 1","Barangay 2","Barangay 3","Barangay 4","Barangay 5",
                "Barangay 6","Barangay 7","Barangay 8","Barangay 9","Barangay 10",
                "Barangay 11","Barangay 12","Barangay 13","Barangay 14","Barangay 15",
                "Barangay 16","Barangay 17","Barangay 18","Barangay 18-A","Barangay 19",
                "Barangay 20","Barangay 21","Barangay 22","Barangay 22-A","Barangay 23",
                "Barangay 24","Barangay 24-A","Barangay 25","Barangay 26"
            };

            CmbBarangayFilter.Items.Clear();
            foreach (var b in barangays)
                CmbBarangayFilter.Items.Add(new ComboBoxItem { Content = b });
            CmbBarangayFilter.SelectedIndex = 0;
        }

  
        private void LoadAsOfMonths()
        {
 
            CmbAsOf.SelectionChanged -= AsOf_SelectionChanged;

            CmbAsOf.Items.Clear();
    

            int currentYear = DateTime.Now.Year;
            int currentMonth = DateTime.Now.Month;

            for (int year = START_YEAR; year <= currentYear; year++)
            {
           
                int maxMonth = (year == currentYear) ? currentMonth : 12;

                for (int m = 1; m <= maxMonth; m++)
                {
              
                    string label = new DateTime(year, m, 1).ToString("MMM yyyy");
                    CmbAsOf.Items.Add(new ComboBoxItem { Content = label });
                }
            }

    
            string currentLabel = DateTime.Now.ToString("MMM yyyy");
            bool found = false;
            foreach (ComboBoxItem item in CmbAsOf.Items)
            {
                if (item.Content?.ToString() == currentLabel)
                {
                    CmbAsOf.SelectedItem = item;
                    found = true;
                    break;
                }
            }
    
            if (!found && CmbAsOf.Items.Count > 0)
                CmbAsOf.SelectedIndex = CmbAsOf.Items.Count - 1;

            CmbAsOf.SelectionChanged += AsOf_SelectionChanged;
        }

  
        private string GetSavedBarangay(string tab)
        {
            switch (tab)
            {
                case "Barangay": return _barangayFilterBarangay;
                case "Quarterly": return _barangayFilterQuarterly;
                case "AICS": return _barangayFilterAICS;
                case "APR": return _barangayFilterAPR;
                case "BEREAVED": return _barangayFilterBereaved;
                default: return "ALL BARANGAYS";
            }
        }

        private void SaveCurrentBarangay(string brgy)
        {
            switch (_activeTab)
            {
                case "Barangay": _barangayFilterBarangay = brgy; break;
                case "Quarterly": _barangayFilterQuarterly = brgy; break;
                case "AICS": _barangayFilterAICS = brgy; break;
                case "APR": _barangayFilterAPR = brgy; break;
                case "BEREAVED": _barangayFilterBereaved = brgy; break;
            }
        }

        private void SelectBarangay(string brgyName)
        {
            foreach (ComboBoxItem item in CmbBarangayFilter.Items)
            {
                if (item.Content?.ToString() == brgyName)
                {
                    CmbBarangayFilter.SelectedItem = item;
                    return;
                }
            }
            CmbBarangayFilter.SelectedIndex = 0;
        }

  
        private string GetSavedAsOf(string tab)
        {
            string currentMonth = DateTime.Now.ToString("MMM yyyy");
            switch (tab)
            {
                case "Barangay": return _asOfFilterBarangay;
                case "Quarterly": return _asOfFilterQuarterly;
                case "AICS": return _asOfFilterAICS;
                case "APR": return _asOfFilterAPR;
                case "BEREAVED": return _asOfFilterBereaved;
                default: return currentMonth;
            }
        }

        private void SaveCurrentAsOf(string asOf)
        {
            switch (_activeTab)
            {
                case "Barangay": _asOfFilterBarangay = asOf; break;
                case "Quarterly": _asOfFilterQuarterly = asOf; break;
                case "AICS": _asOfFilterAICS = asOf; break;
                case "APR": _asOfFilterAPR = asOf; break;
                case "BEREAVED": _asOfFilterBereaved = asOf; break;
            }
        }

        private void SelectAsOf(string asOfValue)
        {
            foreach (ComboBoxItem item in CmbAsOf.Items)
            {
                if (item.Content?.ToString() == asOfValue)
                {
                    CmbAsOf.SelectedItem = item;
                    return;
                }
            }
            CmbAsOf.SelectedIndex = 0;  
        }

 
        private string GetAsOfFilter()
            => (CmbAsOf?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "ALL MONTHS";

        // ════════════════════════════════════════════════════════════

        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            string selected = (CmbBarangayFilter?.SelectedItem as ComboBoxItem)?.Content?.ToString()
                              ?? "ALL BARANGAYS";
            SaveCurrentBarangay(selected);
            RefreshActiveTab();
        }

  
        private void AsOf_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            string selected = GetAsOfFilter();
            SaveCurrentAsOf(selected);   
            RefreshActiveTab();
        }

        private void RefreshActiveTab()
        {
            switch (_activeTab)
            {
                case "Barangay": LoadBarangayList(); break;
                case "Quarterly": LoadQuarterlyList(); break;
                case "AICS": LoadAICSList(); break;
                case "APR": LoadAPRList(); break;
                case "BEREAVED": LoadBereavedList(); break;
            }
        }

  
        private void BtnTabBarangay_Click(object sender, RoutedEventArgs e) => SetActiveTab("Barangay");
        private void BtnTabQuarterly_Click(object sender, RoutedEventArgs e) => SetActiveTab("Quarterly");
        private void BtnTabAICS_Click(object sender, RoutedEventArgs e) => SetActiveTab("AICS");
        private void BtnTabAPR_Click(object sender, RoutedEventArgs e) => SetActiveTab("APR");
        private void BtnTabBereaved_Click(object sender, RoutedEventArgs e) => SetActiveTab("BEREAVED");

        private void SetActiveTab(string tab)
        {
            _activeTab = tab;

   
            BtnTabBarangay.Style = (Style)FindResource("TabButtonStyle");
            BtnTabQuarterly.Style = (Style)FindResource("TabButtonStyle");
            BtnTabAICS.Style = (Style)FindResource("TabButtonStyle");
            BtnTabAPR.Style = (Style)FindResource("TabButtonStyle");
            BtnTabBereaved.Style = (Style)FindResource("TabButtonStyle");

    
            PanelBarangayList.Visibility = Visibility.Collapsed;
            PanelQuarterlyTab.Visibility = Visibility.Collapsed;
            PanelAICSTab.Visibility = Visibility.Collapsed;
            PanelAPRTab.Visibility = Visibility.Collapsed;
            PanelBereavedTab.Visibility = Visibility.Collapsed;

            LblFilterLabel.Text = tab == "Barangay"
                ? "LIST OF SENIOR CITIZEN'S BARANGAY:" : "BARANGAY:";

  
            CmbBarangayFilter.SelectionChanged -= Filter_SelectionChanged;
            SelectBarangay(GetSavedBarangay(tab));
            CmbBarangayFilter.SelectionChanged += Filter_SelectionChanged;

    
            CmbAsOf.SelectionChanged -= AsOf_SelectionChanged;
            SelectAsOf(GetSavedAsOf(tab));
            CmbAsOf.SelectionChanged += AsOf_SelectionChanged;

            try
            {
                switch (tab)
                {
                    case "Barangay":
                        BtnTabBarangay.Style = (Style)FindResource("TabButtonActiveStyle");
                        PanelBarangayList.Visibility = Visibility.Visible;
                        LoadBarangayList();
                        break;
                    case "Quarterly":
                        BtnTabQuarterly.Style = (Style)FindResource("TabButtonActiveStyle");
                        PanelQuarterlyTab.Visibility = Visibility.Visible;
                        LoadQuarterlyList();
                        break;
                    case "AICS":
                        BtnTabAICS.Style = (Style)FindResource("TabButtonActiveStyle");
                        PanelAICSTab.Visibility = Visibility.Visible;
                        LoadAICSList();
                        break;
                    case "APR":
                        BtnTabAPR.Style = (Style)FindResource("TabButtonActiveStyle");
                        PanelAPRTab.Visibility = Visibility.Visible;
                        LoadAPRList();
                        break;
                    case "BEREAVED":
                        BtnTabBereaved.Style = (Style)FindResource("TabButtonActiveStyle");
                        PanelBereavedTab.Visibility = Visibility.Visible;
                        LoadBereavedList();
                        break;
                }
            }
            catch { }
        }

  
        private (int month, int year) ParseAsOf()
        {
            string asOf = GetAsOfFilter();
            if (DateTime.TryParse("1 " + asOf, out DateTime dt))
                return (dt.Month, dt.Year);
            // Fallback to current month
            return (DateTime.Now.Month, DateTime.Now.Year);
        }

     
        private static bool TryParseRecordDate(string raw, out DateTime result)
        {
            result = DateTime.MinValue;
            if (string.IsNullOrWhiteSpace(raw)) return false;

            string[] formats = {
                "MM-dd-yy", 
                "MM-dd-yyyy", 
                "M-d-yy",    
                "M-d-yyyy",
                "MM/dd/yy",
                "MM/dd/yyyy",
                "M/d/yy",
                "M/d/yyyy",
                "yyyy-MM-dd", 
            };

            return DateTime.TryParseExact(
                raw.Trim(),
                formats,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out result);
        }

        private void LoadBarangayList()
        {
            try
            {
                string brgy = GetSelectedBarangay();
                if (string.IsNullOrEmpty(brgy)) return;

                var data = brgy == "ALL BARANGAYS"
                    ? SeniorDB.GetAllSeniors() ?? new ObservableCollection<Models.BarangayListItem>()
                    : SeniorDB.GetByBarangay(brgy) ?? new ObservableCollection<Models.BarangayListItem>();

          
                var (month, year) = ParseAsOf();
                data = new ObservableCollection<Models.BarangayListItem>(
                    data.Where(d =>
                    {
                        if (!TryParseRecordDate(d.DateIssued, out DateTime dt)) return false;
                        return dt.Month == month && dt.Year == year;
                    }));

                DgBarangayList.ItemsSource = data;
            }
            catch { DgBarangayList.ItemsSource = new ObservableCollection<Models.BarangayListItem>(); }
        }

        private void LoadQuarterlyList()
        {
            try
            {
                string brgy = GetSelectedBarangay();
                var rawData = PensionDB.GetAllActiveSeniorsForQuarterly("");
                var result = new ObservableCollection<QuarterlyPensionListItem>();
                DateTime today = DateTime.Today;

                foreach (var item in rawData)
                {
          
                    if (brgy != "ALL BARANGAYS" && !string.IsNullOrEmpty(brgy) &&
                        !string.Equals(item.Barangay, brgy, StringComparison.OrdinalIgnoreCase))
                        continue;

       
                    if (!TryParseRecordDate(item.Date, out DateTime registrationDate))
                        registrationDate = today;

                    DateTime releaseDate = registrationDate.AddMonths(3);

            
                    var (filterMonth, filterYear) = ParseAsOf();
                    if (!(registrationDate.Month == filterMonth && registrationDate.Year == filterYear))
                        continue;

               
                    bool isReleased = today >= releaseDate;
                    DateTime firstOfReleaseMonth = new DateTime(releaseDate.Year, releaseDate.Month, 1);
                    DateTime firstOfUpcomingMonth = firstOfReleaseMonth.AddMonths(-1);
                    DateTime lastOfUpcomingMonth = firstOfReleaseMonth.AddDays(-1);
                    bool isUpcoming = !isReleased
                                      && today >= firstOfUpcomingMonth
                                      && today <= lastOfUpcomingMonth;

                    string statusText, bg, border, fg;
                    if (isReleased)
                    {
                        statusText = "Released";
                        bg = "#DCFCE7"; border = "#22C55E"; fg = "#166534";
                    }
                    else if (isUpcoming)
                    {
                        statusText = $"Upcoming ({releaseDate:MMM dd})";
                        bg = "#DBEAFE"; border = "#3B82F6"; fg = "#1E3A8A";
                    }
                    else
                    {
                        statusText = "Pending";
                        bg = "#FEF9C3"; border = "#EAB308"; fg = "#854D0E";
                    }

                    result.Add(new QuarterlyPensionListItem
                    {
                        OscaId = item.OscaId ?? "",
                        FullName = item.FullName ?? "",
                        Date = item.Date ?? "",
                        Barangay = item.Barangay ?? "",
                        Amount = item.Amount ?? "2,250",
                        ReleaseDate = releaseDate.ToString("MM/dd/yyyy"),
                        Status = statusText,
                        StatusBg = bg,
                        StatusBorder = border,
                        StatusFg = fg
                    });
                }

                DgQuarterlyList.ItemsSource = result;
            }
            catch { DgQuarterlyList.ItemsSource = new ObservableCollection<QuarterlyPensionListItem>(); }
        }

        private void LoadAICSList()
        {
            try
            {
                string brgy = GetSelectedBarangay();
                var data = PensionDB.GetAICS() ?? new ObservableCollection<Models.PensionItem>();

                if (brgy != "ALL BARANGAYS" && !string.IsNullOrEmpty(brgy))
                    data = new ObservableCollection<Models.PensionItem>(
                        data.Where(d => string.Equals(d.Brgy, brgy, StringComparison.OrdinalIgnoreCase)));

             
                var (month, year) = ParseAsOf();
                data = new ObservableCollection<Models.PensionItem>(
                    data.Where(d =>
                    {
                        if (!TryParseRecordDate(d.Date, out DateTime dt)) return false;
                        return dt.Month == month && dt.Year == year;
                    }));

                DgAICSList.ItemsSource = data;
            }
            catch { DgAICSList.ItemsSource = new ObservableCollection<Models.PensionItem>(); }
        }

        private void LoadAPRList()
        {
            try
            {
                string brgy = GetSelectedBarangay();
                var data = PensionDB.GetAPR() ?? new ObservableCollection<Models.PensionItem>();

                if (brgy != "ALL BARANGAYS" && !string.IsNullOrEmpty(brgy))
                    data = new ObservableCollection<Models.PensionItem>(
                        data.Where(d => string.Equals(d.Brgy, brgy, StringComparison.OrdinalIgnoreCase)));

                var (month, year) = ParseAsOf();
                data = new ObservableCollection<Models.PensionItem>(
                    data.Where(d =>
                    {
                        if (!TryParseRecordDate(d.Date, out DateTime dt)) return false;
                        return dt.Month == month && dt.Year == year;
                    }));

                DgAPRList.ItemsSource = data;
            }
            catch { DgAPRList.ItemsSource = new ObservableCollection<Models.PensionItem>(); }
        }

        private void LoadBereavedList()
        {
            try
            {
                string brgy = GetSelectedBarangay();
                var data = PensionDB.GetBereaved() ?? new ObservableCollection<Models.BereavedItem>();

                if (brgy != "ALL BARANGAYS" && !string.IsNullOrEmpty(brgy))
                    data = new ObservableCollection<Models.BereavedItem>(
                        data.Where(d => string.Equals(d.Brgy, brgy, StringComparison.OrdinalIgnoreCase)));

                var (month, year) = ParseAsOf();
                data = new ObservableCollection<Models.BereavedItem>(
                    data.Where(d =>
                    {
                        if (!TryParseRecordDate(d.Date, out DateTime dt)) return false;
                        return dt.Month == month && dt.Year == year;
                    }));

                DgBereavedTab.ItemsSource = data;
            }
            catch { DgBereavedTab.ItemsSource = new ObservableCollection<Models.BereavedItem>(); }
        }

      
        private string GetSelectedBarangay()
            => (CmbBarangayFilter?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";

        private void BtnDashboard_Click(object sender, RoutedEventArgs e)
        { new Staffdashboard().Show(); this.Close(); }
        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        { new Staffregister().Show(); this.Close(); }
        private void BtnAAB_Click(object sender, RoutedEventArgs e)
        { new Staffaab().Show(); this.Close(); }
        private void BtnPensionList_Click(object sender, RoutedEventArgs e)
        { LoadBarangayList(); }
        private void BtnReports_Click(object sender, RoutedEventArgs e)
        { new Staffreports().Show(); this.Close(); }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Logout?", "Logout",
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
