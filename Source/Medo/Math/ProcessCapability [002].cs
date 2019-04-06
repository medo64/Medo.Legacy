/* Josip Medved <jmedved@jmedved.com> * www.medo64.com * MIT License */

//2011-03-13: Added minimum/maximum.
//2011-03-05: Initial version (based on description at http://en.wikipedia.org/wiki/Process_capability_index).


using System.Collections.Generic;

namespace Medo.Math {

    /// <summary>
    /// Statistical calculations for process capability.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "While this class offers ICollection interface, it is not a collection as such.")]
    public class ProcessCapability : ICollection<double> {

        private readonly List<double> Items = new List<double>();

        /// <summary>
        /// Creates new instance.
        /// </summary>
        public ProcessCapability() {
            LowerLimit = double.NaN;
            UpperLimit = double.NaN;
            TargetMean = double.NaN;
        }

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="lowerLimit">Lower specification limit.</param>
        /// <param name="upperLimit">Upper specification limit.</param>
        public ProcessCapability(double lowerLimit, double upperLimit) {
            LowerLimit = lowerLimit;
            UpperLimit = upperLimit;
            TargetMean = double.NaN;
        }

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="lowerLimit">Lower specification limit.</param>
        /// <param name="upperLimit">Upper specification limit.</param>
        /// <param name="targetMean">Target process mean.</param>
        public ProcessCapability(double lowerLimit, double upperLimit, double targetMean) {
            LowerLimit = lowerLimit;
            UpperLimit = upperLimit;
            TargetMean = targetMean;
        }


        /// <summary>
        /// Gets lower specification limit.
        /// </summary>
        public double LowerLimit { get; private set; }

        /// <summary>
        /// Gets upper specification limit.
        /// </summary>
        public double UpperLimit { get; private set; }

        /// <summary>
        /// Gets target process mean.
        /// </summary>
        public double TargetMean { get; private set; }

        private bool _useBesselCorrection = true;
        /// <summary>
        /// Gets/sets whether Bessel's correction is used inside standard deviation calculations.
        /// Default is true.
        /// </summary>
        public bool UseBesselCorrection {
            get { return _useBesselCorrection; }
            set {
                _useBesselCorrection = value;
                _meanCache = null;
                _stDevCache = null;
                _minimumCache = null;
                _maximumCache = null;
            }
        }

        private double? _meanCache = null;
        /// <summary>
        /// Returns mean value of all items of Double.NaN if there are no items.
        /// </summary>
        public double Mean {
            get {
                if (_meanCache == null) {
                    if (Items.Count > 0) {
                        double sum = 0;
                        for (int i = 0; i < Items.Count; i++) {
                            sum += Items[i];
                        }
                        _meanCache = sum / Items.Count;
                    } else {
                        _meanCache = double.NaN;
                    }
                }
                return _meanCache.Value;
            }
        }

        private double? _stDevCache = null;
        /// <summary>
        /// Returns standard deviation (σ) of all items of Double.NaN if there are no items.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "St", Justification = "This is intended naming.")]
        public double StDev {
            get {
                if (_stDevCache == null) {
                    if (Items.Count > 0) {
                        double mean = Mean;
                        double diffSum = 0;
                        for (int i = 0; i < Items.Count; i++) {
                            diffSum += System.Math.Pow(Items[i] - mean, 2);
                        }
                        if (UseBesselCorrection) {
                            if (Items.Count > 1) {
                                _stDevCache = System.Math.Sqrt((1.0 / (Items.Count - 1)) * diffSum);
                            } else {
                                _stDevCache = double.NaN;
                            }
                        } else {
                            _stDevCache = System.Math.Sqrt((1.0 / Items.Count) * diffSum);
                        }
                    } else {
                        _stDevCache = double.NaN;
                    }
                }
                return _stDevCache.Value;
            }
        }

        private double? _minimumCache = null;
        /// <summary>
        /// Returns minimum value of all items of Double.NaN if there are no items.
        /// </summary>
        public double Minimum {
            get {
                if (_minimumCache == null) {
                    if (Items.Count > 0) {
                        double minimum = Items[0];
                        for (int i = 1; i < Items.Count; i++) {
                            if (Items[i] < minimum) {
                                minimum = Items[i];
                            }
                        }
                        _minimumCache = minimum;
                    } else {
                        _minimumCache = double.NaN;
                    }
                }
                return _minimumCache.Value;
            }
        }

