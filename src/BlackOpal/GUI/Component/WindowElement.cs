using System.Drawing;
using PrismAPI.Graphics;
using Color = PrismAPI.Graphics.Color;

namespace BlackOpal.GUI.Component
{
    internal class WindowElement
    {
        public enum ElementType
        {
            CONTROL,
            STRING,
            IMAGE,
            SHAPE,            
            OTHER
        }

        public Color ElementColor = Color.Black;
        public Point ElementPosition = new Point(0, 0);
        public ElementType Type = ElementType.OTHER;
        public byte[] ElementData = new byte[1];
    }
}
