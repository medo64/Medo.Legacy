using Medo.Localization.Croatia;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace Test
{
    
    
    /// <summary>
    ///This is a test class for NumberDeclinationTest and is intended
    ///to contain all NumberDeclinationTest Unit Tests
    ///</summary>
    [TestClass()]
    public class NumberDeclinationTest {


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
        ///A test for Item
        ///</summary>
        [TestMethod()]
        public void ItemTest() {
            NumberDeclination nd1 = new NumberDeclination("tim", "tima", "timova");
            Assert.AreEqual("1 tim", nd1[1]);
            Assert.AreEqual("2 tima", nd1[2]);
            Assert.AreEqual("3 tima", nd1[3]);
            Assert.AreEqual("4 tima", nd1[4]);
            Assert.AreEqual("5 timova", nd1[5]);
            Assert.AreEqual("6 timova", nd1[6]);
            Assert.AreEqual("11 timova", nd1[11]);
            Assert.AreEqual("21 tim", nd1[21]);
            Assert.AreEqual("101 tim", nd1[101]);

            NumberDeclination nd2 = new NumberDeclination("jabuka", "jabuke", "jabuka");
            Assert.AreEqual("1 jabuka", nd2[1]);
            Assert.AreEqual("2 jabuke", nd2[2]);
            Assert.AreEqual("3 jabuke", nd2[3]);
            Assert.AreEqual("5 jabuka", nd2[5]);
            Assert.AreEqual("6 jabuka", nd2[6]);
            Assert.AreEqual("11 jabuka", nd2[11]);
            Assert.AreEqual("21 jabuka", nd2[21]);
            Assert.AreEqual("101 jabuka", nd2[101]);
        }
    }
}
