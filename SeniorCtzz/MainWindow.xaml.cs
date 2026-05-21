using System.Windows;
using System.Windows.Controls;

namespace SeniorCtzz
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnSignIn_Click(object sender, RoutedEventArgs e)
        {
            string userId = TxtID.Text.Trim();
            string password = TxtPassword.Password.Trim();

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(password))
            {
                ShowError("Please enter your ID and password.");
                return;
            }

            // ── Authenticate — returns user_id string, "" if failed ───
            string loggedInId = StaffDB.Login(userId, password);

            if (string.IsNullOrEmpty(loggedInId))
            {
                ShowError("Invalid ID or password. Please try again.");
                return;
            }

            // ── Store session ─────────────────────────────────────────
            AppSession.UserId = loggedInId;
            AppSession.FullName = StaffDB.GetFullName(loggedInId);
            AppSession.Role = StaffDB.GetRole(loggedInId);

            HideError();

            // ── Route by role ─────────────────────────────────────────
            switch (AppSession.Role)
            {
                case "Staff Encoder":
                    new Staffdashboard().Show();
                    this.Close();
                    break;

                case "Section Head":
                case "Admin":
                    new Admindashboard().Show();
                    this.Close();
                    break;

                default:
                    ShowError($"Role '{AppSession.Role}' is not recognized. Contact your administrator.");
                    break;
            }
        }

        private void ForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Please contact your system administrator to reset your password.",
                            "Forgot Password", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowError(string message)
        {
            TxtError.Text = message;
            TxtError.Visibility = Visibility.Visible;
        }

        private void HideError()
        {
            TxtError.Text = "";
            TxtError.Visibility = Visibility.Collapsed;
        }

        private void TxtID_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TxtError.Visibility == Visibility.Visible)
                HideError();
        }
    }
}