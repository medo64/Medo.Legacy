using Medo.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Test {

    [TestClass()]
    public class PasswordTest {

        public TestContext TestContext { get; set; }


        #region MD5

        [TestMethod()]
        public void Password_Md5_Create() {
            var hash = Password.Create("Test", PasswordAlgorithm.MD5);
            Assert.AreEqual(true, Password.Verify("Test", hash));
            Assert.AreEqual(false, Password.Verify("NotTest", hash));
            Assert.AreEqual(8, hash.Split('$')[2].Length); //default salt length
        }

        [TestMethod()]
        public void Password_Md5_CreateWithoutSalt() {
            Assert.AreEqual("$1$$smLce1bQjZePWXbJ5eh58/", Password.Create("Test", 0, PasswordAlgorithm.MD5));
        }

        [TestMethod()]
        public void Password_Md5_CreateLong() {
            var hash = Password.Create("ABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZ", PasswordAlgorithm.MD5);
            Assert.AreEqual(true, Password.Verify("ABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZ", hash));
        }

        [TestMethod()]
        public void Password_Md5_Create16() {
            var hash = Password.Create("1234567890123456", 0, PasswordAlgorithm.MD5);
            Assert.AreEqual("$1$$RvxDspYl0hrlDuSmGR1fc/", hash);
        }

        [TestMethod()]
        public void Password_Md5_Verify() {
            Assert.AreEqual(true, Password.Verify("Test", "$1$SALT$iYTuv61EcPDadxVotGguH0"));
            Assert.AreEqual(false, Password.Verify("Test", "$1$SALT$iYTuv61EcPDadxVotGguHF"));
        }

        [TestMethod()]
        public void Password_Md5_VerifyWithoutSalt() {
            Assert.AreEqual(true, Password.Verify("Test", "$1$$smLce1bQjZePWXbJ5eh58/"));
            Assert.AreEqual(false, Password.Verify("Test", "$1$$smLce1bQjZePWXbJ5eh580"));
        }

        [TestMethod()]
        public void Password_Md5_Verify2() {
            Assert.AreEqual(true, Password.Verify("password", "$1$3azHgidD$SrJPt7B.9rekpmwJwtON31"));
            Assert.AreEqual(false, Password.Verify("password", "$1$3azHgidD$SrJPt7B.9rekpmwJwtON30"));
        }

        #endregion

        #region MD5-Apache

        [TestMethod()]
        public void Password_Md5Apache_Create() {
            var hash = Password.Create("Test", PasswordAlgorithm.MD5Apache);
            Assert.AreEqual(true, Password.Verify("Test", hash));
            Assert.AreEqual(false, Password.Verify("NotTest", hash));
            Assert.AreEqual(8, hash.Split('$')[2].Length); //default salt length
        }

        [TestMethod()]
        public void Password_Md5Apache_CreateWithoutSalt() {
            Assert.AreEqual("$apr1$$zccNMO7jOau6cLaAIpdIp1", Password.Create("Test", 0, PasswordAlgorithm.MD5Apache));
        }

        [TestMethod()]
        public void Password_Md5Apache_Verify() {
            Assert.AreEqual(true, Password.Verify("Test", "$apr1$8fNPKrzo$LMfFH4wsbetnSxVk8zvnL/"));
            Assert.AreEqual(false, Password.Verify("Test", "$apr1$8fNPKrzo$LMfFH4wsbetnSxVk8zvnL0"));
        }

        [TestMethod()]
        public void Password_Md5Apache_Verify2() {
            Assert.AreEqual(true, Password.Verify("myPassword", "$apr1$qHDFfhPC$nITSVHgYbDAK1Y0acGRnY0"));
        }

        [TestMethod()]
        public void Password_Md5Apache_VerifyWithoutSalt() {
            Assert.AreEqual(true, Password.Verify("Test", "$apr1$$zccNMO7jOau6cLaAIpdIp1"));
            Assert.AreEqual(false, Password.Verify("Test", "$apr1$$zccNMO7jOau6cLaAIpdIp0"));
        }

        #endregion

        #region SHA-256

        [TestMethod()]
        public void Password_Sha256_Create() {
            var hash = Password.Create("Test", PasswordAlgorithm.Sha256);
            Assert.AreEqual(true, Password.Verify("Test", hash));
            Assert.AreEqual(false, Password.Verify("NotTest", hash));
            Assert.AreEqual(16, hash.Split('$')[2].Length); //default salt length
        }

        [TestMethod()]
        public void Password_Sha256_CreateWithoutSalt() {
            Assert.AreEqual("$5$$HznmDc1T0z.rHKK6lKLl06rT2QuK1hhSbA09Zur2KsD", Password.Create("Test", 0, PasswordAlgorithm.Sha256));
        }

        [TestMethod()]
        public void Password_Sha256_Create32() {
            var hash = Password.Create("12345678901234567890123456789012", 0, PasswordAlgorithm.Sha256);
            Assert.AreEqual("$5$$aBwv.7LCzECcVRAUqSSEFrd.zN54eADoVnXZWC5res6", hash);
        }

        [TestMethod()]
        public void Password_Sha256_CreateLong() {
            var hash = Password.Create("ABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZ", PasswordAlgorithm.Sha256);
            Assert.AreEqual(true, Password.Verify("ABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZ", hash));
        }

        [TestMethod()]
        public void Password_Sha256_CreateExplicitRounds() {
            Assert.AreEqual("$5$rounds=7777$$Z7sdS/EnisPsr1uK7pcVGQACIOOtoEREEqXJUHY.ja3", Password.Create("Test", 0, PasswordAlgorithm.Sha256, 7777));
        }

        [TestMethod()]
        public void Password_Sha256_Verify() {
            Assert.AreEqual(true, Password.Verify("Test", "$5$SALT$hRDk2PDSQpm22hspBC9DW3wmKv58ZcIPLrTAI/PyBc9"));
            Assert.AreEqual(false, Password.Verify("Test", "$5$SALT$hRDk2PDSQpm22hspBC9DW3wmKv58ZcIPLrTAI/PyBc0"));
        }

        [TestMethod()]
        public void Password_Sha256_VerifyWithoutSalt() {
            Assert.AreEqual(true, Password.Verify("Test", "$5$$HznmDc1T0z.rHKK6lKLl06rT2QuK1hhSbA09Zur2KsD"));
            Assert.AreEqual(false, Password.Verify("Test", "$5$$HznmDc1T0z.rHKK6lKLl06rT2QuK1hhSbA09Zur2Ks0"));
        }

        [TestMethod()]
        public void Password_Sha256_VerifySaltTooLong() {
            Assert.AreEqual(true, Password.Verify("Hello world!", "$5$rounds=10000$saltstringsaltst$3xv.VbSHBb41AL9AvLeujZkZRBAwqFMz2.opqey6IcA"));
        }

        [TestMethod()]
        public void Password_Sha256_ExtraLongPassword() {
            Assert.AreEqual(true, Password.Verify("we have a short salt string but not a short password", "$5$rounds=77777$short$JiO1O3ZpDAxGJeaDIuqCoEFysAe1mZNJRs3pw0KQRd/"));
        }

        #endregion

        #region SHA-512

        [TestMethod()]
        public void Password_Sha512_Create() {
            var hash = Password.Create("Test", PasswordAlgorithm.Sha512);
            Assert.AreEqual(true, Password.Verify("Test", hash));
            Assert.AreEqual(false, Password.Verify("NotTest", hash));
            Assert.AreEqual(16, hash.Split('$')[2].Length); //default salt length
        }

        [TestMethod()]
        public void Password_Sha512_CreateWithoutSalt() {
            Assert.AreEqual("$6$$A2vGKWUkCCh28GOsloAzFlH9OgSh8Kv37fsIgM/FmwIPpmZXE/Rx6h6Fdjw7bEasMtpE/e9QQL9Te0d1pUJk./", Password.Create("Test", 0, PasswordAlgorithm.Sha512));
        }

        [TestMethod()]
        public void Password_Sha512_Create64() {
            var hash = Password.Create("1234567890123456789012345678901234567890123456789012345678901234", 0, PasswordAlgorithm.Sha512);
            Assert.AreEqual("$6$$WnKefX4kEZjuyvYWY6Bf5.Us3GWgJCcwj8faQRpFtCg9/aJOhojZ1vpchMG6CmNRYbn.y/Z.l6WotGTVuFSFW0", hash);
        }

        [TestMethod()]
        public void Password_Sha512_CreateExplicitRounds() {
            Assert.AreEqual("$6$rounds=7777$$UNBSSrJ9WQTbHqvso9.yDg0XdJAraq1dZir/V3SPvApoa.E0ilnLP.803MJqIHjOtTvuhxGv/cAXJ0ccTpYBP1", Password.Create("Test", 0, PasswordAlgorithm.Sha512, 7777));
        }

        [TestMethod()]
        public void Password_Sha512_Verify() {
            Assert.AreEqual(true, Password.Verify("Test", "$6$SALT$8GXK57PY.bq4j7Ng3f0LF6NcPXxUXqmmseKtw1ugn8uoKXiPJWG8Ub6bxJcHAPBL2y0ppLmQJcpR8mYJbdjVF1"));
            Assert.AreEqual(false, Password.Verify("Test", "$6$SALT$8GXK57PY.bq4j7Ng3f0LF6NcPXxUXqmmseKtw1ugn8uoKXiPJWG8Ub6bxJcHAPBL2y0ppLmQJcpR8mYJbdjVF0"));
        }

        [TestMethod()]
        public void Password_Sha512_VerifyWithoutSalt() {
            Assert.AreEqual(true, Password.Verify("Test", "$6$$A2vGKWUkCCh28GOsloAzFlH9OgSh8Kv37fsIgM/FmwIPpmZXE/Rx6h6Fdjw7bEasMtpE/e9QQL9Te0d1pUJk./"));
            Assert.AreEqual(false, Password.Verify("Test", "$6$$A2vGKWUkCCh28GOsloAzFlH9OgSh8Kv37fsIgM/FmwIPpmZXE/Rx6h6Fdjw7bEasMtpE/e9QQL9Te0d1pUJk.0"));
        }

        [TestMethod()]
        public void Password_Sha512_VerifySaltTooLong() {
            Assert.AreEqual(true, Password.Verify("Hello world!", "$6$rounds=10000$saltstringsaltst$OW1/O6BYHV6BcXZu8QVeXbDWra3Oeqh0sbHbbMCVNSnCM/UrjmM0Dp8vOuZeHBy/YTBmSK6H9qs/y3RnOaw5v."));
        }

        [TestMethod()]
        public void Password_Sha512_ExtraLongPassword() {
            Assert.AreEqual(true, Password.Verify("a very much longer text to encrypt.  This one even stretches over morethan one line.", "$6$rounds=1400$anotherlongsalts$POfYwTEok97VWcjxIiSOjiykti.o/pQs.wPvMxQ6Fm7I6IoYN3CmLs66x9t0oSwbtEW7o7UmJEiDwGqd8p4ur1"));
        }

        #endregion

        #region Default

        [TestMethod()]
        public void Password_Default_Create() {
            var hash = Password.Create("Test", PasswordAlgorithm.Sha512);
            Assert.IsTrue(hash.StartsWith("$6$", StringComparison.OrdinalIgnoreCase));
            Assert.AreEqual(106, hash.Length); //total length
            Assert.AreEqual(16, hash.Split('$')[2].Length); //default salt length
            Assert.AreEqual(86, hash.Split('$')[3].Length); //hash length
        }

        #endregion

        #region Errors

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Password_Error_NullPassword() {
            Password.Create(null, PasswordAlgorithm.MD5);
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Password_Error_NullSalt() {
            Password.Create(new byte[] { }, null, PasswordAlgorithm.MD5, 5000);
        }


        [TestMethod()]
        public void Password_Error_UnknownAlgorithm() {
            Assert.AreEqual(false, Password.Verify("Test", "$XXX$SALT$8GXK57PY.bq4j7Ng3f0LF6NcPXxUXqmmseKtw1ugn8uoKXiPJWG8Ub6bxJcHAPBL2y0ppLmQJcpR8mYJbdjVF1"));
        }

        [TestMethod()]
        public void Password_Error_NullPasswordHash() {
            Assert.AreEqual(false, Password.Verify("Test", null));
        }

        [TestMethod()]
        public void Password_Error_EmptyPasswordHash() {
            Assert.AreEqual(false, Password.Verify("Test", ""));
        }

        #endregion

    }
}
