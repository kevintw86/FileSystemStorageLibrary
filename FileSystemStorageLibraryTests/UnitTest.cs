using FileSystemStorageLibrary.Interfaces;
using FileSystemStorageLibrary.Services;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;

namespace FileSystemStorageLibraryTests
{
    public class Tests
    {
        private IEncryptor encryptor;
        private IStorage storage;

        [SetUp]
        public void Setup()
        {
            this.encryptor = new EncryptorAES();
            this.storage = new FileSystemStorage(this.encryptor);
        }

        [Test]
        public void Test()
        {
            #region Defining constant target files names

            var dir = Path.GetDirectoryName(typeof(Tests).Assembly.Location);

            const string singleWithFieldsFileName = "singleWithFields.txt";
            const string singleWithPropertiesFileName = "singleWithProperties.txt";
            const string collectionOfClassesWithFieldsFileName = "collectionOfClassesWithFields.txt";
            const string collectionOfClassesWithPropertiesFileName = "collectionOfClassesWithProperties.txt";

            var singleWithFieldsFullPath = Path.Combine(dir, singleWithFieldsFileName);
            var singleWithPropertiesFullPath = Path.Combine(dir, singleWithPropertiesFileName);
            var collectionOfClassesWithFieldsFullPath = Path.Combine(dir, collectionOfClassesWithFieldsFileName);
            var collectionOfClassesWithPropertiesFullPath = Path.Combine(dir, collectionOfClassesWithPropertiesFileName);

            #endregion

            #region Defining constant values

            const string regularPlainParam = "PlainParam";
            const string someParamToCipher = "ThisIsCipheredParam";
            const int someIntParam = 123;

            #endregion

            #region Defining property/field names to cipher

            const string propertyToCipher = nameof(SingleClassWithFields.SomeParamToCipher);

            #endregion

            #region Creating instances of classes to Save on disk with enryption

            var singleClassWithFields = new SingleClassWithFields
            {
                RegularPlainParam = regularPlainParam,
                SomeParamToCipher = someParamToCipher,
                SomeIntParam = someIntParam
            };

            var singleClassWithProperties = new SingleClassWithProperties
            {
                RegularPlainParam = regularPlainParam,
                SomeParamToCipher = someParamToCipher,
                SomeIntParam = someIntParam
            };

            var collectionOfClassesWithFields = new CollectionOfClassesWithFields(
                new SingleClassWithFields[] 
                { 
                    new SingleClassWithFields 
                    { 
                        RegularPlainParam = regularPlainParam, 
                        SomeParamToCipher = someParamToCipher, 
                        SomeIntParam = someIntParam 
                    } 
                });

            var collectionOfClassesWithProperties = new CollectionOfClassesWithProperties(
                new List<SingleClassWithProperties>
                { 
                    new SingleClassWithProperties 
                    { RegularPlainParam = regularPlainParam, 
                        SomeParamToCipher = someParamToCipher, 
                        SomeIntParam = someIntParam 
                    } 
                });

            #endregion

            // Saving class instances to a disk
            storage.SaveData(singleClassWithFields, singleWithFieldsFullPath, propertyToCipher);
            storage.SaveData(singleClassWithProperties, singleWithPropertiesFullPath, propertyToCipher);
            storage.SaveData(collectionOfClassesWithFields, collectionOfClassesWithFieldsFullPath, propertyToCipher);
            storage.SaveData(collectionOfClassesWithProperties, collectionOfClassesWithPropertiesFullPath, propertyToCipher);

            // Checking that our original parameter 'SomeParamToCipher' NOT EQUALS to that saved on disk. Means that parameter is encrypted successfully.
            Assert.AreNotEqual(someParamToCipher, JsonConvert.DeserializeObject<SingleClassWithFields>(File.ReadAllText(singleWithFieldsFullPath)).SomeParamToCipher);
            Assert.AreNotEqual(someParamToCipher, JsonConvert.DeserializeObject<SingleClassWithProperties>(File.ReadAllText(singleWithPropertiesFullPath)).SomeParamToCipher);
            Assert.AreNotEqual(someParamToCipher, JsonConvert.DeserializeObject<CollectionOfClassesWithFields>(File.ReadAllText(collectionOfClassesWithFieldsFullPath)).SingleClassWithProperties[0].SomeParamToCipher);
            Assert.AreNotEqual(someParamToCipher, JsonConvert.DeserializeObject<CollectionOfClassesWithProperties>(File.ReadAllText(collectionOfClassesWithPropertiesFullPath)).SingleClassWithProperties[0].SomeParamToCipher);

            // Checking that our classes instances parameter 'RegularPlainParam' equals to that saved on disk. Means that only required class parameters have been ciphered no touching the other parameters.
            Assert.AreEqual(singleClassWithFields.RegularPlainParam, JsonConvert.DeserializeObject<SingleClassWithFields>(File.ReadAllText(singleWithFieldsFullPath)).RegularPlainParam);
            Assert.AreEqual(singleClassWithProperties.RegularPlainParam, JsonConvert.DeserializeObject<SingleClassWithProperties>(File.ReadAllText(singleWithPropertiesFullPath)).RegularPlainParam);
            Assert.AreEqual(collectionOfClassesWithFields.SingleClassWithProperties[0].RegularPlainParam, JsonConvert.DeserializeObject<CollectionOfClassesWithFields>(File.ReadAllText(collectionOfClassesWithFieldsFullPath)).SingleClassWithProperties[0].RegularPlainParam);
            Assert.AreEqual(collectionOfClassesWithProperties.SingleClassWithProperties[0].RegularPlainParam, JsonConvert.DeserializeObject<CollectionOfClassesWithProperties>(File.ReadAllText(collectionOfClassesWithPropertiesFullPath)).SingleClassWithProperties[0].RegularPlainParam);

            // Loading class instances back from disk
            singleClassWithFields = storage.LoadData<SingleClassWithFields>(singleWithFieldsFullPath, propertyToCipher);
            singleClassWithProperties = storage.LoadData<SingleClassWithProperties>(singleWithPropertiesFullPath, propertyToCipher);
            collectionOfClassesWithFields = storage.LoadData<CollectionOfClassesWithFields>(collectionOfClassesWithFieldsFullPath, propertyToCipher);
            collectionOfClassesWithProperties = storage.LoadData<CollectionOfClassesWithProperties>(collectionOfClassesWithPropertiesFullPath, propertyToCipher);

            // Checking that our original constant parameter 'SomeParamToCipher' IS EQUALS to those Loaded and decrypted from disk.
            Assert.AreEqual(someParamToCipher, singleClassWithFields.SomeParamToCipher);
            Assert.AreEqual(someParamToCipher, singleClassWithProperties.SomeParamToCipher);
            Assert.AreEqual(someParamToCipher, collectionOfClassesWithFields.SingleClassWithProperties[0].SomeParamToCipher);
            Assert.AreEqual(someParamToCipher, collectionOfClassesWithProperties.SingleClassWithProperties[0].SomeParamToCipher);

            this.encryptor.Dispose();
            this.storage.Dispose();
        }

