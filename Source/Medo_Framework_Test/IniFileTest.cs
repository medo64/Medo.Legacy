using Medo.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Reflection;


namespace Test {

    [TestClass()]
    public class IniFileTest {

        #region Parse

        [TestMethod()]
        public void IniFile_Parse_Basic() {
            var file = new IniFile(Assembly.GetExecutingAssembly().GetManifestResourceStream("Test.IniFile.Basic.ini"));
            Assert.AreEqual(2, file.Sections.Count);
            Assert.AreEqual("Section 1", file.Sections[0].Name);
            Assert.AreEqual("Section2", file.Sections[1].Name);
            Assert.AreEqual(2, file.Sections[0].Properties.Count);
            Assert.AreEqual("Key11", file.Sections[0].Properties[0].Name);
            Assert.AreEqual("Value11", file.Sections[0].Properties[0].Value);
            Assert.AreEqual("Key12", file.Sections[0].Properties[1].Name);
            Assert.AreEqual("Value12", file.Sections[0].Properties[1].Value);
            Assert.AreEqual(3, file.Sections[1].Properties.Count);
            Assert.AreEqual("Key21", file.Sections[1].Properties[0].Name);
            Assert.AreEqual("Value 21", file.Sections[1].Properties[0].Value);
            Assert.AreEqual("Key22", file.Sections[1].Properties[1].Name);
            Assert.AreEqual("Value 22", file.Sections[1].Properties[1].Value);
            Assert.AreEqual("Key23", file.Sections[1].Properties[2].Name);
            Assert.AreEqual("Value 23", file.Sections[1].Properties[2].Value);
        }

        [TestMethod()]
        public void IniFile_Parse_BasicWithComments() {
            var file = new IniFile(Assembly.GetExecutingAssembly().GetManifestResourceStream("Test.IniFile.BasicWithComments.ini"));
            Assert.AreEqual(2, file.Sections.Count);
            Assert.AreEqual("Section 1", file.Sections[0].Name);
            Assert.AreEqual("Section2", file.Sections[1].Name);
            Assert.AreEqual(2, file.Sections[0].Properties.Count);
            Assert.AreEqual("Key11", file.Sections[0].Properties[0].Name);
            Assert.AreEqual("Value11", file.Sections[0].Properties[0].Value);
            Assert.AreEqual("Key12", file.Sections[0].Properties[1].Name);
            Assert.AreEqual("Value12", file.Sections[0].Properties[1].Value);
            Assert.AreEqual(3, file.Sections[1].Properties.Count);
            Assert.AreEqual("Key21", file.Sections[1].Properties[0].Name);
            Assert.AreEqual("Value 21", file.Sections[1].Properties[0].Value);
            Assert.AreEqual("Key22", file.Sections[1].Properties[1].Name);
            Assert.AreEqual("Value 22", file.Sections[1].Properties[1].Value);
            Assert.AreEqual("Key23", file.Sections[1].Properties[2].Name);
            Assert.AreEqual("Value 23", file.Sections[1].Properties[2].Value);
        }

        [TestMethod()]
        public void IniFile_Parse_Quoted() {
            var file = new IniFile(Assembly.GetExecutingAssembly().GetManifestResourceStream("Test.IniFile.Quoted.ini"));
            Assert.AreEqual(2, file.Sections.Count);
            Assert.AreEqual("Section 1", file.Sections[0].Name);
            Assert.AreEqual("Section2", file.Sections[1].Name);
            Assert.AreEqual(2, file.Sections[0].Properties.Count);
            Assert.AreEqual("Key11", file.Sections[0].Properties[0].Name);
            Assert.AreEqual("Value11", file.Sections[0].Properties[0].Value);
            Assert.AreEqual("Key12", file.Sections[0].Properties[1].Name);
            Assert.AreEqual("Value12", file.Sections[0].Properties[1].Value);
            Assert.AreEqual(3, file.Sections[1].Properties.Count);
            Assert.AreEqual("Key21", file.Sections[1].Properties[0].Name);
            Assert.AreEqual("Value 21", file.Sections[1].Properties[0].Value);
            Assert.AreEqual("Key22", file.Sections[1].Properties[1].Name);
            Assert.AreEqual("Value 22", file.Sections[1].Properties[1].Value);
            Assert.AreEqual("Key23", file.Sections[1].Properties[2].Name);
            Assert.AreEqual("Value 23", file.Sections[1].Properties[2].Value);
        }

