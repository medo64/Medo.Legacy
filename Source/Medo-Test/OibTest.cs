using Medo.Localization.Croatia;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace Test {


    /// <summary>
    ///This is a test class for OibTest and is intended
    ///to contain all OibTest Unit Tests
    ///</summary>
    [TestClass()]
    public class OibTest {


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
        ///A test.
        ///</summary>
        [TestMethod()]
        public void OibTest_1() {
            Oib target = new Oib("83734260749");
            Assert.AreEqual(true, target.IsValid);
        }

        /// <summary>
        ///A test.
        ///</summary>
        [TestMethod()]
        public void OibTest_2() {
            Oib target = new Oib("44289226757");
            Assert.AreEqual(true, target.IsValid);
        }

        /// <summary>
        ///A test.
        ///</summary>
        [TestMethod()]
        public void OibTest_3() {
            Oib target = new Oib("11111111111");
            Assert.AreEqual(false, target.IsValid);
        }

        /// <summary>
        ///A test.
        ///</summary>
        [TestMethod()]
        public void OibTest_4() {
            Oib target = new Oib("7235");
            Assert.AreEqual(false, target.IsValid);
        }

    }
}
