using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace SeniorCtzz
{
    public partial class Staffregister : Window
    {
        private Dictionary<string, string> _barangayClassification = new Dictionary<string, string>();
        private ObservableCollection<RecordItem> _allRecords = new ObservableCollection<RecordItem>();

        public Staffregister()
        {
            InitializeComponent();
            SetUserInfo();
            LoadBarangayClassifications();
            LoadBarangays();
            LoadAllRecords();
        }

        private void SetUserInfo()
        {
            TxtUserName.Text = AppSession.FullName;
            TxtAvatar.Text = AppSession.GetInitials();
        }

        private void LoadBarangayClassifications()
        {
            for (int i = 1; i <= 26; i++)
                _barangayClassification[$"Barangay {i}"] = "Urban";

            _barangayClassification["Barangay 18-A"] = "Urban";
            _barangayClassification["Barangay 22-A"] = "Urban";
            _barangayClassification["Barangay 24-A"] = "Urban";

            foreach (var b in new[] {
                "Agay-ayan","Anakan","Daan-Lungsod","Kalagonoy","Lawaan",
                "Libertad","Lunao","Pangasihan","Samay","San Juan",
                "San Luis","Santiago","Talisay" })
                _barangayClassification[b] = "Coastal";

            foreach (var b in new[] {
                "Alagatan","Bagubad","Bakidbakid","Bal-ason","Bantaawan",
                "Binakalan","Capitulangan","Dinawehan","Eureka","Hindangon",
                "Kalipay","Kamanikan","Kianlagan","Kibuging","Kipuntos",
                "Lawit","Libon","Lunotan","Malibud","Malinao","Maribucao",
                "Mimbalagon","Mimbunga","Mimbuntong","Minsapinit","Murallon",
                "Odiongan","Pigsaluhan","Punong","Ricoro","San Jose",
                "San Miguel","Sangalan","Tagpako","Talon","Tinabalan","Tinulongan" })
                _barangayClassification[b] = "Rural";
        }

        private string[] GetAllBarangays() => new string[] {
            "Agay-ayan","Alagatan","Anakan","Bagubad","Bakidbakid","Bal-ason","Bantaawan","Binakalan","Capitulangan","Daan-Lungsod",
            "Dinawehan","Eureka","Hindangon","Kalagonoy","Kalipay","Kamanikan","Kianlagan","Kibuging","Kipuntos","Lawaan",
            "Lawit","Libertad","Libon","Lunao","Lunotan","Malibud","Malinao","Maribucao","Mimbuntong","Mimbalagon",
            "Mimbunga","Minsapinit","Murallon","Odiongan","Pangasihan","Pigsaluhan","Punong","Ricoro","Samay","Sangalan",
            "San Jose","San Juan","San Luis","San Miguel","Santiago","Tagpako","Talisay","Talon","Tinabalan","Tinulongan",
            "Barangay 1","Barangay 2","Barangay 3","Barangay 4","Barangay 5","Barangay 6","Barangay 7","Barangay 8","Barangay 9","Barangay 10",
            "Barangay 11","Barangay 12","Barangay 13","Barangay 14","Barangay 15","Barangay 16","Barangay 17","Barangay 18","Barangay 18-A","Barangay 19",
            "Barangay 20","Barangay 21","Barangay 22","Barangay 22-A","Barangay 23","Barangay 24","Barangay 24-A","Barangay 25","Barangay 26"
        };

        private void LoadBarangays()
        {
            CmbBarangay.Items.Clear();
            foreach (var b in GetAllBarangays())
                CmbBarangay.Items.Add(b);
        }

 
        private void LoadAllRecords()
        {
            try
            {
                _allRecords.Clear();
                var seniors = SeniorDB.GetAllSeniors();
                if (seniors != null)
                {
                    foreach (var s in seniors)
                    {
                   
                        string pensionType = s.PensionType ?? "";
                        string assistSource = BuildAssistanceDisplay(s.Lgu, s.Dswd, s.Waitlist);

                    
                        if (string.IsNullOrEmpty(assistSource) && !string.IsNullOrEmpty(pensionType))
                        {
                            SplitPensionString(pensionType, out pensionType, out assistSource);
                        }

                        _allRecords.Add(new RecordItem
                        {
                            LastName = s.LastName ?? "",
                            FirstName = s.FirstName ?? "",
                            MiddleName = s.MiddleName ?? "",
                            Suffix = s.Suffix ?? "",
                            Sex = s.Sex ?? "",
                            BirthDate = s.BirthDate ?? "",
                            Age = s.Age ?? "",
                            CivilStatus = s.Status ?? "",
                            OscaId = s.OscaId ?? "",
                            DateIssued = s.DateIssued ?? "",
                            Barangay = s.Barangay ?? "",
                            BloodType = s.BloodType ?? "",
                            PensionType = pensionType,  
                            AssistanceSource = assistSource,  
                            RegStatus = "OSCA-Members"
                        });
                    }
                }
                DgRecords.ItemsSource = _allRecords;
                TxtRecordCount.Text = $"{_allRecords.Count} records";
            }
            catch
            {
                DgRecords.ItemsSource = _allRecords;
            }
        }

    
        private string BuildAssistanceDisplay(string lgu, string dswd, string waitlist)
        {
            var pts = new List<string>();
            if (lgu == "✔") pts.Add("LGU");
            if (dswd == "✔") pts.Add("DSWD");
            if (waitlist == "✔") pts.Add("WAITLIST");
            return string.Join(", ", pts);
        }

    
        private static void SplitPensionString(string combined,
            out string pensionType, out string assistanceSource)
        {
            var pension = new List<string>();
            var assist = new List<string>();
            var pensionSet = new HashSet<string> { "SSS", "GSIS" };
            var assistSet = new HashSet<string> { "LGU", "DSWD", "WAITLIST" };

            foreach (var part in combined.Split(new[] { ',', ';', '/' },
                         StringSplitOptions.RemoveEmptyEntries))
            {
                string token = part.Trim().ToUpper();
                if (pensionSet.Contains(token) && !pension.Contains(token))
                    pension.Add(token);
                else if (assistSet.Contains(token) && !assist.Contains(token))
                    assist.Add(token);
            }

            pensionType = string.Join(", ", pension);
            assistanceSource = string.Join(", ", assist);
        }

        private void RefreshRecordsList() => LoadAllRecords();

        private void TxtSearchRecords_TextChanged(object sender, TextChangedEventArgs e)
        {
            string kw = TxtSearchRecords.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(kw))
            {
                DgRecords.ItemsSource = _allRecords;
            }
            else
            {
                DgRecords.ItemsSource = new ObservableCollection<RecordItem>(
                    _allRecords.Where(r =>
                        (r.LastName?.ToLower().Contains(kw) ?? false) ||
                        (r.FirstName?.ToLower().Contains(kw) ?? false) ||
                        (r.OscaId?.ToLower().Contains(kw) ?? false) ||
                        (r.Barangay?.ToLower().Contains(kw) ?? false)));
            }
            TxtRecordCount.Text = $"{(DgRecords.ItemsSource as ObservableCollection<RecordItem>)?.Count ?? 0} records";
        }

        private void DpBirthDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!DpBirthDate.SelectedDate.HasValue) return;
            DateTime birth = DpBirthDate.SelectedDate.Value;
            int age = DateTime.Today.Year - birth.Year;
            if (birth.Date > DateTime.Today.AddYears(-age)) age--;
            TxtAge.Text = age.ToString();
            if (age < 60) ShowError("Senior citizen must be at least 60 years old.");
            else HideError();
        }

   
        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(TxtLastName.Text))
            { ShowError("Last Name is required."); return false; }
            if (string.IsNullOrWhiteSpace(TxtFirstName.Text))
            { ShowError("First Name is required."); return false; }
            if (CmbSex.SelectedItem == null)
            { ShowError("Sex is required."); return false; }
            if (!DpBirthDate.SelectedDate.HasValue)
            { ShowError("Birth Date is required."); return false; }
            if (string.IsNullOrWhiteSpace(TxtOscaId.Text))
            { ShowError("OSCA I.D. is required."); return false; }
            if (!DpDateIssued.SelectedDate.HasValue)
            { ShowError("Date of Issued is required."); return false; }
            if (CmbBarangay.SelectedItem == null)
            { ShowError("Barangay is required."); return false; }

            // At least one from PENSION TYPE or ASSISTANCE SOURCE must be selected
            bool hasPension = ChkSSS.IsChecked.GetValueOrDefault() ||
                              ChkGSIS.IsChecked.GetValueOrDefault();
            bool hasAssist = ChkLGU.IsChecked.GetValueOrDefault() ||
                              ChkDSWD.IsChecked.GetValueOrDefault() ||
                              ChkWaitlist.IsChecked.GetValueOrDefault();

            if (!hasPension && !hasAssist)
            {
                ShowError("Please select at least one Pension Type or Assistance Source.");
                return false;
            }

            if (!int.TryParse(TxtAge.Text, out int age) || age < 60)
            { ShowError("Senior citizen must be at least 60 years old."); return false; }

            HideError();
            return true;
        }

     


        private string GetPensionType()
        {
            var t = new List<string>();
            if (ChkSSS.IsChecked == true) t.Add("SSS");
            if (ChkGSIS.IsChecked == true) t.Add("GSIS");
            return string.Join(",", t);
        }

     
        private string GetAssistanceSource()
        {
            var t = new List<string>();
            if (ChkLGU.IsChecked == true) t.Add("LGU");
            if (ChkDSWD.IsChecked == true) t.Add("DSWD");
            if (ChkWaitlist.IsChecked == true) t.Add("WAITLIST");
            return string.Join(",", t);
        }

        private string GetSelectedBarangay() => CmbBarangay.SelectedItem?.ToString() ?? "";
        private string GetBloodType() => (CmbBloodType.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
        private string GetResidency(string b) =>
            _barangayClassification.ContainsKey(b) ? _barangayClassification[b] : "";


        private void BtnReviewConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm()) return;

            string ln = TxtLastName.Text.Trim();
            string fn = TxtFirstName.Text.Trim();
            string mn = TxtMiddleName.Text.Trim();
            string sfx = (CmbSuffix.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
            string sex = (CmbSex.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";

        
            DateTime bdt = DpBirthDate.SelectedDate ?? DateTime.Today;
            string bd = bdt.ToString("MM-dd-yy");

            string age = TxtAge.Text;
            string status = (CmbStatus.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
            string osca = TxtOscaId.Text.Trim();

          
            DateTime dit = DpDateIssued.SelectedDate ?? DateTime.Today;
            string di = dit.ToString("MM-dd-yy");

            string brgy = GetSelectedBarangay();
            string bt = GetBloodType();
            string res = GetResidency(brgy);

            string pensionType = GetPensionType();
            string assistSource = GetAssistanceSource();

            var confirm = new ConfirmationRegister(
                ln, fn, mn, sfx, sex, bd, age, status,
                osca, di, brgy, pensionType, assistSource, bt, res);
            confirm.Owner = this;
            confirm.ShowDialog();

            if (confirm.IsConfirmed)
            {
                int.TryParse(age, out int a);

                if (SeniorDB.Save(ln, fn, mn, sfx, sex, bdt, a, status,
                                  osca, dit, brgy,
                                  pensionType,
                                  bt, res,
                                  AppSession.UserId,
                                  assistSource))
                {
                    MessageBox.Show($"{fn} {ln} registered successfully!",
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    ClearForm();
                    RefreshRecordsList();
                }
            }
        }
  
        private void BtnImport_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv",
                Title = "Import Senior Citizens"
            };
            if (dlg.ShowDialog() != true) return;

            try
            {
                string[] lines = File.ReadAllLines(dlg.FileName, Encoding.UTF8);
                if (lines.Length < 2) { MessageBox.Show("File is empty."); return; }

                string[] headers = lines[0].Split(',').Select(h => h.Trim()).ToArray();
                var map = MapHeaders(headers);
                int success = 0, failed = 0;

                for (int i = 1; i < lines.Length; i++)
                {
                    string[] vals = ParseCsvLine(lines[i]);
                    if (vals.Length < headers.Length) { failed++; continue; }

                    string ln = GetValue(map, vals, "lastname");
                    string fn = GetValue(map, vals, "firstname");
                    if (string.IsNullOrWhiteSpace(ln) || string.IsNullOrWhiteSpace(fn))
                    { failed++; continue; }

                    string mn = GetValue(map, vals, "middlename");
                    string sfx = GetValue(map, vals, "suffix");
                    string sex = GetValue(map, vals, "sex");
                    sex = (!string.IsNullOrEmpty(sex) && sex.Length >= 1)
                        ? sex.Substring(0, 1).ToUpper() : "M";

                    string bdStr = GetValue(map, vals, "birthdate");
                    string ageStr = GetValue(map, vals, "age");
                    string stat = GetValue(map, vals, "status", "civilstatus");
                    if (string.IsNullOrEmpty(stat)) stat = "M";

                    string osca = GetValue(map, vals, "oscaid");
                    string diStr = GetValue(map, vals, "dateissued");
                    string brgy = GetValue(map, vals, "barangay");
                    if (string.IsNullOrEmpty(brgy)) brgy = GetSelectedBarangay();
                    string bt = GetValue(map, vals, "bloodtype");

              
                    string rawPension = GetValue(map, vals, "pensiontype", "pension_type");
                    if (string.IsNullOrWhiteSpace(rawPension))
                    {
                        foreach (var key in new[] { "pension", "typeofpension" })
                        {
                            rawPension = GetValue(map, vals, key);
                            if (!string.IsNullOrWhiteSpace(rawPension)) break;
                        }
                        if (string.IsNullOrWhiteSpace(rawPension))
                            foreach (var kvp in map)
                                if (kvp.Key.Contains("pension") && kvp.Value < vals.Length &&
                                    !string.IsNullOrWhiteSpace(vals[kvp.Value]))
                                { rawPension = vals[kvp.Value]; break; }
                    }

                   
                    string rawAssist = GetValue(map, vals, "assistancesource", "assistance_source");

                
                    string ptClean = SanitizePart(rawPension, new[] { "SSS", "GSIS" });
                    string asClean = SanitizePart(
                        string.IsNullOrEmpty(rawAssist) ? rawPension : rawAssist,
                        new[] { "LGU", "DSWD", "WAITLIST" });

                
                    if (string.IsNullOrEmpty(ptClean) && string.IsNullOrEmpty(asClean))
                    {
                        ptClean = SanitizePart(rawPension, new[] { "SSS", "GSIS" });
                        asClean = SanitizePart(rawPension, new[] { "LGU", "DSWD", "WAITLIST" });
                    }

                    DateTime.TryParse(bdStr, out DateTime bdt);
                    DateTime.TryParse(diStr, out DateTime dit);
                    int.TryParse(ageStr, out int age);
                    string res = GetResidency(brgy);

    
                    if (SeniorDB.Save(ln, fn, mn, sfx, sex, bdt, age, stat,
                                      osca, dit, brgy,
                                      ptClean,          
                                      bt, res,
                                      AppSession.UserId,
                                      asClean))         
                        success++;
                    else
                        failed++;
                }

                MessageBox.Show(
                    $"Import complete!\nSuccess: {success}\nFailed: {failed}",
                    "Import Result", MessageBoxButton.OK, MessageBoxImage.Information);
                TxtImportStatus.Text = $"Imported: {success} saved | {failed} skipped";
                RefreshRecordsList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Import error: {ex.Message}");
            }
        }

  
        private string SanitizePart(string input, string[] allowedTokens)
        {
            if (string.IsNullOrWhiteSpace(input)) return "";
            string upper = input.Trim().ToUpper();
            var detected = new List<string>();
            string[] parts = upper.Split(new[] { ',', ';', '/', '|', ' ', '-', '_' },
                                          StringSplitOptions.RemoveEmptyEntries);
            foreach (string part in parts)
            {
                string clean = part.Trim();
                if (allowedTokens.Contains(clean) && !detected.Contains(clean))
                    detected.Add(clean);
            }
   
            if (detected.Count == 0)
                foreach (var token in allowedTokens)
                    if (upper.Contains(token) && !detected.Contains(token))
                        detected.Add(token);

            return string.Join(",", detected);
        }

        private string GetValue(Dictionary<string, int> map, string[] vals,
                                string key1, string key2 = null)
        {
            if (map.ContainsKey(key1) && map[key1] < vals.Length)
                return vals[map[key1]].Trim();
            if (key2 != null && map.ContainsKey(key2) && map[key2] < vals.Length)
                return vals[map[key2]].Trim();
            return "";
        }

        private Dictionary<string, int> MapHeaders(string[] headers)
        {
            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < headers.Length; i++)
            {
                string h = headers[i].Trim().ToLower()
                    .Replace(" ", "").Replace("_", "").Replace("-", "").Replace(".", "");
                if (!map.ContainsKey(h)) map[h] = i;
            }
            return map;
        }

        private string[] ParseCsvLine(string line)
        {
            var r = new List<string>();
            bool q = false;
            var sb = new StringBuilder();
            foreach (char c in line)
            {
                if (c == '"') { q = !q; continue; }
                if (c == ',' && !q) { r.Add(sb.ToString().Trim()); sb.Clear(); continue; }
                sb.Append(c);
            }
            r.Add(sb.ToString().Trim());
            return r.ToArray();
        }

     
        private void ClearForm()
        {
            TxtLastName.Text = "";
            TxtFirstName.Text = "";
            TxtMiddleName.Text = "";
            CmbSuffix.SelectedIndex = 0;
            CmbSex.SelectedIndex = 0;
            DpBirthDate.SelectedDate = null;    
            TxtAge.Text = "";
            CmbStatus.SelectedIndex = 0;
            TxtOscaId.Text = "";
            DpDateIssued.SelectedDate = DateTime.Today; 
            CmbBarangay.SelectedIndex = -1;
            CmbBloodType.SelectedIndex = 0;
            TxtResidency.Text = "";

            ChkSSS.IsChecked = false;
            ChkGSIS.IsChecked = false;
            ChkLGU.IsChecked = false;
            ChkDSWD.IsChecked = false;
            ChkWaitlist.IsChecked = false;

            HideError();
        }
        private void ShowError(string msg)
        {
            if (!string.IsNullOrEmpty(msg))
            {
                TxtError.Text = msg;
                ErrorBorder.Visibility = Visibility.Visible;
                TxtError.Visibility = Visibility.Visible;
            }
        }

        private void HideError()
        {
            TxtError.Text = "";
            ErrorBorder.Visibility = Visibility.Collapsed;
            TxtError.Visibility = Visibility.Collapsed;
        }

        private void CmbBarangay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbBarangay.SelectedItem != null)
            {
                TxtResidency.Text = GetResidency(CmbBarangay.SelectedItem.ToString());
                HideError();
            }
        }

   
        private void BtnDashboard_Click(object sender, RoutedEventArgs e)
        { new Staffdashboard().Show(); this.Close(); }
        private void BtnRegister_Click(object sender, RoutedEventArgs e) { }
        private void BtnAAB_Click(object sender, RoutedEventArgs e)
        { new Staffaab().Show(); this.Close(); }
        private void BtnPensionList_Click(object sender, RoutedEventArgs e)
        { new Staffpensionlist().Show(); this.Close(); }
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


    public class RecordItem
    {
        public string LastName { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string MiddleName { get; set; } = "";
        public string Suffix { get; set; } = "";
        public string Sex { get; set; } = "";
        public string BirthDate { get; set; } = "";
        public string Age { get; set; } = "";
        public string CivilStatus { get; set; } = "";
        public string OscaId { get; set; } = "";
        public string DateIssued { get; set; } = "";
        public string Barangay { get; set; } = "";
        public string BloodType { get; set; } = "";
        public string PensionType { get; set; } = "";  
        public string AssistanceSource { get; set; } = "";  
        public string RegStatus { get; set; } = "OSCA Members";
    }
}
