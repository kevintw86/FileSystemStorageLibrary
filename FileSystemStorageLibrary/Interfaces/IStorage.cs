using System;
using System.Threading.Tasks;

namespace FileSystemStorageLibrary.Interfaces
{
    public interface IStorage : IDisposable
    {
        void SaveData<T>(T obj, string filePath, params string[] propertiesOrFieldNamesToCipher) where T : class;
        Task SaveDataAsync<T>(T obj, string filePath, params string[] propertiesOrFieldNamesToCipher) where T : class;

        T LoadData<T>(string filePath, params string[] propertiesOrFieldNamesToUnCipher) where T : class;
        Task<T> LoadDataAsync<T>(string filePath, params string[] propertiesOrFieldNamesUnCipher) where T : class;
    }
}
