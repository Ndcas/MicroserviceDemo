using System.Security.Cryptography;
using System.Text;

namespace user.Utils
{
    public class Hash
    {
        public static string GenerateHash(string input)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));

            return Convert.ToHexString(bytes);
        }
    }
}
