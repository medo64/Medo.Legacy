using System;
using Medo.Device;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test {

    [TestClass()]
    public class HermoTests {

        public TestContext TestContext { get; set; }


        [TestMethod()]
        public void Hermo_Parse_Empty() {
            var p = new PrivateType(typeof(Hermo));
            var x = (HermoReading)p.InvokeStatic("ParseLine", "");
            Assert.IsNull(x);
        }


        [TestMethod()]
        public void Hermo_Parse_0x10_17() {
            var p = new PrivateType(typeof(Hermo));
            var x = (HermoReading)p.InvokeStatic("ParseLine", "109A29130000003E09000705FFFF184EC4");

            Assert.AreEqual(0x3E00000013299A10, x.Code);
            Assert.AreEqual(0x2D299A10, x.Code32);
            Assert.AreEqual(0xB739, x.Code16);
            Assert.AreEqual(0x10, x.FamilyId);
            Assert.AreEqual(0x00000013299A, x.Serial);

            Assert.AreEqual(true, x.HasTemperature);
            Assert.AreEqual(4.25, x.Temperature);
            Assert.AreEqual(0.25, x.Resolution);
        }

        [TestMethod()]
        public void Hermo_Parse_0x10_17_CrcFailRomScratch() {
            var p = new PrivateType(typeof(Hermo));
            var x = (HermoReading)p.InvokeStatic("ParseLine", "109A29130000003D 09000705FFFF184EC3");

            Assert.IsNull(x);
        }

        [TestMethod()]
        public void Hermo_Parse_0x10_17_CrcFailRom() {
            var p = new PrivateType(typeof(Hermo));
            var x = (HermoReading)p.InvokeStatic("ParseLine", "109A29130000003F 09000705FFFF184EC4");

            Assert.IsNull(x);
        }

        [TestMethod()]
        public void Hermo_Parse_0x10_17_CrcFailScratch() {
            var p = new PrivateType(typeof(Hermo));
            var x = (HermoReading)p.InvokeStatic("ParseLine", "109A29130000003E 09000705FFFF184EC5");

            Assert.AreEqual(0x3E00000013299A10, x.Code);
            Assert.AreEqual(0x2D299A10, x.Code32);
            Assert.AreEqual(0xB739, x.Code16);
            Assert.AreEqual(0x10, x.FamilyId);
            Assert.AreEqual(0x00000013299A, x.Serial);

            Assert.AreEqual(false, x.HasTemperature);
        }

        [TestMethod()]
        public void Hermo_Parse_0x10_16() {
            var p = new PrivateType(typeof(Hermo));
            var x = (HermoReading)p.InvokeStatic("ParseLine", "109A29130000003E09000705FFFF184E");

            Assert.AreEqual(0x3E00000013299A10, x.Code);
            Assert.AreEqual(0x2D299A10, x.Code32);
            Assert.AreEqual(0xB739, x.Code16);
            Assert.AreEqual(0x10, x.FamilyId);
            Assert.AreEqual(0x00000013299A, x.Serial);

            Assert.AreEqual(false, x.HasTemperature);
        }

        [TestMethod()]
        public void Hermo_Parse_0x10_15() {
            var p = new PrivateType(typeof(Hermo));
            var x = (HermoReading)p.InvokeStatic("ParseLine", "109A29130000003E09000705FFFF18");

            Assert.AreEqual(0x3E00000013299A10, x.Code);
            Assert.AreEqual(0x2D299A10, x.Code32);
            Assert.AreEqual(0xB739, x.Code16);
            Assert.AreEqual(0x10, x.FamilyId);
            Assert.AreEqual(0x00000013299A, x.Serial);

            Assert.AreEqual(false, x.HasTemperature);
        }

        [TestMethod()]
        public void Hermo_Parse_0x10_12() {
            var p = new PrivateType(typeof(Hermo));
            var x = (HermoReading)p.InvokeStatic("ParseLine", "109A29130000003E09000705");

            Assert.AreEqual(0x3E00000013299A10, x.Code);
            Assert.AreEqual(0x2D299A10, x.Code32);
            Assert.AreEqual(0xB739, x.Code16);
            Assert.AreEqual(0x10, x.FamilyId);
            Assert.AreEqual(0x00000013299A, x.Serial);

            Assert.AreEqual(false, x.HasTemperature);
        }

        [TestMethod()]
        public void Hermo_Parse_0x10_11() {
            var p = new PrivateType(typeof(Hermo));
            var x = (HermoReading)p.InvokeStatic("ParseLine", "109A29130000003E090007");

            Assert.AreEqual(0x3E00000013299A10, x.Code);
            Assert.AreEqual(0x2D299A10, x.Code32);
            Assert.AreEqual(0xB739, x.Code16);
            Assert.AreEqual(0x10, x.FamilyId);
            Assert.AreEqual(0x00000013299A, x.Serial);

            Assert.AreEqual(false, x.HasTemperature);
        }

        [TestMethod()]
        public void Hermo_Parse_0x10_10() {
            var p = new PrivateType(typeof(Hermo));
            var x = (HermoReading)p.InvokeStatic("ParseLine", "109A29130000003E0900");

            Assert.AreEqual(0x3E00000013299A10, x.Code);
            Assert.AreEqual(0x2D299A10, x.Code32);
            Assert.AreEqual(0xB739, x.Code16);
            Assert.AreEqual(0x10, x.FamilyId);
            Assert.AreEqual(0x00000013299A, x.Serial);

            Assert.AreEqual(false, x.HasTemperature);
        }

        [TestMethod()]
        public void Hermo_Parse_0x10_09() {
            var p = new PrivateType(typeof(Hermo));
            var x = (HermoReading)p.InvokeStatic("ParseLine", "109A29130000003E09");

            Assert.AreEqual(0x3E00000013299A10, x.Code);
            Assert.AreEqual(0x2D299A10, x.Code32);
            Assert.AreEqual(0xB739, x.Code16);
            Assert.AreEqual(0x10, x.FamilyId);
            Assert.AreEqual(0x00000013299A, x.Serial);

            Assert.AreEqual(false, x.HasTemperature);
        }

        [TestMethod()]
        public void Hermo_Parse_0x10_08() {
            var p = new PrivateType(typeof(Hermo));
            var x = (HermoReading)p.InvokeStatic("ParseLine", "109A29130000003E");

            Assert.AreEqual(0x3E00000013299A10, x.Code);
            Assert.AreEqual(0x2D299A10, x.Code32);
            Assert.AreEqual(0xB739, x.Code16);
            Assert.AreEqual(0x10, x.FamilyId);
            Assert.AreEqual(0x00000013299A, x.Serial);

            Assert.AreEqual(false, x.HasTemperature);
        }

        [TestMethod()]
        public void Hermo_Parse_0x10_08_CrcFailRom() {
            var p = new PrivateType(typeof(Hermo));
            var x = (HermoReading)p.InvokeStatic("ParseLine", "109A29130000002E");

            Assert.IsNull(x);
        }

        [TestMethod()]
        public void Hermo_Parse_0x10_07() {
            var p = new PrivateType(typeof(Hermo));
            var x = (HermoReading)p.InvokeStatic("ParseLine", "109A2913000000");

            Assert.IsNull(x);
        }

        [TestMethod()]
        public void Hermo_Parse_0x10_06() {
            var p = new PrivateType(typeof(Hermo));
            var x = (HermoReading)p.InvokeStatic("ParseLine", "109A29130000");

            Assert.IsNull(x);
        }

        [TestMethod()]
        public void Hermo_Parse_01_01() {
            var p = new PrivateType(typeof(Hermo));
            var x = (HermoReading)p.InvokeStatic("ParseLine", "10");

            Assert.IsNull(x);
        }


        [TestMethod()]
        public void Hermo_Parse_0x28_17() {
            var p = new PrivateType(typeof(Hermo));
            var x = (HermoReading)p.InvokeStatic("ParseLine", "2824F76B000000872C014B467FFF041014");

            Assert.AreEqual(unchecked((Int64)0x870000006BF72428), x.Code);
            Assert.AreEqual(unchecked((Int32)0xECF72428), x.Code32);
            Assert.AreEqual(0xC8DF, x.Code16);
            Assert.AreEqual(0x28, x.FamilyId);
            Assert.AreEqual(0x0000006BF724, x.Serial);

            Assert.AreEqual(true, x.HasTemperature);
            Assert.AreEqual(18.75, x.Temperature);
            Assert.AreEqual(0.0625, x.Resolution);
        }

        [TestMethod()]
        public void Hermo_Parse_0x28_17_CrcFailRomScratch() {
            var p = new PrivateType(typeof(Hermo));
            var x = (HermoReading)p.InvokeStatic("ParseLine", "2824F76B00000086 2C014B467FFF041013");

            Assert.IsNull(x);
        }

        [TestMethod()]
        public void Hermo_Parse_0x28_17_CrcFailRom() {
            var p = new PrivateType(typeof(Hermo));
            var x = (HermoReading)p.InvokeStatic("ParseLine", "2824F76B00000088 2C014B467FFF041014");

            Assert.IsNull(x);
        }

        [TestMethod()]
        public void Hermo_Parse_0x28_17_CrcFailScratch() {
            var p = new PrivateType(typeof(Hermo));
            var x = (HermoReading)p.InvokeStatic("ParseLine", "2824F76B00000087 2C014B467FFF041015");

            Assert.AreEqual(unchecked((Int64)0x870000006BF72428), x.Code);
            Assert.AreEqual(unchecked((Int32)0xECF72428), x.Code32);
            Assert.AreEqual(0xC8DF, x.Code16);
            Assert.AreEqual(0x28, x.FamilyId);
            Assert.AreEqual(0x0000006BF724, x.Serial);

            Assert.AreEqual(false, x.HasTemperature);
        }

        [TestMethod()]
        public void Hermo_Parse_0x28_16() {
            var p = new PrivateType(typeof(Hermo));
            var x = (HermoReading)p.InvokeStatic("ParseLine", "2824F76B000000872C014B467FFF0410");

            Assert.AreEqual(unchecked((Int64)0x870000006BF72428), x.Code);
            Assert.AreEqual(unchecked((Int32)0xECF72428), x.Code32);
            Assert.AreEqual(0xC8DF, x.Code16);
            Assert.AreEqual(0x28, x.FamilyId);
            Assert.AreEqual(0x0000006BF724, x.Serial);

            Assert.AreEqual(false, x.HasTemperature);
        }

        [TestMethod()]
        public void Hermo_Parse_0x28_15() {
            var p = new PrivateType(typeof(Hermo));
            var x = (HermoReading)p.InvokeStatic("ParseLine", "2824F76B000000872C014B467FFF04");

            Assert.AreEqual(unchecked((Int64)0x870000006BF72428), x.Code);
            Assert.AreEqual(unchecked((Int32)0xECF72428), x.Code32);
            Assert.AreEqual(0xC8DF, x.Code16);
            Assert.AreEqual(0x28, x.FamilyId);
            Assert.AreEqual(0x0000006BF724, x.Serial);

            Assert.AreEqual(false, x.HasTemperature);
        }

        [TestMethod()]
        public void Hermo_Parse_0x28_13() {
            var p = new PrivateType(typeof(Hermo));
            var x = (HermoReading)p.InvokeStatic("ParseLine", "2824F76B000000872C014B467F");

            Assert.AreEqual(unchecked((Int64)0x870000006BF72428), x.Code);
            Assert.AreEqual(unchecked((Int32)0xECF72428), x.Code32);
            Assert.AreEqual(0xC8DF, x.Code16);
            Assert.AreEqual(0x28, x.FamilyId);
            Assert.AreEqual(0x0000006BF724, x.Serial);

            Assert.AreEqual(false, x.HasTemperature);
        }

        [TestMethod()]
        public void Hermo_Parse_0x28_12() {
            var p = new PrivateType(typeof(Hermo));
            var x = (HermoReading)p.InvokeStatic("ParseLine", "2824F76B000000872C014B46");

            Assert.AreEqual(unchecked((Int64)0x870000006BF72428), x.Code);
            Assert.AreEqual(unchecked((Int32)0xECF72428), x.Code32);
            Assert.AreEqual(0xC8DF, x.Code16);
            Assert.AreEqual(0x28, x.FamilyId);
            Assert.AreEqual(0x0000006BF724, x.Serial);

            Assert.AreEqual(false, x.HasTemperature);
        }

        [TestMethod()]
        public void Hermo_Parse_0x28_11() {
            var p = new PrivateType(typeof(Hermo));
            var x = (HermoReading)p.InvokeStatic("ParseLine", "2824F76B000000872C014B");

            Assert.AreEqual(unchecked((Int64)0x870000006BF72428), x.Code);
            Assert.AreEqual(unchecked((Int32)0xECF72428), x.Code32);
            Assert.AreEqual(0xC8DF, x.Code16);
            Assert.AreEqual(0x28, x.FamilyId);
            Assert.AreEqual(0x0000006BF724, x.Serial);

            Assert.AreEqual(false, x.HasTemperature);
        }

        [TestMethod()]
        public void Hermo_Parse_0x28_10() {
            var p = new PrivateType(typeof(Hermo));
            var x = (HermoReading)p.InvokeStatic("ParseLine", "2824F76B000000872C01");

            Assert.AreEqual(unchecked((Int64)0x870000006BF72428), x.Code);
            Assert.AreEqual(unchecked((Int32)0xECF72428), x.Code32);
            Assert.AreEqual(0xC8DF, x.Code16);
            Assert.AreEqual(0x28, x.FamilyId);
            Assert.AreEqual(0x0000006BF724, x.Serial);

            Assert.AreEqual(false, x.HasTemperature);
        }

        [TestMethod()]
        public void Hermo_Parse_0x28_09() {
            var p = new PrivateType(typeof(Hermo));
            var x = (HermoReading)p.InvokeStatic("ParseLine", "2824F76B000000872c");

            Assert.AreEqual(unchecked((Int64)0x870000006BF72428), x.Code);
            Assert.AreEqual(unchecked((Int32)0xECF72428), x.Code32);
            Assert.AreEqual(0xC8DF, x.Code16);
            Assert.AreEqual(0x28, x.FamilyId);
            Assert.AreEqual(0x0000006BF724, x.Serial);

            Assert.AreEqual(false, x.HasTemperature);
        }

        [TestMethod()]
        public void Hermo_Parse_0x28_08() {
            var p = new PrivateType(typeof(Hermo));
            var x = (HermoReading)p.InvokeStatic("ParseLine", "2824F76B00000087");

            Assert.AreEqual(unchecked((Int64)0x870000006BF72428), x.Code);
            Assert.AreEqual(unchecked((Int32)0xECF72428), x.Code32);
            Assert.AreEqual(0xC8DF, x.Code16);
            Assert.AreEqual(0x28, x.FamilyId);
            Assert.AreEqual(0x0000006BF724, x.Serial);

            Assert.AreEqual(false, x.HasTemperature);
        }

        [TestMethod()]
        public void Hermo_Parse_0x28_08_CrcFailRom() {
            var p = new PrivateType(typeof(Hermo));
            var x = (HermoReading)p.InvokeStatic("ParseLine", "2824F76B00000086");

            Assert.IsNull(x);
        }

        [TestMethod()]
        public void Hermo_Parse_0x28_07() {
            var p = new PrivateType(typeof(Hermo));
            var x = (HermoReading)p.InvokeStatic("ParseLine", "2824F76B000000");

            Assert.IsNull(x);
        }

        [TestMethod()]
        public void Hermo_Parse_0x28_06() {
            var p = new PrivateType(typeof(Hermo));
            var x = (HermoReading)p.InvokeStatic("ParseLine", "2824F76B0000");

            Assert.IsNull(x);
        }

        [TestMethod()]
        public void Hermo_Parse_0x28_01() {
            var p = new PrivateType(typeof(Hermo));
            var x = (HermoReading)p.InvokeStatic("ParseLine", "28");

            Assert.IsNull(x);
        }


        [TestMethod()]
        public void Hermo_Parse_01() {
            var p = new PrivateType(typeof(Hermo));
            var x = (HermoReading)p.InvokeStatic("ParseLine", "108D6A45020800AF3A004B46FFFF0B102F");

            Assert.AreEqual(unchecked((Int64)0xAF000802456A8D10), x.Code);
            Assert.AreEqual(unchecked((Int32)0xEA6A8512), x.Code32);
            Assert.AreEqual(0x6F78, x.Code16);
            Assert.AreEqual(0x10, x.FamilyId);
            Assert.AreEqual(0x000802456A8D, x.Serial);

            Assert.AreEqual(true, x.HasTemperature);
            Assert.AreEqual(28.75, x.Temperature);
            Assert.AreEqual(0.25, x.Resolution);
        }

        [TestMethod()]
        public void Hermo_Parse_02() {
            var p = new PrivateType(typeof(Hermo));
            var x = (HermoReading)p.InvokeStatic("ParseLine", "2846185F0300003DCA014B467FFF061065");

            Assert.AreEqual(0x3D0000035F184628, x.Code);
            Assert.AreEqual(0x6218462B, x.Code32);
            Assert.AreEqual(0x2433, x.Code16);
            Assert.AreEqual(0x28, x.FamilyId);
            Assert.AreEqual(0x0000035F1846, x.Serial);

            Assert.AreEqual(true, x.HasTemperature);
            Assert.AreEqual(28.625, x.Temperature);
            Assert.AreEqual(0.0625, x.Resolution);
        }

        [TestMethod()]
        public void Hermo_Parse_03() {
            var p = new PrivateType(typeof(Hermo));
            var x = (HermoReading)p.InvokeStatic("ParseLine", "10C8DD540108007C2E004B46FFFF0A10AA");

            Assert.AreEqual(0x7C00080154DDC810, x.Code);
            Assert.AreEqual(0x28DDC011, x.Code32);
            Assert.AreEqual(0xE8CC, x.Code16);
            Assert.AreEqual(0x10, x.FamilyId);
            Assert.AreEqual(0x00080154DDC8, x.Serial);

            Assert.AreEqual(true, x.HasTemperature);
            Assert.AreEqual(22.75, x.Temperature);
            Assert.AreEqual(0.25, x.Resolution);
        }

        [TestMethod()]
        public void Hermo_Parse_04() {
            var p = new PrivateType(typeof(Hermo));
            var x = (HermoReading)p.InvokeStatic("ParseLine", "10C8DD540108007C2F004B46FFFF081078");

            Assert.AreEqual(0x7C00080154DDC810, x.Code);
            Assert.AreEqual(0x28DDC011, x.Code32);
            Assert.AreEqual(0xE8CC, x.Code16);
            Assert.AreEqual(0x10, x.FamilyId);
            Assert.AreEqual(0x00080154DDC8, x.Serial);

            Assert.AreEqual(true, x.HasTemperature);
            Assert.AreEqual(23.25, x.Temperature);
            Assert.AreEqual(0.25, x.Resolution);
        }

        [TestMethod()]
        public void Hermo_Parse_05() {
            var p = new PrivateType(typeof(Hermo));
            var x = (HermoReading)p.InvokeStatic("ParseLine", "104C5F450208005139004B46FFFF0410F2");

            Assert.AreEqual(0x51000802455F4C10, x.Code);
            Assert.AreEqual(0x145F4412, x.Code32);
            Assert.AreEqual(0x504D, x.Code16);
            Assert.AreEqual(0x10, x.FamilyId);
            Assert.AreEqual(0x000802455F4C, x.Serial);

            Assert.AreEqual(true, x.HasTemperature);
            Assert.AreEqual(28.25, x.Temperature);
            Assert.AreEqual(0.25, x.Resolution);
        }

        [TestMethod()]
        public void Hermo_Parse_06() {
            var p = new PrivateType(typeof(Hermo));
            var x = (HermoReading)p.InvokeStatic("ParseLine", "108D742C0208007237004B46FFFF031025");

            Assert.AreEqual(0x720008022C748D10, x.Code);
            Assert.AreEqual(0x5E748512, x.Code32);
            Assert.AreEqual(0xDB66, x.Code16);
            Assert.AreEqual(0x10, x.FamilyId);
            Assert.AreEqual(0x0008022C748D, x.Serial);

            Assert.AreEqual(true, x.HasTemperature);
            Assert.AreEqual(27.25, x.Temperature);
            Assert.AreEqual(0.25, x.Resolution);
        }

        [TestMethod()]
        public void Hermo_Parse_07() {
            var p = new PrivateType(typeof(Hermo));
            var x = (HermoReading)p.InvokeStatic("ParseLine", "2846185F0300003DC6014B467FFF0A1017");

            Assert.AreEqual(0x3D0000035F184628, x.Code);
            Assert.AreEqual(0x6218462B, x.Code32);
            Assert.AreEqual(0x2433, x.Code16);
            Assert.AreEqual(0x28, x.FamilyId);
            Assert.AreEqual(0x0000035F1846, x.Serial);

            Assert.AreEqual(true, x.HasTemperature);
            Assert.AreEqual(28.375, x.Temperature);
            Assert.AreEqual(0.0625, x.Resolution);
        }

    }
}
