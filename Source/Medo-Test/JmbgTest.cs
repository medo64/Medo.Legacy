using Medo.Localization.Croatia;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace Test {


    /// <summary>
    ///This is a test class for JmbgTest and is intended
    ///to contain all JmbgTest Unit Tests
    ///</summary>
    [TestClass()]
    public class JmbgTest {


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
        public void JmbgTest_1() {
            Jmbg target = new Jmbg("2801979302606", false);
            Assert.AreEqual(true, target.IsValid);
            Assert.AreEqual(true, target.IsBirthDateValid);
            Assert.AreEqual(new System.DateTime(1979, 1, 28), target.BirthDate);
            Assert.AreEqual(JmbgRegion.Croatia, target.Region);
            Assert.AreEqual(JmbgGender.Male, target.Gender);
        }

        /// <summary>
        ///A test.
        ///</summary>
        [TestMethod()]
        public void JmbgTest_2() {
            Jmbg target = new Jmbg("2801979", false);
            Assert.AreEqual(false, target.IsValid);
            Assert.AreEqual(true, target.IsBirthDateValid);
            Assert.AreEqual(new System.DateTime(1979, 1, 28), target.BirthDate);
            Assert.AreEqual(JmbgRegion.Unknown, target.Region);
            Assert.AreEqual(JmbgGender.Unknown, target.Gender);
        }

        /// <summary>
        ///A test.
        ///</summary>
        [TestMethod()]
        public void JmbgTest_3() {
            Jmbg target = new Jmbg("2902980", false);
            Assert.AreEqual(false, target.IsValid);
            Assert.AreEqual(true, target.IsBirthDateValid);
            Assert.AreEqual(new System.DateTime(1980, 2, 29), target.BirthDate);
            Assert.AreEqual(JmbgRegion.Unknown, target.Region);
            Assert.AreEqual(JmbgGender.Unknown, target.Gender);
        }

        /// <summary>
        ///A test.
        ///</summary>
        [TestMethod()]
        public void JmbgOibTest_0() {
            Jmbg target = new Jmbg("83734260749", false);
            Assert.AreEqual(false, target.IsValid);
            Assert.AreEqual(false, target.IsBirthDateValid);
            Assert.AreEqual(JmbgRegion.Unknown, target.Region);
            Assert.AreEqual(JmbgGender.Unknown, target.Gender);
        }

        /// <summary>
        ///A test.
        ///</summary>
        [TestMethod()]
        public void JmbgOibTest_1() {
            Jmbg target = new Jmbg("83734260749", true);
            Assert.AreEqual(true, target.IsValid);
            Assert.AreEqual(false, target.IsBirthDateValid);
            Assert.AreEqual(JmbgRegion.Unknown, target.Region);
            Assert.AreEqual(JmbgGender.Unknown, target.Gender);
        }

        /// <summary>
        ///A test.
        ///</summary>
        [TestMethod()]
        public void JmbgOibTest_2() {
            Jmbg target = new Jmbg("44289226757", true);
            Assert.AreEqual(true, target.IsValid);
            Assert.AreEqual(false, target.IsBirthDateValid);
            Assert.AreEqual(JmbgRegion.Unknown, target.Region);
            Assert.AreEqual(JmbgGender.Unknown, target.Gender);
        }

        /// <summary>
        ///A test.
        ///</summary>
        [TestMethod()]
        public void JmbgOibTest_3() {
            Jmbg target = new Jmbg("11111111111", true);
            Assert.AreEqual(false, target.IsValid);
            Assert.AreEqual(false, target.IsBirthDateValid);
            Assert.AreEqual(JmbgRegion.Unknown, target.Region);
            Assert.AreEqual(JmbgGender.Unknown, target.Gender);
        }

    }
}
