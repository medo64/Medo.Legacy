using Medo.Math;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Test {


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
            target.UseBesselCorrection = false;
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
            target.UseBesselCorrection = false;
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
            target.UseBesselCorrection = false;
            target.Add(6);
            target.Add(6);
            target.Add(8);
            target.Add(8);
            Assert.AreEqual(7, target.Mean);
            Assert.AreEqual(1, target.StDev);
        }

        [TestMethod()]
        public void IndicesTest1() {
            ProcessCapability target = new ProcessCapability(0, 20, 10);
            target.Add(0);
            target.Add(0);
            target.Add(14);
            target.Add(14);
            Assert.AreEqual(7, target.Mean);
            Assert.AreEqual(8.08290, Math.Round(target.StDev, 5));
            Assert.AreEqual(0.41239, Math.Round(target.Cp, 5));
            Assert.AreEqual(0.28868, Math.Round(target.CpLower, 5));
            Assert.AreEqual(0.53611, Math.Round(target.CpUpper, 5));
            Assert.AreEqual(0.28868, Math.Round(target.Cpk, 5));
            Assert.AreEqual(0.38662, Math.Round(target.Cpm, 5));
            Assert.AreEqual(0.27064, Math.Round(target.Cpkm, 5));
        }

        [TestMethod()]
        public void IndicesTest2() {
            ProcessCapability target = new ProcessCapability(5, 15, 10);
            target.Add(0);
            target.Add(0);
            target.Add(14);
            target.Add(14);
            Assert.AreEqual(7, target.Mean);
            Assert.AreEqual(8.08290, Math.Round(target.StDev, 5));
            Assert.AreEqual(0.20620, Math.Round(target.Cp, 5));
            Assert.AreEqual(0.08248, Math.Round(target.CpLower, 5));
            Assert.AreEqual(0.32991, Math.Round(target.CpUpper, 5));
            Assert.AreEqual(0.08248, Math.Round(target.Cpk, 5));
            Assert.AreEqual(0.19331, Math.Round(target.Cpm, 5));
            Assert.AreEqual(0.07732, Math.Round(target.Cpkm, 5));
        }

        [TestMethod()]
        public void IndicesTest3() {
            ProcessCapability target = new ProcessCapability(2, 12, 7);
            target.Add(6);
            target.Add(6);
            target.Add(8);
            target.Add(8);
            Assert.AreEqual(7, target.Mean);
            Assert.AreEqual(1.15470, Math.Round(target.StDev, 5));
            Assert.AreEqual(1.44338, Math.Round(target.Cp, 5));
            Assert.AreEqual(1.44338, Math.Round(target.CpLower, 5));
            Assert.AreEqual(1.44338, Math.Round(target.CpUpper, 5));
            Assert.AreEqual(1.44338, Math.Round(target.Cpk, 5));
            Assert.AreEqual(1.44338, Math.Round(target.Cpm, 5));
            Assert.AreEqual(1.44338, Math.Round(target.Cpkm, 5));
        }

        [TestMethod()]
        public void IncrementalIndicesTest1() {
            ProcessCapability target = new ProcessCapability(0, 20, 10);
            target.Add(4);
            target.Add(6);
            Assert.AreEqual(5, target.Mean);
            Assert.AreEqual(1.41421, Math.Round(target.StDev, 5));
            Assert.AreEqual(2.35702, Math.Round(target.Cp, 5));
            Assert.AreEqual(1.17851, Math.Round(target.CpLower, 5));
            Assert.AreEqual(3.53553, Math.Round(target.CpUpper, 5));
            Assert.AreEqual(1.17851, Math.Round(target.Cpk, 5));
            Assert.AreEqual(0.64150, Math.Round(target.Cpm, 5));
            Assert.AreEqual(0.32075, Math.Round(target.Cpkm, 5));
            target.Add(8);
            target.Add(10);
            Assert.AreEqual(7, target.Mean);
            Assert.AreEqual(2.58199, Math.Round(target.StDev, 5));
            Assert.AreEqual(1.29099, Math.Round(target.Cp, 5));
            Assert.AreEqual(0.90370, Math.Round(target.CpLower, 5));
            Assert.AreEqual(1.67829, Math.Round(target.CpUpper, 5));
            Assert.AreEqual(0.90370, Math.Round(target.Cpk, 5));
            Assert.AreEqual(0.84215, Math.Round(target.Cpm, 5));
            Assert.AreEqual(0.58951, Math.Round(target.Cpkm, 5));
        }

        [TestMethod()]
        public void IndicesNaNTest() {
            ProcessCapability target = new ProcessCapability();
            target.Add(0);
            target.Add(0);
            target.Add(14);
            target.Add(14);
            Assert.AreEqual(7, target.Mean);
            Assert.AreEqual(8.08290, Math.Round(target.StDev, 5));
            Assert.AreEqual(double.NaN, target.Cp);
            Assert.AreEqual(double.NaN, Math.Round(target.CpLower, 5));
            Assert.AreEqual(double.NaN, Math.Round(target.CpUpper, 5));
            Assert.AreEqual(double.NaN, Math.Round(target.Cpk, 5));
            Assert.AreEqual(double.NaN, Math.Round(target.Cpm, 5));
            Assert.AreEqual(double.NaN, Math.Round(target.Cpkm, 5));
        }

    }
}
