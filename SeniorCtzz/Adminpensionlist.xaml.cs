using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static SeniorCtzz.Models;

namespace SeniorCtzz
{
    public partial class Adminpensionlist : Window
    {
        private string _barangayBrgList = "ALL BARANGAYS";
        private string _barangayQuarterly = "ALL BARANGAYS";
        private string _barangayAICS = "ALL BARANGAYS";
        private string _barangayAPR = "ALL BARANGAYS";
        private string _barangayBereaved = "ALL BARANGAYS";

        private string[] _months = { "Jan", "Feb", "Mar", "Apr", "May", "Jun",
                                      "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

        private string _defaultMonthYear;

        private string _asOfBrgList;
        private string _asOfQuarterly;
        private string _asOfAICS;
        private string _asOfAPR;
        private string _asOfBereaved;

        private string _activeTab = "BrgList";

        public Adminpensionlist()
        {
            InitializeComponent();
            Loaded += Adminpensionlist_Loaded;
        }

        private void Adminpensionlist_Loaded(object sender, RoutedEventArgs e)
        {
            LoadBarangays();
            LoadMonths();

            _asOfBrgList = _defaultMonthYear;
            _asOfQuarterly = _defaultMonthYear;
            _asOfAICS = _defaultMonthYear;
            _asOfAPR = _defaultMonthYear;
            _asOfBereaved = _defaultMonthYear;

            SetActiveTab("BrgList");
        }

        private void LoadBarangays()
        {
            string[] barangays = {
                "ALL BARANGAYS","Agay-ayan","Alagatan","Anakan","Bagubad","Bakidbakid","Bal-ason","Bantaawan",
                "Binakalan","Capitulangan","Daan-Lungsod","Dinawehan","Eureka","Hindangon","Kalagonoy","Kalipay",
                "Kamanikan","Kianlagan","Kibuging","Kipuntos","Lawaan","Lawit","Libertad","Libon","Lunao","Lunotan",
                "Malibud","Malinao","Maribucao","Mimbuntong","Mimbalagon","Mimbunga","Minsapinit","Murallon","Odiongan",
                "Pangasihan","Pigsaluhan","Punong","Ricoro","Samay","Sangalan","San Jose","San Juan","San Luis",
                "San Miguel","Santiago","Tagpako","Talisay","Talon","Tinabalan","Tinulongan",
                "Barangay 1","Barangay 2","Barangay 3","Barangay 4","Barangay 5","Barangay 6","Barangay 7","Barangay 8",
                "Barangay 9","Barangay 10","Barangay 11","Barangay 12","Barangay 13","Barangay 14","Barangay 15",
                "Barangay 16","Barangay 17","Barangay 18","Barangay 18-A","Barangay 19","Barangay 20","Barangay 21",
                "Barangay 22","Barangay 22-A","Barangay 23","Barangay 24","Barangay 24-A","Barangay 25","Barangay 26"
            };
            cmbBarangay.Items.Clear();
            foreach (var b in barangays)
                cmbBarangay.Items.Add(new ComboBoxItem { Content = b });
            cmbBarangay.SelectedIndex = 0;
        }

        private void LoadMonths()
        {
            cmbAsOf.Items.Clear();

            int startYear = 1500;
            int currentYear = DateTime.Now.Year;
            int currentMonth = DateTime.Now.Month;

            for (int y = startYear; y <= currentYear; y++)
            {
                int maxMonth = (y == currentYear) ? currentMonth : 12;
                for (int m = 1; m <= maxMonth; m++)
                    cmbAsOf.Items.Add(new ComboBoxItem { Content = $"{_months[m - 1]} {y}" });
            }

            _defaultMonthYear = $"{_months[currentMonth - 1]} {currentYear}";

            foreach (ComboBoxItem item in cmbAsOf.Items)
            {
                if (item.Content?.ToString() == _defaultMonthYear)
                {
                    cmbAsOf.SelectedItem = item;
                    break;
                }
            }
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
                raw.Trim(), formats,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out result);
        }


        private (int month, int year) ParseAsOf()
        {
            string asOf = GetSelectedAsOf();
            if (DateTime.TryParse("1 " + asOf, out DateTime dt))
                return (dt.Month, dt.Year);
            return (DateTime.Now.Month, DateTime.Now.Year);
        }

