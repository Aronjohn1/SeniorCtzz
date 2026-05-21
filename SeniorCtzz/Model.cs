namespace SeniorCtzz
{
    public static class Models
    {
        // ── Staff / Users ─────────────────────────────────────────────
        public class StaffItem
        {
            public string No { get; set; }
            public string FullName { get; set; } = "";
            public string StaffId { get; set; } = "";
            public string Password { get; set; } = "";   // ← must exist
            public string PasswordDisplay => string.IsNullOrEmpty(Password) ? "" : new string('●', 8);
            public string Role { get; set; } = "";
        }

        public class AdminQuarterlyItem
        {
            public string No { get; set; } = "";
            public string Date { get; set; } = "";
            public string FirstName { get; set; } = "";
            public string LastName { get; set; } = "";
            public string Brgy { get; set; } = "";
            public string Amount { get; set; } = "2,250";
            public string ReleaseDate { get; set; } = "";
            public string Status { get; set; } = "";
            public string StatusBg { get; set; } = "#FEF9C3";
            public string StatusBorder { get; set; } = "#EAB308";
            public string StatusFg { get; set; } = "#854D0E";
        }

        // ── Senior Citizen search result ──────────────────────────────
        public class SeniorSearchItem
        {
            public string SeniorId { get; set; }
            public string FullName { get; set; } = "";
            public string BirthDate { get; set; } = "";
            public string Barangay { get; set; } = "";
        }
        public class DashboardRecentItem
        {
            public string Date { get; set; } = "";
            public string LastName { get; set; } = "";
            public string FirstName { get; set; } = "";
            public string Barangay { get; set; } = "";
            public string AICSCount { get; set; } = "0";
            public string APRCount { get; set; } = "0";
            public string QuarterlyCount { get; set; } = "0";
            public string BereavedCount { get; set; } = "0";
            public string TotalPensions { get; set; } = "0";
        }
        // ── Barangay list (full senior citizen registry per barangay) ─
        public class BarangayListItem
        {
            public string No { get; set; } = "";
            public string LastName { get; set; } = "";
            public string FirstName { get; set; } = "";
            public string MiddleName { get; set; } = "";
            public string Suffix { get; set; } = "";
            public string Sex { get; set; } = "";
            public string BirthDate { get; set; } = "";
            public string Age { get; set; } = "";
            public string Status { get; set; } = "";
            public string OscaId { get; set; } = "";
            public string DateIssued { get; set; } = "";
            public string BloodType { get; set; } = "";
            public string LGU { get; set; } = "";   // Staffpensionlist XAML
            public string DSWD { get; set; } = "";   // Staffpensionlist XAML
            public string Sss { get; set; }        // ✅ NEW
            public string Gsis { get; set; }
            public string Waitlist { get; set; } = "";
            public string Lgu => LGU;               // Staffreports XAML alias
            public string Dswd => DSWD;              // Staffreports XAML alias
            public string Barangay { get; set; } = "";
            public string PensionType { get; set; } // ✅ NEW - raw pension type string
        }

        // ── Overall SC summary (Urban / Coastal / Rural) ──────────────
        public class OverAllSCItem
        {
            public string No { get; set; } = "";
            public string Barangay { get; set; } = "";
            public string NatMale { get; set; } = "0";
            public string NatFemale { get; set; } = "0";
            public string LocMale { get; set; } = "0";
            public string LocFemale { get; set; } = "0";
            public string WaitMale { get; set; } = "0";
            public string WaitFemale { get; set; } = "0";
            public string SssMale { get; set; } = "0";
            public string SssFemale { get; set; } = "0";
            public string GsisMale { get; set; } = "0";
            public string GsisFemale { get; set; } = "0";
            public string TotalMale { get; set; } = "0";
            public string TotalFemale { get; set; } = "0";
        }

        // ── Pension / AICS / APR (shared layout) ─────────────────────
        public class PensionItem
        {
            public string No { get; set; }
            public string Date { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Brgy { get; set; }
            public string Amount { get; set; }
            public string HospitalBill { get; set; }
            public string Percentage { get; set; }
        }
        // Add this inside the Models class
        // Add this inside the public static class Models
        public class QuarterlyItem
        {
            public string ReleaseId { get; set; }
            public string SeniorId { get; set; }
            public string OscaId { get; set; }
            public string FullName { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Date { get; set; }
            public string Barangay { get; set; }
            public string Amount { get; set; }
            public string ReleaseDate { get; set; }
            public string Quarter { get; set; }
            public string Status { get; set; }

            // Color properties for XAML binding
            public string StatusBg { get; set; } = "#FEF9C3";
            public string StatusBorder { get; set; } = "#EAB308";
            public string StatusFg { get; set; } = "#854D0E";
        }
        // ── Bereaved Assistance ───────────────────────────────────────
        public class BereavedItem
        {
            public string No { get; set; } = "";
            public string Date { get; set; } = "";
            public string FirstName { get; set; } = "";
            public string LastName { get; set; } = "";
            public string Brgy { get; set; } = "";  // Staffreports
            public string Barangay => Brgy;             // Staffpensionlist alias
            public string RecipientName { get; set; } = "";
            public string Relationship { get; set; } = "";
            public string Amount { get; set; } = "";
            public string Remarks { get; set; } = "";
        }

        public class RecentItem
        {
            public string Date { get; set; } = "";
            public string LastName { get; set; } = "";
            public string FirstName { get; set; } = "";
            public string Barangay { get; set; } = "";  // Staffdashboard XAML
            public string Brgy => Barangay;          // Admindashboard XAML alias
            public string Amount { get; set; } = "";
            public string PensionType { get; set; } = "";
        }

        // ── Pension list other-tab row ────────────────────────────────
        public class OtherTabItem
        {
            public string No { get; set; } = "";
            public string Date { get; set; } = "";
            public string FirstName { get; set; } = "";
            public string LastName { get; set; } = "";
            public string Barangay { get; set; } = "";
            public string Amount { get; set; } = "";
        }
    }
}