using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousCompanionAppService
{
    public class RijndaelHelper : IDisposable
    {
        Rijndael rijndael;
        UTF8Encoding encoding;

        public RijndaelHelper(byte[] key, byte[] vector)
        {
            encoding = new UTF8Encoding();
            rijndael = Rijndael.Create();
            rijndael.Key = key;
            rijndael.IV = vector;
        }

        public string Encrypt(string valueToEncrypt)
        {
            var bytes = encoding.GetBytes(valueToEncrypt);
            using (var encryptor = rijndael.CreateEncryptor())
            using (var stream = new MemoryStream())
            using (var crypto = new CryptoStream(stream, encryptor, CryptoStreamMode.Write))
            {
                crypto.Write(bytes, 0, bytes.Length);
                crypto.FlushFinalBlock();
                stream.Position = 0;
                var encrypted = new byte[stream.Length];
                stream.Read(encrypted, 0, encrypted.Length);
                return Convert.ToBase64String(encrypted);
            }
        }

        public string Decrypt(string encryptedValue)
        {
            using (var decryptor = rijndael.CreateDecryptor())
            using (var stream = new MemoryStream())
            using (var crypto = new CryptoStream(stream, decryptor, CryptoStreamMode.Write))
            {
                var encrypted = Convert.FromBase64String(encryptedValue);
                crypto.Write(encrypted, 0, encrypted.Length);
                crypto.FlushFinalBlock();
                stream.Position = 0;
                var decryptedBytes = new Byte[stream.Length];
                stream.Read(decryptedBytes, 0, decryptedBytes.Length);
                return encoding.GetString(decryptedBytes);
            }
        }

        public void Dispose()
        {
            if (rijndael != null)
            {
                rijndael.Dispose();
            }
        }
    }
}
