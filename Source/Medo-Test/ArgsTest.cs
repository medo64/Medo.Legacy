using Medo.Application;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace Test {


    /// <summary>
    ///This is a test class for ArgsTest and is intended
    ///to contain all ArgsTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ArgsTest {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext {
            get {
                return testContextInstance;
            }
            set {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for ContainsKey
        ///</summary>
        [TestMethod()]
        public void ContainsKeyTest_2() {
            Args target = new Args("/a=X /b=1 /a=\"0 1\" file Name", false);
            bool result = target.ContainsKey("AB");

            Assert.AreEqual(false, result);
        }

        /// <summary>
        ///A test for ContainsKey
        ///</summary>
        [TestMethod()]
        public void ContainsKeyTest_1() {
            Args target = new Args("/a=X /b=1 /a=\"0 1\" file Name", false);
            bool result = target.ContainsKey("A");

            Assert.AreEqual(true, result);
        }

        /// <summary>
        ///A test for GetKeys
        ///</summary>
        [TestMethod()]
        public void GetKeysTest() {
            Args target = new Args("/a=X /b=1 /a=\"0 1\" file Name", false);
            string[] result = target.GetKeys();

            Assert.AreEqual(3, result.Length);
            Assert.AreEqual("A", result[0]);
            Assert.AreEqual("B", result[1]);
            Assert.AreEqual("", result[2]);
        }

        /// <summary>
        ///A test for GetValue
        ///</summary>
        [TestMethod()]
        public void GetValueTest_String_1() {
            Args target = new Args("/a=X /b=1 /a=\"0 1\" file Name", false);
            string result = target.GetValue("A", "XX");

            Assert.AreEqual("0 1", result);
        }

        /// <summary>
        ///A test for GetValue
        ///</summary>
        [TestMethod()]
        public void GetValueTest_String_2() {
            Args target = new Args("/a=X /b=1 /a=\"0 1\" file Name", false);
            string result = target.GetValue("X", "XX");

            Assert.AreEqual("XX", result);
        }

        /// <summary>
        ///A test for GetValue
        ///</summary>
        [TestMethod()]
        public void GetValueTest_Bool_1() {
            Args target = new Args("/a=X /b=1 /a=\"0\" file Name", false);
            bool result = target.GetValue("A", true);

            Assert.AreEqual(false, result);
        }

        /// <summary>
        ///A test for GetValue
        ///</summary>
        [TestMethod()]
        public void GetValueTest_Bool_2() {
            Args target = new Args("/a=X /b=1 /a=\"faLse\" file Name", false);
            bool result = target.GetValue("A", true);

            Assert.AreEqual(false, result);
        }

        /// <summary>
        ///A test for GetValue
        ///</summary>
        [TestMethod()]
        public void GetValueTest_Bool_3() {
            Args target = new Args("/a=X /b=1 /a=\"0.123\" file Name", false);
            bool result = target.GetValue("X", true);

            Assert.AreEqual(true, result);
        }

        /// <summary>
        ///A test for GetValue
        ///</summary>
        [TestMethod()]
        public void GetValueTest_Bool_4() {
            Args target = new Args("/a=X /b=1 /a=\"0.123\" file Name", false);
            bool result = target.GetValue("A", true);

            Assert.AreEqual(true, result);
        }

        /// <summary>
        ///A test for GetValue
        ///</summary>
        [TestMethod()]
        public void GetValueTest_Int_1() {
            Args target = new Args("/a=X /b=1 /a=\"3\" file Name", false);
            int result = target.GetValue("A", 2);

            Assert.AreEqual(3, result);
        }

        /// <summary>
        ///A test for GetValue
        ///</summary>
        [TestMethod()]
        public void GetValueTest_Int_2() {
            Args target = new Args("/a=X /b=1 /a=\"3\" file Name", false);
            int result = target.GetValue("X", 2);

            Assert.AreEqual(2, result);
        }

        /// <summary>
        ///A test for GetValue
        ///</summary>
        [TestMethod()]
        public void GetValueTest_Int_3() {
            Args target = new Args("/a=X /b=1 /a=\"0.123\" file Name", false);
            double result = target.GetValue("A", 1);

            Assert.AreEqual(1, result);
        }

        /// <summary>
        ///A test for GetValue
        ///</summary>
        [TestMethod()]
        public void GetValueTest_Double_1() {
            Args target = new Args("/a=X /b=1 /a=\"3\" file Name", false);
            double result = target.GetValue("A", 1.00);

            Assert.AreEqual(3.00, result);
        }

        /// <summary>
        ///A test for GetValue
        ///</summary>
        [TestMethod()]
        public void GetValueTest_Double_2() {
            Args target = new Args("/a=X /b=1 /a=\"0.123\" file Name", false);
            double result = target.GetValue("X", 1);

            Assert.AreEqual(1, result);
        }

        /// <summary>
        ///A test for GetValue
        ///</summary>
        [TestMethod()]
        public void GetValueTest_1() {
            Args target = new Args("/a=0 /b=1 /a=\"x y\" fileName", false);
            string result = target.GetValue("A");

            Assert.AreEqual("x y", result);
        }

        /// <summary>
        ///A test for GetValue
        ///</summary>
        [TestMethod()]
        public void GetValueTest_2() {
            Args target = new Args("/a=0 /b=1 /a=\"x y\" fileName", false);
            string result = target.GetValue("Y");

            Assert.AreEqual(null, result);
        }

        /// <summary>
        ///A test for GetValues
        ///</summary>
        [TestMethod()]
        public void GetValuesTest_First_1() {
            Args target = new Args("\"first one\" /a=0 /b=1 /a=\"x y\" \"File name\"", false);
            string[] results = target.GetValues(null);

            Assert.AreEqual(2, results.Length);
            Assert.AreEqual("first one", results[0]);
            Assert.AreEqual("File name", results[1]);
        }

        /// <summary>
        ///A test for GetValues
        ///</summary>
        [TestMethod()]
        public void GetValuesTest_First_2() {
            Args target = new Args("/a=0 /b=1 /a=\"x y\" \"File name\"", false);
            string[] results = target.GetValues(null);

            Assert.AreEqual(1, results.Length);
            Assert.AreEqual("File name", results[0]);
        }

        /// <summary>
        ///A test for GetValues
        ///</summary>
        [TestMethod()]
        public void GetValuesTest_Slash() {
            Args target = new Args("/a=0 /b=1 /a=\"x y\" fileName", false);
            string[] results = target.GetValues("A");

            Assert.AreEqual(2, results.Length);
            Assert.AreEqual("0", results[0]);
            Assert.AreEqual("x y", results[1]);
        }

        /// <summary>
        ///A test for GetValues
        ///</summary>
        [TestMethod()]
        public void GetValuesTest_Dash() {
            Args target = new Args("-a:0 -b:1 -a:\"x y\" fileName", false);
            string[] results = target.GetValues("A");

            Assert.AreEqual(2, results.Length);
            Assert.AreEqual("0", results[0]);
            Assert.AreEqual("x y", results[1]);
        }

        /// <summary>
        ///A test for GetValues
        ///</summary>
        [TestMethod()]
        public void GetValuesTest_DoubleDash() {
            Args target = new Args("--a=0 --b=1 --a:\"x y\" fileName", false);
            string[] results = target.GetValues("A");

            Assert.AreEqual(2, results.Length);
            Assert.AreEqual("0", results[0]);
            Assert.AreEqual("x y", results[1]);
        }

        /// <summary>
        ///A test for GetValues
        ///</summary>
        [TestMethod()]
        public void GetValueTest_ArrayInit_1() {
            Args target = new Args(new string[] { "Q:\\Projects\\Personal\\Home\\0.00\\Binaries\\HomeService.vshost.exe", "-servicedebug", "-x=file name" }, 1, 2);
            bool result1 = target.ContainsKey("ServiceDebug");
            string result2 = target.GetValue("x");
            int result3 = target.GetValue("g", 2);

            Assert.AreEqual(true, result1);
            Assert.AreEqual("file name", result2);
            Assert.AreEqual(2, result3);
        }

    }
}
