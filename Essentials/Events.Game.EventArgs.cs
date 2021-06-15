using Essentials.UI;
using System;
using UnityEngine;

namespace Essentials
{
    public class HudStateChangedEventArgs : EventArgs
    {
        public readonly bool Active;

        public HudStateChangedEventArgs(bool active)
        {
            Active = active;
        }
    }

    public class ResolutionChangedEventArgs : EventArgs
    {
        public readonly int OldPixelWidth, OldPixelHeight;

        public readonly float OldWidth, OldHeight;

        public readonly Vector2 OldTopLeft, OldTopRight, OldBottomLeft, OldBottomRight, OldTop, OldBottom, OldLeft, OldRight;

        public ResolutionChangedEventArgs(int oldPixelWidth, int oldPixelHeight, float oldWidth, float oldHeight)
        {
            OldPixelWidth = oldPixelWidth;
            OldPixelHeight = oldPixelHeight;

            OldWidth = oldWidth;
            OldHeight = oldHeight;

            OldBottomLeft = new Vector2(-OldWidth / 2, -OldHeight / 2);
            OldBottomRight = HudPosition.GetAlignedOffset(in OldBottomLeft, HudAlignment.BottomRight);
            OldTopLeft = HudPosition.GetAlignedOffset(in OldBottomLeft, HudAlignment.TopLeft);
            OldTopRight = HudPosition.GetAlignedOffset(in OldBottomLeft, HudAlignment.TopRight);
            OldTop = new Vector2(0, -OldBottomLeft.y);
            OldBottom = new Vector2(0, OldBottomLeft.y);
            OldLeft = new Vector2(OldBottomLeft.x, 0);
            OldRight = new Vector2(-OldBottomLeft.x, 0);
        }
    }
}