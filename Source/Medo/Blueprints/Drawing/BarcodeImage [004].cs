/* Josip Medved <jmedved@jmedved.com> http://www.jmedved.com */

//2008-04-05: New version.
//2008-04-11: Refactoring.
//2008-11-05: Refactoring (Microsoft.Maintainability : 'BarcodeImage.InitCode128()' has a maintainability index of 13).
//2008-12-09: Changed Code 128 pattern 27 from 311212 to 312212.


using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;

namespace Medo.Drawing {

    /// <summary>
    /// Base for all barcode drawing.
    /// </summary>
    public class BarcodeImage {

		private List<char> _allowedValueCharacters;
		private List<char> _allowedStartCharacters;
		private List<char> _allowedEndCharacters;


        private BarcodeImage() {
            this.BarColor = Color.Black;
            this.GapColor = Color.White;
            this.SpaceColor = Color.White;
        }

        /// <summary>
        /// Returns implementation of "Codabar" barcode drawing with 'A' as both start and end character.
        /// Also known as "NW-7", "USD-4" and "Code 2 of 7".
        /// Supported characters are: 0-9 - $ : / . +.
        /// Supported start/end characters are: A-D.
        /// </summary>
        /// <param name="value">Value.</param>
        public static BarcodeImage GetNewCodabar(string value) {
            return GetNewCodabar('A', value, 'A');
        }

        /// <summary>
        /// Returns implementation of "Codabar" barcode drawing.
        /// Also known as "NW-7", "USD-4" and "Code 2 of 7".
        /// Supported characters are: 0-9 - $ : / . +.
        /// Supported start/end characters are: A-D.
        /// </summary>
        /// <param name="startCharacter">Starting character.</param>
        /// <param name="value">Value.</param>
        /// <param name="endCharacter">Ending character.</param>
        public static BarcodeImage GetNewCodabar(char startCharacter, string value, char endCharacter) {
            BarcodeImage barcode = new BarcodeImage();

            barcode.InitCodabar();
            barcode.StartCharacter = startCharacter;
            barcode.EndCharacter = endCharacter;
            barcode.Value = value;
            barcode.ValueEncoder = CodabarValueEncoder;
            barcode.ValueEncoder.Invoke(barcode);
            return barcode;
        }

        /// <summary>
        /// Returns implementation of "Code 128" barcode drawing.
        /// Supported characters are all from 7-bit ASCII.
        /// No start/end characters are supported.
        /// </summary>
        /// <param name="value">Value.</param>
        public static BarcodeImage GetNewCode128(string value) {
            BarcodeImage barcode = new BarcodeImage();

            barcode.InitCode128();
            barcode.Value = value;
            barcode.ValueEncoder = Code128ValueEncoder;
            barcode.ValueEncoder.Invoke(barcode);
            return barcode;
        }



        private delegate void ValueEncoderDelegate(BarcodeImage barcode);
        private ValueEncoderDelegate ValueEncoder { get; set; }

        private int[] EncodedValue { get; set; }


        /// <summary>
        /// Returns width of barcode.
        /// </summary>
        public float MeasureWidth() {
            float totalWidth = 0;
            for (int i = 0; i < this.EncodedValue.Length; i++) {
                switch (i % 2) {
                    case 0: //bar
                        totalWidth += this.EncodedValue[i] * this.BarWidth;
                        break;
                    case 1: //gap or space
                        if (this.EncodedValue[i] != 0) { //gap
                            totalWidth += this.EncodedValue[i] * this.GapWidth;
                        } else { //space
                            totalWidth += this.SpaceWidth;
                        }
                        break;
                }
            }
            return totalWidth;
        }

        private char _startCharacter;
        /// <summary>
        /// Gets/sets start character.
        /// </summary>
        public char StartCharacter {
            get { return this._startCharacter; }
            set {
                if (!IsStartCharacterSupported(value)) { throw new System.ArgumentOutOfRangeException("value", string.Format(CultureInfo.InvariantCulture, Resources.FormatExceptionCharacterIsNotSupported, value)); }
                this._startCharacter = value;
                if (this.ValueEncoder != null) { this.ValueEncoder.Invoke(this); }
            }
        }

        private char _endCharacter;
        /// <summary>
        /// Gets/sets start character.
        /// </summary>
        public char EndCharacter {
            get { return this._endCharacter; }
            set {
                if (!IsEndCharacterSupported(value)) { throw new System.ArgumentOutOfRangeException("value", string.Format(CultureInfo.InvariantCulture, Resources.FormatExceptionCharacterIsNotSupported, value)); }
                this._endCharacter = value;
                if (this.ValueEncoder != null) { this.ValueEncoder.Invoke(this); }
            }
        }


        private string _value;
        /// <summary>
        /// Gets/sets value.
        /// </summary>
        public string Value {
            get { return this._value; }
            set {
                if (value == null) { value = string.Empty; }
                for (int i = 0; i < value.Length; ++i) {
                    if (!IsCharacterSupported(value[i])) { throw new System.ArgumentOutOfRangeException("value", string.Format(CultureInfo.InvariantCulture, Resources.FormatExceptionCharacterIsNotSupported, value[i])); }
                }
                this._value = value;
                if (this.ValueEncoder != null) { this.ValueEncoder.Invoke(this); }
            }
        }

        /// <summary>
        /// Returns true if character is supported.
        /// </summary>
        /// <param name="value">Character to check.</param>
        public bool IsCharacterSupported(char value) {
			return _allowedValueCharacters.Contains(value);
        }

        /// <summary>
        /// Returns true if character is supported.
        /// </summary>
        /// <param name="value">Character to check.</param>
        public bool IsStartCharacterSupported(char value) {
			return _allowedStartCharacters.Contains(value);
		}

        /// <summary>
        /// Returns true if character is supported.
        /// </summary>
        /// <param name="value">Character to check.</param>
        public bool IsEndCharacterSupported(char value) {
			return _allowedEndCharacters.Contains(value);
		}

        private float _barWidth = 1;
        /// <summary>
        /// Gets/sets width of bars.
        /// </summary>
        /// <exception cref="System.ArgumentOutOfRangeException">Width must be larger than 0.</exception>
        public float BarWidth {
            get { return this._barWidth; }
            set {
                if (value <= 0) { throw new System.ArgumentOutOfRangeException("value", Resources.ExceptionWidthMustBeLargerThan0); }
                this._barWidth = value;
            }
        }

