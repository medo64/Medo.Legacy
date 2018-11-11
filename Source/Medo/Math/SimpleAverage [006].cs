/* Josip Medved <jmedved@jmedved.com> * www.medo64.com * MIT License */

//2011-03-05: Moved to Medo.Math.
//2010-05-14: Changed namespace from Medo.Math.Averaging to Medo.Math.
//2008-04-11: Cleaned code to match FxCop 1.36 beta 2 (DoNotRaiseExceptionsInUnexpectedLocations).
//2008-01-05: Moved to Medo.Math.Averaging.
//2008-01-03: Added Resources.
//2007-09-19: Moved to common.


namespace Medo.Math {

	/// <summary>
	/// Calculates average for added items.
	/// </summary>
	public class SimpleAverage {

		private double _itemsSum;
		private int _itemsCount;


		/// <summary>
		/// Creates new instance.
		/// </summary>
		public SimpleAverage() {
		}


		/// <summary>
		/// Adds an item and returns current average.
		/// </summary>
		/// <param name="value">Value to be added.</param>
		public void Add(double value) {
			_itemsSum += value;
			_itemsCount += 1;
		}

		/// <summary>
		/// Resets average.
		/// </summary>
		public void Clear() {
			_itemsSum = 0;
			_itemsCount = 0;
		}

		/// <summary>
		/// Gets whether there are items inside.
		/// </summary>
		public bool IsEmpty {
			get { return _itemsCount == 0; }
		}


        /// <summary>
        /// Returns average or NaN if there is no data to calculate.
        /// </summary>
		public double Average {
			get {
				if (IsEmpty) {
					return double.NaN;
				} else {
					return _itemsSum / _itemsCount;
				}
			}
        }



	}

}
