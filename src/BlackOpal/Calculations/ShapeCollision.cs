/* NAMESPACES */
using Cosmos.System;
using System.Drawing;
using System;

namespace BlackOpal.Calculations
{
    /* CLASSES */
    internal class ShapeCollision
    {
        /* FUNCTIONS */
        // Check if a point is inside a circle
        public static bool IsPointInsideCircle(int X1, int Y1, int X2, int Y2, int Radius)
        {
            int dx = X1 - X2;
            int dy = Y1 - Y2;
            return dx * dx + dy * dy <= Radius * Radius;
        }

        public static bool IsPointInsideRectangle(int PX, int PY, int X1, int Y1, int X2, int Y2)
        {
            return Math.Abs(X1) <= PX && PX <= Math.Abs(X2) && Math.Abs(Y1) <= PY && PY <= Math.Abs(Y2);
        }
    }
}
