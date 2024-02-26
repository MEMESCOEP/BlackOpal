using System;

namespace BlackOpal.Calculations
{
    internal class MathHelpers
    {
        // Truncate a number to n decimal places
        public static double TruncateToDecimalPlace(double Value, int Places)
        {
            float DecimalPlaces = Places * 100f;
            return Math.Ceiling(Value * DecimalPlaces) / DecimalPlaces;
        }
    }
}
