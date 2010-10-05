using Medo.Security.Checksum;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace Test
{
    
    
    /// <summary>
    ///This is a test class for Iso7064Test and is intended
    ///to contain all Iso7064Test Unit Tests
    ///</summary>
    [TestClass()]
    public class Iso7064Test {


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
        ///A test for Digest property
        ///</summary>
        [TestMethod()]
        public void GetDigest1() {
            Iso7064 actualCrc = new Iso7064();
            actualCrc.Append("0");
            char actual = actualCrc.Digest;
            Assert.AreEqual('2', actual);
        }

        /// <summary>
        ///A test for Digest property
        ///</summary>
        [TestMethod()]
        public void GetDigest2() {
            Iso7064 actualCrc = new Iso7064();
            actualCrc.Append("1");
            char actual = actualCrc.Digest;
            Assert.AreEqual('9', actual);
        }

        /// <summary>
        ///A test for Digest property
        ///</summary>
        [TestMethod()]
        public void GetDigest3() {
            Iso7064 actualCrc = new Iso7064();
            actualCrc.Append("6");
            char actual = actualCrc.Digest;
            Assert.AreEqual('0', actual);
        }

        /// <summary>
        ///A test for Digest property
        ///</summary>
        [TestMethod()]
        public void GetDigest4() {
            Iso7064 actualCrc = new Iso7064();
            actualCrc.Append("9");
            char actual = actualCrc.Digest;
            Assert.AreEqual('4', actual);
        }

        /// <summary>
        ///A test for Digest property
        ///</summary>
        [TestMethod()]
        public void GetDigest5() {
            Iso7064 actualCrc = new Iso7064();
            actualCrc.Append("0823");
            char actual = actualCrc.Digest;
            Assert.AreEqual('5', actual);
        }

        /// <summary>
        ///A test for Digest property
        ///</summary>
        [TestMethod()]
        public void GetDigest6() {
            Iso7064 actualCrc = new Iso7064();
            actualCrc.Append("276616973212561");
            char actual = actualCrc.Digest;
            Assert.AreEqual('5', actual);
        }

        /// <summary>
        ///A test for Digest property
        ///</summary>
        [TestMethod()]
        public void GetDigest7() {
            Iso7064 actualCrc = new Iso7064();
            actualCrc.Append("65");
            char actual = actualCrc.Digest;
            Assert.AreEqual('0', actual);
        }

        /// <summary>
        ///A test for Digest property
        ///</summary>
        [TestMethod()]
        public void GetDigest8() {
            Iso7064 actualCrc = new Iso7064();
            actualCrc.Append("56");
            char actual = actualCrc.Digest;
            Assert.AreEqual('0', actual);
        }

        /// <summary>
        ///A test for Digest property
        ///</summary>
        [TestMethod()]
        public void GetDigest9() {
            Iso7064 actualCrc = new Iso7064();
            actualCrc.Append("732");
            char actual = actualCrc.Digest;
            Assert.AreEqual('5', actual);
        }

        /// <summary>
        ///A test for Digest property
        ///</summary>
        [TestMethod()]
        public void GetDigest10() {
            Iso7064 actualCrc = new Iso7064();
            actualCrc.Append("723");
            char actual = actualCrc.Digest;
            Assert.AreEqual('5', actual);
        }

        /// <summary>
        ///A test for Digest property
        ///</summary>
        [TestMethod()]
        public void GetDigest11() {
            Iso7064 actualCrc = new Iso7064();
            actualCrc.Append("8373426074");
            char actual = actualCrc.Digest;
            Assert.AreEqual('9', actual);
        }

        /// <summary>
        ///A test for Digest property
        ///</summary>
        [TestMethod()]
        public void GetDigest12() {
            Iso7064 actualCrc = new Iso7064();
            actualCrc.Append("4428922675");
            char actual = actualCrc.Digest;
            Assert.AreEqual('7', actual);
        }

    }
}
