using Medo.Security.Checksum;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace Test
{
    
    
    /// <summary>
    ///This is a test class for Crc16Test and is intended
    ///to contain all Crc16Test Unit Tests
    ///</summary>
    [TestClass()]
    public class Crc16Test {


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
        ///A test for GetZmodem
        ///</summary>
        [TestMethod()]
        public void GetZmodemTest() {
            string expected = "5E1B";
            Crc16 actualCrc = Crc16.GetZmodem();
            actualCrc.Append("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ", true);
            string actual = actualCrc.Digest.ToString("X4");
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for GetXmodem
        ///</summary>
        [TestMethod()]
        public void GetXmodemTest() {
            string expected = "16A3";
            Crc16 actualCrc = Crc16.GetXmodem();
            actualCrc.Append("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ", true);
            string actual = actualCrc.Digest.ToString("X4");
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for GetX25
        ///</summary>
        [TestMethod()]
        public void GetX25Test() {
            string expected = "CB47";
            Crc16 actualCrc = Crc16.GetX25();
            actualCrc.Append("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ", true);
            string actual = actualCrc.Digest.ToString("X4");
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for GetKermit
        ///</summary>
        [TestMethod()]
        public void GetKermitTest() {
            string expected = "9839";
            Crc16 actualCrc = Crc16.GetKermit();
            actualCrc.Append("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ", true);
            string actual = actualCrc.Digest.ToString("X4");
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for GetIeee
        ///</summary>
        [TestMethod()]
        public void GetIeeeTest() {
            string expected = "178C";
            Crc16 actualCrc = Crc16.GetIeee();
            actualCrc.Append("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ", true);
            string actual = actualCrc.Digest.ToString("X4");
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for GetCcitt
        ///</summary>
        [TestMethod()]
        public void GetCcittTest() {
            string expected = "DF2E";
            Crc16 actualCrc = Crc16.GetCcitt();
            actualCrc.Append("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ", true);
            string actual = actualCrc.Digest.ToString("X4");
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for GetArc
        ///</summary>
        [TestMethod()]
        public void GetArcTest() {
            string expected = "178C";
            Crc16 actualCrc = Crc16.GetArc();
            actualCrc.Append("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ", true);
            string actual = actualCrc.Digest.ToString("X4");
            Assert.AreEqual(expected, actual);
        }
    }
}
