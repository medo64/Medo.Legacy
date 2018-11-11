/* Josip Medved <jmedved@jmedved.com> * www.medo64.com * MIT License */

//2008-12-09: Changed Code 128 pattern 27 from 311212 to 312212.
//2008-11-05: Refactoring (Microsoft.Maintainability : 'BarcodeImage.InitCode128()' has a maintainability index of 13).
//2008-04-11: Refactoring.
//2008-04-05: New version.


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
            BarColor = Color.Black;
            GapColor = Color.White;
            SpaceColor = Color.White;
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
            for (int i = 0; i < EncodedValue.Length; i++) {
                switch (i % 2) {
                    case 0: //bar
                        totalWidth += EncodedValue[i] * BarWidth;
                        break;
                    case 1: //gap or space
                        if (EncodedValue[i] != 0) { //gap
                            totalWidth += EncodedValue[i] * GapWidth;
                        } else { //space
                            totalWidth += SpaceWidth;
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
            get { return _startCharacter; }
            set {
                if (!IsStartCharacterSupported(value)) { throw new System.ArgumentOutOfRangeException("value", string.Format(CultureInfo.InvariantCulture, Resources.FormatExceptionCharacterIsNotSupported, value)); }
                _startCharacter = value;
                if (ValueEncoder != null) { ValueEncoder.Invoke(this); }
            }
        }

        private char _endCharacter;
        /// <summary>
        /// Gets/sets start character.
        /// </summary>
        public char EndCharacter {
            get { return _endCharacter; }
            set {
                if (!IsEndCharacterSupported(value)) { throw new System.ArgumentOutOfRangeException("value", string.Format(CultureInfo.InvariantCulture, Resources.FormatExceptionCharacterIsNotSupported, value)); }
                _endCharacter = value;
                if (ValueEncoder != null) { ValueEncoder.Invoke(this); }
            }
        }


        private string _value;
        /// <summary>
        /// Gets/sets value.
        /// </summary>
        public string Value {
            get { return _value; }
            set {
                if (value == null) { value = string.Empty; }
                for (int i = 0; i < value.Length; ++i) {
                    if (!IsCharacterSupported(value[i])) { throw new System.ArgumentOutOfRangeException("value", string.Format(CultureInfo.InvariantCulture, Resources.FormatExceptionCharacterIsNotSupported, value[i])); }
                }
                _value = value;
                if (ValueEncoder != null) { ValueEncoder.Invoke(this); }
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
            get { return _barWidth; }
            set {
                if (value <= 0) { throw new System.ArgumentOutOfRangeException("value", Resources.ExceptionWidthMustBeLargerThan0); }
                _barWidth = value;
            }
        }

        private float _gapWidth = 1;
        /// <summary>
        /// Gets/sets width of gaps.
        /// </summary>
        /// <exception cref="System.ArgumentOutOfRangeException">Width must be larger than 0.</exception>
        public float GapWidth {
            get { return _gapWidth; }
            set {
                if (value <= 0) { throw new System.ArgumentOutOfRangeException("value", Resources.ExceptionWidthMustBeLargerThan0); }
                _gapWidth = value;
            }
        }

        private float _spaceWidth = 1;
        /// <summary>
        /// Gets/sets width of space.
        /// </summary>
        /// <exception cref="System.ArgumentOutOfRangeException">Width must be larger than 0.</exception>
        public float SpaceWidth {
            get { return _spaceWidth; }
            set {
                if (value <= 0) { throw new System.ArgumentOutOfRangeException("value", Resources.ExceptionWidthMustBeLargerThan0); }
                _spaceWidth = value;
            }
        }

        /// <summary>
        /// Sets width of bars, gaps and spaces.
        /// </summary>
        /// <exception cref="System.ArgumentOutOfRangeException">Width must be larger than 0.</exception>
        public void SetElementWidth(float value) {
            if (value <= 0) { throw new System.ArgumentOutOfRangeException("value", Resources.ExceptionWidthMustBeLargerThan0); }
            _barWidth = value;
            _gapWidth = value;
            _spaceWidth = value;
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

                using (SolidBrush barBrush = new SolidBrush(BarColor))
                using (SolidBrush gapBrush = new SolidBrush(GapColor))
                using (SolidBrush spaceBrush = new SolidBrush(SpaceColor)) {

                    for (int i = 0; i < EncodedValue.Length; i++) {
                        switch (i % 2) {
                            case 0: { //bar
                                    int multiplier = EncodedValue[i];
                                    graphics.FillRectangle(barBrush, currLeft, top, multiplier * BarWidth, height);
                                    currLeft += multiplier * BarWidth;
                                    break;
                                }
                            case 1: { //gap or space
                                    int multiplier = EncodedValue[i];
                                    if (multiplier != 0) { //gap
                                        graphics.FillRectangle(gapBrush, currLeft, top, multiplier * GapWidth, height);
                                        currLeft += multiplier * GapWidth;
                                    } else { //space
                                        graphics.FillRectangle(gapBrush, currLeft, top, SpaceWidth, height);
                                        currLeft += SpaceWidth;
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
            _allowedStartCharacters = new List<char> {
                'A',
                'B',
                'C',
                'D'
            };

            _allowedEndCharacters = new List<char> {
                'A',
                'B',
                'C',
                'D'
            };

            _allowedValueCharacters = new List<char> {
                '0',
                '1',
                '2',
                '3',
                '4',
                '5',
                '6',
                '7',
                '8',
                '9',
                '-',
                '$',
                ':',
                '/',
                '.',
                '+'
            };


            _codabarStartStopEncoding = new Dictionary<char, int[]> {
                { 'A', new int[] { 1, 1, 2, 2, 1, 2, 1 } },
                { 'B', new int[] { 1, 1, 1, 2, 1, 2, 2 } },
                { 'C', new int[] { 1, 2, 1, 2, 1, 1, 2 } },
                { 'D', new int[] { 1, 1, 1, 2, 2, 2, 1 } }
            };

            _codabarValueEncoding = new Dictionary<char, int[]> {
                { '0', new int[] { 1, 1, 1, 1, 1, 2, 2 } },
                { '1', new int[] { 1, 1, 1, 1, 2, 2, 1 } },
                { '2', new int[] { 1, 1, 1, 2, 1, 1, 2 } },
                { '3', new int[] { 2, 2, 1, 1, 1, 1, 1 } },
                { '4', new int[] { 1, 1, 2, 1, 1, 2, 1 } },
                { '5', new int[] { 2, 1, 1, 1, 1, 2, 1 } },
                { '6', new int[] { 1, 2, 1, 1, 1, 1, 2 } },
                { '7', new int[] { 1, 2, 1, 1, 2, 1, 1 } },
                { '8', new int[] { 1, 2, 2, 1, 1, 1, 1 } },
                { '9', new int[] { 2, 1, 1, 2, 1, 1, 1 } },
                { '-', new int[] { 1, 1, 1, 2, 2, 1, 1 } },
                { '$', new int[] { 1, 1, 2, 2, 1, 1, 1 } },
                { ':', new int[] { 2, 1, 1, 1, 2, 1, 2 } },
                { '/', new int[] { 2, 1, 2, 1, 1, 1, 2 } },
                { '.', new int[] { 2, 1, 2, 1, 2, 1, 1 } },
                { '+', new int[] { 1, 1, 2, 1, 2, 1, 2 } }
            };
        }

        private static void CodabarValueEncoder(BarcodeImage barcode) {
            var ev = new List<int>();
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
			_allowedStartCharacters = new List<char>();
			_allowedEndCharacters = new List<char>();
			_allowedValueCharacters = new List<char>();
            for (int i = 0; i < 128; ++i) {
				_allowedValueCharacters.Add(System.Convert.ToChar(i));
            }

            InitCode128Lookup();
            InitCode128MappingLookup();
        }

        private void InitCode128MappingLookup() {
            _code128MappingLookup = new Dictionary<int, string>[] { new Dictionary<int, string>(), new Dictionary<int, string>(), new Dictionary<int, string>() };
            InitCode128MappingLookup0();
            InitCode128MappingLookup1();
            InitCode128MappingLookup2();
        }

        private void InitCode128MappingLookup2() {
            _code128MappingLookup[2].Add(0, "00");
            _code128MappingLookup[2].Add(1, "01");
            _code128MappingLookup[2].Add(2, "02");
            _code128MappingLookup[2].Add(3, "03");
            _code128MappingLookup[2].Add(4, "04");
            _code128MappingLookup[2].Add(5, "05");
            _code128MappingLookup[2].Add(6, "06");
            _code128MappingLookup[2].Add(7, "07");
            _code128MappingLookup[2].Add(8, "08");
            _code128MappingLookup[2].Add(9, "09");
            _code128MappingLookup[2].Add(10, "10");
            _code128MappingLookup[2].Add(11, "11");
            _code128MappingLookup[2].Add(12, "12");
            _code128MappingLookup[2].Add(13, "13");
            _code128MappingLookup[2].Add(14, "14");
            _code128MappingLookup[2].Add(15, "15");
            _code128MappingLookup[2].Add(16, "16");
            _code128MappingLookup[2].Add(17, "17");
            _code128MappingLookup[2].Add(18, "18");
            _code128MappingLookup[2].Add(19, "19");
            _code128MappingLookup[2].Add(20, "20");
            _code128MappingLookup[2].Add(21, "21");
            _code128MappingLookup[2].Add(22, "22");
            _code128MappingLookup[2].Add(23, "23");
            _code128MappingLookup[2].Add(24, "24");
            _code128MappingLookup[2].Add(25, "25");
            _code128MappingLookup[2].Add(26, "26");
            _code128MappingLookup[2].Add(27, "27");
            _code128MappingLookup[2].Add(28, "28");
            _code128MappingLookup[2].Add(29, "29");
            _code128MappingLookup[2].Add(30, "30");
            _code128MappingLookup[2].Add(31, "31");
            _code128MappingLookup[2].Add(32, "32");
            _code128MappingLookup[2].Add(33, "33");
            _code128MappingLookup[2].Add(34, "34");
            _code128MappingLookup[2].Add(35, "35");
            _code128MappingLookup[2].Add(36, "36");
            _code128MappingLookup[2].Add(37, "37");
            _code128MappingLookup[2].Add(38, "38");
            _code128MappingLookup[2].Add(39, "39");
            _code128MappingLookup[2].Add(40, "40");
            _code128MappingLookup[2].Add(41, "41");
            _code128MappingLookup[2].Add(42, "42");
            _code128MappingLookup[2].Add(43, "43");
            _code128MappingLookup[2].Add(44, "44");
            _code128MappingLookup[2].Add(45, "45");
            _code128MappingLookup[2].Add(46, "46");
            _code128MappingLookup[2].Add(47, "47");
            _code128MappingLookup[2].Add(48, "48");
            _code128MappingLookup[2].Add(49, "49");
            _code128MappingLookup[2].Add(50, "50");
            _code128MappingLookup[2].Add(51, "51");
            _code128MappingLookup[2].Add(52, "52");
            _code128MappingLookup[2].Add(53, "53");
            _code128MappingLookup[2].Add(54, "54");
            _code128MappingLookup[2].Add(55, "55");
            _code128MappingLookup[2].Add(56, "56");
            _code128MappingLookup[2].Add(57, "57");
            _code128MappingLookup[2].Add(58, "58");
            _code128MappingLookup[2].Add(59, "59");
            _code128MappingLookup[2].Add(60, "60");
            _code128MappingLookup[2].Add(61, "61");
            _code128MappingLookup[2].Add(62, "62");
            _code128MappingLookup[2].Add(63, "63");
            _code128MappingLookup[2].Add(64, "64");
            _code128MappingLookup[2].Add(65, "65");
            _code128MappingLookup[2].Add(66, "66");
            _code128MappingLookup[2].Add(67, "67");
            _code128MappingLookup[2].Add(68, "68");
            _code128MappingLookup[2].Add(69, "69");
            _code128MappingLookup[2].Add(70, "70");
            _code128MappingLookup[2].Add(71, "71");
            _code128MappingLookup[2].Add(72, "72");
            _code128MappingLookup[2].Add(73, "73");
            _code128MappingLookup[2].Add(74, "74");
            _code128MappingLookup[2].Add(75, "75");
            _code128MappingLookup[2].Add(76, "76");
            _code128MappingLookup[2].Add(77, "77");
            _code128MappingLookup[2].Add(78, "78");
            _code128MappingLookup[2].Add(79, "79");
            _code128MappingLookup[2].Add(80, "80");
            _code128MappingLookup[2].Add(81, "81");
            _code128MappingLookup[2].Add(82, "82");
            _code128MappingLookup[2].Add(83, "83");
            _code128MappingLookup[2].Add(84, "84");
            _code128MappingLookup[2].Add(85, "85");
            _code128MappingLookup[2].Add(86, "86");
            _code128MappingLookup[2].Add(87, "87");
            _code128MappingLookup[2].Add(88, "88");
            _code128MappingLookup[2].Add(89, "89");
            _code128MappingLookup[2].Add(90, "90");
            _code128MappingLookup[2].Add(91, "91");
            _code128MappingLookup[2].Add(92, "92");
            _code128MappingLookup[2].Add(93, "93");
            _code128MappingLookup[2].Add(94, "94");
            _code128MappingLookup[2].Add(95, "95");
            _code128MappingLookup[2].Add(96, "96");
            _code128MappingLookup[2].Add(97, "97");
            _code128MappingLookup[2].Add(98, "98");
            _code128MappingLookup[2].Add(99, "99");
        }

        private void InitCode128MappingLookup1() {
            _code128MappingLookup[1].Add(0, " ");
            _code128MappingLookup[1].Add(1, "!");
            _code128MappingLookup[1].Add(2, "\"");
            _code128MappingLookup[1].Add(3, "#");
            _code128MappingLookup[1].Add(4, "$");
            _code128MappingLookup[1].Add(5, "%");
            _code128MappingLookup[1].Add(6, "&");
            _code128MappingLookup[1].Add(7, "'");
            _code128MappingLookup[1].Add(8, "(");
            _code128MappingLookup[1].Add(9, ")");
            _code128MappingLookup[1].Add(10, "*");
            _code128MappingLookup[1].Add(11, "+");
            _code128MappingLookup[1].Add(12, ",");
            _code128MappingLookup[1].Add(13, "-");
            _code128MappingLookup[1].Add(14, ".");
            _code128MappingLookup[1].Add(15, "/");
            _code128MappingLookup[1].Add(16, "0");
            _code128MappingLookup[1].Add(17, "1");
            _code128MappingLookup[1].Add(18, "2");
            _code128MappingLookup[1].Add(19, "3");
            _code128MappingLookup[1].Add(20, "4");
            _code128MappingLookup[1].Add(21, "5");
            _code128MappingLookup[1].Add(22, "6");
            _code128MappingLookup[1].Add(23, "7");
            _code128MappingLookup[1].Add(24, "8");
            _code128MappingLookup[1].Add(25, "9");
            _code128MappingLookup[1].Add(26, ":");
            _code128MappingLookup[1].Add(27, ";");
            _code128MappingLookup[1].Add(28, "<");
            _code128MappingLookup[1].Add(29, "=");
            _code128MappingLookup[1].Add(30, ">");
            _code128MappingLookup[1].Add(31, "?");
            _code128MappingLookup[1].Add(32, "@");
            _code128MappingLookup[1].Add(33, "A");
            _code128MappingLookup[1].Add(34, "B");
            _code128MappingLookup[1].Add(35, "C");
            _code128MappingLookup[1].Add(36, "D");
            _code128MappingLookup[1].Add(37, "E");
            _code128MappingLookup[1].Add(38, "F");
            _code128MappingLookup[1].Add(39, "G");
            _code128MappingLookup[1].Add(40, "H");
            _code128MappingLookup[1].Add(41, "I");
            _code128MappingLookup[1].Add(42, "J");
            _code128MappingLookup[1].Add(43, "K");
            _code128MappingLookup[1].Add(44, "L");
            _code128MappingLookup[1].Add(45, "M");
            _code128MappingLookup[1].Add(46, "N");
            _code128MappingLookup[1].Add(47, "O");
            _code128MappingLookup[1].Add(48, "P");
            _code128MappingLookup[1].Add(49, "Q");
            _code128MappingLookup[1].Add(50, "R");
            _code128MappingLookup[1].Add(51, "S");
            _code128MappingLookup[1].Add(52, "T");
            _code128MappingLookup[1].Add(53, "U");
            _code128MappingLookup[1].Add(54, "V");
            _code128MappingLookup[1].Add(55, "W");
            _code128MappingLookup[1].Add(56, "X");
            _code128MappingLookup[1].Add(57, "Y");
            _code128MappingLookup[1].Add(58, "Z");
            _code128MappingLookup[1].Add(59, "[");
            _code128MappingLookup[1].Add(60, "\\");
            _code128MappingLookup[1].Add(61, "]");
            _code128MappingLookup[1].Add(62, "^");
            _code128MappingLookup[1].Add(63, "_");
            _code128MappingLookup[1].Add(64, "`");
            _code128MappingLookup[1].Add(65, "a");
            _code128MappingLookup[1].Add(66, "b");
            _code128MappingLookup[1].Add(67, "c");
            _code128MappingLookup[1].Add(68, "d");
            _code128MappingLookup[1].Add(69, "e");
            _code128MappingLookup[1].Add(70, "f");
            _code128MappingLookup[1].Add(71, "g");
            _code128MappingLookup[1].Add(72, "h");
            _code128MappingLookup[1].Add(73, "i");
            _code128MappingLookup[1].Add(74, "j");
            _code128MappingLookup[1].Add(75, "k");
            _code128MappingLookup[1].Add(76, "l");
            _code128MappingLookup[1].Add(77, "m");
            _code128MappingLookup[1].Add(78, "n");
            _code128MappingLookup[1].Add(79, "o");
            _code128MappingLookup[1].Add(80, "p");
            _code128MappingLookup[1].Add(81, "q");
            _code128MappingLookup[1].Add(82, "r");
            _code128MappingLookup[1].Add(83, "s");
            _code128MappingLookup[1].Add(84, "t");
            _code128MappingLookup[1].Add(85, "u");
            _code128MappingLookup[1].Add(86, "v");
            _code128MappingLookup[1].Add(87, "w");
            _code128MappingLookup[1].Add(88, "x");
            _code128MappingLookup[1].Add(89, "y");
            _code128MappingLookup[1].Add(90, "z");
            _code128MappingLookup[1].Add(91, "{");
            _code128MappingLookup[1].Add(92, "|");
            _code128MappingLookup[1].Add(93, "}");
            _code128MappingLookup[1].Add(94, "~");
            _code128MappingLookup[1].Add(95, System.Convert.ToChar(127).ToString());
        }

        private void InitCode128MappingLookup0() {
            _code128MappingLookup[0].Add(0, " ");
            _code128MappingLookup[0].Add(1, "!");
            _code128MappingLookup[0].Add(2, "\"");
            _code128MappingLookup[0].Add(3, "#");
            _code128MappingLookup[0].Add(4, "$");
            _code128MappingLookup[0].Add(5, "%");
            _code128MappingLookup[0].Add(6, "&");
            _code128MappingLookup[0].Add(7, "'");
            _code128MappingLookup[0].Add(8, "(");
            _code128MappingLookup[0].Add(9, ")");
            _code128MappingLookup[0].Add(10, "*");
            _code128MappingLookup[0].Add(11, "+");
            _code128MappingLookup[0].Add(12, ",");
            _code128MappingLookup[0].Add(13, "-");
            _code128MappingLookup[0].Add(14, ".");
            _code128MappingLookup[0].Add(15, "/");
            _code128MappingLookup[0].Add(16, "0");
            _code128MappingLookup[0].Add(17, "1");
            _code128MappingLookup[0].Add(18, "2");
            _code128MappingLookup[0].Add(19, "3");
            _code128MappingLookup[0].Add(20, "4");
            _code128MappingLookup[0].Add(21, "5");
            _code128MappingLookup[0].Add(22, "6");
            _code128MappingLookup[0].Add(23, "7");
            _code128MappingLookup[0].Add(24, "8");
            _code128MappingLookup[0].Add(25, "9");
            _code128MappingLookup[0].Add(26, ":");
            _code128MappingLookup[0].Add(27, ";");
            _code128MappingLookup[0].Add(28, "<");
            _code128MappingLookup[0].Add(29, "=");
            _code128MappingLookup[0].Add(30, ">");
            _code128MappingLookup[0].Add(31, "?");
            _code128MappingLookup[0].Add(32, "@");
            _code128MappingLookup[0].Add(33, "A");
            _code128MappingLookup[0].Add(34, "B");
            _code128MappingLookup[0].Add(35, "C");
            _code128MappingLookup[0].Add(36, "D");
            _code128MappingLookup[0].Add(37, "E");
            _code128MappingLookup[0].Add(38, "F");
            _code128MappingLookup[0].Add(39, "G");
            _code128MappingLookup[0].Add(40, "H");
            _code128MappingLookup[0].Add(41, "I");
            _code128MappingLookup[0].Add(42, "J");
            _code128MappingLookup[0].Add(43, "K");
            _code128MappingLookup[0].Add(44, "L");
            _code128MappingLookup[0].Add(45, "M");
            _code128MappingLookup[0].Add(46, "N");
            _code128MappingLookup[0].Add(47, "O");
            _code128MappingLookup[0].Add(48, "P");
            _code128MappingLookup[0].Add(49, "Q");
            _code128MappingLookup[0].Add(50, "R");
            _code128MappingLookup[0].Add(51, "S");
            _code128MappingLookup[0].Add(52, "T");
            _code128MappingLookup[0].Add(53, "U");
            _code128MappingLookup[0].Add(54, "V");
            _code128MappingLookup[0].Add(55, "W");
            _code128MappingLookup[0].Add(56, "X");
            _code128MappingLookup[0].Add(57, "Y");
            _code128MappingLookup[0].Add(58, "Z");
            _code128MappingLookup[0].Add(59, "[");
            _code128MappingLookup[0].Add(60, "\\");
            _code128MappingLookup[0].Add(61, "]");
            _code128MappingLookup[0].Add(62, "^");
            _code128MappingLookup[0].Add(63, "_");
            _code128MappingLookup[0].Add(64, System.Convert.ToChar(0).ToString());
            _code128MappingLookup[0].Add(65, System.Convert.ToChar(1).ToString());
            _code128MappingLookup[0].Add(66, System.Convert.ToChar(2).ToString());
            _code128MappingLookup[0].Add(67, System.Convert.ToChar(3).ToString());
            _code128MappingLookup[0].Add(68, System.Convert.ToChar(4).ToString());
            _code128MappingLookup[0].Add(69, System.Convert.ToChar(5).ToString());
            _code128MappingLookup[0].Add(70, System.Convert.ToChar(6).ToString());
            _code128MappingLookup[0].Add(71, System.Convert.ToChar(7).ToString());
            _code128MappingLookup[0].Add(72, System.Convert.ToChar(8).ToString());
            _code128MappingLookup[0].Add(73, System.Convert.ToChar(9).ToString());
            _code128MappingLookup[0].Add(74, System.Convert.ToChar(10).ToString());
            _code128MappingLookup[0].Add(75, System.Convert.ToChar(11).ToString());
            _code128MappingLookup[0].Add(76, System.Convert.ToChar(12).ToString());
            _code128MappingLookup[0].Add(77, System.Convert.ToChar(13).ToString());
            _code128MappingLookup[0].Add(78, System.Convert.ToChar(14).ToString());
            _code128MappingLookup[0].Add(79, System.Convert.ToChar(15).ToString());
            _code128MappingLookup[0].Add(80, System.Convert.ToChar(16).ToString());
            _code128MappingLookup[0].Add(81, System.Convert.ToChar(17).ToString());
            _code128MappingLookup[0].Add(82, System.Convert.ToChar(18).ToString());
            _code128MappingLookup[0].Add(83, System.Convert.ToChar(19).ToString());
            _code128MappingLookup[0].Add(84, System.Convert.ToChar(20).ToString());
            _code128MappingLookup[0].Add(85, System.Convert.ToChar(21).ToString());
            _code128MappingLookup[0].Add(86, System.Convert.ToChar(22).ToString());
            _code128MappingLookup[0].Add(87, System.Convert.ToChar(23).ToString());
            _code128MappingLookup[0].Add(88, System.Convert.ToChar(24).ToString());
            _code128MappingLookup[0].Add(89, System.Convert.ToChar(25).ToString());
            _code128MappingLookup[0].Add(90, System.Convert.ToChar(26).ToString());
            _code128MappingLookup[0].Add(91, System.Convert.ToChar(27).ToString());
            _code128MappingLookup[0].Add(92, System.Convert.ToChar(28).ToString());
            _code128MappingLookup[0].Add(93, System.Convert.ToChar(29).ToString());
            _code128MappingLookup[0].Add(94, System.Convert.ToChar(30).ToString());
            _code128MappingLookup[0].Add(95, System.Convert.ToChar(31).ToString());
        }

        private void InitCode128Lookup() {
            _code128EncodingLookup = new Dictionary<int, int[]> {
                { 0, new int[] { 2, 1, 2, 2, 2, 2 } },
                { 1, new int[] { 2, 2, 2, 1, 2, 2 } },
                { 2, new int[] { 2, 2, 2, 2, 2, 1 } },
                { 3, new int[] { 1, 2, 1, 2, 2, 3 } },
                { 4, new int[] { 1, 2, 1, 3, 2, 2 } },
                { 5, new int[] { 1, 3, 1, 2, 2, 2 } },
                { 6, new int[] { 1, 2, 2, 2, 1, 3 } },
                { 7, new int[] { 1, 2, 2, 3, 1, 2 } },
                { 8, new int[] { 1, 3, 2, 2, 1, 2 } },
                { 9, new int[] { 2, 2, 1, 2, 1, 3 } },
                { 10, new int[] { 2, 2, 1, 3, 1, 2 } },
                { 11, new int[] { 2, 3, 1, 2, 1, 2 } },
                { 12, new int[] { 1, 1, 2, 2, 3, 2 } },
                { 13, new int[] { 1, 2, 2, 1, 3, 2 } },
                { 14, new int[] { 1, 2, 2, 2, 3, 1 } },
                { 15, new int[] { 1, 1, 3, 2, 2, 2 } },
                { 16, new int[] { 1, 2, 3, 1, 2, 2 } },
                { 17, new int[] { 1, 2, 3, 2, 2, 1 } },
                { 18, new int[] { 2, 2, 3, 2, 1, 1 } },
                { 19, new int[] { 2, 2, 1, 1, 3, 2 } },
                { 20, new int[] { 2, 2, 1, 2, 3, 1 } },
                { 21, new int[] { 2, 1, 3, 2, 1, 2 } },
                { 22, new int[] { 2, 2, 3, 1, 1, 2 } },
                { 23, new int[] { 3, 1, 2, 1, 3, 1 } },
                { 24, new int[] { 3, 1, 1, 2, 2, 2 } },
                { 25, new int[] { 3, 2, 1, 1, 2, 2 } },
                { 26, new int[] { 3, 2, 1, 2, 2, 1 } },
                { 27, new int[] { 3, 1, 2, 2, 1, 2 } },
                { 28, new int[] { 3, 2, 2, 1, 1, 2 } },
                { 29, new int[] { 3, 2, 2, 2, 1, 1 } },
                { 30, new int[] { 2, 1, 2, 1, 2, 3 } },
                { 31, new int[] { 2, 1, 2, 3, 2, 1 } },
                { 32, new int[] { 2, 3, 2, 1, 2, 1 } },
                { 33, new int[] { 1, 1, 1, 3, 2, 3 } },
                { 34, new int[] { 1, 3, 1, 1, 2, 3 } },
                { 35, new int[] { 1, 3, 1, 3, 2, 1 } },
                { 36, new int[] { 1, 1, 2, 3, 1, 3 } },
                { 37, new int[] { 1, 3, 2, 1, 1, 3 } },
                { 38, new int[] { 1, 3, 2, 3, 1, 1 } },
                { 39, new int[] { 2, 1, 1, 3, 1, 3 } },
                { 40, new int[] { 2, 3, 1, 1, 1, 3 } },
                { 41, new int[] { 2, 3, 1, 3, 1, 1 } },
                { 42, new int[] { 1, 1, 2, 1, 3, 3 } },
                { 43, new int[] { 1, 1, 2, 3, 3, 1 } },
                { 44, new int[] { 1, 3, 2, 1, 3, 1 } },
                { 45, new int[] { 1, 1, 3, 1, 2, 3 } },
                { 46, new int[] { 1, 1, 3, 3, 2, 1 } },
                { 47, new int[] { 1, 3, 3, 1, 2, 1 } },
                { 48, new int[] { 3, 1, 3, 1, 2, 1 } },
                { 49, new int[] { 2, 1, 1, 3, 3, 1 } },
                { 50, new int[] { 2, 3, 1, 1, 3, 1 } },
                { 51, new int[] { 2, 1, 3, 1, 1, 3 } },
                { 52, new int[] { 2, 1, 3, 3, 1, 1 } },
                { 53, new int[] { 2, 1, 3, 1, 3, 1 } },
                { 54, new int[] { 3, 1, 1, 1, 2, 3 } },
                { 55, new int[] { 3, 1, 1, 3, 2, 1 } },
                { 56, new int[] { 3, 3, 1, 1, 2, 1 } },
                { 57, new int[] { 3, 1, 2, 1, 1, 3 } },
                { 58, new int[] { 3, 1, 2, 3, 1, 1 } },
                { 59, new int[] { 3, 3, 2, 1, 1, 1 } },
                { 60, new int[] { 3, 1, 4, 1, 1, 1 } },
                { 61, new int[] { 2, 2, 1, 4, 1, 1 } },
                { 62, new int[] { 4, 3, 1, 1, 1, 1 } },
                { 63, new int[] { 1, 1, 1, 2, 2, 4 } },
                { 64, new int[] { 1, 1, 1, 4, 2, 2 } },
                { 65, new int[] { 1, 2, 1, 1, 2, 4 } },
                { 66, new int[] { 1, 2, 1, 4, 2, 1 } },
                { 67, new int[] { 1, 4, 1, 1, 2, 2 } },
                { 68, new int[] { 1, 4, 1, 2, 2, 1 } },
                { 69, new int[] { 1, 1, 2, 2, 1, 4 } },
                { 70, new int[] { 1, 1, 2, 4, 1, 2 } },
                { 71, new int[] { 1, 2, 2, 1, 1, 4 } },
                { 72, new int[] { 1, 2, 2, 4, 1, 1 } },
                { 73, new int[] { 1, 4, 2, 1, 1, 2 } },
                { 74, new int[] { 1, 4, 2, 2, 1, 1 } },
                { 75, new int[] { 2, 4, 1, 2, 1, 1 } },
                { 76, new int[] { 2, 2, 1, 1, 1, 4 } },
                { 77, new int[] { 4, 1, 3, 1, 1, 1 } },
                { 78, new int[] { 2, 4, 1, 1, 1, 2 } },
                { 79, new int[] { 1, 3, 4, 1, 1, 1 } },
                { 80, new int[] { 1, 1, 1, 2, 4, 2 } },
                { 81, new int[] { 1, 2, 1, 1, 4, 2 } },
                { 82, new int[] { 1, 2, 1, 2, 4, 1 } },
                { 83, new int[] { 1, 1, 4, 2, 1, 2 } },
                { 84, new int[] { 1, 2, 4, 1, 1, 2 } },
                { 85, new int[] { 1, 2, 4, 2, 1, 1 } },
                { 86, new int[] { 4, 1, 1, 2, 1, 2 } },
                { 87, new int[] { 4, 2, 1, 1, 1, 2 } },
                { 88, new int[] { 4, 2, 1, 2, 1, 1 } },
                { 89, new int[] { 2, 1, 2, 1, 4, 1 } },
                { 90, new int[] { 2, 1, 4, 1, 2, 1 } },
                { 91, new int[] { 4, 1, 2, 1, 2, 1 } },
                { 92, new int[] { 1, 1, 1, 1, 4, 3 } },
                { 93, new int[] { 1, 1, 1, 3, 4, 1 } },
                { 94, new int[] { 1, 3, 1, 1, 4, 1 } },
                { 95, new int[] { 1, 1, 4, 1, 1, 3 } },
                { 96, new int[] { 1, 1, 4, 3, 1, 1 } },
                { 97, new int[] { 4, 1, 1, 1, 1, 3 } },
                { 98, new int[] { 4, 1, 1, 3, 1, 1 } },
                { 99, new int[] { 1, 1, 3, 1, 4, 1 } },
                { 100, new int[] { 1, 1, 4, 1, 3, 1 } },
                { 101, new int[] { 3, 1, 1, 1, 4, 1 } },
                { 102, new int[] { 4, 1, 1, 1, 3, 1 } },
                { 103, new int[] { 2, 1, 1, 4, 1, 2 } },
                { 104, new int[] { 2, 1, 1, 2, 1, 4 } },
                { 105, new int[] { 2, 1, 1, 2, 3, 2 } },
                { 106, new int[] { 2, 3, 3, 1, 1, 1, 2 } }
            };
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
