using System.Security.Cryptography;
using System.Text;

namespace TransactionApi.Services
{
    public class SignatureService
    {
        public string GenerateSignature(string partnerKey, string partnerRefNo, string partnerPassword, long timestamp, string totalAmount)
        {
            var data = $"{partnerKey}{partnerRefNo}{partnerPassword}{timestamp}{totalAmount}";
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToHexString(hash);
        }
    }
}
