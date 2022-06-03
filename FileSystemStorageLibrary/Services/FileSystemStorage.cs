using FileSystemStorageLibrary.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FileSystemStorageLibrary.Services
{
    public class FileSystemStorage : IStorage
    {
        #region Fields

        private IEncryptor Encryptor;
        private ILoggerService LoggerService;

        #endregion

        /// <summary>
        /// Build the instance of <see cref="FileSystemStorage"/> with the instance of <see cref="EncryptorAES"/>, or create your own class which implements <see cref="IEncryptor"/>, that stands for encryption logic
        /// </summary>
        /// <param name="encryptor">IEncryptor, that does encryption logic. May use the instance of <see cref="EncryptorAES"/>.</param>
        /// <param name="loggerService">ILoggerService that is used to log save/load errors. May be null</param>
        public FileSystemStorage(IEncryptor encryptor, ILoggerService loggerService = null)
        {
            this.Encryptor = encryptor;
            this.LoggerService = loggerService;
        }

        #region Public methods

        /// <summary>
        /// Saves class instance 'obj' to a path, defined in 'filePath'. All the fields and properties defined in 'propertiesOrFieldNamesToCipher' param will be found in 'obj' itself and its child classes recursively and encrypted with the logic implemented by the <see cref="FileSystemStorage.Encryptor"/>.
        /// </summary>
        /// <typeparam name="T">Type of class to save</typeparam>
        /// <param name="obj">The instance of a class to save</param>
        /// <param name="filePath">Path to save a file</param>
        /// <param name="propertiesOrFieldNamesToCipher">field or properties names to encrypt</param>
        /// <remarks>Please be aware that fields and properties of the 'obj' and its childs defined in 'propertiesOrFieldNamesToCipher' will stay ENCRYPTED after save.</remarks>
        public void SaveData<T>(T obj, string filePath, params string[] propertiesOrFieldNamesToCipher) where T : class
        {
            try
            {
                var emailCredentialsCiphered = CipherData(obj, propertiesOrFieldNamesToCipher);
                var emailCredentialsCipheredSerialized = JsonConvert.SerializeObject(emailCredentialsCiphered);

                File.WriteAllText(filePath, emailCredentialsCipheredSerialized, Encoding.UTF8);
            }
            catch (Exception e)
            {
                this.LoggerService?.Log($"Error: {e}");
                throw;
            }
        }

        /// <summary>
        /// Saves class instance 'obj' to a path, defined in 'filePath'. All the fields and properties defined in 'propertiesOrFieldNamesToCipher' param will be found in 'obj' itself and its child classes recursively and encrypted with the logic implemented by the <see cref="FileSystemStorage.Encryptor"/>.
        /// </summary>
        /// <typeparam name="T">Type of class to save</typeparam>
        /// <param name="obj">The instance of a class to save</param>
        /// <param name="filePath">Path to save a file</param>
        /// <param name="propertiesOrFieldNamesToCipher">field or properties names to encrypt</param>
        /// <remarks>Please be aware that fields and properties of the 'obj' and its childs defined in 'propertiesOrFieldNamesToCipher' will stay ENCRYPTED after save.</remarks>
        public async Task SaveDataAsync<T>(T obj, string filePath, params string[] propertiesOrFieldNamesToCipher) where T : class
        {
            try
            {
                var emailCredentialsCiphered = CipherData(obj, propertiesOrFieldNamesToCipher);
                var emailCredentialsCipheredSerialized = JsonConvert.SerializeObject(emailCredentialsCiphered);

                await File.WriteAllTextAsync(filePath, emailCredentialsCipheredSerialized, Encoding.UTF8);
            }
            catch (Exception e)
            {
                this.LoggerService?.Log($"Error: {e}");
                throw;
            }
        }

        /// <summary>
        /// Loads class instance 'obj' from path, defined in 'filePath'. All the fields and properties defined in 'propertiesOrFieldNamesToCipher' param will be found in 'obj' itself and its child classes recursively and decrypted with the logic implemented by the <see cref="FileSystemStorage.Encryptor"/>.
        /// </summary>
        /// <typeparam name="T">Type of class to save</typeparam>
        /// <param name="filePath">Path to a file</param>
        /// <param name="propertiesOrFieldNamesToCipher">field or properties names to encrypt</param>
        /// <returns>Class instance <see cref="T"/>, loaded and encrypted properly</returns>
        public T LoadData<T>(string filePath, params string[] propertiesOrFieldNamesToUnCipher) where T : class
        {
            if (!File.Exists(filePath))
                return default;

            try
            {
                string emailCredentialsCiphered = File.ReadAllText(filePath, Encoding.UTF8);
                T emailCredentialsCipheredDeserialized = JsonConvert.DeserializeObject<T>(emailCredentialsCiphered);

                return UnCipherData(emailCredentialsCipheredDeserialized, propertiesOrFieldNamesToUnCipher);
            }
            catch (Exception e)
            {
                this.LoggerService?.Log($"Error: {e}");
            }

            return default;
        }

        /// <summary>
        /// Loads class instance 'obj' from path, defined in 'filePath'. All the fields and properties defined in 'propertiesOrFieldNamesToCipher' param will be found in 'obj' itself and its child classes recursively and decrypted with the logic implemented by the <see cref="FileSystemStorage.Encryptor"/>.
        /// </summary>
        /// <typeparam name="T">Type of class to save</typeparam>
        /// <param name="filePath">Path to a file</param>
        /// <param name="propertiesOrFieldNamesToCipher">field or properties names to encrypt</param>
        /// <returns>Class instance <see cref="T"/>, loaded and encrypted properly</returns>
        public async Task<T> LoadDataAsync<T>(string filePath, params string[] propertiesOrFieldNamesToUnCipher) where T : class
        {
            if (!File.Exists(filePath))
                return default;

            try
            {
                string emailCredentialsCiphered = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
                T emailCredentialsCipheredDeserialized = JsonConvert.DeserializeObject<T>(emailCredentialsCiphered);

                return UnCipherData(emailCredentialsCipheredDeserialized, propertiesOrFieldNamesToUnCipher);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.Print($"ERROR: {e}");
                this.LoggerService?.Log($"Error: {e}");
            }

            return default;
        }

        #endregion

        #region Private methods

        private T CipherData<T>(T obj, IEnumerable<string> propertiesOrFieldNamesToCipher)
        {
            var item = obj;
            this.CipherObject(ref item, propertiesOrFieldNamesToCipher);
            return item;
        }

        private T UnCipherData<T>(T obj, string[] propertiesOrFieldNamesToUnCipher)
        {
            var item = obj;
            this.UnCipherObject(ref item, propertiesOrFieldNamesToUnCipher);
            return item;
        }

        private void CipherObject<T>(ref T obj, IEnumerable<string> propertiesOrFieldNamesToCipher)
        {
            if (obj == null)
                return;

            FieldInfo[] fieldInfos = obj.GetType().GetFields();
            foreach (var fieldInfo in fieldInfos)
            {
                if (fieldInfo.FieldType.GetInterface("IList", true) != null)
                {
                    var list = fieldInfo.GetValue(obj) as IList;

                    for (int i = 0; i < list.Count; i++)
                    {
                        var itm = list[i];
                        this.CipherObject(ref itm, propertiesOrFieldNamesToCipher);
                    }
                }
                else if (fieldInfo.FieldType.IsArray)
                {
                    var arr = fieldInfo.GetValue(obj) as Array;
                    for (int i = 0; i < arr.Length; i++)
                    {
                        var itm = arr.GetValue(i);
                        this.CipherObject(ref itm, propertiesOrFieldNamesToCipher);
                    }
                }
                else if (fieldInfo.FieldType.IsClass && !fieldInfo.FieldType.FullName.StartsWith("System."))
                {
                    var itm = fieldInfo.GetValue(obj);
                    this.CipherObject(ref itm, propertiesOrFieldNamesToCipher);
                }
                else if (propertiesOrFieldNamesToCipher.Contains(fieldInfo.Name))
                {
                    var instance = fieldInfo.GetValue(obj);
                    string sourceValue = instance.ToString();
                    string encryptedValue = Encryptor.EncryptString(sourceValue);
                    fieldInfo.SetValue(obj, encryptedValue);
                }
            }

            PropertyInfo[] propertyInfos = obj.GetType().GetProperties();
            foreach (var propInfo in propertyInfos)
            {
                if (propInfo.PropertyType.GetInterface("IList", true) != null)
                {
                    var list = propInfo.GetValue(obj) as IList;

                    for (int i = 0; i < list.Count; i++)
                    {
                        var itm = list[i];
                        this.CipherObject(ref itm, propertiesOrFieldNamesToCipher);
                    }
                }
                else if (propInfo.PropertyType.IsArray)
                {
                    var arr = propInfo.GetValue(obj) as Array;
                    for (int i = 0; i < arr.Length; i++)
                    {
                        var itm = arr.GetValue(i);
                        this.CipherObject(ref itm, propertiesOrFieldNamesToCipher);
                    }
                }
                else if (propInfo.PropertyType.IsClass && !propInfo.PropertyType.FullName.StartsWith("System."))
                {
                    var itm = propInfo.GetValue(obj);
                    this.CipherObject(ref itm, propertiesOrFieldNamesToCipher);
                }
                else if (propertiesOrFieldNamesToCipher.Contains(propInfo.Name))
                {
                    var instance = propInfo.GetValue(obj);
                    string sourceValue = instance.ToString();
                    string encryptedValue = Encryptor.EncryptString(sourceValue);
                    propInfo.SetValue(obj, encryptedValue);
                }
            }
        }

        private void UnCipherObject<T>(ref T obj, IEnumerable<string> propertiesOrFieldNamesToUnCipher)
        {
            if (obj == null)
                return;

            FieldInfo[] fieldInfos = obj.GetType().GetFields();
            foreach (FieldInfo fieldInfo in fieldInfos)
            {
                if (fieldInfo.FieldType.GetInterface("IList", true) != null)
                {
                    var list = fieldInfo.GetValue(obj) as IList;

                    for (int i = 0; i < list.Count; i++)
                    {
                        var itm = list[i];
                        this.UnCipherObject(ref itm, propertiesOrFieldNamesToUnCipher);
                    }
                }
                else if (fieldInfo.FieldType.IsArray)
                {
                    var arr = fieldInfo.GetValue(obj) as Array;
                    for (int i = 0; i < arr.Length; i++)
                    {
                        var itm = arr.GetValue(i);
                        this.UnCipherObject(ref itm, propertiesOrFieldNamesToUnCipher);
                    }
                }
                else if (fieldInfo.FieldType.IsClass && !fieldInfo.FieldType.FullName.StartsWith("System."))
                {
                    var itm = fieldInfo.GetValue(obj);
                    this.UnCipherObject(ref itm, propertiesOrFieldNamesToUnCipher);
                }
                else if (propertiesOrFieldNamesToUnCipher.Contains(fieldInfo.Name))
                {
                    string encryptedValue = fieldInfo.GetValue(obj).ToString();
                    string decryptedValue = Encryptor.DecryptString(encryptedValue);
                    fieldInfo.SetValue(obj, decryptedValue);
                }
            }

            PropertyInfo[] PropertyInfos = obj.GetType().GetProperties();
            foreach (var propInfo in PropertyInfos)
            {
                if (propInfo.PropertyType.GetInterface("IList", true) != null)
                {
                    var list = propInfo.GetValue(obj) as IList;

                    for (int i = 0; i < list.Count; i++)
                    {
                        var itm = list[i];
                        this.UnCipherObject(ref itm, propertiesOrFieldNamesToUnCipher);
                    }
                }
                else if (propInfo.PropertyType.IsArray)
                {
                    var arr = propInfo.GetValue(obj) as Array;
                    for (int i = 0; i < arr.Length; i++)
                    {
                        var itm = arr.GetValue(i);
                        this.UnCipherObject(ref itm, propertiesOrFieldNamesToUnCipher);
                    }
                }
                else if (propInfo.PropertyType.IsClass && !propInfo.PropertyType.FullName.StartsWith("System."))
                {
                    var itm = propInfo.GetValue(obj);
                    this.UnCipherObject(ref itm, propertiesOrFieldNamesToUnCipher);
                }
                else if (propertiesOrFieldNamesToUnCipher.Contains(propInfo.Name))
                {
                    string encryptedValue = propInfo.GetValue(obj).ToString();
                    string decryptedValue = Encryptor.DecryptString(encryptedValue);
                    propInfo.SetValue(obj, decryptedValue);
                }
            }
        }

        #endregion

        #region IDisposable

        public virtual void Dispose()
        {
            this.Encryptor?.Dispose();
            this.Encryptor = null;

            this.LoggerService?.Dispose();
            this.LoggerService = null;
        }

        #endregion
    }
}