        [TestMethod()]
        public void IniFile_Parse_Mixed() {
            var file = new IniFile(Assembly.GetExecutingAssembly().GetManifestResourceStream("Test.IniFile.Mixed.ini"));
            Assert.AreEqual(2, file.Sections.Count);
            Assert.AreEqual("Section1", file.Sections[0].Name);
            Assert.AreEqual("Section2", file.Sections[1].Name);
            Assert.AreEqual(2, file.Sections[0].Properties.Count);
            Assert.AreEqual("Key11", file.Sections[0].Properties[0].Name);
            Assert.AreEqual("Value11", file.Sections[0].Properties[0].Value);
            Assert.AreEqual("Key12", file.Sections[0].Properties[1].Name);
            Assert.AreEqual("Value12", file.Sections[0].Properties[1].Value);
            Assert.AreEqual(3, file.Sections[1].Properties.Count);
            Assert.AreEqual("Key21", file.Sections[1].Properties[0].Name);
            Assert.AreEqual("Value 21", file.Sections[1].Properties[0].Value);
            Assert.AreEqual("Key22", file.Sections[1].Properties[1].Name);
            Assert.AreEqual("Value 22", file.Sections[1].Properties[1].Value);
            Assert.AreEqual("Key23", file.Sections[1].Properties[2].Name);
            Assert.AreEqual("Value 23", file.Sections[1].Properties[2].Value);
        }

        [TestMethod()]
        public void IniFile_Parse_Escaping() {
            var file = new IniFile(Assembly.GetExecutingAssembly().GetManifestResourceStream("Test.IniFile.Escaping.ini"));
            Assert.AreEqual(2, file.Sections.Count);
            Assert.AreEqual("\fSection1", file.Sections[0].Name);
            Assert.AreEqual("Section2\f", file.Sections[1].Name);
            Assert.AreEqual(2, file.Sections[0].Properties.Count);
            Assert.AreEqual("Key\r11", file.Sections[0].Properties[0].Name);
            Assert.AreEqual("Value\n11", file.Sections[0].Properties[0].Value);
            Assert.AreEqual("Key\n12", file.Sections[0].Properties[1].Name);
            Assert.AreEqual("Value\r12", file.Sections[0].Properties[1].Value);
            Assert.AreEqual(3, file.Sections[1].Properties.Count);
            Assert.AreEqual("Key\t21", file.Sections[1].Properties[0].Name);
            Assert.AreEqual("Value\t21", file.Sections[1].Properties[0].Value);
            Assert.AreEqual("Key\022", file.Sections[1].Properties[1].Name);
            Assert.AreEqual("Value\022", file.Sections[1].Properties[1].Value);
            Assert.AreEqual("Key;23", file.Sections[1].Properties[2].Name);
            Assert.AreEqual("Value;23", file.Sections[1].Properties[2].Value);
        }


