using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Microsoft.Win32;
using System.Printing;
using System.Windows.Media.Imaging;
using static SeniorCtzz.Models;

namespace SeniorCtzz
{
    public partial class Staffreports : Window
    {
        private enum MainTab { None, BarangayList, OverAllSC, Pension }
        private enum PensionTab { None, Quarterly, AICS, APR, Bereaved }

        private MainTab _activeMain = MainTab.None;
        private PensionTab _activePension = PensionTab.None;

        private List<BarangayListItem> _barangayData = new List<BarangayListItem>();
        private List<OverAllSCItem> _overAllData = new List<OverAllSCItem>();
        private List<PensionItem> _quarterlyData = new List<PensionItem>();
        private List<PensionItem> _aicsData = new List<PensionItem>();
        private List<PensionItem> _aprData = new List<PensionItem>();
        private List<BereavedItem> _bereavdData = new List<BereavedItem>();

        // ── Encoder name used in all signature blocks ──────────────
        private string EncoderName => (AppSession.FullName ?? "").ToUpper();

        public Staffreports()
        {
            InitializeComponent();

            // Sidebar user info
            txtUserName.Text = AppSession.FullName ?? "Staff User";
            txtAvatarInitials.Text = AppSession.GetInitials() ?? "SJ";
            txtUserRole.Text = "Staff Encoder";
            TxtDateBadge.Text = DateTime.Now.ToString("MMMM dd, yyyy");

            LoadFilterBarangays();
            LoadFilterMonths();
            ShowBarangayList();
        }

        // ═══════════════════════════════════════════
        // FILTER SETUP
        // ═══════════════════════════════════════════
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
            cmbReportAsOf.Items.Clear();
            int currentYear = DateTime.Now.Year;
            int currentMonth = DateTime.Now.Month;
            const int startYear = 2020;

            for (int year = startYear; year <= currentYear; year++)
            {
                int maxMonth = (year == currentYear) ? currentMonth : 12;
                for (int m = 1; m <= maxMonth; m++)
                    cmbReportAsOf.Items.Add(
                        new ComboBoxItem { Content = new DateTime(year, m, 1).ToString("MMM yyyy") });
            }

            string cur = DateTime.Now.ToString("MMM yyyy");
            foreach (ComboBoxItem item in cmbReportAsOf.Items)
                if (item.Content?.ToString() == cur) { cmbReportAsOf.SelectedItem = item; return; }
            if (cmbReportAsOf.Items.Count > 0)
                cmbReportAsOf.SelectedIndex = cmbReportAsOf.Items.Count - 1;
        }

