using Medo.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Test {

    [TestClass()]
    public class IniFileTest {

        public TestContext TestContext { get; set; }


        [TestMethod()]
        public void IniFile_Read() {
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
        public void IniFile_ReadEscaping() {
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
        public void IniFile_ReadDouble() {
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
        public void IniFile_Delete() {
            var sb = new StringBuilder();
            sb.AppendLine("[section1]");
            sb.AppendLine("item1=A");
            sb.AppendLine("item2=B");
            sb.AppendLine("[section2]");
            sb.AppendLine("item1=C");
            sb.AppendLine("item2=D");

            IniFile target = new IniFile(new MemoryStream(UTF8Encoding.UTF8.GetBytes(sb.ToString())));

            target.Delete("section1", "item1");
            target.Delete("section2", "item2");
            Assert.AreEqual(null, target.Read("section1", "item1"));
            Assert.AreEqual("B", target.Read("section1", "item2"));
            Assert.AreEqual("C", target.Read("section2", "item1"));
            Assert.AreEqual(null, target.Read("section2", "item2"));

            target.Delete("section1");
            Assert.AreEqual(null, target.Read("section1", "item1"));
            Assert.AreEqual(null, target.Read("section1", "item2"));
            Assert.AreEqual("C", target.Read("section2", "item1"));
            Assert.AreEqual(null, target.Read("section2", "item2"));
        }

        [TestMethod()]
        public void IniFile_EnumerateSectionsAndKeys() {
            var sb = new StringBuilder();
            sb.AppendLine("[section1]");
            sb.AppendLine("item1=A");
            sb.AppendLine("item2=B");
            sb.AppendLine("[section2]");
            sb.AppendLine("item3=C");
            sb.AppendLine("item4=D");

            IniFile target = new IniFile(new MemoryStream(UTF8Encoding.UTF8.GetBytes(sb.ToString())));

            var sections = new List<string>(target.GetSections());
            Assert.AreEqual(2, sections.Count);
            Assert.IsTrue(sections.Contains("section1"));
            Assert.IsTrue(sections.Contains("section2"));

            var keys = new List<string>(target.GetKeys("section2"));
            Assert.AreEqual(2, keys.Count);
            Assert.IsTrue(keys.Contains("item3"));
            Assert.IsTrue(keys.Contains("item4"));
        }

        [TestMethod()]
        public void IniFile_ContainsForSectionsAndKeys() {
            var sb = new StringBuilder();
            sb.AppendLine("[section1]");
            sb.AppendLine("item1=A");
            sb.AppendLine("item2=B");
            sb.AppendLine("[section2]");
            sb.AppendLine("item3=C");
            sb.AppendLine("item4=D");

            IniFile target = new IniFile(new MemoryStream(UTF8Encoding.UTF8.GetBytes(sb.ToString())));

            Assert.IsTrue(target.ContainsSection("section1"));
            Assert.IsTrue(target.ContainsSection("section2"));
            Assert.IsTrue(target.ContainsKey("section1", "item1"));
            Assert.IsTrue(target.ContainsKey("section1", "item2"));
            Assert.IsFalse(target.ContainsKey("section1", "item3"));
            Assert.IsFalse(target.ContainsKey("section1", "item4"));
            Assert.IsFalse(target.ContainsKey("section2", "item1"));
            Assert.IsFalse(target.ContainsKey("section2", "item2"));
            Assert.IsTrue(target.ContainsKey("section2", "item3"));
            Assert.IsTrue(target.ContainsKey("section2", "item4"));
        }

        [TestMethod()]
        public void IniFile_Save() {
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
