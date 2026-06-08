using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Nhakhoa.Services
{
    public static class EncryptionHelper
    {
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("NhakhoaSecretKeyAES256Length32!!"); // 32 bytes for AES-256
        private static readonly byte[] Iv = Encoding.UTF8.GetBytes("NhakhoaIV16Bytes"); // 16 bytes for AES IV

        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return string.Empty;

            using (var aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = Iv;
                var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (var sw = new StreamWriter(cs))
                        {
                            sw.Write(plainText);
                        }
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        public static string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return string.Empty;

            try
            {
                using (var aes = Aes.Create())
                {
                    aes.Key = Key;
                    aes.IV = Iv;
                    var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                    using (var ms = new MemoryStream(Convert.FromBase64String(cipherText)))
                    {
                        using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                        {
                            using (var sr = new StreamReader(cs))
                            {
                                return sr.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch
            {
                return "DECRYPTION_ERROR";
            }
        }
    }
}
