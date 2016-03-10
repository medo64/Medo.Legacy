//Josip Medved <jmedved@jmedved.com>   www.medo64.com

//2016-03-09: Initial version.


using System;
using System.Collections;
using System.Collections.Generic;

namespace Medo.Math {

    /// <summary>
    /// Statistical calculations for Box-and-Whiskers plot.
    /// </summary>
    /// <remarks>
    /// http://www.purplemath.com/modules/boxwhisk.htm
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "While this class offers ICollection interface, it is not a collection as such.")]
    public class BoxAndWhiskers : ICollection<double> {

        private readonly List<double> Items = new List<double>();

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        public BoxAndWhiskers() { }



        /// <summary>
        /// Gets minimum value.
        /// </summary>
        public double Minimum {
            get {
                if (this.Cache == null) { this.Cache = new BoxAndWhiskersCache(this.Items); }
                return this.Cache.Minimum;
            }
        }

        /// <summary>
        /// Gets lower submedian value.
        /// Also called Q3.
        /// </summary>
        public double LowerSubmedian {
            get {
                if (this.Cache == null) { this.Cache = new BoxAndWhiskersCache(this.Items); }
                return this.Cache.LowerSubmedian;
            }
        }

        /// <summary>
        /// Gets median value.
        /// Also called Q2.
        /// </summary>
        public double Median {
            get {
                if (this.Cache == null) { this.Cache = new BoxAndWhiskersCache(this.Items); }
                return this.Cache.Median;
            }
        }

        /// <summary>
        /// Gets upper submedian value.
        /// Also called Q3.
        /// </summary>
        public double UpperSubmedian {
            get {
                if (this.Cache == null) { this.Cache = new BoxAndWhiskersCache(this.Items); }
                return this.Cache.UpperSubmedian;
            }
        }

        /// <summary>
        /// Gets maximum value.
        /// </summary>
        public double Maximum {
            get {
                if (this.Cache == null) { this.Cache = new BoxAndWhiskersCache(this.Items); }
                return this.Cache.Maximum;
            }
        }


        /// <summary>
        /// Gets the five-number summary of the data set.
        /// </summary>
        public double[] GetNumberSummary() {
            return new double[] { this.Minimum, this.LowerSubmedian, this.Median, this.UpperSubmedian, this.Maximum };
        }


        /// <summary>
        /// Gets interquartile range value.
        /// Also called IQR.
        /// </summary>
        public double InterquartileRange {
            get {
                if (this.Cache == null) { this.Cache = new BoxAndWhiskersCache(this.Items); }
                return this.Cache.InterquartileRange;
            }
        }


        /// <summary>
        /// Gets lower fence used to isolate outliers.
        /// Also known as Q1 - 1.5 * IQR.
        /// </summary>
        public double LowerFence {
            get {
                if (this.Cache == null) { this.Cache = new BoxAndWhiskersCache(this.Items); }
                return this.Cache.LowerFence;
            }
        }

        /// <summary>
        /// Gets upper fence used to isolate outliers.
        /// Also known as Q3 + 1.5 * IQR.
        /// </summary>
        public double UpperFence {
            get {
                if (this.Cache == null) { this.Cache = new BoxAndWhiskersCache(this.Items); }
                return this.Cache.UpperFence;
            }
        }


        /// <summary>
        /// Gets lower outer fence used to isolate extremes.
        /// Also known as Q1 - 3 * IQR.
        /// </summary>
        public double LowerOuterFence {
            get {
                if (this.Cache == null) { this.Cache = new BoxAndWhiskersCache(this.Items); }
                return this.Cache.LowerOuterFence;
            }
        }

        /// <summary>
        /// Gets upper outer fence used to isolate extremes.
        /// Also known as Q3 + 3 * IQR.
        /// </summary>
        public double UpperOuterFence {
            get {
                if (this.Cache == null) { this.Cache = new BoxAndWhiskersCache(this.Items); }
                return this.Cache.UpperOuterFence;
            }
        }


        /// <summary>
        /// Gets minimum value excluding outliers.
        /// </summary>
        public double MinimumNonOutlier {
            get {
                if (this.Cache == null) { this.Cache = new BoxAndWhiskersCache(this.Items); }
                return this.Cache.MinimumNonOutlier;
            }
        }

        /// <summary>
        /// Gets maximum value excluding outliers.
        /// </summary>
        public double MaximumNonOutlier {
            get {
                if (this.Cache == null) { this.Cache = new BoxAndWhiskersCache(this.Items); }
                return this.Cache.MaximumNonOutlier;
            }
        }


        /// <summary>
        /// Enumerates lower outliers.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<double> EnumerateLowerOutliers() {
            if (this.Cache == null) { this.Cache = new BoxAndWhiskersCache(this.Items); }
            return this.Cache.EnumerateLowerOutliers();
        }

        /// <summary>
        /// Enumerates upper outliers.
        /// </summary>
        public IEnumerable<double> EnumerateUpperOutliers() {
            if (this.Cache == null) { this.Cache = new BoxAndWhiskersCache(this.Items); }
            return this.Cache.EnumerateUpperOutliers();
        }

        /// <summary>
        /// Enumerates all outliers.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<double> EnumerateOutliers() {
            foreach (var value in this.EnumerateLowerOutliers()) {
                yield return value;
            }
            foreach (var value in this.EnumerateUpperOutliers()) {
                yield return value;
            }
        }


        /// <summary>
        /// Enumerates lower extremes.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<double> EnumerateLowerExtremes() {
            if (this.Cache == null) { this.Cache = new BoxAndWhiskersCache(this.Items); }
            return this.Cache.EnumerateLowerExtremes();
        }

        /// <summary>
        /// Enumerates upper extremes.
        /// </summary>
        public IEnumerable<double> EnumerateUpperExtremes() {
            if (this.Cache == null) { this.Cache = new BoxAndWhiskersCache(this.Items); }
            return this.Cache.EnumerateUpperExtremes();
        }

        /// <summary>
        /// Enumerates all extremes.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<double> EnumerateExtremes() {
            foreach (var value in this.EnumerateLowerExtremes()) {
                yield return value;
            }
            foreach (var value in this.EnumerateUpperExtremes()) {
                yield return value;
            }
        }


        private BoxAndWhiskersCache Cache = null;


        #region IList

        /// <summary>
        /// Adds an item to the collection.
        /// </summary>
        /// <param name="item">The number to add.</param>
        /// <exception cref="ArgumentOutOfRangeException">Must use a real number.</exception>
        public void Add(double item) {
            this.Items.Add(item);
            if (double.IsInfinity(item) || double.IsNaN(item)) { throw new ArgumentOutOfRangeException(nameof(item), "Must use a real number."); }
            this.Cache = null;
        }

        /// <summary>
        /// Adds multiple items to the collection.
        /// </summary>
        /// <param name="items">The numbers to add.</param>
        /// <exception cref="ArgumentOutOfRangeException">Must use real numbers.</exception>
        public void AddRange(IEnumerable<double> items) {
            if (items == null) { throw new ArgumentNullException(nameof(items), "Items cannot be null."); }
            foreach (var item in items) {
                if (double.IsInfinity(item) || double.IsNaN(item)) { throw new ArgumentOutOfRangeException(nameof(items), "Must use real numbers."); }
            }
            this.Items.AddRange(items);
            this.Cache = null;
        }

        /// <summary>
        /// Removes all items from the collection.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "While this class offers ICollection interface, it is not a collection as such.")]
        void ICollection<double>.Clear() {
            this.Items.Clear();
            this.Cache = null;
        }

        /// <summary>
        /// Determines whether the collection contains a specific value.
        /// </summary>
        /// <param name="item">The number to locate in the collection.</param>
        /// <returns>True if item is found in the collection; otherwise, false.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "While this class offers ICollection interface, it is not a collection as such.")]
        bool ICollection<double>.Contains(double item) {
            return this.Items.Contains(item);
        }

        /// <summary>
        /// Copies the elements of the collection to an array, starting at a particular array index.
        /// </summary>
        /// <param name="array">The one-dimensional array that is the destination of the elements copied from collection. The array must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "While this class offers ICollection interface, it is not a collection as such.")]
        void ICollection<double>.CopyTo(double[] array, int arrayIndex) {
            this.Items.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets the number of elements contained in the collection.
        /// </summary>
        public int Count {
            get { return this.Items.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether the collection is read-only.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "While this class offers ICollection interface, it is not a collection as such.")]
        bool ICollection<double>.IsReadOnly {
            get { return false; }
        }

        /// <summary>
        /// Removes the first occurrence of a specific number from the collection.
        /// </summary>
        /// <param name="item">The number to remove from the collection.</param>
        /// <returns>True if item was successfully removed from the collection; otherwise, false. This method also returns false if item is not found in the original collection.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "While this class offers ICollection interface, it is not a collection as such.")]
        bool ICollection<double>.Remove(double item) {
            if (this.Items.Remove(item)) {
                this.Cache = null;
                return true;
            } else {
                return false;
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        public IEnumerator<double> GetEnumerator() {
            if (this.Cache == null) { this.Cache = new BoxAndWhiskersCache(this.Items); } //to sort items (by cache)
            return this.Items.GetEnumerator();
        }

        /// <summary>
        /// Exposes the enumerator, which supports a simple iteration over a non-generic collection.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        #endregion


        private class BoxAndWhiskersCache {

            internal BoxAndWhiskersCache(List<double> items) {
                items.Sort();

                if (items.Count < 5) { throw new InvalidOperationException("Must have at least five values."); }

                this.Items = items;

                var iMedianU = items.Count / 2;
                var iMedianL = (items.Count % 2 == 0) ? iMedianU - 1 : iMedianU;

                var iLowerSubmedianU = iMedianU / 2;
                var iLowerSubmedianL = (iMedianU % 2 == 0) ? iLowerSubmedianU - 1 : iLowerSubmedianU;

                var iUpperSubmedianU = iMedianL + iMedianU / 2 + 1;
                var iUpperSubmedianL = ((iMedianU % 2 == 0) ? iUpperSubmedianU - 1 : iUpperSubmedianU);

                this.Minimum = items[0];
                this.LowerSubmedian = (items[iLowerSubmedianL] + items[iLowerSubmedianU]) / 2;
                this.Median = (items[iMedianL] + items[iMedianU]) / 2;
                this.UpperSubmedian = (items[iUpperSubmedianL] + items[iUpperSubmedianU]) / 2;
                this.Maximum = items[items.Count - 1];

                this.InterquartileRange = this.UpperSubmedian - this.LowerSubmedian;

                this.LowerFence = this.LowerSubmedian - 1.5 * this.InterquartileRange;
                this.UpperFence = this.UpperSubmedian + 1.5 * this.InterquartileRange;
                this.LowerOuterFence = this.LowerSubmedian - 3.0 * this.InterquartileRange;
                this.UpperOuterFence = this.UpperSubmedian + 3.0 * this.InterquartileRange;
            }


            private readonly IList<double> Items;

            internal double Minimum { get; }
            internal double LowerSubmedian { get; }
            internal double Median { get; }
            internal double UpperSubmedian { get; }
            internal double Maximum { get; }

            internal double InterquartileRange { get; }

            internal double LowerFence { get; }
            internal double UpperFence { get; }
            internal double LowerOuterFence { get; }
            internal double UpperOuterFence { get; }


            internal double MinimumNonOutlier {
                get {
                    CalculateOutlierIndices();
                    return this.Items[this.MinimumNonOutlierIndex];
                }
            }

            internal double MaximumNonOutlier {
                get {
                    CalculateOutlierIndices();
                    return this.Items[this.MaximumNonOutlierIndex];
                }
            }


            internal IEnumerable<double> EnumerateLowerExtremes() {
                for (var i = 0; i < this.MinimumNonExtremeIndex; i++) {
                    yield return this.Items[i];
                }
            }

            internal IEnumerable<double> EnumerateUpperExtremes() {
                for (var i = this.MaximumNonExtremeIndex + 1; i < this.Items.Count; i++) {
                    yield return this.Items[i];
                }
            }


            internal IEnumerable<double> EnumerateLowerOutliers() {
                for (var i = 0; i < this.MinimumNonOutlierIndex; i++) {
                    yield return this.Items[i];
                }
            }

            internal IEnumerable<double> EnumerateUpperOutliers() {
                for (var i = this.MaximumNonOutlierIndex + 1; i < this.Items.Count; i++) {
                    yield return this.Items[i];
                }
            }


            private bool CalculatedOutlierIndices;
            private int MinimumNonOutlierIndex;
            private int MinimumNonExtremeIndex;
            private int MaximumNonOutlierIndex;
            private int MaximumNonExtremeIndex;

            private void CalculateOutlierIndices() {
                if (this.CalculatedOutlierIndices) { return; }

                {
                    int i = 0;
                    for (; i < this.Items.Count; i++) {
                        if (this.Items[i] >= this.LowerOuterFence) {
                            this.MinimumNonExtremeIndex = i;
                            break;
                        }
                    }
                    for (; i < this.Items.Count; i++) {
                        if (this.Items[i] >= this.LowerFence) {
                            this.MinimumNonOutlierIndex = i;
                            break;
                        }
                    }
                }

                {
                    int i = this.Items.Count - 1;
                    for (; i >= 0; i--) {
                        if (this.Items[i] <= this.UpperOuterFence) {
                            this.MaximumNonExtremeIndex = i;
                            break;
                        }
                    }
                    for (; i >= 0; i--) {
                        if (this.Items[i] <= this.UpperFence) {
                            this.MaximumNonOutlierIndex = i;
                            break;
                        }
                    }
                }

                this.CalculatedOutlierIndices = true;
            }

        }

    }
}
