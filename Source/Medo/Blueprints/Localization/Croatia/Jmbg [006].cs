/* Josip Medved <jmedved@jmedved.com> http://www.jmedved.com */

//2008-02-15: New version.
//2008-04-11: Cleaned code to match FxCop 1.36 beta 2 (NestedTypesShouldNotBeVisible).
//            Added Republic of Kosovo.
//2008-05-31: Added IsBirthDateValid
//2008-11-05: Decreased cyclomatic complexity.
//2009-01-05: Added support for OIB.
//            Obsoleted constructor with JMBG only in order to ease transition.
//2009-01-09: Added IsValidJmbg method.


using System;

namespace Medo.Localization.Croatia {

    /// <summary>
    /// Handling JMBG/OIB data.
    /// </summary>
    public class Jmbg {

        /// <summary>
        /// Creates new instance based on given JMBG.
        /// </summary>
        /// <param name="value">JMBG.</param>
        /// <remarks>All JMBG's with year digits lower than 800 will be considered as year 2xxx.</remarks>
        [Obsolete("Please use overload that specifies wheter OIB is to be parsed also.")]
        public Jmbg(string value) : this(value, false) { }

        /// <summary>
        /// Creates new instance based on given JMBG/OIB.
        /// </summary>
        /// <param name="value">JMBG/OIB.</param>
        /// <param name="parseOib">If true, OIB is also parsed.</param>
        /// <remarks>All JMBG's with year digits lower than 800 will be considered as year 2xxx.</remarks>
        public Jmbg(string value, bool parseOib) {
            this.Value = value;

            if (parseOib && (value.Length == 11)) { //this is OIB
                int sum = 10;
                for (int i = 0; i < 10; ++i) {
                    if ((value[i] >= '0') && (value[i] <= '9')) {
                        sum += (value[i] - '0');
                        if (sum > 10) { sum -= 10; }
                        sum *= 2;
                        if (sum >= 11) { sum -= 11; }
                    } else {
                        this.IsValid = false;
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
                if (value[10] == checkDigit) {
                    this.IsValid = true;
                    this.IsBirthDateValid = false;
                    this.Region = JmbgRegion.Unknown;
                    this.Gender = JmbgGender.Unknown;
                } else {
                    this.IsValid = false;
                }
                return;
            }

            try {
                if (value.Length >= 7) { //extract date
                    int birthDay = int.Parse(value.Substring(0, 2), System.Globalization.CultureInfo.InvariantCulture);
                    int birthMonth = int.Parse(value.Substring(2, 2), System.Globalization.CultureInfo.InvariantCulture);
                    int birthYear = int.Parse(value.Substring(4, 3), System.Globalization.CultureInfo.InvariantCulture) + 1000;
                    if (birthYear < 1800) { birthYear += 1000; }

                    try {
                        DateTime birthDate = new DateTime(birthYear, birthMonth, birthDay);
                        if ((birthDate.ToString("ddMM", System.Globalization.CultureInfo.InvariantCulture) + birthDate.ToString("yyyy", System.Globalization.CultureInfo.InvariantCulture).Remove(0, 1)) != value.Substring(0, 7)) { //date is invalid
                            return;
                        }
                        this._birthDate = birthDate;
                        this.IsBirthDateValid = birthDate <= DateTime.Today;
                    } catch (System.ArgumentOutOfRangeException) { //date is invalid
                        return;
                    }
                }

                if (value.Length != 13) { //invalid length
                    return;
                }


                int[] digits = new int[13];

                for (int i = 0; i < value.Length; i++) {
                    if (char.IsDigit(value[i])) {
                        digits[i] = int.Parse(value.Substring(i, 1), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);
                    } else { //invalid characters
                        return;
                    }
                }


                int[] mask = new int[] { 7, 6, 5, 4, 3, 2, 7, 6, 5, 4, 3, 2 };
                int sum = 0;
                for (int i = 0; i < 12; i++) {
                    sum += digits[i] * mask[i];
                }

                int checksum;
                switch (sum % 11) {
                    case 0:
                        checksum = 0;
                        break;
                    case 1: //invalid number
                        return;
                    default:
                        checksum = 11 - sum % 11;
                        break;
                }
                if (checksum != digits[12]) { //checksum mismatch
                    return;
                }


                string regionDigits = value.Substring(7, 2);
                if (regionDigits == "03") {
                    this._region = JmbgRegion.Foreign;
                } else if (regionDigits.StartsWith("1", StringComparison.Ordinal)) {
                    this._region = JmbgRegion.BosniaAndHerzegovina;
                } else if (regionDigits.StartsWith("2", StringComparison.Ordinal)) {
                    this._region = JmbgRegion.Montenegro;
                } else if (regionDigits.StartsWith("3", StringComparison.Ordinal)) {
                    this._region = JmbgRegion.Croatia;
                } else if (regionDigits.StartsWith("4", StringComparison.Ordinal)) {
                    this._region = JmbgRegion.Macedonia;
                } else if (regionDigits.StartsWith("5", StringComparison.Ordinal)) {
                    this._region = JmbgRegion.Slovenia;
                } else if (regionDigits.StartsWith("7", StringComparison.Ordinal)) {
                    this._region = JmbgRegion.Serbia;
                } else if (regionDigits.StartsWith("8", StringComparison.Ordinal)) {
                    this._region = JmbgRegion.SerbiaVojvodina;
                } else if (regionDigits.StartsWith("9", StringComparison.Ordinal)) {
                    this._region = JmbgRegion.RepublicOfKosovo;
                } else {
                    return;
                }


                if (int.Parse(value.Substring(9, 3), System.Globalization.CultureInfo.InvariantCulture) < 500) {
                    this._gender = JmbgGender.Male;
                } else {
                    this._gender = JmbgGender.Female;
                }
            } catch (FormatException) {
                return;
            }

            this._isValid = true;
        }


        /// <summary>
        /// Returns JMBG/OIB.
        /// </summary>
        public string Value { get; private set; }

        private DateTime _birthDate = DateTime.MinValue;
        /// <summary>
        /// Returns birth date.
        /// </summary>
        public DateTime BirthDate {
            get { return this._birthDate; }
        }

        private JmbgRegion _region = JmbgRegion.Unknown;
        /// <summary>
        /// Returns region.
        /// </summary>
        public JmbgRegion Region {
            get { return this._region; }
            private set { this._region = value; }
        }

        private JmbgGender _gender = JmbgGender.Unknown;
        /// <summary>
        /// Returns gender.
        /// </summary>
        public JmbgGender Gender {
            get { return this._gender; }
            private set { this._gender = value; }
        }

        private bool _isValid;
        /// <summary>
        /// Returns true if JMBG/OIB is valid.
        /// </summary>
        public bool IsValid {
            get { return this._isValid; }
            private set { this._isValid = value; }
        }

        /// <summary>
        /// Returns true if birth date part is valid.
        /// </summary>
        public bool IsBirthDateValid { get; private set; }

        /// <summary>
        /// Returns true if given valid JMBG.
        /// </summary>
        /// <param name="jmbg">JMBG to check.</param>
        /// <param name="parseOib">If true, OIB is also allowed.</param>
        public static bool IsValidJmbg(string jmbg, bool parseOib) {
            var instance = new Jmbg(jmbg, parseOib);
            return instance.IsValid;
        }

        /// <summary>
        /// Returns JMBG/OIB if one is valid.
        /// </summary>
        public override string ToString() {
            if (this.IsValid) {
                return this.Value;
            } else {
                if (this.IsBirthDateValid) {
                    return this.BirthDate.ToString("ddMM", System.Globalization.CultureInfo.InvariantCulture) + (this.BirthDate.Year % 1000).ToString("000", System.Globalization.CultureInfo.InvariantCulture);
                } else {
                    return string.Empty;
                }
            }
        }

    }



    /// <summary>
    /// Gender information.
    /// </summary>
    public enum JmbgGender {
        /// <summary>
        /// Unknown gender.
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Male gender.
        /// </summary>
        [System.ComponentModel.Description("Male")]
        Male = 1,
        /// <summary>
        /// Female gender.
        /// </summary>
        [System.ComponentModel.Description("Female")]
        Female = 2,
    }



    /// <summary>
    /// Region information.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1027:MarkEnumsWithFlags", Justification = "These are not flags.")]
    public enum JmbgRegion {
        /// <summary>
        /// Unknown region.
        /// </summary>
        [System.ComponentModel.Description("Unknown")]
        Unknown = 0,
        /// <summary>
        /// Bosnia and Herzegovina.
        /// </summary>
        [System.ComponentModel.Description("Bosnia and Herzegovina")]
        BosniaAndHerzegovina = 1,
        /// <summary>
        /// Montenegro.
        /// </summary>
        [System.ComponentModel.Description("Montenegro")]
        Montenegro = 2,
        /// <summary>
        /// Croatia.
        /// </summary>
        [System.ComponentModel.Description("Croatia")]
        Croatia = 3,
        /// <summary>
        /// Former Yugoslav Republic of Macedonia.
        /// </summary>
        [System.ComponentModel.Description("Macedonia")]
        Macedonia = 4,
        /// <summary>
        /// Slovenia.
        /// </summary>
        [System.ComponentModel.Description("Slovenia")]
        Slovenia = 5,
        /// <summary>
        /// Serbia.
        /// </summary>
        [System.ComponentModel.Description("Serbia")]
        Serbia = 7,
        /// <summary>
        /// Vojvodina (Serbia).
        /// </summary>
        [System.ComponentModel.Description("Vojvodina (Serbia)")]
        SerbiaVojvodina = 8,
        /// <summary>
        /// Kosovo (Serbia).
        /// </summary>
        [System.ComponentModel.Description("Kosovo (Serbia)")]
        [Obsolete("In February 2008, Kosovo declared the territory's independence as the Republic of Kosovo. Please use RepublicOfKosovo instead.")]
        SerbiaKosovo = 9,
        /// <summary>
        /// Republic of Kosovo.
        /// </summary>
        [System.ComponentModel.Description("Republic of Kosovo")]
        RepublicOfKosovo = 9,
        /// <summary>
        /// Foreign.
        /// </summary>
        [System.ComponentModel.Description("Foreign")]
        Foreign = 10
    }

}
