/* Josip Medved <jmedved@jmedved.com> * www.medo64.com * MIT License */

//2011-03-05: Moved to Medo.Math.
//2010-05-14: Changed namespace from Medo.Math.Averaging to Medo.Math.
//2008-04-11: Cleaned code to match FxCop 1.36 beta 2 (DoNotRaiseExceptionsInUnexpectedLocations).
//2008-01-05: Moved to Medo.Math.Averaging.
//2008-01-03: Added Resources.
//2007-09-19: Moved to common.


namespace Medo.Math {

	/// <summary>
	/// Calculates exponential moving average for added items.
	/// </summary>
	/// <remarks>
	/// All calculations are done with floats in order to harvest maximum precision.
	/// </remarks>
	public class ExponentialMovingAverage {

		private readonly double _smoothingFactor;
		private double _value;
		private bool _isFilled;


		/// <summary>
		/// Creates new instance with smoothing factor for 10 items (18.18%).
		/// </summary>
		public ExponentialMovingAverage()
			: this(10) {
		}

		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="count">Number of items to use for calculation of smoothing factor.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "count+1", Justification = "This will not overflow.")]
		public ExponentialMovingAverage(int count) {
			if (count == int.MaxValue) {
				_smoothingFactor = 2.0 / (int.MaxValue);
			} else {
				_smoothingFactor = 2.0 / (count + 1);
			}
		}

		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="smoothingFactor">Smoothing factor. Must be between 0 and 1.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">Smoothing factor must be between 0 and 1.</exception>
		public ExponentialMovingAverage(double smoothingFactor) {
			if ((smoothingFactor < 0) || (smoothingFactor > 1)) { throw new System.ArgumentOutOfRangeException("smoothingFactor", Resources.ExceptionSmoothingFactorMustBeBetween0And1); }
			_smoothingFactor = smoothingFactor;
		}

		/// <summary>
		/// Adds an item and returns current average.
		/// </summary>
		/// <param name="value">Value to be added.</param>
		public void Add(double value) {
			if (!_isFilled) {
				_value = value;
				_isFilled = true;
			} else {
				_value += _smoothingFactor * (value - _value);
			}
		}

		/// <summary>
		/// Resets average.
		/// </summary>
		public void Clear() {
			_isFilled = false;
		}

		/// <summary>
		/// Gets whether there are items inside.
		/// </summary>
		public bool IsEmpty {
			get { return !_isFilled; }
		}


		/// <summary>
		/// Returns average or NaN if there is no data to calculate
		/// </summary>
		public double Average {
			get {
				if (IsEmpty) {
					return double.NaN;
				} else {
					return _value;
				}
			}
		}


		private static class Resources {

			internal static string ExceptionSmoothingFactorMustBeBetween0And1 { get { return "Smoothing factor must be between 0 and 1."; } }

		}

	}

}
