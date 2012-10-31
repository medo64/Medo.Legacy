using Medo.Device.Protocol;
using Medo.Extensions.HexadecimalEncoding;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test {

    [TestClass()]
    public class ABusFrameTest {

        public TestContext TestContext { get; set; }


        [TestMethod()]
        public void ABusFrame_Test() {
            var expected = "aa550500010000007d23000000001100007acf";
            var actualFrame = ABusFrame.Parse(HexadecimalEncodingExtensions.FromHexString("aa550500010000007d23000000001100007acf"));
            var actual = HexadecimalEncodingExtensions.ToHexString(actualFrame.GetBytes());
            Assert.AreEqual(expected, actual);
        }

    }
}
