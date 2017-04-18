using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Medo.Configuration;
using Xunit;

namespace Test {
    public class PropertiesTests {

        [Fact(DisplayName = "Properties: Null key throws exception")]
        void NullKey() {
            var ex = Assert.Throws<ArgumentNullException>(() => {
                Properties.Read(null, "");
            });
            Assert.StartsWith("Key cannot be null.", ex.Message);
        }

        [Fact(DisplayName = "Properties: Empty key throws exception")]
        void EmptyKey() {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => {
                Properties.Read("   ", "");
            });
            Assert.StartsWith("Key cannot be empty.", ex.Message);
        }


        [Fact(DisplayName = "Properties: Empty file Load/Save")]
        void EmptySave() {
            using (var loader = new PropertiesLoader("Empty.properties")) {
                Assert.True(Properties.Load(), "File should exist before load.");
                Assert.True(Properties.Save(), "Save should succeed.");
                Assert.Equal(BitConverter.ToString(loader.Bytes), BitConverter.ToString(File.ReadAllBytes(loader.FileName)));
            }
        }


        [Fact(DisplayName = "Properties: CRLF preserved on Save")]
        void EmptyLinesCRLF() {
            using (var loader = new PropertiesLoader("EmptyLinesCRLF.properties")) {
                Properties.Save();
                Assert.Equal(BitConverter.ToString(loader.Bytes), BitConverter.ToString(File.ReadAllBytes(loader.FileName)));
            }
        }

        [Fact(DisplayName = "Properties: LF preserved on Save")]
        void EmptyLinesLF() {
            using (var loader = new PropertiesLoader("EmptyLinesLF.properties")) {
                Properties.Save();
                Assert.Equal(BitConverter.ToString(loader.Bytes), BitConverter.ToString(File.ReadAllBytes(loader.FileName)));
            }
        }

        [Fact(DisplayName = "Properties: CR preserved on Save")]
        void EmptyLinesCR() {
            using (var loader = new PropertiesLoader("EmptyLinesCR.properties")) {
                Properties.Save();
                Assert.Equal(BitConverter.ToString(loader.Bytes), BitConverter.ToString(File.ReadAllBytes(loader.FileName)));
            }
        }

        [Fact(DisplayName = "Properties: Mixed line ending gets normalized on Save")]
        void EmptyLinesMixed() {
            using (var loader = new PropertiesLoader("EmptyLinesMixed.properties", "EmptyLinesMixed.Good.properties")) {
                Properties.Save();
                Assert.Equal(loader.GoodText, File.ReadAllText(loader.FileName));
            }
        }

        [Fact(DisplayName = "Properties: Comments are preserved on Save")]
        void CommentsOnly() {
            using (var loader = new PropertiesLoader("CommentsOnly.properties")) {
                Properties.Save();
                Assert.Equal(BitConverter.ToString(loader.Bytes), BitConverter.ToString(File.ReadAllBytes(loader.FileName)));
            }
        }

        [Fact(DisplayName = "Properties: Values with comments are preserved on Save")]
        void CommentsWithValues() {
            using (var loader = new PropertiesLoader("CommentsWithValues.properties")) {
                Properties.Save();
                Assert.Equal(Encoding.UTF8.GetString(loader.Bytes), Encoding.UTF8.GetString(File.ReadAllBytes(loader.FileName)));
                Assert.Equal(BitConverter.ToString(loader.Bytes), BitConverter.ToString(File.ReadAllBytes(loader.FileName)));
            }
        }

        [Fact(DisplayName = "Properties: Leading spaces are preserved on Save")]
        void SpacingEscape() {
            using (var loader = new PropertiesLoader("SpacingEscape.properties", "SpacingEscape.Good.properties")) {
                Properties.Save();

                Assert.Equal(" Value 1", Properties.Read("Key1", null));
                Assert.Equal("Value 2 ", Properties.Read("Key2", null));
                Assert.Equal(" Value 3 ", Properties.Read("Key3", null));
                Assert.Equal("  Value 4  ", Properties.Read("Key4", null));
                Assert.Equal("\tValue 5\t", Properties.Read("Key5", null));
                Assert.Equal("\tValue 6", Properties.Read("Key6", null));

                Assert.Equal(loader.GoodText, File.ReadAllText(loader.FileName));
            }
        }

