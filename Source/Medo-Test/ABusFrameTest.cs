using Medo.Device.Protocol;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Medo.Extensions.HexadecimalEncoding;

namespace Test
{
    
    
    /// <summary>
    ///This is a test class for ABusFrameTest and is intended
    ///to contain all ABusFrameTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ABusFrameTest {


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
        ///A test for Parse
        ///</summary>
        [TestMethod()]
        public void ParseTest() {
            var expected = "aa550500010000007d23000000001100007acf";
            var actualFrame = ABusFrame.Parse(HexadecimalEncodingExtensions.FromHexString("aa550500010000007d23000000001100007acf"));
            var actual = HexadecimalEncodingExtensions.ToHexString(actualFrame.GetBytes());
            Assert.AreEqual(expected, actual);
        }
    }
}