        [TestMethod()]
        public void IniFile_Save_Basic() {
            var original = new IniFile(Assembly.GetExecutingAssembly().GetManifestResourceStream("Test.IniFile.Basic.ini"));
            var ms = new MemoryStream();
            original.Save(ms);
            var file = new IniFile(new MemoryStream(ms.ToArray()));
            Assert.AreEqual(2, file.Sections.Count);
            Assert.AreEqual("Section 1", file.Sections[0].Name);
            Assert.AreEqual("Section2", file.Sections[1].Name);
            Assert.AreEqual(2, file.Sections[0].Properties.Count);
            Assert.AreEqual("Key11", file.Sections[0].Properties[0].Name);
            Assert.AreEqual("Value11", file.Sections[0].Properties[0].Value);
            Assert.AreEqual("Key12", file.Sections[0].Properties[1].Name);
            Assert.AreEqual("Value12", file.Sections[0].Properties[1].Value);
            Assert.AreEqual(3, file.Sections[1].Properties.Count);
            Assert.AreEqual("Key21", file.Sections[1].Properties[0].Name);
            Assert.AreEqual("Value 21", file.Sections[1].Properties[0].Value);
            Assert.AreEqual("Key22", file.Sections[1].Properties[1].Name);
            Assert.AreEqual("Value 22", file.Sections[1].Properties[1].Value);
            Assert.AreEqual("Key23", file.Sections[1].Properties[2].Name);
            Assert.AreEqual("Value 23", file.Sections[1].Properties[2].Value);
        }

        [TestMethod()]
        public void IniFile_Save_BasicWithComments() {
            var original = new IniFile(Assembly.GetExecutingAssembly().GetManifestResourceStream("Test.IniFile.BasicWithComments.ini"));
            var ms = new MemoryStream();
            original.Save(ms);
            var file = new IniFile(new MemoryStream(ms.ToArray()));
            Assert.AreEqual(2, file.Sections.Count);
            Assert.AreEqual("Section 1", file.Sections[0].Name);
            Assert.AreEqual("Section2", file.Sections[1].Name);
            Assert.AreEqual(2, file.Sections[0].Properties.Count);
            Assert.AreEqual("Key11", file.Sections[0].Properties[0].Name);
            Assert.AreEqual("Value11", file.Sections[0].Properties[0].Value);
            Assert.AreEqual("Key12", file.Sections[0].Properties[1].Name);
            Assert.AreEqual("Value12", file.Sections[0].Properties[1].Value);
            Assert.AreEqual(3, file.Sections[1].Properties.Count);
            Assert.AreEqual("Key21", file.Sections[1].Properties[0].Name);
            Assert.AreEqual("Value 21", file.Sections[1].Properties[0].Value);
            Assert.AreEqual("Key22", file.Sections[1].Properties[1].Name);
            Assert.AreEqual("Value 22", file.Sections[1].Properties[1].Value);
            Assert.AreEqual("Key23", file.Sections[1].Properties[2].Name);
            Assert.AreEqual("Value 23", file.Sections[1].Properties[2].Value);
        }

        [TestMethod()]
        public void IniFile_Save_Quoted() {
            var original = new IniFile(Assembly.GetExecutingAssembly().GetManifestResourceStream("Test.IniFile.Quoted.ini"));
            var ms = new MemoryStream();
            original.Save(ms);
            var file = new IniFile(new MemoryStream(ms.ToArray()));
            Assert.AreEqual(2, file.Sections.Count);
            Assert.AreEqual("Section 1", file.Sections[0].Name);
            Assert.AreEqual("Section2", file.Sections[1].Name);
            Assert.AreEqual(2, file.Sections[0].Properties.Count);
            Assert.AreEqual("Key11", file.Sections[0].Properties[0].Name);
            Assert.AreEqual("Value11", file.Sections[0].Properties[0].Value);
            Assert.AreEqual("Key12", file.Sections[0].Properties[1].Name);
            Assert.AreEqual("Value12", file.Sections[0].Properties[1].Value);
            Assert.AreEqual(3, file.Sections[1].Properties.Count);
            Assert.AreEqual("Key21", file.Sections[1].Properties[0].Name);
            Assert.AreEqual("Value 21", file.Sections[1].Properties[0].Value);
            Assert.AreEqual("Key22", file.Sections[1].Properties[1].Name);
            Assert.AreEqual("Value 22", file.Sections[1].Properties[1].Value);
            Assert.AreEqual("Key23", file.Sections[1].Properties[2].Name);
            Assert.AreEqual("Value 23", file.Sections[1].Properties[2].Value);
        }