        [Fact(DisplayName = "Properties: Basic write")]
        void WriteBasic() {
            using (var loader = new PropertiesLoader("Empty.properties", "WriteBasic.Good.properties")) {
                Properties.Write("Key1", "Value 1");
                Properties.Write("Key2", "Value 2");

                Properties.Save();

                Assert.Equal(loader.GoodText, File.ReadAllText(loader.FileName));
            }
        }

        [Fact(DisplayName = "Properties: Basic write (without empty line ending)")]
        void WriteNoEmptyLine() {
            using (var loader = new PropertiesLoader("WriteNoEmptyLine.properties", "WriteNoEmptyLine.Good.properties")) {
                Properties.Write("Key1", "Value 1");
                Properties.Write("Key2", "Value 2");

                Properties.Save();

                Assert.Equal(loader.GoodText, File.ReadAllText(loader.FileName));
            }
        }

        [Fact(DisplayName = "Properties: Separator equals (=) is preserved upon save")]
        void WriteSameSeparatorEquals() {
            using (var loader = new PropertiesLoader("WriteSameSeparatorEquals.properties", "WriteSameSeparatorEquals.Good.properties")) {
                Properties.Write("Key1", "Value 1");
                Properties.Write("Key2", "Value 2");

                Properties.Save();

                Assert.Equal(loader.GoodText, File.ReadAllText(loader.FileName));
            }
        }

        [Fact(DisplayName = "Properties: Separator space ( ) is preserved upon save")]
        void WriteSameSeparatorSpace() {
            using (var loader = new PropertiesLoader("WriteSameSeparatorSpace.properties", "WriteSameSeparatorSpace.Good.properties")) {
                Properties.Write("Key1", "Value 1");
                Properties.Write("Key2", "Value 2");

                Properties.Save();

                Assert.Equal(loader.GoodText, File.ReadAllText(loader.FileName));
            }
        }

        [Fact(DisplayName = "Properties: Write replaces existing entry")]
        void Replace() {
            using (var loader = new PropertiesLoader("Replace.properties", "Replace.Good.properties")) {
                Properties.Write("Key1", "Value 1a");
                Properties.Write("Key2", "Value 2a");

                Properties.Save();

                Assert.Equal("Value 1a", Properties.Read("Key1", null));
                Assert.Equal("Value 2a", Properties.Read("Key2", null));

                Assert.Equal(loader.GoodText, File.ReadAllText(loader.FileName));
            }
        }

        [Fact(DisplayName = "Properties: Write preserves spacing")]
        void SpacingPreserved() {
            using (var loader = new PropertiesLoader("SpacingPreserved.properties", "SpacingPreserved.Good.properties")) {
                Properties.Write("KeyOne", "Value 1a");
                Properties.Write("KeyTwo", "Value 2b");
                Properties.Write("KeyThree", "Value 3c");

                Properties.Save();

                Assert.Equal(loader.GoodText, File.ReadAllText(loader.FileName));
            }
        }

        [Fact(DisplayName = "Properties: Write without preexisting file")]
        void WriteToEmpty() {
            using (var loader = new PropertiesLoader(null, "Replace.Good.properties")) {
                Properties.Write("Key1", "Value 1a");
                Properties.Write("Key2", "Value 2a");

                Properties.Save();

                Assert.Equal(loader.GoodText, File.ReadAllText(loader.FileName));
            }
        }

        [Fact(DisplayName = "Properties: Write replaces only last instance of same key")]
        void ReplaceOnlyLast() {
            using (var loader = new PropertiesLoader("ReplaceOnlyLast.properties", "ReplaceOnlyLast.Good.properties")) {
                Properties.Write("Key1", "Value 1a");
                Properties.Write("Key2", "Value 2a");

                Properties.Save();

                Assert.Equal("Value 1a", Properties.Read("Key1", null));
                Assert.Equal("Value 2a", Properties.Read("Key2", null));
                Assert.Equal("Value 3", Properties.Read("Key3", null));

                Assert.Equal(loader.GoodText, File.ReadAllText(loader.FileName));
            }
        }


