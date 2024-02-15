using System;
using System.Security.Cryptography;
using System.Text;

namespace BattleLink.Common
{
    // https://learn.microsoft.com/fr-fr/azure/communication-services/tutorials/hmac-header-tutorial
    public class SignHttp
    {
        public static string ComputeContentHash(string content)
        {
            using var sha256 = SHA256.Create();
            byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(content));
            return Convert.ToBase64String(hashedBytes);
        }

        public static string ComputeContentHash(byte[] content)
        {
            using var sha256 = SHA256.Create();
            byte[] hashedBytes = sha256.ComputeHash(content);
            return Convert.ToBase64String(hashedBytes);
        }

        public static string ComputeSignature(string stringToSign)
        {
            string secret = "resourceAccessKey";
            //byte[] base64Key = Convert.FromBase64String(secret);
            byte[] base64Key = Encoding.UTF8.GetBytes(secret);
            using var hmacsha256 = new HMACSHA256(base64Key);
            var bytes = Encoding.UTF8.GetBytes(stringToSign);
            var hashedBytes = hmacsha256.ComputeHash(bytes);
            return Convert.ToBase64String(hashedBytes);
        }
    }
}