        [TestMethod()]
        public void IniFile_Save_Mixed() {
            var original = new IniFile(Assembly.GetExecutingAssembly().GetManifestResourceStream("Test.IniFile.Mixed.ini"));
            var ms = new MemoryStream();
            original.Save(ms);
            var file = new IniFile(new MemoryStream(ms.ToArray()));
            Assert.AreEqual(2, file.Sections.Count);
            Assert.AreEqual("Section1", file.Sections[0].Name);
            Assert.AreEqual("Section2", file.Sections[1].Name);
            Assert.AreEqual(2, file.Sections[0].Properties.Count);
            Assert.AreEqual("Key11", file.Sections[0].Properties[0].Name);
            Assert.AreEqual("Value11", file.Sections[0].Properties[0].Value);
            Assert.AreEqual("Key12", file.Sections[0].Properties[1].Name);
            Assert.AreEqual("Value12", file.Sections[0].Properties[1].Value);
            Assert.AreEqual(3, file.Sections[1].Properties.Count);
            Assert.AreEqual("Key21", file.Sections[1].Properties[0].Name);
            Assert.AreEqual("Value 21", file.Sections[1].Properties[0].Value);
            Assert.AreEqual("Key22", file.Sections[1].Properties[1].Name);
            Assert.AreEqual("Value 22", file.Sections[1].Properties[1].Value);
            Assert.AreEqual("Key23", file.Sections[1].Properties[2].Name);
            Assert.AreEqual("Value 23", file.Sections[1].Properties[2].Value);
        }

        [TestMethod()]
        public void IniFile_Save_Escaping() {
            var original = new IniFile(Assembly.GetExecutingAssembly().GetManifestResourceStream("Test.IniFile.Escaping.ini"));
            var ms = new MemoryStream();
            original.Save(ms);
            var file = new IniFile(new MemoryStream(ms.ToArray()));
            Assert.AreEqual(2, file.Sections.Count);
            Assert.AreEqual("\fSection1", file.Sections[0].Name);
            Assert.AreEqual("Section2\f", file.Sections[1].Name);
            Assert.AreEqual(2, file.Sections[0].Properties.Count);
            Assert.AreEqual("Key\r11", file.Sections[0].Properties[0].Name);
            Assert.AreEqual("Value\n11", file.Sections[0].Properties[0].Value);
            Assert.AreEqual("Key\n12", file.Sections[0].Properties[1].Name);
            Assert.AreEqual("Value\r12", file.Sections[0].Properties[1].Value);
            Assert.AreEqual(3, file.Sections[1].Properties.Count);
            Assert.AreEqual("Key\t21", file.Sections[1].Properties[0].Name);
            Assert.AreEqual("Value\t21", file.Sections[1].Properties[0].Value);
            Assert.AreEqual("Key\022", file.Sections[1].Properties[1].Name);
            Assert.AreEqual("Value\022", file.Sections[1].Properties[1].Value);
            Assert.AreEqual("Key;23", file.Sections[1].Properties[2].Name);
            Assert.AreEqual("Value;23", file.Sections[1].Properties[2].Value);
        }

        #endregion


        #region Section

        [TestMethod()]
        public void IniFile_Section_Create_01() {
            var section = new IniSection("Test");
            Assert.AreEqual("Test", section.Name);
            Assert.AreEqual(0, section.Properties.Count);
        }

        [TestMethod()]
        public void IniFile_Section_Create_02() {
            var section = new IniSection("Test", new IniProperty[] { new IniProperty("PN1", "PV1"), new IniProperty("PN2", "PV2") });
            Assert.AreEqual("Test", section.Name);
            Assert.AreEqual(2, section.Properties.Count);
            Assert.AreEqual("PN1", section.Properties[0].Name);
            Assert.AreEqual("PV1", section.Properties[0].Value);
            Assert.AreEqual("PN2", section.Properties[1].Name);
            Assert.AreEqual("PV2", section.Properties[1].Value);
        }

