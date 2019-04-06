/* Josip Medved <jmedved@jmedved.com> * www.medo64.com * MIT License */

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
	/// All calculations are done with decimals in order to harvest maximum precision.
	/// </remarks>
	public class WeightedMovingAverage  {

		private readonly System.Collections.Generic.List<double> _items = new System.Collections.Generic.List<double>();
		private readonly int _maxCount;


		/// <summary>
		/// Creates new instance with total of 10 items.
		/// </summary>
		public WeightedMovingAverage()
			: this(10) {
		}

		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="maxCount">Number of items to use for calculation.</param>
		public WeightedMovingAverage(int maxCount) {
			_maxCount = maxCount;
		}


		/// <summary>
		/// Adds an item.
		/// </summary>
		/// <param name="value">Value to be added.</param>
		public void Add(double value) {
			_items.Add(value);
			while (_items.Count > _maxCount) {
				_items.RemoveAt(0);
			}
		}

		/// <summary>
		/// Resets average.
		/// </summary>
		public void Clear() {
			_items.Clear();
		}

		/// <summary>
		/// Gets whether there are items inside.
		/// </summary>
		public bool IsEmpty {
			get { return _items.Count == 0; }
		}


		/// <summary>
		/// Returns average or NaN if there is no data to calculate.
		/// </summary>
		public double Average {
			get{
				if (IsEmpty) {
					return double.NaN;
				} else {
					var count = _items.Count;
					var divider = 0;
					double sum = 0;
					for (var i = 0; i < count; i++) {
						sum += _items[i] * (i + 1);
						divider += (i + 1);
					}
					return sum / divider;
				}
			}
		}

	}

}
