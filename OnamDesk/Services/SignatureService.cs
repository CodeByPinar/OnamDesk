using System.Security.Cryptography;
using System.Text;

namespace OnamDesk.Services
{
    public class SignatureService
    {
        public string GenerateSignatureHash(string signatureData)
        {
            if (string.IsNullOrWhiteSpace(signatureData))
            {
                return string.Empty;
            }

            var rawBytes = Encoding.UTF8.GetBytes(signatureData);
            var hashBytes = SHA256.HashData(rawBytes);

            return Convert.ToHexString(hashBytes);
        }

        public string GenerateSignatureHash(string signatureData, DateTime signedAt, string computerName)
        {
            return GenerateSignatureHash(signatureData);
        }

        public string CreateTestSignatureData(string patientName)
        {
            if (string.IsNullOrWhiteSpace(patientName))
            {
                patientName = "Unknown Patient";
            }

            var rawSignature = $"TEST_SIGNATURE::{patientName.Trim()}::{DateTime.UtcNow:O}";
            var rawBytes = Encoding.UTF8.GetBytes(rawSignature);

            return Convert.ToBase64String(rawBytes);
        }

        public bool VerifySignatureHash(string signatureData, string expectedHash)
        {
            if (string.IsNullOrWhiteSpace(signatureData) || string.IsNullOrWhiteSpace(expectedHash))
            {
                return false;
            }

            var calculatedHash = GenerateSignatureHash(signatureData);

            return string.Equals(
                calculatedHash,
                expectedHash,
                StringComparison.OrdinalIgnoreCase);
        }

        public bool VerifySignatureHash(string signatureData, DateTime signedAt, string computerName, string expectedHash)
        {
            return VerifySignatureHash(signatureData, expectedHash);
        }
    }
}