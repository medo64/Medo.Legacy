//Josip Medved <jmedved@jmedved.com>   www.medo64.com

//2008-11-17: Added public Font, Brush and Graphics
//2008-10-29: First version.


using System;
using System.Drawing;

namespace Medo.Drawing
{

    /// <summary>
    /// Layouting text.
    /// </summary>
    public class TextLayout : IDisposable
    {

        /// <summary>
        /// Gets defined Graphics object.
        /// </summary>
        public Graphics Graphics { get; private set; }
        
        /// <summary>
        /// Gets defined Font object.
        /// </summary>
        public Font Font { get; private set; }

        /// <summary>
        /// Get defined Brush object.
        /// </summary>
        public Brush Brush { get; private set; }


        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="graphics">Graphics surface on which text will be drawn.</param>
        /// <param name="font">Font to use for drawing text.</param>
        /// <param name="brush">Brush for drawing text.</param>
        public TextLayout(Graphics graphics, Font font, Brush brush)
        {
            this.Graphics = graphics;
            this.Font = font;
            this.Brush = brush;
        }


        /// <summary>
        /// Draws text at the specified location and returns bounding rectangle.
        /// </summary>
        /// <param name="text">Text to draw.</param>
        /// <param name="x">X-coordinate of origin.</param>
        /// <param name="y">Y-coordinate of origin.</param>
        /// <param name="alignment">Alignment of text in regards to x and y.</param>
        public RectangleF DrawString(string text, float x, float y, ContentAlignment alignment)
        {
            RectangleF box = GetRectangle(this.Graphics, alignment, x, y, text, this.Font);
            this.Graphics.DrawString(text, this.Font, this.Brush, box);
            return box;
        }

        /// <summary>
        /// Draws text at the specified location and returns bounding rectangle.
        /// </summary>
        /// <param name="text">Text to draw.</param>
        /// <param name="x">X-coordinate of origin.</param>
        /// <param name="y">Y-coordinate of origin.</param>
        /// <param name="alignment">Alignment of text in regards to x and y.</param>
        /// <param name="maxWidth">Maximum width of the string. This value will be truncated.</param>
        public RectangleF DrawString(string text, float x, float y, ContentAlignment alignment, float maxWidth)
        {
            RectangleF box = GetRectangle(this.Graphics, alignment, x, y, text, this.Font, maxWidth);
            this.Graphics.DrawString(text, this.Font, this.Brush, box);
            return box;
        }

        /// <summary>
        /// Draws text at the specified location and returns bounding rectangle.
        /// </summary>
        /// <param name="text">Text to draw.</param>
        /// <param name="x">X-coordinate of origin.</param>
        /// <param name="y">Y-coordinate of origin.</param>
        /// <param name="alignment">Alignment of text in regards to x and y.</param>
        /// <param name="layoutArea">Structure that specifies the maximum layout area for the text.</param>
        public RectangleF DrawString(string text, float x, float y, ContentAlignment alignment, SizeF layoutArea)
        {
            RectangleF box = GetRectangle(this.Graphics, alignment, x, y, text, this.Font, layoutArea);
            this.Graphics.DrawString(text, this.Font, this.Brush, box);
            return box;
        }

        /// <summary>
        /// Draws text at the specified location and returns bounding rectangle.
        /// </summary>
        /// <param name="text">Text to draw.</param>
        /// <param name="x">X-coordinate of origin.</param>
        /// <param name="y">Y-coordinate of origin.</param>
        /// <param name="alignment">Alignment of text in regards to x and y.</param>
        /// <param name="maxWidth">Maximum width of the string. This value will be truncated.</param>
        /// <param name="format">System.Drawing.StringFormat that specifies formatting attributes, such as line spacing and alignment, that are applied to the drawn text.</param>
        public RectangleF DrawString(string text, float x, float y, ContentAlignment alignment, float maxWidth, StringFormat format)
        {
            RectangleF box = GetRectangle(this.Graphics, alignment, x, y, text, this.Font, maxWidth, format);
            this.Graphics.DrawString(text, this.Font, this.Brush, box, format);
            return box;
        }

