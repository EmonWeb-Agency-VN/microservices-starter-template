using System.Security.Cryptography;
using System.Text;

namespace Common.SharedKernel.Utilities
{
    public class RandomStringUtils
    {
        public static string GetRandomString()
        {
            var randomNumber = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        public static string Hash(string input)
        {
            using (var algorithm = SHA256.Create())
            {
                var hashBytes = algorithm.ComputeHash(Encoding.UTF8.GetBytes(input));
                return Convert.ToBase64String(hashBytes);
            }
        }
    }
}
