using System.Security.Cryptography;
using System.Text;

namespace Common.SharedKernel.Utilities
{
    public class AesEncryptionUtil
    {
        private static readonly byte[] rawKey =
        {
            201, 219, 55, 183, 156, 64, 85, 204,
            201, 219, 55, 183, 156, 64, 85, 204,
        };

        private static byte[] InnerEncrypt(string plainText, byte[] key)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.GenerateIV();
                byte[] iv = aesAlg.IV;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    msEncrypt.Write(iv, 0, iv.Length); // Write IV at the beginning of the stream
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt, Encoding.UTF8))
                        {
                            swEncrypt.Write(plainText);
                        }
                    }
                    return msEncrypt.ToArray();
                }
            }
        }

        private static string InnerDecrypt(byte[] encryptionData, byte[] key)
        {
            using (Aes aesAlg = Aes.Create())
            {
                byte[] iv = new byte[aesAlg.BlockSize / 8];
                byte[] cipherText = new byte[encryptionData.Length - iv.Length];

                Array.Copy(encryptionData, iv, iv.Length);
                Array.Copy(encryptionData, iv.Length, cipherText, 0, cipherText.Length);

                aesAlg.Key = key;
                aesAlg.IV = iv;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt, Encoding.UTF8))
                        {
                            // Read the decrypted bytes from the stream and return as a string
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }

        public static string EncryptStringWithRawKey(string plainString)
        {
            //var buffer = Encoding.UTF8.GetBytes(plainString);
            var encryptMessageBytes = InnerEncrypt(plainString, rawKey);
            return Convert.ToBase64String(encryptMessageBytes);
        }

        public static string DecryptStringWithRawKey(string encryptedString)
        {
            var buffer = Convert.FromBase64String(encryptedString);
            var decryptMessageBytes = InnerDecrypt(buffer, rawKey);
            return decryptMessageBytes;
        }
    }

    //public static class InnerAPIValidateHelper
    //{
    //    public static bool ValidteSignature(string signature, string validText = "")
    //    {
    //        var isValidPassed = false;
    //        if (!string.IsNullOrWhiteSpace(signature))
    //        {
    //            if (string.IsNullOrEmpty(validText))
    //            {
    //                validText = RuntimeContext.Config.TenantId;
    //            }
    //            isValidPassed = AesEncryptionUtil.DecryptStringWithCustomKey(signature, RuntimeContext.Config.InnerAPISecret).EqualsIgnoreCase(validText);
    //        }
    //        return isValidPassed;
    //    }

    //    public static string GenerateSignature(string plainText = "")
    //    {
    //        if (string.IsNullOrEmpty(plainText))
    //        {
    //            plainText = RuntimeContext.Config.TenantId;
    //        }
    //        return AesEncryptionUtil.EncryptStringWithCustomKey(plainText, RuntimeContext.Config.InnerAPISecret);
    //    }
    //}
}
