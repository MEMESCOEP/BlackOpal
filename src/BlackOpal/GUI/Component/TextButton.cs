using Cosmos.System.Coroutines;
using Cosmos.System;
using System.Collections.Generic;
using System.Threading;
using System.Drawing;
using System;
using BlackOpal.Utilities.Calculations;
using PrismAPI.Hardware.GPU;
using Color = PrismAPI.Graphics.Color;

namespace GUI.Component
{
    internal class TextButton
    {
        /* CONSTRUCTOR */
        public TextButton (Point Position)
        {
            ButtonLocalPosition = Position;
            ButtonGlobalPosition = Position;
        }

        /* VARIABLES */
        public Action PressedAction = new Action(() => { });
        public Display ScreenCanvas;
        public Color ButtonHighlightColor = Color.White;
        public Color ButtonPressedColor = Color.DeepGray;
        public Color ButtonColor = Color.LightGray;
        public Color TextColor = Color.Black;
        public Point ButtonGlobalPosition;
        public Point ButtonLocalPosition;
        public Point ButtonBottomRight;
        public ushort ButtonPixelLength = 0;
        public string ButtonText = "Text";
        public bool DrawFromLocalPosition = false;
        public bool UpdateScreenOnAction = true;
        public bool CalculateBounds = true;
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
                ButtonPixelLength = (ushort)(UserInterface.BIOSFont.MeasureString(ButtonText) + 12);
            }

            // Calculate where the bottom right corner of the button is
            if (CalculateBounds)
            {
                ButtonBottomRight.X = ButtonLocalPosition.X + ButtonPixelLength;
                ButtonBottomRight.Y = ButtonLocalPosition.Y + 16;
            }

            // Draw the button at the specified position, with colors text
            if (IsPressed())
            {
                ScreenCanvas.DrawFilledRectangle(ButtonLocalPosition.X, ButtonLocalPosition.Y, ButtonPixelLength, 17, 0, ButtonPressedColor);

                if (DrawFromLocalPosition)
                {
                    ScreenCanvas.DrawString(ButtonLocalPosition.X + (ButtonPixelLength / 2), ButtonLocalPosition.Y + (17 / 2) + 1, ButtonText, UserInterface.BIOSFont, TextColor, true);

                    // Vertical lines
                    ScreenCanvas.DrawLine(ButtonLocalPosition.X - 1, ButtonLocalPosition.Y + 1, ButtonLocalPosition.X - 1, ButtonLocalPosition.Y + 17, Color.Black);
                    ScreenCanvas.DrawLine(ButtonLocalPosition.X + ButtonPixelLength, ButtonLocalPosition.Y + 2, ButtonLocalPosition.X + ButtonPixelLength, ButtonBottomRight.Y, Color.White);
                }
                else
                {
                    ScreenCanvas.DrawString((ButtonLocalPosition.X + ButtonBottomRight.X) / 2, (ButtonLocalPosition.Y + ButtonBottomRight.Y) / 2, ButtonText, UserInterface.BIOSFont, TextColor, true);

                    // Vertical lines
                    ScreenCanvas.DrawLine(ButtonLocalPosition.X - 1, ButtonLocalPosition.Y + 1, ButtonLocalPosition.X - 1, ButtonBottomRight.Y, Color.Black);
                    ScreenCanvas.DrawLine(ButtonLocalPosition.X, ButtonLocalPosition.Y + 17, ButtonLocalPosition.X + ButtonPixelLength, ButtonLocalPosition.Y + 17, Color.White);
                }

                // Horizontal lines
                ScreenCanvas.DrawLine(ButtonLocalPosition.X + ButtonPixelLength, ButtonLocalPosition.Y + 2, ButtonLocalPosition.X + ButtonPixelLength, ButtonBottomRight.Y, Color.White);
                ScreenCanvas.DrawLine(ButtonLocalPosition.X, ButtonLocalPosition.Y, ButtonLocalPosition.X + ButtonPixelLength, ButtonLocalPosition.Y, Color.Black);

                if (ActionCalled == false)
                {
                    ActionCalled = true;

                    if (UpdateScreenOnAction)
                    {
                        ScreenCanvas.Update();
                    }
                    else
                    {
                        ((Display)BlackOpal.Kernel.Terminal.Contents).Update();
                    }

                    //Thread.Sleep(10);
                    PressedAction.Invoke();
                }
            }
            else
            {
                if (CheckHighlight())
                {
                    ScreenCanvas.DrawFilledRectangle(ButtonLocalPosition.X, ButtonLocalPosition.Y, ButtonPixelLength, 17, 0, ButtonHighlightColor);
                }
                else
                {
                    ScreenCanvas.DrawFilledRectangle(ButtonLocalPosition.X, ButtonLocalPosition.Y, ButtonPixelLength, 17, 0, ButtonColor);
                }

                if (DrawFromLocalPosition)
                {
                    ScreenCanvas.DrawString(ButtonLocalPosition.X + (ButtonPixelLength / 2), ButtonLocalPosition.Y + (17 / 2) + 1, ButtonText, UserInterface.BIOSFont, TextColor, true);

                    // Vertical lines
                    ScreenCanvas.DrawLine(ButtonLocalPosition.X - 1, ButtonLocalPosition.Y + 1, ButtonLocalPosition.X - 1, ButtonLocalPosition.Y + 17, Color.White);
                    ScreenCanvas.DrawLine(ButtonLocalPosition.X, ButtonLocalPosition.Y + 17, ButtonLocalPosition.X + ButtonPixelLength, ButtonLocalPosition.Y + 17, Color.Black);
                }
                else
                {
                    ScreenCanvas.DrawString((ButtonLocalPosition.X + ButtonBottomRight.X) / 2, (ButtonLocalPosition.Y + ButtonBottomRight.Y) / 2, ButtonText, UserInterface.BIOSFont, TextColor, true);

                    // Vertical lines
                    ScreenCanvas.DrawLine(ButtonLocalPosition.X - 1, ButtonLocalPosition.Y + 1, ButtonLocalPosition.X - 1, ButtonBottomRight.Y, Color.White);
                    ScreenCanvas.DrawLine(ButtonLocalPosition.X, ButtonLocalPosition.Y + 17, ButtonLocalPosition.X + ButtonPixelLength, ButtonLocalPosition.Y + 17, Color.Black);
                }

                // Horizontal lines
                ScreenCanvas.DrawLine(ButtonLocalPosition.X + ButtonPixelLength, ButtonLocalPosition.Y + 2, ButtonLocalPosition.X + ButtonPixelLength, ButtonLocalPosition.Y + 17, Color.Black);
                ScreenCanvas.DrawLine(ButtonLocalPosition.X, ButtonLocalPosition.Y, ButtonLocalPosition.X + ButtonPixelLength, ButtonLocalPosition.Y, Color.White);
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

            return ShapeCollision.IsPointInsideRectangle(UserInterface.ClickPoint.X, UserInterface.ClickPoint.Y, ButtonGlobalPosition.X, ButtonGlobalPosition.Y, ButtonBottomRight.X, ButtonBottomRight.Y);
        }

        // Check if the mouse is hovering over the button
        public bool CheckHighlight()
        {
            return ShapeCollision.IsPointInsideRectangle((int)MouseManager.X, (int)MouseManager.Y, ButtonGlobalPosition.X, ButtonGlobalPosition.Y, ButtonBottomRight.X, ButtonBottomRight.Y);
        }

        // RUn the button action
        IEnumerator<CoroutineControlPoint> RunAction()
        {
            yield return WaitFor.Milliseconds(25);
            PressedAction.Invoke();
        }
    }
}
