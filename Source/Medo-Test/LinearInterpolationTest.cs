using Medo.Math;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test {

    /// <summary>
    ///This is a test class for LinearInterpolationTest and is intended
    ///to contain all LinearInterpolationTest Unit Tests
    ///</summary>
    [TestClass()]
    public class LinearInterpolationTest {

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
        public void LinearInterpolation_GetAdjustedValue_Middle_1_Test() {
            LinearInterpolation target = new LinearInterpolation();
            target.Add(2, 1);
            target.Add(6, 3);
            Assert.AreEqual(2, target.GetAdjustedValue(1));
            Assert.AreEqual(4, target.GetAdjustedValue(2));
            Assert.AreEqual(6, target.GetAdjustedValue(3));
        }

        [TestMethod()]
        public void LinearInterpolation_GetAdjustedValue_Middle_2_Test() {
            LinearInterpolation target = new LinearInterpolation();
            target.Add(-1, -10);
            target.Add(1, 10);
            Assert.AreEqual(0, target.GetAdjustedValue(0));
            Assert.AreEqual(0.5, target.GetAdjustedValue(5));
        }

        [TestMethod()]
        public void LinearInterpolation_GetAdjustedValue_Middle_3_Test() {
            LinearInterpolation target = new LinearInterpolation();
            target.Add(0, 0);
            target.Add(1, 10);
            Assert.AreEqual(0.5, target.GetAdjustedValue(5));
        }

        [TestMethod()]
        public void LinearInterpolation_GetAdjustedValue_Below_1_Test() {
            LinearInterpolation target = new LinearInterpolation();
            target.Add(1, 0);
            Assert.AreEqual(1, target.GetAdjustedValue(0));
            Assert.AreEqual(2, target.GetAdjustedValue(1));
        }

        [TestMethod()]
        public void LinearInterpolation_GetAdjustedValue_Below_2_Test() {
            LinearInterpolation target = new LinearInterpolation();
            target.Add(2, 1);
            target.Add(4, 2);
            target.Add(6, 3);
            Assert.AreEqual(8, target.GetAdjustedValue(4));
            Assert.AreEqual(10, target.GetAdjustedValue(5));
        }

        [TestMethod()]
        public void LinearInterpolation_GetAdjustedValue_Above_1_Test() {
            LinearInterpolation target = new LinearInterpolation();
            target.Add(11, 10);
            Assert.AreEqual(11, target.GetAdjustedValue(10));
            Assert.AreEqual(10, target.GetAdjustedValue(9));
        }

        [TestMethod()]
        public void LinearInterpolation_GetAdjustedValue_Above_2_Test() {
            LinearInterpolation target = new LinearInterpolation();
            target.Add(6, 3);
            target.Add(8, 4);
            target.Add(10, 5);
            Assert.AreEqual(2, target.GetAdjustedValue(1));
            Assert.AreEqual(4, target.GetAdjustedValue(2));
        }

        [TestMethod()]
        public void LinearInterpolation_GetAdjustedValue_CalibratedThreePoints_Test() {
            LinearInterpolation target = new LinearInterpolation();
            target.Add(1, 1.1);
            target.Add(2, 1.2);
            target.Add(3, 1.3);
            Assert.AreEqual(2.5, target.GetAdjustedValue(1.25));
        }

        [TestMethod()]
        public void LinearInterpolation_GetAdjustedValue_CalibratedValueHit_Test() {
            LinearInterpolation target = new LinearInterpolation();
            target.Add(1, 0);
            target.Add(2, 1);
            target.Add(3, 2);
            Assert.AreEqual(3, target.GetAdjustedValue(2));
        }

        [TestMethod()]
        public void LinearInterpolation_GetAdjustedValue_NoCalibration_Test() {
            LinearInterpolation target = new LinearInterpolation();
            Assert.AreEqual(0, target.GetAdjustedValue(0));
            Assert.AreEqual(1, target.GetAdjustedValue(1));
        }
    }
}
