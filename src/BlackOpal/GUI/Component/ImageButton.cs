using Cosmos.System;
using System.Drawing;
using System;
using BlackOpal.Utilities.Calculations;
using PrismAPI.Graphics;
using Color = PrismAPI.Graphics.Color;

namespace GUI.Component
{
    internal class ImageButton
    {
        /* VARIABLES */
        public Action PressedAction = new Action(() => { });
        public Canvas ScreenCanvas;
        public Canvas ButtonImage;
        public Color ButtonPressedOverlay = new Color(32, Color.LightGray.R, Color.LightGray.G, Color.LightGray.B);
        public Color HighlightedBackColor = Color.Transparent;
        public Color BackColorPressed = Color.Transparent;
        public Color BackColor = Color.Transparent;
        public Point ButtonBottomRight = new Point(0, 0);
        public Point ButtonPosition = new Point(0, 0);
        public bool DrawWithAlphaChannel = true;
        public bool RequireMouseHeld = false;
        public bool ActionCalled = false;
        public bool Toggled = false;

        /* FUNCTIONS */
        public void Draw()
        {
            // Calculate where the bottom right cornet of the button is
            ButtonBottomRight.X = ButtonPosition.X + ButtonImage.Width;
            ButtonBottomRight.Y = ButtonPosition.Y + ButtonImage.Height;

            // Draw the button at the specified position
            if (IsPressed() || Toggled)
            {
                ScreenCanvas.DrawFilledRectangle(ButtonPosition.X, ButtonPosition.Y, ButtonImage.Width, ButtonImage.Height, 0, BackColorPressed);
                ScreenCanvas.DrawLine(ButtonPosition.X, ButtonPosition.Y, ButtonBottomRight.X, ButtonPosition.Y, Color.Black);
                ScreenCanvas.DrawLine(ButtonPosition.X - 1, ButtonPosition.Y + 1, ButtonPosition.X - 1, ButtonBottomRight.Y, Color.Black);
                ScreenCanvas.DrawLine(ButtonPosition.X, ButtonBottomRight.Y, ButtonBottomRight.X, ButtonBottomRight.Y, Color.White);
                ScreenCanvas.DrawLine(ButtonBottomRight.X, ButtonPosition.Y + 1, ButtonBottomRight.X, ButtonBottomRight.Y, Color.White);

                if (ActionCalled == false)
                {
                    ActionCalled = true;
                    //ScreenCanvas.Display();
                    //Thread.Sleep(10);
                    PressedAction.Invoke();

                    if (RequireMouseHeld)
                        Toggled = !Toggled;
                }
            }
            else
            {
                if (CheckHighlight())
                {
                    ScreenCanvas.DrawFilledRectangle(ButtonPosition.X, ButtonPosition.Y, ButtonImage.Width, ButtonImage.Height, 0, HighlightedBackColor);
                }
                else
                {
                    ScreenCanvas.DrawFilledRectangle(ButtonPosition.X, ButtonPosition.Y, ButtonImage.Width, ButtonImage.Height, 0, BackColor);
                }

                ScreenCanvas.DrawLine(ButtonPosition.X, ButtonPosition.Y, ButtonBottomRight.X, ButtonPosition.Y, Color.White);
                ScreenCanvas.DrawLine(ButtonPosition.X - 1, ButtonPosition.Y + 1, ButtonPosition.X - 1, ButtonBottomRight.Y, Color.White);
                ScreenCanvas.DrawLine(ButtonPosition.X, ButtonBottomRight.Y, ButtonBottomRight.X, ButtonBottomRight.Y, Color.Black);
                ScreenCanvas.DrawLine(ButtonPosition.X + ButtonImage.Width, ButtonPosition.Y + 1, ButtonBottomRight.X, ButtonBottomRight.Y, Color.Black);
            }

            // Draw the image
            ScreenCanvas.DrawImage(ButtonPosition.X, ButtonPosition.Y, ButtonImage, DrawWithAlphaChannel);
        }

        // Check if the button is pressed
        public bool IsPressed()
        {
            if (MouseManager.MouseState != MouseState.Left || MouseManager.LastMouseState == MouseState.Left)
            {
                ActionCalled = false;
                return false;
            }

            return ShapeCollision.IsPointInsideRectangle(UserInterface.ClickPoint.X, UserInterface.ClickPoint.Y, ButtonPosition.X, ButtonPosition.Y, ButtonBottomRight.X, ButtonBottomRight.Y);
        }

        // Check if the mouse cursor is inside the button
        public bool CheckHighlight()
        {
            return ShapeCollision.IsPointInsideRectangle((int)MouseManager.X, (int)MouseManager.Y, ButtonPosition.X, ButtonPosition.Y, ButtonBottomRight.X, ButtonBottomRight.Y);
        }
    }
}
