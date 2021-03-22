using System;

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
}