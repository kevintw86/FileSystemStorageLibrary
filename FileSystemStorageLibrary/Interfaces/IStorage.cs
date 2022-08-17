using System;
using System.Threading.Tasks;

namespace FileSystemStorageLibrary.Interfaces
{
    public interface IStorage : IDisposable
    {
        /// <summary>
        /// Save the data of 'obj' (must be Serializable) to 'filePath' with encryption of the class properties defined in 'fieldNamesToCipher'
        /// </summary>
        /// <typeparam name="T">Specify the type of the object</typeparam>
        /// <param name="obj">The instance of the object</param>
        /// <param name="filePath">Target file path</param>
        void SaveData<T>(T obj, string filePath, params string[] propertiesOrFieldNamesToCipher) where T : class;

        /// <summary>
        /// Save the data of 'obj' (must be Serializable) to 'filePath' with encryption of the class properties defined in 'fieldNamesToCipher'
        /// </summary>
        /// <typeparam name="T">Specify the type of the object</typeparam>
        /// <param name="obj">The instance of the object</param>
        /// <param name="filePath">Target file path</param>
        Task SaveDataAsync<T>(T obj, string filePath, params string[] propertiesOrFieldNamesToCipher) where T : class;

        /// <summary>
        /// Load the data from 'filePath' with decryption of the class properties defined in 'fieldNamesToCipher' and Dererialize into T class
        /// </summary>
        /// <typeparam name="T">Specify the type of the object</typeparam>
        /// <param name="filePath">Target file path</param>
        /// <param name="fieldNamesToCipher">Name of properties to cipher</param>
        T LoadData<T>(string filePath, params string[] propertiesOrFieldNamesToUnCipher) where T : class;

        /// <summary>
        /// Load the data from 'filePath' with decryption of the class properties defined in 'fieldNamesToCipher' and Dererialize into T class
        /// </summary>
        /// <typeparam name="T">Specify the type of the object</typeparam>
        /// <param name="filePath">Target file path</param>
        /// <param name="fieldNamesToCipher">Name of properties to cipher</param>
        Task<T> LoadDataAsync<T>(string filePath, params string[] propertiesOrFieldNamesUnCipher) where T : class;
    }
}
