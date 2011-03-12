using Medo.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;

namespace Test {


    /// <summary>
    ///This is a test class for IniFileTest and is intended
    ///to contain all IniFileTest Unit Tests
    ///</summary>
    [TestClass()]
    public class IniFileTest {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext {
            get {
                return testContextInstance;
            }
            set {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        [TestMethod()]
        public void IniFileReadTest() {
            var sb = new StringBuilder();
            sb.AppendLine("; last modified 1 April 2001 by John Doe");
            sb.AppendLine("[owner]");
            sb.AppendLine("name=John Doe");
            sb.AppendLine("organization=Acme Widgets Inc.");
            sb.AppendLine("[database]");
            sb.AppendLine("server=192.0.2.62     ; use IP address in case network name resolution is not working");
            sb.AppendLine("port=143");
            sb.AppendLine("file = \"payroll.dat\"");

            IniFile target = new IniFile(new MemoryStream(UTF8Encoding.UTF8.GetBytes(sb.ToString())));
            Assert.AreEqual("John Doe", target.Read("owner", "name"));
            Assert.AreEqual("Acme Widgets Inc.", target.Read("owner", "Organization"));
            Assert.AreEqual("192.0.2.62", target.Read("Database", "Server"));
            Assert.AreEqual(143, target.Read("database", "port", 0));
            Assert.AreEqual("payroll.dat", target.Read("database", "file"));
            Assert.AreEqual("test1", target.Read("databaseXXX", "file", "test1"));
            Assert.AreEqual("test2", target.Read("database", "fileXXX", "test2"));
        }

        [TestMethod()]
        public void IniFileReadEscapingTest() {
            var sb = new StringBuilder();
            sb.AppendLine(@"   ; testing whitespace line");
            sb.AppendLine(@"[lines]  ;comment here");
            sb.AppendLine(@"line-0=""\"""" ;escaping quotes");
            sb.AppendLine(@"line-1=""\\"" ;escaping backslash");
            sb.AppendLine("line-2\t\t\t=\tXXX\t ;tabs");
            sb.AppendLine(@"line-n  =   ""\r\n\t""");

            IniFile target = new IniFile(new MemoryStream(UTF8Encoding.UTF8.GetBytes(sb.ToString())));
            Assert.AreEqual("\"", target.Read("lines", "line-0"));
            Assert.AreEqual("\\", target.Read("lines", "line-1"));
            Assert.AreEqual("XXX", target.Read("lines", "line-2"));
            Assert.AreEqual("\r\n\t", target.Read("lines", "line-n"));
        }

        [TestMethod()]
        public void IniFileReadDoubleTest() {
            var sb = new StringBuilder();
            sb.AppendLine(@"[numbers]  ;comment here");
            sb.AppendLine(@"number-0=0.01");
            sb.AppendLine(@"number-1=1.23e6");
            sb.AppendLine(@"number-2=123e-3");
            sb.AppendLine(@"number-3=NaN");

            IniFile target = new IniFile(new MemoryStream(UTF8Encoding.UTF8.GetBytes(sb.ToString())));
            Assert.AreEqual(0.01, target.Read("numbers", "number-0", double.NaN));
            Assert.AreEqual(1230000, target.Read("numbers", "number-1", double.NaN));
            Assert.AreEqual(0.123, target.Read("numbers", "number-2", double.NaN));
            Assert.AreEqual(double.NaN, target.Read("numbers", "number-3", double.NaN));
        }

        [TestMethod()]
        public void IniFileSaveTest() {
            IniFile target = new IniFile();
            target.Write("default", "name", "john");
            Assert.AreEqual("john", target.Read("default", "name"));
            target.Write("Default", "Name", "doe");
            Assert.AreEqual("doe", target.Read("default", "name"));
            target.Write("DEFAULT", "NAME", null);
            Assert.AreEqual(null, target.Read("default", "name"));

            target.Write("personal", "firstName", "John");
            target.Write("personal", "lastName", "Doe");
            target.Write("information", "isDead", true);
            target.Write("information", "age", 20);
            Assert.AreEqual("John", target.Read("personal", "firstName"));
            Assert.AreEqual("Doe", target.Read("personal", "lastName"));
            Assert.AreEqual(true, target.Read("information", "isDead", false));
            Assert.AreEqual(20, target.Read("information", "age", 0));
            Assert.AreEqual(21, target.Read("information", "ageXXX", 21));

            var stream = new MemoryStream();
            target.Save(stream);

            stream.Seek(0, SeekOrigin.Begin);
            var content = (new UTF8Encoding(false)).GetString(stream.ToArray());
            var lines = content.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            Assert.AreEqual(8, lines.Length);
            Assert.AreEqual("[personal]", lines[0]);
            Assert.AreEqual("firstName = John", lines[1]);
            Assert.AreEqual("lastName = Doe", lines[2]);
            Assert.AreEqual("", lines[3]);
            Assert.AreEqual("[information]", lines[4]);
            Assert.AreEqual("isDead = True", lines[5]);
            Assert.AreEqual("age = 20", lines[6]);
            Assert.AreEqual("", lines[7]);
        }

    }
}
