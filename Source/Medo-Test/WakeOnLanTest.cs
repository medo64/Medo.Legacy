using Medo.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace Test
{
    
    
    /// <summary>
    ///This is a test class for WakeOnLanTest and is intended
    ///to contain all WakeOnLanTest Unit Tests
    ///</summary>
    [TestClass()]
    public class WakeOnLanTest {


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
        ///A test for GetPacketBytes
        ///</summary>
        [TestMethod()]
        [DeploymentItem("Medo.dll")]
        public void GetPacketBytesTest_1() {
            byte[] target = WakeOnLan_Accessor.GetPacketBytes( "01-02-03-04-05-06", null);
            string result = System.BitConverter.ToString(target);
            Assert.AreEqual("FF-FF-FF-FF-FF-FF-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06", result);
        }


        /// <summary>
        ///A test for GetPacketBytes
        ///</summary>
        [TestMethod()]
        [DeploymentItem("Medo.dll")]
        public void GetPacketBytesTest_2() {
            byte[] target = WakeOnLan_Accessor.GetPacketBytes("01:02:03:04:05:06", null);
            string result = System.BitConverter.ToString(target);
            Assert.AreEqual("FF-FF-FF-FF-FF-FF-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06", result);
        }

        /// <summary>
        ///A test for GetPacketBytes
        ///</summary>
        [TestMethod()]
        [DeploymentItem("Medo.dll")]
        public void GetPacketBytesTest_3() {
            byte[] target = WakeOnLan_Accessor.GetPacketBytes("01:02:03:04:05:06", "A0:A1:A2:A3:A4:A5");
            string result = System.BitConverter.ToString(target);
            Assert.AreEqual("FF-FF-FF-FF-FF-FF-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-A0-A1-A2-A3-A4-A5", result);
        }

        /// <summary>
        ///A test for GetPacketBytes
        ///</summary>
        [TestMethod()]
        [DeploymentItem("Medo.dll")]
        public void GetPacketBytesTest_4() {
            byte[] target = WakeOnLan_Accessor.GetPacketBytes("01-02:03-04:05-06", "A0-A1-A2-A3-A4-A5");
            string result = System.BitConverter.ToString(target);
            Assert.AreEqual("FF-FF-FF-FF-FF-FF-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-A0-A1-A2-A3-A4-A5", result);
        }

    }
}
