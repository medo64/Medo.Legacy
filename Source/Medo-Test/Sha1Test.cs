using Medo.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace Test {


    /// <summary>
    ///This is a test class for Sha1Test and is intended
    ///to contain all Sha1Test Unit Tests
    ///</summary>
    [TestClass()]
    public class Sha1Test {


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
        ///A test for ComputeHash
        ///</summary>
        [TestMethod()]
        public void ComputeHashTest() {
            byte[] result = Sha1.ComputeHash(new byte[] { 0, 1, 2, 3 }, 1, 2);
            string resultString = System.BitConverter.ToString(result);

            Assert.AreEqual("0C-A6-23-E2-85-5F-2C-75-C8-42-AD-30-2F-E8-20-E4-1B-4D-19-7D", resultString);
        }
    }
}
