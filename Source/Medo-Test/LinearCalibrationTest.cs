using Medo.Math;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Test {

    /// <summary>
    ///This is a test class for LinearCalibrationTest and is intended
    ///to contain all LinearCalibrationTest Unit Tests
    ///</summary>
    [TestClass()]
    public class LinearCalibrationTest {

        public TestContext TestContext { get; set; }



        [TestMethod()]
        public void LinearCalibration_NoCalibration() {
            LinearCalibration target = new LinearCalibration();
            Assert.AreEqual(0, target.GetAdjustedValue(0));
            Assert.AreEqual(1, target.GetAdjustedValue(1));
            Assert.AreEqual(1, target.CorrelationCoefficient);
        }


        [TestMethod()]
        public void LinearCalibration_OnePoint() {
            LinearCalibration target = new LinearCalibration();
            target.AddCalibrationPoint(0, 1);
            Assert.AreEqual(0, target.GetAdjustedValue(1));
            Assert.AreEqual(1, target.CorrelationCoefficient);
        }

        [TestMethod()]
        public void LinearCalibration_OnePoint_NoChange() {
            LinearCalibration target = new LinearCalibration();
            target.AddCalibrationPoint(0, 0);
            Assert.AreEqual(1, target.GetAdjustedValue(1));
            Assert.AreEqual(1, target.CorrelationCoefficient);
        }


        [TestMethod()]
        public void LinearCalibration_TwoPoint_Simple() {
            LinearCalibration target = new LinearCalibration();
            target.AddCalibrationPoint(0, 1);
            target.AddCalibrationPoint(100, 101);
            Assert.AreEqual(50, target.GetAdjustedValue(51));
            Assert.AreEqual(1, target.CorrelationCoefficient);
        }

        [TestMethod()]
        public void LinearCalibration_TwoPoint_Double() {
            LinearCalibration target = new LinearCalibration();
            target.AddCalibrationPoint(0, 0);
            target.AddCalibrationPoint(100, 50);
            Assert.AreEqual(50, target.GetAdjustedValue(25));
            Assert.AreEqual(1, target.CorrelationCoefficient);
        }


        [TestMethod()]
        public void LinearCalibration_ThreePoint_Simple() {
            LinearCalibration target = new LinearCalibration();
            target.AddCalibrationPoint(0, 1);
            target.AddCalibrationPoint(50, 51);
            target.AddCalibrationPoint(100, 101);
            Assert.AreEqual(25, target.GetAdjustedValue(26));
            Assert.AreEqual(1, target.CorrelationCoefficient);
        }

        [TestMethod()]
        public void LinearCalibration_ThreePoint_Double() {
            LinearCalibration target = new LinearCalibration();
            target.AddCalibrationPoint(0, 0);
            target.AddCalibrationPoint(50, 25);
            target.AddCalibrationPoint(100, 50);
            Assert.AreEqual(80, target.GetAdjustedValue(40));
            Assert.AreEqual(1, target.CorrelationCoefficient);
        }

        [TestMethod()]
        public void LinearCalibration_ThreePoint_Halve() {
            LinearCalibration target = new LinearCalibration();
            target.AddCalibrationPoint(0, 0);
            target.AddCalibrationPoint(50, 100);
            target.AddCalibrationPoint(100, 200);
            Assert.AreEqual(75, target.GetAdjustedValue(150));
            Assert.AreEqual(1, target.CorrelationCoefficient);
        }

        [TestMethod()]
        public void LinearCalibration_ThreePoint_Freestyle_1() {
            LinearCalibration target = new LinearCalibration();
            target.AddCalibrationPoint(0, 0.5);
            target.AddCalibrationPoint(50, 50);
            target.AddCalibrationPoint(100, 99.5);
            Assert.AreEqual(24.74747, System.Math.Round(target.GetAdjustedValue(25), 5));
            Assert.AreEqual(50, target.GetAdjustedValue(50));
            Assert.AreEqual(75.25253, System.Math.Round(target.GetAdjustedValue(75), 5));
            Assert.AreEqual(1, target.CorrelationCoefficient);
        }

        [TestMethod()]
        public void LinearCalibration_ThreePoint_Freestyle_2() {
            LinearCalibration target = new LinearCalibration();
            target.AddCalibrationPoint(0, 1);
            target.AddCalibrationPoint(50, 49);
            target.AddCalibrationPoint(100, 100);
            Assert.AreEqual(24.74747, System.Math.Round(target.GetAdjustedValue(25), 5));
            Assert.AreEqual(50, target.GetAdjustedValue(50));
            Assert.AreEqual(75.25253, System.Math.Round(target.GetAdjustedValue(75), 5));
            Assert.AreEqual(0.99985, Math.Round(target.CorrelationCoefficient, 5));
        }

        [TestMethod()]
        public void LinearCalibration_ThreePoint_Freestyle_3() {
            LinearCalibration target = new LinearCalibration();
            target.AddCalibrationPoint(0, 1);
            target.AddCalibrationPoint(20, -10);
            target.AddCalibrationPoint(100, 50);
            Assert.AreEqual(60.08439, System.Math.Round(target.GetAdjustedValue(25), 5));
            Assert.AreEqual(104.38819, System.Math.Round(target.GetAdjustedValue(50), 5));
            Assert.AreEqual(148.69198, System.Math.Round(target.GetAdjustedValue(75), 5));
            Assert.AreEqual(0.93477, Math.Round(target.CorrelationCoefficient, 5));
        }


        [TestMethod()]
        public void LinearCalibration_Multipoint() {
            LinearCalibration target = new LinearCalibration();
            target.AddCalibrationPoint(0, 0.025);
            target.AddCalibrationPoint(1, 0.217);
            target.AddCalibrationPoint(2, 0.388);
            target.AddCalibrationPoint(3, 0.634);
            target.AddCalibrationPoint(4, 0.777);
            target.AddCalibrationPoint(5, 1.011);
            target.AddCalibrationPoint(6, 1.166);
            Assert.AreEqual(0.1929, Math.Round(target.Slope, 4));
            Assert.AreEqual(0.0240, Math.Round(target.Intercept, 4));
            Assert.AreEqual(0.9976, Math.Round(target.CoefficientOfDetermination, 4));
            Assert.AreEqual(1.19, Math.Round(target.GetAdjustedValue(0.254), 2));
        }

    }
}
