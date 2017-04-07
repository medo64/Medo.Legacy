//Josip Medved <jmedved@jmedved.com>   www.medo64.com

//2008-04-09: First version.


using System;
using System.Collections.Generic;
using System.Drawing;

namespace Medo.Drawing {

	/// <summary>
	/// Calculations of splitted areas (e.g. for paper labels).
	/// </summary>
	public class AreaSplitter {

		private AreaSplitter() { }


        /// <summary>
        /// Returns new table area.
        /// </summary>
        /// <param name="columns">Number of columns.</param>
        /// <param name="rows">Number of rows.</param>
        /// <param name="itemWidth">Width of single item.</param>
        /// <param name="itemHeight">Height of single item.</param>
        /// <param name="offsetX">Offset from left edge for leftmost item.</param>
        /// <param name="offsetY">Offset from top edge for topmost item.</param>
        /// <param name="spacingX">Spacing between items on horizontal plane.</param>
        /// <param name="spacingY">Spacing between items on vertical plane.</param>
		public static AreaSplitter GetNewTableArea(int columns, int rows, float itemWidth, float itemHeight, float offsetX, float offsetY, float spacingX, float spacingY) {
			List<RectangleF> rects = new List<RectangleF>();
			for (int i = 0; i < columns; ++i) {
				for (int j = 0; j < rows; ++j) {
					float x = offsetX + i * (spacingX + itemWidth);
					float y = offsetY + j * (spacingY + itemHeight);
					rects.Add(new RectangleF(x, y, itemWidth, itemHeight));
				}
			}

			AreaSplitter area = new AreaSplitter();
			area._rectangles = rects.ToArray();
			return area;
		}

		private RectangleF[] _rectangles;

        /// <summary>
        /// Gets number of items.
        /// </summary>
		public int Count {
			get { return this._rectangles.Length; }
		}

        /// <summary>
        /// Gets single item.
        /// </summary>
        /// <param name="index">Index of item to retrieve.</param>
        /// <returns></returns>
		public RectangleF this[int index] {
			get { return this._rectangles[index]; }
		}

        /// <summary>
        /// Returns single Rectangle.
        /// </summary>
        /// <param name="index">Index of item to retrieve.</param>
        public Rectangle GetRectangle(int index) {
            return new Rectangle((int)this._rectangles[index].Left, (int)this._rectangles[index].Top, (int)this._rectangles[index].Width, (int)this._rectangles[index].Height);
        }

        /// <summary>
        /// Returns single RectangleF.
        /// </summary>
        /// <param name="index">Index of item to retrieve.</param>
        public RectangleF GetRectangleF(int index) {
            return this._rectangles[index];
        }

	}

}