        [TestMethod()]
        public void IniFile_Section_Modify_01() {
            var section = new IniSection("Test", new IniProperty[] { new IniProperty("PN1", "PV1"), new IniProperty("PN2", "PV2") });
            section.Properties.Add(new IniProperty("PN3", "PV3"));
            Assert.AreEqual(3, section.Properties.Count);
            Assert.AreEqual("PN1", section.Properties[0].Name);
            Assert.AreEqual("PV1", section.Properties[0].Value);
            Assert.AreEqual("PN2", section.Properties[1].Name);
            Assert.AreEqual("PV2", section.Properties[1].Value);
            Assert.AreEqual("PN3", section.Properties[2].Name);
            Assert.AreEqual("PV3", section.Properties[2].Value);
        }

        [TestMethod()]
        public void IniFile_Section_Modify_02() {
            var section = new IniSection("Test");
            section.Properties.Add(new IniProperty("PN", "PV"));
            Assert.AreEqual(1, section.Properties.Count);
            Assert.AreEqual("PN", section.Properties[0].Name);
            Assert.AreEqual("PV", section.Properties[0].Value);
        }

        [TestMethod()]
        public void IniFile_Section_Modify_03() {
            var section = new IniSection("Test");
            section.AddProperty("PN", "PV");
            Assert.AreEqual(1, section.Properties.Count);
            Assert.AreEqual("PN", section.Properties[0].Name);
            Assert.AreEqual("PV", section.Properties[0].Value);
        }

        #endregion


        #region Property

        [TestMethod()]
        public void IniFile_Property_Create() {
            var property = new IniProperty("PN", "PV");
            Assert.AreEqual("PN", property.Name);
            Assert.AreEqual("PV", property.Value);
        }

        [TestMethod()]
        public void IniFile_Property_Create_EqualsInName() {
            var property = new IniProperty("PN=", "PV");
            Assert.AreEqual("PN=", property.Name);
            Assert.AreEqual("PV", property.Value);
        }

        [TestMethod()]
        public void IniFile_Property_Create_SemicolonInName_L() {
            var property = new IniProperty(";PN", "PV");
            Assert.AreEqual(";PN", property.Name);
            Assert.AreEqual("PV", property.Value);
        }

        [TestMethod()]
        public void IniFile_Property_Create_SemicolonInName_R() {
            var property = new IniProperty("PN;", "PV");
            Assert.AreEqual("PN;", property.Name);
            Assert.AreEqual("PV", property.Value);
        }

        [TestMethod()]
        public void IniFile_Property_Create_SemicolonInValue_L() {
            var property = new IniProperty("PN", ";PV");
            Assert.AreEqual("PN", property.Name);
            Assert.AreEqual(";PV", property.Value);
        }

        [TestMethod()]
        public void IniFile_Property_Create_SemicolonInValue_R() {
            var property = new IniProperty("PN", "PV;");
            Assert.AreEqual("PN", property.Name);
            Assert.AreEqual("PV;", property.Value);
        }

        [TestMethod()]
        public void IniFile_Property_Create_QuoteInName() {
            var property = new IniProperty(@"P""N", "PV");
            Assert.AreEqual(@"P""N", property.Name);
            Assert.AreEqual("PV", property.Value);
        }

        [TestMethod()]
        public void IniFile_Property_Create_QuoteInValue() {
            var property = new IniProperty("PN", @"P""V");
            Assert.AreEqual("PN", property.Name);
            Assert.AreEqual(@"P""V", property.Value);
        }

        [TestMethod()]
        public void IniFile_Property_Create_BackslashInName() {
            var property = new IniProperty(@"P\N", "PV");
            Assert.AreEqual(@"P\N", property.Name);
            Assert.AreEqual("PV", property.Value);
        }