        private float _gapWidth = 1;
        /// <summary>
        /// Gets/sets width of gaps.
        /// </summary>
        /// <exception cref="System.ArgumentOutOfRangeException">Width must be larger than 0.</exception>
        public float GapWidth {
            get { return this._gapWidth; }
            set {
                if (value <= 0) { throw new System.ArgumentOutOfRangeException("value", Resources.ExceptionWidthMustBeLargerThan0); }
                this._gapWidth = value;
            }
        }

        private float _spaceWidth = 1;
        /// <summary>
        /// Gets/sets width of space.
        /// </summary>
        /// <exception cref="System.ArgumentOutOfRangeException">Width must be larger than 0.</exception>
        public float SpaceWidth {
            get { return this._spaceWidth; }
            set {
                if (value <= 0) { throw new System.ArgumentOutOfRangeException("value", Resources.ExceptionWidthMustBeLargerThan0); }
                this._spaceWidth = value;
            }
        }

        /// <summary>
        /// Sets width of bars, gaps and spaces.
        /// </summary>
        /// <exception cref="System.ArgumentOutOfRangeException">Width must be larger than 0.</exception>
        public void SetElementWidth(float value) {
            if (value <= 0) { throw new System.ArgumentOutOfRangeException("value", Resources.ExceptionWidthMustBeLargerThan0); }
            this._barWidth = value;
            this._gapWidth = value;
            this._spaceWidth = value;
        }

        /// <summary>
        /// Gets/sets color of bars.
        /// </summary>
        public Color BarColor { get; set; }

        /// <summary>
        /// Gets/sets color of gaps.
        /// </summary>
        public Color GapColor { get; set; }

        /// <summary>
        /// Gets/sets color of spaces.
        /// </summary>
        public Color SpaceColor { get; set; }


        /// <summary>
        /// Paints barcode on given Graphics surface.
        /// </summary>
        /// <param name="graphics">Graphics.</param>
        /// <param name="left">Starting x coordinate.</param>
        /// <param name="top">Starting y coordinate.</param>
        /// <param name="height">Height.</param>
        public void Paint(Graphics graphics, float left, float top, float height) {
            if (graphics == null) { return; }
            InterpolationMode oldInterpolationMode = graphics.InterpolationMode;
            SmoothingMode oldSmoothingMode = graphics.SmoothingMode;

            graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            graphics.SmoothingMode = SmoothingMode.None;

            try {
                float currLeft = left;

                using (SolidBrush barBrush = new SolidBrush(this.BarColor))
                using (SolidBrush gapBrush = new SolidBrush(this.GapColor))
                using (SolidBrush spaceBrush = new SolidBrush(this.SpaceColor)) {

                    for (int i = 0; i < this.EncodedValue.Length; i++) {
                        switch (i % 2) {
                            case 0: { //bar
                                    int multiplier = this.EncodedValue[i];
                                    graphics.FillRectangle(barBrush, currLeft, top, multiplier * this.BarWidth, height);
                                    currLeft += multiplier * this.BarWidth;
                                    break;
                                }
                            case 1: { //gap or space
                                    int multiplier = this.EncodedValue[i];
                                    if (multiplier != 0) { //gap
                                        graphics.FillRectangle(gapBrush, currLeft, top, multiplier * this.GapWidth, height);
                                        currLeft += multiplier * this.GapWidth;
                                    } else { //space
                                        graphics.FillRectangle(gapBrush, currLeft, top, this.SpaceWidth, height);
                                        currLeft += this.SpaceWidth;
                                    }
                                }
                                break;
                        }
                    }

                }
            } finally {
                graphics.InterpolationMode = oldInterpolationMode;
                graphics.SmoothingMode = oldSmoothingMode;
            }
        }


        #region Codabar

		private Dictionary<char, int[]> _codabarValueEncoding;
		private Dictionary<char, int[]> _codabarStartStopEncoding;

        private void InitCodabar() {
			this._allowedStartCharacters = new List<char>();
			this._allowedStartCharacters.Add('A');
			this._allowedStartCharacters.Add('B');
			this._allowedStartCharacters.Add('C');
			this._allowedStartCharacters.Add('D');

			this._allowedEndCharacters = new List<char>();
			this._allowedEndCharacters.Add('A');
			this._allowedEndCharacters.Add('B');
			this._allowedEndCharacters.Add('C');
			this._allowedEndCharacters.Add('D');

			this._allowedValueCharacters = new List<char>();
			this._allowedValueCharacters.Add('0');
			this._allowedValueCharacters.Add('1');
			this._allowedValueCharacters.Add('2');
			this._allowedValueCharacters.Add('3');
			this._allowedValueCharacters.Add('4');
			this._allowedValueCharacters.Add('5');
			this._allowedValueCharacters.Add('6');
			this._allowedValueCharacters.Add('7');
			this._allowedValueCharacters.Add('8');
			this._allowedValueCharacters.Add('9');
			this._allowedValueCharacters.Add('-');
			this._allowedValueCharacters.Add('$');
			this._allowedValueCharacters.Add(':');
			this._allowedValueCharacters.Add('/');
			this._allowedValueCharacters.Add('.');
			this._allowedValueCharacters.Add('+');


			this._codabarStartStopEncoding = new Dictionary<char, int[]>();
            this._codabarStartStopEncoding.Add('A', new int[] { 1, 1, 2, 2, 1, 2, 1 });
            this._codabarStartStopEncoding.Add('B', new int[] { 1, 1, 1, 2, 1, 2, 2 });
            this._codabarStartStopEncoding.Add('C', new int[] { 1, 2, 1, 2, 1, 1, 2 });
            this._codabarStartStopEncoding.Add('D', new int[] { 1, 1, 1, 2, 2, 2, 1 });

            this._codabarValueEncoding = new Dictionary<char, int[]>();
            this._codabarValueEncoding.Add('0', new int[] { 1, 1, 1, 1, 1, 2, 2 });
            this._codabarValueEncoding.Add('1', new int[] { 1, 1, 1, 1, 2, 2, 1 });
            this._codabarValueEncoding.Add('2', new int[] { 1, 1, 1, 2, 1, 1, 2 });
            this._codabarValueEncoding.Add('3', new int[] { 2, 2, 1, 1, 1, 1, 1 });
            this._codabarValueEncoding.Add('4', new int[] { 1, 1, 2, 1, 1, 2, 1 });
            this._codabarValueEncoding.Add('5', new int[] { 2, 1, 1, 1, 1, 2, 1 });
            this._codabarValueEncoding.Add('6', new int[] { 1, 2, 1, 1, 1, 1, 2 });
            this._codabarValueEncoding.Add('7', new int[] { 1, 2, 1, 1, 2, 1, 1 });
            this._codabarValueEncoding.Add('8', new int[] { 1, 2, 2, 1, 1, 1, 1 });
            this._codabarValueEncoding.Add('9', new int[] { 2, 1, 1, 2, 1, 1, 1 });
            this._codabarValueEncoding.Add('-', new int[] { 1, 1, 1, 2, 2, 1, 1 });
            this._codabarValueEncoding.Add('$', new int[] { 1, 1, 2, 2, 1, 1, 1 });
            this._codabarValueEncoding.Add(':', new int[] { 2, 1, 1, 1, 2, 1, 2 });
            this._codabarValueEncoding.Add('/', new int[] { 2, 1, 2, 1, 1, 1, 2 });
            this._codabarValueEncoding.Add('.', new int[] { 2, 1, 2, 1, 2, 1, 1 });
            this._codabarValueEncoding.Add('+', new int[] { 1, 1, 2, 1, 2, 1, 2 });
        }

