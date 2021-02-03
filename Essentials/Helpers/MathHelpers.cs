using System;
using System.Collections.Generic;
using System.Text;

namespace Essentials.Helpers
{
    public static class MathHelpers
    {
        public static float Scale(float value, float min, float max, float minScale, float maxScale)
        {
            return minScale + (value - min) / (max - min) * (maxScale - minScale);
        }
    }
}
