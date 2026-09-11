using System.Security.Cryptography;
using System.Text;

namespace UserService.Application.Utils;

public static class Hash
{
    public static string GenerateHash(string input)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));

        return Convert.ToHexString(bytes);
    }
}