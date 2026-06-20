using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static SeniorCtzz.Models;

namespace SeniorCtzz
{
    public partial class Adminreports : Window
    {
        private enum MainTab { None, BarangayList, OverAllSC, Pension }
        private enum PensionTab { None, All, Quarterly, AICS, APR, Bereaved }
        private enum PensionFilter { All, Quarterly, AICS, APR, Bereaved }

        private MainTab _activeMain = MainTab.None;
        private PensionTab _activePension = PensionTab.None;
        private PensionFilter _pensionFilter = PensionFilter.All;

        private List<BarangayListItem> _barangayData = new List<BarangayListItem>();
        private List<OverAllSCItem> _overAllData = new List<OverAllSCItem>();
        private List<PensionItem> _quarterlyData = new List<PensionItem>();
        private List<PensionItem> _aicsData = new List<PensionItem>();
        private List<PensionItem> _aprData = new List<PensionItem>();
        private List<BereavedItem> _bereavdData = new List<BereavedItem>();


        private static readonly string[] _months = {
            "Jan","Feb","Mar","Apr","May","Jun",
            "Jul","Aug","Sep","Oct","Nov","Dec"
        };

        private string _defaultMonthYear;

        public Adminreports()
        {
            InitializeComponent();
            txtUserName.Text = AppSession.FullName ?? "Rebecca A. Reyes";
            txtUserRole.Text = AppSession.Role ?? "Section Head";
            txtInitials.Text = AppSession.GetInitials() ?? "RR";
            TxtDateBadge.Text = DateTime.Now.ToString("MMMM dd, yyyy");

            _defaultMonthYear = $"{_months[DateTime.Now.Month - 1]} {DateTime.Now.Year}";

            LoadFilterBarangays();
            LoadFilterMonths();
            LoadPensionMonths();
            ShowBarangayList();
        }

        private void LoadFilterBarangays()
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
            cmbReportBarangay.Items.Clear();
            foreach (var b in barangays)
                cmbReportBarangay.Items.Add(new ComboBoxItem { Content = b });
            cmbReportBarangay.SelectedIndex = 0;
        }

  
        private void LoadFilterMonths()
        {
            cmbReportAsOf.SelectionChanged -= FilterReport_SelectionChanged;
            cmbReportAsOf.Items.Clear();

            int currentYear = DateTime.Now.Year;
            int currentMonth = DateTime.Now.Month;

            for (int y = 1500; y <= currentYear; y++)
            {
                int maxMonth = (y == currentYear) ? currentMonth : 12;
                for (int m = 1; m <= maxMonth; m++)
                    cmbReportAsOf.Items.Add(new ComboBoxItem { Content = $"{_months[m - 1]} {y}" });
            }

     
            foreach (ComboBoxItem item in cmbReportAsOf.Items)
            {
                if (item.Content?.ToString() == _defaultMonthYear)
                { cmbReportAsOf.SelectedItem = item; break; }
            }

            cmbReportAsOf.SelectionChanged += FilterReport_SelectionChanged;
        }


        private void LoadPensionMonths()
        {
            cmbPensionAsOf.SelectionChanged -= PensionAsOf_SelectionChanged;
            cmbPensionAsOf.Items.Clear();

            int currentYear = DateTime.Now.Year;
            int currentMonth = DateTime.Now.Month;

            for (int y = 1500; y <= currentYear; y++)
            {
                int maxMonth = (y == currentYear) ? currentMonth : 12;
                for (int m = 1; m <= maxMonth; m++)
                    cmbPensionAsOf.Items.Add(new ComboBoxItem { Content = $"{_months[m - 1]} {y}" });
            }

   
            foreach (ComboBoxItem item in cmbPensionAsOf.Items)
            {
                if (item.Content?.ToString() == _defaultMonthYear)
                { cmbPensionAsOf.SelectedItem = item; break; }
            }

            cmbPensionAsOf.SelectionChanged += PensionAsOf_SelectionChanged;
        }

