/* Josip Medved <jmedved@jmedved.com> * www.medo64.com * MIT License */

//2009-01-09: Added IsValidOib method.
//2009-01-05: Initial version.


using System;

namespace Medo.Localization.Croatia {

    /// <summary>
    /// Handling OIB data.
    /// </summary>
    public class Oib {

        /// <summary>
        /// Creates new instance based on given OIB.
        /// </summary>
        /// <param name="oib">OIB.</param>
        /// <exception cref="System.ArgumentNullException">Parameter cannot be null.</exception>
        public Oib(string oib) {
            Value = oib ?? throw new ArgumentNullException("oib", "Parameter cannot be null.");

            if (oib.Length != 11) {
                IsValid = false;
                return;
            }

            int sum = 10;
            for (int i = 0; i < 10; ++i) {
                if ((oib[i] >= '0') && (oib[i] <= '9')) {
                    sum += (oib[i] - '0');
                    if (sum > 10) { sum -= 10; }
                    sum *= 2;
                    if (sum >= 11) { sum -= 11; }
                } else {
                    IsValid = false;
                    return;
                }
            }
            char checkDigit;
            int sum2 = 11 - sum;
            if (sum2 == 10) {
                checkDigit = '0';
            } else {
                checkDigit = System.Convert.ToChar('0' + sum2);
            }
            IsValid = (oib[10] == checkDigit);
        }


        /// <summary>
        /// Returns OIB.
        /// </summary>
        public string Value { get; private set; }


        /// <summary>
        /// Returns true if OIB is valid.
        /// </summary>
        public bool IsValid { get; private set; }


        /// <summary>
        /// Returns true if given valid OIB.
        /// </summary>
        /// <param name="oib">OIB to check.</param>
        /// <exception cref="System.ArgumentNullException">Parameter cannot be null.</exception>
        public static bool IsValidOib(string oib) {
            var instance = new Oib(oib);
            return instance.IsValid;
        }


        /// <summary>
        /// Returns OIB if one is valid.
        /// </summary>
        public override string ToString() {
            if (IsValid) {
                return Value;
            } else {
                return string.Empty;
            }
        }

    }

}