        private static void CodabarValueEncoder(BarcodeImage barcode) {
            List<int> ev = new List<int>();
            ev.AddRange(barcode._codabarStartStopEncoding[barcode.StartCharacter]);
            ev.Add(0);
            for (int i = 0; i < barcode.Value.Length; i++) {
                ev.AddRange(barcode._codabarValueEncoding[barcode.Value[i]]);
                ev.Add(0);
            }
            ev.AddRange(barcode._codabarStartStopEncoding[barcode.EndCharacter]);

            barcode.EncodedValue = ev.ToArray();
        }

        #endregion

        #region Code128

        private const byte A_FNC3 = 96;
        private const byte A_FNC2 = 97;
        private const byte A_SHIFT = 98;
        private const byte A_CODEC = 99;
        private const byte A_CODEB = 100;
        private const byte A_FNC4 = 101;
        private const byte A_FNC1 = 102;
        private const byte A_STARTA = 103;
        private const byte A_STARTB = 104;
        private const byte A_STARTC = 105;
        private const byte A_STOP = 106;
        private const byte B_FNC3 = 96;
        private const byte B_FNC2 = 97;
        private const byte B_SHIFT = 98;
        private const byte B_CODEC = 99;
        private const byte B_FNC4 = 100;
        private const byte B_CODE1 = 101;
        private const byte B_FNC1 = 102;
        private const byte B_STARTA = 103;
        private const byte B_STARTB = 104;
        private const byte B_STARTC = 105;
        private const byte B_STOP = 106;
        private const byte C_CODEB = 100;
        private const byte C_CODEA = 101;
        private const byte C_FNC1 = 102;
        private const byte C_STARTA = 103;
        private const byte C_STARTB = 104;
        private const byte C_STARTC = 105;
        private const byte C_STOP = 106;

		private Dictionary<int, int[]> _code128EncodingLookup;
		private Dictionary<int, string>[] _code128MappingLookup;

        private void InitCode128() {
			this._allowedStartCharacters = new List<char>();
			this._allowedEndCharacters = new List<char>();
			this._allowedValueCharacters = new List<char>();
            for (int i = 0; i < 128; ++i) {
				this._allowedValueCharacters.Add(System.Convert.ToChar(i));
            }

            InitCode128Lookup();
            InitCode128MappingLookup();
        }

        private void InitCode128MappingLookup() {
            this._code128MappingLookup = new Dictionary<int, string>[] { new Dictionary<int, string>(), new Dictionary<int, string>(), new Dictionary<int, string>() };
            InitCode128MappingLookup0();
            InitCode128MappingLookup1();
            InitCode128MappingLookup2();
        }

