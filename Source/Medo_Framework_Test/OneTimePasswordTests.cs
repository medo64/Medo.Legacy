using Medo.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;

namespace OneTimePasswordSample.Test {

    [TestClass]
    public class OneTimePasswordTests {

        #region New

        [TestMethod]
        public void OneTimePassword_New_1() {
            var o1 = new OneTimePassword();
            var o2 = new OneTimePassword();

            Assert.AreEqual(20, o1.GetSecret().Length);
            Assert.AreNotEqual(BitConverter.ToString(o1.GetSecret()), BitConverter.ToString(o2.GetSecret()));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void OneTimePassword_New_2() {
            var o = new OneTimePassword("@");
        }

        [TestMethod]
        public void OneTimePassword_New_3() {
            var o = new OneTimePassword("MZx w6\tyT   bo I=");
            Assert.AreEqual("mzxw6ytboi", o.GetBase32Secret(SecretFormatFlags.None));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void OneTimePassword_New_4() {
            var o = new OneTimePassword(null as string);
        }

        #endregion New


        #region Base32

        [TestMethod]
        public void OneTimePassword_Secret_Base32_0() {
            var o = new OneTimePassword("");

            Assert.AreEqual("", BitConverter.ToString(o.GetSecret()));
            Assert.AreEqual("", o.GetBase32Secret());
            Assert.AreEqual("", o.GetBase32Secret(SecretFormatFlags.None));
            Assert.AreEqual("", o.GetBase32Secret(SecretFormatFlags.Spacing));
            Assert.AreEqual("", o.GetBase32Secret(SecretFormatFlags.Padding));
            Assert.AreEqual("", o.GetBase32Secret(SecretFormatFlags.Spacing | SecretFormatFlags.Padding));
        }

        [TestMethod]
        public void OneTimePassword_Secret_Base32_1() {
            var o = new OneTimePassword("m");

            Assert.AreEqual("60", BitConverter.ToString(o.GetSecret()));
            Assert.AreEqual("ma", o.GetBase32Secret());
            Assert.AreEqual("ma", o.GetBase32Secret(SecretFormatFlags.None));
            Assert.AreEqual("ma", o.GetBase32Secret(SecretFormatFlags.Spacing));
            Assert.AreEqual("ma======", o.GetBase32Secret(SecretFormatFlags.Padding));
            Assert.AreEqual("ma== ====", o.GetBase32Secret(SecretFormatFlags.Spacing | SecretFormatFlags.Padding));
        }

        [TestMethod]
        public void OneTimePassword_Secret_Base32_2() {
            var o = new OneTimePassword("my");

            Assert.AreEqual("66", BitConverter.ToString(o.GetSecret()));
            Assert.AreEqual("my", o.GetBase32Secret());
            Assert.AreEqual("my", o.GetBase32Secret(SecretFormatFlags.None));
            Assert.AreEqual("my", o.GetBase32Secret(SecretFormatFlags.Spacing));
            Assert.AreEqual("my======", o.GetBase32Secret(SecretFormatFlags.Padding));
            Assert.AreEqual("my== ====", o.GetBase32Secret(SecretFormatFlags.Spacing | SecretFormatFlags.Padding));
        }

        [TestMethod]
        public void OneTimePassword_Secret_Base32_2p() {
            var o = new OneTimePassword("MY======");

            Assert.AreEqual("66", BitConverter.ToString(o.GetSecret()));
            Assert.AreEqual("my", o.GetBase32Secret());
            Assert.AreEqual("my", o.GetBase32Secret(SecretFormatFlags.None));
            Assert.AreEqual("my", o.GetBase32Secret(SecretFormatFlags.Spacing));
            Assert.AreEqual("my======", o.GetBase32Secret(SecretFormatFlags.Padding));
            Assert.AreEqual("my== ====", o.GetBase32Secret(SecretFormatFlags.Spacing | SecretFormatFlags.Padding));
        }

        [TestMethod]
        public void OneTimePassword_Secret_Base32_3() {
            var o = new OneTimePassword("mzx");

            Assert.AreEqual("66-6E", BitConverter.ToString(o.GetSecret()));
            Assert.AreEqual("mzxa", o.GetBase32Secret());
            Assert.AreEqual("mzxa", o.GetBase32Secret(SecretFormatFlags.None));
            Assert.AreEqual("mzxa", o.GetBase32Secret(SecretFormatFlags.Spacing));
            Assert.AreEqual("mzxa====", o.GetBase32Secret(SecretFormatFlags.Padding));
            Assert.AreEqual("mzxa ====", o.GetBase32Secret(SecretFormatFlags.Spacing | SecretFormatFlags.Padding));
        }

        [TestMethod]
        public void OneTimePassword_Secret_Base32_4() {
            var o = new OneTimePassword("mzxq");

            Assert.AreEqual("66-6F", BitConverter.ToString(o.GetSecret()));
            Assert.AreEqual("mzxq", o.GetBase32Secret());
            Assert.AreEqual("mzxq", o.GetBase32Secret(SecretFormatFlags.None));
            Assert.AreEqual("mzxq", o.GetBase32Secret(SecretFormatFlags.Spacing));
            Assert.AreEqual("mzxq====", o.GetBase32Secret(SecretFormatFlags.Padding));
            Assert.AreEqual("mzxq ====", o.GetBase32Secret(SecretFormatFlags.Spacing | SecretFormatFlags.Padding));
        }

        [TestMethod]
        public void OneTimePassword_Secret_Base32_4p() {
            var o = new OneTimePassword("MZXQ====");

            Assert.AreEqual("66-6F", BitConverter.ToString(o.GetSecret()));
            Assert.AreEqual("mzxq", o.GetBase32Secret());
            Assert.AreEqual("mzxq", o.GetBase32Secret(SecretFormatFlags.None));
            Assert.AreEqual("mzxq", o.GetBase32Secret(SecretFormatFlags.Spacing));
            Assert.AreEqual("mzxq====", o.GetBase32Secret(SecretFormatFlags.Padding));
            Assert.AreEqual("mzxq ====", o.GetBase32Secret(SecretFormatFlags.Spacing | SecretFormatFlags.Padding));
        }

        [TestMethod]
        public void OneTimePassword_Secret_Base32_5() {
            var o = new OneTimePassword("mzxw6");

            Assert.AreEqual("66-6F-6F", BitConverter.ToString(o.GetSecret()));
            Assert.AreEqual("mzxw 6", o.GetBase32Secret());
            Assert.AreEqual("mzxw6", o.GetBase32Secret(SecretFormatFlags.None));
            Assert.AreEqual("mzxw 6", o.GetBase32Secret(SecretFormatFlags.Spacing));
            Assert.AreEqual("mzxw6===", o.GetBase32Secret(SecretFormatFlags.Padding));
            Assert.AreEqual("mzxw 6===", o.GetBase32Secret(SecretFormatFlags.Spacing | SecretFormatFlags.Padding));
        }

        [TestMethod]
        public void OneTimePassword_Secret_Base32_5p() {
            var o = new OneTimePassword("MZXW6===");

            Assert.AreEqual("66-6F-6F", BitConverter.ToString(o.GetSecret()));
            Assert.AreEqual("mzxw 6", o.GetBase32Secret());
            Assert.AreEqual("mzxw6", o.GetBase32Secret(SecretFormatFlags.None));
            Assert.AreEqual("mzxw 6", o.GetBase32Secret(SecretFormatFlags.Spacing));
            Assert.AreEqual("mzxw6===", o.GetBase32Secret(SecretFormatFlags.Padding));
            Assert.AreEqual("mzxw 6===", o.GetBase32Secret(SecretFormatFlags.Spacing | SecretFormatFlags.Padding));
        }

        [TestMethod]
        public void OneTimePassword_Secret_Base32_6() {
            var o = new OneTimePassword("mzxw6y");

            Assert.AreEqual("66-6F-6F-60", BitConverter.ToString(o.GetSecret()));
            Assert.AreEqual("mzxw 6ya", o.GetBase32Secret());
            Assert.AreEqual("mzxw6ya", o.GetBase32Secret(SecretFormatFlags.None));
            Assert.AreEqual("mzxw 6ya", o.GetBase32Secret(SecretFormatFlags.Spacing));
            Assert.AreEqual("mzxw6ya=", o.GetBase32Secret(SecretFormatFlags.Padding));
            Assert.AreEqual("mzxw 6ya=", o.GetBase32Secret(SecretFormatFlags.Spacing | SecretFormatFlags.Padding));
        }

        [TestMethod]
        public void OneTimePassword_Secret_Base32_7() {
            var o = new OneTimePassword("mzxw6yq");

            Assert.AreEqual("66-6F-6F-62", BitConverter.ToString(o.GetSecret()));
            Assert.AreEqual("mzxw 6yq", o.GetBase32Secret());
            Assert.AreEqual("mzxw6yq", o.GetBase32Secret(SecretFormatFlags.None));
            Assert.AreEqual("mzxw 6yq", o.GetBase32Secret(SecretFormatFlags.Spacing));
            Assert.AreEqual("mzxw6yq=", o.GetBase32Secret(SecretFormatFlags.Padding));
            Assert.AreEqual("mzxw 6yq=", o.GetBase32Secret(SecretFormatFlags.Spacing | SecretFormatFlags.Padding));
        }

        [TestMethod]
        public void OneTimePassword_Secret_Base32_7p() {
            var o = new OneTimePassword("MZXW6YQ=");

            Assert.AreEqual("66-6F-6F-62", BitConverter.ToString(o.GetSecret()));
            Assert.AreEqual("mzxw 6yq", o.GetBase32Secret());
            Assert.AreEqual("mzxw6yq", o.GetBase32Secret(SecretFormatFlags.None));
            Assert.AreEqual("mzxw 6yq", o.GetBase32Secret(SecretFormatFlags.Spacing));
            Assert.AreEqual("mzxw6yq=", o.GetBase32Secret(SecretFormatFlags.Padding));
            Assert.AreEqual("mzxw 6yq=", o.GetBase32Secret(SecretFormatFlags.Spacing | SecretFormatFlags.Padding));
        }

        [TestMethod]
        public void OneTimePassword_Secret_Base32_8() {
            var o = new OneTimePassword("mzxw6ytb");

            Assert.AreEqual("66-6F-6F-62-61", BitConverter.ToString(o.GetSecret()));
            Assert.AreEqual("mzxw 6ytb", o.GetBase32Secret());
            Assert.AreEqual("mzxw6ytb", o.GetBase32Secret(SecretFormatFlags.None));
            Assert.AreEqual("mzxw 6ytb", o.GetBase32Secret(SecretFormatFlags.Spacing));
            Assert.AreEqual("mzxw6ytb", o.GetBase32Secret(SecretFormatFlags.Padding));
            Assert.AreEqual("mzxw 6ytb", o.GetBase32Secret(SecretFormatFlags.Spacing | SecretFormatFlags.Padding));
        }

        [TestMethod]
        public void OneTimePassword_Secret_Base32_8p() {
            var o = new OneTimePassword("MZXW6YTB");

            Assert.AreEqual("66-6F-6F-62-61", BitConverter.ToString(o.GetSecret()));
            Assert.AreEqual("mzxw 6ytb", o.GetBase32Secret());
            Assert.AreEqual("mzxw6ytb", o.GetBase32Secret(SecretFormatFlags.None));
            Assert.AreEqual("mzxw 6ytb", o.GetBase32Secret(SecretFormatFlags.Spacing));
            Assert.AreEqual("mzxw6ytb", o.GetBase32Secret(SecretFormatFlags.Padding));
            Assert.AreEqual("mzxw 6ytb", o.GetBase32Secret(SecretFormatFlags.Spacing | SecretFormatFlags.Padding));
        }

        [TestMethod]
        public void OneTimePassword_Secret_Base32_9() {
            var o = new OneTimePassword("mzxw6ytbo");

            Assert.AreEqual("66-6F-6F-62-61-70", BitConverter.ToString(o.GetSecret()));
            Assert.AreEqual("mzxw 6ytb oa", o.GetBase32Secret());
            Assert.AreEqual("mzxw6ytboa", o.GetBase32Secret(SecretFormatFlags.None));
            Assert.AreEqual("mzxw 6ytb oa", o.GetBase32Secret(SecretFormatFlags.Spacing));
            Assert.AreEqual("mzxw6ytboa======", o.GetBase32Secret(SecretFormatFlags.Padding));
            Assert.AreEqual("mzxw 6ytb oa== ====", o.GetBase32Secret(SecretFormatFlags.Spacing | SecretFormatFlags.Padding));
        }

        [TestMethod]
        public void OneTimePassword_Secret_Base32_10() {
            var o = new OneTimePassword("mzxw6ytboi");

            Assert.AreEqual("66-6F-6F-62-61-72", BitConverter.ToString(o.GetSecret()));
            Assert.AreEqual("mzxw 6ytb oi", o.GetBase32Secret());
            Assert.AreEqual("mzxw6ytboi", o.GetBase32Secret(SecretFormatFlags.None));
            Assert.AreEqual("mzxw 6ytb oi", o.GetBase32Secret(SecretFormatFlags.Spacing));
            Assert.AreEqual("mzxw6ytboi======", o.GetBase32Secret(SecretFormatFlags.Padding));
            Assert.AreEqual("mzxw 6ytb oi== ====", o.GetBase32Secret(SecretFormatFlags.Spacing | SecretFormatFlags.Padding));
        }

        [TestMethod]
        public void OneTimePassword_Secret_Base32_10p() {
            var o = new OneTimePassword("MZXW6YTBOI======");

            Assert.AreEqual("66-6F-6F-62-61-72", BitConverter.ToString(o.GetSecret()));
            Assert.AreEqual("mzxw 6ytb oi", o.GetBase32Secret());
            Assert.AreEqual("mzxw6ytboi", o.GetBase32Secret(SecretFormatFlags.None));
            Assert.AreEqual("mzxw 6ytb oi", o.GetBase32Secret(SecretFormatFlags.Spacing));
            Assert.AreEqual("mzxw6ytboi======", o.GetBase32Secret(SecretFormatFlags.Padding));
            Assert.AreEqual("mzxw 6ytb oi== ====", o.GetBase32Secret(SecretFormatFlags.Spacing | SecretFormatFlags.Padding));
        }

        [TestMethod]
        public void OneTimePassword_Secret_Base32_16() {
            var o = new OneTimePassword("jbsw y3dp ehpk 3pxp");

            Assert.AreEqual("48-65-6C-6C-6F-21-DE-AD-BE-EF", BitConverter.ToString(o.GetSecret()));
            Assert.AreEqual("jbsw y3dp ehpk 3pxp", o.GetBase32Secret());
            Assert.AreEqual("jbswy3dpehpk3pxp", o.GetBase32Secret(SecretFormatFlags.None));
            Assert.AreEqual("jbsw y3dp ehpk 3pxp", o.GetBase32Secret(SecretFormatFlags.Spacing));
            Assert.AreEqual("jbswy3dpehpk3pxp", o.GetBase32Secret(SecretFormatFlags.Padding));
            Assert.AreEqual("jbsw y3dp ehpk 3pxp", o.GetBase32Secret(SecretFormatFlags.Spacing | SecretFormatFlags.Padding));
        }

        #endregion


        #region HOTP

        [TestMethod]
        public void OneTimePassword_HOTP_Generate() {
            var o = new OneTimePassword(ASCIIEncoding.ASCII.GetBytes("12345678901234567890")) { TimeStep = 0 };

            Assert.AreEqual(755224, o.GetCode());
            Assert.AreEqual(287082, o.GetCode());
            Assert.AreEqual(359152, o.GetCode());
            Assert.AreEqual(969429, o.GetCode());
            Assert.AreEqual(338314, o.GetCode());
            Assert.AreEqual(254676, o.GetCode());
            Assert.AreEqual(287922, o.GetCode());
            Assert.AreEqual(162583, o.GetCode());
            Assert.AreEqual(399871, o.GetCode());
            Assert.AreEqual(520489, o.GetCode());
        }

        [TestMethod]
        public void OneTimePassword_HOTP_Validate() {
            var o = new OneTimePassword(ASCIIEncoding.ASCII.GetBytes("12345678901234567890")) { TimeStep = 0 };

            Assert.AreEqual(true, o.IsCodeValid(755224));
            Assert.AreEqual(true, o.IsCodeValid(287082));
            Assert.AreEqual(true, o.IsCodeValid(359152));
            Assert.AreEqual(true, o.IsCodeValid(969429));
            Assert.AreEqual(true, o.IsCodeValid(338314));
            Assert.AreEqual(true, o.IsCodeValid(254676));
            Assert.AreEqual(true, o.IsCodeValid(287922));
            Assert.AreEqual(true, o.IsCodeValid(162583));
            Assert.AreEqual(true, o.IsCodeValid(399871));
            Assert.AreEqual(true, o.IsCodeValid(520489));
        }

        [TestMethod]
        public void OneTimePassword_HOTP_Generate_SHA1() {
            var o = new OneTimePassword(ASCIIEncoding.ASCII.GetBytes("12345678901234567890")) { Digits = 8, TimeStep = 0 };

            o.Counter = 0x0000000000000001;
            Assert.AreEqual(94287082, o.GetCode());

            o.Counter = 0x00000000023523EC;
            Assert.AreEqual(07081804, o.GetCode());

            o.Counter = 0x00000000023523ED;
            Assert.AreEqual(14050471, o.GetCode());

            o.Counter = 0x000000000273EF07;
            Assert.AreEqual(89005924, o.GetCode());

            o.Counter = 0x0000000003F940AA;
            Assert.AreEqual(69279037, o.GetCode());

            o.Counter = 0x0000000027BC86AA;
            Assert.AreEqual(65353130, o.GetCode());
        }

        [TestMethod]
        public void OneTimePassword_HOTP_Validate_SHA1() {
            var o = new OneTimePassword(ASCIIEncoding.ASCII.GetBytes("12345678901234567890")) { Digits = 8, TimeStep = 0 };

            o.Counter = 0x0000000000000001;
            Assert.AreEqual(true, o.IsCodeValid(94287082));
            Assert.AreEqual(true, o.IsCodeValid(94287082));
            Assert.AreEqual(false, o.IsCodeValid(94287082));

            o.Counter = 0x00000000023523EC;
            Assert.AreEqual(true, o.IsCodeValid("0708 1804"));

            o.Counter = 0x00000000023523ED;
            Assert.AreEqual(true, o.IsCodeValid(14050471));

            o.Counter = 0x000000000273EF07;
            Assert.AreEqual(true, o.IsCodeValid(89005924));

            o.Counter = 0x0000000003F940AA;
            Assert.AreEqual(true, o.IsCodeValid(69279037));

            o.Counter = 0x0000000027BC86AA;
            Assert.AreEqual(true, o.IsCodeValid(65353130));
        }

        #endregion


        #region TOTP

        [TestMethod]
        public void OneTimePassword_TOTP_Generate_SHA1() {
            var o = new OneTimePassword(ASCIIEncoding.ASCII.GetBytes("12345678901234567890")) { Digits = 8 };
            var p = new PrivateObject(o);

            p.SetFieldOrProperty("TestTime", new DateTime(1970, 01, 01, 00, 00, 59));
            Assert.AreEqual(0x0000000000000001, o.Counter);
            Assert.AreEqual(94287082, o.GetCode());

            p.SetFieldOrProperty("TestTime", new DateTime(2005, 03, 18, 01, 58, 29));
            Assert.AreEqual(0x00000000023523EC, o.Counter);
            Assert.AreEqual(07081804, o.GetCode());

            p.SetFieldOrProperty("TestTime", new DateTime(2005, 03, 18, 01, 58, 31));
            Assert.AreEqual(0x00000000023523ED, o.Counter);
            Assert.AreEqual(14050471, o.GetCode());

            p.SetFieldOrProperty("TestTime", new DateTime(2009, 02, 13, 23, 31, 30));
            Assert.AreEqual(0x000000000273EF07, o.Counter);
            Assert.AreEqual(89005924, o.GetCode());

            p.SetFieldOrProperty("TestTime", new DateTime(2033, 05, 18, 03, 33, 20));
            Assert.AreEqual(0x0000000003F940AA, o.Counter);
            Assert.AreEqual(69279037, o.GetCode());

            p.SetFieldOrProperty("TestTime", new DateTime(2603, 10, 11, 11, 33, 20));
            Assert.AreEqual(0x0000000027BC86AA, o.Counter);
            Assert.AreEqual(65353130, o.GetCode());
        }

        [TestMethod]
        public void OneTimePassword_TOTP_Validate_SHA1() {
            var o = new OneTimePassword(ASCIIEncoding.ASCII.GetBytes("12345678901234567890")) { Digits = 8 };
            var p = new PrivateObject(o);

            p.SetFieldOrProperty("TestTime", new DateTime(1970, 01, 01, 00, 00, 59));
            Assert.AreEqual(true, o.IsCodeValid(94287082));

            p.SetFieldOrProperty("TestTime", new DateTime(2005, 03, 18, 01, 58, 29));
            Assert.AreEqual(true, o.IsCodeValid(07081804));

            p.SetFieldOrProperty("TestTime", new DateTime(2005, 03, 18, 01, 58, 31));
            Assert.AreEqual(true, o.IsCodeValid(14050471));

            p.SetFieldOrProperty("TestTime", new DateTime(2009, 02, 13, 23, 31, 30));
            Assert.AreEqual(true, o.IsCodeValid(89005924));

            p.SetFieldOrProperty("TestTime", new DateTime(2033, 05, 18, 03, 33, 20));
            Assert.AreEqual(true, o.IsCodeValid(69279037));

            p.SetFieldOrProperty("TestTime", new DateTime(2603, 10, 11, 11, 33, 20));
            Assert.AreEqual(true, o.IsCodeValid(65353130));
        }


        [TestMethod]
        public void OneTimePassword_TOTP_Generate_SHA256() {
            var o = new OneTimePassword(ASCIIEncoding.ASCII.GetBytes("12345678901234567890123456789012")) { Algorithm = OneTimePasswordAlgorithm.Sha256, Digits = 8 };
            var p = new PrivateObject(o);

            p.SetFieldOrProperty("TestTime", new DateTime(1970, 01, 01, 00, 00, 59));
            Assert.AreEqual(0x0000000000000001, o.Counter);
            Assert.AreEqual(46119246, o.GetCode());

            p.SetFieldOrProperty("TestTime", new DateTime(2005, 03, 18, 01, 58, 29));
            Assert.AreEqual(0x00000000023523EC, o.Counter);
            Assert.AreEqual(68084774, o.GetCode());

            p.SetFieldOrProperty("TestTime", new DateTime(2005, 03, 18, 01, 58, 31));
            Assert.AreEqual(0x00000000023523ED, o.Counter);
            Assert.AreEqual(67062674, o.GetCode());

            p.SetFieldOrProperty("TestTime", new DateTime(2009, 02, 13, 23, 31, 30));
            Assert.AreEqual(0x000000000273EF07, o.Counter);
            Assert.AreEqual(91819424, o.GetCode());

            p.SetFieldOrProperty("TestTime", new DateTime(2033, 05, 18, 03, 33, 20));
            Assert.AreEqual(0x0000000003F940AA, o.Counter);
            Assert.AreEqual(90698825, o.GetCode());

            p.SetFieldOrProperty("TestTime", new DateTime(2603, 10, 11, 11, 33, 20));
            Assert.AreEqual(0x0000000027BC86AA, o.Counter);
            Assert.AreEqual(77737706, o.GetCode());
        }

        [TestMethod]
        public void OneTimePassword_TOTP_Validate_SHA256() {
            var o = new OneTimePassword(ASCIIEncoding.ASCII.GetBytes("12345678901234567890123456789012")) { Algorithm = OneTimePasswordAlgorithm.Sha256, Digits = 8 };
            var p = new PrivateObject(o);

            p.SetFieldOrProperty("TestTime", new DateTime(1970, 01, 01, 00, 00, 59));
            Assert.AreEqual(true, o.IsCodeValid(46119246));

            p.SetFieldOrProperty("TestTime", new DateTime(2005, 03, 18, 01, 58, 29));
            Assert.AreEqual(true, o.IsCodeValid(68084774));

            p.SetFieldOrProperty("TestTime", new DateTime(2005, 03, 18, 01, 58, 31));
            Assert.AreEqual(true, o.IsCodeValid(67062674));

            p.SetFieldOrProperty("TestTime", new DateTime(2009, 02, 13, 23, 31, 30));
            Assert.AreEqual(true, o.IsCodeValid(91819424));

            p.SetFieldOrProperty("TestTime", new DateTime(2033, 05, 18, 03, 33, 20));
            Assert.AreEqual(true, o.IsCodeValid(90698825));

            p.SetFieldOrProperty("TestTime", new DateTime(2603, 10, 11, 11, 33, 20));
            Assert.AreEqual(true, o.IsCodeValid(77737706));
        }


        [TestMethod]
        public void OneTimePassword_TOTP_Generate_SHA512() {
            var o = new OneTimePassword(ASCIIEncoding.ASCII.GetBytes("1234567890123456789012345678901234567890123456789012345678901234")) { Algorithm = OneTimePasswordAlgorithm.Sha512, Digits = 8 };
            var p = new PrivateObject(o);

            p.SetFieldOrProperty("TestTime", new DateTime(1970, 01, 01, 00, 00, 59));
            Assert.AreEqual(0x0000000000000001, o.Counter);
            Assert.AreEqual(90693936, o.GetCode());

            p.SetFieldOrProperty("TestTime", new DateTime(2005, 03, 18, 01, 58, 29));
            Assert.AreEqual(0x00000000023523EC, o.Counter);
            Assert.AreEqual(25091201, o.GetCode());

            p.SetFieldOrProperty("TestTime", new DateTime(2005, 03, 18, 01, 58, 31));
            Assert.AreEqual(0x00000000023523ED, o.Counter);
            Assert.AreEqual(99943326, o.GetCode());

            p.SetFieldOrProperty("TestTime", new DateTime(2009, 02, 13, 23, 31, 30));
            Assert.AreEqual(0x000000000273EF07, o.Counter);
            Assert.AreEqual(93441116, o.GetCode());

            p.SetFieldOrProperty("TestTime", new DateTime(2033, 05, 18, 03, 33, 20));
            Assert.AreEqual(0x0000000003F940AA, o.Counter);
            Assert.AreEqual(38618901, o.GetCode());

            p.SetFieldOrProperty("TestTime", new DateTime(2603, 10, 11, 11, 33, 20));
            Assert.AreEqual(0x0000000027BC86AA, o.Counter);
            Assert.AreEqual(47863826, o.GetCode());
        }

        [TestMethod]
        public void OneTimePassword_TOTP_Validate_SHA512() {
            var o = new OneTimePassword(ASCIIEncoding.ASCII.GetBytes("1234567890123456789012345678901234567890123456789012345678901234")) { Algorithm = OneTimePasswordAlgorithm.Sha512, Digits = 8 };
            var p = new PrivateObject(o);

            p.SetFieldOrProperty("TestTime", new DateTime(1970, 01, 01, 00, 00, 59));
            Assert.AreEqual(true, o.IsCodeValid(90693936));

            p.SetFieldOrProperty("TestTime", new DateTime(2005, 03, 18, 01, 58, 29));
            Assert.AreEqual(true, o.IsCodeValid(25091201));

            p.SetFieldOrProperty("TestTime", new DateTime(2005, 03, 18, 01, 58, 31));
            Assert.AreEqual(true, o.IsCodeValid(99943326));

            p.SetFieldOrProperty("TestTime", new DateTime(2009, 02, 13, 23, 31, 30));
            Assert.AreEqual(true, o.IsCodeValid(93441116));

            p.SetFieldOrProperty("TestTime", new DateTime(2033, 05, 18, 03, 33, 20));
            Assert.AreEqual(true, o.IsCodeValid(38618901));

            p.SetFieldOrProperty("TestTime", new DateTime(2603, 10, 11, 11, 33, 20));
            Assert.AreEqual(true, o.IsCodeValid(47863826));
        }

        #endregion


        #region Parameters

        [TestMethod]
        public void OneTimePassword_Parameter_Digits_1() {
            var o = new OneTimePassword();
            o.Digits = 4;
            o.Digits = 9;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void OneTimePassword_Parameter_Digits_2() {
            var o = new OneTimePassword();
            o.Digits = 3;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void OneTimePassword_Parameter_Digits_3() {
            var o = new OneTimePassword();
            o.Digits = 10;
        }


        [TestMethod]
        public void OneTimePassword_Parameter_Counter_1() {
            var o = new OneTimePassword() { TimeStep = 0 };
            o.Counter = 11;
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void OneTimePassword_Parameter_Counter_2() {
            var o = new OneTimePassword();
            o.Counter = 11;
        }


        [TestMethod]
        public void OneTimePassword_Parameter_Algorithm_1() {
            var o = new OneTimePassword();
            o.Algorithm = OneTimePasswordAlgorithm.Sha1;
            o.Algorithm = OneTimePasswordAlgorithm.Sha256;
            o.Algorithm = OneTimePasswordAlgorithm.Sha512;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void OneTimePassword_Parameter_Algorithm_2() {
            var o = new OneTimePassword();
            o.Algorithm = (OneTimePasswordAlgorithm)3;
        }


        [TestMethod]
        public void OneTimePassword_Parameter_GetCode_1() {
            var o = new OneTimePassword();
            o.GetCode();
            o.GetCode(4);
            o.GetCode(9);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void OneTimePassword_Parameter_GetCode_2() {
            var o = new OneTimePassword();
            o.GetCode(3);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void OneTimePassword_Parameter_GetCode_3() {
            var o = new OneTimePassword();
            o.GetCode(10);
        }

        #endregion

    }
}
