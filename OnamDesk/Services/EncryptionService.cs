using System.Security.Cryptography;
using System.Text;

namespace OnamDesk.Services
{
    public class EncryptionService
    {
        public string Encrypt(string plainText)
        {
            if (string.IsNullOrWhiteSpace(plainText))
            {
                return string.Empty;
            }

            var plainBytes = Encoding.UTF8.GetBytes(plainText);

            return Convert.ToBase64String(plainBytes);
        }

        public string Decrypt(string encryptedText)
        {
            if (string.IsNullOrWhiteSpace(encryptedText))
            {
                return string.Empty;
            }

            try
            {
                var encryptedBytes = Convert.FromBase64String(encryptedText);

                return Encoding.UTF8.GetString(encryptedBytes);
            }
            catch
            {
                return string.Empty;
            }
        }

        public string GenerateSha256Hash(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            var inputBytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = SHA256.HashData(inputBytes);

            return Convert.ToHexString(hashBytes);
        }
    }
}