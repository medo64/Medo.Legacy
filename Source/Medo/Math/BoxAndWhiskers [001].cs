/* Josip Medved <jmedved@jmedved.com> * www.medo64.com * MIT License */

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
                if (Cache == null) { Cache = new BoxAndWhiskersCache(Items); }
                return Cache.Minimum;
            }
        }

        /// <summary>
        /// Gets lower submedian value.
        /// Also called Q3.
        /// </summary>
        public double LowerSubmedian {
            get {
                if (Cache == null) { Cache = new BoxAndWhiskersCache(Items); }
                return Cache.LowerSubmedian;
            }
        }

        /// <summary>
        /// Gets median value.
        /// Also called Q2.
        /// </summary>
        public double Median {
            get {
                if (Cache == null) { Cache = new BoxAndWhiskersCache(Items); }
                return Cache.Median;
            }
        }

        /// <summary>
        /// Gets upper submedian value.
        /// Also called Q3.
        /// </summary>
        public double UpperSubmedian {
            get {
                if (Cache == null) { Cache = new BoxAndWhiskersCache(Items); }
                return Cache.UpperSubmedian;
            }
        }

        /// <summary>
        /// Gets maximum value.
        /// </summary>
        public double Maximum {
            get {
                if (Cache == null) { Cache = new BoxAndWhiskersCache(Items); }
                return Cache.Maximum;
            }
        }


        /// <summary>
        /// Gets the five-number summary of the data set.
        /// </summary>
        public double[] GetNumberSummary() {
            return new double[] { Minimum, LowerSubmedian, Median, UpperSubmedian, Maximum };
        }


        /// <summary>
        /// Gets interquartile range value.
        /// Also called IQR.
        /// </summary>
        public double InterquartileRange {
            get {
                if (Cache == null) { Cache = new BoxAndWhiskersCache(Items); }
                return Cache.InterquartileRange;
            }
        }


        /// <summary>
        /// Gets lower fence used to isolate outliers.
        /// Also known as Q1 - 1.5 * IQR.
        /// </summary>
        public double LowerFence {
            get {
                if (Cache == null) { Cache = new BoxAndWhiskersCache(Items); }
                return Cache.LowerFence;
            }
        }

        /// <summary>
        /// Gets upper fence used to isolate outliers.
        /// Also known as Q3 + 1.5 * IQR.
        /// </summary>
        public double UpperFence {
            get {
                if (Cache == null) { Cache = new BoxAndWhiskersCache(Items); }
                return Cache.UpperFence;
            }
        }


        /// <summary>
        /// Gets lower outer fence used to isolate extremes.
        /// Also known as Q1 - 3 * IQR.
        /// </summary>
        public double LowerOuterFence {
            get {
                if (Cache == null) { Cache = new BoxAndWhiskersCache(Items); }
                return Cache.LowerOuterFence;
            }
        }

        /// <summary>
        /// Gets upper outer fence used to isolate extremes.
        /// Also known as Q3 + 3 * IQR.
        /// </summary>
        public double UpperOuterFence {
            get {
                if (Cache == null) { Cache = new BoxAndWhiskersCache(Items); }
                return Cache.UpperOuterFence;
            }
        }


        /// <summary>
        /// Gets minimum value excluding outliers.
        /// </summary>
        public double MinimumNonOutlier {
            get {
                if (Cache == null) { Cache = new BoxAndWhiskersCache(Items); }
                return Cache.MinimumNonOutlier;
            }
        }

        /// <summary>
        /// Gets maximum value excluding outliers.
        /// </summary>
        public double MaximumNonOutlier {
            get {
                if (Cache == null) { Cache = new BoxAndWhiskersCache(Items); }
                return Cache.MaximumNonOutlier;
            }
        }


        /// <summary>
        /// Enumerates lower outliers.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<double> EnumerateLowerOutliers() {
            if (Cache == null) { Cache = new BoxAndWhiskersCache(Items); }
            return Cache.EnumerateLowerOutliers();
        }

        /// <summary>
        /// Enumerates upper outliers.
        /// </summary>
        public IEnumerable<double> EnumerateUpperOutliers() {
            if (Cache == null) { Cache = new BoxAndWhiskersCache(Items); }
            return Cache.EnumerateUpperOutliers();
        }

        /// <summary>
        /// Enumerates all outliers.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<double> EnumerateOutliers() {
            foreach (var value in EnumerateLowerOutliers()) {
                yield return value;
            }
            foreach (var value in EnumerateUpperOutliers()) {
                yield return value;
            }
        }


        /// <summary>
        /// Enumerates lower extremes.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<double> EnumerateLowerExtremes() {
            if (Cache == null) { Cache = new BoxAndWhiskersCache(Items); }
            return Cache.EnumerateLowerExtremes();
        }

        /// <summary>
        /// Enumerates upper extremes.
        /// </summary>
        public IEnumerable<double> EnumerateUpperExtremes() {
            if (Cache == null) { Cache = new BoxAndWhiskersCache(Items); }
            return Cache.EnumerateUpperExtremes();
        }

        /// <summary>
        /// Enumerates all extremes.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<double> EnumerateExtremes() {
            foreach (var value in EnumerateLowerExtremes()) {
                yield return value;
            }
            foreach (var value in EnumerateUpperExtremes()) {
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
            Items.Add(item);
            if (double.IsInfinity(item) || double.IsNaN(item)) { throw new ArgumentOutOfRangeException(nameof(item), "Must use a real number."); }
            Cache = null;
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
            Items.AddRange(items);
            Cache = null;
        }

        /// <summary>
        /// Removes all items from the collection.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "While this class offers ICollection interface, it is not a collection as such.")]
        void ICollection<double>.Clear() {
            Items.Clear();
            Cache = null;
        }

        /// <summary>
        /// Determines whether the collection contains a specific value.
        /// </summary>
        /// <param name="item">The number to locate in the collection.</param>
        /// <returns>True if item is found in the collection; otherwise, false.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "While this class offers ICollection interface, it is not a collection as such.")]
        bool ICollection<double>.Contains(double item) {
            return Items.Contains(item);
        }

        /// <summary>
        /// Copies the elements of the collection to an array, starting at a particular array index.
        /// </summary>
        /// <param name="array">The one-dimensional array that is the destination of the elements copied from collection. The array must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "While this class offers ICollection interface, it is not a collection as such.")]
        void ICollection<double>.CopyTo(double[] array, int arrayIndex) {
            Items.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets the number of elements contained in the collection.
        /// </summary>
        public int Count {
            get { return Items.Count; }
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
            if (Items.Remove(item)) {
                Cache = null;
                return true;
            } else {
                return false;
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        public IEnumerator<double> GetEnumerator() {
            if (Cache == null) { Cache = new BoxAndWhiskersCache(Items); } //to sort items (by cache)
            return Items.GetEnumerator();
        }

        /// <summary>
        /// Exposes the enumerator, which supports a simple iteration over a non-generic collection.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        #endregion


        private class BoxAndWhiskersCache {

            internal BoxAndWhiskersCache(List<double> items) {
                items.Sort();

                if (items.Count < 5) { throw new InvalidOperationException("Must have at least five values."); }

                Items = items;

                var iMedianU = items.Count / 2;
                var iMedianL = (items.Count % 2 == 0) ? iMedianU - 1 : iMedianU;

                var iLowerSubmedianU = iMedianU / 2;
                var iLowerSubmedianL = (iMedianU % 2 == 0) ? iLowerSubmedianU - 1 : iLowerSubmedianU;

                var iUpperSubmedianU = iMedianL + iMedianU / 2 + 1;
                var iUpperSubmedianL = ((iMedianU % 2 == 0) ? iUpperSubmedianU - 1 : iUpperSubmedianU);

                Minimum = items[0];
                LowerSubmedian = (items[iLowerSubmedianL] + items[iLowerSubmedianU]) / 2;
                Median = (items[iMedianL] + items[iMedianU]) / 2;
                UpperSubmedian = (items[iUpperSubmedianL] + items[iUpperSubmedianU]) / 2;
                Maximum = items[items.Count - 1];

                InterquartileRange = UpperSubmedian - LowerSubmedian;

                LowerFence = LowerSubmedian - 1.5 * InterquartileRange;
                UpperFence = UpperSubmedian + 1.5 * InterquartileRange;
                LowerOuterFence = LowerSubmedian - 3.0 * InterquartileRange;
                UpperOuterFence = UpperSubmedian + 3.0 * InterquartileRange;
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
                    return Items[MinimumNonOutlierIndex];
                }
            }

            internal double MaximumNonOutlier {
                get {
                    CalculateOutlierIndices();
                    return Items[MaximumNonOutlierIndex];
                }
            }


            internal IEnumerable<double> EnumerateLowerExtremes() {
                for (var i = 0; i < MinimumNonExtremeIndex; i++) {
                    yield return Items[i];
                }
            }

            internal IEnumerable<double> EnumerateUpperExtremes() {
                for (var i = MaximumNonExtremeIndex + 1; i < Items.Count; i++) {
                    yield return Items[i];
                }
            }


            internal IEnumerable<double> EnumerateLowerOutliers() {
                for (var i = 0; i < MinimumNonOutlierIndex; i++) {
                    yield return Items[i];
                }
            }

            internal IEnumerable<double> EnumerateUpperOutliers() {
                for (var i = MaximumNonOutlierIndex + 1; i < Items.Count; i++) {
                    yield return Items[i];
                }
            }


            private bool CalculatedOutlierIndices;
            private int MinimumNonOutlierIndex;
            private int MinimumNonExtremeIndex;
            private int MaximumNonOutlierIndex;
            private int MaximumNonExtremeIndex;

            private void CalculateOutlierIndices() {
                if (CalculatedOutlierIndices) { return; }

                {
                    int i = 0;
                    for (; i < Items.Count; i++) {
                        if (Items[i] >= LowerOuterFence) {
                            MinimumNonExtremeIndex = i;
                            break;
                        }
                    }
                    for (; i < Items.Count; i++) {
                        if (Items[i] >= LowerFence) {
                            MinimumNonOutlierIndex = i;
                            break;
                        }
                    }
                }

                {
                    int i = Items.Count - 1;
                    for (; i >= 0; i--) {
                        if (Items[i] <= UpperOuterFence) {
                            MaximumNonExtremeIndex = i;
                            break;
                        }
                    }
                    for (; i >= 0; i--) {
                        if (Items[i] <= UpperFence) {
                            MaximumNonOutlierIndex = i;
                            break;
                        }
                    }
                }

                CalculatedOutlierIndices = true;
            }

        }

    }
}