        private void InitCode128MappingLookup2() {
            this._code128MappingLookup[2].Add(0, "00");
            this._code128MappingLookup[2].Add(1, "01");
            this._code128MappingLookup[2].Add(2, "02");
            this._code128MappingLookup[2].Add(3, "03");
            this._code128MappingLookup[2].Add(4, "04");
            this._code128MappingLookup[2].Add(5, "05");
            this._code128MappingLookup[2].Add(6, "06");
            this._code128MappingLookup[2].Add(7, "07");
            this._code128MappingLookup[2].Add(8, "08");
            this._code128MappingLookup[2].Add(9, "09");
            this._code128MappingLookup[2].Add(10, "10");
            this._code128MappingLookup[2].Add(11, "11");
            this._code128MappingLookup[2].Add(12, "12");
            this._code128MappingLookup[2].Add(13, "13");
            this._code128MappingLookup[2].Add(14, "14");
            this._code128MappingLookup[2].Add(15, "15");
            this._code128MappingLookup[2].Add(16, "16");
            this._code128MappingLookup[2].Add(17, "17");
            this._code128MappingLookup[2].Add(18, "18");
            this._code128MappingLookup[2].Add(19, "19");
            this._code128MappingLookup[2].Add(20, "20");
            this._code128MappingLookup[2].Add(21, "21");
            this._code128MappingLookup[2].Add(22, "22");
            this._code128MappingLookup[2].Add(23, "23");
            this._code128MappingLookup[2].Add(24, "24");
            this._code128MappingLookup[2].Add(25, "25");
            this._code128MappingLookup[2].Add(26, "26");
            this._code128MappingLookup[2].Add(27, "27");
            this._code128MappingLookup[2].Add(28, "28");
            this._code128MappingLookup[2].Add(29, "29");
            this._code128MappingLookup[2].Add(30, "30");
            this._code128MappingLookup[2].Add(31, "31");
            this._code128MappingLookup[2].Add(32, "32");
            this._code128MappingLookup[2].Add(33, "33");
            this._code128MappingLookup[2].Add(34, "34");
            this._code128MappingLookup[2].Add(35, "35");
            this._code128MappingLookup[2].Add(36, "36");
            this._code128MappingLookup[2].Add(37, "37");
            this._code128MappingLookup[2].Add(38, "38");
            this._code128MappingLookup[2].Add(39, "39");
            this._code128MappingLookup[2].Add(40, "40");
            this._code128MappingLookup[2].Add(41, "41");
            this._code128MappingLookup[2].Add(42, "42");
            this._code128MappingLookup[2].Add(43, "43");
            this._code128MappingLookup[2].Add(44, "44");
            this._code128MappingLookup[2].Add(45, "45");
            this._code128MappingLookup[2].Add(46, "46");
            this._code128MappingLookup[2].Add(47, "47");
            this._code128MappingLookup[2].Add(48, "48");
            this._code128MappingLookup[2].Add(49, "49");
            this._code128MappingLookup[2].Add(50, "50");
            this._code128MappingLookup[2].Add(51, "51");
            this._code128MappingLookup[2].Add(52, "52");
            this._code128MappingLookup[2].Add(53, "53");
            this._code128MappingLookup[2].Add(54, "54");
            this._code128MappingLookup[2].Add(55, "55");
            this._code128MappingLookup[2].Add(56, "56");
            this._code128MappingLookup[2].Add(57, "57");
            this._code128MappingLookup[2].Add(58, "58");
            this._code128MappingLookup[2].Add(59, "59");
            this._code128MappingLookup[2].Add(60, "60");
            this._code128MappingLookup[2].Add(61, "61");
            this._code128MappingLookup[2].Add(62, "62");
            this._code128MappingLookup[2].Add(63, "63");
            this._code128MappingLookup[2].Add(64, "64");
            this._code128MappingLookup[2].Add(65, "65");
            this._code128MappingLookup[2].Add(66, "66");
            this._code128MappingLookup[2].Add(67, "67");
            this._code128MappingLookup[2].Add(68, "68");
            this._code128MappingLookup[2].Add(69, "69");
            this._code128MappingLookup[2].Add(70, "70");
            this._code128MappingLookup[2].Add(71, "71");
            this._code128MappingLookup[2].Add(72, "72");
            this._code128MappingLookup[2].Add(73, "73");
            this._code128MappingLookup[2].Add(74, "74");
            this._code128MappingLookup[2].Add(75, "75");
            this._code128MappingLookup[2].Add(76, "76");
            this._code128MappingLookup[2].Add(77, "77");
            this._code128MappingLookup[2].Add(78, "78");
            this._code128MappingLookup[2].Add(79, "79");
            this._code128MappingLookup[2].Add(80, "80");
            this._code128MappingLookup[2].Add(81, "81");
            this._code128MappingLookup[2].Add(82, "82");
            this._code128MappingLookup[2].Add(83, "83");
            this._code128MappingLookup[2].Add(84, "84");
            this._code128MappingLookup[2].Add(85, "85");
            this._code128MappingLookup[2].Add(86, "86");
            this._code128MappingLookup[2].Add(87, "87");
            this._code128MappingLookup[2].Add(88, "88");
            this._code128MappingLookup[2].Add(89, "89");
            this._code128MappingLookup[2].Add(90, "90");
            this._code128MappingLookup[2].Add(91, "91");
            this._code128MappingLookup[2].Add(92, "92");
            this._code128MappingLookup[2].Add(93, "93");
            this._code128MappingLookup[2].Add(94, "94");
            this._code128MappingLookup[2].Add(95, "95");
            this._code128MappingLookup[2].Add(96, "96");
            this._code128MappingLookup[2].Add(97, "97");
            this._code128MappingLookup[2].Add(98, "98");
            this._code128MappingLookup[2].Add(99, "99");
        }

