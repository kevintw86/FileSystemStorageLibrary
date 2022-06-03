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

        public FileSystemStorage(IEncryptor encryptor, ILoggerService loggerService = null)
        {
            this.Encryptor = encryptor;
            this.LoggerService = loggerService;
        }

        #region Public methods

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
