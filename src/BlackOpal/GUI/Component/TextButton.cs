using Cosmos.System;
using System.Threading;
using System.Drawing;
using System;
using BlackOpal.Calculations;
using PrismAPI.Hardware.GPU;
using Color = PrismAPI.Graphics.Color;

namespace GUI.Component
{
    internal class TextButton
    {
        /* VARIABLES */
        public Action PressedAction = new Action(() => { });
        public Display ScreenCanvas;
        public Color ButtonHighlightColor = Color.White;
        public Color ButtonPressedColor = Color.DeepGray;
        public Color ButtonColor = Color.LightGray;
        public Color TextColor = Color.Black;
        public Point ButtonBottomRight = new Point(0, 0);
        public Point ButtonPosition = new Point(0, 0);
        public ushort ButtonPixelLength = 0;
        public string ButtonText = "Text";
        public bool ActionCalled = false;

        /* FUNCTIONS */
        public void Draw()
        {
            // Don't do shit if the canvas isn't defined
            if (ScreenCanvas == null)
                return;

            // Calculate the button's length in pixels
            if (ButtonPixelLength == 0)
            {
                //ButtonPixelLength = (ushort)(ButtonText.Length * 11);
                ButtonPixelLength = (ushort)(PrismAPI.Graphics.Fonts.Font.Fallback.MeasureString(ButtonText) + 8);
            }

            // Calculate where the bottom right corner of the button is
            ButtonBottomRight.X = ButtonPosition.X + ButtonPixelLength;
            ButtonBottomRight.Y = ButtonPosition.Y + 16;

            // Draw the button at the specified position, with colors text
            if (IsPressed())
            {
                ScreenCanvas.DrawFilledRectangle(ButtonPosition.X, ButtonPosition.Y, ButtonPixelLength, 17, 0, ButtonPressedColor);
                ScreenCanvas.DrawString((ButtonPosition.X + ButtonBottomRight.X) / 2 + 2, (ButtonPosition.Y + ButtonBottomRight.Y) / 2 - 2, ButtonText, default, TextColor, true);

                ScreenCanvas.DrawLine(ButtonPosition.X, ButtonPosition.Y, ButtonPosition.X + ButtonPixelLength, ButtonPosition.Y, Color.Black);
                ScreenCanvas.DrawLine(ButtonPosition.X - 1, ButtonPosition.Y + 1, ButtonPosition.X - 1, ButtonBottomRight.Y, Color.Black);

                ScreenCanvas.DrawLine(ButtonPosition.X, ButtonPosition.Y + 17, ButtonPosition.X + ButtonPixelLength, ButtonPosition.Y + 17, Color.White);
                ScreenCanvas.DrawLine(ButtonPosition.X + ButtonPixelLength, ButtonPosition.Y + 2, ButtonPosition.X + ButtonPixelLength, ButtonBottomRight.Y, Color.White);

                if (ActionCalled == false)
                {
                    ActionCalled = true;
                    ScreenCanvas.Update();
                    Thread.Sleep(10);
                    PressedAction.Invoke();
                }
            }
            else
            {
                if (CheckHighlight())
                {
                    ScreenCanvas.DrawFilledRectangle(ButtonPosition.X, ButtonPosition.Y, ButtonPixelLength, 17, 0, ButtonHighlightColor);
                }
                else
                {
                    ScreenCanvas.DrawFilledRectangle(ButtonPosition.X, ButtonPosition.Y, ButtonPixelLength, 17, 0, ButtonColor);
                }

                ScreenCanvas.DrawString((ButtonPosition.X + ButtonBottomRight.X) / 2, (ButtonPosition.Y + ButtonBottomRight.Y) / 2 - 2, ButtonText, default, TextColor, true);

                ScreenCanvas.DrawLine(ButtonPosition.X, ButtonPosition.Y, ButtonPosition.X + ButtonPixelLength, ButtonPosition.Y, Color.White);
                ScreenCanvas.DrawLine(ButtonPosition.X - 1, ButtonPosition.Y + 1, ButtonPosition.X - 1, ButtonBottomRight.Y, Color.White);

                ScreenCanvas.DrawLine(ButtonPosition.X, ButtonPosition.Y + 17, ButtonPosition.X + ButtonPixelLength, ButtonPosition.Y + 17, Color.Black);
                ScreenCanvas.DrawLine(ButtonPosition.X + ButtonPixelLength, ButtonPosition.Y + 2, ButtonPosition.X + ButtonPixelLength, ButtonBottomRight.Y, Color.Black);
            }
        }

        // Check if the mouse has been clicked over the button
        public bool IsPressed()
        {
            if (MouseManager.MouseState != MouseState.Left || MouseManager.LastMouseState == MouseState.Left)
            {
                ActionCalled = false;
                return false;
            }

            return ShapeCollision.IsPointInsideRectangle(UserInterface.ClickPoint.X, UserInterface.ClickPoint.Y, ButtonPosition.X, ButtonPosition.Y, ButtonBottomRight.X, ButtonBottomRight.Y);
        }

        // Check if the mouse is hovering over the button
        public bool CheckHighlight()
        {
            return ShapeCollision.IsPointInsideRectangle((int)MouseManager.X, (int)MouseManager.Y, ButtonPosition.X, ButtonPosition.Y, ButtonBottomRight.X, ButtonBottomRight.Y);
        }
    }
}