        private void InitCode128MappingLookup1() {
            this._code128MappingLookup[1].Add(0, " ");
            this._code128MappingLookup[1].Add(1, "!");
            this._code128MappingLookup[1].Add(2, "\"");
            this._code128MappingLookup[1].Add(3, "#");
            this._code128MappingLookup[1].Add(4, "$");
            this._code128MappingLookup[1].Add(5, "%");
            this._code128MappingLookup[1].Add(6, "&");
            this._code128MappingLookup[1].Add(7, "'");
            this._code128MappingLookup[1].Add(8, "(");
            this._code128MappingLookup[1].Add(9, ")");
            this._code128MappingLookup[1].Add(10, "*");
            this._code128MappingLookup[1].Add(11, "+");
            this._code128MappingLookup[1].Add(12, ",");
            this._code128MappingLookup[1].Add(13, "-");
            this._code128MappingLookup[1].Add(14, ".");
            this._code128MappingLookup[1].Add(15, "/");
            this._code128MappingLookup[1].Add(16, "0");
            this._code128MappingLookup[1].Add(17, "1");
            this._code128MappingLookup[1].Add(18, "2");
            this._code128MappingLookup[1].Add(19, "3");
            this._code128MappingLookup[1].Add(20, "4");
            this._code128MappingLookup[1].Add(21, "5");
            this._code128MappingLookup[1].Add(22, "6");
            this._code128MappingLookup[1].Add(23, "7");
            this._code128MappingLookup[1].Add(24, "8");
            this._code128MappingLookup[1].Add(25, "9");
            this._code128MappingLookup[1].Add(26, ":");
            this._code128MappingLookup[1].Add(27, ";");
            this._code128MappingLookup[1].Add(28, "<");
            this._code128MappingLookup[1].Add(29, "=");
            this._code128MappingLookup[1].Add(30, ">");
            this._code128MappingLookup[1].Add(31, "?");
            this._code128MappingLookup[1].Add(32, "@");
            this._code128MappingLookup[1].Add(33, "A");
            this._code128MappingLookup[1].Add(34, "B");
            this._code128MappingLookup[1].Add(35, "C");
            this._code128MappingLookup[1].Add(36, "D");
            this._code128MappingLookup[1].Add(37, "E");
            this._code128MappingLookup[1].Add(38, "F");
            this._code128MappingLookup[1].Add(39, "G");
            this._code128MappingLookup[1].Add(40, "H");
            this._code128MappingLookup[1].Add(41, "I");
            this._code128MappingLookup[1].Add(42, "J");
            this._code128MappingLookup[1].Add(43, "K");
            this._code128MappingLookup[1].Add(44, "L");
            this._code128MappingLookup[1].Add(45, "M");
            this._code128MappingLookup[1].Add(46, "N");
            this._code128MappingLookup[1].Add(47, "O");
            this._code128MappingLookup[1].Add(48, "P");
            this._code128MappingLookup[1].Add(49, "Q");
            this._code128MappingLookup[1].Add(50, "R");
            this._code128MappingLookup[1].Add(51, "S");
            this._code128MappingLookup[1].Add(52, "T");
            this._code128MappingLookup[1].Add(53, "U");
            this._code128MappingLookup[1].Add(54, "V");
            this._code128MappingLookup[1].Add(55, "W");
            this._code128MappingLookup[1].Add(56, "X");
            this._code128MappingLookup[1].Add(57, "Y");
            this._code128MappingLookup[1].Add(58, "Z");
            this._code128MappingLookup[1].Add(59, "[");
            this._code128MappingLookup[1].Add(60, "\\");
            this._code128MappingLookup[1].Add(61, "]");
            this._code128MappingLookup[1].Add(62, "^");
            this._code128MappingLookup[1].Add(63, "_");
            this._code128MappingLookup[1].Add(64, "`");
            this._code128MappingLookup[1].Add(65, "a");
            this._code128MappingLookup[1].Add(66, "b");
            this._code128MappingLookup[1].Add(67, "c");
            this._code128MappingLookup[1].Add(68, "d");
            this._code128MappingLookup[1].Add(69, "e");
            this._code128MappingLookup[1].Add(70, "f");
            this._code128MappingLookup[1].Add(71, "g");
            this._code128MappingLookup[1].Add(72, "h");
            this._code128MappingLookup[1].Add(73, "i");
            this._code128MappingLookup[1].Add(74, "j");
            this._code128MappingLookup[1].Add(75, "k");
            this._code128MappingLookup[1].Add(76, "l");
            this._code128MappingLookup[1].Add(77, "m");
            this._code128MappingLookup[1].Add(78, "n");
            this._code128MappingLookup[1].Add(79, "o");
            this._code128MappingLookup[1].Add(80, "p");
            this._code128MappingLookup[1].Add(81, "q");
            this._code128MappingLookup[1].Add(82, "r");
            this._code128MappingLookup[1].Add(83, "s");
            this._code128MappingLookup[1].Add(84, "t");
            this._code128MappingLookup[1].Add(85, "u");
            this._code128MappingLookup[1].Add(86, "v");
            this._code128MappingLookup[1].Add(87, "w");
            this._code128MappingLookup[1].Add(88, "x");
            this._code128MappingLookup[1].Add(89, "y");
            this._code128MappingLookup[1].Add(90, "z");
            this._code128MappingLookup[1].Add(91, "{");
            this._code128MappingLookup[1].Add(92, "|");
            this._code128MappingLookup[1].Add(93, "}");
            this._code128MappingLookup[1].Add(94, "~");
            this._code128MappingLookup[1].Add(95, System.Convert.ToChar(127).ToString());
        }

        private void InitCode128MappingLookup0() {
            this._code128MappingLookup[0].Add(0, " ");
            this._code128MappingLookup[0].Add(1, "!");
            this._code128MappingLookup[0].Add(2, "\"");
            this._code128MappingLookup[0].Add(3, "#");
            this._code128MappingLookup[0].Add(4, "$");
            this._code128MappingLookup[0].Add(5, "%");
            this._code128MappingLookup[0].Add(6, "&");
            this._code128MappingLookup[0].Add(7, "'");
            this._code128MappingLookup[0].Add(8, "(");
            this._code128MappingLookup[0].Add(9, ")");
            this._code128MappingLookup[0].Add(10, "*");
            this._code128MappingLookup[0].Add(11, "+");
            this._code128MappingLookup[0].Add(12, ",");
            this._code128MappingLookup[0].Add(13, "-");
            this._code128MappingLookup[0].Add(14, ".");
            this._code128MappingLookup[0].Add(15, "/");
            this._code128MappingLookup[0].Add(16, "0");
            this._code128MappingLookup[0].Add(17, "1");
            this._code128MappingLookup[0].Add(18, "2");
            this._code128MappingLookup[0].Add(19, "3");
            this._code128MappingLookup[0].Add(20, "4");
            this._code128MappingLookup[0].Add(21, "5");
            this._code128MappingLookup[0].Add(22, "6");
            this._code128MappingLookup[0].Add(23, "7");
            this._code128MappingLookup[0].Add(24, "8");
            this._code128MappingLookup[0].Add(25, "9");
            this._code128MappingLookup[0].Add(26, ":");
            this._code128MappingLookup[0].Add(27, ";");
            this._code128MappingLookup[0].Add(28, "<");
            this._code128MappingLookup[0].Add(29, "=");
            this._code128MappingLookup[0].Add(30, ">");
            this._code128MappingLookup[0].Add(31, "?");
            this._code128MappingLookup[0].Add(32, "@");
            this._code128MappingLookup[0].Add(33, "A");
            this._code128MappingLookup[0].Add(34, "B");
            this._code128MappingLookup[0].Add(35, "C");
            this._code128MappingLookup[0].Add(36, "D");
            this._code128MappingLookup[0].Add(37, "E");
            this._code128MappingLookup[0].Add(38, "F");
            this._code128MappingLookup[0].Add(39, "G");
            this._code128MappingLookup[0].Add(40, "H");
            this._code128MappingLookup[0].Add(41, "I");
            this._code128MappingLookup[0].Add(42, "J");
            this._code128MappingLookup[0].Add(43, "K");
            this._code128MappingLookup[0].Add(44, "L");
            this._code128MappingLookup[0].Add(45, "M");
            this._code128MappingLookup[0].Add(46, "N");
            this._code128MappingLookup[0].Add(47, "O");
            this._code128MappingLookup[0].Add(48, "P");
            this._code128MappingLookup[0].Add(49, "Q");
            this._code128MappingLookup[0].Add(50, "R");
            this._code128MappingLookup[0].Add(51, "S");
            this._code128MappingLookup[0].Add(52, "T");
            this._code128MappingLookup[0].Add(53, "U");
            this._code128MappingLookup[0].Add(54, "V");
            this._code128MappingLookup[0].Add(55, "W");
            this._code128MappingLookup[0].Add(56, "X");
            this._code128MappingLookup[0].Add(57, "Y");
            this._code128MappingLookup[0].Add(58, "Z");
            this._code128MappingLookup[0].Add(59, "[");
            this._code128MappingLookup[0].Add(60, "\\");
            this._code128MappingLookup[0].Add(61, "]");
            this._code128MappingLookup[0].Add(62, "^");
            this._code128MappingLookup[0].Add(63, "_");
            this._code128MappingLookup[0].Add(64, System.Convert.ToChar(0).ToString());
            this._code128MappingLookup[0].Add(65, System.Convert.ToChar(1).ToString());
            this._code128MappingLookup[0].Add(66, System.Convert.ToChar(2).ToString());
            this._code128MappingLookup[0].Add(67, System.Convert.ToChar(3).ToString());
            this._code128MappingLookup[0].Add(68, System.Convert.ToChar(4).ToString());
            this._code128MappingLookup[0].Add(69, System.Convert.ToChar(5).ToString());
            this._code128MappingLookup[0].Add(70, System.Convert.ToChar(6).ToString());
            this._code128MappingLookup[0].Add(71, System.Convert.ToChar(7).ToString());
            this._code128MappingLookup[0].Add(72, System.Convert.ToChar(8).ToString());
            this._code128MappingLookup[0].Add(73, System.Convert.ToChar(9).ToString());
            this._code128MappingLookup[0].Add(74, System.Convert.ToChar(10).ToString());
            this._code128MappingLookup[0].Add(75, System.Convert.ToChar(11).ToString());
            this._code128MappingLookup[0].Add(76, System.Convert.ToChar(12).ToString());
            this._code128MappingLookup[0].Add(77, System.Convert.ToChar(13).ToString());
            this._code128MappingLookup[0].Add(78, System.Convert.ToChar(14).ToString());
            this._code128MappingLookup[0].Add(79, System.Convert.ToChar(15).ToString());
            this._code128MappingLookup[0].Add(80, System.Convert.ToChar(16).ToString());
            this._code128MappingLookup[0].Add(81, System.Convert.ToChar(17).ToString());
            this._code128MappingLookup[0].Add(82, System.Convert.ToChar(18).ToString());
            this._code128MappingLookup[0].Add(83, System.Convert.ToChar(19).ToString());
            this._code128MappingLookup[0].Add(84, System.Convert.ToChar(20).ToString());
            this._code128MappingLookup[0].Add(85, System.Convert.ToChar(21).ToString());
            this._code128MappingLookup[0].Add(86, System.Convert.ToChar(22).ToString());
            this._code128MappingLookup[0].Add(87, System.Convert.ToChar(23).ToString());
            this._code128MappingLookup[0].Add(88, System.Convert.ToChar(24).ToString());
            this._code128MappingLookup[0].Add(89, System.Convert.ToChar(25).ToString());
            this._code128MappingLookup[0].Add(90, System.Convert.ToChar(26).ToString());
            this._code128MappingLookup[0].Add(91, System.Convert.ToChar(27).ToString());
            this._code128MappingLookup[0].Add(92, System.Convert.ToChar(28).ToString());
            this._code128MappingLookup[0].Add(93, System.Convert.ToChar(29).ToString());
            this._code128MappingLookup[0].Add(94, System.Convert.ToChar(30).ToString());
            this._code128MappingLookup[0].Add(95, System.Convert.ToChar(31).ToString());
        }

