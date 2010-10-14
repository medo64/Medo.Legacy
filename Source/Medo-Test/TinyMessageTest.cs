using Medo.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;

namespace Test {

    /// <summary>
    ///This is a test class for TinyMessagePacketTest and is intended
    ///to contain all TinyMessagePacketTest Unit Tests
    ///</summary>
    [TestClass()]
    public class TinyMessageTest {

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
        public void Test_TinyMessagePacket_Encode_Default() {
            string product = "Example";
            string operation = "Test";
            string data = "Data";

            var target = new TinyMessagePacket<string>(product, operation, data);

            string actual = System.Text.UTF8Encoding.UTF8.GetString(target.GetBytes());
            string expected = @"Example Test ""Data""";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void Test_TinyMessagePacket_Decode_Default() {
            string product = "Example";
            string operation = "Test";
            string data = "Data";

            var target2 = new TinyMessagePacket<string>(product, operation, data);
            TinyMessagePacket<string> actual = TinyMessagePacket<string>.Parse(target2.GetBytes());

            Assert.AreEqual(product, actual.Product);
            Assert.AreEqual(operation, actual.Operation);
            Assert.AreEqual(data, actual.Data);
        }

        [TestMethod()]
        public void Test_TinyMessagePacket_Encode_DataNull() {
            string product = "Example";
            string operation = "Test";
            string data = null;

            var target = new TinyMessagePacket<string>(product, operation, data);

            string actual = System.Text.UTF8Encoding.UTF8.GetString(target.GetBytes());
            string expected = @"Example Test null";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void Test_TinyMessagePacket_Decode_DataNull() {
            string product = "Example";
            string operation = "Test";
            string data = null;

            var target2 = new TinyMessagePacket<string>(product, operation, data);
            TinyMessagePacket<string> actual = TinyMessagePacket<string>.Parse(target2.GetBytes());

            Assert.AreEqual(product, actual.Product);
            Assert.AreEqual(operation, actual.Operation);
            Assert.AreEqual(data, actual.Data);
        }

    }
}
