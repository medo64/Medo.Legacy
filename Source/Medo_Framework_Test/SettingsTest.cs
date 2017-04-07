using Medo.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Test {

    [TestClass()]
    public class SettingsTest {

        [TestMethod()]
        public void Settings_String() {
            Settings.Write("TestString", null); //to clean leftover values from last run

            Assert.AreEqual("DEF", Settings.Read("TestString", "DEF"));

            Settings.Write("TestString", "123");
            Assert.AreEqual("123", Settings.Read("TestString", "DEF"));

            Settings.Write("TestString", null);
            Assert.AreEqual("DEF", Settings.Read("TestString", "DEF"));

            var defaults = new Dictionary<string, string>();
            defaults.Add("A", "A");
            defaults.Add("TestString", "DEF2");
            defaults.Add("Z", "Z");
            Settings.SetDefaults(defaults);
            Assert.AreEqual("DEF2", Settings.Read("TestString", "DEF"));
        }

        [TestMethod()]
        public void Settings_Int32() {
            Settings.Write("TestInt32", null); //to clean leftover values from last run

            Assert.AreEqual(42, Settings.Read("TestInt32", 42));

            Settings.Write("TestInt32", 1);
            Assert.AreEqual(1, Settings.Read("TestInt32", 42));

            Settings.Write("TestInt32", null);
            Assert.AreEqual(42, Settings.Read("TestInt32", 42));

            Settings.SetDefaults("A", 1);
            Settings.SetDefaults("TestInt32", 43);
            Settings.SetDefaults("B", 100);
            Assert.AreEqual(43, Settings.Read("TestInt32", 42));

            Settings.Write("TestInt32-X", "A");
            Assert.AreEqual(42, Settings.Read("TestInt32-X", 42));

            Settings.Write("TestInt32-X", "41");
            Assert.AreEqual(41, Settings.Read("TestInt32-X", 42));
        }

        [TestMethod()]
        public void Settings_Boolean() {
            Settings.Write("TestBoolean", null); //to clean leftover values from last run

            Assert.AreEqual(true, Settings.Read("TestBoolean", true));

            Settings.Write("TestBoolean", false);
            Assert.AreEqual(false, Settings.Read("TestBoolean", true));

            Settings.Write("TestBoolean", 1);
            Assert.AreEqual(true, Settings.Read("TestBoolean", true));

            Settings.Write("TestBoolean", null);
            Assert.AreEqual(false, Settings.Read("TestBoolean", false));

            Settings.SetDefaults("A", false);
            Settings.SetDefaults("TestBoolean", true);
            Settings.SetDefaults("B", false);
            Assert.AreEqual(true, Settings.Read("TestBoolean", false));

            Settings.Write("TestBoolean-X", "XXX");
            Assert.AreEqual(false, Settings.Read("TestBoolean-X", false));

            Settings.Write("TestBoolean-X", "true");
            Assert.AreEqual(true, Settings.Read("TestBoolean-X", false));
        }

        [TestMethod()]
        public void Settings_Double() {
            Settings.Write("TestDouble", null); //to clean leftover values from last run

            Assert.AreEqual(42.0, Settings.Read("TestDouble", 42.0));

            Settings.Write("TestDouble", 42.42);
            Assert.AreEqual(42.42, Settings.Read("TestDouble", 42.0));

            Settings.Write("TestDouble", null);
            Assert.AreEqual(42.0, Settings.Read("TestDouble", 42.0));

            Settings.SetDefaults("A", 0.0);
            Settings.SetDefaults("TestDouble", 42.42);
            Settings.SetDefaults("B", 100.0);
            Assert.AreEqual(42.42, Settings.Read("TestDouble", 42.0));

            Settings.Write("TestDouble-X", "XXX");
            Assert.AreEqual(42.0, Settings.Read("TestDouble-X", 42.0));

            Settings.Write("TestDouble-X", 41);
            Assert.AreEqual(41.0, Settings.Read("TestDouble-X", 42.0));

            Settings.Write("TestDouble-X", "42.41");
            Assert.AreEqual(42.41, Settings.Read("TestDouble-X", 42.0));

            Settings.Write("TestDouble-X", "1E-3");
            Assert.AreEqual(0.001, Settings.Read("TestDouble-X", 42.0));
        }

    }
}
