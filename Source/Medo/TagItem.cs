//Josip Medved <jmedved@jmedved.com>   www.medo64.com

//2008-04-11: Cleaned code to match FxCop 1.36 beta 2 (IdentifiersShouldHaveCorrectPrefix).
//2008-03-13: Added comparision of key to Equals.
//2008-02-16: Fixed bug with null in ToString().
//2007-11-01: New version.


namespace Medo {

	/// <summary>
	/// Defines a key/value pair that can be set or retrieved.
	/// </summary>
	/// <typeparam name="TKey">Key.</typeparam>
	/// <typeparam name="TValue">Value.</typeparam>
	public struct TagItem<TKey, TValue> {

		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		/// <param name="key">Key.</param>
		/// <param name="value">Value.</param>
		public TagItem(TKey key, TValue value)
			: this(key, value, null) {
		}

		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		/// <param name="key">Key.</param>
		/// <param name="value">Value.</param>
		/// <param name="tag">Additional object.</param>
		public TagItem(TKey key, TValue value, object tag) {
			this._key = key;
			this._value = value;
			this._tag = tag;
		}


		private TKey _key;
		private TValue _value;
		private object _tag;


		/// <summary>
		/// Gets key.
		/// </summary>
		public TKey Key {
			get { return this._key; }
		}

		/// <summary>
		/// Gets value.
		/// </summary>
		public TValue Value {
			get { return this._value; }
			set { this._value = value; }
		}

		/// <summary>
		/// Gets/sets value of tag.
		/// </summary>
		public object Tag {
			get { return this._tag; }
			set { this._tag = value; }
		}


		/// <summary>
		/// Returns a value indicating whether this instance's Key is equal to the specified TagItem object's Key.
		/// </summary>
		/// <param name="obj">A TagItem object to compare to this instance.</param>
		public override bool Equals(object obj) {
			if (obj is TagItem<TKey, TValue>){
				TagItem<TKey, TValue> other = (TagItem<TKey, TValue>)obj;
				return (this._key.Equals(other._key));
			}
			if (obj is TKey) {
				TKey otherKey = (TKey)obj;
				return (this._key.Equals(otherKey));
			}
			return false;
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		public override int GetHashCode() {
			return this.Key.GetHashCode();
		}

		/// <summary>
		/// Converts the value of this instance to its equivalent string representation.
		/// </summary>
		/// <returns>String that represents this instance.</returns>
		public override string ToString() {
			if (this.Value != null) {
				return this.Value.ToString();
			} else {
				return string.Empty;
			}
		}

		/// <summary>
		/// Returns true if both objects have same key.
		/// </summary>
		/// <param name="objA">First object.</param>
		/// <param name="objB">Second object.</param>
		public static bool operator ==(TagItem<TKey, TValue> objA, TagItem<TKey, TValue> objB) {
			return objA._key.Equals(objB._key);
		}

		/// <summary>
		/// Returns true if both objects have different key.
		/// </summary>
		/// <param name="objA">First object.</param>
		/// <param name="objB">Second object.</param>
		public static bool operator !=(TagItem<TKey, TValue> objA, TagItem<TKey, TValue> objB) {
			return !objA._key.Equals(objB._key);
		}

	}

	#region DefaultClass

	/// <summary>
	/// Defines a key/value pair that can be set or retrieved.
	/// </summary>
	public struct TagItem {

		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		/// <param name="key">Key.</param>
		/// <param name="value">Value.</param>
		public TagItem(int key, string value)
			: this(key, value, null) {
		}

		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		/// <param name="key">Key.</param>
		/// <param name="value">Value.</param>
		/// <param name="tag">Additional object.</param>
		public TagItem(int key, string value, object tag) {
			this._key = key;
			this._value = value;
			this._tag = tag;
		}


		private int _key;
		private string _value;
		private object _tag;


		/// <summary>
		/// Gets key.
		/// </summary>
		public int Key {
			get { return this._key; }
		}

		/// <summary>
		/// Gets value.
		/// </summary>
		public string Value {
			get { return this._value; }
			set { this._value = value; }
		}

		/// <summary>
		/// Gets/sets value of tag.
		/// </summary>
		public object Tag {
			get { return this._tag; }
			set { this._tag = value; }
		}


		/// <summary>
		/// Returns a value indicating whether this instance's Key is equal to the specified TagItem object's Key.
		/// </summary>
		/// <param name="obj">A TagItem object to compare to this instance.</param>
		public override bool Equals(object obj) {
			if (obj is TagItem) {
				TagItem other = (TagItem)obj;
				return (this._key.Equals(other._key));
			}
			if (obj is TagItem<int, string>) {
				TagItem<int, string> other2 = (TagItem<int, string>)obj;
				return (this._key.Equals(other2.Key));
			}
			if (obj is int) {
				int other3 = (int)obj;
				return (this._key.Equals(other3));
			}
			return false;
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		public override int GetHashCode() {
			return this.Key.GetHashCode();
		}

		/// <summary>
		/// Converts the value of this instance to its equivalent string representation.
		/// </summary>
		/// <returns>String that represents this instance.</returns>
		public override string ToString() {
			if (this.Value != null) {
				return this.Value.ToString();
			} else {
				return string.Empty;
			}
		}


		/// <summary>
		/// Returns true if both objects have same key.
		/// </summary>
		/// <param name="objA">First object.</param>
		/// <param name="objB">Second object.</param>
		public static bool operator ==(TagItem objA, TagItem objB) {
			return objA._key.Equals(objB._key);
		}

		/// <summary>
		/// Returns true if both objects have different key.
		/// </summary>
		/// <param name="objA">First object.</param>
		/// <param name="objB">Second object.</param>
		public static bool operator !=(TagItem objA, TagItem objB) {
			return !objA._key.Equals(objB._key);
		}

	}

#endregion

}