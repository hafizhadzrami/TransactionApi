using System.Security.Cryptography;
using System.Text;

namespace TransactionApi.Utilities
{
    public class Crypto
    {
        private readonly byte[] _key; 
        private readonly byte[] _iv;  

        public Crypto(IConfiguration config)
        {
            
            var keyB64 = config["Logging:SensitiveKey"];
            var ivB64  = config["Logging:SensitiveIV"];
            if (!string.IsNullOrWhiteSpace(keyB64) && !string.IsNullOrWhiteSpace(ivB64))
            {
                _key = Convert.FromBase64String(keyB64);
                _iv  = Convert.FromBase64String(ivB64);
            }
            else
            {
                
                _key = SHA256.HashData(Encoding.UTF8.GetBytes("fallback-demo-key-please-set-in-appsettings"));
                _iv  = MD5.HashData(Encoding.UTF8.GetBytes("fallback-demo-iv-please-set-in-appsettings"));
            }
        }

        public string Encrypt(string plain)
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV  = _iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var enc = aes.CreateEncryptor();
            var bytes = Encoding.UTF8.GetBytes(plain);
            var cipher = enc.TransformFinalBlock(bytes, 0, bytes.Length);
            return Convert.ToBase64String(cipher);
        }
    }
}
