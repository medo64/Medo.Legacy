using Medo.Math;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Text;

namespace Test {

    [TestClass()]
    public class BoxAndWhiskersTest {

        public TestContext TestContext { get; set; }


        [TestMethod()]
        public void BoxAndWhiskers_1() {
            var target = new BoxAndWhiskers();
            target.AddRange(new double[] { 4.3, 5.1, 3.9, 4.5, 4.4, 4.9, 5.0, 4.7, 4.1, 4.6, 4.4, 4.3, 4.8, 4.4, 4.2, 4.5, 4.4 });

            Assert.AreEqual(3.9, target.Minimum);
            Assert.AreEqual(4.3, target.LowerSubmedian);
            Assert.AreEqual(4.4, target.Median);
            Assert.AreEqual(4.75, target.UpperSubmedian);
            Assert.AreEqual(5.1, target.Maximum);

            Assert.AreEqual(0.45, target.InterquartileRange, 0.00001);

            Assert.AreEqual(3.625, target.LowerFence, 0.00001);
            Assert.AreEqual(5.425, target.UpperFence, 0.00001);
            Assert.AreEqual(2.95, target.LowerOuterFence, 0.00001);
            Assert.AreEqual(6.1, target.UpperOuterFence, 0.00001);

            Assert.AreEqual(3.9, target.MinimumNonOutlier);
            Assert.AreEqual(5.1, target.MaximumNonOutlier);

            Assert.AreEqual(JoinNumbers(new double[] { }), JoinNumbers(target.EnumerateOutliers()));
            Assert.AreEqual(JoinNumbers(new double[] { }), JoinNumbers(target.EnumerateExtremes()));

            var summary = target.GetNumberSummary();
            Assert.AreEqual(3.9, summary[0]);
            Assert.AreEqual(4.3, summary[1]);
            Assert.AreEqual(4.4, summary[2]);
            Assert.AreEqual(4.75, summary[3]);
            Assert.AreEqual(5.1, summary[4]);
        }

        [TestMethod()]
        public void BoxAndWhiskers_2() {
            var target = new BoxAndWhiskers();
            target.AddRange(new double[] { 77, 79, 80, 86, 87, 87, 94, 99 });

            Assert.AreEqual(77, target.Minimum);
            Assert.AreEqual(79.5, target.LowerSubmedian);
            Assert.AreEqual(86.5, target.Median);
            Assert.AreEqual(90.5, target.UpperSubmedian);
            Assert.AreEqual(99, target.Maximum);

            Assert.AreEqual(11, target.InterquartileRange);

            Assert.AreEqual(63, target.LowerFence);
            Assert.AreEqual(107, target.UpperFence);
            Assert.AreEqual(46.5, target.LowerOuterFence);
            Assert.AreEqual(123.5, target.UpperOuterFence);

            Assert.AreEqual(77, target.MinimumNonOutlier);
            Assert.AreEqual(99, target.MaximumNonOutlier);

            Assert.AreEqual(JoinNumbers(new double[] { }), JoinNumbers(target.EnumerateOutliers()));
            Assert.AreEqual(JoinNumbers(new double[] { }), JoinNumbers(target.EnumerateExtremes()));
        }

        [TestMethod()]
        public void BoxAndWhiskers_3() {
            var target = new BoxAndWhiskers();
            target.AddRange(new double[] { 79, 53, 82, 91, 87, 98, 80, 93 });

            Assert.AreEqual(53, target.Minimum);
            Assert.AreEqual(79.5, target.LowerSubmedian);
            Assert.AreEqual(84.5, target.Median);
            Assert.AreEqual(92, target.UpperSubmedian);
            Assert.AreEqual(98, target.Maximum);

            Assert.AreEqual(12.5, target.InterquartileRange);

            Assert.AreEqual(60.75, target.LowerFence);
            Assert.AreEqual(110.75, target.UpperFence);
            Assert.AreEqual(42, target.LowerOuterFence);
            Assert.AreEqual(129.5, target.UpperOuterFence);

            Assert.AreEqual(79, target.MinimumNonOutlier);
            Assert.AreEqual(98, target.MaximumNonOutlier);

            Assert.AreEqual(JoinNumbers(new double[] { 53 }), JoinNumbers(target.EnumerateOutliers()));
            Assert.AreEqual(JoinNumbers(new double[] { }), JoinNumbers(target.EnumerateExtremes()));
        }

        [TestMethod()]
        public void BoxAndWhiskers_4() {
            var target = new BoxAndWhiskers();
            target.AddRange(new double[] { 10.2, 14.1, 14.4, 14.4, 14.4, 14.5, 14.5, 14.6, 14.7, 14.7, 14.7, 14.9, 15.1, 15.9, 16.4 });

            Assert.AreEqual(10.2, target.Minimum);
            Assert.AreEqual(14.4, target.LowerSubmedian);
            Assert.AreEqual(14.6, target.Median);
            Assert.AreEqual(14.9, target.UpperSubmedian);
            Assert.AreEqual(16.4, target.Maximum);

            Assert.AreEqual(0.5, target.InterquartileRange);

            Assert.AreEqual(13.65, target.LowerFence);
            Assert.AreEqual(15.65, target.UpperFence);
            Assert.AreEqual(12.9, target.LowerOuterFence);
            Assert.AreEqual(16.4, target.UpperOuterFence);

            Assert.AreEqual(14.1, target.MinimumNonOutlier);
            Assert.AreEqual(15.1, target.MaximumNonOutlier);

            Assert.AreEqual(JoinNumbers(new double[] { 10.2, 15.9, 16.4 }), JoinNumbers(target.EnumerateOutliers()));
            Assert.AreEqual(JoinNumbers(new double[] { 10.2 }), JoinNumbers(target.EnumerateExtremes()));
        }

        [TestMethod()]
        public void BoxAndWhiskers_5() {
            var target = new BoxAndWhiskers();
            target.AddRange(new double[] { 21, 23, 24, 25, 29, 33, 49 });

            Assert.AreEqual(21, target.Minimum);
            Assert.AreEqual(23, target.LowerSubmedian);
            Assert.AreEqual(25, target.Median);
            Assert.AreEqual(33, target.UpperSubmedian);
            Assert.AreEqual(49, target.Maximum);

            Assert.AreEqual(10, target.InterquartileRange);

            Assert.AreEqual(8, target.LowerFence);
            Assert.AreEqual(48, target.UpperFence);
            Assert.AreEqual(-7, target.LowerOuterFence);
            Assert.AreEqual(63, target.UpperOuterFence);

            Assert.AreEqual(21, target.MinimumNonOutlier);
            Assert.AreEqual(33, target.MaximumNonOutlier);

            Assert.AreEqual(JoinNumbers(new double[] { 49 }), JoinNumbers(target.EnumerateOutliers()));
            Assert.AreEqual(JoinNumbers(new double[] { }), JoinNumbers(target.EnumerateExtremes()));
        }


        private string JoinNumbers(IEnumerable<double> values) {
            var sb = new StringBuilder();
            foreach (var value in values) {
                if (sb.Length > 0) { sb.Append("; "); }
                sb.Append(value);
            }
            return sb.ToString();
        }

    }
}
