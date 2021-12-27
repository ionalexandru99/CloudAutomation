using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using CloudAutomation.Application.Interfaces.Encryption;
using CloudAutomation.Utils.Settings;
using Microsoft.Extensions.Options;

namespace CloudAutomation.Application.Implementations.Encryption
{
    public class Encryption : IEncryption
    {
        private readonly EncryptionSettings _encryptionSettings;
        private readonly byte[] Iv = new byte[16];

        public Encryption(IOptions<EncryptionSettings> encryptionSettings)
        {
            _encryptionSettings = encryptionSettings.Value;
        }

        public string Encrypt(string value)
        {
            byte[] encrypted;

            using (var aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(_encryptionSettings.Token);
                aesAlg.IV = Iv;

                var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(value);
                        }

                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            return ConvertStringToHex(Convert.ToBase64String(encrypted), Encoding.Unicode);
        }

        public string Decrypt(string value)
        {
            var buffer = Convert.FromBase64String(ConvertHexToString(value, Encoding.Unicode));

            string plaintext = null;

            using (var aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(_encryptionSettings.Token);
                aesAlg.IV = Iv;

                var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (var msDecrypt = new MemoryStream(buffer))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }

        private static string ConvertStringToHex(string input, Encoding encoding)
        {
            var stringBytes = encoding.GetBytes(input);
            var sbBytes = new StringBuilder(stringBytes.Length * 2);
            foreach (var b in stringBytes)
                sbBytes.AppendFormat("{0:X2}", b);
            return sbBytes.ToString();
        }

        private static string ConvertHexToString(string hexInput, Encoding encoding)
        {
            var numberChars = hexInput.Length;
            var bytes = new byte[numberChars / 2];
            for (var i = 0; i < numberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hexInput.Substring(i, 2), 16);
            return encoding.GetString(bytes);
        }
    }
}