        private string GetFilterBarangay()
            => (cmbReportBarangay?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "ALL BARANGAYS";
        private string GetFilterAsOf()
            => (cmbReportAsOf?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";

        private void FilterReport_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            if (_activeMain == MainTab.BarangayList) LoadBarangayData();
        }

        // ═══════════════════════════════════════════
        // TAB NAVIGATION
        // ═══════════════════════════════════════════
        private void BtnBarangayList_Click(object sender, RoutedEventArgs e) => ShowBarangayList();
        private void BtnOverAllSC_Click(object sender, RoutedEventArgs e) => ShowOverAllSC();
        private void BtnPension_Click(object sender, RoutedEventArgs e) => ShowPension();
        private void BtnQuarterly_Click(object sender, RoutedEventArgs e) => ShowPensionSubTab(PensionTab.Quarterly);
        private void BtnAICSReport_Click(object sender, RoutedEventArgs e) => ShowPensionSubTab(PensionTab.AICS);
        private void BtnAPR_Click(object sender, RoutedEventArgs e) => ShowPensionSubTab(PensionTab.APR);
        private void BtnBereaved_Click(object sender, RoutedEventArgs e) => ShowPensionSubTab(PensionTab.Bereaved);

        private void HideAllPanels()
        {
            pnlBarangayList.Visibility = Visibility.Collapsed;
            pnlOverAllSC.Visibility = Visibility.Collapsed;
            pnlQuarterly.Visibility = Visibility.Collapsed;
            pnlAICS.Visibility = Visibility.Collapsed;
            pnlAPR.Visibility = Visibility.Collapsed;
            pnlBereaved.Visibility = Visibility.Collapsed;
            pnlPensionSubTabs.Visibility = Visibility.Collapsed;
        }

        private void ResetTabs()
        {
            btnBarangayList.Style = (Style)FindResource("MainTabStyle");
            btnOverAllSC.Style = (Style)FindResource("MainTabStyle");
            btnPension.Style = (Style)FindResource("MainTabStyle");
            btnQuarterly.Style = (Style)FindResource("SubTabStyle");
            btnAICS.Style = (Style)FindResource("SubTabStyle");
            btnAPR.Style = (Style)FindResource("SubTabStyle");
            btnBereaved.Style = (Style)FindResource("SubTabStyle");
        }

        // ═══════════════════════════════════════════
        // BARANGAY LIST
        // ═══════════════════════════════════════════
        private void ShowBarangayList()
        {
            HideAllPanels(); ResetTabs();
            _activeMain = MainTab.BarangayList;
            pnlBarangayList.Visibility = Visibility.Visible;
            btnBarangayList.Style = (Style)FindResource("MainTabActiveStyle");
            TxtSubtitle.Text = "Barangay List";

            // Set encoder name in the on-screen signature block
            TxtBrgySigPreparedName.Text = EncoderName;

            LoadBarangayData();
            SetExportStatus("Barangay List report ready");
        }

        private void LoadBarangayData()
        {
            string brgy = GetFilterBarangay();
            string asOf = GetFilterAsOf();

            // 1 — Get full list (all or filtered by barangay)
            var raw = brgy == "ALL BARANGAYS"
                ? (SeniorDB.GetAllSeniors() ?? new System.Collections.ObjectModel.ObservableCollection<BarangayListItem>()).ToList()
                : (SeniorDB.GetByBarangay(brgy) ?? new System.Collections.ObjectModel.ObservableCollection<BarangayListItem>()).ToList();

            // 2 — Filter by AS OF month/year using DateIssued
            var (filterMonth, filterYear) = ParseAsOf(asOf);
            if (filterMonth > 0)
            {
                raw = raw.Where(d =>
                {
                    if (!TryParseRecordDate(d.DateIssued, out DateTime dt)) return false;
                    return dt.Month == filterMonth && dt.Year == filterYear;
                }).ToList();
            }

            _barangayData = raw;
            dgBarangayList.ItemsSource = _barangayData;
            SetExportStatus($"Barangay List — {_barangayData.Count} record(s) | {(filterMonth > 0 ? asOf : "All Months")}");
        }

        /// <summary>
        /// Parses "MMM yyyy" (e.g. "May 2026") → (month, year).
        /// Returns (0, 0) if parsing fails or string is empty.
        /// </summary>
        private static (int month, int year) ParseAsOf(string asOf)
        {
            if (string.IsNullOrWhiteSpace(asOf)) return (0, 0);
            if (DateTime.TryParse("1 " + asOf,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out DateTime dt))
                return (dt.Month, dt.Year);
            return (0, 0);
        }

        /// <summary>
        /// Safely parses a date stored as "MM-dd-yy" (e.g. "05-19-26" → May 19 2026).
        /// Also handles "MM-dd-yyyy", "MM/dd/yy", "MM/dd/yyyy" as fallbacks.
        /// </summary>
        private static bool TryParseRecordDate(string raw, out DateTime result)
        {
            result = DateTime.MinValue;
            if (string.IsNullOrWhiteSpace(raw)) return false;

            string[] formats = {
                "MM-dd-yy",   // "05-19-26"  ← primary DB format
                "MM-dd-yyyy", // "05-19-2026"
                "M-d-yy",
                "M-d-yyyy",
                "MM/dd/yy",
                "MM/dd/yyyy",
                "M/d/yy",
                "M/d/yyyy",
                "yyyy-MM-dd", // ISO fallback
            };

            return DateTime.TryParseExact(
                raw.Trim(),
                formats,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out result);
        }

        // ═══════════════════════════════════════════
        // OVER-ALL SC
        // ═══════════════════════════════════════════
        private void ShowOverAllSC()
        {
            HideAllPanels(); ResetTabs();
            _activeMain = MainTab.OverAllSC;
            pnlOverAllSC.Visibility = Visibility.Visible;
            btnOverAllSC.Style = (Style)FindResource("MainTabActiveStyle");
            TxtSubtitle.Text = "Overall SC";

            string currentMonthYear = DateTime.Now.ToString("MMMM yyyy").ToUpper();
            txtOverAllTitle.Text = $"OSCA OVER-ALL TOTAL SENIOR CITIZEN 79 BARANGAY AS OF {currentMonthYear}";

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

            txtPreparedByName.Text = EncoderName;
            SetExportStatus($"Overall SC report ready — {_overAllData.Count} classifications");
        }

        // ═══════════════════════════════════════════
        // PENSION
        // ═══════════════════════════════════════════
        private void ShowPension()
        {
            HideAllPanels(); ResetTabs();
            _activeMain = MainTab.Pension;
            pnlPensionSubTabs.Visibility = Visibility.Visible;
            btnPension.Style = (Style)FindResource("MainTabActiveStyle");
            ShowPensionSubTab(PensionTab.Quarterly);
        }

        private void ShowPensionSubTab(PensionTab tab)
        {
            pnlQuarterly.Visibility = Visibility.Collapsed;
            pnlAICS.Visibility = Visibility.Collapsed;
            pnlAPR.Visibility = Visibility.Collapsed;
            pnlBereaved.Visibility = Visibility.Collapsed;

            _activePension = tab;

            btnQuarterly.Style = (Style)FindResource("SubTabStyle");
            btnAICS.Style = (Style)FindResource("SubTabStyle");
            btnAPR.Style = (Style)FindResource("SubTabStyle");
            btnBereaved.Style = (Style)FindResource("SubTabStyle");

            switch (tab)
            {
                case PensionTab.Quarterly:
                    btnQuarterly.Style = (Style)FindResource("SubTabActiveStyle");
                    pnlQuarterly.Visibility = Visibility.Visible;
                    _quarterlyData = PensionDB.GetQuarterly().ToList();
                    dgQuarterly.ItemsSource = _quarterlyData;
                    txtQuarterlyTotal.Text = TotalStr(_quarterlyData);
                    // Set encoder name in on-screen sig block
                    TxtPensionSigPreparedName.Text = EncoderName;
                    SetExportStatus($"Quarterly — {_quarterlyData.Count} records");
                    break;

                case PensionTab.AICS:
                    btnAICS.Style = (Style)FindResource("SubTabActiveStyle");
                    pnlAICS.Visibility = Visibility.Visible;
                    _aicsData = PensionDB.GetAICS().ToList();
                    dgAICS.ItemsSource = _aicsData;
                    txtAICSTotal.Text = TotalStr(_aicsData);
                    TxtAICSSigPreparedName.Text = EncoderName;
                    SetExportStatus($"AICS — {_aicsData.Count} records");
                    break;

                case PensionTab.APR:
                    btnAPR.Style = (Style)FindResource("SubTabActiveStyle");
                    pnlAPR.Visibility = Visibility.Visible;
                    _aprData = PensionDB.GetAPR().ToList();
                    dgAPR.ItemsSource = _aprData;
                    txtAPRTotal.Text = TotalStr(_aprData);
                    TxtAPRSigPreparedName.Text = EncoderName;
                    SetExportStatus($"APR — {_aprData.Count} records");
                    break;

                case PensionTab.Bereaved:
                    btnBereaved.Style = (Style)FindResource("SubTabActiveStyle");
                    pnlBereaved.Visibility = Visibility.Visible;
                    _bereavdData = PensionDB.GetBereaved().ToList();
                    dgBereaved.ItemsSource = _bereavdData;
                    txtBereavedTotal.Text = BereavedTotalStr(_bereavdData);
                    TxtBereavedSigPreparedName.Text = EncoderName;
                    SetExportStatus($"Bereaved — {_bereavdData.Count} records");
                    break;
            }
        }

        // ═══════════════════════════════════════════
        // EXPORT — PDF (print dialog)
        // ═══════════════════════════════════════════
        private void BtnExportPdf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var printDlg = new PrintDialog();
                printDlg.PrintTicket.PageOrientation = PageOrientation.Landscape;
                if (printDlg.ShowDialog() != true) return;

                double pageW = printDlg.PrintableAreaWidth;
                double pageH = printDlg.PrintableAreaHeight;
                double margin = 15;
                double usableW = pageW - margin * 2;

                FlowDocument doc = BuildFlowDocument(usableW);
                doc.PageWidth = pageW;
                doc.PageHeight = pageH;
                doc.ColumnWidth = double.PositiveInfinity;
                doc.PagePadding = new Thickness(margin);

                IDocumentPaginatorSource src = doc;
                printDlg.PrintDocument(src.DocumentPaginator,
                    $"CSWD Report - {DateTime.Now:yyyy-MM-dd}");
                SetExportStatus("PDF sent to printer");
            }
            catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}"); }
        }

        // ═══════════════════════════════════════════
        // EXPORT — CSV
        // ═══════════════════════════════════════════
        private void BtnExportCsv_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string reportType = GetActiveTabName().Replace(" ", "_");
                var dlg = new SaveFileDialog
                {
                    Filter = "CSV Files (*.csv)|*.csv",
                    FileName = $"{reportType}_Report_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };
                if (dlg.ShowDialog() != true) return;
                File.WriteAllText(dlg.FileName, BuildCsv(), Encoding.UTF8);
                SetExportStatus($"CSV saved: {Path.GetFileName(dlg.FileName)}");
                MessageBox.Show("CSV exported successfully!", "Export",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}"); }
        }

        // ═══════════════════════════════════════════
        // CSV BUILDER
        // ═══════════════════════════════════════════
        private string BuildCsv()
        {
            var sb = new StringBuilder();
            string currentMonthYear = DateTime.Now.ToString("MMMM yyyy").ToUpper();

            if (_activeMain == MainTab.OverAllSC)
            {
                sb.AppendLine("REPUBLIC OF THE PHILIPPINES");
                sb.AppendLine("PROVINCE OF MISAMIS ORIENTAL");
                sb.AppendLine("GINGOOG CITY");
                sb.AppendLine("CITY SOCIAL WELFARE & DEVELOPMENT OFFICE");
                sb.AppendLine();
                sb.AppendLine($"OSCA OVER-ALL TOTAL SENIOR CITIZEN 79 BARANGAY AS OF {currentMonthYear}");
                sb.AppendLine();
                sb.AppendLine("NO.,BARANGAY,NAT M,NAT F,LOC M,LOC F,WAIT M,WAIT F,SSS M,SSS F,GSIS M,GSIS F,TOTAL M,TOTAL F");
                foreach (var r in _overAllData)
                    sb.AppendLine($"{E(r.No)},{E(r.Barangay)},{E(r.NatMale)},{E(r.NatFemale)},{E(r.LocMale)},{E(r.LocFemale)},{E(r.WaitMale)},{E(r.WaitFemale)},{E(r.SssMale)},{E(r.SssFemale)},{E(r.GsisMale)},{E(r.GsisFemale)},{E(r.TotalMale)},{E(r.TotalFemale)}");
                sb.AppendLine();
                sb.AppendLine($"TOTAL,,{txtTotNatM.Text},{txtTotNatF.Text},{txtTotLocM.Text},{txtTotLocF.Text},{txtTotWaitM.Text},{txtTotWaitF.Text},{txtTotSssM.Text},{txtTotSssF.Text},{txtTotGsisM.Text},{txtTotGsisF.Text},{txtTotTotM.Text},{txtTotTotF.Text}");
                sb.AppendLine();
                sb.AppendLine($"National Pensioner Total:,{txtGrandNat.Text}");
                sb.AppendLine($"Local Pensioner Total:,{txtGrandLoc.Text}");
                sb.AppendLine($"Waitlist Total:,{txtGrandWait.Text}");
                sb.AppendLine($"SSS Total:,{txtGrandSss.Text}");
                sb.AppendLine($"GSIS Total:,{txtGrandGsis.Text}");
                sb.AppendLine($"GRAND TOTAL:,{txtGrandTotal.Text}");
                sb.AppendLine();
                sb.AppendLine($"PREPARED BY:,{EncoderName}");
                sb.AppendLine(",SENIOR CITIZENS STAFF ENCODER");
                sb.AppendLine();
                sb.AppendLine("Noted by:,DAISY JANE R. ACERO RSW");
                sb.AppendLine(",SWO-III/ SC-Focal Person");
            }
            else if (_activeMain == MainTab.BarangayList)
            {
                string brgy = GetFilterBarangay();
                string asOf = GetFilterAsOf();
                sb.AppendLine("CSWD Gingoog City — Barangay List Report");
                sb.AppendLine($"LIST OF SENIOR CITIZEN'S BARANGAY: {brgy}");
                sb.AppendLine($"AS OF: {asOf}");
                sb.AppendLine($"Generated: {DateTime.Now:MMMM dd, yyyy}");
                sb.AppendLine();
                sb.AppendLine("NO,LAST NAME,FIRST NAME,MIDDLE NAME,SUFFIX,SEX,BIRTH DATE,AGE,STATUS,OSCA ID,DATE ISSUED,BLOOD TYPE,LGU,DSWD,SSS,GSIS,WAITLIST");
                foreach (var r in _barangayData)
                    sb.AppendLine($"{E(r.No)},{E(r.LastName)},{E(r.FirstName)},{E(r.MiddleName)},{E(r.Suffix)},{E(r.Sex)},{E(r.BirthDate)},{E(r.Age)},{E(r.Status)},{E(r.OscaId)},{E(r.DateIssued)},{E(r.BloodType)},{E(r.Lgu)},{E(r.Dswd)},{E(r.Sss)},{E(r.Gsis)},{E(r.Waitlist)}");
                sb.AppendLine();
                sb.AppendLine($"PREPARED BY:,{EncoderName}");
                sb.AppendLine(",ENCODER");
                sb.AppendLine();
                sb.AppendLine("CHECKED BY:,REBECCA A. REYES");
                sb.AppendLine(",ADMIN AIDE II");
                sb.AppendLine();
                sb.AppendLine("NOTED BY:,DAISY JANE R. ACERO RSW");
                sb.AppendLine(",SWO-III/ SC-Focal Person");
            }
            else if (_activeMain == MainTab.Pension)
            {
                string pensionType = _activePension.ToString();
                sb.AppendLine($"CSWD Gingoog City — {pensionType} Report");
                sb.AppendLine($"Generated: {DateTime.Now:MMMM dd, yyyy}");
                sb.AppendLine();

                if (_activePension == PensionTab.Quarterly)
                {
                    sb.AppendLine("NO,DATE,FIRST NAME,LAST NAME,BRGY,AMOUNT");
                    foreach (var r in _quarterlyData)
                        sb.AppendLine($"{E(r.No)},{E(r.Date)},{E(r.FirstName)},{E(r.LastName)},{E(r.Brgy)},{E(r.Amount)}");
                    sb.AppendLine($"TOTAL,,,,,{txtQuarterlyTotal.Text}");
                }
                else if (_activePension == PensionTab.AICS)
                {
                    sb.AppendLine("NO,DATE,FIRST NAME,LAST NAME,BRGY,AMOUNT");
                    foreach (var r in _aicsData)
                        sb.AppendLine($"{E(r.No)},{E(r.Date)},{E(r.FirstName)},{E(r.LastName)},{E(r.Brgy)},{E(r.Amount)}");
                    sb.AppendLine($"TOTAL,,,,,{txtAICSTotal.Text}");
                }
                else if (_activePension == PensionTab.APR)
                {
                    sb.AppendLine("NO,DATE,FIRST NAME,LAST NAME,BRGY,AMOUNT");
                    foreach (var r in _aprData)
                        sb.AppendLine($"{E(r.No)},{E(r.Date)},{E(r.FirstName)},{E(r.LastName)},{E(r.Brgy)},{E(r.Amount)}");
                    sb.AppendLine($"TOTAL,,,,,{txtAPRTotal.Text}");
                }
                else if (_activePension == PensionTab.Bereaved)
                {
                    sb.AppendLine("NO,DATE,FIRST NAME,LAST NAME,BRGY,RECIPIENT,RELATIONSHIP,AMOUNT,REMARKS");
                    foreach (var r in _bereavdData)
                        sb.AppendLine($"{E(r.No)},{E(r.Date)},{E(r.FirstName)},{E(r.LastName)},{E(r.Brgy)},{E(r.RecipientName)},{E(r.Relationship)},{E(r.Amount)},{E(r.Remarks)}");
                    sb.AppendLine($"TOTAL,,,,,,,{txtBereavedTotal.Text}");
                }

                sb.AppendLine();
                sb.AppendLine($"PREPARED BY:,{EncoderName}");
                sb.AppendLine(",ENCODER");
                sb.AppendLine();
                sb.AppendLine("NOTED BY:,DAISY JANE R. ACERO RSW");
                sb.AppendLine(",SWO-III/ SC-Focal Person");
            }

            return sb.ToString();
        }

        // ═══════════════════════════════════════════
        // FLOW DOCUMENT (Print)
        // ═══════════════════════════════════════════
        private FlowDocument BuildFlowDocument(double usableWidth = 1030)
        {
            var doc = new FlowDocument
            {
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 9,
                PagePadding = new Thickness(12),
                ColumnWidth = double.PositiveInfinity
            };

            string currentMonthYear = DateTime.Now.ToString("MMMM yyyy").ToUpper();

            if (_activeMain == MainTab.OverAllSC)
                BuildOverAllSCDocument(doc, currentMonthYear, usableWidth);
            else if (_activeMain == MainTab.BarangayList)
                BuildBarangayListDocument(doc, usableWidth);
            else if (_activeMain == MainTab.Pension)
                BuildPensionDocument(doc, usableWidth);

            return doc;
        }

        // ═══════════════════════════════════════════
        // PRINT — OVERALL SC (signature already existed; unchanged)
        // ═══════════════════════════════════════════
        private void BuildOverAllSCDocument(FlowDocument doc, string currentMonthYear, double usableW)
        {
            double noW = 40;
            double brgyW = 90;
            double colW = Math.Floor((usableW - noW - brgyW) / 12);

            // ── Blue + Red header ──────────────────────────────────────
            var headerTable = new Table { CellSpacing = 0, BorderThickness = new Thickness(0) };
            headerTable.Columns.Add(new TableColumn { Width = new GridLength(usableW) });
            var headerRG = new TableRowGroup();

            var blueRow = new TableRow();
            var blueCell = new TableCell
            {
                Background = new SolidColorBrush(Color.FromRgb(30, 58, 138)),
                Padding = new Thickness(0, 3, 0, 3),
                BorderThickness = new Thickness(0)
            };
            var bluePara = new Paragraph { TextAlignment = TextAlignment.Center, Margin = new Thickness(0) };
            bluePara.Inlines.Add(new Run("REPUBLIC OF THE PHILIPPINES") { FontSize = 8, FontWeight = FontWeights.Bold, Foreground = Brushes.White });
            bluePara.Inlines.Add(new LineBreak());
            bluePara.Inlines.Add(new Run("PROVINCE OF MISAMIS ORIENTAL") { FontSize = 8, FontWeight = FontWeights.Bold, Foreground = Brushes.White });
            blueCell.Blocks.Add(bluePara);
            blueRow.Cells.Add(blueCell);
            headerRG.Rows.Add(blueRow);

            var redRow = new TableRow();
            var redCell = new TableCell
            {
                Background = new SolidColorBrush(Color.FromRgb(185, 28, 28)),
                Padding = new Thickness(12, 10, 12, 10),
                BorderThickness = new Thickness(0)
            };
            var grid = new System.Windows.Controls.Grid();
            grid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = new GridLength(90) });
            grid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            try
            {
                var uri = new Uri("pack://application:,,,/Assets/logo1.png", UriKind.Absolute);
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = uri;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.DecodePixelWidth = 80;
                bitmap.EndInit();
                bitmap.Freeze();

                var circleBorder = new Border
                {
                    Width = 68,
                    Height = 68,
                    CornerRadius = new CornerRadius(34),
                    Background = Brushes.White,
                    BorderBrush = new SolidColorBrush(Color.FromRgb(252, 211, 77)),
                    BorderThickness = new Thickness(2),
                    Child = new Image { Source = bitmap, Width = 58, Height = 58, Stretch = Stretch.Uniform },
                    Padding = new Thickness(4),
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                    VerticalAlignment = System.Windows.VerticalAlignment.Center
                };
                System.Windows.Controls.Grid.SetColumn(circleBorder, 0);
                grid.Children.Add(circleBorder);
            }
            catch { }

            var textStack = new System.Windows.Controls.StackPanel
            {
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center
            };
            textStack.Children.Add(new System.Windows.Controls.TextBlock
            {
                Text = "GINGOOG CITY",
                FontSize = 26,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center
            });
            textStack.Children.Add(new System.Windows.Controls.TextBlock
            {
                Text = "CITY SOCIAL WELFARE & DEVELOPMENT OFFICE",
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(252, 211, 77)),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center
            });
            System.Windows.Controls.Grid.SetColumn(textStack, 1);
            grid.Children.Add(textStack);

            redCell.Blocks.Add(new BlockUIContainer(grid));
            redRow.Cells.Add(redCell);
            headerRG.Rows.Add(redRow);
            headerTable.RowGroups.Add(headerRG);
            doc.Blocks.Add(headerTable);

            // ── Title ──────────────────────────────────────────────────
            doc.Blocks.Add(new Paragraph
            {
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 8, 0, 4),
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(4, 6, 4, 6),
                Inlines = { new Run($"OSCA OVER-ALL TOTAL SENIOR CITIZEN 79 BARANGAY AS OF {currentMonthYear}") { FontSize = 10, FontWeight = FontWeights.Bold } }
            });

            // ── Data table ─────────────────────────────────────────────
            var table = new Table { CellSpacing = 0, BorderBrush = Brushes.Black, BorderThickness = new Thickness(1) };
            table.Columns.Add(new TableColumn { Width = new GridLength(noW) });
            table.Columns.Add(new TableColumn { Width = new GridLength(brgyW) });
            for (int i = 0; i < 12; i++) table.Columns.Add(new TableColumn { Width = new GridLength(colW) });

            var rg = new TableRowGroup();

            var hRow1 = new TableRow();
            hRow1.Cells.Add(MakeHeaderCell("NO.", 1, 2));
            hRow1.Cells.Add(MakeHeaderCell("BARANGAY", 1, 2));
            hRow1.Cells.Add(MakeHeaderCell("NATIONAL\nPENSIONER", 2, 1, Color.FromRgb(234, 179, 8)));
            hRow1.Cells.Add(MakeHeaderCell("LOCAL\nPENSIONER", 2, 1, Color.FromRgb(34, 197, 94)));
            hRow1.Cells.Add(MakeHeaderCell("WAITLIST", 2, 1, Color.FromRgb(252, 165, 165)));
            hRow1.Cells.Add(MakeHeaderCell("SSS", 2, 1, Color.FromRgb(134, 239, 172)));
            hRow1.Cells.Add(MakeHeaderCell("GSIS", 2, 1, Color.FromRgb(134, 239, 172)));
            hRow1.Cells.Add(MakeHeaderCell("TOTAL", 2, 1, Color.FromRgb(147, 197, 253)));
            rg.Rows.Add(hRow1);

            Color[] subColors = {
                Color.FromRgb(254,240,138), Color.FromRgb(254,240,138),
                Color.FromRgb(134,239,172), Color.FromRgb(134,239,172),
                Color.FromRgb(254,202,202), Color.FromRgb(254,202,202),
                Color.FromRgb(134,239,172), Color.FromRgb(134,239,172),
                Color.FromRgb(134,239,172), Color.FromRgb(134,239,172),
                Color.FromRgb(191,219,254), Color.FromRgb(191,219,254)
            };
            string[] subLabels = { "M", "F", "M", "F", "M", "F", "M", "F", "M", "F", "M", "F" };
            var hRow2 = new TableRow();
            for (int i = 0; i < 12; i++) hRow2.Cells.Add(MakeSubHeaderCell(subLabels[i], subColors[i]));
            rg.Rows.Add(hRow2);

            foreach (var item in _overAllData)
            {
                var dr = new TableRow();
                dr.Cells.Add(MakeDataCell(item.No ?? "", true));
                dr.Cells.Add(MakeDataCell(item.Barangay ?? "", true, TextAlignment.Left));
                dr.Cells.Add(MakeDataCell(item.NatMale ?? "0"));
                dr.Cells.Add(MakeDataCell(item.NatFemale ?? "0"));
                dr.Cells.Add(MakeDataCell(item.LocMale ?? "0"));
                dr.Cells.Add(MakeDataCell(item.LocFemale ?? "0"));
                dr.Cells.Add(MakeDataCell(item.WaitMale ?? "0"));
                dr.Cells.Add(MakeDataCell(item.WaitFemale ?? "0"));
                dr.Cells.Add(MakeDataCell(item.SssMale ?? "0"));
                dr.Cells.Add(MakeDataCell(item.SssFemale ?? "0"));
                dr.Cells.Add(MakeDataCell(item.GsisMale ?? "0"));
                dr.Cells.Add(MakeDataCell(item.GsisFemale ?? "0"));
                dr.Cells.Add(MakeDataCell(item.TotalMale ?? "0", true));
                dr.Cells.Add(MakeDataCell(item.TotalFemale ?? "0", true));
                rg.Rows.Add(dr);
            }

            var totRow = new TableRow { Background = new SolidColorBrush(Color.FromRgb(243, 244, 246)) };
            totRow.Cells.Add(MakeDataCell("", true));
            totRow.Cells.Add(MakeDataCell("TOTAL", true));
            totRow.Cells.Add(MakeDataCell(txtTotNatM.Text ?? "0", true));
            totRow.Cells.Add(MakeDataCell(txtTotNatF.Text ?? "0", true));
            totRow.Cells.Add(MakeDataCell(txtTotLocM.Text ?? "0", true));
            totRow.Cells.Add(MakeDataCell(txtTotLocF.Text ?? "0", true));
            totRow.Cells.Add(MakeDataCell(txtTotWaitM.Text ?? "0", true));
            totRow.Cells.Add(MakeDataCell(txtTotWaitF.Text ?? "0", true));
            totRow.Cells.Add(MakeDataCell(txtTotSssM.Text ?? "0", true));
            totRow.Cells.Add(MakeDataCell(txtTotSssF.Text ?? "0", true));
            totRow.Cells.Add(MakeDataCell(txtTotGsisM.Text ?? "0", true));
            totRow.Cells.Add(MakeDataCell(txtTotGsisF.Text ?? "0", true));
            totRow.Cells.Add(MakeDataCell(txtTotTotM.Text ?? "0", true));
            totRow.Cells.Add(MakeDataCell(txtTotTotF.Text ?? "0", true));
            rg.Rows.Add(totRow);

            table.RowGroups.Add(rg);
            doc.Blocks.Add(table);

            // ── Grand-total colour bars ────────────────────────────────
            double gtBarW = colW * 2;
            double gtLeadW = noW + brgyW;
            var gtTable = new Table { CellSpacing = 0, BorderThickness = new Thickness(0), Margin = new Thickness(0, 3, 0, 0) };
            gtTable.Columns.Add(new TableColumn { Width = new GridLength(gtLeadW) });
            for (int i = 0; i < 6; i++) gtTable.Columns.Add(new TableColumn { Width = new GridLength(gtBarW) });
            var gtRG = new TableRowGroup();
            var gtRow = new TableRow();
            gtRow.Cells.Add(new TableCell(new Paragraph()) { BorderThickness = new Thickness(0) });
            AddColorCell(gtRow, txtGrandNat.Text, Color.FromRgb(234, 179, 8));
            AddColorCell(gtRow, txtGrandLoc.Text, Color.FromRgb(34, 197, 94));
            AddColorCell(gtRow, txtGrandWait.Text, Color.FromRgb(252, 165, 165));
            AddColorCell(gtRow, txtGrandSss.Text, Color.FromRgb(134, 239, 172));
            AddColorCell(gtRow, txtGrandGsis.Text, Color.FromRgb(134, 239, 172));
            AddColorCell(gtRow, txtGrandTotal.Text, Color.FromRgb(147, 197, 253));
            gtRG.Rows.Add(gtRow);
            gtTable.RowGroups.Add(gtRG);
            doc.Blocks.Add(gtTable);

            // ── Signatures: PREPARED BY | NOTED BY ────────────────────
            doc.Blocks.Add(new Paragraph { Margin = new Thickness(0, 10, 0, 0), FontSize = 1 });
            BuildTwoColSignatures(doc, usableW, showCheckedBy: false);

            // ── GOLD footer ────────────────────────────────────────────
            BuildGoldFooter(doc, usableW);
        }

        // ═══════════════════════════════════════════
        // PRINT — BARANGAY LIST
        //  Signatures: PREPARED BY | CHECKED BY | NOTED BY
        // ═══════════════════════════════════════════
        private void BuildBarangayListDocument(FlowDocument doc, double usableW = 1030)
        {
            string brgy = GetFilterBarangay();
            string asOf = GetFilterAsOf();

            double[] widths = { 30, 70, 70, 60, 30, 30, 55, 30, 35, 65, 55, 35, 30, 30, 30, 30, 35 };
            double tableW = widths.Sum();
            double centerM = Math.Max(0, (usableW - tableW) / 2);

            // Header
            var header = new Paragraph { TextAlignment = TextAlignment.Center, Margin = new Thickness(0, 0, 0, 6) };
            header.Inlines.Add(new Run("CSWD GINGOOG CITY") { FontSize = 14, FontWeight = FontWeights.Bold });
            header.Inlines.Add(new LineBreak());
            header.Inlines.Add(new Run("Barangay List Report") { FontSize = 11, FontWeight = FontWeights.SemiBold });
            doc.Blocks.Add(header);

            // Filter label
            var filterPara = new Paragraph
            {
                TextAlignment = TextAlignment.Left,
                Margin = new Thickness(centerM, 0, centerM, 8),
                Padding = new Thickness(10, 6, 10, 6),
                Background = new SolidColorBrush(Color.FromRgb(240, 249, 255)),
                BorderThickness = new Thickness(0)
            };
            filterPara.Inlines.Add(new Run("LIST OF SENIOR CITIZEN'S BARANGAY: ")
            { FontSize = 9, FontWeight = FontWeights.SemiBold, Foreground = new SolidColorBrush(Color.FromRgb(71, 85, 105)) });
            filterPara.Inlines.Add(new Run(brgy)
            { FontSize = 9, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(15, 23, 42)) });
            filterPara.Inlines.Add(new Run("     AS OF: ")
            { FontSize = 9, FontWeight = FontWeights.SemiBold, Foreground = new SolidColorBrush(Color.FromRgb(71, 85, 105)) });
            filterPara.Inlines.Add(new Run(asOf)
            { FontSize = 9, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(15, 23, 42)) });
            doc.Blocks.Add(filterPara);

            // Data table
            string[] headers = { "NO", "LAST NAME", "FIRST NAME", "MIDDLE", "SFX", "SEX", "BIRTH DATE", "AGE", "STATUS", "OSCA ID", "ISSUED", "BLOOD", "LGU", "DSWD", "SSS", "GSIS", "WAIT" };
            var table = new Table
            {
                CellSpacing = 0,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0.5),
                Margin = new Thickness(centerM, 0, centerM, 0)
            };
            foreach (var w in widths) table.Columns.Add(new TableColumn { Width = new GridLength(w) });
            var rg = new TableRowGroup();
            var hr = new TableRow { Background = new SolidColorBrush(Color.FromRgb(248, 250, 252)) };
            foreach (var h in headers) hr.Cells.Add(MakeHeaderCell(h));
            rg.Rows.Add(hr);

            foreach (var item in _barangayData)
            {
                var dr = new TableRow();
                dr.Cells.Add(MakeDataCell(item.No ?? ""));
                dr.Cells.Add(MakeDataCell(item.LastName ?? "", false, TextAlignment.Left));
                dr.Cells.Add(MakeDataCell(item.FirstName ?? "", false, TextAlignment.Left));
                dr.Cells.Add(MakeDataCell(item.MiddleName ?? "", false, TextAlignment.Left));
                dr.Cells.Add(MakeDataCell(item.Suffix ?? ""));
                dr.Cells.Add(MakeDataCell(item.Sex ?? ""));
                dr.Cells.Add(MakeDataCell(item.BirthDate ?? ""));
                dr.Cells.Add(MakeDataCell(item.Age ?? ""));
                dr.Cells.Add(MakeDataCell(item.Status ?? ""));
                dr.Cells.Add(MakeDataCell(item.OscaId ?? ""));
                dr.Cells.Add(MakeDataCell(item.DateIssued ?? ""));
                dr.Cells.Add(MakeDataCell(item.BloodType ?? ""));
                dr.Cells.Add(MakeDataCell(item.Lgu ?? ""));
                dr.Cells.Add(MakeDataCell(item.Dswd ?? ""));
                dr.Cells.Add(MakeDataCell(item.Sss ?? ""));
                dr.Cells.Add(MakeDataCell(item.Gsis ?? ""));
                dr.Cells.Add(MakeDataCell(item.Waitlist ?? ""));
                rg.Rows.Add(dr);
            }
            table.RowGroups.Add(rg);
            doc.Blocks.Add(table);

            // Footer record count
            doc.Blocks.Add(new Paragraph
            {
                TextAlignment = TextAlignment.Right,
                Margin = new Thickness(centerM, 6, centerM, 0),
                Inlines = { new Run($"Total Records: {_barangayData.Count} | Generated: {DateTime.Now:MM/dd/yyyy}") { FontSize = 8, Foreground = Brushes.Gray } }
            });

            // ── THREE-COLUMN SIGNATURE BLOCK ──────────────────────────
            // PREPARED BY  |  CHECKED BY  |  NOTED BY
            doc.Blocks.Add(new Paragraph { Margin = new Thickness(0, 14, 0, 0), FontSize = 1 });
            BuildThreeColSignatures(doc, usableW);
        }

        // ═══════════════════════════════════════════
        // PRINT — PENSION
        //  Signatures: PREPARED BY | NOTED BY  (no Checked By)
        // ═══════════════════════════════════════════
        private void BuildPensionDocument(FlowDocument doc, double usableW = 1030)
        {
            string title = _activePension.ToString();
            var items = new List<object>();
            string totalText = "";

            if (_activePension == PensionTab.Quarterly) { items = _quarterlyData.Cast<object>().ToList(); totalText = txtQuarterlyTotal.Text; }
            else if (_activePension == PensionTab.AICS) { items = _aicsData.Cast<object>().ToList(); totalText = txtAICSTotal.Text; }
            else if (_activePension == PensionTab.APR) { items = _aprData.Cast<object>().ToList(); totalText = txtAPRTotal.Text; }
            else if (_activePension == PensionTab.Bereaved) { items = _bereavdData.Cast<object>().ToList(); totalText = txtBereavedTotal.Text; }

            var header = new Paragraph { TextAlignment = TextAlignment.Center, Margin = new Thickness(0, 0, 0, 10) };
            header.Inlines.Add(new Run("CSWD GINGOOG CITY") { FontSize = 14, FontWeight = FontWeights.Bold });
            header.Inlines.Add(new LineBreak());
            header.Inlines.Add(new Run($"{title} Report") { FontSize = 11, FontWeight = FontWeights.SemiBold });
            header.Inlines.Add(new LineBreak());
            header.Inlines.Add(new Run($"Generated: {DateTime.Now:MMMM dd, yyyy}") { FontSize = 9, Foreground = Brushes.Gray });
            doc.Blocks.Add(header);

            if (items == null || items.Count == 0)
            {
                doc.Blocks.Add(new Paragraph(new Run("No records found.")));
                BuildTwoColSignatures(doc, usableW, showCheckedBy: false);
                return;
            }

            if (_activePension == PensionTab.Bereaved)
            {
                double[] widths = { 35, 70, 90, 90, 60, 110, 80, 70, 90 };
                double centerM = Math.Max(0, (usableW - widths.Sum()) / 2);
                string[] hdr = { "NO", "DATE", "FIRST NAME", "LAST NAME", "BRGY", "RECIPIENT", "RELATIONSHIP", "AMOUNT", "REMARKS" };

                var table = new Table { CellSpacing = 0, BorderBrush = Brushes.Black, BorderThickness = new Thickness(0.5), Margin = new Thickness(centerM, 0, centerM, 0) };
                foreach (var w in widths) table.Columns.Add(new TableColumn { Width = new GridLength(w) });
                var rg = new TableRowGroup();
                var hr = new TableRow { Background = new SolidColorBrush(Color.FromRgb(248, 250, 252)) };
                foreach (var h in hdr) hr.Cells.Add(MakeHeaderCell(h));
                rg.Rows.Add(hr);

                foreach (BereavedItem item in items)
                {
                    var dr = new TableRow();
                    dr.Cells.Add(MakeDataCell(item.No ?? ""));
                    dr.Cells.Add(MakeDataCell(item.Date ?? ""));
                    dr.Cells.Add(MakeDataCell(item.FirstName ?? "", false, TextAlignment.Left));
                    dr.Cells.Add(MakeDataCell(item.LastName ?? "", false, TextAlignment.Left));
                    dr.Cells.Add(MakeDataCell(item.Brgy ?? ""));
                    dr.Cells.Add(MakeDataCell(item.RecipientName ?? "", false, TextAlignment.Left));
                    dr.Cells.Add(MakeDataCell(item.Relationship ?? ""));
                    dr.Cells.Add(MakeDataCell(item.Amount ?? ""));
                    dr.Cells.Add(MakeDataCell(item.Remarks ?? "", false, TextAlignment.Left));
                    rg.Rows.Add(dr);
                }
                var tr = new TableRow { Background = new SolidColorBrush(Color.FromRgb(240, 253, 244)) };
                for (int i = 0; i < 7; i++) tr.Cells.Add(MakeDataCell(""));
                tr.Cells.Add(MakeDataCell(totalText, true));
                tr.Cells.Add(MakeDataCell(""));
                rg.Rows.Add(tr);

                table.RowGroups.Add(rg);
                doc.Blocks.Add(table);
            }
            else
            {
                double[] widths = { 40, 80, 120, 120, 80, 90 };
                double centerM = Math.Max(0, (usableW - widths.Sum()) / 2);
                string[] hdr = { "NO", "DATE", "FIRST NAME", "LAST NAME", "BRGY", "AMOUNT" };

                var table = new Table { CellSpacing = 0, BorderBrush = Brushes.Black, BorderThickness = new Thickness(0.5), Margin = new Thickness(centerM, 0, centerM, 0) };
                foreach (var w in widths) table.Columns.Add(new TableColumn { Width = new GridLength(w) });
                var rg = new TableRowGroup();
                var hr = new TableRow { Background = new SolidColorBrush(Color.FromRgb(248, 250, 252)) };
                foreach (var h in hdr) hr.Cells.Add(MakeHeaderCell(h));
                rg.Rows.Add(hr);

                foreach (PensionItem item in items)
                {
                    var dr = new TableRow();
                    dr.Cells.Add(MakeDataCell(item.No ?? ""));
                    dr.Cells.Add(MakeDataCell(item.Date ?? ""));
                    dr.Cells.Add(MakeDataCell(item.FirstName ?? "", false, TextAlignment.Left));
                    dr.Cells.Add(MakeDataCell(item.LastName ?? "", false, TextAlignment.Left));
                    dr.Cells.Add(MakeDataCell(item.Brgy ?? ""));
                    dr.Cells.Add(MakeDataCell(item.Amount ?? ""));
                    rg.Rows.Add(dr);
                }
                var tr = new TableRow { Background = new SolidColorBrush(Color.FromRgb(240, 253, 244)) };
                for (int i = 0; i < 4; i++) tr.Cells.Add(MakeDataCell(""));
                tr.Cells.Add(MakeDataCell("TOTAL", true));
                tr.Cells.Add(MakeDataCell(totalText, true));
                rg.Rows.Add(tr);

                table.RowGroups.Add(rg);
                doc.Blocks.Add(table);
            }

            // ── TWO-COLUMN SIGNATURE (Prepared By | Noted By) ─────────
            doc.Blocks.Add(new Paragraph { Margin = new Thickness(0, 14, 0, 0), FontSize = 1 });
            BuildTwoColSignatures(doc, usableW, showCheckedBy: false);
        }

        // ═══════════════════════════════════════════
        // SIGNATURE HELPERS
        // ═══════════════════════════════════════════

        /// <summary>
        /// Barangay List: PREPARED BY | CHECKED BY (Admin) | NOTED BY
        /// </summary>
        private void BuildThreeColSignatures(FlowDocument doc, double usableW)
        {
            double colW = usableW / 3;
            var t = new Table { CellSpacing = 0, BorderThickness = new Thickness(0) };
            t.Columns.Add(new TableColumn { Width = new GridLength(colW) });
            t.Columns.Add(new TableColumn { Width = new GridLength(colW) });
            t.Columns.Add(new TableColumn { Width = new GridLength(colW) });

            var rg = new TableRowGroup();
            var row = new TableRow();

            // PREPARED BY (Left — encoder name from session)
            row.Cells.Add(BuildSigCell(
                label: "PREPARED BY:",
                name: EncoderName,
                title: "ENCODER",
                align: TextAlignment.Left));

            // CHECKED BY (Center — Admin)
            row.Cells.Add(BuildSigCell(
                label: "CHECKED BY:",
                name: "REBECCA A. REYES",
                title: "ADMIN AIDE II",
                align: TextAlignment.Center));

            // NOTED BY (Right)
            row.Cells.Add(BuildSigCell(
                label: "NOTED BY:",
                name: "DAISY JANE R. ACERO, RSW",
                title: "SWO-III/ SC-Focal Person",
                align: TextAlignment.Right));

            rg.Rows.Add(row);
            t.RowGroups.Add(rg);
            doc.Blocks.Add(t);
        }

        /// <summary>
        /// Overall SC / Pension: PREPARED BY | NOTED BY
        /// Pass showCheckedBy = false for pension / overall SC.
        /// </summary>
        private void BuildTwoColSignatures(FlowDocument doc, double usableW,
                                            bool showCheckedBy = false)
        {
            double colW = usableW / 2;
            var t = new Table { CellSpacing = 0, BorderThickness = new Thickness(0) };
            t.Columns.Add(new TableColumn { Width = new GridLength(colW) });
            t.Columns.Add(new TableColumn { Width = new GridLength(colW) });

            var rg = new TableRowGroup();
            var row = new TableRow();

            row.Cells.Add(BuildSigCell("PREPARED BY:", EncoderName, "SENIOR CITIZENS STAFF ENCODER", TextAlignment.Left));
            row.Cells.Add(BuildSigCell("NOTED BY:", "DAISY JANE R. ACERO, RSW",
                                        "SWO-III/ SC-Focal Person", TextAlignment.Right));

            rg.Rows.Add(row);
            t.RowGroups.Add(rg);
            doc.Blocks.Add(t);
        }

        /// <summary>Builds one signature cell (label + blank line + underlined name + title).</summary>
        private TableCell BuildSigCell(string label, string name, string title,
                                        TextAlignment align)
        {
            var cell = new TableCell { BorderThickness = new Thickness(0), Padding = new Thickness(4, 4, 4, 4) };
            var p = new Paragraph { TextAlignment = align, Margin = new Thickness(0) };
            p.Inlines.Add(new Run(label) { FontSize = 8, FontWeight = FontWeights.Bold, Foreground = Brushes.Gray });
            p.Inlines.Add(new LineBreak());
            p.Inlines.Add(new Run(" ") { FontSize = 14 }); // blank line space
            p.Inlines.Add(new LineBreak());
            p.Inlines.Add(new Run(name) { FontWeight = FontWeights.Bold, FontSize = 10, TextDecorations = TextDecorations.Underline });
            p.Inlines.Add(new LineBreak());
            p.Inlines.Add(new Run(title) { FontSize = 8, Foreground = Brushes.Gray });
            cell.Blocks.Add(p);
            return cell;
        }

        // ═══════════════════════════════════════════
        // GOLD FOOTER (Overall SC only)
        // ═══════════════════════════════════════════
        private void BuildGoldFooter(FlowDocument doc, double usableW)
        {
            doc.Blocks.Add(new Paragraph { Margin = new Thickness(0, 8, 0, 0), FontSize = 1 });

            double footLeftW = usableW * 0.65;
            double footRightW = usableW - footLeftW;

            var footTable = new Table { CellSpacing = 0, BorderThickness = new Thickness(0) };
            footTable.Columns.Add(new TableColumn { Width = new GridLength(footLeftW) });
            footTable.Columns.Add(new TableColumn { Width = new GridLength(footRightW) });
            var footRG = new TableRowGroup();
            var footRow = new TableRow();

            var goldCell = new TableCell
            {
                Background = new SolidColorBrush(Color.FromRgb(59, 75, 140)),
                Padding = new Thickness(10, 6, 10, 6),
                BorderThickness = new Thickness(0)
            };
            var goldPara = new Paragraph { FontSize = 7.5, Foreground = Brushes.White, LineHeight = 14, Margin = new Thickness(0) };
            void AddLetter(string letter, Color bg, string text)
            {
                goldPara.Inlines.Add(new Run(letter) { FontWeight = FontWeights.Bold, Background = new SolidColorBrush(bg), Foreground = letter == "G" ? new SolidColorBrush(Color.FromRgb(28, 25, 23)) : Brushes.White });
                goldPara.Inlines.Add(new Run($"  {text}"));
                goldPara.Inlines.Add(new LineBreak());
            }
            AddLetter("G", Color.FromRgb(234, 179, 8), "GOOD GOVERNANCE, TRANSPARENCY AND ACCOUNTABILITY");
            AddLetter("O", Color.FromRgb(249, 115, 22), "OPPORTUNITIES FOR AGRICULTURE AND TOURISM DEVELOPMENT");
            AddLetter("L", Color.FromRgb(34, 197, 94), "LIVELIHOOD DEVELOPMENT AND IMPLEMENTATION");
            AddLetter("D", Color.FromRgb(239, 68, 68), "DELIVERY OF HEALTH, EDUCATION AND SOCIAL SERVICES");
            goldCell.Blocks.Add(goldPara);
            footRow.Cells.Add(goldCell);

            var tagCell = new TableCell
            {
                Background = new SolidColorBrush(Color.FromRgb(185, 28, 28)),
                Padding = new Thickness(12, 6, 12, 6),
                BorderThickness = new Thickness(0)
            };
            var tagPara = new Paragraph { TextAlignment = TextAlignment.Right, Margin = new Thickness(0) };
            tagPara.Inlines.Add(new Run("Together, we ") { FontSize = 11, FontWeight = FontWeights.Bold, Foreground = Brushes.White });
            tagPara.Inlines.Add(new Run("C") { FontSize = 11, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(239, 68, 68)) });
            tagPara.Inlines.Add(new Run("an Unite") { FontSize = 11, FontWeight = FontWeights.Bold, Foreground = Brushes.White });
            tagPara.Inlines.Add(new LineBreak());
            tagPara.Inlines.Add(new Run("CSWDgingoogcity@gmail.com") { FontSize = 7, Foreground = new SolidColorBrush(Color.FromRgb(252, 165, 165)) });
            tagPara.Inlines.Add(new LineBreak());
            tagPara.Inlines.Add(new Run("Tel. # 861-3163") { FontSize = 7, Foreground = new SolidColorBrush(Color.FromRgb(252, 165, 165)) });
            tagCell.Blocks.Add(tagPara);
            footRow.Cells.Add(tagCell);

            footRG.Rows.Add(footRow);
            footTable.RowGroups.Add(footRG);
            doc.Blocks.Add(footTable);
        }

        // ═══════════════════════════════════════════
        // TABLE CELL HELPERS
        // ═══════════════════════════════════════════
        private TableCell MakeHeaderCell(string text, int colSpan = 1, int rowSpan = 1,
                                          Color? bgColor = null)
        {
            var para = new Paragraph { TextAlignment = TextAlignment.Center, Margin = new Thickness(1), FontSize = 7.5, LineHeight = 10 };
            para.Inlines.Add(new Run(text ?? "") { FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(17, 24, 39)) });
            return new TableCell(para)
            {
                ColumnSpan = colSpan,
                RowSpan = rowSpan,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0.5),
                Padding = new Thickness(2, 3, 2, 3),
                Background = bgColor.HasValue
                    ? new SolidColorBrush(bgColor.Value)
                    : new SolidColorBrush(Color.FromRgb(248, 250, 252))
            };
        }

        private TableCell MakeSubHeaderCell(string text, Color bgColor)
        {
            var para = new Paragraph { TextAlignment = TextAlignment.Center, Margin = new Thickness(1), FontSize = 7 };
            para.Inlines.Add(new Run(text ?? "") { FontWeight = FontWeights.Bold });
            return new TableCell(para)
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0.5),
                Padding = new Thickness(2, 2, 2, 2),
                Background = new SolidColorBrush(bgColor)
            };
        }

        private TableCell MakeDataCell(string text, bool bold = false,
                                        TextAlignment align = TextAlignment.Center)
        {
            var para = new Paragraph { TextAlignment = align, Margin = new Thickness(1), FontSize = 7.5 };
            var run = new Run(text ?? "");
            if (bold) run.FontWeight = FontWeights.Bold;
            para.Inlines.Add(run);
            return new TableCell(para)
            {
                BorderBrush = new SolidColorBrush(Color.FromRgb(229, 231, 235)),
                BorderThickness = new Thickness(0.5),
                Padding = new Thickness(2, 2, 2, 2)
            };
        }

        private void AddColorCell(TableRow row, string text, Color bgColor)
        {
            var cell = new TableCell
            {
                Background = new SolidColorBrush(bgColor),
                BorderThickness = new Thickness(0.5),
                BorderBrush = Brushes.White,
                Padding = new Thickness(2, 4, 2, 4)
            };
            cell.Blocks.Add(new Paragraph(
                new Run(text ?? "0") { FontWeight = FontWeights.Bold, FontSize = 9 })
            { TextAlignment = TextAlignment.Center, Margin = new Thickness(0) });
            row.Cells.Add(cell);
        }

        // ═══════════════════════════════════════════
        // UTILITY
        // ═══════════════════════════════════════════
        private static string TotalStr(List<PensionItem> items)
            => items.Sum(d => ParseDec(d.Amount)).ToString("N0");
        private static string BereavedTotalStr(List<BereavedItem> items)
            => items.Sum(d => ParseDec(d.Amount)).ToString("N0");
        private static decimal ParseDec(string s)
        { decimal.TryParse((s ?? "0").Replace(",", ""), out decimal v); return v; }
        private static int ParseInt(string s)
        { int.TryParse((s ?? "0").Replace(",", ""), out int v); return v; }
        private static string GrandTotal(List<OverAllSCItem> data,
            Func<OverAllSCItem, string> m, Func<OverAllSCItem, string> f)
            => (data.Sum(d => ParseInt(m(d))) + data.Sum(d => ParseInt(f(d)))).ToString();
        private static string E(string s)
            => string.IsNullOrEmpty(s) ? "" :
               (s.Contains(",") || s.Contains("\"")) ? $"\"{s.Replace("\"", "\"\"")}\"" : s;
        private string GetActiveTabName()
            => _activeMain == MainTab.BarangayList ? "Barangay_List"
             : _activeMain == MainTab.OverAllSC ? "OverAll_SC"
             : _activePension.ToString();
        private void SetExportStatus(string msg)
        { if (TxtExportStatus != null) TxtExportStatus.Text = msg; }

        // ═══════════════════════════════════════════
        // NAVIGATION
        // ═══════════════════════════════════════════
        private void BtnDashboard_Click(object sender, RoutedEventArgs e)
        { new Staffdashboard().Show(); Close(); }
        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        { new Staffregister().Show(); Close(); }
        private void BtnAICS_Click(object sender, RoutedEventArgs e)
        { new Staffaab().Show(); Close(); }
        private void BtnPensionList_Click(object sender, RoutedEventArgs e)
        { new Staffpensionlist().Show(); Close(); }
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Logout?", "Logout",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            { AppSession.UserId = ""; AppSession.FullName = ""; AppSession.Role = ""; new MainWindow().Show(); Close(); }
        }
    }
}