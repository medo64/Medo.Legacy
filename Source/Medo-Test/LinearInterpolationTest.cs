using Medo.Math;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test {

    /// <summary>
    ///This is a test class for LinearInterpolationTest and is intended
    ///to contain all LinearInterpolationTest Unit Tests
    ///</summary>
    [TestClass()]
    public class LinearInterpolationTest {

        public TestContext TestContext { get; set; }


        [TestMethod()]
        public void LinearInterpolation_GetAdjustedValue_Middle_1() {
            LinearInterpolation target = new LinearInterpolation();
            target.Add(2, 1);
            target.Add(6, 3);
            Assert.AreEqual(2, target.GetAdjustedValue(1));
            Assert.AreEqual(4, target.GetAdjustedValue(2));
            Assert.AreEqual(6, target.GetAdjustedValue(3));
        }

        [TestMethod()]
        public void LinearInterpolation_GetAdjustedValue_Middle_2() {
            LinearInterpolation target = new LinearInterpolation();
            target.Add(-1, -10);
            target.Add(1, 10);
            Assert.AreEqual(0, target.GetAdjustedValue(0));
            Assert.AreEqual(0.5, target.GetAdjustedValue(5));
        }

        [TestMethod()]
        public void LinearInterpolation_GetAdjustedValue_Middle_3() {
            LinearInterpolation target = new LinearInterpolation();
            target.Add(0, 0);
            target.Add(1, 10);
            Assert.AreEqual(0.5, target.GetAdjustedValue(5));
        }

        [TestMethod()]
        public void LinearInterpolation_GetAdjustedValue_Below_1() {
            LinearInterpolation target = new LinearInterpolation();
            target.Add(1, 0);
            Assert.AreEqual(1, target.GetAdjustedValue(0));
            Assert.AreEqual(2, target.GetAdjustedValue(1));
        }

        [TestMethod()]
        public void LinearInterpolation_GetAdjustedValue_Below_2() {
            LinearInterpolation target = new LinearInterpolation();
            target.Add(2, 1);
            target.Add(4, 2);
            target.Add(6, 3);
            Assert.AreEqual(8, target.GetAdjustedValue(4));
            Assert.AreEqual(10, target.GetAdjustedValue(5));
        }

        [TestMethod()]
        public void LinearInterpolation_GetAdjustedValue_Above_1() {
            LinearInterpolation target = new LinearInterpolation();
            target.Add(11, 10);
            Assert.AreEqual(11, target.GetAdjustedValue(10));
            Assert.AreEqual(10, target.GetAdjustedValue(9));
        }

        [TestMethod()]
        public void LinearInterpolation_GetAdjustedValue_Above_2() {
            LinearInterpolation target = new LinearInterpolation();
            target.Add(6, 3);
            target.Add(8, 4);
            target.Add(10, 5);
            Assert.AreEqual(2, target.GetAdjustedValue(1));
            Assert.AreEqual(4, target.GetAdjustedValue(2));
        }

        [TestMethod()]
        public void LinearInterpolation_GetAdjustedValue_CalibratedThreePoints() {
            LinearInterpolation target = new LinearInterpolation();
            target.Add(1, 1.1);
            target.Add(2, 1.2);
            target.Add(3, 1.3);
            Assert.AreEqual(2.5, target.GetAdjustedValue(1.25));
        }

        [TestMethod()]
        public void LinearInterpolation_GetAdjustedValue_CalibratedValueHit() {
            LinearInterpolation target = new LinearInterpolation();
            target.Add(1, 0);
            target.Add(2, 1);
            target.Add(3, 2);
            Assert.AreEqual(3, target.GetAdjustedValue(2));
        }

        [TestMethod()]
        public void LinearInterpolation_GetAdjustedValue_NoCalibration() {
            LinearInterpolation target = new LinearInterpolation();
            Assert.AreEqual(0, target.GetAdjustedValue(0));
            Assert.AreEqual(1, target.GetAdjustedValue(1));
        }

        [TestMethod()]
        public void LinearInterpolation_GetAdjustedValue_OnePoint() {
            LinearInterpolation target = new LinearInterpolation();
            target.Add(0, 1);
            Assert.AreEqual(0, target.GetAdjustedValue(1));
        }

    }
}
