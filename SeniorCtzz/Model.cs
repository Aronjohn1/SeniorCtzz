namespace SeniorCtzz
{
    public static class Models
    {

        public class StaffItem
        {
            public string No { get; set; }
            public string FullName { get; set; } = "";
            public string StaffId { get; set; } = "";
            public string Password { get; set; } = "";  
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
            public string LGU { get; set; } = "";  
            public string DSWD { get; set; } = "";  
            public string Sss { get; set; }       
            public string Gsis { get; set; }
            public string Waitlist { get; set; } = "";
            public string Lgu => LGU;          
            public string Dswd => DSWD;              
            public string Barangay { get; set; } = "";
            public string PensionType { get; set; } 
        }

    
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

  
            public string StatusBg { get; set; } = "#FEF9C3";
            public string StatusBorder { get; set; } = "#EAB308";
            public string StatusFg { get; set; } = "#854D0E";
        }

        public class BereavedItem
        {
            public string No { get; set; } = "";
            public string Date { get; set; } = "";
            public string FirstName { get; set; } = "";
            public string LastName { get; set; } = "";
            public string Brgy { get; set; } = "";  
            public string Barangay => Brgy;          
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
            public string Barangay { get; set; } = "";  
            public string Brgy => Barangay;          
            public string Amount { get; set; } = "";
            public string PensionType { get; set; } = "";
        }

   
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
