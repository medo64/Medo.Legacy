//Josip Medved <jmedved@jmedved.com>   www.medo64.com

//2007-09-19: New version.


namespace Medo {

	/// <summary>
	/// Represents static pseudo-random number generator, a device that produces a sequence of numbers that meet certain statistical requirements for randomness.
	/// </summary>
	public static class Random {

		private static System.Random _random = new System.Random();


		/// <summary>
		/// Returns a random number within a specified range. Includes both lower and upper number in range.
		/// </summary>
		/// <param name="minValue">The inclusive lower bound of the random number returned.</param>
		/// <param name="maxValue">The inclusive upper bound of the random number returned.</param>
		/// <returns>A 32-bit signed integer that includes bith minValue and maxValue.</returns>
		public static int NextBetween(int minValue, int maxValue) {
			if (maxValue >= minValue) {
				return _random.Next(minValue, maxValue + 1);
			} else {
				return _random.Next(maxValue, minValue + 1);
			}
		}

		/// <summary>
		/// Returns a nonnegative random number.
		/// </summary>
		/// <returns>A 32-bit signed integer greater than or equal to zero and less than System.Int32.MaxValue.</returns>
		public static int Next() {
			return _random.Next();
		}

		/// <summary>
		/// Returns a nonnegative random number less than the specified maximum.
		/// </summary>
		/// <param name="maxValue">The exclusive upper bound of the random number to be generated. maxValue must be greater than or equal to zero.</param>
		/// <returns>A 32-bit signed integer greater than or equal to zero, and less than maxValue; that is, the range of return values includes zero but not maxValue.</returns>
		public static int Next(int maxValue) {
				return _random.Next(maxValue);
		}

		/// <summary>
		/// Returns a random number within a specified range.
		/// </summary>
		/// <param name="minValue">The inclusive lower bound of the random number returned.</param>
		/// <param name="maxValue">The exclusive upper bound of the random number returned. maxValue must be greater than or equal to minValue.</param>
		/// <returns>A 32-bit signed integer greater than or equal to minValue and less than maxValue; that is, the range of return values includes minValue but not maxValue. If minValue equals maxValue, minValue is returned.</returns>
		public static int Next(int minValue, int maxValue) {
			return _random.Next(minValue, maxValue);
		}

		/// <summary>
		/// Fills the elements of a specified array of bytes with random numbers.
		/// </summary>
		/// <param name="buffer">An array of bytes to contain random numbers.</param>
		///<exception cref="System.ArgumentException">Buffer is null.</exception>
		public static void NextBytes(byte[] buffer) {
			_random.NextBytes(buffer);
		}

		/// <summary>
		/// Returns a random number between 0.0 and 1.0.
		/// </summary>
		/// <returns>A double-precision floating point number greater than or equal to 0.0, and less than 1.0.</returns>
		public static double NextDouble() {
			return _random.NextDouble();
		}

	}

}