        private string GetFilterBarangay()
            => (cmbReportBarangay?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "ALL BARANGAYS";

        private string GetFilterAsOf()
            => (cmbReportAsOf?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? _defaultMonthYear;

        private string GetPensionAsOf()
            => (cmbPensionAsOf?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? _defaultMonthYear;

  
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

     
        private static (int month, int year) ParseAsOfString(string asOf)
        {
            if (DateTime.TryParse("1 " + asOf, out DateTime dt))
                return (dt.Month, dt.Year);
            return (DateTime.Now.Month, DateTime.Now.Year);
        }


        private static bool MatchAsOf(string dateStr, string asOf)
        {
            if (string.IsNullOrEmpty(dateStr)) return false;
            if (!TryParseRecordDate(dateStr, out DateTime date)) return false;
            var (month, year) = ParseAsOfString(asOf);
            return date.Month == month && date.Year == year;
        }

        private void FilterReport_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            if (_activeMain == MainTab.BarangayList) LoadBarangayData();
        }

        private void PensionAsOf_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            LoadPensionSummary();
        }

        private void UpdatePensionCards()
        {
            cardQuarterly.Visibility = (_pensionFilter == PensionFilter.All || _pensionFilter == PensionFilter.Quarterly) ? Visibility.Visible : Visibility.Collapsed;
            cardAICS.Visibility = (_pensionFilter == PensionFilter.All || _pensionFilter == PensionFilter.AICS) ? Visibility.Visible : Visibility.Collapsed;
            cardAPR.Visibility = (_pensionFilter == PensionFilter.All || _pensionFilter == PensionFilter.APR) ? Visibility.Visible : Visibility.Collapsed;
            cardBereaved.Visibility = (_pensionFilter == PensionFilter.All || _pensionFilter == PensionFilter.Bereaved) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BtnBarangayList_Click(object sender, RoutedEventArgs e) => ShowBarangayList();
        private void BtnOverAllSC_Click(object sender, RoutedEventArgs e) => ShowOverAllSC();
        private void BtnPension_Click(object sender, RoutedEventArgs e) => ShowPension();

        private void BtnAll_Click(object sender, RoutedEventArgs e)
        {
            _pensionFilter = PensionFilter.All;
            _activePension = PensionTab.All;
            ResetTabs();
            btnPension.Style = (Style)FindResource("MainTabActiveStyle");
            btnAll.Style = (Style)FindResource("SubTabActiveStyle");
            UpdatePensionCards();
        }

        private void BtnQuarterly_Click(object sender, RoutedEventArgs e) => ShowPensionSubTab(PensionTab.Quarterly);
        private void BtnAICSReport_Click(object sender, RoutedEventArgs e) => ShowPensionSubTab(PensionTab.AICS);
        private void BtnAPR_Click(object sender, RoutedEventArgs e) => ShowPensionSubTab(PensionTab.APR);
        private void BtnBereaved_Click(object sender, RoutedEventArgs e) => ShowPensionSubTab(PensionTab.Bereaved);

        private void HideAllPanels()
        {
            pnlBarangayList.Visibility = Visibility.Collapsed;
            pnlOverAllSC.Visibility = Visibility.Collapsed;
            pnlPension.Visibility = Visibility.Collapsed;
            pnlPensionSubTabs.Visibility = Visibility.Collapsed;
        }

        private void ResetTabs()
        {
            btnBarangayList.Style = (Style)FindResource("MainTabStyle");
            btnOverAllSC.Style = (Style)FindResource("MainTabStyle");
            btnPension.Style = (Style)FindResource("MainTabStyle");
            btnAll.Style = (Style)FindResource("SubTabStyle");
            btnQuarterly.Style = (Style)FindResource("SubTabStyle");
            btnAICS.Style = (Style)FindResource("SubTabStyle");
            btnAPR.Style = (Style)FindResource("SubTabStyle");
            btnBereaved.Style = (Style)FindResource("SubTabStyle");
        }

        private void ShowBarangayList()
        {
            HideAllPanels(); ResetTabs();
            _activeMain = MainTab.BarangayList;
            pnlBarangayList.Visibility = Visibility.Visible;
            btnBarangayList.Style = (Style)FindResource("MainTabActiveStyle");
            TxtSubtitle.Text = "Barangay List";
            LoadBarangayData();
        }

        private void LoadBarangayData()
        {
            string brgy = GetFilterBarangay();
            string asOf = GetFilterAsOf();

            ObservableCollection<BarangayListItem> data;
            if (brgy == "ALL BARANGAYS")
                data = SeniorDB.GetAllSeniors() ?? new ObservableCollection<BarangayListItem>();
            else
                data = SeniorDB.GetByBarangay(brgy) ?? new ObservableCollection<BarangayListItem>();

    
            data = new ObservableCollection<BarangayListItem>(
                data.Where(item => MatchAsOf(item.DateIssued, asOf)));

            _barangayData = data.ToList();
            dgBarangayList.ItemsSource = _barangayData;
        }

        private void ShowOverAllSC()
        {
            HideAllPanels(); ResetTabs();
            _activeMain = MainTab.OverAllSC;
            pnlOverAllSC.Visibility = Visibility.Visible;
            btnOverAllSC.Style = (Style)FindResource("MainTabActiveStyle");
            TxtSubtitle.Text = "Overall SC";

            string cm = DateTime.Now.ToString("MMMM yyyy").ToUpper();
            txtOverAllTitle.Text = $"OSCA OVER-ALL TOTAL SENIOR CITIZEN 79 BARANGAY AS OF {cm}";
            _overAllData = SeniorDB.GetOverAllTotals().ToList();
            icOverAllSC.ItemsSource = _overAllData;

            txtGrandNat.Text = GrandTotal(_overAllData, d => d.NatMale, d => d.NatFemale);
            txtGrandLoc.Text = GrandTotal(_overAllData, d => d.LocMale, d => d.LocFemale);
            txtGrandWait.Text = GrandTotal(_overAllData, d => d.WaitMale, d => d.WaitFemale);
            txtGrandSss.Text = GrandTotal(_overAllData, d => d.SssMale, d => d.SssFemale);
            txtGrandGsis.Text = GrandTotal(_overAllData, d => d.GsisMale, d => d.GsisFemale);
            txtGrandTotal.Text = GrandTotal(_overAllData, d => d.TotalMale, d => d.TotalFemale);

            txtTotNatM.Text = _overAllData.Sum(d => ParseInt(d.NatMale)).ToString();
            txtTotNatF.Text = _overAllData.Sum(d => ParseInt(d.NatFemale)).ToString();
            txtTotLocM.Text = _overAllData.Sum(d => ParseInt(d.LocMale)).ToString();
            txtTotLocF.Text = _overAllData.Sum(d => ParseInt(d.LocFemale)).ToString();
            txtTotWaitM.Text = _overAllData.Sum(d => ParseInt(d.WaitMale)).ToString();
            txtTotWaitF.Text = _overAllData.Sum(d => ParseInt(d.WaitFemale)).ToString();
            txtTotSssM.Text = _overAllData.Sum(d => ParseInt(d.SssMale)).ToString();
            txtTotSssF.Text = _overAllData.Sum(d => ParseInt(d.SssFemale)).ToString();
            txtTotGsisM.Text = _overAllData.Sum(d => ParseInt(d.GsisMale)).ToString();
            txtTotGsisF.Text = _overAllData.Sum(d => ParseInt(d.GsisFemale)).ToString();
            txtTotTotM.Text = _overAllData.Sum(d => ParseInt(d.TotalMale)).ToString();
            txtTotTotF.Text = _overAllData.Sum(d => ParseInt(d.TotalFemale)).ToString();
        }

        private void ShowPension()
        {
            HideAllPanels(); ResetTabs();
            _activeMain = MainTab.Pension;
            _activePension = PensionTab.All;
            _pensionFilter = PensionFilter.All;

            pnlPension.Visibility = Visibility.Visible;
            pnlPensionSubTabs.Visibility = Visibility.Visible;
            btnPension.Style = (Style)FindResource("MainTabActiveStyle");
            btnAll.Style = (Style)FindResource("SubTabActiveStyle");
            TxtSubtitle.Text = "Pension";

            LoadPensionSummary();
        }

        private void ShowPensionSubTab(PensionTab tab)
        {
            _activePension = tab;
            ResetTabs();
            btnPension.Style = (Style)FindResource("MainTabActiveStyle");

            switch (tab)
            {
                case PensionTab.Quarterly:
                    btnQuarterly.Style = (Style)FindResource("SubTabActiveStyle");
                    _pensionFilter = PensionFilter.Quarterly;
                    break;
                case PensionTab.AICS:
                    btnAICS.Style = (Style)FindResource("SubTabActiveStyle");
                    _pensionFilter = PensionFilter.AICS;
                    break;
                case PensionTab.APR:
                    btnAPR.Style = (Style)FindResource("SubTabActiveStyle");
                    _pensionFilter = PensionFilter.APR;
                    break;
                case PensionTab.Bereaved:
                    btnBereaved.Style = (Style)FindResource("SubTabActiveStyle");
                    _pensionFilter = PensionFilter.Bereaved;
                    break;
            }

            LoadPensionSummary();
        }

        private void LoadPensionSummary()
        {
            string asOf = GetPensionAsOf();

     
            _quarterlyData = FilterPensionByAsOf(PensionDB.GetQuarterly()?.ToList() ?? new List<PensionItem>(), asOf);
            _aicsData = FilterPensionByAsOf(PensionDB.GetAICS()?.ToList() ?? new List<PensionItem>(), asOf);
            _aprData = FilterPensionByAsOf(PensionDB.GetAPR()?.ToList() ?? new List<PensionItem>(), asOf);
            _bereavdData = FilterBereavedByAsOf(PensionDB.GetBereaved()?.ToList() ?? new List<BereavedItem>(), asOf);

        
            string asOfLabel = $"As of {asOf}";

            txtCardQuarterly.Text = _quarterlyData.Count.ToString();
            txtCardQuarterlyAmount.Text = "₱" + TotalStr(_quarterlyData);
            txtCardAICS.Text = _aicsData.Count.ToString();
            txtCardAICSAmount.Text = "₱" + TotalStr(_aicsData);
            txtCardAPR.Text = _aprData.Count.ToString();
            txtCardAPRAmount.Text = "₱" + TotalStr(_aprData);
            txtCardBereaved.Text = _bereavdData.Count.ToString();
            txtCardBereavedAmount.Text = "₱" + BereavedTotalStr(_bereavdData);

     
            if (TxtPensionAsOfLabel != null)
                TxtPensionAsOfLabel.Text = asOfLabel;

            UpdatePensionCards();
        }

    
        private static List<PensionItem> FilterPensionByAsOf(List<PensionItem> data, string asOf)
            => data.Where(item => MatchAsOf(item.Date, asOf)).ToList();

        private static List<BereavedItem> FilterBereavedByAsOf(List<BereavedItem> data, string asOf)
            => data.Where(item => MatchAsOf(item.Date, asOf)).ToList();

        private void CardQuarterly_Click(object sender, MouseButtonEventArgs e) => ShowPensionSubTab(PensionTab.Quarterly);
        private void CardAICS_Click(object sender, MouseButtonEventArgs e) => ShowPensionSubTab(PensionTab.AICS);
        private void CardAPR_Click(object sender, MouseButtonEventArgs e) => ShowPensionSubTab(PensionTab.APR);
        private void CardBereaved_Click(object sender, MouseButtonEventArgs e) => ShowPensionSubTab(PensionTab.Bereaved);

        private static string TotalStr(List<PensionItem> items)
            => items == null || items.Count == 0 ? "0" : items.Sum(d => ParseDec(d.Amount)).ToString("N0");

        private static string BereavedTotalStr(List<BereavedItem> items)
            => items == null || items.Count == 0 ? "0" : items.Sum(d => ParseDec(d.Amount)).ToString("N0");

        private static decimal ParseDec(string s)
        {
            if (string.IsNullOrEmpty(s)) return 0;
            decimal.TryParse(s.Replace(",", "").Replace("₱", ""), out decimal v);
            return v;
        }

        private static int ParseInt(string s)
        {
            if (string.IsNullOrEmpty(s)) return 0;
            int.TryParse(s.Replace(",", ""), out int v);
            return v;
        }

        private static string GrandTotal(List<OverAllSCItem> d,
            Func<OverAllSCItem, string> m, Func<OverAllSCItem, string> f)
            => (d.Sum(x => ParseInt(m(x))) + d.Sum(x => ParseInt(f(x)))).ToString();

        private void BtnDashboard_Click(object sender, RoutedEventArgs e) { new Admindashboard().Show(); Close(); }
        private void BtnManageStaff_Click(object sender, RoutedEventArgs e) { new Adminmanagestaff().Show(); Close(); }
        private void BtnPensionList_Click(object sender, RoutedEventArgs e) { new Adminpensionlist().Show(); Close(); }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to logout?", "Logout",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                AppSession.UserId = "";
                AppSession.FullName = "";
                AppSession.Role = "";
                new MainWindow().Show();
                Close();
            }
        }
    }
}