        /// <summary>
        /// Draws text at the specified location and returns bounding rectangle.
        /// </summary>
        /// <param name="text">Text to draw.</param>
        /// <param name="x">X-coordinate of origin.</param>
        /// <param name="y">Y-coordinate of origin.</param>
        /// <param name="alignment">Alignment of text in regards to x and y.</param>
        /// <param name="layoutArea">Structure that specifies the maximum layout area for the text.</param>
        /// <param name="format">System.Drawing.StringFormat that specifies formatting attributes, such as line spacing and alignment, that are applied to the drawn text.</param>
        public RectangleF DrawString(string text, float x, float y, ContentAlignment alignment, SizeF layoutArea, StringFormat format)
        {
            RectangleF box = GetRectangle(this.Graphics, alignment, x, y, text, this.Font, layoutArea, format);
            this.Graphics.DrawString(text, this.Font, this.Brush, box, format);
            return box;
        }



        /// <summary>
        /// Draws text at the specified location and returns bounding rectangle.
        /// </summary>
        /// <param name="graphics">Graphics surface on which text will be drawn.</param>
        /// <param name="font">Font to use for drawing text.</param>
        /// <param name="brush">Brush for drawing text.</param>
        /// <param name="text">Text to draw.</param>
        /// <param name="x">X-coordinate of origin.</param>
        /// <param name="y">Y-coordinate of origin.</param>
        /// <param name="alignment">Alignment of text in regards to x and y.</param>
        public static RectangleF DrawString(Graphics graphics, Font font, Brush brush, string text, float x, float y, ContentAlignment alignment)
        {
            RectangleF box = GetRectangle(graphics, alignment, x, y, text, font);
            graphics.DrawString(text, font, brush, box);
            return box;
        }

        /// <summary>
        /// Draws text at the specified location and returns bounding rectangle.
        /// </summary>
        /// <param name="graphics">Graphics surface on which text will be drawn.</param>
        /// <param name="font">Font to use for drawing text.</param>
        /// <param name="brush">Brush for drawing text.</param>
        /// <param name="text">Text to draw.</param>
        /// <param name="x">X-coordinate of origin.</param>
        /// <param name="y">Y-coordinate of origin.</param>
        /// <param name="alignment">Alignment of text in regards to x and y.</param>
        /// <param name="maxWidth">Maximum width of the string. This value will be truncated.</param>
        public static RectangleF DrawString(Graphics graphics, Font font, Brush brush, string text, float x, float y, ContentAlignment alignment, float maxWidth)
        {
            RectangleF box = GetRectangle(graphics, alignment, x, y, text, font, maxWidth);
            graphics.DrawString(text, font, brush, box);
            return box;
        }

        /// <summary>
        /// Draws text at the specified location and returns bounding rectangle.
        /// </summary>
        /// <param name="graphics">Graphics surface on which text will be drawn.</param>
        /// <param name="font">Font to use for drawing text.</param>
        /// <param name="brush">Brush for drawing text.</param>
        /// <param name="text">Text to draw.</param>
        /// <param name="x">X-coordinate of origin.</param>
        /// <param name="y">Y-coordinate of origin.</param>
        /// <param name="alignment">Alignment of text in regards to x and y.</param>
        /// <param name="layoutArea">Structure that specifies the maximum layout area for the text.</param>
        public static RectangleF DrawString(Graphics graphics, Font font, Brush brush, string text, float x, float y, ContentAlignment alignment, SizeF layoutArea)
        {
            RectangleF box = GetRectangle(graphics, alignment, x, y, text, font, layoutArea);
            graphics.DrawString(text, font, brush, box);
            return box;
        }

        /// <summary>
        /// Draws text at the specified location and returns bounding rectangle.
        /// </summary>
        /// <param name="graphics">Graphics surface on which text will be drawn.</param>
        /// <param name="font">Font to use for drawing text.</param>
        /// <param name="brush">Brush for drawing text.</param>
        /// <param name="text">Text to draw.</param>
        /// <param name="x">X-coordinate of origin.</param>
        /// <param name="y">Y-coordinate of origin.</param>
        /// <param name="alignment">Alignment of text in regards to x and y.</param>
        /// <param name="maxWidth">Maximum width of the string. This value will be truncated.</param>
        /// <param name="format">System.Drawing.StringFormat that specifies formatting attributes, such as line spacing and alignment, that are applied to the drawn text.</param>
        public static RectangleF DrawString(Graphics graphics, Font font, Brush brush, string text, float x, float y, ContentAlignment alignment, float maxWidth, StringFormat format)
        {
            RectangleF box = GetRectangle(graphics, alignment, x, y, text, font, maxWidth, format);
            graphics.DrawString(text, font, brush, box, format);
            return box;
        }