        private double? _maximumCache = null;
        /// <summary>
        /// Returns maximum value of all items of Double.NaN if there are no items.
        /// </summary>
        public double Maximum {
            get {
                if (_maximumCache == null) {
                    if (Items.Count > 0) {
                        double maximum = Items[0];
                        for (int i = 1; i < Items.Count; i++) {
                            if (Items[i] > maximum) {
                                maximum = Items[i];
                            }
                        }
                        _maximumCache = maximum;
                    } else {
                        _maximumCache = double.NaN;
                    }
                }
                return _maximumCache.Value;
            }
        }

        /// <summary>
        /// Gets Ĉ(p) process capability index.
        /// Estimates what the process would be capable of producing if the process could be centered. Assumes process output is approximately normally distributed.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Cp", Justification = "This is intended naming.")]
        public double Cp {
            get {
                return (UpperLimit - LowerLimit) / (6 * StDev);
            }
        }

        /// <summary>
        /// Gets Ĉ(p,lower) process capability index.
        /// Estimates process capability for specifications that consist of a lower limit only (for example, strength). Assumes process output is approximately normally distributed.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Cp", Justification = "This is intended naming.")]
        public double CpLower {
            get {
                return (Mean - LowerLimit) / (3 * StDev);
            }
        }

        /// <summary>
        /// Gets Ĉ(p,upper) process capability index.
        /// Estimates process capability for specifications that consist of an upper limit only (for example, concentration). Assumes process output is approximately normally distributed.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Cp", Justification = "This is intended naming.")]
        public double CpUpper {
            get {
                return (UpperLimit - Mean) / (3 * StDev);
            }
        }

        /// <summary>
        /// Gets Ĉ(pk) process capability index.
        /// Estimates what the process is capable of producing if the process target is centered between the specification limits. If the process mean is not centered, Ĉ(pk) overestimates process capability. Ĉ(pk) &lt; 0 if the process mean falls outside of the specification limits. Assumes process output is approximately normally distributed.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Cpk", Justification = "This is intended naming.")]
        public double Cpk {
            get {
                return System.Math.Min((UpperLimit - Mean) / (3 * StDev), (Mean - LowerLimit) / (3 * StDev));
            }
        }

        /// <summary>
        /// Gets Ĉ(pm) process capability index.
        /// Estimates process capability around a target, T. Ĉ(pm) is always greater than zero. Assumes process output is approximately normally distributed. Ĉ(pm) is also known as the Taguchi capability index.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Cpm", Justification = "This is intended naming.")]
        public double Cpm {
            get {
                return Cp / System.Math.Sqrt(1 + System.Math.Pow((Mean - TargetMean) / StDev, 2));
            }
        }

        /// <summary>
        /// Gets Ĉ(pkm) process capability index.
        /// Estimates process capability around a target, T, and accounts for an off-center process mean. Assumes process output is approximately normally distributed.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Cpkm", Justification = "This is intended naming.")]
        public double Cpkm {
            get {
                return Cpk / System.Math.Sqrt(1 + System.Math.Pow((Mean - TargetMean) / StDev, 2));
            }
        }


        #region IList

        /// <summary>
        /// Adds an item to the collection.
        /// </summary>
        /// <param name="item">The number to add.</param>
        public void Add(double item) {
            Items.Add(item);
            _meanCache = null;
            _stDevCache = null;
            _minimumCache = null;
            _maximumCache = null;
        }

        /// <summary>
        /// Removes all items from the collection.
        /// </summary>
        public void Clear() {
            Items.Clear();
            _meanCache = null;
            _stDevCache = null;
            _minimumCache = null;
            _maximumCache = null;
        }

        /// <summary>
        /// Determines whether the collection contains a specific value.
        /// </summary>
        /// <param name="item">The number to locate in the collection.</param>
        /// <returns>True if item is found in the collection; otherwise, false.</returns>
        public bool Contains(double item) {
            return Items.Contains(item);
        }

        /// <summary>
        /// Copies the elements of the collection to an array, starting at a particular array index.
        /// </summary>
        /// <param name="array">The one-dimensional array that is the destination of the elements copied from collection. The array must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo(double[] array, int arrayIndex) {
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
        public bool IsReadOnly {
            get { return false; }
        }

        /// <summary>
        /// Removes the first occurrence of a specific numver from the collection.
        /// </summary>
        /// <param name="item">The number to remove from the collection.</param>
        /// <returns>True if item was successfully removed from the collection; otherwise, false. This method also returns false if item is not found in the original collection.</returns>
        public bool Remove(double item) {
            var res = Items.Remove(item);
            _meanCache = null;
            _stDevCache = null;
            _minimumCache = null;
            _maximumCache = null;
            return res;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        public IEnumerator<double> GetEnumerator() {
            return Items.GetEnumerator();
        }

        /// <summary>
        /// Exposes the enumerator, which supports a simple iteration over a non-generic collection.
        /// </summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return Items.GetEnumerator();
        }

        #endregion

    }
}
