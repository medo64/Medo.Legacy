using Medo.Security.Checksum;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace Test
{
    
    
    /// <summary>
    ///This is a test class for Crc32Test and is intended
    ///to contain all Crc32Test Unit Tests
    ///</summary>
    [TestClass()]
    public class Crc32Test {


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
        ///A test for GetXfer
        ///</summary>
        [TestMethod()]
        public void GetXferTest() {
            string expected = "3A9C355C";
            Crc32 actualCrc = Crc32.GetXfer();
            actualCrc.Append("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ", true);
            string actual = actualCrc.Digest.ToString("X8");
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for GetPosix
        ///</summary>
        [TestMethod()]
        public void GetPosixTest() {
            string expected = "EFC8804E";
            Crc32 actualCrc = Crc32.GetPosix();
            actualCrc.Append("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ", true);
            string actual = actualCrc.Digest.ToString("X8");
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for GetJam
        ///</summary>
        [TestMethod()]
        public void GetJamTest() {
            string expected = "E59A841D";
            Crc32 actualCrc = Crc32.GetJam();
            actualCrc.Append("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ", true);
            string actual = actualCrc.Digest.ToString("X8");
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for GetIeee
        ///</summary>
        [TestMethod()]
        public void GetIeeeTest() {
            string expected = "1A657BE2";
            Crc32 actualCrc = Crc32.GetIeee();
            actualCrc.Append("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ", true);
            string actual = actualCrc.Digest.ToString("X8");
            Assert.AreEqual(expected, actual);
        }
    }
}
