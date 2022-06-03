# FileSystemStorageLibrary
C# .Net Core 3.1 library that allows to Save/Load classes to/from a file system with encryption of sensitive data, without defining any passwords or keys.
Encryption of sensitive data is performed on object itself and its childs recursively.

Usage example:

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
    
The file saved with encryption will look as follow:

    {
       "PasswordField":"MHaQtZOBdUXTEoyGajXJgcNCAAQsHKe1zrqBe1TdyEhb+Ld5iGsUrYZIzew5aZLTEF8psHIgBkH4UEafd",
       "SomeStringField":"StringField",
       "SomeIntField":1,
       "SomeBoolField":true
    }