        private void InitCode128Lookup() {
            this._code128EncodingLookup = new Dictionary<int, int[]>();
            this._code128EncodingLookup.Add(0, new int[] { 2, 1, 2, 2, 2, 2 });
            this._code128EncodingLookup.Add(1, new int[] { 2, 2, 2, 1, 2, 2 });
            this._code128EncodingLookup.Add(2, new int[] { 2, 2, 2, 2, 2, 1 });
            this._code128EncodingLookup.Add(3, new int[] { 1, 2, 1, 2, 2, 3 });
            this._code128EncodingLookup.Add(4, new int[] { 1, 2, 1, 3, 2, 2 });
            this._code128EncodingLookup.Add(5, new int[] { 1, 3, 1, 2, 2, 2 });
            this._code128EncodingLookup.Add(6, new int[] { 1, 2, 2, 2, 1, 3 });
            this._code128EncodingLookup.Add(7, new int[] { 1, 2, 2, 3, 1, 2 });
            this._code128EncodingLookup.Add(8, new int[] { 1, 3, 2, 2, 1, 2 });
            this._code128EncodingLookup.Add(9, new int[] { 2, 2, 1, 2, 1, 3 });
            this._code128EncodingLookup.Add(10, new int[] { 2, 2, 1, 3, 1, 2 });
            this._code128EncodingLookup.Add(11, new int[] { 2, 3, 1, 2, 1, 2 });
            this._code128EncodingLookup.Add(12, new int[] { 1, 1, 2, 2, 3, 2 });
            this._code128EncodingLookup.Add(13, new int[] { 1, 2, 2, 1, 3, 2 });
            this._code128EncodingLookup.Add(14, new int[] { 1, 2, 2, 2, 3, 1 });
            this._code128EncodingLookup.Add(15, new int[] { 1, 1, 3, 2, 2, 2 });
            this._code128EncodingLookup.Add(16, new int[] { 1, 2, 3, 1, 2, 2 });
            this._code128EncodingLookup.Add(17, new int[] { 1, 2, 3, 2, 2, 1 });
            this._code128EncodingLookup.Add(18, new int[] { 2, 2, 3, 2, 1, 1 });
            this._code128EncodingLookup.Add(19, new int[] { 2, 2, 1, 1, 3, 2 });
            this._code128EncodingLookup.Add(20, new int[] { 2, 2, 1, 2, 3, 1 });
            this._code128EncodingLookup.Add(21, new int[] { 2, 1, 3, 2, 1, 2 });
            this._code128EncodingLookup.Add(22, new int[] { 2, 2, 3, 1, 1, 2 });
            this._code128EncodingLookup.Add(23, new int[] { 3, 1, 2, 1, 3, 1 });
            this._code128EncodingLookup.Add(24, new int[] { 3, 1, 1, 2, 2, 2 });
            this._code128EncodingLookup.Add(25, new int[] { 3, 2, 1, 1, 2, 2 });
            this._code128EncodingLookup.Add(26, new int[] { 3, 2, 1, 2, 2, 1 });
            this._code128EncodingLookup.Add(27, new int[] { 3, 1, 2, 2, 1, 2 });
            this._code128EncodingLookup.Add(28, new int[] { 3, 2, 2, 1, 1, 2 });
            this._code128EncodingLookup.Add(29, new int[] { 3, 2, 2, 2, 1, 1 });
            this._code128EncodingLookup.Add(30, new int[] { 2, 1, 2, 1, 2, 3 });
            this._code128EncodingLookup.Add(31, new int[] { 2, 1, 2, 3, 2, 1 });
            this._code128EncodingLookup.Add(32, new int[] { 2, 3, 2, 1, 2, 1 });
            this._code128EncodingLookup.Add(33, new int[] { 1, 1, 1, 3, 2, 3 });
            this._code128EncodingLookup.Add(34, new int[] { 1, 3, 1, 1, 2, 3 });
            this._code128EncodingLookup.Add(35, new int[] { 1, 3, 1, 3, 2, 1 });
            this._code128EncodingLookup.Add(36, new int[] { 1, 1, 2, 3, 1, 3 });
            this._code128EncodingLookup.Add(37, new int[] { 1, 3, 2, 1, 1, 3 });
            this._code128EncodingLookup.Add(38, new int[] { 1, 3, 2, 3, 1, 1 });
            this._code128EncodingLookup.Add(39, new int[] { 2, 1, 1, 3, 1, 3 });
            this._code128EncodingLookup.Add(40, new int[] { 2, 3, 1, 1, 1, 3 });
            this._code128EncodingLookup.Add(41, new int[] { 2, 3, 1, 3, 1, 1 });
            this._code128EncodingLookup.Add(42, new int[] { 1, 1, 2, 1, 3, 3 });
            this._code128EncodingLookup.Add(43, new int[] { 1, 1, 2, 3, 3, 1 });
            this._code128EncodingLookup.Add(44, new int[] { 1, 3, 2, 1, 3, 1 });
            this._code128EncodingLookup.Add(45, new int[] { 1, 1, 3, 1, 2, 3 });
            this._code128EncodingLookup.Add(46, new int[] { 1, 1, 3, 3, 2, 1 });
            this._code128EncodingLookup.Add(47, new int[] { 1, 3, 3, 1, 2, 1 });
            this._code128EncodingLookup.Add(48, new int[] { 3, 1, 3, 1, 2, 1 });
            this._code128EncodingLookup.Add(49, new int[] { 2, 1, 1, 3, 3, 1 });
            this._code128EncodingLookup.Add(50, new int[] { 2, 3, 1, 1, 3, 1 });
            this._code128EncodingLookup.Add(51, new int[] { 2, 1, 3, 1, 1, 3 });
            this._code128EncodingLookup.Add(52, new int[] { 2, 1, 3, 3, 1, 1 });
            this._code128EncodingLookup.Add(53, new int[] { 2, 1, 3, 1, 3, 1 });
            this._code128EncodingLookup.Add(54, new int[] { 3, 1, 1, 1, 2, 3 });
            this._code128EncodingLookup.Add(55, new int[] { 3, 1, 1, 3, 2, 1 });
            this._code128EncodingLookup.Add(56, new int[] { 3, 3, 1, 1, 2, 1 });
            this._code128EncodingLookup.Add(57, new int[] { 3, 1, 2, 1, 1, 3 });
            this._code128EncodingLookup.Add(58, new int[] { 3, 1, 2, 3, 1, 1 });
            this._code128EncodingLookup.Add(59, new int[] { 3, 3, 2, 1, 1, 1 });
            this._code128EncodingLookup.Add(60, new int[] { 3, 1, 4, 1, 1, 1 });
            this._code128EncodingLookup.Add(61, new int[] { 2, 2, 1, 4, 1, 1 });
            this._code128EncodingLookup.Add(62, new int[] { 4, 3, 1, 1, 1, 1 });
            this._code128EncodingLookup.Add(63, new int[] { 1, 1, 1, 2, 2, 4 });
            this._code128EncodingLookup.Add(64, new int[] { 1, 1, 1, 4, 2, 2 });
            this._code128EncodingLookup.Add(65, new int[] { 1, 2, 1, 1, 2, 4 });
            this._code128EncodingLookup.Add(66, new int[] { 1, 2, 1, 4, 2, 1 });
            this._code128EncodingLookup.Add(67, new int[] { 1, 4, 1, 1, 2, 2 });
            this._code128EncodingLookup.Add(68, new int[] { 1, 4, 1, 2, 2, 1 });
            this._code128EncodingLookup.Add(69, new int[] { 1, 1, 2, 2, 1, 4 });
            this._code128EncodingLookup.Add(70, new int[] { 1, 1, 2, 4, 1, 2 });
            this._code128EncodingLookup.Add(71, new int[] { 1, 2, 2, 1, 1, 4 });
            this._code128EncodingLookup.Add(72, new int[] { 1, 2, 2, 4, 1, 1 });
            this._code128EncodingLookup.Add(73, new int[] { 1, 4, 2, 1, 1, 2 });
            this._code128EncodingLookup.Add(74, new int[] { 1, 4, 2, 2, 1, 1 });
            this._code128EncodingLookup.Add(75, new int[] { 2, 4, 1, 2, 1, 1 });
            this._code128EncodingLookup.Add(76, new int[] { 2, 2, 1, 1, 1, 4 });
            this._code128EncodingLookup.Add(77, new int[] { 4, 1, 3, 1, 1, 1 });
            this._code128EncodingLookup.Add(78, new int[] { 2, 4, 1, 1, 1, 2 });
            this._code128EncodingLookup.Add(79, new int[] { 1, 3, 4, 1, 1, 1 });
            this._code128EncodingLookup.Add(80, new int[] { 1, 1, 1, 2, 4, 2 });
            this._code128EncodingLookup.Add(81, new int[] { 1, 2, 1, 1, 4, 2 });
            this._code128EncodingLookup.Add(82, new int[] { 1, 2, 1, 2, 4, 1 });
            this._code128EncodingLookup.Add(83, new int[] { 1, 1, 4, 2, 1, 2 });
            this._code128EncodingLookup.Add(84, new int[] { 1, 2, 4, 1, 1, 2 });
            this._code128EncodingLookup.Add(85, new int[] { 1, 2, 4, 2, 1, 1 });
            this._code128EncodingLookup.Add(86, new int[] { 4, 1, 1, 2, 1, 2 });
            this._code128EncodingLookup.Add(87, new int[] { 4, 2, 1, 1, 1, 2 });
            this._code128EncodingLookup.Add(88, new int[] { 4, 2, 1, 2, 1, 1 });
            this._code128EncodingLookup.Add(89, new int[] { 2, 1, 2, 1, 4, 1 });
            this._code128EncodingLookup.Add(90, new int[] { 2, 1, 4, 1, 2, 1 });
            this._code128EncodingLookup.Add(91, new int[] { 4, 1, 2, 1, 2, 1 });
            this._code128EncodingLookup.Add(92, new int[] { 1, 1, 1, 1, 4, 3 });
            this._code128EncodingLookup.Add(93, new int[] { 1, 1, 1, 3, 4, 1 });
            this._code128EncodingLookup.Add(94, new int[] { 1, 3, 1, 1, 4, 1 });
            this._code128EncodingLookup.Add(95, new int[] { 1, 1, 4, 1, 1, 3 });
            this._code128EncodingLookup.Add(96, new int[] { 1, 1, 4, 3, 1, 1 });
            this._code128EncodingLookup.Add(97, new int[] { 4, 1, 1, 1, 1, 3 });
            this._code128EncodingLookup.Add(98, new int[] { 4, 1, 1, 3, 1, 1 });
            this._code128EncodingLookup.Add(99, new int[] { 1, 1, 3, 1, 4, 1 });
            this._code128EncodingLookup.Add(100, new int[] { 1, 1, 4, 1, 3, 1 });
            this._code128EncodingLookup.Add(101, new int[] { 3, 1, 1, 1, 4, 1 });
            this._code128EncodingLookup.Add(102, new int[] { 4, 1, 1, 1, 3, 1 });
            this._code128EncodingLookup.Add(103, new int[] { 2, 1, 1, 4, 1, 2 });
            this._code128EncodingLookup.Add(104, new int[] { 2, 1, 1, 2, 1, 4 });
            this._code128EncodingLookup.Add(105, new int[] { 2, 1, 1, 2, 3, 2 });
            this._code128EncodingLookup.Add(106, new int[] { 2, 3, 3, 1, 1, 1, 2 });

        }

