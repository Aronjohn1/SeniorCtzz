using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static SeniorCtzz.Models;

namespace SeniorCtzz
{
    public partial class Adminmanagestaff : Window
    {
        private ObservableCollection<StaffItem> _staffList = new();
        private string _editingEmpId = "";

        public Adminmanagestaff()
        {
            InitializeComponent();
            SetUserInfo();
            LoadStaffData();
        }

        private void SetUserInfo()
        {
            txtUserName.Text = AppSession.FullName;
            txtInitials.Text = AppSession.GetInitials();
            txtUserRole.Text = AppSession.Role;
        }

        private void LoadStaffData()
        {
            var allUsers = StaffDB.GetAll();

            _staffList = new ObservableCollection<StaffItem>(
                allUsers.Where(u => u.Role != "Section Head"));
            dgStaff.ItemsSource = _staffList;
            UpdateStaffCount();
        }

        private void UpdateStaffCount()
        {
            txtStaffCount.Text = _staffList.Count == 1
                ? "1 account"
                : $"{_staffList.Count} accounts";
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string fullName = TxtFullName.Text.Trim();
            string empId = TxtID.Text.Trim();
            string password = TxtPassword.Text.Trim();   

            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(empId) ||
                string.IsNullOrWhiteSpace(password))
            {
                ShowMessage("All fields are required.", isError: true);
                return;
            }

            if (string.IsNullOrEmpty(_editingEmpId))
            {
                bool saved = StaffDB.Add(fullName, empId, password);
                if (!saved) return;
                ShowMessage("Staff account added successfully.", isError: false);
            }
            else
            {
                bool updated = StaffDB.Update(_editingEmpId, fullName, password);
                if (!updated) return;
                ShowMessage("Staff account updated.", isError: false);
                _editingEmpId = "";
                BtnSaveStaff.Content = "Save";
            }

            ClearForm();
            LoadStaffData();
        }

        private void BtnEditStaff_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is not string staffId) return;

            var item = _staffList.FirstOrDefault(s => s.StaffId == staffId);
            if (item == null) return;

            TxtFullName.Text = item.FullName ?? "";
            TxtID.Text = item.StaffId ?? "";
            TxtPassword.Text = item.Password ?? "";   

            _editingEmpId = staffId;
            BtnSaveStaff.Content = "Update";
            HideMessage();
            TxtFullName.Focus();
        }

        private void BtnDeleteStaff_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is not string staffId) return;

            var item = _staffList.FirstOrDefault(s => s.StaffId == staffId);
            if (item == null) return;

            var result = MessageBox.Show(
                $"Delete account for \"{item.FullName}\"?\nThis action cannot be undone.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            bool deleted = StaffDB.Delete(staffId);
            if (!deleted) return;

            ShowMessage("Account deleted.", isError: false);

            if (_editingEmpId == staffId)
            {
                _editingEmpId = "";
                BtnSaveStaff.Content = "Save";
                ClearForm();
            }

            LoadStaffData();
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            _editingEmpId = "";
            BtnSaveStaff.Content = "Save";
            HideMessage();
        }

        private void ClearForm()
        {
            TxtFullName.Text = "";
            TxtID.Text = "";
            TxtPassword.Text = "";  
        }

        private void ShowMessage(string message, bool isError)
        {
            TxtMessage.Text = message;
            pnlMessage.Background = isError
                ? new SolidColorBrush(Color.FromRgb(255, 235, 235))
                : new SolidColorBrush(Color.FromRgb(232, 255, 237));
            TxtMessage.Foreground = isError
                ? new SolidColorBrush(Color.FromRgb(198, 40, 40))
                : new SolidColorBrush(Color.FromRgb(0, 130, 60));
            pnlMessage.Visibility = Visibility.Visible;
        }

        private void HideMessage() => pnlMessage.Visibility = Visibility.Collapsed;

        private void BtnDashboard_Click(object sender, RoutedEventArgs e)
        { new Admindashboard().Show(); this.Close(); }

        private void BtnPensionList_Click(object sender, RoutedEventArgs e)
        { new Adminpensionlist().Show(); this.Close(); }

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
