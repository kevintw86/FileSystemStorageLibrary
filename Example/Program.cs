using FileSystemStorageLibrary.Interfaces;
using FileSystemStorageLibrary.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Example
{
    class Program
    {
        static void Main(string[] args)
        {
            var logger = new MyLogger();
            var encryptor = new EncryptorAES();
            var storage = new FileSystemStorage(encryptor, logger);

            var someClass = new SomeClass
            {
                PasswordField = "Some password which I wouldn't share",
                SomeStringField = "StringField",
                SomeBoolField = true,
                SomeIntField = 1,
            };

            string filePath = Path.Combine(AppContext.BaseDirectory, "SavedFile.txt");

            Console.WriteLine("The class with the following fields has been generated:\n");

            foreach (var fieldInfo in typeof(SomeClass).GetFields())
                logger?.Log($"{fieldInfo.Name} ({fieldInfo.FieldType}): {fieldInfo.GetValue(someClass)}");

            foreach (var propertyInfo in typeof(SomeClass).GetProperties())
                logger?.Log($"{propertyInfo.Name} ({propertyInfo.PropertyType}): {propertyInfo.GetValue(someClass)}");

            Console.WriteLine($"\nPress any key to write text file to a disk:\n{filePath}...");
            Console.ReadLine();

            storage.SaveData(obj: someClass, filePath: filePath, propertiesOrFieldNamesToCipher: nameof(SomeClass.PasswordField));
            
            if (File.Exists(filePath))
            {
                logger.Log($"The file \"{filePath}\" has been saved to a disk!\nHere is how it looks now:\n");

                foreach (var fieldInfo in typeof(SomeClass).GetFields())
                    logger?.Log($"{fieldInfo.Name} ({fieldInfo.FieldType}): {fieldInfo.GetValue(someClass)}");

                foreach (var propertyInfo in typeof(SomeClass).GetProperties())
                    logger?.Log($"{propertyInfo.Name} ({propertyInfo.PropertyType}): {propertyInfo.GetValue(someClass)}");

                Console.WriteLine("\nPress any key to load and decrypt it from a disk...");
                Console.ReadLine();

                var loadedClass = storage.LoadData<SomeClass>(filePath, nameof(SomeClass.PasswordField));
                
                logger?.Log("The file has been successfully loaded from disk. Here are its fields:\n");
                
                foreach (var fieldInfo in typeof(SomeClass).GetFields())
                    logger?.Log($"{fieldInfo.Name} ({fieldInfo.FieldType}): {fieldInfo.GetValue(loadedClass)}");

                foreach (var propertyInfo in typeof(SomeClass).GetProperties())
                    logger?.Log($"{propertyInfo.Name} ({propertyInfo.PropertyType}): {propertyInfo.GetValue(loadedClass)}");

                Console.WriteLine("\nHappy coding!");
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine("Operation failed");
            }
        }

        public class MyLogger : ILoggerService
        {
            public void Log(string msg)
            {
                Console.WriteLine(msg);
            }

            public async Task LogAsync(string msg)
            {
                Console.WriteLine(msg);
            }

            public void Dispose()
            {
                // Do whatever you want
            }
        }

        public class SomeClass
        {
            public string PasswordField;
            public string SomeStringField;
            public int SomeIntField;
            public bool SomeBoolField;
        }
    }
}
