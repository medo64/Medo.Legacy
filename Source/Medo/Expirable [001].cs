/* Josip Medved <jmedved@jmedved.com> * www.medo64.com * MIT License */

//2015-03-14: First version.


using System;
using System.Collections.Generic;

namespace Medo {

    /// <summary>
    /// Represents a value that will reset itself after preset amount of time.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    public class Expirable<T> {

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="lifetime">Value lifetime.</param>
        /// <param name="value">Value.</param>
        /// <param name="defaultValue">Default value.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Lifetime cannot be zero or negative.</exception>
        public Expirable(TimeSpan lifetime, T value, T defaultValue) {
            if (lifetime.TotalMilliseconds <= 0) { throw new ArgumentOutOfRangeException("lifetime", "Lifetime cannot be zero or negative."); }
            Lifetime = lifetime;
            Value = value;
            DefaultValue = defaultValue;
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="lifetime">Value lifetime.</param>
        /// <param name="value">Value.</param>
        public Expirable(TimeSpan lifetime, T value)
            : this(lifetime, value, default(T)) {
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="lifetimeInSeconds">Value lifetime in seconds.</param>
        /// <param name="value">Value.</param>
        public Expirable(int lifetimeInSeconds, T value)
            : this(new TimeSpan(0, 0, lifetimeInSeconds), value) {
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="lifetime">Value lifetime.</param>
        public Expirable(TimeSpan lifetime)
            : this(lifetime, default(T), default(T)) {
            ExpireTime = DateTime.UtcNow; //expire it immediatelly since value was never set.
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="lifetimeInSeconds">Value lifetime in seconds.</param>
        public Expirable(int lifetimeInSeconds)
            : this(new TimeSpan(0, 0, lifetimeInSeconds)) {
        }


        /// <summary>
        /// Gets value lifetime.
        /// </summary>
        public TimeSpan Lifetime { get; private set; }

        /// <summary>
        /// Gets default value.
        /// </summary>
        public T DefaultValue { get; private set; }

        /// <summary>
        /// Gets expiration time for current value.
        /// </summary>
        public DateTime ExpireTime { get; private set; }


        private T _value;
        /// <summary>
        /// Gets/sets value.
        /// If value has expired, default value will be returned.
        /// When value is set, expiry time will reset.
        /// </summary>
        public T Value {
            get {
                if (DateTime.UtcNow < ExpireTime) {
                    return _value;
                } else {
                    return DefaultValue;
                }
            }
            set {
                _value = value;
                ExpireTime = DateTime.UtcNow.Add(Lifetime);
            }
        }

        /// <summary>
        /// Gets if non-expired value is present.
        /// </summary>
        public bool HasValue {
            get {
                return (DateTime.UtcNow < ExpireTime);
            }
        }

        /// <summary>
        /// Returns if value can be retrieved.
        /// </summary>
        /// <param name="value">Value if not expired.</param>
        public bool TryGet(out T value) {
            if (DateTime.UtcNow < ExpireTime) {
                value = _value;
                return true;
            } else {
                value = DefaultValue;
                return false;
            }
        }

        /// <summary>
        /// Immediately expires value.
        /// </summary>
        public void Expire() {
            ExpireTime = DateTime.UtcNow;
        }


        #region Overrides

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">Object to compare.</param>
        public override bool Equals(object obj) {
            if (obj is Expirable<T> other) {
                var value = Value;
                var otherValue = other.Value;
                return object.Equals(value, otherValue);
            } else {
                var value = Value;
                return object.Equals(value, obj);
            }
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() {
            var value = Value;
            return (value != null) ? value.GetHashCode() : 0;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        public override string ToString() {
            var value = Value;
            return (value != null) ? value.ToString() : string.Empty;
        }

        #endregion


        #region Operators

        /// <summary>
        /// Returns value from expirable.
        /// </summary>
        /// <param name="expirable">Expirable.</param>
        /// <exception cref="System.ArgumentNullException">Expirable cannot be null.</exception>
        public static implicit operator T(Expirable<T> expirable) {
            return (expirable != null) ? expirable.Value : default(T);
        }

        #endregion

    }
}