        private static void Code128ValueEncoder(BarcodeImage barcode) {
            List<int> ev = new List<int>();

            int startCodeSet = GetCode128BestCodeSet(barcode, barcode.Value, -1);
            int checksum = A_STARTA + startCodeSet;

            ev.AddRange(barcode._code128EncodingLookup[(byte)(A_STARTA + startCodeSet)]);

            int codeSet = startCodeSet;
            int j = 1;
            for (int i = 0; i < barcode.Value.Length; i++) {
                int tmpCharIndex;

                int tmpOldCodeSet = codeSet;
                codeSet = GetCode128BestCodeSet(barcode, barcode.Value.Substring(i, barcode._value.Length - i), tmpOldCodeSet);
                if (codeSet == 2) {
                    tmpCharIndex = GetCode128CharPos(barcode, barcode.Value.Substring(i, 2), codeSet);
                } else {
                    tmpCharIndex = GetCode128CharPos(barcode, barcode.Value.Substring(i, 1), codeSet);
                }//if

                if (codeSet != tmpOldCodeSet) {
                    ev.AddRange(barcode._code128EncodingLookup[101 - codeSet]);
                    checksum += j * (101 - codeSet);
                    j += 1;
                }//if

                ev.AddRange(barcode._code128EncodingLookup[tmpCharIndex]);
                checksum += j * tmpCharIndex;

                if (codeSet == 2) { i++; }
                j += 1;
            }//for(i)

            ev.AddRange(barcode._code128EncodingLookup[checksum % 103]);
            ev.AddRange(barcode._code128EncodingLookup[A_STOP]);

            barcode.EncodedValue = ev.ToArray();
        }

