//Josip Medved <jmedved@jmedved.com>  http://www.jmedved.com  http://blog.jmedved.com

//2010-04-07: First version.


using System;
using System.Collections.Generic;

namespace Medo.Collections.Generic {

    /// <summary>
    /// Represents a collection of keys and values.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    public class ReadOnlyDictionary<TKey, TValue> : IDictionary<TKey, TValue> {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="baseDictionary">Source dictionary.</param>
        public ReadOnlyDictionary(Dictionary<TKey, TValue> baseDictionary)
            : this(baseDictionary, false) {
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="baseDictionary">Source dictionary.</param>
        /// <param name="deepCopy">If true, all data will be copied to private instance. This will prevent changes to underlying dictionary to affect this instance.</param>
        public ReadOnlyDictionary(Dictionary<TKey, TValue> baseDictionary, bool deepCopy) {
            if (deepCopy) {
                var newDictionary = new Dictionary<TKey, TValue>();
                foreach (var item in baseDictionary) {
                    newDictionary.Add(item.Key, item.Value);
                }
                this.BaseDictionary = newDictionary;
            } else {
                this.BaseDictionary = baseDictionary;
            }
        }


        private IDictionary<TKey, TValue> BaseDictionary { get; set; }


        #region IDictionary<TKey,TValue> Members

        /// <summary>
        /// Adds an element with the provided key and value to dictionary.
        /// Not supported!
        /// </summary>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        /// <exception cref="System.NotSupportedException">Dictionary is read-only.</exception>
        public void Add(TKey key, TValue value) {
            throw new NotSupportedException("Dictionary is read-only.");
        }

        /// <summary>
        /// Determines whether dictionary contains an element with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in dictionary.</param>
        public bool ContainsKey(TKey key) {
            return this.BaseDictionary.ContainsKey(key);
        }

        /// <summary>
        /// Gets collection containing the keys of dictionary.
        /// </summary>
        public ICollection<TKey> Keys {
            get { return this.BaseDictionary.Keys; }
        }

        /// <summary>
        /// Removes the element with the specified key from dictionary.
        /// Not supported!
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        public bool Remove(TKey key) {
            throw new NotSupportedException("Dictionary is read-only.");
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
        public bool TryGetValue(TKey key, out TValue value) {
            return this.BaseDictionary.TryGetValue(key, out value);
        }

        /// <summary>
        /// Gets collection containing the values in dictionary.
        /// </summary>
        public ICollection<TValue> Values {
            get { return this.BaseDictionary.Values; }
        }

        /// <summary>
        /// Gets or sets the element with the specified key.
        /// Set operation is not supported!
        /// </summary>
        /// <param name="key">The key of the element to get or set.</param>
        /// <exception cref="System.NotSupportedException">Dictionary is read-only.</exception>
        public TValue this[TKey key] {
            get { return this.BaseDictionary[key]; }
            set { throw new NotSupportedException("Dictionary is read-only."); }
        }

        #endregion

        #region ICollection<KeyValuePair<TKey,TValue>> Members

        /// <summary>
        /// Adds an item to dictionary.
        /// Not supported!
        /// </summary>
        /// <param name="item">The object to add to collection.</param>
        /// <exception cref="System.NotSupportedException">Dictionary is read-only.</exception>
        public void Add(KeyValuePair<TKey, TValue> item) {
            throw new NotSupportedException("Dictionary is read-only.");
        }

        /// <summary>
        /// Removes all items from dictionary.
        /// Not supported!
        /// </summary>
        /// <exception cref="System.NotSupportedException">Dictionary is read-only.</exception>
        public void Clear() {
            throw new NotSupportedException("Dictionary is read-only.");
        }

        /// <summary>
        /// Determines whether collection contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in collection.</param>
        public bool Contains(KeyValuePair<TKey, TValue> item) {
            return this.BaseDictionary.Contains(item);
        }

        /// <summary>
        /// Copies the elements of collection to array, starting at a particular index.
        /// </summary>
        /// <param name="array">The one-dimensional System.Array that is the destination of the elements copied from collection. Array must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
            this.BaseDictionary.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets the number of elements contained in collection.
        /// </summary>
        public int Count {
            get { return this.BaseDictionary.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether collection is read-only.
        /// </summary>
        public bool IsReadOnly {
            get { return true; }
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from collection.
        /// Not supported!
        /// </summary>
        /// <param name="item">The object to remove from collection.</param>
        /// <exception cref="System.NotSupportedException">Dictionary is read-only.</exception>
        public bool Remove(KeyValuePair<TKey, TValue> item) {
            throw new NotSupportedException("Dictionary is read-only.");
        }

        #endregion

        #region IEnumerable<KeyValuePair<TKey,TValue>> Members

        /// <summary>
        /// Exposes the enumerator, which supports a simple iteration over a collection of a specified type.
        /// </summary>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
            return this.BaseDictionary.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Exposes the enumerator, which supports a simple iteration over a non-generic collection.
        /// </summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return this.BaseDictionary.GetEnumerator();
        }

        #endregion

    }


}
