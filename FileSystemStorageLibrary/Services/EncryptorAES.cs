using FileSystemStorageLibrary.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace FileSystemStorageLibrary.Services
{
    public class EncryptorAES : IEncryptor
    {
        #region Fields

        private static Random random = new Random();

        #endregion

        #region Public methods

        public string RandomLetter(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public string EncryptString(string plainText)
        {
            string leadingLetter;
            char leadingChar;
            int leadingIndex;
            string passPhrase;
            byte[] iv = new byte[16];
            byte[] array;
            string base64String;
            string finalString;

            if (string.IsNullOrWhiteSpace(plainText))
                return plainText;

            while (true)
            {
                leadingLetter = RandomLetter(1);
                leadingChar = leadingLetter.ToLower().FirstOrDefault();
                leadingIndex = leadingChar % 32;
                passPhrase = RandomLetter(16);
                
                if (leadingIndex < plainText.Length)
                    break;
            }

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(passPhrase);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using MemoryStream memoryStream = new MemoryStream();
                using CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write);
                using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                {
                    streamWriter.Write(plainText);
                }

                array = memoryStream.ToArray();
            }

            base64String = Convert.ToBase64String(array);
            finalString = leadingLetter + base64String.Substring(0, leadingIndex) + passPhrase + base64String[leadingIndex..];
            return finalString;
        }

        public string DecryptString(string cipherText)
        {
            try
            {
                cipherText = GetCipherText(cipherText, out string key);

                byte[] iv = new byte[16];
                byte[] buffer = Convert.FromBase64String(cipherText);

                using Aes aes = Aes.Create();
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using MemoryStream memoryStream = new MemoryStream(buffer);
                using CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read);
                using StreamReader streamReader = new StreamReader((Stream)cryptoStream);
                return streamReader.ReadToEnd();
            }
            catch { }

            return cipherText;
        }

        #endregion

        #region Private methods

        private string GetCipherText(string cipherText, out string passPhrase)
        {
            string leadingLetter = cipherText.ToArray()[0].ToString();
            char leadingChar = leadingLetter.ToLower().FirstOrDefault();
            int leadingIndex = leadingChar % 32;
            string cipherTextWithoutLeadingLetter = cipherText[1..];
            passPhrase = cipherTextWithoutLeadingLetter.Substring(leadingIndex, 16);
            return cipherTextWithoutLeadingLetter.Substring(0, leadingIndex) + cipherTextWithoutLeadingLetter[(leadingIndex + 16)..];
        }

        #endregion

        #region IDisposable

        public virtual void Dispose()
        {
            random = null;
        }

        #endregion
    }
}