        private string GetSelectedBarangay()
            => (cmbBarangay?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "ALL BARANGAYS";

        private string GetSelectedAsOf()
            => (cmbAsOf?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? _defaultMonthYear;

        private string GetSavedBarangay(string tab)
        {
            switch (tab)
            {
                case "BrgList": return _barangayBrgList;
                case "Quarterly": return _barangayQuarterly;
                case "AICS": return _barangayAICS;
                case "APR": return _barangayAPR;
                case "BEREAVED": return _barangayBereaved;
                default: return "ALL BARANGAYS";
            }
        }

        private void SaveBarangayForTab(string tab, string brgy)
        {
            switch (tab)
            {
                case "BrgList": _barangayBrgList = brgy; break;
                case "Quarterly": _barangayQuarterly = brgy; break;
                case "AICS": _barangayAICS = brgy; break;
                case "APR": _barangayAPR = brgy; break;
                case "BEREAVED": _barangayBereaved = brgy; break;
            }
        }

        private string GetSavedAsOf(string tab)
        {
            switch (tab)
            {
                case "BrgList": return _asOfBrgList;
                case "Quarterly": return _asOfQuarterly;
                case "AICS": return _asOfAICS;
                case "APR": return _asOfAPR;
                case "BEREAVED": return _asOfBereaved;
                default: return _defaultMonthYear;
            }
        }

        private void SaveAsOfForTab(string tab, string asOf)
        {
            switch (tab)
            {
                case "BrgList": _asOfBrgList = asOf; break;
                case "Quarterly": _asOfQuarterly = asOf; break;
                case "AICS": _asOfAICS = asOf; break;
                case "APR": _asOfAPR = asOf; break;
                case "BEREAVED": _asOfBereaved = asOf; break;
            }
        }

        private void SelectBarangayInCombo(string brgyName)
        {
            cmbBarangay.SelectionChanged -= Filter_SelectionChanged;
            foreach (ComboBoxItem item in cmbBarangay.Items)
            {
                if (item.Content?.ToString() == brgyName)
                { cmbBarangay.SelectedItem = item; break; }
            }
            cmbBarangay.SelectionChanged += Filter_SelectionChanged;
        }

        private void SelectAsOfInCombo(string asOfValue)
        {
            cmbAsOf.SelectionChanged -= Filter_SelectionChanged;
            foreach (ComboBoxItem item in cmbAsOf.Items)
            {
                if (item.Content?.ToString() == asOfValue)
                { cmbAsOf.SelectedItem = item; break; }
            }
            cmbAsOf.SelectionChanged += Filter_SelectionChanged;
        }

        private void HideAll()
        {
            pnlBrgList.Visibility = Visibility.Collapsed;
            pnlQuarterly.Visibility = Visibility.Collapsed;
            pnlAICS.Visibility = Visibility.Collapsed;
            pnlAPR.Visibility = Visibility.Collapsed;
            pnlBereaved.Visibility = Visibility.Collapsed;

            btnBrgList.Style = (Style)FindResource("TabButtonStyle");
            btnQuarterly.Style = (Style)FindResource("TabButtonStyle");
            btnAICS.Style = (Style)FindResource("TabButtonStyle");
            btnAPR.Style = (Style)FindResource("TabButtonStyle");
            btnBereaved.Style = (Style)FindResource("TabButtonStyle");
        }

        private void BtnBrgList_Click(object sender, RoutedEventArgs e) => SetActiveTab("BrgList");
        private void BtnQuarterly_Click(object sender, RoutedEventArgs e) => SetActiveTab("Quarterly");
        private void BtnAICS_Click(object sender, RoutedEventArgs e) => SetActiveTab("AICS");
        private void BtnAPR_Click(object sender, RoutedEventArgs e) => SetActiveTab("APR");
        private void BtnBereaved_Click(object sender, RoutedEventArgs e) => SetActiveTab("BEREAVED");

        private void SetActiveTab(string tab)
        {
            _activeTab = tab;
            HideAll();

            LblFilterLabel.Text = tab == "BrgList" ? "LIST OF SENIOR CITIZENS" : "BARANGAY:";
            SelectBarangayInCombo(GetSavedBarangay(tab));
            SelectAsOfInCombo(GetSavedAsOf(tab));

            switch (tab)
            {
                case "BrgList":
                    btnBrgList.Style = (Style)FindResource("TabButtonActiveStyle");
                    pnlBrgList.Visibility = Visibility.Visible;
                    LoadBrgList();
                    break;
                case "Quarterly":
                    btnQuarterly.Style = (Style)FindResource("TabButtonActiveStyle");
                    pnlQuarterly.Visibility = Visibility.Visible;
                    LoadQuarterly();
                    break;
                case "AICS":
                    btnAICS.Style = (Style)FindResource("TabButtonActiveStyle");
                    pnlAICS.Visibility = Visibility.Visible;
                    LoadAICS();
                    break;
                case "APR":
                    btnAPR.Style = (Style)FindResource("TabButtonActiveStyle");
                    pnlAPR.Visibility = Visibility.Visible;
                    LoadAPR();
                    break;
                case "BEREAVED":
                    btnBereaved.Style = (Style)FindResource("TabButtonActiveStyle");
                    pnlBereaved.Visibility = Visibility.Visible;
                    LoadBereaved();
                    break;
            }
        }

        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            SaveBarangayForTab(_activeTab, GetSelectedBarangay());
            SaveAsOfForTab(_activeTab, GetSelectedAsOf());
            SetActiveTab(_activeTab);
        }

        private void LoadBrgList()
        {
            try
            {
                string brgy = GetSelectedBarangay();
                var (month, year) = ParseAsOf();

                ObservableCollection<BarangayListItem> data;
                if (brgy == "ALL BARANGAYS")
                    data = SeniorDB.GetAllSeniors() ?? new ObservableCollection<BarangayListItem>();
                else
                    data = SeniorDB.GetByBarangay(brgy) ?? new ObservableCollection<BarangayListItem>();

               
                data = new ObservableCollection<BarangayListItem>(
                    data.Where(d =>
                    {
                        if (!TryParseRecordDate(d.DateIssued, out DateTime dt)) return false;
                        return dt.Month == month && dt.Year == year;
                    }));

                dgBrgList.ItemsSource = data;
            }
            catch { dgBrgList.ItemsSource = new ObservableCollection<BarangayListItem>(); }
        }

        private void LoadQuarterly()
        {
            try
            {
                string brgy = GetSelectedBarangay();
                var (filterMonth, filterYear) = ParseAsOf();

                var rawData = PensionDB.GetAllActiveSeniorsForQuarterly("");
                var result = new ObservableCollection<AdminQuarterlyItem>();
                DateTime today = DateTime.Today;

                foreach (var item in rawData)
                {
              
                    if (brgy != "ALL BARANGAYS" &&
                        !string.Equals(item.Barangay, brgy, StringComparison.OrdinalIgnoreCase))
                        continue;

                
                    if (!TryParseRecordDate(item.Date, out DateTime registrationDate))
                        registrationDate = today;

             
                    if (registrationDate.Month != filterMonth || registrationDate.Year != filterYear)
                        continue;

               
                    DateTime releaseDate = registrationDate.AddMonths(3);

          
                    bool isReleased = today >= releaseDate;
                    DateTime firstOfReleaseMonth = new DateTime(releaseDate.Year, releaseDate.Month, 1);
                    DateTime firstOfUpcomingMonth = firstOfReleaseMonth.AddMonths(-1);
                    DateTime lastOfUpcomingMonth = firstOfReleaseMonth.AddDays(-1);
                    bool isUpcoming = !isReleased
                                      && today >= firstOfUpcomingMonth
                                      && today <= lastOfUpcomingMonth;

                    string statusText, bg, border, fg;
                    if (isReleased)
                    { statusText = "Released"; bg = "#DCFCE7"; border = "#22C55E"; fg = "#166534"; }
                    else if (isUpcoming)
                    { statusText = $"Upcoming ({releaseDate:MMM dd})"; bg = "#DBEAFE"; border = "#3B82F6"; fg = "#1E3A8A"; }
                    else
                    { statusText = "Pending"; bg = "#FEF9C3"; border = "#EAB308"; fg = "#854D0E"; }

                    result.Add(new AdminQuarterlyItem
                    {
                        No = (result.Count + 1).ToString(),
                        Date = item.Date ?? "",
                        FirstName = item.FullName?.Split(' ').FirstOrDefault() ?? "",
                        LastName = item.FullName?.Contains(" ") == true
                                         ? item.FullName.Substring(item.FullName.IndexOf(' ') + 1) : "",
                        Brgy = item.Barangay ?? "",
                        Amount = item.Amount ?? "2,250",
                        ReleaseDate = releaseDate.ToString("MM/dd/yyyy"),
                        Status = statusText,
                        StatusBg = bg,
                        StatusBorder = border,
                        StatusFg = fg
                    });
                }

                dgQuarterly.ItemsSource = result;

             
                decimal total = 0;
                foreach (var r in result)
                    if (decimal.TryParse((r.Amount ?? "0").Replace(",", ""), out decimal v)) total += v;
                txtQuarterlyTotal.Text = total.ToString("N0");
            }
            catch { dgQuarterly.ItemsSource = new ObservableCollection<AdminQuarterlyItem>(); }
        }

        private void LoadAICS()
        {
            try
            {
                string brgy = GetSelectedBarangay();
                var (month, year) = ParseAsOf();
                var data = PensionDB.GetAICS() ?? new ObservableCollection<PensionItem>();

                if (brgy != "ALL BARANGAYS")
                    data = new ObservableCollection<PensionItem>(
                        data.Where(d => string.Equals(d.Brgy, brgy, StringComparison.OrdinalIgnoreCase)));

                data = new ObservableCollection<PensionItem>(
                    data.Where(d =>
                    {
                        if (!TryParseRecordDate(d.Date, out DateTime dt)) return false;
                        return dt.Month == month && dt.Year == year;
                    }));

                dgAICS.ItemsSource = data;
                txtAICSTotal.Text = ComputeTotal(data);
            }
            catch { dgAICS.ItemsSource = new ObservableCollection<PensionItem>(); }
        }

        private void LoadAPR()
        {
            try
            {
                string brgy = GetSelectedBarangay();
                var (month, year) = ParseAsOf();
                var data = PensionDB.GetAPR() ?? new ObservableCollection<PensionItem>();

                if (brgy != "ALL BARANGAYS")
                    data = new ObservableCollection<PensionItem>(
                        data.Where(d => string.Equals(d.Brgy, brgy, StringComparison.OrdinalIgnoreCase)));

                data = new ObservableCollection<PensionItem>(
                    data.Where(d =>
                    {
                        if (!TryParseRecordDate(d.Date, out DateTime dt)) return false;
                        return dt.Month == month && dt.Year == year;
                    }));

                dgAPR.ItemsSource = data;
                txtAPRTotal.Text = ComputeTotal(data);
            }
            catch { dgAPR.ItemsSource = new ObservableCollection<PensionItem>(); }
        }

        private void LoadBereaved()
        {
            try
            {
                string brgy = GetSelectedBarangay();
                var (month, year) = ParseAsOf();
                var data = PensionDB.GetBereaved() ?? new ObservableCollection<BereavedItem>();

                if (brgy != "ALL BARANGAYS")
                    data = new ObservableCollection<BereavedItem>(
                        data.Where(d => string.Equals(d.Brgy, brgy, StringComparison.OrdinalIgnoreCase)));

                data = new ObservableCollection<BereavedItem>(
                    data.Where(d =>
                    {
                        if (!TryParseRecordDate(d.Date, out DateTime dt)) return false;
                        return dt.Month == month && dt.Year == year;
                    }));

                dgBereaved.ItemsSource = data;
                txtBereavedTotal.Text = ComputeBereavedTotal(data);
            }
            catch { dgBereaved.ItemsSource = new ObservableCollection<BereavedItem>(); }
        }

        private static string ComputeTotal(ObservableCollection<PensionItem> items)
        {
            decimal total = 0;
            foreach (var item in items)
                if (decimal.TryParse((item.Amount ?? "0").Replace(",", ""), out decimal v)) total += v;
            return total.ToString("N0");
        }

        private static string ComputeBereavedTotal(ObservableCollection<BereavedItem> items)
        {
            decimal total = 0;
            foreach (var item in items)
                if (decimal.TryParse((item.Amount ?? "0").Replace(",", ""), out decimal v)) total += v;
            return total.ToString("N0");
        }

        private void BtnDashboard_Click(object sender, RoutedEventArgs e)
        { new Admindashboard().Show(); this.Close(); }

        private void BtnManageStaff_Click(object sender, RoutedEventArgs e)
        { new Adminmanagestaff().Show(); this.Close(); }

        private void BtnReports_Click(object sender, RoutedEventArgs e)
        { new Adminreports().Show(); this.Close(); }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var r = MessageBox.Show("Are you sure you want to logout?", "Logout",
                        MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (r == MessageBoxResult.Yes)
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