        [Fact(DisplayName = "Properties: Write creates directory")]
        void SaveInNonexistingDirectory1() {
            var propertiesFile = Path.Combine(Path.GetTempPath(), "PropertiesDirectory", "Test.properties");
            try {
                Directory.Delete(Path.Combine(Path.GetTempPath(), "PropertiesDirectory"), true);
            } catch (IOException) { }
            Properties.FileName = propertiesFile;

            Assert.False(Properties.Load(), "No file present for load.");

            var x = Properties.Read("Test", "test");
            Assert.Equal("test", x);

            Assert.True(Properties.Save(), "Save should create directory structure and succeed.");


            Assert.True(File.Exists(propertiesFile));
        }

        [Fact(DisplayName = "Properties: Write creates directory (2 levels deep)")]
        void SaveInNonexistingDirectory2() {
            var propertiesFile = Path.Combine(Path.GetTempPath(), "PropertiesDirectoryOuter", "PropertiesDirectoryInner", "Test.properties");
            try {
                Directory.Delete(Path.Combine(Path.GetTempPath(), "PropertiesDirectoryOuter"), true);
            } catch (IOException) { }
            Properties.FileName = propertiesFile;

            Assert.False(Properties.Load(), "No file present for load.");

            var x = Properties.Read("Test", "test");
            Assert.Equal("test", x);

            Assert.True(Properties.Save(), "Save should create directory structure and succeed.");


            Assert.True(File.Exists(propertiesFile));
        }

        [Fact(DisplayName = "Properties: Write creates directory (3 levels deep)")]
        void SaveInNonexistingDirectory3() {
            var propertiesFile = Path.Combine(Path.GetTempPath(), "PropertiesDirectoryOuter", "PropertiesDirectoryMiddle", "PropertiesDirectoryInner", "Test.properties");
            try {
                Directory.Delete(Path.Combine(Path.GetTempPath(), "PropertiesDirectoryOuter"), true);
            } catch (IOException) { }
            Properties.FileName = propertiesFile;

            Assert.False(Properties.Load(), "No file present for load.");

            var x = Properties.Read("Test", "test");
            Assert.Equal("test", x);

            Assert.True(Properties.Save(), "Save should create directory structure and succeed.");


            Assert.True(File.Exists(propertiesFile));
        }


        [Fact(DisplayName = "Properties: Removing entry")]
        void RemoveSingle() {
            using (var loader = new PropertiesLoader("Remove.properties", "Remove.Good.properties")) {
                Properties.Delete("Key1");
                Properties.Save();

                Assert.Equal(loader.GoodText, File.ReadAllText(loader.FileName));
            }
        }

        [Fact(DisplayName = "Properties: Removing multiple entries")]
        void RemoveMulti() {
            using (var loader = new PropertiesLoader("RemoveMulti.properties", "RemoveMulti.Good.properties")) {
                Properties.Delete("Key2");
                Properties.Save();

                Assert.Equal(loader.GoodText, File.ReadAllText(loader.FileName));
            }
        }


        [Fact(DisplayName = "Properties: Override is used first")]
        void UseOverrideFirst() {
            using (var loader = new PropertiesLoader("Replace.properties", resourceOverrideFileName: "Replace.Good.properties")) {
                Assert.Equal("Value 1a", Properties.Read("Key1", null));
            }
        }

        [Fact(DisplayName = "Properties: Override is not written")]
        void DontOverwriteOverride() {
            using (var loader = new PropertiesLoader("Replace.properties", resourceOverrideFileName: "Replace.Good.properties")) {
                Properties.Write("Key1", "XXX");
                Assert.Equal("Value 1a", Properties.Read("Key1", null));
                Properties.OverrideFileName = null;
                Assert.Equal("XXX", Properties.Read("Key1", null));
            }
        }


        [Fact(DisplayName = "Properties: Reading multiple entries")]
        void ReadMulti() {
            using (var loader = new PropertiesLoader("ReplaceOnlyLast.Good.properties")) {
                var list = new List<string>(Properties.Read("Key2"));
                Assert.Equal(2, list.Count);
                Assert.Equal("Value 2", list[0]);
                Assert.Equal("Value 2a", list[1]);
            }
        }

        [Fact(DisplayName = "Properties: Reading multiple entries from override")]
        void ReadMultiFromOverride() {
            using (var loader = new PropertiesLoader("ReplaceOnlyLast.Good.properties", resourceOverrideFileName: "RemoveMulti.properties")) {
                var list = new List<string>(Properties.Read("Key2"));
                Assert.Equal(3, list.Count);
                Assert.Equal("Value 2a", list[0]);
                Assert.Equal("Value 2b", list[1]);
                Assert.Equal("Value 2c", list[2]);
            }
        }

