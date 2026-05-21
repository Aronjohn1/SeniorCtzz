using System;
using System.Collections.ObjectModel;
using System.Data.OleDb;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Input;
using static SeniorCtzz.Models;

namespace SeniorCtzz
{
    // ═══════════════════════════════════════════════════════════════════
    //  DATABASE CONNECTION HELPER
    // ═══════════════════════════════════════════════════════════════════
    public static class DBConnection
    {
        private static readonly string _dbPath =
          Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
             "Database", "SeniorCtzz.accdb");

        public static string ConnectionString =>
            $"Provider=Microsoft.ACE.OLEDB.12.0;" +
            $"Data Source={_dbPath};" +
            $"Persist Security Info=False;";

        public static OleDbConnection GetConnection()
            => new OleDbConnection(ConnectionString);

        public static bool TestConnection()
        {
            try
            {
                using var conn = GetConnection();
                conn.Open();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database error: {ex.Message}",
                                "Connection Failed",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                return false;
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  STAFF / USERS QUERIES
    // ═══════════════════════════════════════════════════════════════════
    public static class StaffDB
    {
        private static readonly string _masterPassword = "OSCA2024!Secret";
        private static readonly byte[] _salt = Encoding.UTF8.GetBytes("GingoogSalt2024");

        private static string EncryptPassword(string plainText)
        {
            using (var deriveBytes = new Rfc2898DeriveBytes(_masterPassword, _salt, 10000, HashAlgorithmName.SHA256))
            {
                byte[] key = deriveBytes.GetBytes(16);
                byte[] iv = deriveBytes.GetBytes(16);
                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;
                    ICryptoTransform encryptor = aes.CreateEncryptor();
                    using (MemoryStream ms = new MemoryStream())
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (StreamWriter sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                        sw.Close();
                        return Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
        }

        private static string DecryptPassword(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return "";
            try
            {
                using (var deriveBytes = new Rfc2898DeriveBytes(_masterPassword, _salt, 10000, HashAlgorithmName.SHA256))
                {
                    byte[] key = deriveBytes.GetBytes(16);
                    byte[] iv = deriveBytes.GetBytes(16);
                    using (Aes aes = Aes.Create())
                    {
                        aes.Key = key;
                        aes.IV = iv;
                        ICryptoTransform decryptor = aes.CreateDecryptor();
                        byte[] cipherBytes = Convert.FromBase64String(cipherText);
                        using (MemoryStream ms = new MemoryStream(cipherBytes))
                        using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                        using (StreamReader sr = new StreamReader(cs))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
            catch { return ""; }
        }

        private static string HashPassword(string rawPassword)
        {
            using (var sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(rawPassword));
                StringBuilder sb = new StringBuilder();
                foreach (byte b in bytes)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        public static string Login(string employeeId, string password)
        {
            try
            {
                using var conn = DBConnection.GetConnection();
                conn.Open();
                string sql = "SELECT user_id, [password] FROM USERS WHERE employee_id = @id";
                using var cmd = new OleDbCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", employeeId);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    string stored = reader["password"]?.ToString() ?? "";

                    string decrypted = DecryptPassword(stored);
                    if (decrypted == password)
                        return reader["user_id"]?.ToString() ?? "";

                    if (stored == password)
                        return reader["user_id"]?.ToString() ?? "";

                    string hashedInput = HashPassword(password);
                    if (stored == hashedInput)
                        return reader["user_id"]?.ToString() ?? "";
                }
                return "";
            }
            catch (Exception ex) { MessageBox.Show($"Login error: {ex.Message}"); return ""; }
        }

        public static string GetRole(string userId)
        {
            try
            {
                using var conn = DBConnection.GetConnection();
                conn.Open();
                string sql = "SELECT role FROM USERS WHERE user_id = @id";
                using var cmd = new OleDbCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", userId);
                return cmd.ExecuteScalar()?.ToString() ?? "";
            }
            catch { return ""; }
        }

        public static string GetFullName(string userId)
        {
            try
            {
                using var conn = DBConnection.GetConnection();
                conn.Open();
                string sql = "SELECT full_name FROM USERS WHERE user_id = @id";
                using var cmd = new OleDbCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", userId);
                return cmd.ExecuteScalar()?.ToString() ?? "";
            }
            catch { return ""; }
        }

        public static int GetActiveStaffCount()
        {
            try
            {
                using var conn = DBConnection.GetConnection();
                conn.Open();
                string sql = "SELECT COUNT(*) FROM USERS WHERE [role] = 'Staff Encoder'";
                using var cmd = new OleDbCommand(sql, conn);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch { return 0; }
        }

        public static ObservableCollection<StaffItem> GetAll()
        {
            var list = new ObservableCollection<StaffItem>();
            try
            {
                using var conn = DBConnection.GetConnection();
                conn.Open();
                string sql = "SELECT user_id, full_name, employee_id, [password], [role] FROM USERS ORDER BY user_id";
                using var cmd = new OleDbCommand(sql, conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string stored = reader["password"]?.ToString() ?? "";
                    string decrypted = DecryptPassword(stored);
                    string displayPassword = string.IsNullOrEmpty(decrypted) ? stored : decrypted;

                    list.Add(new StaffItem
                    {
                        No = reader["user_id"]?.ToString(),
                        FullName = reader["full_name"]?.ToString(),
                        StaffId = reader["employee_id"]?.ToString(),
                        Password = displayPassword,
                        Role = reader["role"]?.ToString(),
                    });
                }
            }
            catch (Exception ex) { MessageBox.Show($"Error loading staff: {ex.Message}"); }
            return list;
        }

        public static bool Add(string fullName, string employeeId, string password, string role = "Staff Encoder")
        {
            try
            {
                string encrypted = EncryptPassword(password);
                using var conn = DBConnection.GetConnection();
                conn.Open();
                string newUserId = GenerateUserId(conn);
                string sql = "INSERT INTO USERS (user_id, full_name, employee_id, [password], [role], created_at) " +
                             "VALUES (@uid, @name, @id, @pw, @role, Now())";
                using var cmd = new OleDbCommand(sql, conn);
                cmd.Parameters.AddWithValue("@uid", newUserId);
                cmd.Parameters.AddWithValue("@name", fullName);
                cmd.Parameters.AddWithValue("@id", employeeId);
                cmd.Parameters.AddWithValue("@pw", encrypted);
                cmd.Parameters.AddWithValue("@role", role);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex) { MessageBox.Show($"Error saving staff: {ex.Message}"); return false; }
        }

        public static bool Update(string employeeId, string fullName, string newPassword)
        {
            try
            {
                string encrypted = EncryptPassword(newPassword);
                using var conn = DBConnection.GetConnection();
                conn.Open();
                string sql = "UPDATE USERS SET full_name = @name, [password] = @pw WHERE employee_id = @id";
                using var cmd = new OleDbCommand(sql, conn);
                cmd.Parameters.AddWithValue("@name", fullName);
                cmd.Parameters.AddWithValue("@pw", encrypted);
                cmd.Parameters.AddWithValue("@id", employeeId);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex) { MessageBox.Show($"Error updating staff: {ex.Message}"); return false; }
        }

        private static string GenerateUserId(OleDbConnection conn)
        {
            try
            {
                string sql = "SELECT COUNT(*) FROM USERS";
                using var cmd = new OleDbCommand(sql, conn);
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return $"USER-{(count + 1):D3}";
            }
            catch { return $"USER-{DateTime.Now:mmssff}"; }
        }

        public static bool Delete(string employeeId)
        {
            try
            {
                using var conn = DBConnection.GetConnection();
                conn.Open();
                string sql = "DELETE FROM USERS WHERE employee_id = @id";
                using var cmd = new OleDbCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", employeeId);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex) { MessageBox.Show($"Error deleting staff: {ex.Message}"); return false; }
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  SENIOR CITIZEN QUERIES
    // ═══════════════════════════════════════════════════════════════════
    public static class SeniorDB
    {
        // ─────────────────────────────────────────────────────────────
        // GET BY BARANGAY
        // ─────────────────────────────────────────────────────────────
        public static ObservableCollection<BarangayListItem> GetByBarangay(string barangayName)
        {
            var list = new ObservableCollection<BarangayListItem>();
            try
            {
                using var conn = DBConnection.GetConnection();
                conn.Open();
                string sql = @"SELECT sc.osca_id, sc.last_name, sc.first_name,
                              sc.middle_name, sc.suffix, sc.sex,
                              sc.birthdate, sc.age, sc.status,
                              sc.date_of_issued, sc.blood_type,
                              sc.pension_type, sc.assistance_source, b.barangay_name
                       FROM SENIOR_CITIZEN sc
                       INNER JOIN BARANGAY b ON sc.barangay_id = b.barangay_id
                       WHERE b.barangay_name = @brgy
                       ORDER BY sc.last_name";
                using var cmd = new OleDbCommand(sql, conn);
                cmd.Parameters.AddWithValue("@brgy", barangayName);
                using var reader = cmd.ExecuteReader();
                int no = 1;
                while (reader.Read())
                {
                    // ── Pension Type  : SSS / GSIS  (from pension_type column)
                    // ── Assist Source : LGU / DSWD / WAITLIST (from assistance_source column)
                    string pt = reader["pension_type"]?.ToString() ?? "";
                    string ast = reader["assistance_source"]?.ToString() ?? "";

                    list.Add(new BarangayListItem
                    {
                        No = (no++).ToString(),
                        LastName = reader["last_name"]?.ToString(),
                        FirstName = reader["first_name"]?.ToString(),
                        MiddleName = reader["middle_name"]?.ToString(),
                        Suffix = reader["suffix"]?.ToString(),
                        Sex = reader["sex"]?.ToString(),
                        BirthDate = reader["birthdate"] != DBNull.Value
                                      ? Convert.ToDateTime(reader["birthdate"]).ToString("MM-dd-yy") : "",
                        Age = reader["age"]?.ToString(),
                        Status = reader["status"]?.ToString(),
                        OscaId = reader["osca_id"]?.ToString(),
                        DateIssued = reader["date_of_issued"] != DBNull.Value
                                      ? Convert.ToDateTime(reader["date_of_issued"]).ToString("MM-dd-yy") : "",
                        BloodType = reader["blood_type"]?.ToString() ?? "",
                        // Assistance Source flags — read from assistance_source column
                        LGU = ast.ToUpper().Contains("LGU") ? "✔" : "",
                        DSWD = ast.ToUpper().Contains("DSWD") ? "✔" : "",
                        Waitlist = ast.ToUpper().Contains("WAITLIST") ? "✔" : "",
                        // Pension Type flags — read from pension_type column
                        Sss = pt.ToUpper().Contains("SSS") ? "✔" : "",
                        Gsis = pt.ToUpper().Contains("GSIS") ? "✔" : "",
                        PensionType = pt,
                    });
                }
            }
            catch (Exception ex) { MessageBox.Show($"Error loading barangay list: {ex.Message}"); }
            return list;
        }

        // ─────────────────────────────────────────────────────────────
        // GET BARANGAY NAMES
        // ─────────────────────────────────────────────────────────────
        public static ObservableCollection<string> GetBarangayNames()
        {
            var list = new ObservableCollection<string>();
            try
            {
                using var conn = DBConnection.GetConnection();
                conn.Open();
                string sql = "SELECT barangay_name FROM BARANGAY ORDER BY barangay_name";
                using var cmd = new OleDbCommand(sql, conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    list.Add(reader["barangay_name"]?.ToString() ?? "");
            }
            catch (Exception ex) { MessageBox.Show($"Error loading barangays: {ex.Message}"); }
            return list;
        }

        // ─────────────────────────────────────────────────────────────
        // COUNT HELPERS
        // ─────────────────────────────────────────────────────────────
        public static int GetActiveCount()
        {
            try
            {
                using var conn = DBConnection.GetConnection();
                conn.Open();
                string sql = @"SELECT COUNT(*) FROM SENIOR_CITIZEN sc
                               LEFT JOIN DECEASED_RECORD d ON sc.senior_id = d.senior_id
                               WHERE d.deceased_id IS NULL";
                using var cmd = new OleDbCommand(sql, conn);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch { return 0; }
        }

        public static int GetInactiveCount()
        {
            try
            {
                using var conn = DBConnection.GetConnection();
                conn.Open();
                string sql = @"SELECT COUNT(*) FROM SENIOR_CITIZEN sc
                               INNER JOIN DECEASED_RECORD d ON sc.senior_id = d.senior_id";
                using var cmd = new OleDbCommand(sql, conn);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch { return 0; }
        }

        public static int GetTotalCount()
        {
            try
            {
                using var conn = DBConnection.GetConnection();
                conn.Open();
                string sql = "SELECT COUNT(*) FROM SENIOR_CITIZEN";
                using var cmd = new OleDbCommand(sql, conn);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch { return 0; }
        }

        public static int GetEntriesThisMonth()
        {
            try
            {
                using var conn = DBConnection.GetConnection();
                conn.Open();
                string sql = @"SELECT COUNT(*) FROM SENIOR_CITIZEN
                               WHERE MONTH(created_at) = MONTH(NOW())
                               AND   YEAR(created_at)  = YEAR(NOW())";
                using var cmd = new OleDbCommand(sql, conn);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch { return 0; }
        }

        // ─────────────────────────────────────────────────────────────
        // SEARCH
        // ─────────────────────────────────────────────────────────────
        public static ObservableCollection<SeniorSearchItem> Search(string keyword)
        {
            var list = new ObservableCollection<SeniorSearchItem>();
            try
            {
                using var conn = DBConnection.GetConnection();
                conn.Open();
                string sql = @"SELECT sc.senior_id, sc.first_name, sc.last_name,
                              sc.birthdate, b.barangay_name
                       FROM SENIOR_CITIZEN sc
                       INNER JOIN BARANGAY b ON sc.barangay_id = b.barangay_id
                       WHERE (sc.first_name & ' ' & sc.last_name LIKE @kw) 
                          OR (sc.last_name  LIKE @kw) 
                          OR (sc.first_name LIKE @kw)
                          OR (sc.osca_id    LIKE @kw)";
                using var cmd = new OleDbCommand(sql, conn);
                cmd.Parameters.AddWithValue("@kw", $"%{keyword}%");
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string firstName = reader["first_name"]?.ToString() ?? "";
                    string lastName = reader["last_name"]?.ToString() ?? "";
                    list.Add(new SeniorSearchItem
                    {
                        SeniorId = reader["senior_id"]?.ToString() ?? "",
                        FullName = $"{firstName} {lastName}".Trim(),
                        BirthDate = reader["birthdate"] != DBNull.Value
                                    ? Convert.ToDateTime(reader["birthdate"]).ToString("MM-dd-yy") : "",
                        Barangay = reader["barangay_name"]?.ToString() ?? "",
                    });
                }
            }
            catch (Exception ex) { MessageBox.Show($"Error searching: {ex.Message}"); }
            return list;
        }

        // ─────────────────────────────────────────────────────────────
        // OVERALL TOTALS REPORT
        // ─────────────────────────────────────────────────────────────
        public static ObservableCollection<OverAllSCItem> GetOverAllTotals()
        {
            var list = new ObservableCollection<OverAllSCItem>();
            try
            {
                using var conn = DBConnection.GetConnection();
                conn.Open();

                // Uses assistance_source for LGU/DSWD/WAITLIST checks
                // Uses pension_type for SSS/GSIS checks
                string sql = @"
            SELECT 
                sc.residency_classification,
                SUM(IIF(sc.sex='M' AND sc.assistance_source LIKE '%DSWD%',     1, 0)) AS nat_m,
                SUM(IIF(sc.sex='F' AND sc.assistance_source LIKE '%DSWD%',     1, 0)) AS nat_f,
                SUM(IIF(sc.sex='M' AND sc.assistance_source LIKE '%LGU%',      1, 0)) AS loc_m,
                SUM(IIF(sc.sex='F' AND sc.assistance_source LIKE '%LGU%',      1, 0)) AS loc_f,
                SUM(IIF(sc.sex='M' AND sc.assistance_source LIKE '%WAITLIST%', 1, 0)) AS wait_m,
                SUM(IIF(sc.sex='F' AND sc.assistance_source LIKE '%WAITLIST%', 1, 0)) AS wait_f,
                SUM(IIF(sc.sex='M' AND sc.pension_type      LIKE '%SSS%',      1, 0)) AS sss_m,
                SUM(IIF(sc.sex='F' AND sc.pension_type      LIKE '%SSS%',      1, 0)) AS sss_f,
                SUM(IIF(sc.sex='M' AND sc.pension_type      LIKE '%GSIS%',     1, 0)) AS gsis_m,
                SUM(IIF(sc.sex='F' AND sc.pension_type      LIKE '%GSIS%',     1, 0)) AS gsis_f,
                SUM(IIF(sc.sex='M', 1, 0)) AS tot_m,
                SUM(IIF(sc.sex='F', 1, 0)) AS tot_f
            FROM SENIOR_CITIZEN sc
            INNER JOIN BARANGAY b ON sc.barangay_id = b.barangay_id
            GROUP BY sc.residency_classification
            ORDER BY sc.residency_classification";

                using var cmd = new OleDbCommand(sql, conn);
                using var reader = cmd.ExecuteReader();
                int no = 1;
                bool hasUrban = false, hasCoastal = false, hasRural = false;

                while (reader.Read())
                {
                    string classification = reader["residency_classification"]?.ToString() ?? "";
                    list.Add(new OverAllSCItem
                    {
                        No = (no++).ToString(),
                        Barangay = classification,
                        NatMale = reader["nat_m"]?.ToString() ?? "0",
                        NatFemale = reader["nat_f"]?.ToString() ?? "0",
                        LocMale = reader["loc_m"]?.ToString() ?? "0",
                        LocFemale = reader["loc_f"]?.ToString() ?? "0",
                        WaitMale = reader["wait_m"]?.ToString() ?? "0",
                        WaitFemale = reader["wait_f"]?.ToString() ?? "0",
                        SssMale = reader["sss_m"]?.ToString() ?? "0",
                        SssFemale = reader["sss_f"]?.ToString() ?? "0",
                        GsisMale = reader["gsis_m"]?.ToString() ?? "0",
                        GsisFemale = reader["gsis_f"]?.ToString() ?? "0",
                        TotalMale = reader["tot_m"]?.ToString() ?? "0",
                        TotalFemale = reader["tot_f"]?.ToString() ?? "0",
                    });

                    if (classification.Equals("Urban", StringComparison.OrdinalIgnoreCase)) hasUrban = true;
                    if (classification.Equals("Coastal", StringComparison.OrdinalIgnoreCase)) hasCoastal = true;
                    if (classification.Equals("Rural", StringComparison.OrdinalIgnoreCase)) hasRural = true;
                }

                if (!hasUrban) list.Insert(0, BlankOverAll("1", "Urban"));
                if (!hasCoastal) list.Insert(1, BlankOverAll("2", "Coastal"));
                if (!hasRural) list.Add(BlankOverAll("3", "Rural"));

                for (int i = 0; i < list.Count; i++)
                    list[i].No = (i + 1).ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading overall SC: {ex.Message}",
                                "Report Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return list;
        }

        private static OverAllSCItem BlankOverAll(string no, string label) => new OverAllSCItem
        {
            No = no,
            Barangay = label,
            NatMale = "0",
            NatFemale = "0",
            LocMale = "0",
            LocFemale = "0",
            WaitMale = "0",
            WaitFemale = "0",
            SssMale = "0",
            SssFemale = "0",
            GsisMale = "0",
            GsisFemale = "0",
            TotalMale = "0",
            TotalFemale = "0"
        };

        // ─────────────────────────────────────────────────────────────
        // BARANGAY ID HELPERS
        // ─────────────────────────────────────────────────────────────
        public static string GetBarangayId(string barangayName)
        {
            try
            {
                using var conn = DBConnection.GetConnection(); conn.Open();
                using var cmd = new OleDbCommand(
                    "SELECT barangay_id FROM BARANGAY WHERE barangay_name = @name", conn);
                cmd.Parameters.AddWithValue("@name", barangayName);
                return cmd.ExecuteScalar()?.ToString() ?? "";
            }
            catch { return ""; }
        }

        public static string GetOrCreateBarangayIdStr(string barangayName)
        {
            try
            {
                using var conn = DBConnection.GetConnection();
                conn.Open();

                using (var selectCmd = new OleDbCommand(
                    "SELECT barangay_id FROM BARANGAY WHERE barangay_name = @name", conn))
                {
                    selectCmd.Parameters.AddWithValue("@name", barangayName);
                    var existing = selectCmd.ExecuteScalar();
                    if (existing != null) return existing.ToString();
                }

                int maxNum = 0;
                using (var allCmd = new OleDbCommand("SELECT barangay_id FROM BARANGAY", conn))
                using (var reader = allCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string id = reader[0]?.ToString() ?? "";
                        if (id.StartsWith("BRGY-", StringComparison.OrdinalIgnoreCase) &&
                            int.TryParse(id.Substring(5), out int n))
                            maxNum = Math.Max(maxNum, n);
                    }
                }

                string newId = $"BRGY-{maxNum + 1}";
                string bType = GetBarangayType(barangayName);
                using var insertCmd = new OleDbCommand(
                    "INSERT INTO BARANGAY (barangay_id, barangay_name, barangay_type) VALUES (@id, @name, @type)", conn);
                insertCmd.Parameters.AddWithValue("@id", newId);
                insertCmd.Parameters.AddWithValue("@name", barangayName);
                insertCmd.Parameters.AddWithValue("@type", bType);
                insertCmd.ExecuteNonQuery();
                return newId;
            }
            catch { return ""; }
        }

        public static string GetBarangayType(string barangayName)
        {
            var coastal = new System.Collections.Generic.HashSet<string>(
                System.StringComparer.OrdinalIgnoreCase)
            {
                "Agay-ayan","Anakan","Daan-Lungsod","Kalagonoy","Lawaan",
                "Libertad","Lunao","Pangasihan","Samay","San Juan",
                "San Luis","Santiago","Talisay"
            };

            var rural = new System.Collections.Generic.HashSet<string>(
                System.StringComparer.OrdinalIgnoreCase)
            {
                "Alagatan","Bagubad","Bakidbakid","Bal-ason","Bantaawan",
                "Binakalan","Capitulangan","Dinawehan","Eureka","Hindangon",
                "Kalipay","Kamanikan","Kianlagan","Kibuging","Kipuntos",
                "Lawit","Libon","Lunotan","Malibud","Malinao","Maribucao",
                "Mimbalagon","Mimbunga","Mimbuntong","Minsapinit","Murallon",
                "Odiongan","Pigsaluhan","Punong","Ricoro","San Jose",
                "San Miguel","Sangalan","Tagpako","Talon","Tinabalan","Tinulongan"
            };

            if (coastal.Contains(barangayName)) return "Coastal";
            if (rural.Contains(barangayName)) return "Rural";
            return "Urban";
        }

        // ─────────────────────────────────────────────────────────────
        // GENERATE SENIOR ID
        // ─────────────────────────────────────────────────────────────
        private static string GenerateSeniorId(OleDbConnection conn)
        {
            try
            {
                string year = DateTime.Now.Year.ToString();
                string sql = "SELECT COUNT(*) FROM SENIOR_CITIZEN WHERE senior_id LIKE @pattern";
                using var cmd = new OleDbCommand(sql, conn);
                cmd.Parameters.AddWithValue("@pattern", $"SC-{year}-%");
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return $"SC-{year}-{(count + 1):D4}";
            }
            catch { return $"SC-{DateTime.Now:yyyy}-{DateTime.Now:mmssff}"; }
        }

        // ═════════════════════════════════════════════════════════════
        // SAVE — NOW WITH SEPARATE pension_type AND assistance_source
        //   pensionType      → SSS / GSIS only
        //   assistanceSource → LGU / DSWD / WAITLIST only
        // ═════════════════════════════════════════════════════════════
        public static bool Save(string lastName, string firstName, string middleName,
                                string suffix, string sex, DateTime birthDate,
                                int age, string status, string oscaId,
                                DateTime dateIssued, string barangayName,
                                string pensionType,                          // SSS / GSIS
                                string bloodType = "",
                                string residency = "",
                                string encoderId = "",
                                string assistanceSource = "")                // LGU / DSWD / WAITLIST
        {
            try
            {
                using var conn = DBConnection.GetConnection();
                conn.Open();

                string barangayId = GetOrCreateBarangayIdStr(barangayName);
                if (string.IsNullOrEmpty(barangayId))
                {
                    MessageBox.Show("Barangay not found and could not be created.",
                                    "Save Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                string seniorId = GenerateSeniorId(conn);

                string sql = @"INSERT INTO SENIOR_CITIZEN
                       (senior_id, last_name, first_name, middle_name, suffix, sex,
                        birthdate, age, status, osca_id, date_of_issued,
                        blood_type, pension_type, assistance_source,
                        residency_classification, barangay_id, encoder_id,
                        created_at, update_at)
                       VALUES
                       (@sid, @ln, @fn, @mn, @sfx, @sex,
                        @bd,  @age, @stat, @oid, @di,
                        @bt,  @pt,  @as,
                        @rc,  @bid, @eid,
                        Now(), Now())";

                using var cmd = new OleDbCommand(sql, conn);
                cmd.Parameters.AddWithValue("@sid", seniorId);
                cmd.Parameters.AddWithValue("@ln", lastName);
                cmd.Parameters.AddWithValue("@fn", firstName);
                cmd.Parameters.AddWithValue("@mn", string.IsNullOrEmpty(middleName) ? DBNull.Value : (object)middleName);
                cmd.Parameters.AddWithValue("@sfx", string.IsNullOrEmpty(suffix) ? DBNull.Value : (object)suffix);
                cmd.Parameters.AddWithValue("@sex", sex);
                cmd.Parameters.AddWithValue("@bd", birthDate);
                cmd.Parameters.AddWithValue("@age", age);
                cmd.Parameters.AddWithValue("@stat", status);
                cmd.Parameters.AddWithValue("@oid", oscaId);
                cmd.Parameters.AddWithValue("@di", dateIssued);
                cmd.Parameters.AddWithValue("@bt", string.IsNullOrEmpty(bloodType) ? DBNull.Value : (object)bloodType);
                cmd.Parameters.AddWithValue("@pt", string.IsNullOrEmpty(pensionType) ? DBNull.Value : (object)pensionType);
                cmd.Parameters.AddWithValue("@as", string.IsNullOrEmpty(assistanceSource) ? DBNull.Value : (object)assistanceSource);
                cmd.Parameters.AddWithValue("@rc", string.IsNullOrEmpty(residency) ? DBNull.Value : (object)residency);
                cmd.Parameters.AddWithValue("@bid", barangayId);
                cmd.Parameters.AddWithValue("@eid", string.IsNullOrEmpty(encoderId) ? DBNull.Value : (object)encoderId);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving senior citizen: {ex.Message}");
                return false;
            }
        }

        // ─────────────────────────────────────────────────────────────
        // GET ALL SENIORS
        // ─────────────────────────────────────────────────────────────
        public static ObservableCollection<BarangayListItem> GetAllSeniors()
        {
            var list = new ObservableCollection<BarangayListItem>();
            try
            {
                using var conn = DBConnection.GetConnection();
                conn.Open();
                string sql = @"SELECT sc.osca_id, sc.last_name, sc.first_name,
                      sc.middle_name, sc.suffix, sc.sex,
                      sc.birthdate, sc.age, sc.status,
                      sc.date_of_issued, sc.blood_type,
                      sc.pension_type, sc.assistance_source, b.barangay_name
               FROM SENIOR_CITIZEN sc
               INNER JOIN BARANGAY b ON sc.barangay_id = b.barangay_id
               ORDER BY sc.last_name";
                using var cmd = new OleDbCommand(sql, conn);
                using var reader = cmd.ExecuteReader();
                int no = 1;
                while (reader.Read())
                {
                    // pension_type     → SSS / GSIS
                    // assistance_source → LGU / DSWD / WAITLIST
                    string pt = reader["pension_type"]?.ToString() ?? "";
                    string ast = reader["assistance_source"]?.ToString() ?? "";

                    list.Add(new BarangayListItem
                    {
                        No = (no++).ToString(),
                        LastName = reader["last_name"]?.ToString() ?? "",
                        FirstName = reader["first_name"]?.ToString() ?? "",
                        MiddleName = reader["middle_name"]?.ToString() ?? "",
                        Suffix = reader["suffix"]?.ToString() ?? "",
                        Sex = reader["sex"]?.ToString() ?? "",
                        BirthDate = reader["birthdate"] != DBNull.Value
                                      ? Convert.ToDateTime(reader["birthdate"]).ToString("MM-dd-yy") : "",
                        Age = reader["age"]?.ToString() ?? "",
                        Status = reader["status"]?.ToString() ?? "",
                        OscaId = reader["osca_id"]?.ToString() ?? "",
                        DateIssued = reader["date_of_issued"] != DBNull.Value
                                      ? Convert.ToDateTime(reader["date_of_issued"]).ToString("MM-dd-yy") : "",
                        BloodType = reader["blood_type"]?.ToString() ?? "",
                        // Assistance Source — from assistance_source column
                        LGU = ast.ToUpper().Contains("LGU") ? "✔" : "",
                        DSWD = ast.ToUpper().Contains("DSWD") ? "✔" : "",
                        Waitlist = ast.ToUpper().Contains("WAITLIST") ? "✔" : "",
                        // Pension Type — from pension_type column
                        Sss = pt.ToUpper().Contains("SSS") ? "✔" : "",
                        Gsis = pt.ToUpper().Contains("GSIS") ? "✔" : "",
                        PensionType = pt,
                        Barangay = reader["barangay_name"]?.ToString() ?? "",
                    });
                }
            }
            catch (Exception ex) { MessageBox.Show($"Error loading all seniors: {ex.Message}"); }
            return list;
        }
    }


    // ═══════════════════════════════════════════════════════════════════
    //  PENSION TRANSACTION QUERIES
    // ═══════════════════════════════════════════════════════════════════
    public static class PensionDB
    {
        public static int GetCount(string type)
        {
            string table = type switch
            {
                "AICS" => "AICS_TRANSACTION",
                "APR" => "APR_TRANSACTION",
                "BEREAVED" => "BEREAVED_ASSISTANCE",
                "QUARTERLY" => "QUARTERLY_PENSION_RELEASE",
                _ => "QUARTERLY_PENSION_RELEASE"
            };
            try
            {
                using var conn = DBConnection.GetConnection();
                conn.Open();
                string sql = $"SELECT COUNT(*) FROM {table}";
                using var cmd = new OleDbCommand(sql, conn);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch { return 0; }
        }

        public static int GetReleasesThisMonth()
        {
            int total = 0;
            var queries = new (string table, string dateCol)[]
            {
                ("QUARTERLY_PENSION_RELEASE", "scheduled_date"),
                ("AICS_TRANSACTION",          "date"),
                ("APR_TRANSACTION",           "date"),
                ("BEREAVED_ASSISTANCE",       "actual_release_date"),
            };

            try
            {
                using var conn = DBConnection.GetConnection();
                conn.Open();
                foreach (var (table, dateCol) in queries)
                {
                    try
                    {
                        string sql = $@"SELECT COUNT(*) FROM {table}
                                        WHERE MONTH({dateCol}) = MONTH(NOW())
                                        AND   YEAR({dateCol})  = YEAR(NOW())";
                        using var cmd = new OleDbCommand(sql, conn);
                        total += Convert.ToInt32(cmd.ExecuteScalar());
                    }
                    catch { }
                }
            }
            catch { }
            return total;
        }

        public static ObservableCollection<RecentItem> GetRecentBySeniorId(string seniorId, bool thisMonthOnly = false)
        {
            var list = new ObservableCollection<RecentItem>();
            try
            {
                using var conn = DBConnection.GetConnection();
                conn.Open();

                void AppendFrom(string sql, string pensionType)
                {
                    using var cmd = new OleDbCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@sid", seniorId);
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        list.Add(new RecentItem
                        {
                            Date = reader["trans_date"] != DBNull.Value
                                            ? Convert.ToDateTime(reader["trans_date"]).ToString("MM-dd-yy") : "--",
                            LastName = reader["last_name"]?.ToString() ?? "",
                            FirstName = reader["first_name"]?.ToString() ?? "",
                            Barangay = reader["barangay_name"]?.ToString() ?? "",
                            Amount = reader["amount"] != DBNull.Value
                                            ? Convert.ToDecimal(reader["amount"]).ToString("N0") : "0",
                            PensionType = pensionType
                        });
                    }
                }

                string mf = thisMonthOnly
                    ? "AND MONTH(q.scheduled_date) = MONTH(NOW()) AND YEAR(q.scheduled_date) = YEAR(NOW())" : "";
                AppendFrom($@"SELECT q.scheduled_date AS trans_date, sc.last_name, sc.first_name,
                             b.barangay_name, q.amount
                      FROM (QUARTERLY_PENSION_RELEASE q
                      INNER JOIN SENIOR_CITIZEN sc ON q.senior_id = sc.senior_id)
                      INNER JOIN BARANGAY b ON q.barangay_id = b.barangay_id
                      WHERE q.senior_id = @sid {mf}
                      ORDER BY q.scheduled_date DESC", "Quarterly");

                mf = thisMonthOnly
                    ? "AND MONTH(a.date) = MONTH(NOW()) AND YEAR(a.date) = YEAR(NOW())" : "";
                AppendFrom($@"SELECT a.date AS trans_date, sc.last_name, sc.first_name,
                             b.barangay_name, a.total AS amount
                      FROM (AICS_TRANSACTION a
                      INNER JOIN SENIOR_CITIZEN sc ON a.senior_id = sc.senior_id)
                      INNER JOIN BARANGAY b ON a.barangay_id = b.barangay_id
                      WHERE a.senior_id = @sid {mf}
                      ORDER BY a.date DESC", "AICS");

                mf = thisMonthOnly
                    ? "AND MONTH(a.date) = MONTH(NOW()) AND YEAR(a.date) = YEAR(NOW())" : "";
                AppendFrom($@"SELECT a.date AS trans_date, sc.last_name, sc.first_name,
                             b.barangay_name, a.total AS amount
                      FROM (APR_TRANSACTION a
                      INNER JOIN SENIOR_CITIZEN sc ON a.senior_id = sc.senior_id)
                      INNER JOIN BARANGAY b ON a.barangay_id = b.barangay_id
                      WHERE a.senior_id = @sid {mf}
                      ORDER BY a.date DESC", "APR");

                mf = thisMonthOnly
                    ? "AND MONTH(ba.actual_release_date) = MONTH(NOW()) AND YEAR(ba.actual_release_date) = YEAR(NOW())" : "";
                AppendFrom($@"SELECT ba.actual_release_date AS trans_date, sc.last_name, sc.first_name,
                             b.barangay_name, ba.amount
                      FROM (BEREAVED_ASSISTANCE ba
                      INNER JOIN SENIOR_CITIZEN sc ON ba.senior_id = sc.senior_id)
                      INNER JOIN BARANGAY b ON ba.barangay_id = b.barangay_id
                      WHERE ba.senior_id = @sid {mf}
                      ORDER BY ba.actual_release_date DESC", "BEREAVED");
            }
            catch { }
            return list;
        }

        public static ObservableCollection<PensionItem> GetQuarterly()
        {
            var list = new ObservableCollection<PensionItem>();
            try
            {
                using var conn = DBConnection.GetConnection();
                conn.Open();
                string sql = @"SELECT q.release_id, q.scheduled_date AS qdate, q.amount,
              sc.first_name, sc.last_name, b.barangay_name
       FROM (QUARTERLY_PENSION_RELEASE q
             INNER JOIN SENIOR_CITIZEN sc ON q.senior_id = sc.senior_id)
       INNER JOIN BARANGAY b ON q.barangay_id = b.barangay_id
       ORDER BY q.scheduled_date DESC";
                using var cmd = new OleDbCommand(sql, conn);
                using var reader = cmd.ExecuteReader();
                int no = 1;
                while (reader.Read())
                {
                    list.Add(new PensionItem
                    {
                        No = (no++).ToString(),
                        Date = reader["qdate"] != DBNull.Value
                                        ? Convert.ToDateTime(reader["qdate"]).ToString("MM-dd-yy") : "--",
                        FirstName = reader["first_name"]?.ToString(),
                        LastName = reader["last_name"]?.ToString(),
                        Brgy = reader["barangay_name"]?.ToString(),
                        Amount = reader["amount"] != DBNull.Value
                                        ? Convert.ToDecimal(reader["amount"]).ToString("N0") : "0",
                    });
                }
            }
            catch (Exception ex) { MessageBox.Show($"Error loading quarterly: {ex.Message}"); }
            return list;
        }

        public static ObservableCollection<PensionItem> GetAICS()
        {
            var list = new ObservableCollection<PensionItem>();
            try
            {
                using var conn = DBConnection.GetConnection();
                conn.Open();
                string sql = @"SELECT a.aics_id, a.scheduled_date, a.amount, a.percentage, a.total,
                              sc.first_name, sc.last_name, b.barangay_name
                       FROM (AICS_TRANSACTION a
                       INNER JOIN SENIOR_CITIZEN sc ON a.senior_id = sc.senior_id)
                       INNER JOIN BARANGAY b ON a.barangay_id = b.barangay_id
                       ORDER BY a.scheduled_date DESC";
                using var cmd = new OleDbCommand(sql, conn);
                using var reader = cmd.ExecuteReader();
                int no = 1;
                while (reader.Read())
                {
                    list.Add(new PensionItem
                    {
                        No = (no++).ToString(),
                        Date = reader["scheduled_date"] != DBNull.Value
                                          ? Convert.ToDateTime(reader["scheduled_date"]).ToString("MM-dd-yy") : "--",
                        FirstName = reader["first_name"]?.ToString(),
                        LastName = reader["last_name"]?.ToString(),
                        Brgy = reader["barangay_name"]?.ToString(),
                        HospitalBill = reader["amount"] != DBNull.Value
                                          ? Convert.ToDecimal(reader["amount"]).ToString("N0") : "0",
                        Percentage = reader["percentage"] != DBNull.Value
                                          ? Convert.ToDecimal(reader["percentage"]).ToString("N0") + "%" : "30%",
                        Amount = reader["total"] != DBNull.Value
                                          ? Convert.ToDecimal(reader["total"]).ToString("N2") : "0",
                    });
                }
            }
            catch { }
            return list;
        }

        public static ObservableCollection<PensionItem> GetAPR()
        {
            var list = new ObservableCollection<PensionItem>();
            try
            {
                using var conn = DBConnection.GetConnection();
                conn.Open();
                string sql = @"SELECT a.apr_id, a.scheduled_date, a.amount, a.total,
                              sc.first_name, sc.last_name, b.barangay_name
                       FROM (APR_TRANSACTION a
                       INNER JOIN SENIOR_CITIZEN sc ON a.senior_id = sc.senior_id)
                       INNER JOIN BARANGAY b ON a.barangay_id = b.barangay_id
                       ORDER BY a.scheduled_date DESC";
                using var cmd = new OleDbCommand(sql, conn);
                using var reader = cmd.ExecuteReader();
                int no = 1;
                while (reader.Read())
                {
                    list.Add(new PensionItem
                    {
                        No = (no++).ToString(),
                        Date = reader["scheduled_date"] != DBNull.Value
                                           ? Convert.ToDateTime(reader["scheduled_date"]).ToString("MM-dd-yy") : "--",
                        FirstName = reader["first_name"]?.ToString(),
                        LastName = reader["last_name"]?.ToString(),
                        Brgy = reader["barangay_name"]?.ToString(),
                        HospitalBill = reader["amount"] != DBNull.Value
                                           ? Convert.ToDecimal(reader["amount"]).ToString("N0") : "0",
                        Amount = reader["total"] != DBNull.Value
                                           ? Convert.ToDecimal(reader["total"]).ToString("N2") : "5,000",
                    });
                }
            }
            catch { }
            return list;
        }

        public static ObservableCollection<BereavedItem> GetBereaved()
        {
            var list = new ObservableCollection<BereavedItem>();
            try
            {
                using var conn = DBConnection.GetConnection();
                conn.Open();
                string sql = @"SELECT ba.bereaved_id, ba.actual_release_date AS bdate, ba.amount,
      ba.recipient_name, ba.relationship, ba.remarks,
      sc.first_name, sc.last_name, b.barangay_name
FROM (BEREAVED_ASSISTANCE ba
     INNER JOIN SENIOR_CITIZEN sc ON ba.senior_id = sc.senior_id)
INNER JOIN BARANGAY b ON ba.barangay_id = b.barangay_id
ORDER BY ba.actual_release_date DESC";
                using var cmd = new OleDbCommand(sql, conn);
                using var reader = cmd.ExecuteReader();
                int no = 1;
                while (reader.Read())
                {
                    list.Add(new BereavedItem
                    {
                        No = (no++).ToString(),
                        Date = reader["bdate"] != DBNull.Value
                                            ? Convert.ToDateTime(reader["bdate"]).ToString("MM-dd-yy") : "--",
                        FirstName = reader["first_name"]?.ToString(),
                        LastName = reader["last_name"]?.ToString(),
                        Brgy = reader["barangay_name"]?.ToString(),
                        RecipientName = reader["recipient_name"]?.ToString(),
                        Relationship = reader["relationship"]?.ToString(),
                        Amount = reader["amount"] != DBNull.Value
                                            ? Convert.ToDecimal(reader["amount"]).ToString("N0") : "0",
                        Remarks = reader["remarks"]?.ToString(),
                    });
                }
            }
            catch (Exception ex) { MessageBox.Show($"Error loading bereaved data: {ex.Message}"); }
            return list;
        }

        public static string CheckAICSRestriction(string seniorId)
        {
            try
            {
                using var conn = DBConnection.GetConnection();
                conn.Open();
                string sql = @"SELECT TOP 1 amount, scheduled_date 
                       FROM AICS_TRANSACTION 
                       WHERE senior_id = @sid 
                       ORDER BY scheduled_date DESC";
                using var cmd = new OleDbCommand(sql, conn);
                cmd.Parameters.AddWithValue("@sid", seniorId);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    decimal amount = Convert.ToDecimal(reader["amount"]);
                    DateTime lastDate = Convert.ToDateTime(reader["scheduled_date"]);
                    if (lastDate.AddMonths(2) > DateTime.Now)
                        return $"You already received the 30% AICS transaction amounting to {amount:N2}. Please wait for 2 months before claiming again.";
                }
            }
            catch { }
            return "";
        }

        public static string CheckAPRRestriction(string seniorId)
        {
            try
            {
                using var conn = DBConnection.GetConnection();
                conn.Open();
                string sql = @"SELECT TOP 1 scheduled_date 
                       FROM APR_TRANSACTION 
                       WHERE senior_id = @sid 
                       ORDER BY scheduled_date DESC";
                using var cmd = new OleDbCommand(sql, conn);
                cmd.Parameters.AddWithValue("@sid", seniorId);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    DateTime lastDate = Convert.ToDateTime(reader["scheduled_date"]);
                    if (lastDate.AddYears(1) > DateTime.Now)
                        return "This senior citizen already received APR within the year. Please wait until next year before claiming again.";
                }
            }
            catch { }
            return "";
        }

        public static bool SaveAICS(string seniorId, decimal hospitalBill, string recordedBy)
        {
            try
            {
                using var conn = DBConnection.GetConnection();
                conn.Open();

                string infoSql = @"SELECT sc.barangay_id, sc.first_name, sc.last_name
                          FROM SENIOR_CITIZEN sc WHERE sc.senior_id = @sid";
                string barangayId = "", firstName = "", lastName = "";
                using (var infoCmd = new OleDbCommand(infoSql, conn))
                {
                    infoCmd.Parameters.AddWithValue("@sid", seniorId);
                    using var reader = infoCmd.ExecuteReader();
                    if (reader.Read())
                    {
                        barangayId = reader["barangay_id"]?.ToString() ?? "";
                        firstName = reader["first_name"]?.ToString() ?? "";
                        lastName = reader["last_name"]?.ToString() ?? "";
                    }
                }

                decimal percentage = 30;
                decimal total = hospitalBill * 0.3m;
                string aicsId = $"AICS-{DateTime.Now:yyyyMMddHHmmss}";

                string sql = @"INSERT INTO AICS_TRANSACTION
               (aics_id, senior_id, barangay_id, first_name, last_name,
                scheduled_date, amount, percentage, total, recorded_by, created_at)
               VALUES (@aid, @sid, @bid, @fn, @ln, Now(), @amt, @pct, @tot, @rb, Now())";
                using var cmd = new OleDbCommand(sql, conn);
                cmd.Parameters.AddWithValue("@aid", aicsId);
                cmd.Parameters.AddWithValue("@sid", seniorId);
                cmd.Parameters.AddWithValue("@bid", barangayId);
                cmd.Parameters.AddWithValue("@fn", firstName);
                cmd.Parameters.AddWithValue("@ln", lastName);
                cmd.Parameters.AddWithValue("@amt", hospitalBill);
                cmd.Parameters.AddWithValue("@pct", percentage);
                cmd.Parameters.AddWithValue("@tot", total);
                cmd.Parameters.AddWithValue("@rb", recordedBy);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex) { MessageBox.Show($"Error saving AICS: {ex.Message}"); return false; }
        }

        public static bool SaveAPR(string seniorId, decimal amount, string reason, string recordedBy)
        {
            try
            {
                using var conn = DBConnection.GetConnection();
                conn.Open();

                string infoSql = @"SELECT sc.barangay_id, sc.first_name, sc.last_name
                          FROM SENIOR_CITIZEN sc WHERE sc.senior_id = @sid";
                string barangayId = "", firstName = "", lastName = "";
                using (var infoCmd = new OleDbCommand(infoSql, conn))
                {
                    infoCmd.Parameters.AddWithValue("@sid", seniorId);
                    using var reader = infoCmd.ExecuteReader();
                    if (reader.Read())
                    {
                        barangayId = reader["barangay_id"]?.ToString() ?? "";
                        firstName = reader["first_name"]?.ToString() ?? "";
                        lastName = reader["last_name"]?.ToString() ?? "";
                    }
                }

                string aprId = $"APR-{DateTime.Now:yyyyMMddHHmmss}";
                string sql = @"INSERT INTO APR_TRANSACTION
               (apr_id, senior_id, barangay_id, first_name, last_name,
                scheduled_date, amount, total, recorded_by, created_at)
               VALUES (@aid, @sid, @bid, @fn, @ln, Now(), @amt, @tot, @rb, Now())";
                using var cmd = new OleDbCommand(sql, conn);
                cmd.Parameters.AddWithValue("@aid", aprId);
                cmd.Parameters.AddWithValue("@sid", seniorId);
                cmd.Parameters.AddWithValue("@bid", barangayId);
                cmd.Parameters.AddWithValue("@fn", firstName);
                cmd.Parameters.AddWithValue("@ln", lastName);
                cmd.Parameters.AddWithValue("@amt", amount);
                cmd.Parameters.AddWithValue("@tot", amount);
                cmd.Parameters.AddWithValue("@rb", recordedBy);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex) { MessageBox.Show($"Error saving APR: {ex.Message}"); return false; }
        }

        public static bool SaveBereaved(string seniorId, decimal amount,
                                string recipientName, string relationship,
                                string remarks, string recordedBy,
                                DateTime dateOfDeath)
        {
            try
            {
                using var conn = DBConnection.GetConnection();
                conn.Open();
                DateTime now = DateTime.Now;

                string infoSql = @"SELECT sc.barangay_id, sc.first_name, sc.last_name
                     FROM SENIOR_CITIZEN sc WHERE sc.senior_id = @sid";
                string barangayId = "1", firstName = "", lastName = "";
                using (var infoCmd = new OleDbCommand(infoSql, conn))
                {
                    infoCmd.Parameters.AddWithValue("@sid", seniorId);
                    using var reader = infoCmd.ExecuteReader();
                    if (reader.Read())
                    {
                        barangayId = reader["barangay_id"]?.ToString() ?? "1";
                        firstName = reader["first_name"]?.ToString() ?? "";
                        lastName = reader["last_name"]?.ToString() ?? "";
                    }
                }

                string deceasedId = $"DCD-{now:yyyyMMddHHmmss}";
                string deceasedSql = @"INSERT INTO DECEASED_RECORD
        (deceased_id, senior_id, date_of_death, reported_by, remakrs, recorded_by, created_at)
        VALUES (@did, @sid, @dod, @rpt, @rem, @rb, @cat)";
                using var decCmd = new OleDbCommand(deceasedSql, conn);
                decCmd.Parameters.Add("@did", OleDbType.VarChar).Value = deceasedId;
                decCmd.Parameters.Add("@sid", OleDbType.VarChar).Value = seniorId;
                decCmd.Parameters.Add("@dod", OleDbType.Date).Value = dateOfDeath;
                decCmd.Parameters.Add("@rpt", OleDbType.VarChar).Value = recipientName ?? "";
                decCmd.Parameters.Add("@rem", OleDbType.LongVarChar).Value = remarks ?? "";
                decCmd.Parameters.Add("@rb", OleDbType.VarChar).Value = recordedBy ?? "";
                decCmd.Parameters.Add("@cat", OleDbType.Date).Value = now;
                decCmd.ExecuteNonQuery();

                string bereavedId = $"BRV-{now:yyyyMMddHHmmss}";
                string bereavedSql = @"INSERT INTO BEREAVED_ASSISTANCE
    (bereaved_id, senior_id, deceased_id, barangay_id, first_name, last_name,
     actual_release_date, recipient_name, relationship,
     amount, total, remarks, recorded_by, created_at)
    VALUES (@bid, @sid, @did, @bgid, @fn, @ln,
            @ard, @rn, @rel,
            @amt, @tot, @rem, @rb, @cat)";
                using var brvCmd = new OleDbCommand(bereavedSql, conn);
                brvCmd.Parameters.Add("@bid", OleDbType.VarChar).Value = bereavedId;
                brvCmd.Parameters.Add("@sid", OleDbType.VarChar).Value = seniorId;
                brvCmd.Parameters.Add("@did", OleDbType.VarChar).Value = deceasedId;
                brvCmd.Parameters.Add("@bgid", OleDbType.VarChar).Value = barangayId;
                brvCmd.Parameters.Add("@fn", OleDbType.VarChar).Value = firstName;
                brvCmd.Parameters.Add("@ln", OleDbType.VarChar).Value = lastName;
                brvCmd.Parameters.Add("@ard", OleDbType.Date).Value = now;
                brvCmd.Parameters.Add("@rn", OleDbType.VarChar).Value = recipientName ?? "";
                brvCmd.Parameters.Add("@rel", OleDbType.VarChar).Value = relationship ?? "";
                brvCmd.Parameters.Add("@amt", OleDbType.Currency).Value = amount;
                brvCmd.Parameters.Add("@tot", OleDbType.Currency).Value = amount;
                brvCmd.Parameters.Add("@rem", OleDbType.LongVarChar).Value = remarks ?? "";
                brvCmd.Parameters.Add("@rb", OleDbType.VarChar).Value = recordedBy ?? "";
                brvCmd.Parameters.Add("@cat", OleDbType.Date).Value = now;
                brvCmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex) { MessageBox.Show($"Error saving bereaved: {ex.Message}"); return false; }
        }

        public static ObservableCollection<Models.QuarterlyItem> GetQuarterlyByStatus(string status)
        {
            var list = new ObservableCollection<Models.QuarterlyItem>();
            try
            {
                using var conn = DBConnection.GetConnection();
                conn.Open();
                string sql = @"SELECT sc.senior_id, sc.osca_id, sc.first_name, sc.last_name,
                       b.barangay_name, sc.barangay_id
                FROM (SENIOR_CITIZEN sc
                INNER JOIN BARANGAY b ON sc.barangay_id = b.barangay_id)
                LEFT JOIN DECEASED_RECORD d ON sc.senior_id = d.senior_id
                WHERE d.deceased_id IS NULL
                ORDER BY sc.last_name, sc.first_name";
                using var cmd = new OleDbCommand(sql, conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new Models.QuarterlyItem
                    {
                        SeniorId = reader["senior_id"]?.ToString() ?? "",
                        OscaId = reader["osca_id"]?.ToString() ?? "",
                        FullName = $"{reader["first_name"]} {reader["last_name"]}",
                        Date = DateTime.Now.ToString("MM-dd-yy"),
                        Barangay = reader["barangay_name"]?.ToString() ?? "",
                        Amount = "2,250",
                        Status = status
                    });
                }
            }
            catch (Exception ex) { MessageBox.Show($"Error loading quarterly: {ex.Message}"); }
            return list;
        }

        public static ObservableCollection<Models.QuarterlyItem> GetAllActiveSeniorsForQuarterly(string status)
        {
            var list = new ObservableCollection<Models.QuarterlyItem>();
            try
            {
                using var conn = DBConnection.GetConnection();
                conn.Open();
                string sql = @"SELECT sc.senior_id, sc.osca_id, sc.first_name, sc.last_name,
                       b.barangay_name, sc.created_at
                FROM (SENIOR_CITIZEN sc
                INNER JOIN BARANGAY b ON sc.barangay_id = b.barangay_id)
                LEFT JOIN DECEASED_RECORD d ON sc.senior_id = d.senior_id
                WHERE d.deceased_id IS NULL
                ORDER BY sc.last_name, sc.first_name";
                using var cmd = new OleDbCommand(sql, conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    DateTime registrationDate = reader["created_at"] != DBNull.Value
                        ? Convert.ToDateTime(reader["created_at"]) : DateTime.Now;

                    int registrationDay = registrationDate.Day;
                    DateTime today = DateTime.Today;
                    int currentMonth = today.Month;
                    int currentYear = today.Year;
                    int monthsSinceRegistration = ((currentYear - registrationDate.Year) * 12) + currentMonth - registrationDate.Month;
                    int quartersCompleted = monthsSinceRegistration / 3;
                    int nextQuarterMonth = registrationDate.Month + ((quartersCompleted + 1) * 3);

                    int releaseYear = currentYear;
                    int releaseMonth = nextQuarterMonth;
                    while (releaseMonth > 12) { releaseMonth -= 12; releaseYear++; }

                    int releaseDay = Math.Min(registrationDay, DateTime.DaysInMonth(releaseYear, releaseMonth));
                    DateTime nextReleaseDate = new DateTime(releaseYear, releaseMonth, releaseDay);

                    string itemStatus = status;
                    if (status == "Upcoming")
                    {
                        if ((nextReleaseDate - today).Days <= 30 && (nextReleaseDate - today).Days > 0)
                            itemStatus = "Upcoming";
                        else continue;
                    }
                    else if (status == "Released")
                    {
                        if (nextReleaseDate < today) itemStatus = "Released";
                        else continue;
                    }

                    list.Add(new Models.QuarterlyItem
                    {
                        SeniorId = reader["senior_id"]?.ToString() ?? "",
                        OscaId = reader["osca_id"]?.ToString() ?? "",
                        FullName = $"{reader["first_name"]} {reader["last_name"]}",
                        Date = registrationDate.ToString("MM-dd-yy"),
                        Barangay = reader["barangay_name"]?.ToString() ?? "",
                        Amount = "2,250",
                        ReleaseDate = nextReleaseDate.ToString("MM-dd-yy"),
                        Status = itemStatus
                    });
                }
            }
            catch (Exception ex) { MessageBox.Show($"Error loading quarterly: {ex.Message}"); }
            return list;
        }

        public static void GenerateQuarterlyPension(string recordedBy)
        {
            try
            {
                using var conn = DBConnection.GetConnection();
                conn.Open();
                int quarter = (DateTime.Now.Month - 1) / 3 + 1;
                int year = DateTime.Now.Year;

                string sql = @"SELECT sc.senior_id, sc.first_name, sc.last_name, sc.barangay_id
                          FROM SENIOR_CITIZEN sc
                          LEFT JOIN DECEASED_RECORD d ON sc.senior_id = d.senior_id
                          WHERE d.deceased_id IS NULL";

                var seniors = new System.Collections.Generic.List<(string id, string fn, string ln, string bid)>();
                using (var cmd = new OleDbCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        seniors.Add((reader["senior_id"]?.ToString() ?? "",
                                     reader["first_name"]?.ToString() ?? "",
                                     reader["last_name"]?.ToString() ?? "",
                                     reader["barangay_id"]?.ToString() ?? ""));
                }

                foreach (var s in seniors)
                {
                    string releaseId = $"QRL-{year}-{quarter:D2}-{s.id}";
                    string insertSql = @"INSERT INTO QUARTERLY_PENSION_RELEASE
                    (release_id, senior_id, barangay_id, first_name, last_name,
                     scheduled_date, amount, quarter, status, recorded_by, created_at)
                    VALUES (@rid, @sid, @bid, @fn, @ln, Now(), 2250, @qtr, 'Pending', @rb, Now())";
                    using var insertCmd = new OleDbCommand(insertSql, conn);
                    insertCmd.Parameters.AddWithValue("@rid", releaseId);
                    insertCmd.Parameters.AddWithValue("@sid", s.id);
                    insertCmd.Parameters.AddWithValue("@bid", s.bid);
                    insertCmd.Parameters.AddWithValue("@fn", s.fn);
                    insertCmd.Parameters.AddWithValue("@ln", s.ln);
                    insertCmd.Parameters.AddWithValue("@qtr", quarter);
                    insertCmd.Parameters.AddWithValue("@rb", recordedBy);
                    insertCmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex) { MessageBox.Show($"Error generating quarterly: {ex.Message}"); }
        }

        public static bool ReleaseQuarterly(string releaseId, string recordedBy)
        {
            try
            {
                using var conn = DBConnection.GetConnection();
                conn.Open();
                string sql = @"UPDATE QUARTERLY_PENSION_RELEASE 
                          SET status = 'Released', actual_release_date = Now(), recorded_by = @rb
                          WHERE release_id = @rid";
                using var cmd = new OleDbCommand(sql, conn);
                cmd.Parameters.AddWithValue("@rid", releaseId);
                cmd.Parameters.AddWithValue("@rb", recordedBy);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex) { MessageBox.Show($"Error releasing quarterly: {ex.Message}"); return false; }
        }

        public static ObservableCollection<RecentItem> GetRecent(int topN = 10)
        {
            var list = new ObservableCollection<RecentItem>();
            try
            {
                using var conn = DBConnection.GetConnection();
                conn.Open();

                TryAppendRecent(conn, list, "Quarterly",
                    $@"SELECT TOP {topN} q.scheduled_date AS trans_date, 
                           sc.last_name, sc.first_name, b.barangay_name, q.amount
                    FROM (QUARTERLY_PENSION_RELEASE q
                    INNER JOIN SENIOR_CITIZEN sc ON q.senior_id = sc.senior_id)
                    INNER JOIN BARANGAY b ON q.barangay_id = b.barangay_id
                    ORDER BY q.scheduled_date DESC");

                TryAppendRecent(conn, list, "AICS",
                    $@"SELECT TOP {topN} a.date AS trans_date,
                           sc.last_name, sc.first_name, b.barangay_name, a.amount
                    FROM (AICS_TRANSACTION a
                    INNER JOIN SENIOR_CITIZEN sc ON a.senior_id = sc.senior_id)
                    INNER JOIN BARANGAY b ON a.barangay_id = b.barangay_id
                    ORDER BY a.date DESC");

                TryAppendRecent(conn, list, "APR",
                    $@"SELECT TOP {topN} a.date AS trans_date,
                           sc.last_name, sc.first_name, b.barangay_name, a.amount
                    FROM (APR_TRANSACTION a
                    INNER JOIN SENIOR_CITIZEN sc ON a.senior_id = sc.senior_id)
                    INNER JOIN BARANGAY b ON a.barangay_id = b.barangay_id
                    ORDER BY a.date DESC");

                TryAppendRecent(conn, list, "BEREAVED",
                    $@"SELECT TOP {topN} ba.actual_release_date AS trans_date,
                           sc.last_name, sc.first_name, b.barangay_name, ba.amount
                    FROM (BEREAVED_ASSISTANCE ba
                    INNER JOIN SENIOR_CITIZEN sc ON ba.senior_id = sc.senior_id)
                    INNER JOIN BARANGAY b ON ba.barangay_id = b.barangay_id
                    ORDER BY ba.actual_release_date DESC");
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("not found") && !ex.Message.Contains("exist"))
                    MessageBox.Show($"Error loading recent: {ex.Message}");
            }
            return list;
        }

        private static void TryAppendRecent(OleDbConnection conn,
                                            ObservableCollection<RecentItem> list,
                                            string pensionType, string sql)
        {
            try { AppendRecent(conn, sql, list, pensionType); }
            catch { }
        }

        private static void AppendRecent(OleDbConnection conn, string sql,
                                         ObservableCollection<RecentItem> list, string pensionType)
        {
            using var cmd = new OleDbCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new RecentItem
                {
                    Date = reader["trans_date"] != DBNull.Value
                                    ? Convert.ToDateTime(reader["trans_date"]).ToString("MM-dd-yy") : "--",
                    LastName = reader["last_name"]?.ToString() ?? "",
                    FirstName = reader["first_name"]?.ToString() ?? "",
                    Barangay = reader["barangay_name"]?.ToString() ?? "",
                    Amount = reader["amount"] != DBNull.Value
                                    ? Convert.ToDecimal(reader["amount"]).ToString("N0") : "0",
                    PensionType = pensionType,
                });
            }
        }

        public static ObservableCollection<Models.DashboardRecentItem> GetDashboardRecent(string seniorIdFilter = null)
        {
            var list = new ObservableCollection<Models.DashboardRecentItem>();
            try
            {
                using var conn = DBConnection.GetConnection();
                conn.Open();

                string whereClause = string.IsNullOrEmpty(seniorIdFilter)
                    ? "" : "WHERE sc.senior_id = @sid";

                string sql = $@"SELECT sc.senior_id, sc.last_name, sc.first_name, 
                       sc.birthdate AS reg_date, b.barangay_name
                FROM (SENIOR_CITIZEN sc
                INNER JOIN BARANGAY b ON sc.barangay_id = b.barangay_id)
                {whereClause}
                ORDER BY sc.last_name, sc.first_name";

                using var cmd = new OleDbCommand(sql, conn);
                if (!string.IsNullOrEmpty(seniorIdFilter))
                    cmd.Parameters.AddWithValue("@sid", seniorIdFilter);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string sid = reader["senior_id"]?.ToString() ?? "";
                    int aics = CountForSenior(conn, "AICS_TRANSACTION", sid);
                    int apr = CountForSenior(conn, "APR_TRANSACTION", sid);
                    int q = CountForSenior(conn, "QUARTERLY_PENSION_RELEASE", sid);
                    int b = CountForSenior(conn, "BEREAVED_ASSISTANCE", sid);

                    list.Add(new Models.DashboardRecentItem
                    {
                        Date = reader["reg_date"] != DBNull.Value
                                             ? Convert.ToDateTime(reader["reg_date"]).ToString("MM-dd-yy") : "--",
                        LastName = reader["last_name"]?.ToString() ?? "",
                        FirstName = reader["first_name"]?.ToString() ?? "",
                        Barangay = reader["barangay_name"]?.ToString() ?? "",
                        AICSCount = aics.ToString(),
                        APRCount = apr.ToString(),
                        QuarterlyCount = q.ToString(),
                        BereavedCount = b.ToString(),
                        TotalPensions = (aics + apr + q + b).ToString()
                    });
                }
            }
            catch { }
            return list;
        }

        private static int CountForSenior(OleDbConnection conn, string table, string seniorId)
        {
            try
            {
                string sql = $"SELECT COUNT(*) FROM {table} WHERE senior_id = @sid";
                using var cmd = new OleDbCommand(sql, conn);
                cmd.Parameters.AddWithValue("@sid", seniorId);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch { return 0; }
        }
    }
}