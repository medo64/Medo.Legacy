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


        /// <summary>
        ///A test for GetAdjustedValue
        ///</summary>
        [TestMethod()]
        public void LinearInterpolation_GetAdjustedValue_1_Test() {
            LinearInterpolation target = new LinearInterpolation(); // TODO: Initialize to an appropriate value
            target.AddCalibration(0, 1);
            target.AddCalibration(10, 12);
            Assert.AreEqual(5 + 1.5, target.GetAdjustedValue(5));
        }

        [TestMethod()]
        public void LinearInterpolation_GetAdjustedValue_2_Test() {
            LinearInterpolation target = new LinearInterpolation();
            target.AddCalibration(0, 2);
            target.AddCalibration(10, 11);
            Assert.AreEqual(5 + 1.5, target.GetAdjustedValue(5));
        }

        [TestMethod()]
        public void LinearInterpolation_GetAdjustedValue_3_Test() {
            LinearInterpolation target = new LinearInterpolation(); // TODO: Initialize to an appropriate value
            target.AddCalibration(0, 1);
            target.AddCalibration(10, 9);
            Assert.AreEqual(5 + 0, target.GetAdjustedValue(5));
        }

        [TestMethod()]
        public void LinearInterpolation_GetAdjustedValue_4_Test() {
            LinearInterpolation target = new LinearInterpolation(); // TODO: Initialize to an appropriate value
            target.AddCalibration(0, 0);
            target.AddCalibration(3, 6);
            Assert.AreEqual(1 + 1, target.GetAdjustedValue(1));
        }

        [TestMethod()]
        public void LinearInterpolation_GetAdjustedValue_CalibratedThreePoints_Test() {
            LinearInterpolation target = new LinearInterpolation();
            target.AddCalibration(0, 1);
            target.AddCalibration(1, 1);
            target.AddCalibration(2, 1);
            Assert.AreEqual(1.5 + -0.5, target.GetAdjustedValue(1.5));
        }

        [TestMethod()]
        public void LinearInterpolation_GetAdjustedValue_CalibratedValueHit_Test() {
            LinearInterpolation target = new LinearInterpolation();
            target.AddCalibration(0, 1);
            target.AddCalibration(1, 1);
            target.AddCalibration(2, 1);
            Assert.AreEqual(2 + -1, target.GetAdjustedValue(2));
        }

        [TestMethod()]
        public void LinearInterpolation_GetAdjustedValue_OnlyCalibratedBelow_Test() {
            LinearInterpolation target = new LinearInterpolation();
            target.AddCalibration(0, 1);
            Assert.AreEqual(2 + 1, target.GetAdjustedValue(2));
        }

        [TestMethod()]
        public void LinearInterpolation_GetAdjustedValue_OnlyCalibratedAbove_Test() {
            LinearInterpolation target = new LinearInterpolation();
            target.AddCalibration(0, 1);
            Assert.AreEqual(-2 + 1, target.GetAdjustedValue(-2));
        }

        [TestMethod()]
        public void LinearInterpolation_GetAdjustedValue_NoCalibration_Test() {
            LinearInterpolation target = new LinearInterpolation();
            Assert.AreEqual(0, target.GetAdjustedValue(0));
            Assert.AreEqual(1, target.GetAdjustedValue(1));
        }
    }
}