        [Fact(DisplayName = "Properties: Reading multi entries when override is not found")]
        void ReadMultiFromOverrideNotFound() {
            using (var loader = new PropertiesLoader("ReplaceOnlyLast.Good.properties", resourceOverrideFileName: "RemoveMulti.properties")) {
                var list = new List<string>(Properties.Read("Key3"));
                Assert.Equal(1, list.Count);
                Assert.Equal("Value 3", list[0]);
            }
        }

        [Fact(DisplayName = "Properties: Multi-value write")]
        void MultiWrite() {
            using (var loader = new PropertiesLoader(null, resourceFileNameGood: "WriteMulti.Good.properties")) {
                Properties.Write("Key1", "Value 1");
                Properties.Write("Key2", new string[] { "Value 2a", "Value 2b", "Value 2c" });
                Properties.Write("Key3", "Value 3");
                Assert.Equal(loader.GoodText, File.ReadAllText(loader.FileName));

                Assert.Equal("Value 1", Properties.Read("Key1", null));
                Assert.Equal("Value 3", Properties.Read("Key3", null));

                var list = new List<string>(Properties.Read("Key2"));
                Assert.Equal(3, list.Count);
                Assert.Equal("Value 2a", list[0]);
                Assert.Equal("Value 2b", list[1]);
                Assert.Equal("Value 2c", list[2]);
            }
        }

        [Fact(DisplayName = "Properties: Multi-value replace")]
        void MultiReplace() {
            using (var loader = new PropertiesLoader("WriteMulti.properties", resourceFileNameGood: "WriteMulti.Good.properties")) {
                Properties.Write("Key2", new string[] { "Value 2a", "Value 2b", "Value 2c" });
                Assert.Equal(loader.GoodText, File.ReadAllText(loader.FileName));

                Assert.Equal("Value 1", Properties.Read("Key1", null));
                Assert.Equal("Value 3", Properties.Read("Key3", null));

                var list = new List<string>(Properties.Read("Key2"));
                Assert.Equal(3, list.Count);
                Assert.Equal("Value 2a", list[0]);
                Assert.Equal("Value 2b", list[1]);
                Assert.Equal("Value 2c", list[2]);
            }
        }

        [Fact(DisplayName = "Properties: Multi-value override is not written")]
        void DontOverwriteOverrideMulti() {
            using (var loader = new PropertiesLoader("ReplaceOnlyLast.Good.properties", resourceOverrideFileName: "RemoveMulti.properties")) {
                Properties.Write("Key2", "Value X");
                var list = new List<string>(Properties.Read("Key2"));
                Assert.Equal(3, list.Count);
                Assert.Equal("Value 2a", list[0]);
                Assert.Equal("Value 2b", list[1]);
                Assert.Equal("Value 2c", list[2]);
            }
        }


        [Fact(DisplayName = "Properties: Test conversion")]
        void TestConversion() {
            using (var loader = new PropertiesLoader(null, resourceFileNameGood: "WriteConverted.Good.properties")) {
                Properties.Write("Integer", 42);
                Properties.Write("Integer Min", int.MinValue);
                Properties.Write("Integer Max", int.MaxValue);
                Properties.Write("Long", 42L);
                Properties.Write("Long Min", long.MinValue);
                Properties.Write("Long Max", long.MaxValue);
                Properties.Write("Boolean", true);
                Properties.Write("Double", 42.42);
                Properties.Write("Double Pi", Math.PI);
                Properties.Write("Double Third", 1.0 / 3);
                Properties.Write("Double Seventh", 1.0 / 7);
                Properties.Write("Double Min", double.MinValue);
                Properties.Write("Double Max", double.MaxValue);
                Properties.Write("Double NaN", double.NaN);
                Properties.Write("Double Infinity+", double.PositiveInfinity);
                Properties.Write("Double Infinity-", double.NegativeInfinity);

                Assert.Equal(loader.GoodText, File.ReadAllText(loader.FileName));

                using (var loader2 = new PropertiesLoader(loader.FileName, resourceFileNameGood: "WriteConverted.Good.properties")) {
                    Assert.Equal(42, Properties.Read("Integer", 0));
                    Assert.Equal(int.MinValue, Properties.Read("Integer Min", 0));
                    Assert.Equal(int.MaxValue, Properties.Read("Integer Max", 0));
                    Assert.Equal(42, Properties.Read("Long", 0L));
                    Assert.Equal(long.MinValue, Properties.Read("Long Min", 0L));
                    Assert.Equal(long.MaxValue, Properties.Read("Long Max", 0L));
                    Assert.Equal(true, Properties.Read("Boolean", false));
                    Assert.Equal(42.42, Properties.Read("Double", 0.0));
                    Assert.Equal(Math.PI, Properties.Read("Double Pi", 0.0));
                    Assert.Equal(1.0 / 3, Properties.Read("Double Third", 0.0));
                    Assert.Equal(1.0 / 7, Properties.Read("Double Seventh", 0.0));
                    Assert.Equal(double.MinValue, Properties.Read("Double Min", 0.0));
                    Assert.Equal(double.MaxValue, Properties.Read("Double Max", 0.0));
                    Assert.Equal(double.NaN, Properties.Read("Double NaN", 0.0));
                    Assert.Equal(double.PositiveInfinity, Properties.Read("Double Infinity+", 0.0));
                    Assert.Equal(double.NegativeInfinity, Properties.Read("Double Infinity-", 0.0));
                }
            }
        }

