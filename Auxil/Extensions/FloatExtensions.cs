namespace Auxil.Extensions;

/// <summary>Provides convenience members for working with <see cref="float"/> values.</summary>
[PublicAPI]
public static class FloatExtensions
{
    extension(float floatToConvert)
    {
        /// <summary>Converts the value to an <see cref="int"/>, truncating toward zero.</summary>
        public int ToInt() => (int)floatToConvert;

        /// <summary>Rounds the value to the given number of fractional digits.</summary>
        /// <param name="digits">The number of fractional digits.</param>
        public float Round(int digits) => MathF.Round(floatToConvert, digits);
    }
}
