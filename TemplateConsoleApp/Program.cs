using FileSystemStorageLibrary.Services;

namespace TemplateConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var encryptor = new EncryptorAES();
            var storage = new FileSystemStorage(encryptor);

            var someClass = new SomeClass
            {
                PasswordField = "Some password which I wouldn't share",
                SomeStringField = "StringField",
                SomeBoolField = true,
                SomeIntField = 1,
            };

            string filePath = @"C:\SavedFile.txt";
            storage.SaveData(obj: someClass, filePath: filePath, propertiesOrFieldNamesToCipher: nameof(SomeClass.PasswordField));
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