        public class CollectionOfClassesWithFields
        {
            public readonly SingleClassWithFields[] SingleClassWithProperties;

            public CollectionOfClassesWithFields(SingleClassWithFields[] singleClassWithProperties)
            {
                this.SingleClassWithProperties = singleClassWithProperties;
            }
        }

        public class CollectionOfClassesWithProperties
        {
            private List<SingleClassWithProperties> _SingleClassWithProperties;
            public List<SingleClassWithProperties> SingleClassWithProperties
            {
                get { return _SingleClassWithProperties; }
                set { _SingleClassWithProperties = value; }
            }

            public CollectionOfClassesWithProperties(List<SingleClassWithProperties> singleClassWithProperties)
            {
                this.SingleClassWithProperties = singleClassWithProperties;
            }
        }

        public class SingleClassWithFields
        {
            public string SomeParamToCipher;
            public string RegularPlainParam;
            public int SomeIntParam;
        }

        public class SingleClassWithProperties
        {
            private string _SomeParamToCipher;
            public string SomeParamToCipher
            {
                get { return _SomeParamToCipher; }
                set { _SomeParamToCipher = value; }
            }

            private string _RegularPlainParam;
            public string RegularPlainParam
            {
                get { return _RegularPlainParam; }
                set { _RegularPlainParam = value; }
            }

            private int _SomeIntParam;
            public int SomeIntParam
            {
                get { return _SomeIntParam; }
                set { _SomeIntParam = value; }
            }

        }
    }
}