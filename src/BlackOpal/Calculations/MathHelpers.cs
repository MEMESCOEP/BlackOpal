using System;

namespace BlackOpal.Calculations
{
    internal class MathHelpers
    {
        // Truncate a number to n decimal places
        public static double TruncateToDecimalPlace(double Value, int Places)
        {
            /*double PowerPlaces = Math.Pow(10, Places);
            double Truncated = Math.Truncate(Math.Abs(Value) * PowerPlaces) / PowerPlaces;

            if (Value < 0)
                Truncated *= -1;

            return Truncated;*/
            float DecimalPlaces = Places * 100f;

            return Math.Ceiling(Value * DecimalPlaces) / DecimalPlaces;
        }
    }
}
