using Medo.Math;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Test {

    [TestClass()]
    public class ProcessCapabilityTest {

        public TestContext TestContext { get; set; }


        [TestMethod()]
        public void ProcessCapability_MeanAndStDev_Basic_1() {
            ProcessCapability target = new ProcessCapability();
            target.UseBesselCorrection = false;
            target.Add(0);
            target.Add(0);
            target.Add(14);
            target.Add(14);
            Assert.AreEqual(7, target.Mean);
            Assert.AreEqual(7, target.StDev);
            Assert.AreEqual(0, target.Minimum);
            Assert.AreEqual(14, target.Maximum);
        }

        [TestMethod()]
        public void ProcessCapability_MeanAndStDev_Basic_2() {
            ProcessCapability target = new ProcessCapability();
            target.UseBesselCorrection = false;
            target.Add(0);
            target.Add(6);
            target.Add(8);
            target.Add(14);
            Assert.AreEqual(7, target.Mean);
            Assert.AreEqual(5, target.StDev);
            Assert.AreEqual(0, target.Minimum);
            Assert.AreEqual(14, target.Maximum);
        }

        [TestMethod()]
        public void ProcessCapability_MeanAndStDev_Basic_3() {
            ProcessCapability target = new ProcessCapability();
            target.UseBesselCorrection = false;
            target.Add(6);
            target.Add(6);
            target.Add(8);
            target.Add(8);
            Assert.AreEqual(7, target.Mean);
            Assert.AreEqual(1, target.StDev);
            Assert.AreEqual(6, target.Minimum);
            Assert.AreEqual(8, target.Maximum);
        }


        [TestMethod()]
        public void ProcessCapability_Indices_1() {
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
        public void ProcessCapability_Indices_2() {
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
        public void ProcessCapability_Indices_3() {
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
        public void ProcessCapability_Indices_Incremental() {
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
        public void ProcessCapability_Indices_NaN() {
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
