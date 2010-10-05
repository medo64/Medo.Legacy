using Medo.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace Test {


    /// <summary>
    ///This is a test class for KeyValuePacketTest and is intended
    ///to contain all KeyValuePacketTest Unit Tests
    ///</summary>
    [TestClass()]
    public class KeyValuePacketTest {


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


        /// <summary>
        ///A test for ToArray
        ///</summary>
        [TestMethod()]
        public void ToArrayTest_NullKeyValue() {
            KeyValuePacket kvp = new KeyValuePacket();
            byte[] result = kvp.ToArray();
            string resultString = System.BitConverter.ToString(result);

            Assert.AreEqual("4B-56-50-01-FF-FF-FF-FF-FF-FF-FF-FF-4B-56-50-01", resultString);
        }

        /// <summary>
        ///A test for ToArray
        ///</summary>
        [TestMethod()]
        public void ToArrayTest_NullKey() {
            KeyValuePacket kvp = new KeyValuePacket(null, new byte[] { });
            byte[] result = kvp.ToArray();
            string resultString = System.BitConverter.ToString(result);

            Assert.AreEqual("4B-56-50-01-FF-FF-FF-FF-00-00-00-00-B4-A9-AF-FE", resultString);
        }

        /// <summary>
        ///A test for ToArray
        ///</summary>
        [TestMethod()]
        public void ToArrayTest_NullValue() {
            KeyValuePacket kvp = new KeyValuePacket("", null);
            byte[] result = kvp.ToArray();
            string resultString = System.BitConverter.ToString(result);

            Assert.AreEqual("4B-56-50-01-00-00-00-00-FF-FF-FF-FF-B4-A9-AF-FE", resultString);
        }

        /// <summary>
        ///A test for ToArray
        ///</summary>
        [TestMethod()]
        public void ToArrayTest() {
            KeyValuePacket kvp = new KeyValuePacket("XXX", new byte[] { 0, 1, 2, 3 });
            byte[] result = kvp.ToArray();
            string resultString = System.BitConverter.ToString(result);

            Assert.AreEqual("4B-56-50-01-00-00-00-03-00-00-00-04-4B-52-53-01-58-58-58-00-01-02-03", resultString);
        }


        /// <summary>
        ///A test for Parse
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(System.FormatException))]
        public void ParseTest_CrcError() {
            byte[] input = new byte[] { 0x4B, 0x56, 0x50, 0x01, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x4B, 0x56, 0x50, 0x02 };

            KeyValuePacket kvp = KeyValuePacket.Parse(input);
            Assert.AreEqual(kvp.Key, null);
            Assert.AreEqual(kvp.GetValue(), null);
        }

        /// <summary>
        ///A test for Parse
        ///</summary>
        [TestMethod()]
        public void ParseTest_NullKeyValue() {
            byte[] input = new byte[] { 0x4B, 0x56, 0x50, 0x01, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x4B, 0x56, 0x50, 0x01 };

            KeyValuePacket kvp = KeyValuePacket.Parse(input);
            Assert.AreEqual(kvp.Key, null);
            Assert.AreEqual(kvp.GetValue(), null);
        }

        /// <summary>
        ///A test for Parse
        ///</summary>
        [TestMethod()]
        public void ParseTest_NullKey() {
            byte[] input = new byte[] { 0x4B, 0x56, 0x50, 0x01, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0xB4, 0xA9, 0xAF, 0xFE };

            KeyValuePacket kvp = KeyValuePacket.Parse(input);
            Assert.AreEqual(kvp.Key, null);
            Assert.AreEqual(kvp.GetValue().Length, 0);
        }

        /// <summary>
        ///A test for Parse
        ///</summary>
        [TestMethod()]
        public void ParseTest_NullValue() {
            byte[] input = new byte[] { 0x4B, 0x56, 0x50, 0x01, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xB4, 0xA9, 0xAF, 0xFE };

            KeyValuePacket kvp = KeyValuePacket.Parse(input);
            Assert.AreEqual(kvp.Key.Length, 0);
            Assert.AreEqual(kvp.GetValue(), null);
        }

        /// <summary>
        ///A test for Parse
        ///</summary>
        [TestMethod()]
        public void ParseTest() {
            byte[] input = new byte[] { 0x4B, 0x56, 0x50, 0x01, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x04, 0x4B, 0x52, 0x53, 0x01, 0x58, 0x58, 0x58, 0x00, 0x01, 0x02, 0x03 };

            KeyValuePacket kvp = KeyValuePacket.Parse(input);
            Assert.AreEqual(kvp.Key, "XXX");
            Assert.AreEqual(kvp.GetValue()[0], 0);
            Assert.AreEqual(kvp.GetValue()[1], 1);
            Assert.AreEqual(kvp.GetValue()[2], 2);
            Assert.AreEqual(kvp.GetValue()[3], 3);
        }

    }
}
