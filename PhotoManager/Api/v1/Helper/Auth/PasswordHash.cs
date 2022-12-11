namespace PhotoManager.Api.v1.Helper.Auth;

using System.Text;
using System.Security.Cryptography;

public static class PasswordHash
{
    public static string Encode(string text, string key)
    {
        ASCIIEncoding encoding = new ASCIIEncoding();

        Byte[] textBytes = encoding.GetBytes(text);
        Byte[] keyBytes = encoding.GetBytes(key);

        using (HMACSHA256 hash = new HMACSHA256(keyBytes))
        {
            Byte[] hashBytes = hash.ComputeHash(textBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
}