        /// <summary>
        /// Draws text at the specified location and returns bounding rectangle.
        /// </summary>
        /// <param name="graphics">Graphics surface on which text will be drawn.</param>
        /// <param name="font">Font to use for drawing text.</param>
        /// <param name="brush">Brush for drawing text.</param>
        /// <param name="text">Text to draw.</param>
        /// <param name="x">X-coordinate of origin.</param>
        /// <param name="y">Y-coordinate of origin.</param>
        /// <param name="alignment">Alignment of text in regards to x and y.</param>
        /// <param name="layoutArea">Structure that specifies the maximum layout area for the text.</param>
        /// <param name="format">System.Drawing.StringFormat that specifies formatting attributes, such as line spacing and alignment, that are applied to the drawn text.</param>
        public static RectangleF DrawString(Graphics graphics, Font font, Brush brush, string text, float x, float y, ContentAlignment alignment, SizeF layoutArea, StringFormat format)
        {
            RectangleF box = GetRectangle(graphics, alignment, x, y, text, font, layoutArea, format);
            graphics.DrawString(text, font, brush, box, format);
            return box;
        }



        #region Private

        private static RectangleF GetRectangle(Graphics graphics, ContentAlignment alignment, float x, float y, string text, Font font)
        {
            SizeF size = graphics.MeasureString(text, font);
            return GetRectangleFromSize(alignment, x, y, size);
        }

        private static RectangleF GetRectangle(Graphics graphics, ContentAlignment alignment, float x, float y, string text, Font font, float maxWidth)
        {
            SizeF size = graphics.MeasureString(text, font, (int)System.Math.Truncate(maxWidth));
            return GetRectangleFromSize(alignment, x, y, size);
        }

        private static RectangleF GetRectangle(Graphics graphics, ContentAlignment alignment, float x, float y, string text, Font font, SizeF layoutArea)
        {
            SizeF size = graphics.MeasureString(text, font, layoutArea);
            return GetRectangleFromSize(alignment, x, y, size);
        }

        private static RectangleF GetRectangle(Graphics graphics, ContentAlignment alignment, float x, float y, string text, Font font, float maxWidth, StringFormat format)
        {
            SizeF size = graphics.MeasureString(text, font, (int)System.Math.Truncate(maxWidth), format);
            return GetRectangleFromSize(alignment, x, y, size);
        }

        private static RectangleF GetRectangle(Graphics graphics, ContentAlignment alignment, float x, float y, string text, Font font, SizeF layoutArea, StringFormat format)
        {
            SizeF size = graphics.MeasureString(text, font, layoutArea, format);
            return GetRectangleFromSize(alignment, x, y, size);
        }

        private static RectangleF GetRectangleFromSize(ContentAlignment alignment, float x, float y, SizeF size)
        {
            switch (alignment)
            {
                case ContentAlignment.TopLeft: return new RectangleF(x, y, size.Width, size.Height);
                case ContentAlignment.TopCenter: return new RectangleF(x - size.Width / 2, y, size.Width, size.Height);
                case ContentAlignment.TopRight: return new RectangleF(x - size.Width, y, size.Width, size.Height);
                case ContentAlignment.MiddleLeft: return new RectangleF(x, y - size.Height / 2, size.Width, size.Height);
                case ContentAlignment.MiddleCenter: return new RectangleF(x - size.Width / 2, y - size.Height / 2, size.Width, size.Height);
                case ContentAlignment.MiddleRight: return new RectangleF(x - size.Width, y - size.Height / 2, size.Width, size.Height);
                case ContentAlignment.BottomLeft: return new RectangleF(x, y - size.Height, size.Width, size.Height);
                case ContentAlignment.BottomCenter: return new RectangleF(x - size.Width / 2, y - size.Height, size.Width, size.Height);
                case ContentAlignment.BottomRight: return new RectangleF(x - size.Width, y - size.Height, size.Width, size.Height);
                default: return RectangleF.Empty;
            }
        }

        #endregion


        #region IDisposable Members

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">True if managed resources should be disposed; otherwise, false.</param>
        protected virtual void Dispose(bool disposing)
        {
            //if (disposing) {
                //if ((this._shouldDisposeGraphics) && (this._graphics != null))
                //{
                //    this._graphics.Dispose();
                //    this._graphics = null;
                //}

                //if ((this._shouldDisposeFont) && (this._font != null))
                //{
                //    this._font.Dispose();
                //    this._font = null;
                //}

                //if ((this._shouldDisposeBrush )&&(this._brush != null))
                //{
                //    this._brush.Dispose();
                //    this._brush = null;
                //}
            //}
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
