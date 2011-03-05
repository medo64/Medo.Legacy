//Josip Medved <jmedved@jmedved.com>  http://www.jmedved.com  http://blog.jmedved.com

//2011-05-05: Initial version.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Medo.Math {

    /// <summary>
    /// Statistical calculations for process capability.
    /// </summary>
    public class ProcessCapability : ICollection<double> {

        private readonly List<double> Items = new List<double>();

        /// <summary>
        /// Creates new instance.
        /// </summary>
        public ProcessCapability() {
        }

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="lowerLimit">Lower specification limit.</param>
        /// <param name="upperLimit">Upper specification limit.</param>
        public ProcessCapability(double lowerLimit, double upperLimit) {
            this.LowerLimit = lowerLimit;
            this.UpperLimit = upperLimit;
        }


        /// <summary>
        /// Gets lower specification limit.
        /// </summary>
        public double? LowerLimit { get; private set; }

        /// <summary>
        /// Gets upper specification limit.
        /// </summary>
        public double? UpperLimit { get; private set; }


        /// <summary>
        /// Returns mean value of all items of Double.NaN if there are no items.
        /// </summary>
        public double Mean {
            get {
                if (this.Items.Count > 0) {
                    double sum = 0;
                    for (int i = 0; i < this.Items.Count; i++) {
                        sum += this.Items[i];
                    }
                    return sum / this.Items.Count;
                } else {
                    return double.NaN;
                }
            }
        }

        /// <summary>
        /// Returns standard deviation (σ) of all items of Double.NaN if there are no items.
        /// </summary>
        public double StDev {
            get {
                if (this.Items.Count > 0) {
                    double mean = this.Mean;
                    double diffSum = 0;
                    for (int i = 0; i < this.Items.Count; i++) {
                        diffSum += System.Math.Pow(this.Items[i] - mean, 2);
                    }
                    return System.Math.Sqrt((1.0 / this.Items.Count) * diffSum);
                } else {
                    return double.NaN;
                }
            }
        }


        #region IList

        /// <summary>
        /// Adds an item to the collection.
        /// </summary>
        /// <param name="item">The number to add.</param>
        public void Add(double item) {
            this.Items.Add(item);
        }

        /// <summary>
        /// Removes all items from the collection.
        /// </summary>
        public void Clear() {
            this.Items.Clear();
        }

        /// <summary>
        /// Determines whether the collection contains a specific value.
        /// </summary>
        /// <param name="item">The number to locate in the collection.</param>
        /// <returns>True if item is found in the collection; otherwise, false.</returns>
        public bool Contains(double item) {
            return this.Items.Contains(item);
        }

        /// <summary>
        /// Copies the elements of the collection to an array, starting at a particular array index.
        /// </summary>
        /// <param name="array">The one-dimensional array that is the destination of the elements copied from collection. The array must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo(double[] array, int arrayIndex) {
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
        public bool IsReadOnly {
            get { return false; }
        }

        /// <summary>
        /// Removes the first occurrence of a specific numver from the collection.
        /// </summary>
        /// <param name="item">The number to remove from the collection.</param>
        /// <returns>True if item was successfully removed from the collection; otherwise, false. This method also returns false if item is not found in the original collection.</returns>
        public bool Remove(double item) {
            return this.Items.Remove(item);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        public IEnumerator<double> GetEnumerator() {
            return this.Items.GetEnumerator();
        }

        /// <summary>
        /// Exposes the enumerator, which supports a simple iteration over a non-generic collection.
        /// </summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return this.Items.GetEnumerator();
        }

        #endregion

    }
}