        [TestMethod()]
        public void IniFile_Property_Create_BackslashInValue() {
            var property = new IniProperty("PN", @"P\V");
            Assert.AreEqual("PN", property.Name);
            Assert.AreEqual(@"P\V", property.Value);
        }

        [TestMethod()]
        public void IniFile_Property_Create_Whitespace_L() {
            var property = new IniProperty(" PN", " PV");
            Assert.AreEqual(" PN", property.Name);
            Assert.AreEqual(" PV", property.Value);
        }

        [TestMethod()]
        public void IniFile_Property_Create_Whitespace_R() {
            var property = new IniProperty("PN ", "PV ");
            Assert.AreEqual("PN ", property.Name);
            Assert.AreEqual("PV ", property.Value);
        }

        [TestMethod()]
        public void IniFile_Property_Create_Whitespace_LR() {
            var property = new IniProperty("\n PN\r", "\rPV \n");
            Assert.AreEqual("\n PN\r", property.Name);
            Assert.AreEqual("\rPV \n", property.Value);
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IniFile_Property_Create_Null() {
            var property = new IniProperty("Test", null);
        }

        [TestMethod()]
        public void IniFile_Property_Modify_Name() {
            var property = new IniProperty("PN", "PV") {
                Name = "PNnew"
            };
            Assert.AreEqual("PNnew", property.Name);
            Assert.AreEqual("PV", property.Value);
        }

        [TestMethod()]
        public void IniFile_Property_Modify_Value() {
            var property = new IniProperty("PN", "PV") {
                Value = "PVnew"
            };
            Assert.AreEqual("PN", property.Name);
            Assert.AreEqual("PVnew", property.Value);
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IniFile_Property_Modify_NameNull() {
            var property = new IniProperty("PN", "PV") {
                Name = null
            };
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IniFile_Property_Modify_ValueNull() {
            var property = new IniProperty("PN", "PV") {
                Value = null
            };
        }

        #endregion

        #region DoubleLoad

        [TestMethod()]
        public void IniFile_DoubleLoad() {
            var file = new IniFile();
            file.AddSection("S").AddProperty("K", "V");
            Assert.AreEqual(1, file.Sections.Count);
            Assert.AreEqual("S", file.Sections[0].Name);
            Assert.AreEqual(1, file.Sections[0].Properties.Count);
            Assert.AreEqual("K", file.Sections[0].Properties[0].Name);
            Assert.AreEqual("V", file.Sections[0].Properties[0].Value);
            file.Load(Assembly.GetExecutingAssembly().GetManifestResourceStream(@"Test.IniFile.Basic.ini"));
            Assert.AreEqual(2, file.Sections.Count);
            Assert.AreEqual("Section 1", file.Sections[0].Name);
            Assert.AreEqual("Section2", file.Sections[1].Name);
            Assert.AreEqual(2, file.Sections[0].Properties.Count);
            Assert.AreEqual("Key11", file.Sections[0].Properties[0].Name);
            Assert.AreEqual("Value11", file.Sections[0].Properties[0].Value);
            Assert.AreEqual("Key12", file.Sections[0].Properties[1].Name);
            Assert.AreEqual("Value12", file.Sections[0].Properties[1].Value);
            Assert.AreEqual(3, file.Sections[1].Properties.Count);
            Assert.AreEqual("Key21", file.Sections[1].Properties[0].Name);
            Assert.AreEqual("Value 21", file.Sections[1].Properties[0].Value);
            Assert.AreEqual("Key22", file.Sections[1].Properties[1].Name);
            Assert.AreEqual("Value 22", file.Sections[1].Properties[1].Value);
            Assert.AreEqual("Key23", file.Sections[1].Properties[2].Name);
            Assert.AreEqual("Value 23", file.Sections[1].Properties[2].Value);
        }

        #endregion

    }
}
