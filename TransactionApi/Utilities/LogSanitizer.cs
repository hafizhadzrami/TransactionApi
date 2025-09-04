using System.Text.RegularExpressions;

namespace TransactionApi.Utilities
{
    public static class LogSanitizer
    {
        public static string Sanitize(string json)
        {
            if (string.IsNullOrEmpty(json)) return json;

            return Regex.Replace(
                json,
                "(\"partnerPassword\"\\s*:\\s*\").*?(\")",
                "$1<REDACTED>$2",
                RegexOptions.IgnoreCase
            );
        }
    }
}
