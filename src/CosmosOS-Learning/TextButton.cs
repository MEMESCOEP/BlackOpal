using Cosmos.System;
using Cosmos.System.Graphics;
using Cosmos.System.Graphics.Fonts;
using System;
using System.Drawing;
using System.Reflection;

namespace CosmosOS_Learning
{
    internal class TextButton
    {
        /* VARIABLES */
        public PCScreenFont TextFont = PCScreenFont.Default;
        public Action PressedAction = new Action(() => { });
        public Canvas ScreenCanvas;
        public Color ButtonPressedColor = Color.Gray;
        public Color ButtonColor = Color.LightGray;
        public Color TextColor = Color.Black;
        public Point ButtonBottomRight = new Point(0, 0);
        public Point ButtonPosition = new Point(0, 0);
        public string ButtonText = "Text";
        public bool ActionCalled = false;
        public int ButtonPixelLength = 0;

        /* FUNCTIONS */
        public void Draw()
        {
            // Don't do shit if the canvas isn't defined
            if (ScreenCanvas == null)
                return;

            // Calculate the button's length in pixels
            if (ButtonPixelLength == 0)
            {
                ButtonPixelLength = (ButtonText.Length * 8) + 8;
            }

            // Calculate where the bottom right corner of the button is
            ButtonBottomRight = new Point(ButtonPosition.X + ButtonPixelLength, ButtonPosition.Y + 16);

            // Draw the button at the specified position, with colors text
            if (IsPressed())
            {
                ScreenCanvas.DrawFilledRectangle(ButtonPressedColor, ButtonPosition.X, ButtonPosition.Y, ButtonPixelLength, 17);
                ScreenCanvas.DrawString(ButtonText, TextFont, TextColor, ButtonPosition.X + 4, ButtonPosition.Y + 2);

                ScreenCanvas.DrawLine(Color.Black, ButtonPosition.X, ButtonPosition.Y, ButtonPosition.X + ButtonPixelLength, ButtonPosition.Y);
                ScreenCanvas.DrawLine(Color.Black, ButtonPosition.X - 1, ButtonPosition.Y + 1, ButtonPosition.X - 1, ButtonBottomRight.Y);

                ScreenCanvas.DrawLine(Color.White, ButtonPosition.X, ButtonPosition.Y + 17, ButtonPosition.X + ButtonPixelLength, ButtonPosition.Y + 17);
                ScreenCanvas.DrawLine(Color.White, ButtonPosition.X + ButtonPixelLength, ButtonPosition.Y + 2, ButtonPosition.X + ButtonPixelLength, ButtonBottomRight.Y);

                if (ActionCalled == false)
                {
                    ActionCalled = true;
                    ScreenCanvas.Display();
                    PressedAction.Invoke();
                }
            }
            else
            {
                ScreenCanvas.DrawFilledRectangle(ButtonColor, ButtonPosition.X, ButtonPosition.Y, ButtonPixelLength, 17);
                ScreenCanvas.DrawString(ButtonText, TextFont, TextColor, ButtonPosition.X + 4, ButtonPosition.Y + 2);

                ScreenCanvas.DrawLine(Color.White, ButtonPosition.X, ButtonPosition.Y, ButtonPosition.X + ButtonPixelLength, ButtonPosition.Y);
                ScreenCanvas.DrawLine(Color.White, ButtonPosition.X - 1, ButtonPosition.Y + 1, ButtonPosition.X - 1, ButtonBottomRight.Y);

                ScreenCanvas.DrawLine(Color.Black, ButtonPosition.X, ButtonPosition.Y + 17, ButtonPosition.X + ButtonPixelLength, ButtonPosition.Y + 17);
                ScreenCanvas.DrawLine(Color.Black, ButtonPosition.X + ButtonPixelLength, ButtonPosition.Y + 2, ButtonPosition.X + ButtonPixelLength, ButtonBottomRight.Y);
            }
        }

        public bool IsPressed()
        {
            if (MouseManager.LastMouseState == MouseManager.MouseState || MouseManager.MouseState != MouseState.Left)
            {
                ActionCalled = false;
                return false;
            }

            return (ButtonPosition.X <= MouseManager.X && MouseManager.X <= ButtonBottomRight.X) && 
                (ButtonPosition.Y <= MouseManager.Y && MouseManager.Y <= ButtonBottomRight.Y);
        }
    }
}
