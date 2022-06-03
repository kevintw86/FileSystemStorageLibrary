using System;

namespace FileSystemStorageLibrary.Interfaces
{
    public interface IEncryptor : IDisposable
    {
        string EncryptString(string plainText);
        string DecryptString(string cipherText);
    }
}
