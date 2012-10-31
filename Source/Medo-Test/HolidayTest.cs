using Medo.Localization.Croatia;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Test {

    [TestClass()]
    public class HolidayTest {

        public TestContext TestContext { get; set; }


        [TestMethod()]
        public void Holiday_2002() {
            Assert.AreEqual(true, Holiday.IsHoliday(new DateTime(2002, 1, 1)));
            Assert.AreEqual(false, Holiday.IsHoliday(new DateTime(2002, 1, 6)));
            Assert.AreEqual(true, Holiday.IsHoliday(new DateTime(2002, 5, 1)));
            Assert.AreEqual(true, Holiday.IsHoliday(new DateTime(2002, 6, 22)));
            Assert.AreEqual(true, Holiday.IsHoliday(new DateTime(2002, 6, 25)));
            Assert.AreEqual(true, Holiday.IsHoliday(new DateTime(2002, 8, 5)));
            Assert.AreEqual(true, Holiday.IsHoliday(new DateTime(2002, 8, 15)));
            Assert.AreEqual(true, Holiday.IsHoliday(new DateTime(2002, 10, 8)));
            Assert.AreEqual(true, Holiday.IsHoliday(new DateTime(2002, 11, 1)));
            Assert.AreEqual(true, Holiday.IsHoliday(new DateTime(2002, 12, 25)));
            Assert.AreEqual(true, Holiday.IsHoliday(new DateTime(2002, 12, 26)));
            Assert.AreEqual(true, Holiday.IsHoliday(new DateTime(2002, 3, 31)));
            Assert.AreEqual(true, Holiday.IsHoliday(new DateTime(2002, 4, 1)));
            Assert.AreEqual(true, Holiday.IsHoliday(new DateTime(2002, 5, 30)));
        }

        [TestMethod()]
        public void Holiday_2012() {
            Assert.AreEqual(true, Holiday.IsHoliday(new DateTime(2012, 1, 1)));
            Assert.AreEqual(true, Holiday.IsHoliday(new DateTime(2012, 1, 6)));
            Assert.AreEqual(true, Holiday.IsHoliday(new DateTime(2012, 5, 1)));
            Assert.AreEqual(true, Holiday.IsHoliday(new DateTime(2012, 6, 22)));
            Assert.AreEqual(true, Holiday.IsHoliday(new DateTime(2012, 6, 25)));
            Assert.AreEqual(true, Holiday.IsHoliday(new DateTime(2012, 8, 5)));
            Assert.AreEqual(true, Holiday.IsHoliday(new DateTime(2012, 8, 15)));
            Assert.AreEqual(true, Holiday.IsHoliday(new DateTime(2012, 10, 8)));
            Assert.AreEqual(true, Holiday.IsHoliday(new DateTime(2012, 11, 1)));
            Assert.AreEqual(true, Holiday.IsHoliday(new DateTime(2012, 12, 25)));
            Assert.AreEqual(true, Holiday.IsHoliday(new DateTime(2012, 12, 26)));
            Assert.AreEqual(true, Holiday.IsHoliday(new DateTime(2012, 4, 8)));
            Assert.AreEqual(true, Holiday.IsHoliday(new DateTime(2012, 4, 9)));
            Assert.AreEqual(true, Holiday.IsHoliday(new DateTime(2012, 6, 7)));
        }

    }
}