        private static int GetCode128BestCodeSet(BarcodeImage barcode, string value, int currentCodeSetIndex) {
            int[] tmpCount = { 0, 0, 0 };

            int i;

            for (i = 0; i < value.Length; i++) {
                if (GetCode128CharPos(barcode, value.Substring(i, 1), 0) == -1) {
                    break;
                }
            }//for i
            tmpCount[0] = i;

            for (i = 0; i < value.Length; i++) {
                if (GetCode128CharPos(barcode, value.Substring(i, 1), 1) == -1) {
                    break;
                }
            }//for i
            tmpCount[1] = i;

            for (i = 0; i < value.Length - 1; i++) {
                if (GetCode128CharPos(barcode, value.Substring(i, 2), 2) == -1) {
                    break;
                }
            }//for i
            tmpCount[2] = i * 2;

            switch (currentCodeSetIndex) {
                case 0:
                    if (tmpCount[0] > 0) { tmpCount[0] += 1; }
                    break;
                case 1:
                    if (tmpCount[1] > 0) { tmpCount[1] += 1; }
                    break;
                case 2:
                    if (tmpCount[2] > 0) { tmpCount[2] += 2; }
                    break;
            }//switch

            int tmpCountMax = tmpCount[0];
            int tmpCountMaxIndex = 0;
            if (tmpCount[1] > tmpCountMax) { tmpCountMax = tmpCount[1]; tmpCountMaxIndex = 1; }
            if (tmpCount[2] > tmpCountMax) { tmpCountMax = tmpCount[2]; tmpCountMaxIndex = 2; }

            return tmpCountMaxIndex;
        }//GetCode128BestCodeSet


        private static int GetCode128CharPos(BarcodeImage barcode, string value, int codeTableIndex) {
            System.Collections.Generic.Dictionary<int, string>.Enumerator iMapping = barcode._code128MappingLookup[codeTableIndex].GetEnumerator();
            while (iMapping.MoveNext()) {
                if (iMapping.Current.Value == value) {
                    return iMapping.Current.Key;
                }//if
            }//while
            return -1;
        }//GetCode128CharPos

        #endregion


        private static class Resources {

            internal static string ExceptionWidthMustBeLargerThan0 { get { return "Width must be larger than 0."; } }
            internal static string FormatExceptionCharacterIsNotSupported { get { return "Character '{0}' is not supported."; } }

        }

    }

}
