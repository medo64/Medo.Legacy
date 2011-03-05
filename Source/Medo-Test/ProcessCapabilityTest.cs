using Medo.Math;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Test
{
    
    
    /// <summary>
    ///This is a test class for ProcessCapabilityTest and is intended
    ///to contain all ProcessCapabilityTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ProcessCapabilityTest {


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


        [TestMethod()]
        public void BasicMeanAndStDevTest1() {
            ProcessCapability target = new ProcessCapability();
            target.Add(0);
            target.Add(0);
            target.Add(14);
            target.Add(14);
            Assert.AreEqual(7, target.Mean);
            Assert.AreEqual(7, target.StDev);
        }

        [TestMethod()]
        public void BasicMeanAndStDevTest2() {
            ProcessCapability target = new ProcessCapability();
            target.Add(0);
            target.Add(6);
            target.Add(8);
            target.Add(14);
            Assert.AreEqual(7, target.Mean);
            Assert.AreEqual(5, target.StDev);
        }

        [TestMethod()]
        public void BasicMeanAndStDevTest3() {
            ProcessCapability target = new ProcessCapability();
            target.Add(6);
            target.Add(6);
            target.Add(8);
            target.Add(8);
            Assert.AreEqual(7, target.Mean);
            Assert.AreEqual(1, target.StDev);
        }

    }
}
