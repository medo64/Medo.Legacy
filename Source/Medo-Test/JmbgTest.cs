using Medo.Localization.Croatia;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test {

    [TestClass()]
    public class JmbgTest {

        public TestContext TestContext { get; set; }


        [TestMethod()]
        public void Jmbg_1() {
            Jmbg target = new Jmbg("2801979302606", false);
            Assert.AreEqual(true, target.IsValid);
            Assert.AreEqual(true, target.IsBirthDateValid);
            Assert.AreEqual(new System.DateTime(1979, 1, 28), target.BirthDate);
            Assert.AreEqual(JmbgRegion.Croatia, target.Region);
            Assert.AreEqual(JmbgGender.Male, target.Gender);
        }

        [TestMethod()]
        public void Jmbg_2() {
            Jmbg target = new Jmbg("2801979", false);
            Assert.AreEqual(false, target.IsValid);
            Assert.AreEqual(true, target.IsBirthDateValid);
            Assert.AreEqual(new System.DateTime(1979, 1, 28), target.BirthDate);
            Assert.AreEqual(JmbgRegion.Unknown, target.Region);
            Assert.AreEqual(JmbgGender.Unknown, target.Gender);
        }

        [TestMethod()]
        public void Jmbg_3() {
            Jmbg target = new Jmbg("2902980", false);
            Assert.AreEqual(false, target.IsValid);
            Assert.AreEqual(true, target.IsBirthDateValid);
            Assert.AreEqual(new System.DateTime(1980, 2, 29), target.BirthDate);
            Assert.AreEqual(JmbgRegion.Unknown, target.Region);
            Assert.AreEqual(JmbgGender.Unknown, target.Gender);
        }

        [TestMethod()]
        public void Jmbg_Oib_0() {
            Jmbg target = new Jmbg("83734260749", false);
            Assert.AreEqual(false, target.IsValid);
            Assert.AreEqual(false, target.IsBirthDateValid);
            Assert.AreEqual(JmbgRegion.Unknown, target.Region);
            Assert.AreEqual(JmbgGender.Unknown, target.Gender);
        }

        [TestMethod()]
        public void Jmbg_Oib_1() {
            Jmbg target = new Jmbg("83734260749", true);
            Assert.AreEqual(true, target.IsValid);
            Assert.AreEqual(false, target.IsBirthDateValid);
            Assert.AreEqual(JmbgRegion.Unknown, target.Region);
            Assert.AreEqual(JmbgGender.Unknown, target.Gender);
        }

        [TestMethod()]
        public void Jmbg_Oib_2() {
            Jmbg target = new Jmbg("44289226757", true);
            Assert.AreEqual(true, target.IsValid);
            Assert.AreEqual(false, target.IsBirthDateValid);
            Assert.AreEqual(JmbgRegion.Unknown, target.Region);
            Assert.AreEqual(JmbgGender.Unknown, target.Gender);
        }

        [TestMethod()]
        public void Jmbg_Oib_3() {
            Jmbg target = new Jmbg("11111111111", true);
            Assert.AreEqual(false, target.IsValid);
            Assert.AreEqual(false, target.IsBirthDateValid);
            Assert.AreEqual(JmbgRegion.Unknown, target.Region);
            Assert.AreEqual(JmbgGender.Unknown, target.Gender);
        }

    }
}
