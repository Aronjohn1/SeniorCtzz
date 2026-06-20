using System.Windows;
using System.Windows.Input;

namespace SeniorCtzz
{
    public partial class ConfirmationRegister : Window
    {
        public bool IsConfirmed { get; private set; } = false;

        private readonly string _lastName;
        private readonly string _firstName;
        private readonly string _middleName;
        private readonly string _suffix;
        private readonly string _sex;
        private readonly string _birthDate;
        private readonly string _age;
        private readonly string _status;
        private readonly string _oscaId;
        private readonly string _dateIssued;
        private readonly string _barangay;
        private readonly string _pensionType;     
        private readonly string _assistanceSource; 
        private readonly string _bloodType;
        private readonly string _residency;

    
        public ConfirmationRegister(
            string lastName, string firstName, string middleName, string suffix,
            string sex, string birthDate, string age, string status,
            string oscaId, string dateIssued, string barangay,
            string pensionType,      
            string assistanceSource, 
            string bloodType = "",
            string residency = "")
        {
            InitializeComponent();

            _lastName = lastName;
            _firstName = firstName;
            _middleName = middleName;
            _suffix = suffix;
            _sex = sex;
            _birthDate = birthDate;
            _age = age;
            _status = status;
            _oscaId = oscaId;
            _dateIssued = dateIssued;
            _barangay = barangay;
            _pensionType = pensionType;
            _assistanceSource = assistanceSource;
            _bloodType = bloodType;
            _residency = residency;

            PopulateReviewFields();
        }

        private void PopulateReviewFields()
        {
            TxtReviewLastName.Text = _lastName;
            TxtReviewFirstName.Text = _firstName;
            TxtReviewMiddleName.Text = string.IsNullOrEmpty(_middleName) ? "—" : _middleName;
            TxtReviewSuffix.Text = string.IsNullOrEmpty(_suffix) ? "—" : _suffix;
            TxtReviewSex.Text = _sex;
            TxtReviewBirthDate.Text = _birthDate;
            TxtReviewAge.Text = _age;
            TxtReviewStatus.Text = _status;
            TxtReviewOscaId.Text = _oscaId;
            TxtReviewDateIssued.Text = _dateIssued;
            TxtReviewBarangay.Text = _barangay;

  
            TxtReviewPensionType.Text =
                string.IsNullOrEmpty(_pensionType) ? "—" : _pensionType;

            TxtReviewAssistanceSource.Text =
                string.IsNullOrEmpty(_assistanceSource) ? "—" : _assistanceSource;

            TxtReviewBloodType.Text = string.IsNullOrEmpty(_bloodType) ? "—" : _bloodType;
            TxtReviewResidency.Text = string.IsNullOrEmpty(_residency) ? "—" : _residency;
        }

        private void BtnBackToEdit_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = false;
            this.Close();
        }

        private void BtnConfirmRegister_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = true;
            this.Close();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = false;
            this.Close();
        }

        private void Overlay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            IsConfirmed = false;
            this.Close();
        }
    }
}