        [Fact(DisplayName = "Properties: Key whitespace reading and saving")]
        void KeyWhitespace() {
            using (var loader = new PropertiesLoader("KeyWhitespace.properties", "KeyWhitespace.Good.properties")) {
                Properties.Save();

                Assert.Equal(loader.GoodText, File.ReadAllText(loader.FileName));

                Assert.Equal("Value 1", Properties.Read("Key 1", null));
                Assert.Equal("Value 3", Properties.Read("Key 3", null));

                var list = new List<string>(Properties.Read("Key 2"));
                Assert.Equal(3, list.Count);
                Assert.Equal("Value 2a", list[0]);
                Assert.Equal("Value 2b", list[1]);
                Assert.Equal("Value 2c", list[2]);
            }
        }


        //TODO: Read multi with whitespace in key (\_)


        #region Utils

        private class PropertiesLoader : IDisposable {

            public string FileName { get; }
            public byte[] Bytes { get; }
            public byte[] GoodBytes { get; }

            public PropertiesLoader(string resourceFileName, string resourceFileNameGood = null, string resourceOverrideFileName = null) {
                if (File.Exists(resourceFileName)) {
                    this.Bytes = File.ReadAllBytes(resourceFileName);
                } else {
                    this.Bytes = (resourceFileName != null) ? GetResourceStreamBytes(resourceFileName) : null;
                }
                this.GoodBytes = (resourceFileNameGood != null) ? GetResourceStreamBytes(resourceFileNameGood) : null;
                var overrideBytes = (resourceOverrideFileName != null) ? GetResourceStreamBytes(resourceOverrideFileName) : null;

                this.FileName = Path.GetTempFileName();
                if (resourceFileName == null) {
                    File.Delete(this.FileName); //to start fresh
                } else {
                    File.WriteAllBytes(this.FileName, this.Bytes);
                }

                Properties.FileName = this.FileName;

                var overrideFileName = (resourceOverrideFileName != null) ? Path.GetTempFileName() : null;
                if (overrideFileName != null) {
                    File.WriteAllBytes(overrideFileName, overrideBytes);
                    Properties.OverrideFileName = overrideFileName;
                } else {
                    Properties.OverrideFileName = null;
                }
            }

            private readonly Encoding Utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
            public string Text { get => Utf8.GetString(Bytes); }
            public string GoodText { get => Utf8.GetString(GoodBytes ?? new byte[0]); }

            #region IDisposable Support

            ~PropertiesLoader() {
                this.Dispose(false);
            }

            protected virtual void Dispose(bool disposing) {
                try {
                    File.Delete(this.FileName);
                } catch (IOException) { }
            }

            public void Dispose() {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            #endregion

        }

        private static byte[] GetResourceStreamBytes(string fileName) {
            var resStream = typeof(PropertiesTests).GetTypeInfo().Assembly.GetManifestResourceStream("Test.Resources.Properties." + fileName);
            var buffer = new byte[(int)resStream.Length];
            resStream.Read(buffer, 0, buffer.Length);
            return buffer;
        }

        #endregion

    }
}
