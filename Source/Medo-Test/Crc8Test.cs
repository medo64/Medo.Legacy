using Medo.Security.Checksum;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace Test
{
    
    
    /// <summary>
    ///This is a test class for Crc8Test and is intended
    ///to contain all Crc8Test Unit Tests
    ///</summary>
    [TestClass()]
    public class Crc8Test {


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
        ///A test for GetDallas
        ///</summary>
        [TestMethod()]
        public void GetDallasTest() {
            string expected = "80";
            Crc8 actualCrc = Crc8.GetDallas();
            actualCrc.Append("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ", true);
            string actual = actualCrc.Digest.ToString("X2");
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for GetMaxim
        ///</summary>
        [TestMethod()]
        public void GetMaximTest() {
            string expected = "80";
            Crc8 actualCrc = Crc8.GetMaxim();
            actualCrc.Append("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ", true);
            string actual = actualCrc.Digest.ToString("X2");
            Assert.AreEqual(expected, actual);
        }

    }
}
