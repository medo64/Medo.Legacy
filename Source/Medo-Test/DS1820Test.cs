using System;
using Medo.Device;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test {

    [TestClass()]
    public class DS1820Test {

        public TestContext TestContext { get; set; }


        [TestMethod()]
        public void Test_DS1820_Parse_Empty() {
            var x = DS1820.Parse("");
            Assert.IsFalse(x.IsRomCodeValid);
            Assert.AreEqual(0, x.RomCode);
            Assert.AreEqual(0x00, x.Family);
            Assert.AreEqual(-1, x.Serial);
            Assert.IsFalse(x.IsScratchpadValid);
            Assert.AreEqual(double.NaN, x.Temperature);
            Assert.AreEqual(0x00, x.UserByte1);
            Assert.AreEqual(double.NaN, x.TemperatureHigh);
            Assert.AreEqual(0x00, x.UserByte2);
            Assert.AreEqual(double.NaN, x.TemperatureLow);
            Assert.AreEqual(0, x.Resolution);
        }


        [TestMethod()]
        public void Test_DS1820_Parse_01_17() {
            var x = DS1820.Parse("109A29130000003E09000705FFFF184EC4");
            Assert.IsTrue(x.IsRomCodeValid);
            Assert.AreEqual(0x3E00000013299A10, x.RomCode);
            Assert.AreEqual(0x10, x.Family);
            Assert.AreEqual(0x00000013299A, x.Serial);
            Assert.IsTrue(x.IsScratchpadValid);
            Assert.AreEqual(4.25, x.Temperature);
            Assert.AreEqual(0x07, x.UserByte1);
            Assert.AreEqual(7.0, x.TemperatureHigh);
            Assert.AreEqual(0x05, x.UserByte2);
            Assert.AreEqual(5.0, x.TemperatureLow);
            Assert.AreEqual(9, x.Resolution);
        }

        [TestMethod()]
        public void Test_DS1820_Parse_01_17_ReverseRomScratch() {
            var x = DS1820.Parse("3E00000013299A10 C44E18FFFF05070009");
            Assert.IsTrue(x.IsRomCodeValid);
            Assert.AreEqual(0x3E00000013299A10, x.RomCode);
            Assert.AreEqual(0x10, x.Family);
            Assert.AreEqual(0x00000013299A, x.Serial);
            Assert.IsTrue(x.IsScratchpadValid);
            Assert.AreEqual(4.25, x.Temperature);
            Assert.AreEqual(0x07, x.UserByte1);
            Assert.AreEqual(7.0, x.TemperatureHigh);
            Assert.AreEqual(0x05, x.UserByte2);
            Assert.AreEqual(5.0, x.TemperatureLow);
            Assert.AreEqual(9, x.Resolution);
        }

        [TestMethod()]
        public void Test_DS1820_Parse_01_ReverseRom() {
            var x = DS1820.Parse("3E00000013299A10 09000705FFFF184EC4");
            Assert.IsTrue(x.IsRomCodeValid);
            Assert.AreEqual(0x3E00000013299A10, x.RomCode);
            Assert.AreEqual(0x10, x.Family);
            Assert.AreEqual(0x00000013299A, x.Serial);
            Assert.IsTrue(x.IsScratchpadValid);
            Assert.AreEqual(4.25, x.Temperature);
            Assert.AreEqual(0x07, x.UserByte1);
            Assert.AreEqual(7.0, x.TemperatureHigh);
            Assert.AreEqual(0x05, x.UserByte2);
            Assert.AreEqual(5.0, x.TemperatureLow);
            Assert.AreEqual(9, x.Resolution);
        }

        [TestMethod()]
        public void Test_DS1820_Parse_01_17_ReverseScratch() {
            var x = DS1820.Parse("109A29130000003E C44E18FFFF05070009");
            Assert.IsTrue(x.IsRomCodeValid);
            Assert.AreEqual(0x3E00000013299A10, x.RomCode);
            Assert.AreEqual(0x10, x.Family);
            Assert.AreEqual(0x00000013299A, x.Serial);
            Assert.IsTrue(x.IsScratchpadValid);
            Assert.AreEqual(4.25, x.Temperature);
            Assert.AreEqual(0x07, x.UserByte1);
            Assert.AreEqual(7.0, x.TemperatureHigh);
            Assert.AreEqual(0x05, x.UserByte2);
            Assert.AreEqual(5.0, x.TemperatureLow);
            Assert.AreEqual(9, x.Resolution);
        }

        [TestMethod()]
        public void Test_DS1820_Parse_01_17_CrcFailRomScratch() {
            var x = DS1820.Parse("109A29130000003D 09000705FFFF184EC3");
            Assert.IsFalse(x.IsRomCodeValid);
            Assert.AreEqual(0, x.RomCode);
            Assert.AreEqual(0x10, x.Family);
            Assert.AreEqual(0x00000013299A, x.Serial);
            Assert.IsFalse(x.IsScratchpadValid);
            Assert.AreEqual(4.25, x.Temperature);
            Assert.AreEqual(0x07, x.UserByte1);
            Assert.AreEqual(7.0, x.TemperatureHigh);
            Assert.AreEqual(0x05, x.UserByte2);
            Assert.AreEqual(5.0, x.TemperatureLow);
            Assert.AreEqual(9, x.Resolution);
        }

        [TestMethod()]
        public void Test_DS1820_Parse_01_17_CrcFailRom() {
            var x = DS1820.Parse("109A29130000003F 09000705FFFF184EC4");
            Assert.IsFalse(x.IsRomCodeValid);
            Assert.AreEqual(0, x.RomCode);
            Assert.AreEqual(0x10, x.Family);
            Assert.AreEqual(0x00000013299A, x.Serial);
            Assert.IsTrue(x.IsScratchpadValid);
            Assert.AreEqual(4.25, x.Temperature);
            Assert.AreEqual(0x07, x.UserByte1);
            Assert.AreEqual(7.0, x.TemperatureHigh);
            Assert.AreEqual(0x05, x.UserByte2);
            Assert.AreEqual(5.0, x.TemperatureLow);
            Assert.AreEqual(9, x.Resolution);
        }

        [TestMethod()]
        public void Test_DS1820_Parse_01_17_CrcFailScratch() {
            var x = DS1820.Parse("109A29130000003E 09000705FFFF184EC5");
            Assert.IsTrue(x.IsRomCodeValid);
            Assert.AreEqual(0x3E00000013299A10, x.RomCode);
            Assert.AreEqual(0x10, x.Family);
            Assert.AreEqual(0x00000013299A, x.Serial);
            Assert.IsFalse(x.IsScratchpadValid);
            Assert.AreEqual(4.25, x.Temperature);
            Assert.AreEqual(0x07, x.UserByte1);
            Assert.AreEqual(7.0, x.TemperatureHigh);
            Assert.AreEqual(0x05, x.UserByte2);
            Assert.AreEqual(5.0, x.TemperatureLow);
            Assert.AreEqual(9, x.Resolution);
        }

        [TestMethod()]
        public void Test_DS1820_Parse_01_16() {
            var x = DS1820.Parse("109A29130000003E09000705FFFF184E");
            Assert.IsTrue(x.IsRomCodeValid);
            Assert.AreEqual(0x3E00000013299A10, x.RomCode);
            Assert.AreEqual(0x10, x.Family);
            Assert.AreEqual(0x00000013299A, x.Serial);
            Assert.IsFalse(x.IsScratchpadValid);
            Assert.AreEqual(4.25, x.Temperature);
            Assert.AreEqual(0x07, x.UserByte1);
            Assert.AreEqual(7.0, x.TemperatureHigh);
            Assert.AreEqual(0x05, x.UserByte2);
            Assert.AreEqual(5.0, x.TemperatureLow);
            Assert.AreEqual(9, x.Resolution);
        }

        [TestMethod()]
        public void Test_DS1820_Parse_01_15() {
            var x = DS1820.Parse("109A29130000003E09000705FFFF18");
            Assert.IsTrue(x.IsRomCodeValid);
            Assert.AreEqual(0x3E00000013299A10, x.RomCode);
            Assert.AreEqual(0x10, x.Family);
            Assert.AreEqual(0x00000013299A, x.Serial);
            Assert.IsFalse(x.IsScratchpadValid);
            Assert.AreEqual(4.5, x.Temperature);
            Assert.AreEqual(0x07, x.UserByte1);
            Assert.AreEqual(7.0, x.TemperatureHigh);
            Assert.AreEqual(0x05, x.UserByte2);
            Assert.AreEqual(5.0, x.TemperatureLow);
            Assert.AreEqual(9, x.Resolution);
        }

        [TestMethod()]
        public void Test_DS1820_Parse_01_12() {
            var x = DS1820.Parse("109A29130000003E09000705");
            Assert.IsTrue(x.IsRomCodeValid);
            Assert.AreEqual(0x3E00000013299A10, x.RomCode);
            Assert.AreEqual(0x10, x.Family);
            Assert.AreEqual(0x00000013299A, x.Serial);
            Assert.IsFalse(x.IsScratchpadValid);
            Assert.AreEqual(4.5, x.Temperature);
            Assert.AreEqual(0x07, x.UserByte1);
            Assert.AreEqual(7.0, x.TemperatureHigh);
            Assert.AreEqual(0x05, x.UserByte2);
            Assert.AreEqual(5.0, x.TemperatureLow);
            Assert.AreEqual(9, x.Resolution);
        }

        [TestMethod()]
        public void Test_DS1820_Parse_01_11() {
            var x = DS1820.Parse("109A29130000003E090007");
            Assert.IsTrue(x.IsRomCodeValid);
            Assert.AreEqual(0x3E00000013299A10, x.RomCode);
            Assert.AreEqual(0x10, x.Family);
            Assert.AreEqual(0x00000013299A, x.Serial);
            Assert.IsFalse(x.IsScratchpadValid);
            Assert.AreEqual(4.5, x.Temperature);
            Assert.AreEqual(0x07, x.UserByte1);
            Assert.AreEqual(7.0, x.TemperatureHigh);
            Assert.AreEqual(0x00, x.UserByte2);
            Assert.AreEqual(double.NaN, x.TemperatureLow);
            Assert.AreEqual(9, x.Resolution);
        }

        [TestMethod()]
        public void Test_DS1820_Parse_01_10() {
            var x = DS1820.Parse("109A29130000003E0900");
            Assert.IsTrue(x.IsRomCodeValid);
            Assert.AreEqual(0x3E00000013299A10, x.RomCode);
            Assert.AreEqual(0x10, x.Family);
            Assert.AreEqual(0x00000013299A, x.Serial);
            Assert.IsFalse(x.IsScratchpadValid);
            Assert.AreEqual(4.5, x.Temperature);
            Assert.AreEqual(0x00, x.UserByte1);
            Assert.AreEqual(double.NaN, x.TemperatureHigh);
            Assert.AreEqual(0x00, x.UserByte2);
            Assert.AreEqual(double.NaN, x.TemperatureLow);
            Assert.AreEqual(9, x.Resolution);
        }

        [TestMethod()]
        public void Test_DS1820_Parse_01_10_ReverseRom() {
            var x = DS1820.Parse("3E00000013299A10 0900");
            Assert.IsTrue(x.IsRomCodeValid);
            Assert.AreEqual(0x3E00000013299A10, x.RomCode);
            Assert.AreEqual(0x10, x.Family);
            Assert.AreEqual(0x00000013299A, x.Serial);
            Assert.IsFalse(x.IsScratchpadValid);
            Assert.AreEqual(4.5, x.Temperature);
            Assert.AreEqual(0x00, x.UserByte1);
            Assert.AreEqual(double.NaN, x.TemperatureHigh);
            Assert.AreEqual(0x00, x.UserByte2);
            Assert.AreEqual(double.NaN, x.TemperatureLow);
            Assert.AreEqual(9, x.Resolution);
        }

        [TestMethod()]
        public void Test_DS1820_Parse_01_09() {
            var x = DS1820.Parse("109A29130000003E09");
            Assert.IsTrue(x.IsRomCodeValid);
            Assert.AreEqual(0x3E00000013299A10, x.RomCode);
            Assert.AreEqual(0x10, x.Family);
            Assert.AreEqual(0x00000013299A, x.Serial);
            Assert.IsFalse(x.IsScratchpadValid);
            Assert.AreEqual(double.NaN, x.Temperature);
            Assert.AreEqual(0x00, x.UserByte1);
            Assert.AreEqual(double.NaN, x.TemperatureHigh);
            Assert.AreEqual(0x00, x.UserByte2);
            Assert.AreEqual(double.NaN, x.TemperatureLow);
            Assert.AreEqual(0, x.Resolution);
        }

        [TestMethod()]
        public void Test_DS1820_Parse_01_08() {
            var x = DS1820.Parse("109A29130000003E");
            Assert.IsTrue(x.IsRomCodeValid);
            Assert.AreEqual(0x3E00000013299A10, x.RomCode);
            Assert.AreEqual(0x10, x.Family);
            Assert.AreEqual(0x00000013299A, x.Serial);
            Assert.IsFalse(x.IsScratchpadValid);
            Assert.AreEqual(double.NaN, x.Temperature);
            Assert.AreEqual(0x00, x.UserByte1);
            Assert.AreEqual(double.NaN, x.TemperatureHigh);
            Assert.AreEqual(0x00, x.UserByte2);
            Assert.AreEqual(double.NaN, x.TemperatureLow);
            Assert.AreEqual(0, x.Resolution);
        }

        [TestMethod()]
        public void Test_DS1820_Parse_01_08_ReverseRom() {
            var x = DS1820.Parse("3E00000013299A10 ");
            Assert.IsTrue(x.IsRomCodeValid);
            Assert.AreEqual(0x3E00000013299A10, x.RomCode);
            Assert.AreEqual(0x10, x.Family);
            Assert.AreEqual(0x00000013299A, x.Serial);
            Assert.IsFalse(x.IsScratchpadValid);
            Assert.AreEqual(double.NaN, x.Temperature);
            Assert.AreEqual(0x00, x.UserByte1);
            Assert.AreEqual(double.NaN, x.TemperatureHigh);
            Assert.AreEqual(0x00, x.UserByte2);
            Assert.AreEqual(double.NaN, x.TemperatureLow);
            Assert.AreEqual(0, x.Resolution);
        }

        [TestMethod()]
        public void Test_DS1820_Parse_01_08_CrcFailRom() {
            var x = DS1820.Parse("2E00000013299A10 ");
            Assert.IsFalse(x.IsRomCodeValid);
            Assert.AreEqual(0, x.RomCode);
            Assert.AreEqual(0x10, x.Family);
            Assert.AreEqual(0x00000013299A, x.Serial);
            Assert.IsFalse(x.IsScratchpadValid);
            Assert.AreEqual(double.NaN, x.Temperature);
            Assert.AreEqual(0x00, x.UserByte1);
            Assert.AreEqual(double.NaN, x.TemperatureHigh);
            Assert.AreEqual(0x00, x.UserByte2);
            Assert.AreEqual(double.NaN, x.TemperatureLow);
            Assert.AreEqual(0, x.Resolution);
        }

        [TestMethod()]
        public void Test_DS1820_Parse_01_07() {
            var x = DS1820.Parse("109A2913000000");
            Assert.IsFalse(x.IsRomCodeValid);
            Assert.AreEqual(0, x.RomCode);
            Assert.AreEqual(0x10, x.Family);
            Assert.AreEqual(0x00000013299A, x.Serial);
            Assert.IsFalse(x.IsScratchpadValid);
            Assert.AreEqual(double.NaN, x.Temperature);
            Assert.AreEqual(0x00, x.UserByte1);
            Assert.AreEqual(double.NaN, x.TemperatureHigh);
            Assert.AreEqual(0x00, x.UserByte2);
            Assert.AreEqual(double.NaN, x.TemperatureLow);
            Assert.AreEqual(0, x.Resolution);
        }

        [TestMethod()]
        public void Test_DS1820_Parse_01_06() {
            var x = DS1820.Parse("109A29130000");
            Assert.IsFalse(x.IsRomCodeValid);
            Assert.AreEqual(0, x.RomCode);
            Assert.AreEqual(0x10, x.Family);
            Assert.AreEqual(-1, x.Serial);
            Assert.IsFalse(x.IsScratchpadValid);
            Assert.AreEqual(double.NaN, x.Temperature);
            Assert.AreEqual(0x00, x.UserByte1);
            Assert.AreEqual(double.NaN, x.TemperatureHigh);
            Assert.AreEqual(0x00, x.UserByte2);
            Assert.AreEqual(double.NaN, x.TemperatureLow);
            Assert.AreEqual(0, x.Resolution);
        }

        [TestMethod()]
        public void Test_DS1820_Parse_01_01() {
            var x = DS1820.Parse("10");
            Assert.IsFalse(x.IsRomCodeValid);
            Assert.AreEqual(0, x.RomCode);
            Assert.AreEqual(0x10, x.Family);
            Assert.AreEqual(-1, x.Serial);
            Assert.IsFalse(x.IsScratchpadValid);
            Assert.AreEqual(double.NaN, x.Temperature);
            Assert.AreEqual(0x00, x.UserByte1);
            Assert.AreEqual(double.NaN, x.TemperatureHigh);
            Assert.AreEqual(0x00, x.UserByte2);
            Assert.AreEqual(double.NaN, x.TemperatureLow);
            Assert.AreEqual(0, x.Resolution);
        }


        [TestMethod()]
        public void Test_DS1820_Parse_02_17() {
            var x = DS1820.Parse("2824F76B000000872C014B467FFF041014");
            Assert.IsTrue(x.IsRomCodeValid);
            Assert.AreEqual(unchecked((Int64)0x870000006BF72428), x.RomCode);
            Assert.AreEqual(0x28, x.Family);
            Assert.AreEqual(0x0000006BF724, x.Serial);
            Assert.IsTrue(x.IsScratchpadValid);
            Assert.AreEqual(18.75, x.Temperature);
            Assert.AreEqual(0x4B, x.UserByte1);
            Assert.AreEqual(75, x.TemperatureHigh);
            Assert.AreEqual(0x46, x.UserByte2);
            Assert.AreEqual(70, x.TemperatureLow);
            Assert.AreEqual(12, x.Resolution);
        }

        [TestMethod()]
        public void Test_DS1820_Parse_02_17_ReverseRomScratch() {
            var x = DS1820.Parse("870000006BF72428 141004FF7F464B012C");
            Assert.IsTrue(x.IsRomCodeValid);
            Assert.AreEqual(unchecked((Int64)0x870000006BF72428), x.RomCode);
            Assert.AreEqual(0x28, x.Family);
            Assert.AreEqual(0x0000006BF724, x.Serial);
            Assert.IsTrue(x.IsScratchpadValid);
            Assert.AreEqual(18.75, x.Temperature);
            Assert.AreEqual(0x4B, x.UserByte1);
            Assert.AreEqual(75, x.TemperatureHigh);
            Assert.AreEqual(0x46, x.UserByte2);
            Assert.AreEqual(70, x.TemperatureLow);
            Assert.AreEqual(12, x.Resolution);
        }

        [TestMethod()]
        public void Test_DS1820_Parse_02_17_ReverseRom() {
            var x = DS1820.Parse("870000006BF72428 2C014B467FFF041014");
            Assert.IsTrue(x.IsRomCodeValid);
            Assert.AreEqual(unchecked((Int64)0x870000006BF72428), x.RomCode);
            Assert.AreEqual(0x28, x.Family);
            Assert.AreEqual(0x0000006BF724, x.Serial);
            Assert.IsTrue(x.IsScratchpadValid);
            Assert.AreEqual(18.75, x.Temperature);
            Assert.AreEqual(0x4B, x.UserByte1);
            Assert.AreEqual(75, x.TemperatureHigh);
            Assert.AreEqual(0x46, x.UserByte2);
            Assert.AreEqual(70, x.TemperatureLow);
            Assert.AreEqual(12, x.Resolution);
        }

        [TestMethod()]
        public void Test_DS1820_Parse_02_17_ReverseScratch() {
            var x = DS1820.Parse("2824F76B00000087 141004FF7F464B012C");
            Assert.IsTrue(x.IsRomCodeValid);
            Assert.AreEqual(unchecked((Int64)0x870000006BF72428), x.RomCode);
            Assert.AreEqual(0x28, x.Family);
            Assert.AreEqual(0x0000006BF724, x.Serial);
            Assert.IsTrue(x.IsScratchpadValid);
            Assert.AreEqual(18.75, x.Temperature);
            Assert.AreEqual(0x4B, x.UserByte1);
            Assert.AreEqual(75, x.TemperatureHigh);
            Assert.AreEqual(0x46, x.UserByte2);
            Assert.AreEqual(70, x.TemperatureLow);
            Assert.AreEqual(12, x.Resolution);
        }

        [TestMethod()]
        public void Test_DS1820_Parse_02_17_CrcFailRomScratch() {
            var x = DS1820.Parse("2824F76B00000086 2C014B467FFF041013");
            Assert.IsFalse(x.IsRomCodeValid);
            Assert.AreEqual(0, x.RomCode);
            Assert.AreEqual(0x28, x.Family);
            Assert.AreEqual(0x0000006BF724, x.Serial);
            Assert.IsFalse(x.IsScratchpadValid);
            Assert.AreEqual(18.75, x.Temperature);
            Assert.AreEqual(0x4B, x.UserByte1);
            Assert.AreEqual(75, x.TemperatureHigh);
            Assert.AreEqual(0x46, x.UserByte2);
            Assert.AreEqual(70, x.TemperatureLow);
            Assert.AreEqual(12, x.Resolution);
        }

        [TestMethod()]
        public void Test_DS1820_Parse_02_17_CrcFailRom() {
            var x = DS1820.Parse("2824F76B00000088 2C014B467FFF041014");
            Assert.IsFalse(x.IsRomCodeValid);
            Assert.AreEqual(0, x.RomCode);
            Assert.AreEqual(0x28, x.Family);
            Assert.AreEqual(0x0000006BF724, x.Serial);
            Assert.IsTrue(x.IsScratchpadValid);
            Assert.AreEqual(18.75, x.Temperature);
            Assert.AreEqual(0x4B, x.UserByte1);
            Assert.AreEqual(75, x.TemperatureHigh);
            Assert.AreEqual(0x46, x.UserByte2);
            Assert.AreEqual(70, x.TemperatureLow);
            Assert.AreEqual(12, x.Resolution);
        }

        [TestMethod()]
        public void Test_DS1820_Parse_02_17_CrcFailScratch() {
            var x = DS1820.Parse("2824F76B00000087 2C014B467FFF041015");
            Assert.IsTrue(x.IsRomCodeValid);
            Assert.AreEqual(unchecked((Int64)0x870000006BF72428), x.RomCode);
            Assert.AreEqual(0x28, x.Family);
            Assert.AreEqual(0x0000006BF724, x.Serial);
            Assert.IsFalse(x.IsScratchpadValid);
            Assert.AreEqual(18.75, x.Temperature);
            Assert.AreEqual(0x4B, x.UserByte1);
            Assert.AreEqual(75, x.TemperatureHigh);
            Assert.AreEqual(0x46, x.UserByte2);
            Assert.AreEqual(70, x.TemperatureLow);
            Assert.AreEqual(12, x.Resolution);
        }

        [TestMethod()]
        public void Test_DS1820_Parse_02_16() {
            var x = DS1820.Parse("2824F76B000000872C014B467FFF0410");
            Assert.IsTrue(x.IsRomCodeValid);
            Assert.AreEqual(unchecked((Int64)0x870000006BF72428), x.RomCode);
            Assert.AreEqual(0x28, x.Family);
            Assert.AreEqual(0x0000006BF724, x.Serial);
            Assert.IsFalse(x.IsScratchpadValid);
            Assert.AreEqual(18.75, x.Temperature);
            Assert.AreEqual(0x4B, x.UserByte1);
            Assert.AreEqual(75, x.TemperatureHigh);
            Assert.AreEqual(0x46, x.UserByte2);
            Assert.AreEqual(70, x.TemperatureLow);
            Assert.AreEqual(12, x.Resolution);
        }

        [TestMethod()]
        public void Test_DS1820_Parse_02_15() {
            var x = DS1820.Parse("2824F76B000000872C014B467FFF04");
            Assert.IsTrue(x.IsRomCodeValid);
            Assert.AreEqual(unchecked((Int64)0x870000006BF72428), x.RomCode);
            Assert.AreEqual(0x28, x.Family);
            Assert.AreEqual(0x0000006BF724, x.Serial);
            Assert.IsFalse(x.IsScratchpadValid);
            Assert.AreEqual(18.75, x.Temperature);
            Assert.AreEqual(0x4B, x.UserByte1);
            Assert.AreEqual(75, x.TemperatureHigh);
            Assert.AreEqual(0x46, x.UserByte2);
            Assert.AreEqual(70, x.TemperatureLow);
            Assert.AreEqual(12, x.Resolution);
        }

        [TestMethod()]
        public void Test_DS1820_Parse_02_13() {
            var x = DS1820.Parse("2824F76B000000872C014B467F");
            Assert.IsTrue(x.IsRomCodeValid);
            Assert.AreEqual(unchecked((Int64)0x870000006BF72428), x.RomCode);
            Assert.AreEqual(0x28, x.Family);
            Assert.AreEqual(0x0000006BF724, x.Serial);
            Assert.IsFalse(x.IsScratchpadValid);
            Assert.AreEqual(18.75, x.Temperature);
            Assert.AreEqual(0x4B, x.UserByte1);
            Assert.AreEqual(75, x.TemperatureHigh);
            Assert.AreEqual(0x46, x.UserByte2);
            Assert.AreEqual(70, x.TemperatureLow);
            Assert.AreEqual(12, x.Resolution);
        }

        [TestMethod()]
        public void Test_DS1820_Parse_02_12() {
            var x = DS1820.Parse("2824F76B000000872C014B46");
            Assert.IsTrue(x.IsRomCodeValid);
            Assert.AreEqual(unchecked((Int64)0x870000006BF72428), x.RomCode);
            Assert.AreEqual(0x28, x.Family);
            Assert.AreEqual(0x0000006BF724, x.Serial);
            Assert.IsFalse(x.IsScratchpadValid);
            Assert.AreEqual(18.75, x.Temperature);
            Assert.AreEqual(0x4B, x.UserByte1);
            Assert.AreEqual(75, x.TemperatureHigh);
            Assert.AreEqual(0x46, x.UserByte2);
            Assert.AreEqual(70, x.TemperatureLow);
            Assert.AreEqual(0, x.Resolution);
        }

        [TestMethod()]
        public void Test_DS1820_Parse_02_11() {
            var x = DS1820.Parse("2824F76B000000872C014B");
            Assert.IsTrue(x.IsRomCodeValid);
            Assert.AreEqual(unchecked((Int64)0x870000006BF72428), x.RomCode);
            Assert.AreEqual(0x28, x.Family);
            Assert.AreEqual(0x0000006BF724, x.Serial);
            Assert.IsFalse(x.IsScratchpadValid);
            Assert.AreEqual(18.75, x.Temperature);
            Assert.AreEqual(0x4B, x.UserByte1);
            Assert.AreEqual(75, x.TemperatureHigh);
            Assert.AreEqual(0x00, x.UserByte2);
            Assert.AreEqual(double.NaN, x.TemperatureLow);
            Assert.AreEqual(0, x.Resolution);
        }

        [TestMethod()]
        public void Test_DS1820_Parse_02_10() {
            var x = DS1820.Parse("2824F76B000000872C01");
            Assert.IsTrue(x.IsRomCodeValid);
            Assert.AreEqual(unchecked((Int64)0x870000006BF72428), x.RomCode);
            Assert.AreEqual(0x28, x.Family);
            Assert.AreEqual(0x0000006BF724, x.Serial);
            Assert.IsFalse(x.IsScratchpadValid);
            Assert.AreEqual(18.75, x.Temperature);
            Assert.AreEqual(0x00, x.UserByte1);
            Assert.AreEqual(double.NaN, x.TemperatureHigh);
            Assert.AreEqual(0x00, x.UserByte2);
            Assert.AreEqual(double.NaN, x.TemperatureLow);
            Assert.AreEqual(0, x.Resolution);
        }

        [TestMethod()]
        public void Test_DS1820_Parse_02_10_ReverseRom() {
            var x = DS1820.Parse("870000006BF72428 2C01");
            Assert.IsTrue(x.IsRomCodeValid);
            Assert.AreEqual(unchecked((Int64)0x870000006BF72428), x.RomCode);
            Assert.AreEqual(0x28, x.Family);
            Assert.AreEqual(0x0000006BF724, x.Serial);
            Assert.IsFalse(x.IsScratchpadValid);
            Assert.AreEqual(18.75, x.Temperature);
            Assert.AreEqual(0x00, x.UserByte1);
            Assert.AreEqual(double.NaN, x.TemperatureHigh);
            Assert.AreEqual(0x00, x.UserByte2);
            Assert.AreEqual(double.NaN, x.TemperatureLow);
            Assert.AreEqual(0, x.Resolution);
        }

        [TestMethod()]
        public void Test_DS1820_Parse_02_09() {
            var x = DS1820.Parse("2824F76B000000872C");
            Assert.IsTrue(x.IsRomCodeValid);
            Assert.AreEqual(unchecked((Int64)0x870000006BF72428), x.RomCode);
            Assert.AreEqual(0x28, x.Family);
            Assert.AreEqual(0x0000006BF724, x.Serial);
            Assert.IsFalse(x.IsScratchpadValid);
            Assert.AreEqual(double.NaN, x.Temperature);
            Assert.AreEqual(0x00, x.UserByte1);
            Assert.AreEqual(double.NaN, x.TemperatureHigh);
            Assert.AreEqual(0x00, x.UserByte2);
            Assert.AreEqual(double.NaN, x.TemperatureLow);
            Assert.AreEqual(0, x.Resolution);
        }

        [TestMethod()]
        public void Test_DS1820_Parse_02_08() {
            var x = DS1820.Parse("2824F76B00000087");
            Assert.IsTrue(x.IsRomCodeValid);
            Assert.AreEqual(unchecked((Int64)0x870000006BF72428), x.RomCode);
            Assert.AreEqual(0x28, x.Family);
            Assert.AreEqual(0x0000006BF724, x.Serial);
            Assert.IsFalse(x.IsScratchpadValid);
            Assert.AreEqual(double.NaN, x.Temperature);
            Assert.AreEqual(0x00, x.UserByte1);
            Assert.AreEqual(double.NaN, x.TemperatureHigh);
            Assert.AreEqual(0x00, x.UserByte2);
            Assert.AreEqual(double.NaN, x.TemperatureLow);
            Assert.AreEqual(0, x.Resolution);
        }

        [TestMethod()]
        public void Test_DS1820_Parse_02_08_ReverseRom() {
            var x = DS1820.Parse("870000006BF72428 ");
            Assert.IsTrue(x.IsRomCodeValid);
            Assert.AreEqual(unchecked((Int64)0x870000006BF72428), x.RomCode);
            Assert.AreEqual(0x28, x.Family);
            Assert.AreEqual(0x0000006BF724, x.Serial);
            Assert.IsFalse(x.IsScratchpadValid);
            Assert.AreEqual(double.NaN, x.Temperature);
            Assert.AreEqual(0x00, x.UserByte1);
            Assert.AreEqual(double.NaN, x.TemperatureHigh);
            Assert.AreEqual(0x00, x.UserByte2);
            Assert.AreEqual(double.NaN, x.TemperatureLow);
            Assert.AreEqual(0, x.Resolution);
        }

        [TestMethod()]
        public void Test_DS1820_Parse_02_08_CrcFailRom() {
            var x = DS1820.Parse("970000006BF72428 ");
            Assert.IsFalse(x.IsRomCodeValid);
            Assert.AreEqual(0, x.RomCode);
            Assert.AreEqual(0x28, x.Family);
            Assert.AreEqual(0x0000006BF724, x.Serial);
            Assert.IsFalse(x.IsScratchpadValid);
            Assert.AreEqual(double.NaN, x.Temperature);
            Assert.AreEqual(0x00, x.UserByte1);
            Assert.AreEqual(double.NaN, x.TemperatureHigh);
            Assert.AreEqual(0x00, x.UserByte2);
            Assert.AreEqual(double.NaN, x.TemperatureLow);
            Assert.AreEqual(0, x.Resolution);
        }

        [TestMethod()]
        public void Test_DS1820_Parse_02_07() {
            var x = DS1820.Parse("2824F76B000000");
            Assert.IsFalse(x.IsRomCodeValid);
            Assert.AreEqual(0, x.RomCode);
            Assert.AreEqual(0x28, x.Family);
            Assert.AreEqual(0x0000006BF724, x.Serial);
            Assert.IsFalse(x.IsScratchpadValid);
            Assert.AreEqual(double.NaN, x.Temperature);
            Assert.AreEqual(0x00, x.UserByte1);
            Assert.AreEqual(double.NaN, x.TemperatureHigh);
            Assert.AreEqual(0x00, x.UserByte2);
            Assert.AreEqual(double.NaN, x.TemperatureLow);
            Assert.AreEqual(0, x.Resolution);
        }

        [TestMethod()]
        public void Test_DS1820_Parse_02_06() {
            var x = DS1820.Parse("2824F76B0000");
            Assert.IsFalse(x.IsRomCodeValid);
            Assert.AreEqual(0, x.RomCode);
            Assert.AreEqual(0x28, x.Family);
            Assert.AreEqual(-1, x.Serial);
            Assert.IsFalse(x.IsScratchpadValid);
            Assert.AreEqual(double.NaN, x.Temperature);
            Assert.AreEqual(0x00, x.UserByte1);
            Assert.AreEqual(double.NaN, x.TemperatureHigh);
            Assert.AreEqual(0x00, x.UserByte2);
            Assert.AreEqual(double.NaN, x.TemperatureLow);
            Assert.AreEqual(0, x.Resolution);
        }

        [TestMethod()]
        public void Test_DS1820_Parse_02_01() {
            var x = DS1820.Parse("28");
            Assert.IsFalse(x.IsRomCodeValid);
            Assert.AreEqual(0, x.RomCode);
            Assert.AreEqual(0x28, x.Family);
            Assert.AreEqual(-1, x.Serial);
            Assert.IsFalse(x.IsScratchpadValid);
            Assert.AreEqual(double.NaN, x.Temperature);
            Assert.AreEqual(0x00, x.UserByte1);
            Assert.AreEqual(double.NaN, x.TemperatureHigh);
            Assert.AreEqual(0x00, x.UserByte2);
            Assert.AreEqual(double.NaN, x.TemperatureLow);
            Assert.AreEqual(0, x.Resolution);
        }


        [TestMethod()]
        public void Test_DS1820_Parse_03() {
            var x = DS1820.Parse("108D6A45020800AF3A004B46FFFF0B102F");
            Assert.IsTrue(x.IsRomCodeValid);
            Assert.AreEqual(unchecked((Int64)0xAF000802456A8D10), x.RomCode);
            Assert.AreEqual(0x10, x.Family);
            Assert.AreEqual(0x000802456A8D, x.Serial);
            Assert.IsTrue(x.IsScratchpadValid);
            Assert.AreEqual(28.75, x.Temperature);
            Assert.AreEqual(0x4B, x.UserByte1);
            Assert.AreEqual(75, x.TemperatureHigh);
            Assert.AreEqual(0x46, x.UserByte2);
            Assert.AreEqual(70, x.TemperatureLow);
            Assert.AreEqual(9, x.Resolution);
        }

        [TestMethod()]
        public void Test_DS1820_Parse_04() {
            var x = DS1820.Parse("2846185F0300003DCA014B467FFF061065");
            Assert.IsTrue(x.IsRomCodeValid);
            Assert.AreEqual(unchecked((Int64)0x3D0000035F184628), x.RomCode);
            Assert.AreEqual(0x28, x.Family);
            Assert.AreEqual(0x0000035F1846, x.Serial);
            Assert.IsTrue(x.IsScratchpadValid);
            Assert.AreEqual(28.625, x.Temperature);
            Assert.AreEqual(0x4B, x.UserByte1);
            Assert.AreEqual(75, x.TemperatureHigh);
            Assert.AreEqual(0x46, x.UserByte2);
            Assert.AreEqual(70, x.TemperatureLow);
            Assert.AreEqual(12, x.Resolution);
        }

    }
}
