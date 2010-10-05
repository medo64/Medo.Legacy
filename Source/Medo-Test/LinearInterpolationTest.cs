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
            target.Add(0, 1);
            target.Add(10, 2);
            var expected = 5 + 1.5;
            var actual = target.GetAdjustedValue(5);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void LinearInterpolation_GetAdjustedValue_2_Test() {
            LinearInterpolation target = new LinearInterpolation();
            target.Add(0, 2);
            target.Add(10, 1);
            var expected = 5 + 1.5;
            var actual = target.GetAdjustedValue(5);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void LinearInterpolation_GetAdjustedValue_3_Test() {
            LinearInterpolation target = new LinearInterpolation(); // TODO: Initialize to an appropriate value
            target.Add(0, 1);
            target.Add(10, -1);
            var expected = 5 + 0;
            var actual = target.GetAdjustedValue(5);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void LinearInterpolation_GetAdjustedValue_4_Test() {
            LinearInterpolation target = new LinearInterpolation(); // TODO: Initialize to an appropriate value
            target.Add(0, 0);
            target.Add(3, 3);
            var expected = 1 + 1;
            var actual = target.GetAdjustedValue(1);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void LinearInterpolation_GetAdjustedValue_CalibratedThreePoints_Test() {
            LinearInterpolation target = new LinearInterpolation();
            target.Add(0, 1);
            target.Add(1, 0);
            target.Add(2, -1);
            var expected = 1.5 + -0.5;
            var actual = target.GetAdjustedValue(1.5);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void LinearInterpolation_GetAdjustedValue_CalibratedValueHit_Test() {
            LinearInterpolation target = new LinearInterpolation();
            target.Add(0, 1);
            target.Add(1, 0);
            target.Add(2, -1);
            var expected = 2 + -1;
            var actual = target.GetAdjustedValue(2);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void LinearInterpolation_GetAdjustedValue_OnlyCalibratedBelow_Test() {
            LinearInterpolation target = new LinearInterpolation();
            target.Add(0, 1);
            var expected = 2 + 1;
            var actual = target.GetAdjustedValue(2);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void LinearInterpolation_GetAdjustedValue_OnlyCalibratedAbove_Test() {
            LinearInterpolation target = new LinearInterpolation();
            target.Add(0, 1);
            var expected = -2 + 1;
            var actual = target.GetAdjustedValue(-2);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void LinearInterpolation_GetAdjustedValue_NoCalibration_Test() {
            LinearInterpolation target = new LinearInterpolation();
            var expected = 0;
            var actual = target.GetAdjustedValue(0);
            Assert.AreEqual(expected, actual);
        }
    }
}
