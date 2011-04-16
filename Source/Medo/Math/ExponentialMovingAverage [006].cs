//Copyright (c) 2007 Josip Medved <jmedved@jmedved.com>

//2007-09-19: Moved to common.
//2008-01-03: Added Resources.
//2008-01-05: Moved to Medo.Math.Averaging.
//2008-04-11: Cleaned code to match FxCop 1.36 beta 2 (DoNotRaiseExceptionsInUnexpectedLocations).
//2010-05-14: Changed namespace from Medo.Math.Averaging to Medo.Math.
//2011-03-05: Moved to Medo.Math.


namespace Medo.Math {

	/// <summary>
	/// Calculates exponential moving average for added items.
	/// </summary>
	/// <remarks>
	/// All calculations are done with floats in order to harvest maximum precision.
	/// </remarks>
	public class ExponentialMovingAverage {

		private double _smoothingFactor;
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
				this._smoothingFactor = 2.0 / (int.MaxValue);
			} else {
				this._smoothingFactor = 2.0 / (count + 1);
			}
		}

		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="smoothingFactor">Smoothing factor. Must be between 0 and 1.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">Smoothing factor must be between 0 and 1.</exception>
		public ExponentialMovingAverage(double smoothingFactor) {
			if ((smoothingFactor < 0) || (smoothingFactor > 1)) { throw new System.ArgumentOutOfRangeException("smoothingFactor", Resources.ExceptionSmoothingFactorMustBeBetween0And1); }
			this._smoothingFactor = smoothingFactor;
		}

		/// <summary>
		/// Adds an item and returns current average.
		/// </summary>
		/// <param name="value">Value to be added.</param>
		public void Add(double value) {
			if (!_isFilled) {
				this._value = value;
				_isFilled = true;
			} else {
				this._value = this._value + this._smoothingFactor * (value - this._value);
			}
		}

		/// <summary>
		/// Resets average.
		/// </summary>
		public void Clear() {
			this._isFilled = false;
		}

		/// <summary>
		/// Gets whether there are items inside.
		/// </summary>
		public bool IsEmpty {
			get { return !this._isFilled; }
		}


		/// <summary>
		/// Returns average or NaN if there is no data to calculate
		/// </summary>
		public double Average {
			get {
				if (this.IsEmpty) {
					return double.NaN;
				} else {
					return this._value;
				}
			}
		}


		private static class Resources {

			internal static string ExceptionSmoothingFactorMustBeBetween0And1 { get { return "Smoothing factor must be between 0 and 1."; } }

		}

	}

}