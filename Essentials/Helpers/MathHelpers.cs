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