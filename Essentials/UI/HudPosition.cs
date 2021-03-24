using Essentials.Extensions;
using System;
using UnityEngine;

namespace Essentials.UI
{
    public class HudPosition
    {
        /// <summary>
        /// Screen width in pixels.
        /// </summary>
        public static int PixelWidth { get; private set; }
        /// <summary>
        /// Screen height in pixels.
        /// </summary>
        public static int PixelHeight { get; private set; }

        /// <summary>
        /// Screen width in units.
        /// </summary>
        public static float Width { get; private set; }
        /// <summary>
        /// Screen height in units.
        /// </summary>
        public static float Height { get; private set; }

        /// <summary>
        /// Top left corner units vector.
        /// </summary>
        public static Vector2 TopLeft { get; private set; }
        /// <summary>
        /// Top right corner units vector.
        /// </summary>
        public static Vector2 TopRight { get; private set; }
        /// <summary>
        /// Bottom left corner units vector.
        /// </summary>
        public static Vector2 BottomLeft { get; private set; }
        /// <summary>
        /// Bottom right corner units vector.
        /// </summary>
        public static Vector2 BottomRight { get; private set; }
        /// <summary>
        /// Top-center (horizontally) units vector.
        /// </summary>
        public static Vector2 Top { get; private set; }
        /// <summary>
        /// Bottom-center (horizontally) units vector.
        /// </summary>
        public static Vector2 Bottom { get; private set; }
        /// <summary>
        /// Left-center (vertically) units vector.
        /// </summary>
        public static Vector2 Left { get; private set; }
        /// <summary>
        /// Right-center (vertically) units vector.
        /// </summary>
        public static Vector2 Right { get; private set; }

        internal static void Load()
        {
            Events.HudCreated += HudUpdated;
            Events.HudUpdated += HudUpdated;
        }

        private static void HudUpdated(object sender, EventArgs e)
        {
            // Check for resolution change
            if (PixelWidth == Camera.main.pixelWidth && PixelHeight == Camera.main.pixelHeight) return;

            int oldPixelWidth = PixelWidth, oldPixelHeight = PixelHeight;
            float oldWidth = Width, oldHeight = Height;

            PixelWidth = Camera.main.pixelWidth;
            PixelHeight = Camera.main.pixelHeight;

            // Get corner position in units
            Vector3 bottomLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0)) - Camera.main.transform.localPosition;

            Width = -bottomLeft.x * 2;
            Height = -bottomLeft.y * 2;

            // Transform to other corners
            BottomLeft = bottomLeft;
            BottomRight = GetAlignedOffset(BottomLeft, HudAlignment.BottomRight);
            TopLeft = GetAlignedOffset(BottomLeft, HudAlignment.TopLeft);
            TopRight = GetAlignedOffset(BottomLeft, HudAlignment.TopRight);
            Top = new Vector2(0, -BottomLeft.y);
            Bottom = new Vector2(0, BottomLeft.y);
            Left = new Vector2(BottomLeft.x, 0);
            Right = new Vector2(-BottomLeft.x, 0);

            // Raise event
            if (oldPixelWidth != 0 && oldPixelHeight != 0) Events.RaiseResolutionChanged(oldPixelWidth, oldPixelHeight, oldWidth, oldHeight);
        }

        /// <param name="alignment">Alignment position</param>
        /// <returns>The vector position corresponding to the alignment</returns>
        public static Vector2 GetAlignmentVector(HudAlignment alignment)
        {
            return alignment switch
            {
                HudAlignment.BottomLeft => BottomLeft,
                HudAlignment.BottomRight => BottomRight,
                HudAlignment.TopLeft => TopLeft,
                HudAlignment.TopRight => TopRight,
                HudAlignment.Bottom => Bottom,
                HudAlignment.Top => Top,
                HudAlignment.Left => Left,
                HudAlignment.Right => Right,
                _ => BottomLeft,
            };
        }

        /// <summary>
        /// Adjusts offset values from the default (<see cref="HudAlignment.BottomLeft"/>) to other positions.
        /// </summary>
        /// <param name="offset">Offset to align</param>
        /// <param name="alignment">Alignment position</param>
        /// <returns>The offset adjusted to the corresponding alignment</returns>
        public static Vector2 GetAlignedOffset(Vector2 offset, HudAlignment alignment)
        {
            return alignment switch
            {
                HudAlignment.BottomLeft => offset,
                HudAlignment.BottomRight => new Vector2(-offset.x, offset.y),
                HudAlignment.TopLeft => new Vector2(offset.x, -offset.y),
                HudAlignment.TopRight => new Vector2(-offset.x, -offset.y),
                HudAlignment.Bottom => offset,
                HudAlignment.Top => new Vector2(offset.x, -offset.y),
                HudAlignment.Left => offset,
                HudAlignment.Right => new Vector2(-offset.x, offset.y),
                _ => offset,
            };
        }

        /// <summary>
        /// Offset vector before adjustment.
        /// </summary>
        public Vector2 Offset { get; set; }
        /// <summary>
        /// Alignment position.
        /// </summary>
        public HudAlignment Alignment { get; set; }

        /// <summary>
        /// An adjusted offset from the alignment position.
        /// </summary>
        /// <param name="offset">Offset to adjust</param>
        /// <param name="alignment">Alignment position</param>
        public HudPosition(Vector2 offset, HudAlignment alignment = HudAlignment.BottomLeft)
        {
            Offset = offset;

            Alignment = alignment;
        }

        /// <summary>
        /// An adjusted offset from the alignment position.
        /// </summary>
        /// <param name="xOffset">X offset to adjust</param>
        /// <param name="yOffset">Y offset to adjust</param>
        /// <param name="alignment">Alignment position</param>
        public HudPosition(float xOffset = 0F, float yOffset = 0F, HudAlignment alignment = HudAlignment.BottomLeft) : this(new Vector2(xOffset, yOffset), alignment)
        {
        }

        /// <summary>
        /// An adjusted offset from the alignment position.
        /// </summary>
        /// <param name="alignment">Alignment position</param>
        public HudPosition(HudAlignment alignment) : this(Vector2.zero, alignment)
        {
        }

        /// <returns>The vector position corresponding to the alignment</returns>
        private Vector2 GetAlignmentVector()
        {
            return GetAlignmentVector(Alignment);
        }

        /// <returns>The offset adjusted to the corresponding alignment</returns>
        private Vector2 GetAlignedOffset()
        {
            return GetAlignedOffset(Offset, Alignment);
        }

        /// <returns>An adjusted offset from the alignment position</returns>
        public Vector2 GetVector()
        {
            return GetAlignmentVector() + GetAlignedOffset();
        }

        /// <returns>An adjusted offset from the alignment position with the provided Z</returns>
        public Vector3 GetVector3(float z = 0F)
        {
            return GetVector().ToVector3(z);
        }

        public static implicit operator HudPosition(Vector2 offset)
        {
            return new HudPosition(offset);
        }

        public static implicit operator Vector2(HudPosition uiPos)
        {
            return uiPos.GetVector();
        }

        public static implicit operator Vector3(HudPosition uiPos)
        {
            return uiPos.GetVector();
        }
    }
}