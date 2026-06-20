using System;

namespace SeniorCtzz
{
    public static class AppSession
    {
        public static string UserId { get; set; } = "";  
        public static string FullName { get; set; } = "";
        public static string Role { get; set; } = "";

        public static string GetInitials()
        {
            if (string.IsNullOrWhiteSpace(FullName)) return "??";
            var parts = FullName.Trim().Split(' ');
            return parts.Length >= 2
                ? $"{parts[0][0]}{parts[^1][0]}".ToUpper()
                : FullName[..Math.Min(2, FullName.Length)].ToUpper();
        }
    }
}
