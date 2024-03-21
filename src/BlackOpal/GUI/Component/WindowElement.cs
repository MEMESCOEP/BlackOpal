using System.Drawing;
using Color = PrismAPI.Graphics.Color;

namespace BlackOpal.GUI.Component
{
    internal class WindowElement
    {
        public enum ElementType
        {
            IMAGE_BUTTON,
            TEXT_BUTTON,
            CONTROL,
            STRING,
            IMAGE,
            SHAPE,            
            OTHER
        }

        public Color ElementColor = Color.Black;
        public Point ElementPosition = new Point(0, 0);
        public ElementType Type = ElementType.OTHER;
        public object ElementData;
    }
}
